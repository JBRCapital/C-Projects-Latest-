using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Generic;
using SalesforceDataLibrary;
using System.Data;

namespace VehicleUpdater
{
    public class DataHelper
    {
        public static string Domain = ConfigurationManager.AppSettings["Domain"];
        public static string SecurityToken = ConfigurationManager.AppSettings["SecurityToken"];
        public static string ConsumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
        public static string ConsumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
        public static string Username = ConfigurationManager.AppSettings["Username"];
        public static string Password = ConfigurationManager.AppSettings["Password"];

        
        //SQL ---------------------------------------------------------------------------------------------

        public static string connectionString = "Server=JBRDB01;Database=JBR_Internal;Trusted_Connection=True;";


        private static SqlConnection sqlConn;
        public static SqlConnection SqlConn
        {
            get
            {
                if (sqlConn == null) { sqlConn = new SqlConnection(connectionString); sqlConn.Open(); }
                return sqlConn;
            }
        }

        public static void RunCommand(string commmandText, CommandType commandType, List<SqlParameter> sqlParameterCollection)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = commmandText;
                cmd.CommandType = commandType;

                foreach (var parameter in sqlParameterCollection)
                {
                    cmd.Parameters.Add(parameter);
                }

                cmd.Connection = SqlConn;
                cmd.ExecuteNonQuery();
            }
        }

        //End of SQL -----------------------------------------------------------------------------------------


        //public static SqlConnection SqlConn1 { get { if (sqlConn1 == null) { sqlConn1 = DataHelper.GetOpenConnection(); } return sqlConn1; } }

        public static SalesforceClient /*async static Task<ForceClient>*/ GetSalesforceConnection()
        {
            SalesforceClient.Init(new SalesforceClient.SalesforceSettings
            {
                Domain = Domain,
                ConsumerKey = ConsumerKey,
                ConsumerSecret = ConsumerSecret,
                Username = Username,
                Password = Password,
                SecurityToken = SecurityToken
            }, VehicleUpdater.EmailTransactionLog, VehicleUpdater.EmailErrorLog);

            return new SalesforceClient();
        }

        #region setvalue

        public static void setValue(string salesforceValue, string databaseValue, IDictionary<string, object> changedValues, string name, ref bool changed)
        {
            if (salesforceValue == string.Empty) { salesforceValue = null; }
            if (string.IsNullOrEmpty(databaseValue)) { databaseValue = null; } else { databaseValue = databaseValue.Trim(); }
            if (salesforceValue == null && databaseValue != null || salesforceValue != null && salesforceValue != databaseValue)
            { changedValues.Add(name, databaseValue); changed = true; }
        }

        public static void setValue(int? salesforceValue, int? databaseValue, IDictionary<string, object> changedValues, string name, ref bool changed)
        {
            if (!salesforceValue.HasValue && databaseValue.HasValue || salesforceValue.HasValue && (!databaseValue.HasValue || salesforceValue.Value != databaseValue.Value))
            { changedValues.Add(name, databaseValue.Value); changed = true; }
        }

        public static void setValue(double? salesforceValue, double? databaseValue, IDictionary<string, object> changedValues, string name, ref bool changed)
        {
            if (!salesforceValue.HasValue && databaseValue.HasValue || salesforceValue.HasValue && (!databaseValue.HasValue || salesforceValue.Value != databaseValue.Value))
            { changedValues.Add(name, databaseValue.Value); changed = true; }
        }

        public static void setValue(DateTime? salesforceValue, DateTime? databaseValue, IDictionary<string, object> changedValues, string name, ref bool changed)
        {
            if (!salesforceValue.HasValue && databaseValue.HasValue || salesforceValue.HasValue && (!databaseValue.HasValue || salesforceValue.Value != databaseValue.Value))
            { changedValues.Add(name, !databaseValue.HasValue ? (DateTime?)null : databaseValue.Value); changed = true; }
        }

        public static void setValue(bool? salesforceValue, bool? databaseValue, IDictionary<string, object> changedValues, string name, ref bool changed)
        {
            if (!salesforceValue.HasValue && databaseValue.HasValue || salesforceValue.HasValue && (!databaseValue.HasValue || salesforceValue.Value != databaseValue.Value))
            { changedValues.Add(name, databaseValue.Value); changed = true; }
        }

        #endregion

        #region GetTypeValue

        public static bool? GetBoolean(object value)
        {
            return value is DBNull || value == null ? false : Convert.ToBoolean(value);
        }

        public static double? GetDouble(object value)
        {

            if (value == null || value is DBNull) return 0;

            return double.Parse(value.ToString());
            //return (value is double) ? (double)value
            //: (value is IConvertible) ? (value as IConvertible).ToDouble(null)
            //: double.Parse(value.ToString());

            //return Double.TryParse(value, out result) ? result : 0;

            //    || value.ToString() == "" ? 0 /*default(double?)*/ : Convert.ToDouble(value);
        }

        public static int? GetInt32(object value)
        {
            return value is DBNull || value == null || value.ToString() == "" ? 0 /*default(int?)*/ : Convert.ToInt32(value);
        }

        public static string GetDateTime(object value)
        {
            return value is DBNull || value == null ? null /*default(DateTime?)*/ : Convert.ToDateTime(value).ToString("s", System.Globalization.CultureInfo.InvariantCulture);
        }

        public static string GetDate(object value)
        {
            return value is DBNull || value == null ? null : Convert.ToDateTime(value).ToString("yyyy-MM-dd");
        }

        #endregion

    }
}
