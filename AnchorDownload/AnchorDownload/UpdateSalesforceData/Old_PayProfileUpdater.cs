/*using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Salesforce.Common.Models.Json;
using System.Threading;
using System.Linq;

namespace UpdateSalesforceData
{
    public class PayProfileUpdater
    {

        public static void UpdateAgreementPayProfile(SqlConnection sqlConn, SqlConnection sqlConn1, SalesforceHttpClient salesforceClient)
        {
            Console.WriteLine(string.Concat("Started sync of Agreement Pay profile"));

            var payProfileData = GetAllAgreementPayProfilesFromSQL(sqlConn);

            int updateBatchNumber = Helper.GetEpoc();

            List<AgreementData> agreementRecordsFromSalesforce = getAgreementRecordsFromSalesforce(salesforceClient);

            while (payProfileData.Read())
            {
                UpdatePayProfileForAgreement(salesforceClient, updateBatchNumber, agreementRecordsFromSalesforce, payProfileData);//,  sqlConn1);//payProfileData["PayProAgreementNumber"].ToString(),
            };

            payProfileData.Close();

            Console.WriteLine(string.Concat("Ended Sync of Agreement Pay profile"));
        }

        private static SqlDataReader GetAllAgreementPayProfilesFromSQL(SqlConnection sqlConn)
        {
            return new SqlCommand("SELECT * FROM GetAllAgreementPayProfileForSalesforce()", sqlConn) { CommandTimeout = 0 }.ExecuteReader();
        }

        //Update PayProle

        private static List<AgreementData> getAgreementRecordsFromSalesforce(SalesforceHttpClient salesforceClient)
        {
            List<AgreementData> result = new List<AgreementData>();
            var queryString = string.Concat(@"SELECT id, approval_agreementNumber__c FROM Agreement__c");

            QueryResult<AgreementData> agreementData = null;

            Task.Run(async () =>
            {
                agreementData = await salesforceClient.QueryAsync<AgreementData>(queryString);
                var finished = false;
                do
                {
                    finished = agreementData.Done;

                    // add any records
                    if (agreementData.Records.Any())
                        result.AddRange(agreementData.Records);

                    // if more
                    if (!finished)
                    {
                        // get next batch
                        agreementData = await salesforceClient.QueryContinuationAsync<AgreementData>(agreementData.NextRecordsUrl);
                    }
                } while (!finished);
            }).Wait(Timeout.InfiniteTimeSpan);

            return result;
        }

        private static void UpdatePayProfileForAgreement(SalesforceHttpClient salesforceClient, int updateBatchNumber, List<AgreementData> agreementRecordsFromSalesforce, SqlDataReader payProfileData) //SqlConnection sqlConn,string agreementNumber, 
        {
            //Console.WriteLine(string.Concat("Checking Agreement Pay profile: ", agreementNumber));

            var agreement = agreementRecordsFromSalesforce.Find(salesforceAgreement => salesforceAgreement.approval_agreementNumber__c == payProfileData["PayProAgreementNumber"].ToString());
            if (agreement == null) {
                Console.WriteLine("Agreement Not Found");
                return;
            }

            var PayProfileData = new PayProfileData
            {
                //PayProID__c = DataHelper.GetDouble(payProfileData["PayProId"]),
                Name = DataHelper.GetDateTime(payProfileData["PayDate"]).Value.Date.ToShortDateString(),
                Agreement__c = agreement.id,
                Sentinel_Agreement_Number__c = payProfileData["PayProAgreementNumber"].ToString(),
                PayDate__c = DataHelper.GetDateTime(payProfileData["PayDate"]),
                Instalment__c = DataHelper.GetDouble(payProfileData["InstalmentValue"]),
                Principle__c = DataHelper.GetDouble(payProfileData["Principle"]),
                Interest__c = DataHelper.GetDouble(payProfileData["Interest"]),
                Fee__c = DataHelper.GetDouble(payProfileData["Fee"]),
                VATonFee__c = DataHelper.GetDouble(payProfileData["VATonFee"]),
                PayFallenDue__c = DataHelper.GetBoolean(payProfileData["PayFallenDue"]),

                UpdateBatchNumber__c = updateBatchNumber

            };

            SuccessResponse successResponse = null;

            Task.Run(async () =>
            {
                Console.WriteLine(string.Concat("Upserting PayProfile:", payProfileData["PayProId"]));
                successResponse = await salesforceClient.UpsertExternalAsync("AgreementPayProfile__c", "PayProID__c", payProfileData["PayProId"].ToString(), PayProfileData);
                Console.WriteLine(successResponse != null && successResponse.Success ? "Success" : "Failed");
            }).Wait(Timeout.InfiniteTimeSpan);
            
        }

        //Delete Irrelevant Pay Profiles

        private static List<PayProfileData> getIrrelevantPayProfilesecordsFromSalesforce(SalesforceHttpClient salesforceClient, int updateBatchNumber)
        {
            List<PayProfileData> result = new List<PayProfileData>();
            var queryString = string.Concat("SELECT id FROM AgreementPayProfile__c Where UpdateBatchNumber__c <> " + updateBatchNumber);

            QueryResult<PayProfileData> payProfiles = null;

            Task.Run(async () =>
            {
                payProfiles = await salesforceClient.QueryAsync<PayProfileData>(queryString);
                var finished = false;
                do
                {
                    finished = payProfiles.Done;

                    // add any records
                    if (payProfiles.Records.Any())
                        result.AddRange(payProfiles.Records);

                    // if more
                    if (!finished)
                    {
                        // get next batch
                        payProfiles = await salesforceClient.QueryContinuationAsync<PayProfileData>(payProfiles.NextRecordsUrl);
                    }
                } while (!finished);
            }).Wait(Timeout.InfiniteTimeSpan);

            return result;
        }

        private static void DeleteIrrelevantPayProfileForAgreement(SalesforceHttpClient salesforceClient, int updateBatchNumber) //SqlConnection sqlConn,string agreementNumber, 
        {
            // create an Id list for the original strongly typed accounts created
            //var idBatch = new SObjectList<SObject>();
            //idBatch.AddRange(getIrrelevantPayProfilesecordsFromSalesforce(salesforceClient, updateBatchNumber).Select(result => new SObject { { "Id", result.Id } }));
            //Task.Run(async () =>
            //{

            //    // delete all the strongly typed accounts
            //    var results = await salesforceClient.RunJobAndPollAsync("AgreementPayProfile__c",
            //                                                        BulkConstants.OperationType.Delete,
            //                                                        getIrrelevantPayProfilesecordsFromSalesforce(salesforceClient, updateBatchNumber));
            //    //new List<SObjectList<SObject>> { idBatch 
            //}).Wait(Timeout.InfiniteTimeSpan);
        }

        //QueryResult<AgreementPayProfileData> agreementPayProfileSFData = GetSalesForceAgreementPayProfileData(salesforceClient, updateBatchNumber, agreementNumber);

            //if (agreementPayProfileSFData == null || agreementPayProfileSFData.Records.Count == 0)
            //{
            //    //Theres no agreement in SalesForce so no need to add a payprofile to something which does not exist
            //    return;
            //}

            //List<PayProfileData> agreementPayProfileSQLData = GetAgreementPayProfilesFromSQL(agreementNumber, sqlConn);

            //List<PayProfileData> salesforcePayProfiles = new List<PayProfileData>();

            //if (agreementPayProfileSFData.Records[0].PayProfiles__r != null)
            //{
            //    foreach (var salesForcePayProfile in agreementPayProfileSFData.Records[0].PayProfiles__r.Records)
            //    {
            //        //Check if the Salesforce pay profile exists in SQL, if it does not exist in SQL then delete it from Sales
            //        if (!agreementPayProfileSQLData.Exists(sqlPayProfile => sqlPayProfile.PayDate__c == salesForcePayProfile.PayDate__c))
            //        {
            //            Console.WriteLine(string.Concat("Deleting: ", salesForcePayProfile.Id));

            //            //Delete Payprofile from Salesforce
            //            Task.Run(async () =>
            //            {
            //                bool success = await salesforceClient.DeleteAsync<bool>("AgreementPayProfile__c", salesForcePayProfile.Id);
            //            }).Wait(Timeout.InfiniteTimeSpan);
            //        }
            //        else
            //        {
            //            salesforcePayProfiles.Add(salesForcePayProfile);
            //        }
            //    }
            //}
            //Either Add or Edit the following SQL entries

            //Compare if the payprofile has changed....
            //foreach (var sqlPayProfile in agreementPayProfileSQLData)
            //{
            //    var matchingSalesforcePayProfile = salesforcePayProfiles.Find(salesforcePayprofile => salesforcePayprofile.PayDate__c == sqlPayProfile.PayDate__c);
            //    if (matchingSalesforcePayProfile != null)
            //    {
            //        //Check if the PayProfile has changed
            //        if (
            //            matchingSalesforcePayProfile.Instalment__c != sqlPayProfile.Instalment__c ||
            //            matchingSalesforcePayProfile.Principle__c != sqlPayProfile.Principle__c ||
            //            matchingSalesforcePayProfile.Interest__c != sqlPayProfile.Interest__c ||
            //            matchingSalesforcePayProfile.Fee__c != sqlPayProfile.Fee__c ||
            //            matchingSalesforcePayProfile.VATonFee__c != sqlPayProfile.VATonFee__c ||
            //            matchingSalesforcePayProfile.PayFallenDue__c != sqlPayProfile.PayFallenDue__c
            //            )
            //        {
            //            Console.WriteLine(string.Concat("Updating: ", matchingSalesforcePayProfile.Id));

            //            //the salesforce object has changed so we need to update the payprofile in salesforce
            //            Task.Run(async () =>
            //            {
            //                sqlPayProfile.Agreement__c = agreementPayProfileSFData.Records[0].Id;
            //                sqlPayProfile.Sentinel_Agreement_Number__c = agreementNumber;
            //                var successResponse = await salesforceClient.UpdateAsync("AgreementPayProfile__c", matchingSalesforcePayProfile.Id, sqlPayProfile);
            //            }).Wait(Timeout.InfiniteTimeSpan);

            //        }
            //    }
            //    else {
            //        Console.WriteLine(string.Concat("Adding: ", agreementNumber, " - ", sqlPayProfile.Name));

            //        //..otherwise we create the payProfile in Salesforce
            //        Task.Run(async () =>
            //        {
            //            sqlPayProfile.Agreement__c = agreementPayProfileSFData.Records[0].Id;
            //            sqlPayProfile.Sentinel_Agreement_Number__c = agreementNumber;
            //            var successResponse = await salesforceClient.CreateAsync("AgreementPayProfile__c", sqlPayProfile);
            //        }).Wait(Timeout.InfiniteTimeSpan);
            //    }
            //}
            //}

            //private static List<PayProfileData> GetAgreementPayProfilesFromSQL(string agreementNumber, SqlConnection sqlConn)
            //{
            //    List<PayProfileData> sqlPayProfileRecords = new List<PayProfileData>();

            //    SqlDataReader agreementPayProfileData = new SqlCommand("SELECT * from GetAgreementPayProfileForSalesforce('" + agreementNumber + "',0)", sqlConn){ CommandTimeout = 0 }.ExecuteReader();

            //    while (agreementPayProfileData.Read())
            //    {
            //        sqlPayProfileRecords.Add(new PayProfileData
            //        {
            //            Name = DataHelper.GetDateTime(agreementPayProfileData["PayDate"]).Value.Date.ToShortDateString(),
            //            PayDate__c = DataHelper.GetDateTime(agreementPayProfileData["PayDate"]),
            //            Instalment__c = DataHelper.GetDouble(agreementPayProfileData["InstalmentValue"]),
            //            Principle__c = DataHelper.GetDouble(agreementPayProfileData["Principle"]),
            //            Interest__c = DataHelper.GetDouble(agreementPayProfileData["Interest"]),
            //            Fee__c = DataHelper.GetDouble(agreementPayProfileData["Fee"]),
            //            VATonFee__c = DataHelper.GetDouble(agreementPayProfileData["VATonFee"]),
            //            PayFallenDue__c = DataHelper.GetBoolean(agreementPayProfileData["PayFallenDue"]),
            //        });
            //    };

            //    agreementPayProfileData.Close();

            //    return sqlPayProfileRecords;
            //}

            //private static QueryResult<AgreementPayProfileData> GetSalesForceAgreementPayProfileData(SalesforceHttpClient salesforceClient,  string agreementNumber)
            //{
            //    var queryString = string.Concat(@"SELECT approval_agreementNumber__c, Id,
            //                                        (
            //                                            SELECT Id, Name, Agreement__c, Fee__c, Instalment__c, Interest__c, 
            //                                            PayDate__c, PayFallenDue__c, Principle__c, VATonFee__c
            //                                            FROM PayProfiles__r
            //                                            WHERE IsDeleted = FALSE
            //                                            ORDER BY PayDate__c
            //                                        ) FROM Agreement__c
            //                                      WHERE approval_agreementNumber__c = '", agreementNumber, "'");

            //    QueryResult<AgreementPayProfileData> agreementPayProfileData = null;
            //    Task.Run(async () =>
            //    {
            //        agreementPayProfileData = await salesforceClient.QueryAllAsync<AgreementPayProfileData>(queryString);
            //    }).Wait(Timeout.InfiniteTimeSpan);
            //    return agreementPayProfileData;
            //}

        }
}
*/