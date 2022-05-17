using System.Collections.Generic;
using System.IO;
using System.Net;

namespace FTPHelper
{
    public static class Helpers
    {
        public static void UploadTheFileToFTP(string userName, string password, string remoteFtpPath, string pdfFilePath)
        {
            using (var client = new WebClient())
            {
                client.Credentials = new NetworkCredential(userName, password);
                client.UploadFile(remoteFtpPath, WebRequestMethods.Ftp.UploadFile, pdfFilePath);
            }
        }


        public static List<string> ListDirectory(string userName, string password, string ftpPath)
        {
            var fileList = new List<string>();

            var request = (FtpWebRequest)WebRequest.Create(ftpPath);
            request.Credentials = new NetworkCredential(userName, password);
            request.Method = WebRequestMethods.Ftp.ListDirectory;

            using (var response = (FtpWebResponse)request.GetResponse())
            {
                using (var stream = response.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream, true))
                    {
                        while (!reader.EndOfStream)
                        {
                            fileList.Add(reader.ReadLine());
                        }
                    }
                }
            }

            return fileList;
        }

        public static bool DeleteDirectoryFiles(string userName, string password, string ftpServer, string ftpPath)
        {
            foreach (var file in ListDirectory(userName, password, string.Concat(ftpServer,'/', ftpPath)))
            {
                var response = DeleteFile(userName, password, string.Concat(ftpServer, '/', file));
                if (!response.Contains("250")) {
                    return false;
                }
            }

            return true;
        }

        public static string DeleteFile(string userName, string password,string ftpServerPathAndFileName)
        {
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(ftpServerPathAndFileName);
            request.Method = WebRequestMethods.Ftp.DeleteFile;
            request.Credentials = new NetworkCredential(userName, password);

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                return response.StatusDescription;
            }
        }
    }
}
