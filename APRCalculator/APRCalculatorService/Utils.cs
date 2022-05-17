using System;
using System.Collections.Specialized;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Linq;

namespace APRCalculatorService
{
    public class Utils
    {
        public static void AllowsCORS(HttpContext context)
        {
            HttpClient client = new HttpClient();
            //client.BaseAddress = new Uri();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Properties.Resources.Base64UserNameAndPassword); //  "MTgvSkJSYXBpOmRvd2pvbmVz"


            if (context.Request.UrlReferrer != null && context.Request.UrlReferrer.GetLeftPart(UriPartial.Authority) != null)
            {
                context.Response.Headers.Remove("Access-Control-Allow-Origin");
                context.Response.AddHeader("Access-Control-Allow-Origin", context.Request.UrlReferrer.GetLeftPart(UriPartial.Authority));
            }

            context.Response.Headers.Remove("Access-Control-Allow-Credentials");
            context.Response.AddHeader("Access-Control-Allow-Credentials", "true");

            context.Response.Headers.Remove("Access-Control-Allow-Methods");
            context.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST");
        }

        public static bool ContainsKey(NameValueCollection collection, string key)
        {
            if (collection.Get(key) == null)
            {
                return collection.AllKeys.Contains(key);
            }

            return true;
        }

    }
}