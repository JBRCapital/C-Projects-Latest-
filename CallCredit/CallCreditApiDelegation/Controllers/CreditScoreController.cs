using CallCreditApiDelegation.Helpers;
using CallCreditApiDelegation.Migrations;
using CallCreditApiDelegation.Models;
using CallCreditWrapper;
using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Configuration;
using System.Web.Http;
using System.Web.Http.Cors;

namespace CallCreditApiDelegation.Controllers
{
    /// <summary>
    /// the main controller for the api, used to test or consume
    /// the actual api using the appropriate data
    /// </summary>
    public class CreditScoreController : ApiController
    {
        /// <summary>
        /// the main call for the wrapper api, used to
        /// parse all the UI form's data and the recaptcha's
        /// result string to validate both of them and call
        /// the call credit api to retrive back the credit score
        /// </summary>
        /// <param name="model">the UI's view model</param>
        /// <returns>return ok/bad request on success/fail, on success it returns the credit score</returns>
        [HttpPost]
        public IHttpActionResult Post(CallCreditModel model) //FormDataCollection formData)//
        {

            //try
            //{
            //CallCreditModel model = null;
            if (!AccessHelper.CheckAccessIsAllowed(Request, ModelState, model))
            {
                return BadRequest("Something went wrong! bad request!");
            }

            //initializing the api's wrapper using the credientials in the web config
            //creating the main function of the api's wrapper with the UI information
            using (var apicontext = new ApiContext())
            {
                var currentPassword = apicontext.CallCreditInformation.First().Password;
                var creditScoreResult = new CallCredit(
                WebConfigurationManager.AppSettings["CC_CompanyName"],
                WebConfigurationManager.AppSettings["CC_Username"],
                currentPassword).Search(model);


                //check if the request succeeded or failed, returns ok/badrequest with the score/error
                if (creditScoreResult.Succeeded)
                {
                    //saving information to sales force
                    Helpers.SalesForceHelper.SaveInformationOnSalesForce(model, creditScoreResult);

                    //send the email
                    Helpers.EmailHelper.SendScoreEmail(model, creditScoreResult);

                    return Content(HttpStatusCode.OK, new { creditScoreResult.creditScoreCategory, creditScoreResult.creditScoreText } );
                }
                else
                {
                    return Content(HttpStatusCode.BadRequest, new { creditScoreResult.Error });
                }
            }
            //if data check fails, the recaptcha check fails or the referrer wasn't allowed
            //it return this catch all client has protection against submitting bad data 
            //or not challenging recaptcha but it's there mainly for bots and what not
            //}
            //catch (Exception e)
            //{
            //    File.WriteAllText($"{DateTime.Now.Ticks}.txt", e.ToString());
            //    return BadRequest(e.ToString());
            //}
        }

        [HttpGet]
        public IHttpActionResult ViewLimitationForDevelopingPurposes()
        {
            var result = new LimitationForDevelopingPurposesModel()
            {
                Today = DateTime.Now,
                ClientsCounts = new List<ClientsCountsForDevelopingPurposesModel>()
            };
            using (var apicontext = new ApiContext())
            {
                result.NumberOfRequestsToday = apicontext.dailyMonitorReports
                .SingleOrDefault(r => r.RequestDate.Day == DateTime.Now.Day)?.DailyRequestsCounter ?? 0;
                var usersToday = apicontext.iPMonitorEntries
                    .Where(entry => entry.RequestDate.Day == DateTime.Now.Day).DistinctBy(User => User.IP).ToList();
                foreach (var user in usersToday)
                {
                    result.ClientsCounts.Add(new ClientsCountsForDevelopingPurposesModel()
                    {
                        ClientIP = user.IP,
                        NumberOfRequestsToday = apicontext.iPMonitorEntries
                            .Count(entries => entries.IP == user.IP && entries.RequestDate.Day == DateTime.Now.Day)
                    });
                }
            }
            return Ok(result);
        }

        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage Getimg(int score)
        {
            return HTTPHelpers.getImageHttpResponseMessage(
                Convert.FromBase64String(SerializationServices.ReadFromBinaryFile<List<KeyValuePair<int, string>>>
                (@"C:\JBR\CallCreditEmailTemplates\test.dat")[score].Value));
        }

        [HttpGet]
        [EnableCors(origins: "*", headers: "*", methods: "*")]
        public HttpResponseMessage logo()
        {
            return HTTPHelpers.getImageHttpResponseMessage(Convert.FromBase64String(WebConfigurationManager.AppSettings["logo"]));
        }

        [HttpGet]
        public string setup()
        {
            new ApiContext().Database.CreateIfNotExists();
            return "done";
        }

        [HttpGet]
        public string migrate()
        {
            var configuration = new Configuration();
            var migrator = new DbMigrator(configuration);
            migrator.Update();
            return "done";
        }
    }
}
