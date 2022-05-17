/*using Salesforce.Common.Models.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UpdateSalesforceData
{
    class DealerUpdater
    {

        public static void UpdateAccountData(SqlConnection sqlConn, SalesforceHttpClient salesforceClient)
        {
            Console.WriteLine(string.Concat("Started Sync of Dealers"));

            var AccountData = GetAccountDataFromSQL(sqlConn);
            var dateTimeSyncStarted = DateTime.Now;

            while (AccountData.Read())
            {
                var AccountDataForSF = new AccountData()
                {
                    sentinalCompanyId__c = string.Concat("S", AccountData["Number"].ToString().TrimStart('0')),
                    Name = AccountData["Name"].ToString(),
                    BillingStreet = string.Join("\n", new List<string>() {
                             AccountData["Address1"].ToString(),
                             AccountData["Address2"].ToString(),
                             AccountData["Address3"].ToString(),
                             AccountData["Address4"].ToString(),
                             AccountData["Address5"].ToString() }.Where(x => x.Trim().Length != 0)),
                    BillingPostalCode = AccountData["PostCode"].ToString(),
                    Phone = AccountData["PhoneNumber1"].ToString(),
                    company_contact_altPhone__c = AccountData["PhoneNumber2"].ToString(),
                    company_contact_EmailAddress__c = AccountData["Email"].ToString(),
                    company_contact_name__c = AccountData["ContactName"].ToString()
                };

                updateDealerRecordInSalesforce(salesforceClient, AccountDataForSF);

            }

            AccountData.Close();

            //UpdateLastTimeAgreementsWereUpdated(dateTimeSyncStarted, sqlConn);

            Console.WriteLine(string.Concat("Ended Sync of Dealers"));
        }

        private static void updateDealerRecordInSalesforce(SalesforceHttpClient salesforceClient, AccountData AccountData)
        {
            //try
            //{
            var queryString = string.Concat(@"SELECT id, Name, BillingStreet, BillingPostalCode, Phone, 
                                                     company_contact_altPhone__c, company_contact_EmailAddress__c, company_contact_name__c
                                              FROM Account
                                              WHERE sentinalCompanyId__c = '", AccountData.sentinalCompanyId__c, "'");

            QueryResult<AccountData> anchorWebServices = null;

            Task.Run(async () =>
            {
                anchorWebServices = await salesforceClient.QueryAsync<AccountData>(queryString);
            }).Wait(Timeout.InfiniteTimeSpan);

            var changed = false;
            var runUpsert = false;
            dynamic obj = null;

            if (anchorWebServices.Records.Count > 0)
            {
                Console.WriteLine(string.Concat("Checking Dealer:", AccountData.sentinalCompanyId__c));
                obj = getUpdateObject(AccountData, anchorWebServices, ref changed);
                Console.WriteLine(string.Concat("Dealer Changed: ", changed ? "Yes" : "No"));

                if (changed)
                {
                    runUpsert = true;
                }
            }
            else
            {
                runUpsert = true;
                obj = new ExpandoObject();
                obj.Name = AccountData.Name;
                obj.BillingStreet = AccountData.BillingStreet;
                obj.BillingPostalCode = AccountData.BillingPostalCode;
                obj.Phone = AccountData.Phone;
                obj.company_contact_altPhone__c = AccountData.company_contact_altPhone__c;
                obj.company_contact_EmailAddress__c = AccountData.company_contact_EmailAddress__c;
                obj.company_contact_name__c = AccountData.company_contact_name__c;
            }

            if (runUpsert)
            {
                obj.RecordType = new ExpandoObject();
                obj.RecordType.Name = "Supplier";
                SuccessResponse successResponse = null;

                Task.Run(async () =>
                {
                    Console.WriteLine(string.Concat("Upserted Dealer:", AccountData.sentinalCompanyId__c));
                    successResponse = await salesforceClient.UpsertExternalAsync("Account", "sentinalCompanyId__c", AccountData.sentinalCompanyId__c, obj);
                    Console.WriteLine(successResponse != null && successResponse.Success ? "Success" : "Failed");
                }).Wait(Timeout.InfiniteTimeSpan);
            }
        }

        private static dynamic getUpdateObject(AccountData AccountData, QueryResult<AccountData> anchorWebServices, ref bool changed)
        {
            dynamic obj = new ExpandoObject() as IDictionary<string, object>;
            var awsR = anchorWebServices.Records[0];

            //Name
            DataHelper.setValue(awsR.Name, AccountData.Name, obj, "Name", ref changed);

            //BillingAddress.Street
            DataHelper.setValue(awsR.BillingStreet, AccountData.BillingStreet, obj, "BillingStreet", ref changed);

            //BillingAddress.PostalCode
            DataHelper.setValue(awsR.BillingPostalCode, AccountData.BillingPostalCode, obj, "BillingPostalCode", ref changed);

            //Phone
            DataHelper.setValue(awsR.Phone, AccountData.Phone, obj, "Phone", ref changed);

            //company_contact_altPhone__c
            DataHelper.setValue(awsR.company_contact_altPhone__c, AccountData.company_contact_altPhone__c, obj, "company_contact_altPhone__c", ref changed);

            //company_contact_EmailAddress__c
            DataHelper.setValue(awsR.company_contact_EmailAddress__c, AccountData.company_contact_EmailAddress__c, obj, "company_contact_EmailAddress__c", ref changed);

            //company_contact_name__c
            DataHelper.setValue(awsR.company_contact_name__c, AccountData.company_contact_name__c, obj, "company_contact_name__c", ref changed);

            ((IDictionary<string, object>)obj).Add("lastUpdatedFromSentinel__c", DateTime.Now);

            return obj;
        }

        private static SqlDataReader GetAccountDataFromSQL(SqlConnection sqlConn)
        {
            return new SqlCommand("SELECT * FROM GetDealerDetailsForSalesforce() Order By Number", sqlConn){ CommandTimeout = 0 }.ExecuteReader();
        }

        //private static void UpdateLastTimeAgreementsWereUpdated(DateTime lastTimeAgreementsWereUpdated, SqlConnection sqlConn)
        //{
        //    using (SqlCommand cmd = new SqlCommand())
        //    {
        //        cmd.CommandText = "UpdateLastTimeAgreementsWereUpdated";
        //        cmd.CommandType = CommandType.StoredProcedure;
        //        cmd.Parameters.AddWithValue("DateOfLastUpdate", lastTimeAgreementsWereUpdated);

        //        cmd.Connection = sqlConn;
        //        cmd.ExecuteNonQuery();
        //    }
        //}

    }
}
*/