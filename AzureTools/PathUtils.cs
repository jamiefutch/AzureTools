using System;
using System.IO;


namespace AzureTools
{
    public static class PathUtils
    {
        public static string FixAzureBlobPath(string blobName, string localPath)
        {
            string retval = blobName.Replace('/', '\\');
            retval = Path.Combine(localPath, retval);
            return retval;
        }

        public static void CreateDestinationDirectory(string blobDirectoryPrefix, string localPath)
        {
            var localpathCombined = FixAzureBlobPath(blobDirectoryPrefix, localPath);
            if (!Directory.Exists(localpathCombined))
            {
                try
                {
                    Directory.CreateDirectory(localpathCombined);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}
