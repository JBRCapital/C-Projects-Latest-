using System.Data.SqlClient;
using Salesforce.Force;
using System.Threading.Tasks;
using System.Collections.Generic;
using Salesforce.Common.Models;
using System;
using Salesforce.Common.Models.Json;
using System.Threading;

namespace UpdateSalesforceData
{
    public class PayProfileUpdater
    {

        public static void UpdateAgreementPayProfile()
        {
            Console.WriteLine(string.Concat("Started sync of Agreement Pay profile"));

            var payProfileData = GetPayProfilesFromSQL();

            while (payProfileData.Read())
            {
                UpdatePayProfileForAgreement(Program.salesforceClient, payProfileData["PayProAgreementNumber"].ToString(), Program.sqlConn1);
            };

            payProfileData.Close();

            Console.WriteLine(string.Concat("Ended Sync of Agreement Pay profile"));
        }

        private static SqlDataReader GetPayProfilesFromSQL()
        {
            return new SqlCommand("SELECT * FROM GetPayProfilesForSalesforce()", Program.sqlConn){ CommandTimeout = 0 }.ExecuteReader();
        }

        private static List<PayProfileData> GetAgreementPayProfilesFromSQL(string agreementNumber, SqlConnection sqlConn)
        {
            List<PayProfileData> sqlPayProfileRecords = new List<PayProfileData>();

            SqlDataReader agreementPayProfileData = new SqlCommand("SELECT * from GetAgreementPayProfileForSalesforce('" + agreementNumber + "',0)", sqlConn){ CommandTimeout = 0 }.ExecuteReader();

            //while (agreementPayProfileData.Read())
            //{
            //    sqlPayProfileRecords.Add(new PayProfileData
            //    {
            //        Name = DataHelper.GetDateTime(agreementPayProfileData["PayDate"]).Value.Date.ToShortDateString(),
            //        PayDate__c = DataHelper.GetDateTime(agreementPayProfileData["PayDate"]),
            //        Instalment__c = DataHelper.GetDouble(agreementPayProfileData["InstalmentValue"]),
            //        Principle__c = DataHelper.GetDouble(agreementPayProfileData["Principle"]),
            //        Interest__c = DataHelper.GetDouble(agreementPayProfileData["Interest"]),
            //        Fee__c = DataHelper.GetDouble(agreementPayProfileData["Fee"]),
            //        VATonFee__c = DataHelper.GetDouble(agreementPayProfileData["VATonFee"]),
            //        PayFallenDue__c = DataHelper.GetBoolean(agreementPayProfileData["PayFallenDue"]),
            //    });
            //};

            agreementPayProfileData.Close();

            return sqlPayProfileRecords;
        }

