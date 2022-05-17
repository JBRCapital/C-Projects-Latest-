using System.Configuration;
using SalesforceDataLibrary;

namespace AntiFraudLetterGenerator
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
                null,
                null);
            }

            //SQL ---------------------------------------------------------------------------------------------

            //public static string connectionString = ConfigurationManager.AppSettings["SQLConnectionString"];

            //End of SQL -----------------------------------------------------------------------------------------

        }
    
}
