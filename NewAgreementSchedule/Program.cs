using SMTPHelper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using AutoDocHelper;
using DocxFromTemplateGenerator;

namespace AnnualStatementsGenerator
{
    class Program
    {
        internal static string
        templateFolder = ConfigurationManager.AppSettings["TemplateFolder"],
        pdfOutputFolder = ConfigurationManager.AppSettings["PdfOutputFolder"],

        newAgreementStatementsFileTemplate = ConfigurationManager.AppSettings["NewAgreementStatementsFile"],

        outputFileName = ConfigurationManager.AppSettings["OutputFile"],

        //ftpServer = ConfigurationManager.AppSettings["ftpServer"],
        //ftpPath = ConfigurationManager.AppSettings["ftpPath"],
        //ftpUserName = ConfigurationManager.AppSettings["ftpUserName"],
        //ftpPassword = ConfigurationManager.AppSettings["ftpPassword"],

        //clicksendCollectFileURL = ConfigurationManager.AppSettings["clicksendCollectFileURL"],
        //clicksendUserName = ConfigurationManager.AppSettings["clicksendUserName"],
        //clicksendPassword = ConfigurationManager.AppSettings["clicksendPassword"],

        SMTPSubject = ConfigurationManager.AppSettings["SMTPSubject"],
        SMTPBody = ConfigurationManager.AppSettings["SMTPBody"],

        zipFolder = ConfigurationManager.AppSettings["zipFolder"];

        static void Main(string[] args)
        {
            FolderAndFileFunctions.DeleteOutputFolderContents(pdfOutputFolder);

            var reader = DataHelper.GetFirstStatementDataFromSQL();

            List<string> successfulAgreementSends = new List<string>();
            List<string> unsuccessfulAgreementSends = new List<string>();

            //List<string> successfulFinalAgreementIds = new List<string>();
            //List<string> failedAgreementIds = new List<string>();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    if (!SMTPHelper.Helpers.IsValidEmail(reader["CustomerEmail"].ToString(), false)) {
                        unsuccessfulAgreementSends.Add(string.Concat(reader["AgreementNumber"].ToString(), ", " + reader["CustomerFullName"].ToString(), reader["CustomerCompanyOriginal"].ToString() != string.Empty ? " (" + reader["CustomerCompanyOriginal"].ToString() + ")" : String.Empty, ", " + reader["CustomerEmail"].ToString()));
                        continue;
                    }

                    DataTable payProfile = DataHelper.GetPayProfileTable(reader["AgreementNumber"].ToString(), DateTime.Parse(reader["AgreementDate"].ToString()).ToString("yyyy-MM-dd"));

                    var statementPath = GenerateDocument.Generate(

                        Path.Combine(Program.templateFolder, Program.newAgreementStatementsFileTemplate),
                        String.Concat(Path.Combine(Program.pdfOutputFolder, Program.outputFileName), " - ",
                        reader["AgreementNumber"], " - Pay Profile - ", string.Empty, Guid.NewGuid(), ".pdf"),

                        DocumentInformation.GetDocumentInformationForFirstStatement(reader), DocTemplateFunctions.PostDocumentFunctions,
                        new Dictionary<string, DataTable> { { "transactions", null }, { "schedule", payProfile } }
                        );

                 
                    SMTPHelper.Helpers.SendEmail(SMTPHelper.Helpers.CreateSMTPClient(),
                     new email(
                      reader["CustomerEmail"].ToString(), //ConfigurationManager.AppSettings["SMTPLogTo"], 
                      reader["CustomerEmail"].ToString(), //reader["CustomerShortName"].ToString() //ConfigurationManager.AppSettings["SMTPLogToNAME"],
                      ReplaceTokens(reader, SMTPSubject),
                      ReplaceTokens(reader, System.Web.HttpUtility.HtmlDecode(SMTPBody)),
                      ConfigurationManager.AppSettings["SMTPSender"],
                      ConfigurationManager.AppSettings["SMTPSenderName"],
                      new List<string>(),
                      new List<string>(ConfigurationManager.AppSettings["SMTPBCCTo"].Split(",")) 
                        ), statementPath, true);
                    


