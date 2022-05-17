using ClicksendHelper;
using DocxFromTemplateGenerator;
using SalesforceDataLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text.RegularExpressions;
using AutoDocHelper;
using ClickSendHelper;

namespace AntiFraudLetterGenerator
{
    class Antifraud
    {
        public static int maxNumberOfLettersAllowedToSendByClicksend = int.Parse(ConfigurationManager.AppSettings["MaxNumberOfLettersAllowedToSendByClicksend"]);

        internal static List<string> TakeLastLines(string text, int count)
        {
            List<string> lines = new List<string>();
            Match match = Regex.Match(text, "^.*$", RegexOptions.Multiline | RegexOptions.RightToLeft);

            while (match.Success && lines.Count < count)
            {
                lines.Insert(0, match.Value);
                match = match.NextMatch();
            }

            return lines;
        }

        private static List<ProposalCustomerData> getCustomerRequiringAntiFraudLetters()
        {
            return SalesforceDataHelper.SalesforceClient.GetRecordsFromSalesforce<ProposalCustomerData>(
                 "ProposalCustomerData", @"SELECT Id, proposal__r.sentinalID__c, proposal__r.CreatedDate,
                                                customer__r.panther_id__c, 
                                                customer__r.Title, customer__r.Name, customer__r.Email, customer__r.Phone, 
                                                customer__r.HomePhone, customer__r.OtherPhone, 
                                                customer__r.MobilePhone, customer__r.mobileNumber__c, 
                                                customer__r.MailingStreet, customer__r.MailingCity, customer__r.MailingPostalCode, 
                                                customer__r.MailingState, customer__r.MailingCountry,
                                                customer__r.fraudIdentityConfirmationNo__c,
                                                customer__r.fraudIdentityConfirmationLetterSent__c,
                                                proposal__r.PrimaryVehicle__r.make__c,
                                                proposal__r.PrimaryVehicle__r.model__c,
                                                proposal__r.lastDateTimeAccepted__c
                                                FROM ProposalCustomer__c
                                                WHERE customer__r.fraudIdentityConfirmationNo__c = null AND 
                                                proposal__r.status__c = 'Archived' AND
                                                (proposal__r.sentinalWorkflowStageId__c <> 5
                                                AND proposal__r.sentinalWorkflowStageId__c <> 101
                                                AND proposal__r.sentinalWorkflowStageId__c <> 102
                                                AND proposal__r.sentinalWorkflowStageId__c <> 103
                                                AND proposal__r.sentinalWorkflowStageId__c <> 104
                                                AND proposal__r.sentinalWorkflowStageId__c <> 105
                                                AND proposal__r.sentinalWorkflowStageId__c <> 111
                                                AND proposal__r.sentinalWorkflowStageId__c <> 133
                                                AND proposal__r.sentinalWorkflowStageId__c <> 134
                                                AND proposal__r.sentinalWorkflowStageId__c <> 142
                                                )
                                                 AND           
                                                proposal__r.lastDateTimeAccepted__c >= 2021-10-13T00:00:00Z AND
                                                
                                                proposal__r.IsLatest__c = True AND
                                                proposal__r.isDummyProposal__c = False AND
                                                customer__r.Name != null
                                            ORDER BY proposal__r.CreatedDate desc");
        }

        public static void sendAntiFraudLetters()
        {
            Helpers.CleanOldFiles(ConfigurationManager.AppSettings["PdfOutputFolder"], ".txt");
            Helpers.CleanOldFiles(ConfigurationManager.AppSettings["PdfOutputFolder"], ".pdf");

            var customersRequiringAntiFraudLetters = getCustomerRequiringAntiFraudLetters();

            if (customersRequiringAntiFraudLetters.Count > maxNumberOfLettersAllowedToSendByClicksend)
            {
                //todo: email sales support to say it looks wrong that there are so many new antifraud letters
                return;    
            }

            if (customersRequiringAntiFraudLetters.Count == 0) return;
            

                List<decimal> customerIds = new List<decimal>();
                //check that we have not already proccessed this customer customer__r.panther_id__c
                foreach (var proposalCustomer in customersRequiringAntiFraudLetters)
                {
                    if (proposalCustomer.proposal__r.sentinalID__c == null || proposalCustomer.customer__r.panther_id__c == null)
                    { continue; }

                    if (!decimal.TryParse(proposalCustomer.proposal__r.sentinalID__c, out decimal sentinalId)
                        ||
                        !decimal.TryParse(proposalCustomer.customer__r.panther_id__c, out decimal pantherId))
                    { continue; }

                    if (customerIds.Contains(pantherId)) { continue; }

                    Console.WriteLine(string.Concat("Sending letter to:", proposalCustomer.customer__r.Name));

                    var securityCode = string.Concat(sentinalId.ToString("0"), "/", pantherId.ToString("0"));

                    bool success = sendLetter(proposalCustomer, securityCode);

                    if (success == false) { securityCode = "ErrorWithClickSend"; continue; }

                    customerIds.Add(pantherId);

                    saveInAntifraudDetailsInSalesforce(proposalCustomer, securityCode);
            }
        }

        private static void saveInAntifraudDetailsInSalesforce(ProposalCustomerData proposalCustomer, string securityCode)
        {
            string error;

            SalesforceDataHelper.SalesforceClient.UpsertAsync(
            "Contact",
            "panther_id__c",
            new CustomSObject()
            {
                        { "panther_id__c", proposalCustomer.customer__r.panther_id__c},
                        { "fraudIdentityConfirmationNo__c",  securityCode},
                        { "fraudIdentityConfirmationLetterSent__c",  DateTime.Now }
            }, out error
            );

            Console.WriteLine(error != null ? "Success" : error);
        }

        static string JoinWith(string text1, string text2, string joiner) {

            if (text1 == null) { text1 = string.Empty; }
            if (text2 == null) { text2 = string.Empty; }

            if (text1.Length > 0 && text2.Length > 0)
            {
                return string.Concat(text1, joiner, text2);
            }

            return text1 + text2;
        }


        public static string getAddress(ProposalCustomerData proposalCustomer)
        {

            var address = JoinWith(proposalCustomer.customer__r.Title.Trim(), proposalCustomer.customer__r.Name.Trim(), " ");
            var lineBreak = "\v";

            address = StringFunctions.JoinWith(address, proposalCustomer.customer__r.MailingStreet.Replace("\n", "\v"), lineBreak);
            address = StringFunctions.JoinWith(address, proposalCustomer.customer__r.MailingCity, lineBreak);
            address = StringFunctions.JoinWith(address, proposalCustomer.customer__r.MailingPostalCode, lineBreak);
            address = StringFunctions.JoinWith(address, proposalCustomer.customer__r.MailingState, lineBreak);
            address = StringFunctions.JoinWith(address, proposalCustomer.customer__r.MailingCountry, lineBreak);
  
            return address;
        }

        static Dictionary<string,string> getWordLetterValues(ProposalCustomerData proposalCustomer, string securityCode)
        {

            return new Dictionary<string, string>()
            {
                { "<Date>", DateTime.Now.ToString("dd MMMM \\'yy") },

                { "<VehicleDetail>", JoinWith(
                proposalCustomer.proposal__r.PrimaryVehicle__r.make__c.Trim(),
                proposalCustomer.proposal__r.PrimaryVehicle__r.model__c == null ? string.Empty : proposalCustomer.proposal__r.PrimaryVehicle__r.model__c.Trim(),
                " ")},

                { "<FullName>" , JoinWith(
                proposalCustomer.customer__r.Title.Trim(),
                proposalCustomer.customer__r.Name.Trim(),
                " ") },

                { "<Address>" , getAddress(proposalCustomer) },

                { "<SecurityCode>", securityCode }
            };
        
        }

        static string useEither(string value1, string value2)
        {
            if (!string.IsNullOrEmpty(value1)) { return value1; }
            return value2;
        }

        static string getLeft(string value, int length)
        {
            if (value == null) {
                return string.Empty;
            }
            return value.Substring(0, Math.Min(length, value.Length));
        }

        static ClickSendValues getSendClickValues(ProposalCustomerData proposalCustomer)
        {
            string[] addressLines = proposalCustomer.customer__r.MailingStreet.Split(
                new[] { Environment.NewLine },
                StringSplitOptions.None
            );

            var addressLine1 = addressLines[0];
            var addressLine2 = string.Empty;
            if (addressLines.Length > 1) {
                addressLine2 = addressLines[1];
            }

            return new ClickSendValues
            {
                address_name = getLeft(JoinWith(proposalCustomer.customer__r.Title.Trim(), proposalCustomer.customer__r.Name.Trim(), " "),50),
                address_line_1 = getLeft(addressLine1, 50),
                address_line_2 = getLeft(addressLine2, 50),
                address_city = getLeft(useEither(proposalCustomer.customer__r.MailingCity, TakeLastLines(proposalCustomer.customer__r.MailingStreet.Trim(),1)[0]),30),
                address_state = getLeft(proposalCustomer.customer__r.MailingState,30),
                address_postal_code = getLeft(proposalCustomer.customer__r.MailingPostalCode,10),
                address_country = "GB"//proposalCustomer.customer__r.MailingCountry
            };
        }

        static bool sendLetter(ProposalCustomerData proposalCustomer, string securityCode) {

            Console.WriteLine(string.Concat("Generating antifraud letter for: ", proposalCustomer.customer__r.Name));

            var statementPath = GenerateDocument.Generate(ConfigurationManager.AppSettings["TemplateFile"],
                                                          String.Concat(ConfigurationManager.AppSettings["PdfOutputFolder"],
                                                          proposalCustomer.customer__r.panther_id__c, " - Antifraud Letter - ", Guid.NewGuid(), ".pdf"), 
                                                          getWordLetterValues(proposalCustomer, securityCode),
                                                          null,null);

            Console.WriteLine(string.Concat("Sending antifraud letter to: ", proposalCustomer.customer__r.Name));
            Console.WriteLine();

            return ClickSend.SendClickSendLetter(statementPath, getSendClickValues(proposalCustomer), 
                ConfigurationManager.AppSettings["clicksendUserName"],
                ConfigurationManager.AppSettings["clicksendPassword"]);
        }
    
        }
}
