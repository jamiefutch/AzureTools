//MIT License:
//Copyright 2019 Jamie Futch
//
//Permission is hereby granted, free of charge, to any person obtaining a copy 
//of this software and associated documentation files (the "Software"), to deal 
//in the Software without restriction, including without limitation the rights 
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies 
//of the Software, and to permit persons to whom the Software is furnished to do 
//so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all 
//copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A 
//PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT 
//HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION 
//OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE 
//SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

using AzureTools.Models;

namespace AzureTools
{
    public class AzBlobAndContainerUtils :IDisposable
    {
        private readonly CloudStorageAccount _storageAccount;
        private readonly CloudBlobClient _cloudBlobClient;
        private readonly string _storageConnectionString;

        public AzBlobAndContainerUtils(string storageConnectionString)
        {
            _storageConnectionString = storageConnectionString;
            if (CloudStorageAccount.TryParse(_storageConnectionString, out _storageAccount))
            {
                _cloudBlobClient = _storageAccount.CreateCloudBlobClient();
            }
        }

        private async Task<IEnumerable<CloudBlobContainer>> ListContainers(string containerPrefix)
        {
            BlobContinuationToken continuationToken = null;
            var containers = new List<CloudBlobContainer>();

            do {
                var response = await _cloudBlobClient
                    .ListContainersSegmentedAsync(containerPrefix, continuationToken)
                    .ConfigureAwait(false);
                
                if(response.Results.Any())
                {
                    foreach (var resultContainer in response.Results)
                    {
                        var cr = _cloudBlobClient.GetContainerReference(resultContainer.Name);
                    }
                }
                containers.AddRange(response.Results);
                continuationToken = response.ContinuationToken;
            }
            while (continuationToken != null);
            return containers;
        }

        /// <summary>
        /// Get list of blobs as type List of BlobItem
        /// usage: var blobs = GetAllBlobsAsList(container).GetAwaiter().GetResult();
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        private async Task<List<BlobItem>> GetAllBlobsAsList(CloudBlobContainer container)
        {
            var blobs = new List<BlobItem>();
            BlobContinuationToken continuationToken = null;
            List<CloudAppendBlob> cab = new List<CloudAppendBlob>();
            do
            {
                var response = await container.ListBlobsSegmentedAsync(string.Empty, true, BlobListingDetails.None, new int?(), continuationToken, null, null);
                continuationToken = response.ContinuationToken;
                foreach (var blob in response.Results)
                {
                    Console.WriteLine(blob.Uri);
                    var b = (CloudBlockBlob) blob;
                    var bi = new BlobItem
                    {
                        Name = b.Name,
                        BlockBlob = b
                    };
                    blobs.Add(bi);
                }
            } while (continuationToken != null);
            return blobs;
        }

        /// <summary>
        /// Download Blobs to local filesystem
        /// usage: DownloadBlobs(blobs, localPathName).GetAwaiter().GetResult();
        /// </summary>
        /// <param name="blobs"></param>
        /// <param name="localPath"></param>
        /// <returns></returns>
        private static async Task DownloadBlobs(List<BlobItem> blobs, string localPath)
        {
            foreach (BlobItem blob in blobs)
            {
                CloudBlockBlob cbBlob = blob.BlockBlob.Container.GetBlockBlobReference(blob.Name);
                var path = PathUtils.FixAzureBlobPath(cbBlob.Name, localPath);
                var parent = cbBlob.Parent.Prefix;
                PathUtils.CreateDestinationDirectory(parent,localPath);
                await cbBlob.DownloadToFileAsync(path, FileMode.Create);
            }
        }

        
        public void Dispose()
        {
            
        }
    }
}
