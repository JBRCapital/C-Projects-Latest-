using System;
using System.Collections.Generic;
using System.Configuration;
using AutoDocHelper;

namespace AnnualStatementsGenerator
{
    class Program
    {
        internal static string
        templateFolder = ConfigurationManager.AppSettings["TemplateFolder"],
        pdfOutputFolder = ConfigurationManager.AppSettings["PdfOutputFolder"],

        unregulatedTemplate = ConfigurationManager.AppSettings["UnregulatedTemplateFile"],
        regulatedTemplateWithSchedule = ConfigurationManager.AppSettings["RegulatedTemplateWithScheduleFile"],
        regulatedTemplateWithoutSchedule = ConfigurationManager.AppSettings["RegulatedTemplateWithoutScheduleFile"],
        regulatedTemplateFinalWithoutSchedule = ConfigurationManager.AppSettings["RegulatedTemplateFinalWithoutScheduleFile"],

        outputFileName = ConfigurationManager.AppSettings["OutputFile"],

        clicksendUserName = ConfigurationManager.AppSettings["clicksendUserName"],
        clicksendPassword = ConfigurationManager.AppSettings["clicksendPassword"],

        zipFolder = ConfigurationManager.AppSettings["zipFolder"];

        static void Main(string[] args)
        {
            FolderAndFileFunctions.DeleteOutputFolderContents(pdfOutputFolder);

            AnnualStatementsSender.sendAnnualStatementLetters(DataHelper.GetAnnualStatementDataFromSQL(),
                                       out List<string> successfulAgreementIds,
                                       out List<string> successfulFinalAgreementIds,
                                       out List<string> failedAgreementIds);

            //Delete the files copied to the FTP
            //Console.WriteLine("Deleting the files copied to the FTP");
            //FTPHelper.Helpers.DeleteDirectoryFiles(ftpUserName, ftpPassword, ftpServer, ftpPath);

            //Zip the files locally and send them by email
            Console.WriteLine("Zipping the PDF and address files created locally and sending them by email");
            string zipPath = ZipFunctions.ZipFilesAndSendToEmail(pdfOutputFolder, zipFolder);

            //Delete all the local files including the zip
            Console.WriteLine("Deleting all the local files including the zip");
            FolderAndFileFunctions.DeleteOutputFolderContents(pdfOutputFolder);
            ZipFunctions.DeleteZipFileIfExists(zipPath);

            AnnualStatementsSender.EmailLogOfAnnualStatements(successfulAgreementIds, successfulFinalAgreementIds, failedAgreementIds);

            //updateSalesForceAgreementStatementData(successfullAgreementIds, successfullFinalAgreementIds);

            //Console.ReadLine();
        }

    }
}