                    //if (sendAnnualStatementLetter(statementPath, Sendclick.getSendClickValues(reader), reader["CustomerLetterName"].ToString()))
                    //{
                    //    if (agreementFinal)
                    //        successfulFinalAgreementIds.Add(reader["AgreementNumber"].ToString());
                    //    else
                    successfulAgreementSends.Add(string.Concat(reader["AgreementNumber"].ToString(), ", " + reader["CustomerFullName"].ToString() , reader["CustomerCompanyOriginal"].ToString() != string.Empty ? " (" + reader["CustomerCompanyOriginal"].ToString() + ")": String.Empty ,  ", " + reader["CustomerEmail"].ToString()));
                    //}
                    //else
                    //{
                    //    failedAgreementIds.Add(reader["AgreementNumber"].ToString());
                    //}
                }
            }
            else
            {
                Console.WriteLine("No rows found.");
            }
            reader.Close();


            ////Delete the files copied to the FTP
            //Console.WriteLine("Deleting the files copied to the FTP");
            //FTPHelper.Helpers.DeleteDirectoryFiles(ftpUserName, ftpPassword, ftpServer, ftpPath);

            ////Zip the files locally and send them by email
            //Console.WriteLine("Zipping the PDF and address files created locally and sending them by email");
            //string zipPath = ZipFunctions.ZipFilesAndSendToEmail(pdfOutputFolder, zipFolder);

            //Delete all the local files including the zip
            Console.WriteLine("Deleting all the local files including the zip");
            FolderAndFileFunctions.DeleteOutputFolderContents(pdfOutputFolder);

            SQLDataHelper.Helper.GetSqlConnection(ConfigurationManager.AppSettings["SQLConnectionString"]);
            SQLDataHelper.Helper.RunCommand("FirstStatements_AgreementData_SetSelectionStartDate_TOBEREPLACEDWITHBONAFIDEE", CommandType.StoredProcedure, null);

            sendLogToJacoboAndDavid(unsuccessfulAgreementSends, "UNSUCCESFUL - " + ConfigurationManager.AppSettings["SMTPLogSubject"]);
            sendLogToJacoboAndDavid(successfulAgreementSends, ConfigurationManager.AppSettings["SMTPLogSubject"]);


            //ZipFunctions.DeleteZipFileIfExists(zipPath);

            //AnnualStatementsSender.EmailLogOfAnnualStatements(successfulAgreementIds, successfulFinalAgreementIds, failedAgreementIds);

            //updateSalesForceAgreementStatementData(successfullAgreementIds, successfullFinalAgreementIds);

            //Console.ReadLine();
        }

        private static void sendLogToJacoboAndDavid(List<string> agreementSends, string subject)
        {
            if (agreementSends.Count == 0) return;

            SMTPHelper.Helpers.SendEmail(SMTPHelper.Helpers.CreateSMTPClient(),
             new email(
              ConfigurationManager.AppSettings["SMTPLogTo"],
              ConfigurationManager.AppSettings["SMTPLogToNAME"],
              subject,
              string.Join(Environment.NewLine, agreementSends),
              ConfigurationManager.AppSettings["SMTPSender"],
              ConfigurationManager.AppSettings["SMTPSenderName"],
              new List<string>(),
              new List<string>()
                ),null , false);
        }

        static string ReplaceTokens(SqlDataReader reader, string inputString) {

            return inputString//.Replace("[Address]", DocumentInformation.getAddress(reader))
            //.Replace("[Date]", DateTime.Now.ToString("01/MM/yyyy"))
            .Replace("[CustomerAndCompanyName]", reader["CustomerShortName"].ToString() + (reader["CustomerCompanyOriginal"].ToString() != String.Empty ? " (" + reader["CustomerCompanyOriginal"].ToString() + ")" : string.Empty))
            //.Replace("[AgreementType]", reader["AgreementType"].ToString())
            .Replace("[AgreementReference]", reader["AgreementNumber"].ToString())
            //.Replace("[AccountHolders]", reader["CustomerNames"].ToString())
            //.Replace("[StatementPeriodStart]", reader["StatementStartDate"].ToString())
            //.Replace("[StatementPeriodEnd]", reader["StatementEndDate"].ToString())
            //.Replace("[RegistrationPlates]", reader["VehicleRegNo"].ToString())
            .Replace("[VehicleDetails]", reader["VehicleDescription"].ToString())
            //.Replace("[StartDateOfAgreement]", reader["AgreementDate"].ToString())
            //.Replace("[AgreementTermInMonths]", reader["AgreementDuration"].ToString())
            //.Replace("[Advance]", reader["AgreementAdvance"].ToString())
            //.Replace("[APR]", reader["AgreementAPR"].ToString())
            .Replace("[DocFee]", "£"+Double.Parse(reader["DocFee"].ToString()).ToString("N2"));
        }

    }
}
