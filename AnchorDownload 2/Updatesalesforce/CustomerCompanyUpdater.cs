using SalesforceDataLibrary;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace UpdateSalesforceData
{
    class CustomerCompanyUpdater
    {

        public static void Run()
        {
            SalesforceDataHelper.SalesforceClient.BulkUpsertFromSQL("CustomerCompany", "Account", "sentinalCompanyId__c",
                true, GetCustomerCompanyDataFromSQL, customerCompanyData =>
                    new CustomSObject()
                    {
                        { "sentinalCompanyId__c", SQLDataHelper.Helper.GetStringMaxLength(string.Concat("C", customerCompanyData["CustomerNumber"].ToString().TrimStart('0')), 30) },
                        { "Name", SQLDataHelper.Helper.GetStringMaxLength(customerCompanyData["CustomerCompany"].ToString().Trim(), 255) },
                        { "BillingStreet", SQLDataHelper.Helper.GetStringMaxLength(string.Join("\n", new List<string>() {
                            customerCompanyData["CustomerAddress1"].ToString(),
                            customerCompanyData["CustomerAddress2"].ToString(),
                            customerCompanyData["CustomerAddress3"].ToString()}.Where(x => x.Trim().Length != 0)).Trim(),255) },
                        { "BillingCity", SQLDataHelper.Helper.GetStringMaxLength(customerCompanyData["CustomerAddress4"].ToString().Trim(), 40) },
                        { "BillingState", SQLDataHelper.Helper.GetStringMaxLength(customerCompanyData["CustomerAddress5"].ToString().Trim(), 80) },
                        { "BillingPostalCode", SQLDataHelper.Helper.GetStringMaxLength(customerCompanyData["CustomerPostCode"].ToString().Trim(), 20) },
                        { "BillingCountry", SQLDataHelper.Helper.GetStringMaxLength(customerCompanyData["CustomerCountry"].ToString().Trim(), 80) },
                        { "Phone", SQLDataHelper.Helper.GetStringMaxLength(customerCompanyData["CustomerPhoneNumber"].ToString().Trim(), 40) },
                        { "company_contact_altPhone__c", SQLDataHelper.Helper.GetStringMaxLength(customerCompanyData["CustomerMobileNumber"].ToString().Trim(), 255) },
                        { "company_contact_EmailAddress__c", SQLDataHelper.Helper.GetStringMaxLength(customerCompanyData["CustomerEmail"].ToString().Trim(), 80) },
                        { "company_registration_VATNumber__c", SQLDataHelper.Helper.GetStringMaxLength(customerCompanyData["CustomerVATNumber"].ToString().Trim(), 255) },
                        { "lastUpdatedFromSentinel__c" , SQLDataHelper.Helper.GetDateTime(DateTime.Now)},
                    }, null);

            LogHelper.Logger.WriteOutput(string.Concat("Ended Sync of CustomerCompany"), Program.EmailTransactionLog);
        }


        private static SqlDataReader GetCustomerCompanyDataFromSQL()
        {
            return new SqlCommand("SELECT * FROM SQL_SF_Sync_CustomerCompanies() Order By CustomerNumber", SQLDataConnectionHelper.SqlConnection) { CommandTimeout = 0 }.ExecuteReader();
        }

    }
}
