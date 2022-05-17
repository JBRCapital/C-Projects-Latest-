using Salesforce.Common.Models;
using Salesforce.Common.Models.Json;
using Salesforce.Common.Models.Xml;
using Salesforce.Force;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UpdateSalesforceData
{
    class CustomerCompanyUpdater
    {

        public static void UpdateCustomerCompanyData()
        {
            Console.WriteLine(string.Concat("Started Sync of CustomerCompany"));

            Program.salesforceClient.BulkUpsertFromSQL("Account", "sentinalCompanyId__c",
                GetCustomerCompanyDataFromSQL, customerCompanyData =>
                    new SObject()
                    {
                        { "sentinalCompanyId__c", string.Concat("C", customerCompanyData["Id"].ToString().TrimStart('0')) },
                        { "Name", customerCompanyData["CompanyName"].ToString().Trim() },
                        { "BillingStreet", string.Join("\n", new List<string>() {
                            customerCompanyData["Address1"].ToString(),
                            customerCompanyData["Address2"].ToString(),
                            customerCompanyData["Address3"].ToString()}.Where(x => x.Trim().Length != 0)).Trim() },
                        { "BillingCity", customerCompanyData["Town"].ToString().Trim() },
                        { "BillingState", customerCompanyData["County"].ToString().Trim() },
                        { "BillingPostalCode", customerCompanyData["PostCode"].ToString().Trim() },
                        { "BillingCountry", customerCompanyData["Country"].ToString().Trim() },
                        { "Phone", customerCompanyData["Phone1"].ToString().Trim() },
                        { "company_contact_altPhone__c", customerCompanyData["Phone2"].ToString().Trim() },
                        { "company_contact_EmailAddress__c", customerCompanyData["Email"].ToString().Trim() },
                        { "company_registration_VATNumber__c", customerCompanyData["VATNumber"].ToString().Trim() }
                    });

            Console.WriteLine(string.Concat("Ended Sync of CustomerCompany"));
        }


        private static SqlDataReader GetCustomerCompanyDataFromSQL(SqlConnection sqlConn)
        {
            return new SqlCommand("SELECT * FROM GetCustomersOrCompaniesDetails(9) Order By Id", sqlConn) { CommandTimeout = 0 }.ExecuteReader();
        }


        #region Legacy and commented code
        private static void updateCustomerCompanyRecordInSalesforce(CustomerCompanyData customerCompanyData)
        {
            //try
            //{
            var queryString = string.Concat(@"SELECT id, Name, BillingStreet, BillingCity,BillingState, BillingPostalCode, BillingCountry, Phone, 
                                                     company_contact_altPhone__c, company_contact_EmailAddress__c, company_contact_name__c, 
                                                     company_registration_VATNumber__c
                                              FROM Account
                                              WHERE sentinalCompanyId__c = '", customerCompanyData.sentinalCompanyId__c, "'");

            QueryResult<CustomerCompanyData> anchorWebServices = null;

            Task.Run(async () =>
            {
                anchorWebServices = await Program.salesforceClient.QueryAsync<CustomerCompanyData>(queryString);
            }).Wait(Timeout.InfiniteTimeSpan);

            var changed = false;

            if (anchorWebServices.Records.Count > 0)
            {
                Console.WriteLine(string.Concat("Checking CustomerCompany:", customerCompanyData.sentinalCompanyId__c));
                dynamic obj = getUpdateObject(customerCompanyData, anchorWebServices, ref changed);
                Console.WriteLine(string.Concat("CustomerCompany Changed: ", changed ? "Yes" : "No"));

                if (changed)
                {
                    SuccessResponse successResponse = null;

                    Task.Run(async () =>
                    {
                        Console.WriteLine(string.Concat("Upserted CustomerCompany:", customerCompanyData.sentinalCompanyId__c));
                        successResponse = await Program.salesforceClient.UpsertExternalAsync("Account", "sentinalCompanyId__c", customerCompanyData.sentinalCompanyId__c, obj);
                        Console.WriteLine(successResponse != null && successResponse.Success ? "Success" : "Failed");
                    }).Wait(Timeout.InfiniteTimeSpan);
                }
            }
        }

        private static dynamic getUpdateObject(CustomerCompanyData customerCompanyData, QueryResult<CustomerCompanyData> anchorWebServices, ref bool changed)
        {
            dynamic obj = new ExpandoObject() as IDictionary<string, object>;
            var awsR = anchorWebServices.Records[0];

            //Name
            DataHelper.setValue(awsR.Name, customerCompanyData.Name, obj, "Name", ref changed);

            //BillingStreet
            DataHelper.setValue(awsR.BillingStreet, customerCompanyData.BillingStreet, obj, "BillingStreet", ref changed);

            //BillingTown
            DataHelper.setValue(awsR.BillingCity, customerCompanyData.BillingCity, obj, "BillingCity", ref changed);

            //BillingCounty
            DataHelper.setValue(awsR.BillingState, customerCompanyData.BillingState, obj, "BillingState", ref changed);

            //BillingPostalCode
            DataHelper.setValue(awsR.BillingPostalCode, customerCompanyData.BillingPostalCode, obj, "BillingPostalCode", ref changed);

            //BillingCountry
            DataHelper.setValue(awsR.BillingCountry, customerCompanyData.BillingCountry, obj, "BillingCountry", ref changed);

            //Phone
            DataHelper.setValue(awsR.Phone, customerCompanyData.Phone, obj, "Phone", ref changed);

            //company_contact_altPhone__c
            DataHelper.setValue(awsR.company_contact_altPhone__c, customerCompanyData.company_contact_altPhone__c, obj, "company_contact_altPhone__c", ref changed);

            //company_contact_EmailAddress__c
            DataHelper.setValue(awsR.company_contact_EmailAddress__c, customerCompanyData.company_contact_EmailAddress__c, obj, "company_contact_EmailAddress__c", ref changed);

            //company_contact_name__c
            //DataHelper.setValue(awsR.company_contact_name__c, customerCompanyData.company_contact_name__c, obj, "company_contact_name__c", ref changed);

            //company_registration_VATNumber__c
            DataHelper.setValue(awsR.company_registration_VATNumber__c, customerCompanyData.company_registration_VATNumber__c, obj, "company_registration_VATNumber__c", ref changed);

            ((IDictionary<string, object>)obj).Add("lastUpdatedFromSentinel__c", DateTime.Now);

            return obj;
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

        #endregion
    }
}
