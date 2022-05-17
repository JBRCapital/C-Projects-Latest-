using Renci.SshNet.Security;
using SMTPHelper;
using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Net.Mail;
using System.Text;

namespace AnchorDownload
{
    class AnchorDownload
    {
        
        public static StringBuilder EmailLog = new StringBuilder();

        public static void Main()
        {
            
            Utils.EmailLog = EmailLog;

            try
            {
                var zipFolder = ConfigurationManager.AppSettings["zipFolder"];
                var workingFolder = ConfigurationManager.AppSettings["workingFolder"];

                var backupFolder = ConfigurationManager.AppSettings["backupFolder"];
                var backupLogFolder = ConfigurationManager.AppSettings["backupLogFolder"];

                LogHelper.Logger.WriteOutput("Creating missing directories...", EmailLog);
                if (!Directory.Exists(workingFolder)) Directory.CreateDirectory(workingFolder);
                if (!Directory.Exists(zipFolder)) Directory.CreateDirectory(zipFolder);

                LogHelper.Logger.WriteOutput("Delete older 7z files...", EmailLog);
                Utils.DeleteOlderZipFiles(zipFolder);

                LogHelper.Logger.WriteOutput("Delete working files and folders...", EmailLog);
                Utils.DeleteAllFilesAndFolders(workingFolder);

                if (!Directory.Exists(backupFolder)) Directory.CreateDirectory(backupFolder);
                if (!Directory.Exists(backupLogFolder)) Directory.CreateDirectory(backupLogFolder);

                //Utils.DeleteOlderFiles(backupFolder);
                //Utils.DeleteOlderFiles(backupLogFolder);

                LogHelper.Logger.WriteOutput("Downloading any missing 7z files from SFTP...", EmailLog);
                Utils.DownloadAllMissingFiles();

                LogHelper.Logger.WriteOutput("Extracting 7z files in their appropriate directories...", EmailLog);
                Utils.ExtractFiles(zipFolder, workingFolder);

                LogHelper.Logger.WriteOutput("Converting *.sqb files to *.bak and Generating SQL Restore script", EmailLog);
                Utils.GenerateSQLRestoreScript(
                    Utils.runSqb2mtfAndGetListOfFilesToExecute(
                    ConfigurationManager.AppSettings["backupFolder"],
                    new[] { "FULL_H2S2_JBR_Collections_", "FULL_H2S2_JBR_Proposal_", "FULL_H2S2_JBR_S3CUSTDB_", "FULL_H2S2_JBR_S3DB01_" }),

                    Utils.runSqb2mtfAndGetListOfFilesToExecute(
                    ConfigurationManager.AppSettings["backupLogFolder"],
                    new[] { "LOG_H2S2_JBR_Collections_", "LOG_H2S2_JBR_Proposal_", "LOG_H2S2_JBR_S3CUSTDB_", "LOG_H2S2_JBR_S3DB01_" })
                
                    );

                LogHelper.Logger.WriteOutput("Running generated SQL Restore script", EmailLog);
                Utils.runGeneratedRestoreScriptsSQL();

                //Console.WriteLine("Run UpdateSalesforceData, this can take a few minutes...");
                //Utils.runUpdateSalesforceData();

                //return 0;
            }
            catch (Exception ex)
            {
                LogHelper.Logger.WriteOutput(string.Concat("Error:", Environment.NewLine, LogHelper.Logger.GetExceptionDetails(ex)), EmailLog);
                //ExceptionLogging.ExceptionLogging.Write(ex);
            }
            finally
            {
                LogHelper.Logger.WriteOutput(string.Concat("Finished - ", DateTime.Now.ToString()), EmailLog);

                Helpers.CreateEmailSender(out SmtpClient client, out email email, " - " + DateTime.Today.Day + "-" + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Today.Month) + "-" + DateTime.Today.Year);
                email.body = EmailLog.ToString();
                Helpers.SendEmail(client, email, null);

                Environment.Exit(1);
            }
        }
    }
}