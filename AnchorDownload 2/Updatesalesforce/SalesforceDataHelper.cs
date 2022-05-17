using System.Configuration;
using SalesforceDataLibrary;


namespace UpdateSalesforceData
{
    public class SalesforceDataHelper
    {
        public static SalesforceClient SalesforceClient
        {
            get => SalesforceClient.GetSalesforceConnection(new SalesforceClient.SalesforceSettings
            {
                Domain = ConfigurationManager.AppSettings["Domain"],
                ConsumerKey = ConfigurationManager.AppSettings["ConsumerKey"],
                ConsumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"],
                Username = ConfigurationManager.AppSettings["Username"],
                Password = ConfigurationManager.AppSettings["Password"],
                SecurityToken = ConfigurationManager.AppSettings["SecurityToken"]

            },
            Program.EmailTransactionLog,
            Program.EmailErrorLog);
        }

    }
}
