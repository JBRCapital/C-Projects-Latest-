using Salesforce.Common.Models;
using Salesforce.Common.Models.Json;
using Salesforce.Common.Models.Xml;
using Salesforce.Force;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UpdateSalesforceData
{
    public class TransactionUpdater
    {
        public static void UpdateAgreementTransactions()
        {
            Console.WriteLine("Started Sync of Transactions");

            /////
            //var jobInfo = await salesforceClient.CreateJobAsync("AgreementTransaction__c", BulkConstants.OperationType.Upsert);
            /////

            ILookup<string, string> salesForceAgreementIds = GetSalesForceAgreementIds(Program.salesforceClient);

            //var maxTransactionId = GetMaxTransactionsForSalesforceFromSQL(sqlConn);

            var agreementNumbersData = GetAgreementNumbersForSalesforceFromSQL(Program.sqlConn);
            while (agreementNumbersData.Read())
            {
                var agreementNumber = agreementNumbersData["AgreementNumber"].ToString();

                Console.WriteLine("Syncing Transactions for Agreement Number: " + agreementNumber);

                var transactionsData = GetTransactionsForSalesforceFromSQL(Program.sqlConn1, agreementNumber);
                var lastTransactionUpdatedId = 0;

                while (transactionsData.Read())
                {
                    int transactionId;
                    TransactionData transactionObject = getTransactionObject(transactionsData, salesForceAgreementIds, out transactionId);

                    if (transactionObject == null)
                    {
                        Console.WriteLine(string.Concat("Continuing as transaction for '", transactionsData["TransAgreementNumber"].ToString(), "' not found"));
                        continue;
                    }

                    Task.Run(async () =>
                    {
                        Console.WriteLine(string.Concat("Upserting transaction: ", transactionId));
                        var successResponse = await Program.salesforceClient.UpsertExternalAsync("AgreementTransaction__c", "TransactionId__c", transactionId.ToString(), transactionObject);
                        Console.WriteLine(string.Concat("Upserted transaction: ", transactionId));

                        if (successResponse.Success == true) { lastTransactionUpdatedId = transactionId; }
                    }).Wait(Timeout.InfiniteTimeSpan);
                };


   


                transactionsData.Close();
            }
            agreementNumbersData.Close();

            //Console.WriteLine(string.Concat("Saving last transaction Id: ", lastTransactionUpdatedId));
            //if (lastTransactionUpdatedId > 0) { UpdateAgreementLastTransactionCounter(lastTransactionUpdatedId, sqlConn); }

            Console.WriteLine(string.Concat("Ended Sync of Transactions"));
        }

        private static SqlDataReader GetAgreementNumbersForSalesforceFromSQL(SqlConnection sqlConn)
        {
            return new SqlCommand("SELECT * FROM GetAgreementNumbersForSalesforceFromSQL() ORDER BY AgreementNumber", sqlConn){ CommandTimeout = 0 }.ExecuteReader();
        }


        private static SqlDataReader GetTransactionsForSalesforceFromSQL(SqlConnection sqlConn, string agreementNumber)
        {
            return new SqlCommand(string.Concat("SELECT * FROM GetTransactionsForSalesforce('", agreementNumber, "') ORDER BY TransCounter"), sqlConn){ CommandTimeout = 0 }.ExecuteReader();
        }



        private static TransactionData getTransactionObject(SqlDataReader transactionsData, ILookup<string, string> salesForceAgreementIds, out int transactionId)
        {
            var agreement__c = salesForceAgreementIds[transactionsData["TransAgreementNumber"].ToString()].FirstOrDefault();
            if (agreement__c == null) { transactionId = 0; return null; }

            transactionId = Convert.ToInt32(transactionsData["TransCounter"]);
            
            return new TransactionData()
            {
                Agreement__c = agreement__c,
                SentinelAgreementNumber__c = transactionsData["TransAgreementNumber"].ToString(),
                //TransactionId__c = transactionId,
                TransactionDate__c = Convert.ToDateTime(transactionsData["TransDate"]),
                Amount__c = Convert.ToDouble(transactionsData["Amount"]),
                InstalmentDue__c = Convert.ToDouble(transactionsData["InstalmentDue"]),
                StandingOrder__c = Convert.ToDouble(transactionsData["StandingOrder"]),
                BankTransfer__c = Convert.ToDouble(transactionsData["BankTransfer"]),
                DirectDebit__c = Convert.ToDouble(transactionsData["DirectDebit"]),
                CardPayment__c = Convert.ToDouble(transactionsData["CardPayment"]),
                Cheque__c = Convert.ToDouble(transactionsData["Cheque"]),
                Adjustment__c = Convert.ToDouble(transactionsData["Adjustment"]),
                SettlementPayment__c = Convert.ToDouble(transactionsData["SettlementPayment"]),
                ArrearsAdjustment__c = Convert.ToDouble(transactionsData["ArrearsAdjustment"]),
                WriteOff__c = Convert.ToDouble(transactionsData["WriteOff"]),
                TransactionReversal__c = Convert.ToDouble(transactionsData["TransactionReversal"]),
                ReturnedCheque__c = Convert.ToDouble(transactionsData["ReturnedCheque"]),
                Refund__c = Convert.ToDouble(transactionsData["Refund"]),
                SettlementPaymentReversal__c = Convert.ToDouble(transactionsData["SettlementPaymentReversal"]),
                ReturnedDD_RTP__c = Convert.ToDouble(transactionsData["ReturnedDD_RTP"]),
                ReturnedDD_IC__c = Convert.ToDouble(transactionsData["ReturnedDD_IC"]),
                ReturnedDD_NI__c = Convert.ToDouble(transactionsData["ReturnedDD_NI"]),
                ReturnedDD_NA__c = Convert.ToDouble(transactionsData["ReturnedDD_NA"]),
                ReturnedDD_ANYD__c = Convert.ToDouble(transactionsData["ReturnedDD_ANYD"]),
                ReturnedDD_PD__c = Convert.ToDouble(transactionsData["ReturnedDD_PD"]),
                ReturnedDD_AC__c = Convert.ToDouble(transactionsData["ReturnedDD_AC"]),
                RebateOfInterest__c = Convert.ToDouble(transactionsData["RebateOfInterest"]),
                PenaltyInterest__c = Convert.ToDouble(transactionsData["PenaltyInterest"]),
                EarlySettlementFee__c = Convert.ToDouble(transactionsData["EarlySettlementFee"]),
                LatePaymentFee__c = Convert.ToDouble(transactionsData["LatePaymentFee"]),
                ExcessMileageCharge__c = Convert.ToDouble(transactionsData["ExcessMileageCharge"]),
                FairWearAndTearCharge__c = Convert.ToDouble(transactionsData["FairWearAndTearCharge"]),
                OtherFeesVATable__c = Convert.ToDouble(transactionsData["OtherFeesVATable"]),
                OtherFeesNoVAT__c = Convert.ToDouble(transactionsData["OtherFeesNoVAT"]),
                RescheduleFee__c = Convert.ToDouble(transactionsData["RescheduleFee"]),
                RebateOfInterestReversal__c = Convert.ToDouble(transactionsData["RebateOfInterestReversal"]),
                Balance__c = Convert.ToDouble(transactionsData["Balance"])
            };
        }


        private static int GetMaxTransactionsForSalesforceFromSQL(SqlConnection sqlConn)
        {
            SqlDataReader reader = new SqlCommand("SELECT * FROM [dbo].[GetMaxTransactionNumberForSalesforce] ()", sqlConn) { CommandTimeout = 0 }.ExecuteReader();
            reader.Read();
            int maxTransaction = Convert.ToInt32(reader["MaxTransCounter"]);
            reader.Close();

            return maxTransaction;
        }

        private static void UpdateAgreementLastTransactionCounter(int lastTransCounter, SqlConnection sqlConn)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = "UpdateLastTransactionCounter";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("TransCounter", lastTransCounter);

                cmd.Connection = sqlConn;
                cmd.ExecuteNonQuery();
            }
        }

        private static void getResults(List<BatchResultList> results)
        {
            Console.WriteLine("All Batches Complete, \"var results\" contains the result objects (Each is a list containing a result for each record):");
            foreach (var result in results.SelectMany(resultList => resultList.Items))
            {
                Console.WriteLine("Id:{0}, Created:{1}, Success:{2}, Errors:{3}", result.Id, result.Created, result.Success, result.Errors != null);
                if (result.Errors != null)
                {
                    Console.WriteLine("\tErrors:");
                    var resultErrors = result.Errors;
                    foreach (var field in resultErrors.Fields)
                    {
                        Console.WriteLine("\tField:{0}", field);
                    }
                    Console.WriteLine("\t{0}", resultErrors.Message);
                    Console.WriteLine("\t{0}", resultErrors.StatusCode);
                }
            }
        }

        private static ILookup<string, string> GetSalesForceAgreementIds(SalesforceHttpClient salesforceClient)
        {
            QueryResult<AgreementNumberAndIdData> agreementNumberAndIdData = new QueryResult<AgreementNumberAndIdData>();
            //try
            //{
            var queryString = string.Concat(@"SELECT approval_agreementNumber__c, Id
                                              FROM Agreement__c
                                              WHERE approval_agreementNumber__c  <> ''");

            List<AgreementNumberAndIdData> agreementNumberAndIdDataList = new List<AgreementNumberAndIdData>();

            Task.Run(async () =>
            {
                agreementNumberAndIdData = await salesforceClient.QueryAsync<AgreementNumberAndIdData>(queryString);
                var finished = false;
                do
                {
                    finished = agreementNumberAndIdData.Done;

                    // add any records
                    if (agreementNumberAndIdData.Records.Any())
                        agreementNumberAndIdDataList.AddRange(agreementNumberAndIdData.Records);

                    // if more
                    if (!finished)
                    {
                        // get next batch
                        agreementNumberAndIdData = await salesforceClient.QueryContinuationAsync<AgreementNumberAndIdData>(agreementNumberAndIdData.NextRecordsUrl);
                    }
                } while (!finished);
            }).Wait(Timeout.InfiniteTimeSpan);
            
            return agreementNumberAndIdDataList.ToLookup(x => x.approval_agreementNumber__c, y => y.Id);
        }
    }
}
