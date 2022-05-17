using System;
using System.Collections.Generic;
using System.IO;

using Renci.SshNet;
using System.Diagnostics;

namespace AnchorDownload
{
    internal static class Utils
    {
        internal static void runUpdateSalesforceData()
        {
            runExe(@"C:\JBR Code\AnchorDownload\AnchorDownload\UpdateSalesforceData\bin\Debug\UpdateSalesforceData.exe", "");
        }

        internal static void downloadFiles(out string sqlBackupfileNameAndPath, out string zipFileNameAndPath)
        {
            var localPath = @"C:\JBRData\";
            var keyFile = new PrivateKeyFile(@"C:\JBR Code\AnchorDownload\AnchorDownload\jbr_private_key.ppk", "KKKnhkU3387Yye6");
            var keyFiles = new[] { keyFile };
            var username = "jbr";

            var methods = new List<AuthenticationMethod>
            {
                new PasswordAuthenticationMethod(username, ""),
                new PrivateKeyAuthenticationMethod(username, keyFiles)
            };

            var zipFileName = string.Empty;
            var sqlBackupfileName = "SQLBackupfilename.txt";

            var con = new ConnectionInfo("fileshare.anchor.co.uk", 22, username, methods.ToArray());
            using (var client = new SftpClient(con))
            {
                client.Connect();

                downloadFile(localPath, sqlBackupfileName, client);

                zipFileName = readTextFileContents(localPath, sqlBackupfileName).Trim();
                downloadFile(localPath, zipFileName, client);

                client.Disconnect();
            }

            sqlBackupfileNameAndPath = string.Concat(localPath, sqlBackupfileName);
            zipFileNameAndPath = string.Concat(localPath, zipFileName);
        }

        internal static void extractFile(string source, string destination)
        {
            runExe(@"C:\Program Files\7-Zip\7zG.exe", string.Concat("x \"", source, "\" -o\"", destination, "\" -y"));
        }

        internal static void runSqb2mtf(string source, string destination)
        {
            runExe(@"C:\JBRData\sqb2mtf.exe", string.Concat("\"", source, "\" \"", destination, "\"", " ", "\"Xoh8iep8ETheimaiZ4Sh\""));
        }

        internal static void runScriptsSQLFile(string scriptFile)
        {
            runExe(@"C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\sqlcmd.exe", string.Concat(@"-S STEPHAM2 -i ", scriptFile));
        }

        internal static void runRestoreScriptsSQL()
        {
            runScriptsSQLFile(@"C:\JBRData\RestoreScripts.sql");
        }

        internal static void runDeleteDatabasesSQL()
        {
            runScriptsSQLFile(@"C:\JBRData\DeleteDatabasesScripts.sql");
        }

        #region General Methods

        internal static string readTextFileContents(string localPath, string SQLBackupfileName)
        {
            var fileContents = string.Empty;

            try
            {
                using (StreamReader sr = new StreamReader(localPath + SQLBackupfileName))
                {
                    fileContents = sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("The file could not be read:");
                //Console.WriteLine(e.Message);
            }

            return fileContents;
        }

        internal static void downloadFile(string localPath, string file, SftpClient client)
        {
            Console.WriteLine(file);
            using (var fs = new FileStream(localPath + file, FileMode.Create))
            {
                client.DownloadFile(file, fs);
                fs.Close();
            }

            //return SQLBackupfile;
        }

        internal static void runExe(string exePath, string args)
        {
            try
            {
                ProcessStartInfo pro = new ProcessStartInfo();
                pro.WindowStyle = ProcessWindowStyle.Hidden;
                pro.FileName = exePath;
                pro.Arguments = args;
                Process x = Process.Start(pro);
                x.WaitForExit();
            }
            catch (Exception ex)
            {
                //DO logic here 
            }
        }

        internal static void emptyFolder(DirectoryInfo directory)
        {

            foreach (FileInfo file in directory.GetFiles())
            {
                deleteFile(file.FullName);
            }

            foreach (DirectoryInfo subDirectory in directory.GetDirectories())
            {
                emptyFolder(subDirectory);
            }

        }

        internal static void deleteFile(string filePathAndName)
        {
            if (File.Exists(filePathAndName))
            {
                //File.SetAttributes(filePathAndName, FileAttributes.Normal);

                try { File.Delete(filePathAndName); }
                catch (Exception ex) { }

            }
        }

        #endregion
    }
}
