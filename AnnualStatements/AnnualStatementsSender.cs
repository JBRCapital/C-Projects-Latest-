using AutoDocHelper;
using ClickSendHelper;
using DocxFromTemplateGenerator;
using SMTPHelper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;

namespace AnnualStatementsGenerator
{
    internal class AnnualStatementsSender
    {

        internal static void sendAnnualStatementLetters(SqlDataReader reader,
                                        out List<string> successfulAgreementIds,
                                        out List<string> successfulFinalAgreementIds,
                                        out List<string> failedAgreementIds)
        {

            successfulAgreementIds = new List<string>();
            successfulFinalAgreementIds = new List<string>();
            failedAgreementIds = new List<string>();


            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    string agreementNumber = reader["AgreementNumber"].ToString();

                    bool agreementRegulated = DataHelper.GetBoolean(reader["AgreementRegulated"]).Value;
                    bool agreementFinal = reader["StatementType"].ToString() == "Final Statement";

                    DataTable transactions = agreementRegulated ? DataHelper.GetTransactionsTable(agreementNumber, DateTime.Now.ToString("yyyy-MM") + "-01") : null;
                    DataTable payProfile = agreementFinal ? null : DataHelper.GetPayProfileTable(agreementNumber, DateTime.Now.ToString("yyyy-MM") + "-01");

                    var statementPath = GenerateDocument.Generate(
                    Path.Combine(Program.templateFolder, !agreementRegulated ?
                                 //Is Unregulated - Annual Statement
                                 Program.unregulatedTemplate :
                                 //Is Regulated - Final Statment
                                 agreementFinal ?
                                 Program.regulatedTemplateFinalWithoutSchedule :
                                 //Is Regulated - Annual Statement
                                 payProfile == null ?
                                //Is Regulated - Annual Statement - Without Schedule
                                 Program.regulatedTemplateWithoutSchedule :
                                 //Is Regulated - Annual Statement - With Schedule
                                 Program.regulatedTemplateWithSchedule),
                    String.Concat(Path.Combine(Program.pdfOutputFolder, Program.outputFileName), " - ", reader["AgreementNumber"], " - ", agreementRegulated ? "regulated" : "unregulated", " - ", agreementFinal ? "final - " : string.Empty, Guid.NewGuid() + ".pdf"),
                    DocumentInformation.GetDocumentInformation(reader), DocTemplateFunctions.PostDocumentFunctions, new Dictionary<string, DataTable> { { "transactions", transactions }, { "schedule", payProfile } });

                    Console.WriteLine(string.Concat("Sending letter to: ", reader["CustomerLetterName"].ToString()));
                    if (ClickSend.SendClickSendLetter(statementPath, Sendclick.getSendClickValues(reader), //Program.ftpServer,
                        //Program.ftpPath,
                        //Program.ftpUserName,
                        //Program.ftpPassword,
                        Program.clicksendUserName,
                        Program.clicksendPassword//,
                        //Program.clicksendCollectFileURL
                        ))
                    {
                        if (agreementFinal)
                            successfulFinalAgreementIds.Add(reader["AgreementNumber"].ToString());
                        else
                            successfulAgreementIds.Add(reader["AgreementNumber"].ToString());
                    }
                    else
                    {
                        failedAgreementIds.Add(reader["AgreementNumber"].ToString());
                    }
                }
            }
            else
            {
                Console.WriteLine("No rows found.");
            }
            reader.Close();
        }

        internal static void EmailLogOfAnnualStatements(List<string> successfulAgreementIds, List<string> successfulFinalAgreementIds, List<string> failedAgreementIds)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine((successfulAgreementIds.Count + successfulFinalAgreementIds.Count) + " were a success");

            if (failedAgreementIds.Count > 0)
            {
                sb.AppendLine(failedAgreementIds.Count + " failed:");
                foreach (var failedAgreement in failedAgreementIds)
                {

                    sb.AppendLine(failedAgreement);
                }
            }

            SMTPHelper.Helpers.SendEmail(SMTPHelper.Helpers.CreateSMTPClient(),
                new email(
                              ConfigurationManager.AppSettings["SMTPLogTo"],
                              ConfigurationManager.AppSettings["SMTPLogToName"],
                              "Annual Statements Log",
                              sb.ToString(),
                              ConfigurationManager.AppSettings["SMTPSender"],
                              ConfigurationManager.AppSettings["SMTPSenderName"],
                              new List<string>(),
                              new List<string>()
            ), null);
        }

        //public static void updateSalesForceAgreementStatementData(
        //    List<string> successfullAgreementIds,
        //    List<string> successfullFinalAgreementIds)
        //{
        //    Console.WriteLine(string.Concat("Started Update of Agreements"));

        //    introducersAndDealers = SalesforceClient.getRecordsFromSalesforce<AccountData>(
        //      @"SELECT id, sentinalCompanyId__c, introducerType__c, dealerType__c FROM Account WHERE sentinalCompanyId__c LIKE 'I%' OR sentinalCompanyId__c LIKE 'S%'");

        //    ILookup<string, string> salesforceUserIds = GetSalesforceUserIds(SalesforceDataHelper.SalesforceClient);

        //    var dateTimeSyncStarted = DateTime.Now;
        //    //SalesforceDataHelper.SalesforceClient.
        //    SalesforceDataHelper.SalesforceClient.BulkUpsertFromSQL("Agreement__c", "sentinalProposal_Id__c",
        //        false, GetAgreementDataFromSQL, agreementData =>
        //            new CustomSObject()
        //            {
        //                { "sentinalProposal_Id__c", agreementData["AgreementProposalID"].ToString().Trim() },
        //                { "FinalStatementSent__c", DataHelper.GetDateTime(agreementData["AgreementAgreementDate"]) },
        //                { "LastAnnualStatementSent__c", DataHelper.GetDateTime(agreementData["AgreementSettledDate"]) }
        //            }, null);

        //    Console.WriteLine(string.Concat("Ended Update of Agreements"));
        //}
    }
}
