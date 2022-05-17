using Salesforce.Common.Models.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Salesforce.Common.Models.Xml;

namespace UpdateSalesforceData
{
    class CustomerUpdater
    {
        public static void UpdateCustomerData()
        {
            Console.WriteLine(string.Concat("Started Sync of Customer"));

            var sb = Program.salesforceClient.BulkUpsertFromSQL("Contact", "panther_id__c",
                GetCustomerDataFromSQL, customerData =>
                    new SObject()
                    {
                        { "panther_id__c", customerData["Id"].ToString().Trim() },
                        { "Title", customerData["Title"].ToString().Trim() },
                        { "FirstName", customerData["FirstName"].ToString().Trim() },
                        { "LastName", customerData["LastName"].ToString().Trim() },
                        { "Name", customerData["Name"].ToString().Trim() },
                        { "Birthdate", DataHelper.GetDateTime(customerData["DOB"]) },
                        { "MailingStreet", string.Join("\n", new List<string>() {
                            customerData["Address1"].ToString(),
                            customerData["Address2"].ToString(),
                            customerData["Address3"].ToString()}.Where(x => x.Trim().Length != 0)) },
                        { "MailingCity", customerData["Town"].ToString().Trim()},
                        { "MailingState", customerData["County"].ToString().Trim()},
                        { "MailingPostalCode", customerData["PostCode"].ToString().Trim()},
                        { "MailingCountry", customerData["Country"].ToString().Trim()},
                        { "Phone", customerData["Phone1"].ToString().Trim()},
                        { "HomePhone", customerData["Phone2"].ToString().Trim()},
                        { "OtherPhone", customerData["Phone3"].ToString().Trim()},
                        { "MobilePhone", customerData["MobileNumber"].ToString().Trim()},
                        { "mobileNumber__c", customerData["AlternativeMobileNumber"].ToString().Trim()},
                        { "Email", Helper.IsValidEmail(customerData["Email"].ToString(), true) ? customerData["Email"].ToString() : "syncErrorWithEmail@jbrcaptial.com"},
                        { "HasOptedOutOfEmail", DataHelper.GetBoolean(customerData["HasOptedOutOfEmail"])},
                        { "DoNotCall", DataHelper.GetBoolean(customerData["DoNotCall"])}
                    },
                customerData =>
                    !Helper.IsValidEmail(customerData["Email"].ToString(), true) ?
                        $"{customerData["Id"]}-{customerData["FirstName"]}-{customerData["LastName"]}-{customerData["Email"]}" :
                        string.Empty);

            Console.Write(sb.ToString());

            Console.WriteLine(string.Concat("Ended Sync of Customer"));
        }

        private static SqlDataReader GetCustomerDataFromSQL(SqlConnection sqlConn)
        {
            return new SqlCommand("SELECT * FROM GetCustomersOrCompaniesDetails(1) Order By Id", sqlConn) { CommandTimeout = 0 }.ExecuteReader();
        }

