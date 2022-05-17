using System;
using System.Data.SqlClient;
using System.Configuration;
using Salesforce.Common;
using Salesforce.Force;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Web;

namespace UpdateSalesforceData
{
    public class DataHelper
    {
        public static string SecurityToken = ConfigurationManager.AppSettings["SecurityToken"];
        public static string ConsumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
        public static string ConsumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
        public static string Username = ConfigurationManager.AppSettings["Username"];
        public static string Password = ConfigurationManager.AppSettings["Password"];
        public static string connectionString = ConfigurationManager.AppSettings["DatabaseConnectionString"];

        public static SqlConnection GetOpenConnection()
        {
            var sqlConn = new SqlConnection(connectionString);
            sqlConn.Open();

            return sqlConn;
        }

        public static SalesforceHttpClient /*async static Task<ForceClient>*/ GetSalesforceConnection()
        {
            SalesforceHttpClient.SalesforceSettings salesforceSettings = new SalesforceHttpClient.SalesforceSettings
            {
                Domain = "https://login.salesforce.com",
                ConsumerKey = ConsumerKey,
                ConsumerSecret = ConsumerSecret,
                Username = Username,
                Password = Password,
                SecurityToken = SecurityToken
            };

            SalesforceHttpClient.Init(salesforceSettings);

            //for test..
            //var testUrl = "https://test.salesforce.com/services/oauth2/token";
            //await auth.UsernamePasswordAsync(ConsumerKey, ConsumerSecret, Username, Password + SecurityToken, testUrl);

            //for live..
            //await auth.UsernamePasswordAsync(ConsumerKey, ConsumerSecret, Username, Password + SecurityToken);

            //GetSalesforceConnection

            //SalesforceHttpClient salesforceClient = new SalesforceHttpClient();
            //salesforceClient.


            //return new ForceClient(auth.InstanceUrl, auth.AccessToken, auth.ApiVersion);

            return new SalesforceHttpClient();
        }

        #region setvalue

        public static void setValue(String salesforceValue, String databaseValue, IDictionary<string, object> changedValues, string name, ref bool changed)
        {
            if (salesforceValue == string.Empty) { salesforceValue = null; }
            if (string.IsNullOrEmpty(databaseValue)) { databaseValue = null; } else { databaseValue = databaseValue.Trim(); }
            if ((salesforceValue == null && databaseValue != null) || ((salesforceValue != null && salesforceValue != databaseValue)))
            { changedValues.Add(name, databaseValue); changed = true; }
        }

        public static void setValue(Int32? salesforceValue, Int32? databaseValue, IDictionary<string, object> changedValues, string name, ref bool changed)
        {
            if ((!salesforceValue.HasValue && databaseValue.HasValue) || (salesforceValue.HasValue && (!databaseValue.HasValue || salesforceValue.Value != databaseValue.Value)))
            { changedValues.Add(name, databaseValue.Value); changed = true; }
        }

        public static void setValue(double? salesforceValue, double? databaseValue, IDictionary<string, object> changedValues, string name, ref bool changed)
        {
            if ((!salesforceValue.HasValue && databaseValue.HasValue) || (salesforceValue.HasValue && (!databaseValue.HasValue || salesforceValue.Value != databaseValue.Value)))
            { changedValues.Add(name, databaseValue.Value); changed = true; }
        }

        public static void setValue(DateTime? salesforceValue, DateTime? databaseValue, IDictionary<string, object> changedValues, string name, ref bool changed)
        {
            if ((!salesforceValue.HasValue && databaseValue.HasValue) || (salesforceValue.HasValue && (!databaseValue.HasValue || salesforceValue.Value != databaseValue.Value)))
            { changedValues.Add(name, (!databaseValue.HasValue ? (DateTime?)null : databaseValue.Value)); changed = true; }
        }

        public static void setValue(Boolean? salesforceValue, Boolean? databaseValue, IDictionary<string, object> changedValues, string name, ref bool changed)
        {
            if ((!salesforceValue.HasValue && databaseValue.HasValue) || (salesforceValue.HasValue && (!databaseValue.HasValue || salesforceValue.Value != databaseValue.Value)))
            { changedValues.Add(name, databaseValue.Value); changed = true; }
        }

        #endregion

        #region GetTypeValue
        public static string GetString(object value)
        {
            if (value is DBNull || value == null) return null;
            if (string.IsNullOrEmpty(value.ToString())) { return string.Empty; }
            return HttpUtility.HtmlEncode(value.ToString().Trim());
        }

        public static Boolean? GetBoolean(object value)
        {
            return value is DBNull || value == null ? false : Convert.ToBoolean(value);
        }

        public static double? GetDouble(object value)
        {
            return value is DBNull || value == null || value.ToString() == "" ? 0 /*default(double?)*/ : Convert.ToDouble(value);
        }

        public static Int32? GetInt32(object value)
        {
            return value is DBNull || value == null || value.ToString() == "" ? 0 /*default(int?)*/ : Convert.ToInt32(value);
        }

        public static string GetDateTime(object value)
        {
            return value is DBNull || value == null ? "Null"/*((DateTime?)null).ToString() default(DateTime?)*/ : Convert.ToDateTime(value).ToString("s");
        }

        #endregion

    }
}
