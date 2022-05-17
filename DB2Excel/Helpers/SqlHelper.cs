using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using DB2Excel.DAL.Enum;
using DB2Excel.DAL.Obj;

namespace DB2Excel.Helpers
{
    public class SqlHelper
    {
        public static DataTable RunOperation(Operation op)
        {
            var cmd = string.Empty;
            switch (op.OperationType)
            {
                case OperationType.SPROC:
                    cmd = $"EXEC {op.Target} {(op.Params == null ? string.Empty : (string.Join(", ", op.Params)))}";
                    break;
                case OperationType.TABLE:
                    cmd = $"SELECT * FROM {op.Target}";
                    break;
                case OperationType.FUNCTION:
                    cmd = $"SELECT * FROM {op.Target}()";
                    break;
                default:
                    throw new NotImplementedException();
            }

            using var SQLConnection =
                new SqlConnection(ConfigurationManager.ConnectionStrings["localadv"].ConnectionString);
            var adapter = new SqlDataAdapter(cmd, SQLConnection);
            adapter.SelectCommand.CommandTimeout = 0;
            var ds = new DataSet();
            adapter.Fill(ds);
            if (ds.Tables.Count == 0)
                return null;
            ds.Tables[0].TableName = op.TabName;
            return ds.Tables[0];
        }
    }
}
