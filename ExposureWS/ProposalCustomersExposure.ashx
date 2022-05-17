<% @ webhandler language="C#" class="ProposalCustomersExposure" %> 

using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Script.Serialization;

/// <summary>
/// Summary description for ProposalCustomersExposure
/// </summary>
public class ProposalCustomersExposure : IHttpHandler
{

public void ProcessRequest(HttpContext context)
{
    var proposalId = context.Request["proposalId"];
    //proposalId = "222";
    string response;

    context.Response.Headers.Add("Content-type", "text/json");
    context.Response.Headers.Add("Content-type", "application/json");

    using (SqlConnection cn = new SqlConnection(ConfigurationManager.ConnectionStrings["LocalSqlServer"].ToString()))
    {
        SqlCommand cmd = new SqlCommand("GetProposalCustomersExposure", cn);
        cmd.CommandType = CommandType.StoredProcedure;
        cmd.Parameters.AddWithValue("proposalId", proposalId);
        cn.Open();

        using (SqlDataReader dr = cmd.ExecuteReader())
        {
            var tb = new DataTable(); tb.Load(dr);
            response = DataTableToJSON(tb);
        }

        cn.Close();
    }

    context.Response.Write(response);
}


public static string DataTableToJSON(DataTable table)
{
    var list = new List<Dictionary<string, object>>();

    foreach (DataRow row in table.Rows)
    {
        var dict = new Dictionary<string, object>();

        foreach (DataColumn col in table.Columns)
        {
            dict[col.ColumnName] = row[col];
        }
        list.Add(dict);
    }
    JavaScriptSerializer serializer = new JavaScriptSerializer();
    return serializer.Serialize(list);
}

public bool IsReusable { get { return false; } }

}