        private static QueryResult<AgreementPayProfileData> GetSalesForceAgreementPayProfileData(SalesforceHttpClient salesforceClient, string agreementNumber)
        {
            var queryString = string.Concat(@"SELECT approval_agreementNumber__c, Id,
                                                (
                                                    SELECT Id, Name, Agreement__c, Fee__c, Instalment__c, Interest__c, 
                                                    PayDate__c, PayFallenDue__c, Principle__c, VATonFee__c
                                                    FROM PayProfiles__r
                                                    WHERE IsDeleted = FALSE
                                                    ORDER BY PayDate__c
                                                ) FROM Agreement__c
                                              WHERE approval_agreementNumber__c = '", agreementNumber, "'");

            QueryResult<AgreementPayProfileData> agreementPayProfileData = null;
            Task.Run(async () =>
            {
                agreementPayProfileData = await salesforceClient.QueryAllAsync<AgreementPayProfileData>(queryString);
            }).Wait(Timeout.InfiniteTimeSpan);
            return agreementPayProfileData;
        }

        private static void UpdatePayProfileForAgreement(SalesforceHttpClient salesforceClient, string agreementNumber, SqlConnection sqlConn)
        {
            Console.WriteLine(string.Concat("Checking Agreement Pay profile: ", agreementNumber));

            QueryResult<AgreementPayProfileData> agreementPayProfileSFData = GetSalesForceAgreementPayProfileData(salesforceClient, agreementNumber);

            if (agreementPayProfileSFData == null || agreementPayProfileSFData.Records.Count == 0)
            {
                //Theres no agreement in SalesForce so no need to add a payprofile to something which does not exist
                return;
            }

            List<PayProfileData> agreementPayProfileSQLData = GetAgreementPayProfilesFromSQL(agreementNumber, sqlConn);

            List<PayProfileData> salesforcePayProfiles = new List<PayProfileData>();

            if (agreementPayProfileSFData.Records[0].PayProfiles__r != null)
            {
                foreach (var salesForcePayProfile in agreementPayProfileSFData.Records[0].PayProfiles__r.Records)
                {
                    //Check if the Salesforce pay profile exists in SQL, if it does not exist in SQL then delete it from Sales
                    if (!agreementPayProfileSQLData.Exists(sqlPayProfile => sqlPayProfile.PayDate__c == salesForcePayProfile.PayDate__c))
                    {
                        Console.WriteLine(string.Concat("Deleting: ", salesForcePayProfile.Id));

                        //Delete Payprofile from Salesforce
                        Task.Run(async () =>
                        {
                            bool success = await salesforceClient.DeleteAsync<bool>("AgreementPayProfile__c", salesForcePayProfile.Id);
                        }).Wait(Timeout.InfiniteTimeSpan);
                    }
                    else
                    {
                        salesforcePayProfiles.Add(salesForcePayProfile);
                    }
                }
            }
            //Either Add or Edit the following SQL entries

            //Compare if the payprofile has changed....
            foreach (var sqlPayProfile in agreementPayProfileSQLData)
            {
                var matchingSalesforcePayProfile = salesforcePayProfiles.Find(salesforcePayprofile => salesforcePayprofile.PayDate__c == sqlPayProfile.PayDate__c);
                if (matchingSalesforcePayProfile != null)
                {
                    //Check if the PayProfile has changed
                    if (
                        matchingSalesforcePayProfile.Instalment__c != sqlPayProfile.Instalment__c ||
                        matchingSalesforcePayProfile.Principle__c != sqlPayProfile.Principle__c ||
                        matchingSalesforcePayProfile.Interest__c != sqlPayProfile.Interest__c ||
                        matchingSalesforcePayProfile.Fee__c != sqlPayProfile.Fee__c ||
                        matchingSalesforcePayProfile.VATonFee__c != sqlPayProfile.VATonFee__c ||
                        matchingSalesforcePayProfile.PayFallenDue__c != sqlPayProfile.PayFallenDue__c
                        )
                    {
                        Console.WriteLine(string.Concat("Updating: ", matchingSalesforcePayProfile.Id));

                        //the salesforce object has changed so we need to update the payprofile in salesforce
                        Task.Run(async () =>
                        {
                            sqlPayProfile.Agreement__c = agreementPayProfileSFData.Records[0].Id;
                            sqlPayProfile.Sentinel_Agreement_Number__c = agreementNumber;
                            var successResponse = await salesforceClient.UpdateAsync("AgreementPayProfile__c", matchingSalesforcePayProfile.Id, sqlPayProfile);
                        }).Wait(Timeout.InfiniteTimeSpan);

                    }
                }
                else {
                    Console.WriteLine(string.Concat("Adding: ", agreementNumber, " - ", sqlPayProfile.Name));

                    //..otherwise we create the payProfile in Salesforce
                    Task.Run(async () =>
                    {
                        sqlPayProfile.Agreement__c = agreementPayProfileSFData.Records[0].Id;
                        sqlPayProfile.Sentinel_Agreement_Number__c = agreementNumber;
                        var successResponse = await salesforceClient.CreateAsync("AgreementPayProfile__c", sqlPayProfile);
                    }).Wait(Timeout.InfiniteTimeSpan);
                }
            }
        }

    }
}
