using Docx2ClickSend;
using Salesforce.Common.Models;
using Salesforce.Force;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading.Tasks;

namespace ClickSend
{
    class Antifraud
    {
        public static int maxNumberOfLettersAllowedToSendByClicksend = int.Parse(ConfigurationManager.AppSettings["MaxNumberOfLettersAllowedToSendByClicksend"]);

        private static QueryResult<ProposalCustomerData> getCustomerRequiringAntiFraudLetters(ForceClient salesforceClient)
        {
            var queryString = string.Concat(@"SELECT Id, proposal__r.sentinalID__c,
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
                                                proposal__r.lastDateTimeAccepted__c >= 2018-09-07T00:00:00Z AND
                                                (proposal__r.sentinalWorkflowStageId__c <> 5) AND
                                                proposal__r.IsLatest__c = True AND
                                                proposal__r.isDummyProposal__c = False AND
                                                customer__r.Name != null");

            QueryResult<ProposalCustomerData> anchorWebServices = null;

            Task.Run(async () =>
            {
                anchorWebServices = await salesforceClient.QueryAsync<ProposalCustomerData>(queryString);
            }).Wait(5600000);

            return anchorWebServices;
        }

        public static void sendAntiFraudLetters(ForceClient salesforceClient, System.IO.StreamWriter file)
        {
            Helpers.CleanOldFiles(ConfigurationManager.AppSettings["DocxOutputFolder"], ".docx");
            Helpers.CleanOldFiles(ConfigurationManager.AppSettings["PdfOutputFolder"], ".pdf");

            var customersRequiringAntiFraudLetters = getCustomerRequiringAntiFraudLetters(salesforceClient);

            if (customersRequiringAntiFraudLetters.Records.Count > maxNumberOfLettersAllowedToSendByClicksend)
            {
                //todo: email sales support to say it looks wrong that there are so many new antifraud letters
                return;    
            }

            if (customersRequiringAntiFraudLetters.Records.Count > 0)
            {

                List<decimal> customerIds = new List<decimal>();
                //check that we have not already proccessed this customer customer__r.panther_id__c
                foreach (var proposalCustomer in customersRequiringAntiFraudLetters.Records)
                {
                    if (proposalCustomer.proposal__r.sentinalID__c == null || proposalCustomer.customer__r.panther_id__c == null)
                    { continue; }

                    if (!decimal.TryParse(proposalCustomer.proposal__r.sentinalID__c, out decimal sentinalId)
                        ||
                        !decimal.TryParse(proposalCustomer.customer__r.panther_id__c, out decimal pantherId))
                    { continue; }

                    if (customerIds.Contains(pantherId)) { continue; }

                    Console.WriteLine(string.Concat("Sending letter to:", proposalCustomer.customer__r.Name));
                    //file.WriteLine(string.Concat("Sending letter to:", proposalCustomer.customer__r.Name));

                    var securityCode = string.Concat(sentinalId.ToString("0"), "/", pantherId.ToString("0"));
                    
                    bool success = sendLetter(getSendClickValues(proposalCustomer), getWordLetterValues(proposalCustomer, securityCode));
                    //todo: if sendLetter is not successful then dont save the new confirmation number

                    if (success == false) { securityCode = "ErrorWithClickSend"; continue; }

                    customerIds.Add(pantherId);

                    Task.Run(async () =>
                    {
                        SuccessResponse successResponse = await salesforceClient.UpsertExternalAsync(
                            "Contact",
                            "panther_id__c",
                            proposalCustomer.customer__r.panther_id__c,
                            new {
                                fraudIdentityConfirmationNo__c = securityCode,
                                fraudIdentityConfirmationLetterSent__c = DateTime.Now
                            }
                            );
                        Console.WriteLine(successResponse != null && successResponse.Success ? "Success" : "Failed");
                    }).Wait(5600000);
                }

                }
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

        static WordValues getWordLetterValues(ProposalCustomerData proposalCustomer, string securityCode)
        {
            var address = JoinWith(proposalCustomer.customer__r.Title.Trim(), proposalCustomer.customer__r.Name.Trim(), " ");
            address = JoinWith(address, proposalCustomer.customer__r.MailingStreet, Environment.NewLine);
            address = JoinWith(address, proposalCustomer.customer__r.MailingCity, Environment.NewLine);
            address = JoinWith(address, proposalCustomer.customer__r.MailingPostalCode, Environment.NewLine);
            address = JoinWith(address, proposalCustomer.customer__r.MailingState, Environment.NewLine);
            address = JoinWith(address, proposalCustomer.customer__r.MailingCountry, Environment.NewLine);

            return new WordValues
            {
                Date = DateTime.Now.ToString("dd MMMM \\'yy"),

                VehicleDetail = JoinWith(
                proposalCustomer.proposal__r.PrimaryVehicle__r.make__c.Trim(),
                proposalCustomer.proposal__r.PrimaryVehicle__r.model__c == null ? string.Empty : proposalCustomer.proposal__r.PrimaryVehicle__r.model__c.Trim(),
                " "),

                FullName = JoinWith(
                proposalCustomer.customer__r.Title.Trim(),
                proposalCustomer.customer__r.Name.Trim(),
                " "),

                Address = address,
                
                SecurityCode = securityCode
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

        static SendClickValues getSendClickValues(ProposalCustomerData proposalCustomer)
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

            return new SendClickValues
            {
                address_name = getLeft(JoinWith(proposalCustomer.customer__r.Title.Trim(), proposalCustomer.customer__r.Name.Trim(), " "),50),
                address_line_1 = getLeft(addressLine1, 50),
                address_line_2 = getLeft(addressLine2, 50),
                address_city = getLeft(useEither(proposalCustomer.customer__r.MailingCity, DataHelper.TakeLastLines(proposalCustomer.customer__r.MailingStreet.Trim(),1)[0]),30),
                address_state = getLeft(proposalCustomer.customer__r.MailingState,30),
                address_postal_code = getLeft(proposalCustomer.customer__r.MailingPostalCode,10),
                address_country = "GB"//proposalCustomer.customer__r.MailingCountry
            };
        }

        static bool sendLetter(SendClickValues sendClickValues, WordValues wordLetterValues) {

            var libraryImplementationTest = new Kernel(ConfigurationManager.AppSettings["TemplateFile"],
                                                       ConfigurationManager.AppSettings["DocxOutputFolder"],
                                                       ConfigurationManager.AppSettings["PdfOutputFolder"]);

            var credentials = new ClickSendCredentials("", "");
            var kernel = libraryImplementationTest.Replace(wordLetterValues).Convert();
            var asyncTaskResult = kernel.Send(sendClickValues, credentials);
            asyncTaskResult.Wait();

            return asyncTaskResult.Result.response_code == "SUCCESS";
        }
    
        }
}
