using CallCreditApiDelegation.Models;
using CallCreditWrapper;
using System;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Web.Configuration;
using System.Web.Http.ModelBinding;

namespace CallCreditApiDelegation.Helpers
{
    public class AccessHelper
    {
        public static Boolean CheckAccessIsAllowed(HttpRequestMessage request, ModelStateDictionary modelState, CallCreditModel model)
        {
            //getting the referrer host, while testing, it's "callcredittest.tk"
            //and check if it matches the allowed referrer in the web config
            if (request.Headers.Referrer.Host != WebConfigurationManager.AppSettings["allowedReferrer"])
                return false;

            //used to validate the view model attributes, e.x. required, length, datatype, etc..
            if (!modelState.IsValid) return false;

            //used to verify the recaptcha response sent by the client after the user
            //do the recaptcha challange and submit the form
            if (!RecaptchaHelper.ReCaptchaPassed(model.gRecaptchaResponse)) return false;

            //Check the daily limits
            if (!CheckDailyLimits()) { return false; }

            return true;
        }

        private static bool CheckDailyLimits()
        {
            //initializing db instance
            var apicontext = new ApiContext();
            var clientIP = IPHelper.GetIPAddress();
            var todaysDate = DateTime.Now.Date;

            #region Check Daily Limits

            //number of requests at the same day for all clients
            var totalRequestsToday = apicontext.dailyMonitorReports.
                FirstOrDefault(r => DbFunctions.TruncateTime(r.RequestDate) == todaysDate);
            //limiting daily requests to 200
            if (totalRequestsToday != null &&
                totalRequestsToday.DailyRequestsCounter > int.Parse(WebConfigurationManager.AppSettings["maxDailyTotalRequests"]))
                return false;

            //number of requests at the same day for this client
            var totalClientRequestsToday = apicontext.iPMonitorEntries.
                FirstOrDefault(r => r.IP == clientIP && DbFunctions.TruncateTime(r.RequestDate) == todaysDate);
            //limiting client requests to 10 per day
            if (totalClientRequestsToday != null &&
                totalClientRequestsToday.DailyRequestsCounter > int.Parse(WebConfigurationManager.AppSettings["maxDailyClientRequests"]))
                return false;

            #endregion

            #region Check IP Limits

            //create/add the request to the daily report count
            (totalRequestsToday ?? apicontext.dailyMonitorReports.Add(new DailyMonitorReport() //if report doesn't exist (first report of the day)
            {
                RequestDate = todaysDate,
                DailyRequestsCounter = 0
            })).DailyRequestsCounter++;

            //create/add the request to the daily report count
            (totalClientRequestsToday ?? apicontext.iPMonitorEntries.Add(new IPMonitorEntry() //if report doesn't exist (first report of the day)
            {
                IP = clientIP,
                RequestDate = todaysDate,
                DailyRequestsCounter = 0
            })).DailyRequestsCounter++;  //add the request to the daily report counts

            #endregion

            //request is added toward the limit even if the api call failed or didn't find the score
            apicontext.SaveChanges();

            return true;
        }

    }
}