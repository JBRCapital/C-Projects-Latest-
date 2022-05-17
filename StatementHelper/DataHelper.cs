using System;
using System.Data;
using System.Data.SqlClient;

namespace AutoDocHelper
{
    public class DataHelper
    {
        public static string connectionString = "Server=JBRDB01;Database=JBR_Internal;Trusted_Connection=True;";
        private static SqlConnection sqlConn = null;

        public static SqlConnection GetSingletonConnection()
        {
            if (sqlConn != null) return sqlConn;

            sqlConn = GetConnection();
            return sqlConn;
        }

        public static SqlConnection GetConnection()
        {
            var sqlConn = new SqlConnection(connectionString);
            sqlConn.Open();

            return sqlConn;
        }

        public static SqlDataReader GetFirstStatementDataFromSQL()
        {
            return new SqlCommand("SELECT * FROM [dbo].[FirstStatements_AgreementData_TOBEREPLACEDWITHBONAFIDEE]()" +
                                  " ORDER BY AgreementDate, AgreementNumber",
                                   GetConnection())
            { CommandTimeout = 0 }.ExecuteReader();
        }

        public static SqlDataReader GetAnnualStatementDataFromSQL()
        {
            return new SqlCommand("SELECT * FROM [dbo].[AnnualStatements_AgreementData]('"+ DateTime.Now.ToString("yyyy-MM") +"-01')" +
                                  " WHERE StatementType <> 'No Statement' AND StatementType <> 'First Statement' ORDER BY AgreementDate, AgreementNumber",
                                   GetConnection()) { CommandTimeout = 0 }.ExecuteReader();
        }

        public static DataTable GetTransactionsTable(string agreementNumberNo, string startDateTime)
        {
            SqlDataReader reader = new SqlCommand(string.Concat("SELECT * FROM [dbo].[AnnualStatments_Transactions]('", agreementNumberNo, "', '" + startDateTime + "')  ORDER BY OrderId"),
                                GetSingletonConnection()) { CommandTimeout = 0 }.ExecuteReader();

            if (!reader.HasRows) { reader.Close(); return null; }

            var dt = new DataTable("DetailsTable");
            dt.Columns.AddRange(new[]
            {
                new DataColumn("Date"),
                new DataColumn("Description"),
                new DataColumn("Debit £"),
                new DataColumn("Credit £"),
                new DataColumn("Balance £"),
            });

            while (reader.Read())
            {
                dt.Rows.Add(reader["Date"], reader["Description"], reader["Credit"], reader["Debit"], reader["Balance"]);
            }

            reader.Close();
            
            return dt;
        }

        public static DataTable GetPayProfileTable(string AgreementNumberNo, string startDateTime)
        {
            SqlDataReader reader = new SqlCommand(string.Concat("SELECT * FROM [dbo].[AnnualStatments_PayProfile]('", AgreementNumberNo, "', '" + startDateTime + "')  ORDER BY OrderId"),
                                GetSingletonConnection())
            { CommandTimeout = 0 }.ExecuteReader(); ;

            if (!reader.HasRows) { reader.Close(); return null; }

            var dt = new DataTable("ScheduleTable");
            dt.Columns.AddRange(new[] {
                new DataColumn("Payment Due Date"),
                new DataColumn("Payment Amount £")
            });

            while (reader.Read())
            {
                dt.Rows.Add(reader["Date"], reader["Amount"]);
            }

            reader.Close();

            return dt;
        }

        #region GetTypeValue

        public static Boolean? GetBoolean(object value)
        {
            return value is DBNull || value == null ? false : Convert.ToBoolean(value);
        }

        //public static double? GetDouble(object value)
        //{
        //    return value is DBNull || value == null || value.ToString() == "" ? 0 /*default(double?)*/ : Convert.ToDouble(value);
        //}

        //public static Int32? GetInt32(object value)
        //{
        //    return value is DBNull || value == null || value.ToString() == "" ? 0 /*default(int?)*/ : Convert.ToInt32(value);
        //}

        //public static DateTime? GetDateTime(object value)
        //{
        //    return value is DBNull || value == null ? default(DateTime?) : Convert.ToDateTime(value);
        //}

        #endregion

    }
}
