using System.Configuration;
using System.Data.SqlClient;

namespace UpdateSalesforceData
{
    internal class SQLDataConnectionHelper
    {

        internal static SqlConnection SqlConnection
        {
            get => SQLDataHelper.Helper.GetSqlConnection(ConfigurationManager.AppSettings["SQLConnectionString"]);
        }

    }
}
