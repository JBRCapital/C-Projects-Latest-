using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace SQLDataHelper
{
    public class Helper
    {

        private static SqlConnection sqlConn;
        public static SqlConnection GetSqlConnection(string ConnectionString)
        {
                if (sqlConn == null) { sqlConn = new SqlConnection(ConnectionString); sqlConn.Open(); }
                return sqlConn;
        }

        public static void RunCommand(string commmandText, CommandType commandType, List<SqlParameter> sqlParameterCollection)
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.CommandText = commmandText;
                cmd.CommandType = commandType;

                if (sqlParameterCollection != null)
                foreach (var parameter in sqlParameterCollection)
                {
                    cmd.Parameters.Add(parameter);
                }

                cmd.Connection = sqlConn;
                cmd.ExecuteNonQuery();
            }
        }

        #region GetTypeValue

        public static Boolean? GetBoolean(object value)
        {
            return value is DBNull || value == null ? false : Convert.ToBoolean(value);
        }

        public static string GetStringMaxLength(string value, int maxlength)
        {
            if (value is null) return value;
            return value.Substring(0, Math.Min(value.Length, maxlength));
        }

        public static double? GetDouble(object value)
        {

            if (value == null || value is DBNull) return 0;

            return Double.Parse(value.ToString());
            //return (value is double) ? (double)value
            //: (value is IConvertible) ? (value as IConvertible).ToDouble(null)
            //: double.Parse(value.ToString());

            //return Double.TryParse(value, out result) ? result : 0;

            //    || value.ToString() == "" ? 0 /*default(double?)*/ : Convert.ToDouble(value);
        }

        public static Int32? GetInt32(object value)
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
