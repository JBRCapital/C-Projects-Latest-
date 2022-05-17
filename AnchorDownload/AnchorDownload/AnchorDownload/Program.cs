using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AnchorDownload
{
    class AnchorDownload
    {
        public static int Main()
        {
            try
            {
                Console.WriteLine("Downloading files...");
                string sqlBackupFileNameAndPath = string.Empty, zipFileAndPath = string.Empty;
                Utils.downloadFiles(out sqlBackupFileNameAndPath, out zipFileAndPath);

                //sqlBackupFileNameAndPath = @"C:\JBRData\SQLBackupfilename.txt";
                //zipFileAndPath = @"C:\JBRData\SQLBackup-copy-20170126-0232.7z";

                Console.WriteLine("Create the backup folder if it doesnt exist");
                var backupFolder = @"C:\JBRData\backup";
                //Directory.CreateDirectory(backupFolder);

                Console.WriteLine(string.Concat("Deleting files from ", backupFolder));
                //Delete all contents of the C:\JBRData\backup folder
                Utils.emptyFolder(new DirectoryInfo(backupFolder));

                Console.WriteLine(string.Concat("Extracting files ", zipFileAndPath, " to ", backupFolder));
                //Open the 
                Utils.extractFile(zipFileAndPath, @"C:\JBRData");// backupFolder);

                Console.WriteLine("Restoring files with Sqb2mtf");
                //Sqb2mtf the backups
                foreach (var fileName in Directory.GetFiles(backupFolder, "*.sqb", SearchOption.AllDirectories))
                {
                    if (fileName.Contains("FULL_H2S2_JBR_Collections_")) { Utils.runSqb2mtf(fileName, @"c:\JBRData\backup\JBR_Collections.bak"); }
                    if (fileName.Contains("FULL_H2S2_JBR_Proposal_")) { Utils.runSqb2mtf(fileName, @"c:\JBRData\backup\JBR_Proposal.bak"); }
                    if (fileName.Contains("FULL_H2S2_JBR_S3CUSTDB_")) { Utils.runSqb2mtf(fileName, @"c:\JBRData\backup\JBR_S3CUSTDB.bak"); }
                    if (fileName.Contains("FULL_H2S2_JBR_S3DB01_")) { Utils.runSqb2mtf(fileName, @"c:\JBRData\backup\JBR_S3DB01.bak"); }
                }

                Console.WriteLine("Restoring SQL databases using restore script");
                Utils.runRestoreScriptsSQL();


                Console.WriteLine("Run UpdateSalesforceData, this can take a few minutes...");
                //Utils.runUpdateSalesforceData();

                Console.WriteLine("Deleting SQL databases using delete script");
                //Utils.runDeleteDatabasesSQL();

                //Delete all contents of the C:\JBRData\backup folder
                Console.WriteLine(string.Concat("Deleting files from", backupFolder));
                Utils.emptyFolder(new DirectoryInfo(backupFolder));

                //Delete zip
                Console.WriteLine(string.Concat("Deleting file ", zipFileAndPath));
                Utils.deleteFile(zipFileAndPath);
                Console.WriteLine(string.Concat("Deleting file ", sqlBackupFileNameAndPath));
                Utils.deleteFile(sqlBackupFileNameAndPath);

                Console.WriteLine("Sending Email");

                SendEmail().Wait();

                Console.WriteLine("Email Sent");

                //Console.ReadKey();
                return 0;
            }
            catch (Exception ex)
            {
                ExceptionLogging.ExceptionLogging.Write(ex);
                return 0;
            }
        }

        static async Task SendEmail()
        {
            var apiKey = "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";
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress("david.scheiner@jbrcapital.com", "David Scheiner");
            var subject = DateTime.Now.ToString("dddd, dd MMMM yyyy, h:mm tt") + " - Sql Overnight Download / Restore Completed";
            var to = new EmailAddress("david.scheiner@jbrcapital.com", "David Scheiner");
            var plainTextContent = DateTime.Now.ToString("dddd, dd MMMM yyyy, h:mm tt") + " - Sql Overnight Download / Restore Completed";
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, plainTextContent);
            var response = await client.SendEmailAsync(msg);
        }
}
}