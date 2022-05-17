using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Configuration;

namespace CallCreditApiDelegation.Helpers
{
    /// <summary>
    /// recaptcha helper, used to encapsulate all the logic releated to
    /// the recaptcha verification, this class uses the api secret key
    /// from the web config file
    /// </summary>
    static class RecaptchaHelper
    {
        /// <summary>
        /// validate recaptcha response with the server to make sure
        /// the client isn't spoofing the response, changing the public key
        /// of the recaptcha here or in the client, require changing
        /// the other party for both to match
        /// </summary>
        /// <param name="gRecaptchaResponse">the client recaptcha challange response</param>
        /// <returns>returns the result of the recaptcha challange response server validation</returns>
        public static bool ReCaptchaPassed(string gRecaptchaResponse)
        {
            HttpClient httpClient = new HttpClient();
            var res = httpClient.GetAsync($"https://www.google.com/recaptcha/api/siteverify?secret={WebConfigurationManager.AppSettings["recaptchasecret"]}&response={gRecaptchaResponse}").Result;
            if (res.StatusCode != HttpStatusCode.OK) return false;
            string JSONres = res.Content.ReadAsStringAsync().Result;
            dynamic JSONdata = JObject.Parse(JSONres);
            if (JSONdata.success != "true") return false;
            return true;
        }
    }
}