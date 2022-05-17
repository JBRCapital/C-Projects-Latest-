using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net.Mail;
using SMTPHelper;

namespace AutoDocHelper
{
    public class ZipFunctions
    {
        public static void DeleteZipFileIfExists(string zipPath)
        {
            if (File.Exists(zipPath)) { File.Delete(zipPath); }
        }

        public static void DeleteZipFileIsExistsAndRecreate(string zipPath, string pdfOutputFolder)
        {
            DeleteZipFileIfExists(zipPath);
            //Create ZipFile
            ZipFile.CreateFromDirectory(pdfOutputFolder, zipPath);
        }

        public static string ZipFilesAndSendToEmail(string pdfOutputFolder, string zipFolder)
        {
            string zipPath = string.Concat(zipFolder, "Annual Statements - ", DateTime.Now.ToString("yyyy-MM"), "-01.zip");
            ZipFunctions.DeleteZipFileIsExistsAndRecreate(zipPath, pdfOutputFolder);
            SMTPHelper.Helpers.CreateEmailSender(out SmtpClient client, out email email, " - " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Today.Month) + "-" + DateTime.Today.Year);
            SMTPHelper.Helpers.SendEmail(client, email, zipPath);
            System.Threading.Thread.Sleep(5000);
            return zipPath;
        }

    }
}
