using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using SalesforceDataLibrary;

namespace UpdateSalesforceData
{
    class DealerUpdater
    {

        public static void Run()
        {
            SalesforceDataHelper.SalesforceClient.BulkUpsertFromSQL("Dealers", "Account", "sentinalCompanyId__c",
                true, GetAccountDataFromSQL, AccountData =>
                    new CustomSObject()
                    {
                        { "sentinalCompanyId__c", string.Concat("S", AccountData["Number"].ToString().TrimStart('0'))},
                        { "Name",  SQLDataHelper.Helper.GetStringMaxLength(AccountData["Name"].ToString().Trim(),255)},
                        { "BillingStreet",  SQLDataHelper.Helper.GetStringMaxLength(string.Join("\n", new List<string>() {
                            AccountData["Address1"].ToString(),
                            AccountData["Address2"].ToString(),
                            AccountData["Address3"].ToString(),
                            AccountData["Address4"].ToString(),
                            AccountData["Address5"].ToString() }.Where(x => x.Trim().Length != 0)),255)},
                        { "BillingPostalCode",  SQLDataHelper.Helper.GetStringMaxLength(AccountData["PostCode"].ToString().Trim(),20)},
                        { "Phone",  SQLDataHelper.Helper.GetStringMaxLength(AccountData["PhoneNumber1"].ToString().Trim(),40)},
                        { "company_contact_altPhone__c",  SQLDataHelper.Helper.GetStringMaxLength(AccountData["PhoneNumber2"].ToString().Trim(),255)},
                        { "company_contact_EmailAddress__c",  SQLDataHelper.Helper.GetStringMaxLength(AccountData["Email"].ToString().Trim(),255)},
                        { "company_contact_name__c",  SQLDataHelper.Helper.GetStringMaxLength(AccountData["ContactName"].ToString().Trim(),255)},
                        { "lastUpdatedFromSentinel__c" , SQLDataHelper.Helper.GetDateTime(DateTime.Now)}
                    }, null);

            LogHelper.Logger.WriteOutput(string.Concat("Ended Sync of Dealers"), Program.EmailTransactionLog);
        }

        private static SqlDataReader GetAccountDataFromSQL()
        {
            return new SqlCommand("SELECT * FROM SQL_SF_Sync_Dealers() Order By Number", SQLDataConnectionHelper.SqlConnection){ CommandTimeout = 0 }.ExecuteReader();
        }

        
    }
}
