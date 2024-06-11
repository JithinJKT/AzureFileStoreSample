using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using System.Text;

namespace AzureFileStoreSample.Utils
{
    public class AzureFileShareServices
    {
        private static ShareClient m_ShareClient;

        private static string ConnectionString { get; set; }
        private static string ShareName { get; set; }
        public static string BaseFolderPath { get; set; }

        public static void SetConnectionString(string strConnectionString, string shareName, string baseFolderPath)
        {
            //set connection string for further reference            
            ConnectionString = strConnectionString;
            ShareName = shareName;
            BaseFolderPath = baseFolderPath;

        }
        public async Task<string> GetFileBase64Async(string uploadedfilePath, string fileNamewithextensions)
        {
            try
            {
                m_ShareClient = new ShareClient(ConnectionString, ShareName);
                ShareDirectoryClient directory = m_ShareClient.GetDirectoryClient(BaseFolderPath + uploadedfilePath);
                ShareFileClient file = directory.GetFileClient(fileNamewithextensions);
                String FileData = string.Empty;
                // Download the file
                ShareFileDownloadInfo download = await file.DownloadAsync();
                if (download != null)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        download.Content.CopyTo(ms);
                        Byte[] bytes = ms.ToArray();
                        FileData = Convert.ToBase64String(bytes);
                    }                   
                   
                }
                return FileData;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                m_ShareClient.Exists();
            }
        }
        public async Task<string> GetFileTextAsync(string uploadedfilePath, string fileNamewithextensions)
        {
            try
            {
                m_ShareClient = new ShareClient(ConnectionString, ShareName);
                ShareDirectoryClient directory = m_ShareClient.GetDirectoryClient(BaseFolderPath + uploadedfilePath);
                ShareFileClient file = directory.GetFileClient(fileNamewithextensions);
                string text = string.Empty;
                // Download the file
                ShareFileDownloadInfo download = await file.DownloadAsync();
                if (download != null)
                {
                    StreamReader reader = new StreamReader(download.Content, Encoding.GetEncoding("iso-8859-1"));
                    text = reader.ReadToEnd();
                }
                return text;
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                m_ShareClient.Exists();
            }
        }
      
        public async Task<bool> UploadTextFileToAzure(string uploadingPath, string fileNamewithextensions, string Content)
        {
            try
            {
                m_ShareClient = new ShareClient(ConnectionString, ShareName);
                string longpath = BaseFolderPath + uploadingPath;
                

                //creating directory if not exixt
                var dir = m_ShareClient.GetRootDirectoryClient();
                var pathChain = longpath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                int dirCount = pathChain.Length - 1;

                for (int i = 0; i < dirCount; ++i)
                {
                    dir = dir.GetSubdirectoryClient(pathChain[i]);
                    await dir.CreateIfNotExistsAsync();
                }

                //saving file
                ShareDirectoryClient directory = m_ShareClient.GetDirectoryClient(longpath);
                ShareFileClient FileClient = directory.GetFileClient(fileNamewithextensions);
                Encoding encoding = Encoding.GetEncoding("iso-8859-1");
                var byt = encoding.GetBytes(Content);
                var stream = new MemoryStream(byt);
                //Check if the file already exists.
                if (!FileClient.Exists())
                {
                    //Create an empty file if the file doesn't exist.
                    await FileClient.CreateAsync(stream.Length);
                }
                ShareFileUploadInfo shareFileUploadInfo = await FileClient.UploadAsync(stream);
                if (shareFileUploadInfo != null)
                {
                    return true;
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                m_ShareClient.Exists();
            }
        }

    }
}
