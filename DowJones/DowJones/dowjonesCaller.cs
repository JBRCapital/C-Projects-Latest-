using System;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace DowJones
{
    public static class DowjonesCaller
    {
        public static async Task CallDowJones(HttpContext context, string url, string restfulParamName = "")
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
            context.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS");

            //client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //name = medvedev & record - type = P & search - type = precise & exclude - deceased = true & hits - from = 0 & hits - to = 4

            try
            {
                HttpResponseMessage response = await client.GetAsync(string.Concat(url, getResfulParameter(context, restfulParamName),"?", convertFormToURLString(context, restfulParamName)));

                string s = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(s);

                    string json = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.Indented, false);
                    context.Response.ContentType = "application/json";
                    context.Response.Write(json);
                }
                else
                {
                    context.Response.Write(s);
                }

            }
            catch (Exception e)
            {
                context.Response.Write(e.StackTrace + " - " + e.Message);
            }
        }

        public static string getResfulParameter(HttpContext context, string restfulParamName) {
            
            if (restfulParamName == null) return string.Empty;
            if (!context.Request.Form.AllKeys.Contains(restfulParamName)) return string.Empty;

            return HttpUtility.UrlEncode(context.Request.Form[restfulParamName]);
        }

        public static string convertFormToURLString(HttpContext context, string restfulParamName)
        {
            string urlString = string.Empty;

            List<string> listValues = new List<string>();
            foreach (string key in context.Request.Form.AllKeys)
            {
                if (key != restfulParamName)
                {
                    urlString += string.Concat(key, "=", HttpUtility.UrlEncode(context.Request.Form[key]));
                }
            }

            return urlString;
        }

        private static bool ContainsKey(NameValueCollection collection, string key)
        {
            if (collection.Get(key) == null)
            {
                return collection.AllKeys.Contains(key);
            }

            return true;
        }

        }
}