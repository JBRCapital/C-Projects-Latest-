using System;
using System.Configuration;
using Salesforce.Common;
using Salesforce.Force;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace APRCalculatorService
{
    public class DataHelper
    {

        static string SecurityToken = Properties.Resources.SecurityToken;
        static string ConsumerKey = Properties.Resources.ConsumerKey;
        static string ConsumerSecret = Properties.Resources.ConsumerSecret;
        static string Username = Properties.Resources.Username;
        static string Password = Properties.Resources.Password;
        //string connectionString = "Server=DEDICAT-0J2R68U;Database=JBR_Internal;Trusted_Connection=True;";

        /*public static SqlConnection GetOpenConnection()
        {
            var sqlConn = new SqlConnection(connectionString);
            sqlConn.Open();

            return sqlConn;
        }*/

        public async static Task<ForceClient> GetSalesforceConnection()
        {
            var auth = new AuthenticationClient();

            //for test..
            //var testUrl = "https://test.salesforce.com/services/oauth2/token";
            //await auth.UsernamePasswordAsync(ConsumerKey, ConsumerSecret, Username, Password + SecurityToken, testUrl);

            //for live..
            await auth.UsernamePasswordAsync(ConsumerKey, ConsumerSecret, Username, Password + SecurityToken);

            return new ForceClient(auth.InstanceUrl, auth.AccessToken, auth.ApiVersion);
        }

        #region setvalue

        public static void setValue(String salesforceValue, String databaseValue, IDictionary<string, object> changedValues, string name, ref bool changed)
        {
            if (salesforceValue == string.Empty) { salesforceValue = null; }
            if (databaseValue == string.Empty) { databaseValue = null; } else { databaseValue = databaseValue.Trim(); }
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
            return value is DBNull || value == null || value.ToString() == "" ? default(int?) : Convert.ToInt32(value);
        }

        public static DateTime? GetDateTime(object value)
        {
            return value is DBNull || value == null ? default(DateTime?) : Convert.ToDateTime(value);
        }

        #endregion

    }
}