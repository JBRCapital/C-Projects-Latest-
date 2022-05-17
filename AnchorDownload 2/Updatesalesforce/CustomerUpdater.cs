using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using SalesforceDataLibrary;

namespace UpdateSalesforceData
{
    class CustomerUpdater
    {
        public static void Run()
        {
            SalesforceDataHelper.SalesforceClient.BulkUpsertFromSQL("Customer", "Contact", "panther_id__c",
                true, GetCustomerDataFromSQL,
                customerData =>
                    new CustomSObject()
                    {
                        { "sentinelLongNumber__c", SQLDataHelper.Helper.GetStringMaxLength(customerData["LongCustomerNumber"].ToString().Trim(),18) },
                        { "panther_id__c", SQLDataHelper.Helper.GetStringMaxLength(customerData["CustomerNumber"].ToString().Trim(),18) },
                        { "Title", SQLDataHelper.Helper.GetStringMaxLength(customerData["CustomerTitle"].ToString().Trim(),128) },
                        { "FirstName", SQLDataHelper.Helper.GetStringMaxLength(customerData["CustomerFirstPlusMiddleName"].ToString().Trim(),40) },
                        { "LastName", SQLDataHelper.Helper.GetStringMaxLength(customerData["CustomerLastName"].ToString().Trim(),80) },
                       // { "Name", customerData["CustomerName"].ToString().Trim() },
                        { "CustomerStatus__c", SQLDataHelper.Helper.GetStringMaxLength(customerData["CustomerStatus"].ToString().Trim(), 255) },
                        { "Birthdate", SQLDataHelper.Helper.GetDateTime(customerData["CustomerDateOfBirth"]) },
                        { "MailingStreet",  SQLDataHelper.Helper.GetStringMaxLength(string.Join("\n", new List<string>() {
                            customerData["CustomerAddress1"].ToString(),
                            customerData["CustomerAddress2"].ToString(),
                            customerData["CustomerAddress3"].ToString()}.Where(x => x.Trim().Length != 0)),255) },
                        { "MailingCity", SQLDataHelper.Helper.GetStringMaxLength(customerData["CustomerAddress4"].ToString().Trim(),40)},
                        { "MailingState", SQLDataHelper.Helper.GetStringMaxLength(customerData["CustomerAddress5"].ToString().Trim(),20)},
                        { "MailingPostalCode",  SQLDataHelper.Helper.GetStringMaxLength(customerData["CustomerPostCode"].ToString().Trim(),20)},
                        { "MailingCountry", SQLDataHelper.Helper.GetStringMaxLength(customerData["CustomerCountry"].ToString().Trim(),40)},
                        { "Phone", SQLDataHelper.Helper.GetStringMaxLength(customerData["CustomerPhoneNumber"].ToString().Trim(),40)},
                        { "HomePhone", SQLDataHelper.Helper.GetStringMaxLength(customerData["CustomerPhone2"].ToString().Trim(),40)},
                        { "OtherPhone", SQLDataHelper.Helper.GetStringMaxLength(customerData["CustomerPhone3"].ToString().Trim(),40)},
                        { "MobilePhone", SQLDataHelper.Helper.GetStringMaxLength(customerData["CustomerMobileNumber"].ToString().Trim(),40)},
                        { "mobileNumber__c", SQLDataHelper.Helper.GetStringMaxLength(customerData["CustomerAlternativeMobileNumber"].ToString().Trim(),255)},
                        { "DoNotCall", SQLDataHelper.Helper.GetBoolean(customerData["CustomerDoNotCall"])},
                        { "Email", SMTPHelper.Helpers.IsValidEmail(customerData["CustomerEmail"].ToString(), true) ? customerData["CustomerEmail"].ToString() : "syncErrorWithEmail@jbrcaptial.com"},
                        { "HasOptedOutOfEmail", SQLDataHelper.Helper.GetBoolean(customerData["CustomerHasOptedOutOfEmail"])},
                        { "lastUpdatedFromSentinel__c" , SQLDataHelper.Helper.GetDateTime(DateTime.Now)}
                    }, customerData =>
                    !SMTPHelper.Helpers.IsValidEmail(customerData["CustomerEmail"].ToString(), true) ?
                        $"Invalid email address: {customerData["CustomerNumber"]}-{customerData["CustomerFirstName"]}-{customerData["CustomerLastName"]}-{customerData["CustomerEmail"]}" :
                        string.Empty, 50);

            LogHelper.Logger.WriteOutput(string.Concat("Ended Sync of Customer"), Program.EmailTransactionLog);
        }

        private static SqlDataReader GetCustomerDataFromSQL()
        {
            return new SqlCommand("SELECT * FROM SQL_SF_Sync_Customers() ORDER BY CustomerNumber", SQLDataConnectionHelper.SqlConnection) { CommandTimeout = 0 }.ExecuteReader();
        }

    }
}