        #region Legacy and commented code
        private static void updateCustomerCompanyRecordInSalesforce(CustomerData customerData)
        {
            //try
            //{

            var queryString = string.Concat(@"SELECT panther_id__c,

             Title,FirstName,LastName,Name,
          
             Birthdate,
          
             MailingStreet,MailingCity,MailingState,MailingPostalCode,MailingCountry,
             Phone,HomePhone,OtherPhone,
          
             MobilePhone, mobileNumber__c,
             Email,

             DoNotCall, HasOptedOutOfEmail

            FROM Contact
            WHERE panther_id__c = ", customerData.panther_id__c);

            ////HasOptedOutOfPost, HasOptedOutOfSMS

            QueryResult<CustomerData> anchorWebServices = null;

            Task.Run(async () =>
            {
                anchorWebServices = await Program.salesforceClient.QueryAsync<CustomerData>(queryString);
            }).Wait(Timeout.InfiniteTimeSpan);

            var changed = false;

            if (anchorWebServices.Records.Count > 0)
            {
                Console.WriteLine(string.Concat("Checking Customer:", customerData.panther_id__c));

                dynamic obj = getUpdateObject(customerData, anchorWebServices, ref changed);
                Console.WriteLine(string.Concat("Customer Changed: ", changed ? "Yes" : "No"));

                if (changed)
                {
                    SuccessResponse successResponse = null;

                    Task.Run(async () =>
                    {
                        Console.WriteLine(string.Concat("Upserted Customer:", customerData.panther_id__c));
                        try
                        {
                            successResponse = await Program.salesforceClient.UpsertExternalAsync("Contact", "panther_id__c", customerData.panther_id__c, obj);
                        }
                        catch (Exception ex)
                        {
                        }
                        Console.WriteLine(successResponse != null && successResponse.Success ? "Success" : "Failed");
                    }).Wait(Timeout.InfiniteTimeSpan);
                }
            }
        }

        private static dynamic getUpdateObject(CustomerData customerData, QueryResult<CustomerData> anchorWebServices, ref bool changed)
        {
            dynamic obj = new ExpandoObject() as IDictionary<string, object>;
            var awsR = anchorWebServices.Records[0];

            //Title
            DataHelper.setValue(awsR.Title, customerData.Title, obj, "Title", ref changed);

            //FirstName
            DataHelper.setValue(awsR.FirstName, customerData.FirstName, obj, "FirstName", ref changed);

            //MiddleName
            //DataHelper.setValue(awsR.MiddleName, customerData.MiddleName, obj, "MiddleName", ref changed);

            //LastName
            DataHelper.setValue(awsR.LastName, customerData.LastName, obj, "LastName", ref changed);

            //Name
            //DataHelper.setValue(awsR.Name, customerData.Name, obj, "Name", ref changed);

            //Birthdate
            DataHelper.setValue(awsR.Birthdate, customerData.Birthdate, obj, "Birthdate", ref changed);

            //MailingStreet
            DataHelper.setValue(awsR.MailingStreet, customerData.MailingStreet, obj, "MailingStreet", ref changed);
            //MailingCity
            DataHelper.setValue(awsR.MailingCity, customerData.MailingCity, obj, "MailingCity", ref changed);
            //MailingState
            DataHelper.setValue(awsR.MailingState, customerData.MailingState, obj, "MailingState", ref changed);

            //MailingPostalCode
            DataHelper.setValue(awsR.MailingPostalCode, customerData.MailingPostalCode, obj, "MailingPostalCode", ref changed);
            //MailingCountry
            DataHelper.setValue(awsR.MailingCountry, customerData.MailingCountry, obj, "MailingCountry", ref changed);

            //Phone
            DataHelper.setValue(awsR.Phone, customerData.Phone, obj, "Phone", ref changed);
            //HomePhone
            DataHelper.setValue(awsR.HomePhone, customerData.HomePhone, obj, "HomePhone", ref changed);
            //OtherPhone
            DataHelper.setValue(awsR.OtherPhone, customerData.OtherPhone, obj, "OtherPhone", ref changed);

            //MobilePhone
            DataHelper.setValue(awsR.MobilePhone, customerData.MobilePhone, obj, "MobilePhone", ref changed);
            //mobileNumber__c
            DataHelper.setValue(awsR.mobileNumber__c, customerData.mobileNumber__c, obj, "mobileNumber__c", ref changed);

            //Email
            DataHelper.setValue(awsR.Email, customerData.Email, obj, "Email", ref changed);

            //DoNotCall
            DataHelper.setValue(awsR.DoNotCall, customerData.DoNotCall, obj, "DoNotCall", ref changed);

            //HasOptedOutOfEmail
            DataHelper.setValue(awsR.HasOptedOutOfEmail, customerData.HasOptedOutOfEmail, obj, "HasOptedOutOfEmail", ref changed);

            //HasOptedOutOfPost
            //HasOptedOutOfSMS

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
