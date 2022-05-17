using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Configuration;
using System.Globalization;
using System.Text;

using Renci.SshNet;
using System.Net.NetworkInformation;

namespace AnchorDownload
{
    internal static class Utils
    {
        internal static StringBuilder EmailLog = null;

        internal static void runUpdateSalesforceData()
        {
            runExe(@"C:\JBR Code\AnchorDownload\AnchorDownload\UpdateSalesforceData\bin\Debug\UpdateSalesforceData.exe", "");
        }

        internal static void ExtractFiles(string zipFolder, string workingFolder)
        {
            foreach (var compressedFile in Directory.GetFiles(zipFolder + @"\"))
            { //.Where(f => Path.GetExtension(f) == ".7z" &&
              //        Utils.ParseFileDate(f, out var dt) &&
              //        dt.Date == DateTime.Today.Date)) {
                Utils.extractFile(compressedFile, workingFolder);
            }
        }

        //internal static void downloadFiles(out string sqlBackupfileNameAndPath, out string zipFileNameAndPath)
        //{
        //    var localPath = ConfigurationManager.AppSettings["workingFolder"];
        //    var keyFile = new PrivateKeyFile(@"C:\JBR Code\private_key.ppk", ConfigurationManager.AppSettings["ftpPassword"]);
        //    var keyFiles = new[] { keyFile };
        //    var username = "userName";

        //    var methods = new List<AuthenticationMethod>
        //    {
        //        new PasswordAuthenticationMethod(username, ""),
        //        new PrivateKeyAuthenticationMethod(username, keyFiles)
        //    };

        //    var zipFileName = string.Empty;
        //    var sqlBackupfileName = "SQLBackupfilename.txt";

        //    var con = new ConnectionInfo("ftp.co.uk", 22, username, methods.ToArray());
        //    using (var client = new SftpClient(con))
        //    {
        //        client.Connect();

        //        downloadFile(localPath, sqlBackupfileName, client);

        //        zipFileName = readTextFileContents(localPath, sqlBackupfileName).Trim();
        //        downloadFile(localPath, zipFileName, client);

        //        client.Disconnect();
        //    }

        //    sqlBackupfileNameAndPath = string.Concat(localPath, sqlBackupfileName);
        //    zipFileNameAndPath = string.Concat(localPath, zipFileName);
        //}

        internal static void DownloadAllMissingFiles()
        {
            
            var username = ConfigurationManager.AppSettings["ftpUser"];

            using (var client = new SftpClient(
                                                new ConnectionInfo(ConfigurationManager.AppSettings["ftpAddress"], 
                                                22, 
                                                username,
                                                new List<AuthenticationMethod>
                                                {
                                                    new PasswordAuthenticationMethod(username, ""),
                                                    new PrivateKeyAuthenticationMethod(username, new[] { new PrivateKeyFile(@"C:\JBR\jbr_private_key.ppk",
                                                                                                             ConfigurationManager.AppSettings["ftpPassword"]) })
                                                }.ToArray())))
            {
                client.Connect();

            
                var zipDir = new DirectoryInfo(ConfigurationManager.AppSettings["zipFolder"]);
                foreach (var file in client.ListDirectory(".").Where(file => file.Name.Contains(DateTime.Now.ToString("-yyyyMMdd-")) && //DateTime.Now.ToString("-yyyyMMdd-"))
                                           !zipDir.GetFiles().Any(f => string.Equals(f.Name, file.Name, StringComparison.CurrentCultureIgnoreCase)
                                                                    && f.Length == file.Length)
                                           ))
                                           downloadFile(zipDir.FullName + @"\", file.Name, client);
                
                client.Disconnect();
            }
        }

        internal static void extractFile(string source, string destination)
        {
            runExe(@"C:\Program Files\7-Zip\7zG.exe", string.Concat("x \"", source, "\" -o\"", destination, "\" -y"));
        }

        internal static void runSqb2mtf(string source, string destination)
        {
            runExe(@"F:\JBRData\sqb2mtf.exe", string.Concat("\"", source, "\" \"", destination, "\"", " ", "\"", ConfigurationManager.AppSettings["Sqb2mtfPassword"] , "\""));
        }

        internal static void runScriptsSQLFile(string scriptFile)
        {
            runExe(@"C:\Program Files\Microsoft SQL Server\Client SDK\ODBC\170\Tools\Binn\sqlcmd.exe", string.Concat(@"-S ", ConfigurationManager.AppSettings["SQLServerName"] ," -i ", scriptFile));
        }

        //internal static void runRestoreScriptsSQL()
        //{
        //    runScriptsSQLFile(@"F:\JBRData\RestoreScripts.sql");
        //}

        internal static void runGeneratedRestoreScriptsSQL() => 
            runScriptsSQLFile(Path.Combine(ConfigurationManager.AppSettings["workingFolder"], "GeneratedRestoreScript.sql"));

        //internal static void runDeleteDatabasesSQL()
        //{
        //    runScriptsSQLFile(@"F:\JBRData\DeleteDatabasesScripts.sql");
        //}

        #region General Methods

        //internal static string readTextFileContents(string localPath, string SQLBackupfileName)
        //{
        //    var fileContents = string.Empty;

        //    try
        //    {
        //        using (StreamReader sr = new StreamReader(localPath + SQLBackupfileName))
        //        {
        //            fileContents = sr.ReadToEnd();
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        //Console.WriteLine("The file could not be read:");
        //        //Console.WriteLine(e.Message);
        //    }

        //    return fileContents;
        //}

        internal static void downloadFile(string localPath, string file, SftpClient client)
        {
            LogHelper.Logger.WriteOutput("Downloading file from SFTP: " + file, EmailLog); 
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

                Process x = new Process();
                x.StartInfo = pro;
                x.StartInfo.UseShellExecute = false;
                x.StartInfo.RedirectStandardInput = true;
                x.StartInfo.RedirectStandardOutput = true;
                x.StartInfo.RedirectStandardError = true;
                x.Start();

                string output = x.StandardOutput.ReadToEnd();
                string err = x.StandardError.ReadToEnd();

                if (output.Length > 0) { LogHelper.Logger.WriteOutput( "Restore Output: " + output, EmailLog); }
                if (err.Length > 0) { LogHelper.Logger.WriteOutput("Restore Error Output: " + err, EmailLog); }

                x.WaitForExit();
            }
            catch (Exception ex)
            {
                //DO logic here 
            }
        }

        //internal static void emptyFolder(DirectoryInfo directory)
        //{

        //    foreach (FileInfo file in directory.GetFiles())
        //    {
        //        deleteFile(file.FullName);
        //    }

        //    foreach (DirectoryInfo subDirectory in directory.GetDirectories())
        //    {
        //        emptyFolder(subDirectory);
        //    }

        //}

        //internal static void deleteFile(string filePathAndName)
        //{
        //    if (File.Exists(filePathAndName))
        //    {
        //        //File.SetAttributes(filePathAndName, FileAttributes.Normal);

        //        try { File.Delete(filePathAndName); }
        //        catch (Exception ex) { }

        //    }
        //}

        #endregion

        public static void DeleteOlderZipFiles(string path)
        {
            foreach (var file in Directory.GetFiles(path))
            {
                if (!file.Contains(DateTime.Now.ToString("-yyyyMMdd-")))
                    File.Delete(file);
            }
        }

        internal static void DeleteAllFilesAndFolders(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        //public static void DeleteOlderFiles(string path)
        //{
        //    //foreach (var file in Directory.GetFiles(path))
        //    //{
        //    //    !file.Contains(DateTime.Now.ToString("-yyyyMMdd-")
        //    //        File.Delete(file);
        //    //    //those has the same name everyday
        //    //    //we delete them and download the least everytime
        //    //    //SQLBackupfilename.txt
        //    //    //SQLBackupLOGfilename.txt
        //    //    //if (Path.GetExtension(file) == ".txt")
        //    //    //    File.Delete(file);
        //    //    //else
        //    //    //{
        //    //    //we always keep the full backups
        //    //    //even when they're older
        //    //    //and delete old logs
        //    //    //SQLBackup-copy-20191007-0238.7z
        //    //    //SQLBackupLOG-copy-20191007-0815.7z

        //    //    //if (Path.GetFileName(file).StartsWith("SQLBackup-copy")) continue;

        //    //    //files from yesterday or older than one day
        //    //    if (ParseFileDate(file, out var fileDate))
        //    //            if (fileDate.Day < DateTime.Now.Day || (DateTime.Now - fileDate).Days > 1)
        //    //                File.Delete(file);
        //    //    //}
        //    //}
        //}

        public static bool ParseFileDate(string filename, out DateTime dt)
        {
            try
            {
                return DateTime.TryParseExact(
                    Path.GetFileNameWithoutExtension(filename).Replace("LOG_H2S2_JBR_", string.Empty)
                        .Replace("FULL_H2S2_JBR_", string.Empty)
                        .Split(
                            new string[]
                            {
                                "SQLBackup-copy-", "SQLBackupLOG-copy-", "Collections_", "Proposal_", "S3CUSTDB_",
                                "S3DB01_", "S3DB02_", "DNS_"
                            },
                            StringSplitOptions.None)[1], new[] {"yyyyMMdd-HHmm", "yyyyMMdd_HHmmss"}, null,
                    DateTimeStyles.None, out dt);
            }
            catch
            {
                dt = DateTime.Now;
                return false;
            }
        }

        public static void GenerateSQLRestoreScript(List<string> fullFilesList, List<string> logFilesList)
        {
            var workingDirectory = ConfigurationManager.AppSettings["workingFolder"];
            var backupFolder = ConfigurationManager.AppSettings["backupFolder"];
            var backupLogFolder = ConfigurationManager.AppSettings["backupLogFolder"];

            var databases = new[]
            {
                new { databaseName = "JBR_Collections", fileName = "JBR_Collections" },
                new { databaseName = "JBR_Proposals", fileName = "JBR_Proposal" },
                new { databaseName = "JBR_S3CUSTDB", fileName = "JBR_S3CUSTDB" },
                new { databaseName = "JBR_S3DB01", fileName = "JBR_S3DB01" },
            };
            
            var sb = new StringBuilder();

            sb.AppendLine("use master");
            sb.AppendLine("GO");
            sb.AppendLine();

            foreach (var db in databases)
            {
                var fileToExecute = fullFilesList.FirstOrDefault(f => f.Contains(db.fileName));
                if(fileToExecute == null) continue;

                ///TODO: 4-8-21: DAVID TOOK THIS OFF AS THE LOG FILES DONT RESTORE PROPERLY, IF ANCHOR CONFIRM ITS WORKING THEN RESTORE THIS
                //var hasLogFiles = logFilesList.Any(f => f.Contains(db.fileName));
                var hasLogFiles = false;

                sb.AppendLine($"Execute JBR_Internal.dbo.KillConnections '{db.databaseName}'");
                sb.AppendLine("GO");
                sb.AppendLine($"PRINT 'Restoring {db.databaseName}....'");
                sb.AppendLine("PRINT CHAR(13) + CHAR(10)");
                sb.AppendLine($"RESTORE DATABASE [{db.databaseName}] FROM");
                for (int i = 0; i < 4; i++)
                    sb.AppendLine(String.Concat($"DISK = N'{Path.Combine(backupFolder, $"{fileToExecute}_0{i}.bak")}'", i == 3 ? string.Empty : ","));
                sb.AppendLine(String.Concat("WITH REPLACE" , hasLogFiles ? ", NORECOVERY" : string.Empty));
                sb.AppendLine("GO");
                sb.AppendLine("PRINT CHAR(13) + CHAR(10)");
                sb.AppendLine();

                ///TODO: 4-8-21: DAVID TOOK THIS OFF AS THE LOG FILES DONT RESTORE PROPERLY, IF ANCHOR CONFIRM ITS WORKING THEN RESTORE THIS
                //if(!hasLogFiles) continue;

                //var filesToExecute = logFilesList.Where(f => f.Contains(db.fileName)).OrderBy(f =>
                //    {
                //        ParseFileDate(f, out var dt);
                //        return dt;
                //    }).ToList();

                
                //foreach (var f in filesToExecute)
                //{
                //    sb.AppendLine($"RESTORE LOG [{db.databaseName}] FROM");
                
                //    for (int i = 0; i < 2; i++)
                //        sb.AppendLine(String.Concat($"DISK = N'{Path.Combine(backupLogFolder, $"{f}_0{i}.bak")}'", i == 1 ? string.Empty : ","));

                //    if (f != filesToExecute.Last()) { sb.AppendLine("WITH NORECOVERY"); }
                //    sb.AppendLine("GO");
                //}

                //sb.AppendLine();
                //sb.AppendLine();
            }

            File.WriteAllText(Path.Combine(workingDirectory, "GeneratedRestoreScript.sql"), sb.ToString());
        }

        public static List<string> runSqb2mtfAndGetListOfFilesToExecute(string folder, string[] filesOfInterest)
        {
            List<string> NamesOfFilesToExecute = new List<string>();
            foreach (var foi in Directory.GetFiles(folder, "*.sqb", SearchOption.AllDirectories)
                .Where(f => filesOfInterest.Any(Path.GetFileName(f).StartsWith)))
            {
                Utils.runSqb2mtf(foi,
                    Path.Combine(folder, $"{Path.GetFileNameWithoutExtension(foi)}.bak"));
                NamesOfFilesToExecute.Add(Path.GetFileNameWithoutExtension(foi));
            }

            return NamesOfFilesToExecute;
        }
    }
}
