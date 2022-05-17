using ClicksendHelper;
using System.Data.SqlClient;

namespace AutoDocHelper
{
    public class Sendclick
    {
        public static ClickSendValues getSendClickValues(SqlDataReader reader)
        {
            return new ClickSendValues
            {
                address_name = StringFunctions.getLeft(reader["CustomerShortName"].ToString(), 50),
                address_line_1 = StringFunctions.getLeft(reader["CustomerAddress1"].ToString(), 50),
                address_line_2 = StringFunctions.getLeft(reader["CustomerAddress2"].ToString(), 50),
                address_city = StringFunctions.getLeft(reader["CustomerAddress3"].ToString(), 30),
                address_state = StringFunctions.getLeft(reader["CustomerAddress4"].ToString(), 30),
                address_postal_code = StringFunctions.getLeft(reader["CustomerPostCode"].ToString(), 10),
                address_country = "GB",

                colour = "1",
                duplex = "1",
                priority_post = "0" //1 = first class, 0 = second class
            };
        }

    }
}
