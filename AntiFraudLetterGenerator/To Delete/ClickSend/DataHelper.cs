using System.Threading.Tasks;
using System.Configuration;
using Salesforce.Force;
using Salesforce.Common;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ClickSend
{
    class DataHelper
    {
        public static string SecurityToken = ConfigurationManager.AppSettings["SecurityToken"];
        public static string ConsumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
        public static string ConsumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
        public static string Username = ConfigurationManager.AppSettings["Username"];
        public static string Password = ConfigurationManager.AppSettings["Password"];

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

        internal static List<string> TakeLastLines(string text, int count)
        {
            List<string> lines = new List<string>();
            Match match = Regex.Match(text, "^.*$", RegexOptions.Multiline | RegexOptions.RightToLeft);

            while (match.Success && lines.Count < count)
            {
                lines.Insert(0, match.Value);
                match = match.NextMatch();
            }

            return lines;
        }
    }
}
