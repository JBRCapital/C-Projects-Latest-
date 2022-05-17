using Newtonsoft.Json;
using Salesforce.Common.Models;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;

namespace APRCalculatorService
{
    /// <summary>
    /// Summary description for APRCalculatorEnquire
    /// </summary>
    public class APRCalculatorEnquire : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            Utils.AllowsCORS(context);

            if (
                !Utils.ContainsKey(context.Request.Form, "firstName") ||
                !Utils.ContainsKey(context.Request.Form, "lastName") ||
                !Utils.ContainsKey(context.Request.Form, "email") ||
                !Utils.ContainsKey(context.Request.Form, "phone") ||
                !Utils.ContainsKey(context.Request.Form, "vehicleMake") ||

                //Inputs
                !Utils.ContainsKey(context.Request.Form, "carType") ||
                !Utils.ContainsKey(context.Request.Form, "costOfVehicle") ||
                !Utils.ContainsKey(context.Request.Form, "deposit") ||
                !Utils.ContainsKey(context.Request.Form, "term") ||
                !Utils.ContainsKey(context.Request.Form, "balloon") ||
                !Utils.ContainsKey(context.Request.Form, "creditProfile") ||

                //Outputs
                !Utils.ContainsKey(context.Request.Form, "monthlyRepayment") ||
                !Utils.ContainsKey(context.Request.Form, "cashPrice") ||
                !Utils.ContainsKey(context.Request.Form, "deposit") ||
                !Utils.ContainsKey(context.Request.Form, "totalAmountOfCredit") ||
                !Utils.ContainsKey(context.Request.Form, "totalChargeForCredit") ||
                //!Utils.ContainsKey(context.Request.Form, "totalAdminFees") ||
                !Utils.ContainsKey(context.Request.Form, "durationOfAgreement") ||
                !Utils.ContainsKey(context.Request.Form, "flatRate") ||
                !Utils.ContainsKey(context.Request.Form, "monthlyFlatRate") ||
                !Utils.ContainsKey(context.Request.Form, "representativeAPR") ||
                !Utils.ContainsKey(context.Request.Form, "totalAmountPayable")

            //!Utils.ContainsKey(context.Request.Form, "vehicleModel") ||
            //!Utils.ContainsKey(context.Request.Form, "notes")
            )
            {
                context.Response.Status = "500 Bad Input";
                context.Response.StatusCode = 500;
                context.ApplicationInstance.CompleteRequest();
            }


            //HttpCookie myCookie = new HttpCookie("utm");
            //var utmCookie = context.Request.Cookies["utm"];
            //var utm = string.Empty;
            //if (utmCookie != null) { utm = utmCookie.Value; }

            var notes =
            "Inputs" + Environment.NewLine +
            "------" + Environment.NewLine +
            "Car Type: " + context.Request.Form["carType"] + Environment.NewLine +
            "Cost Of Vehicle: " + context.Request.Form["costOfVehicle"] + Environment.NewLine +
            "Deposit: " + context.Request.Form["deposit"] + Environment.NewLine +
            "Term: " + context.Request.Form["term"] + Environment.NewLine +
            "Balloon: " + context.Request.Form["balloon"] + Environment.NewLine +
            "Credit Profile: " + context.Request.Form["creditProfile"] + Environment.NewLine +
            Environment.NewLine +
            Environment.NewLine +
            "Outputs:" + Environment.NewLine +
            "------" + Environment.NewLine +
            "Monthly Repayment: " + context.Request.Form["monthlyRepayment"] + Environment.NewLine +
            "Cash Price: " + context.Request.Form["cashPrice"] + Environment.NewLine +
            "Deposit: " + context.Request.Form["deposit"] + Environment.NewLine +
            "Total Amount Of Credit: " + context.Request.Form["totalAmountOfCredit"] + Environment.NewLine +
            "Total Charge For Credit: " + context.Request.Form["totalChargeForCredit"] + Environment.NewLine +
            "Total Admin Fees: " + context.Request.Form["totalAdminFees"] + Environment.NewLine +
            "Duration Of Agreement: " + context.Request.Form["durationOfAgreement"] + Environment.NewLine +
            "Flat Rate: " + context.Request.Form["flatRate"] + Environment.NewLine +
            "Monthly Flat Rate: " + context.Request.Form["monthlyFlatRate"] + Environment.NewLine +
            "Representative APR: " + context.Request.Form["representativeAPR"] + Environment.NewLine +
            "Total Amount Payable: " + context.Request.Form["totalAmountPayable"] + Environment.NewLine +
            Environment.NewLine +
            Environment.NewLine +
            "Notes:" + Environment.NewLine +
            "------" + Environment.NewLine +
            context.Request.Form["notes"];

            var RecordType = new { Name = "End User Prospect" };
        
        var leadDataForSF = new
            {
                FirstName = context.Request.Form["firstName"].ToString(),
                LastName = context.Request.Form["lastName"].ToString(),
                //Name = context.Request.Form["firstName"].ToString() + " " + context.Request.Form["lastName"].ToString(),
                Phone = context.Request.Form["phone"].ToString(),
                Email = context.Request.Form["email"].ToString(),

                RecordType = RecordType,

                Company = "None",
                
                vehicleMake__c = context.Request.Form["vehicleMake"].ToString(),
                vehicleModelAndOtherInfo__c = context.Request.Form["vehicleModel"].ToString(),
                vehicleValue__c = context.Request.Form["cashPrice"],
                dateOfEnquiryVisit__c = DateTime.Now,
                Enquiry_Method__c = "Finance Calculator",
                enquiryURL__c = HttpContext.Current.Request.Url.AbsoluteUri,

                advanceRequested__c = context.Request.Form["totalAmountOfCredit"],

                Notes__c = notes,

                LeadSource = context.Request.Form["campaignDetailsForOriginalVisit"],
                arrivedFromURL__c = context.Request.Form["arrivedFromURL"],
                Arrived_from_URL_Session__c = context.Request.Form["Arrived_from_URL_Session"],
                campaignDetailsForEnquiryVisit__c = context.Request.Form["campaignDetailsForEnquiryVisit"],
                campaignDetailsForOriginalVisit__c = context.Request.Form["campaignDetailsForOriginalVisit"],
                gclid_original__c = context.Request.Form["gclid_original"],
                gclid_session__c = context.Request.Form["gclid_session"],
                Landing_Page_Original__c = context.Request.Form["Landing_Page_Original"],
                Landing_Page_Session__c = context.Request.Form["Landing_Page_Session"],
                Sub_Channel_Enquiry__c = context.Request.Form["Sub_Channel_Enquiry"],
                Sub_Channel_Original__c = context.Request.Form["Sub_Channel_Original"],
                trafficSourceForEnquiryVisit__c = context.Request.Form["trafficSourceForEnquiryVisit"],
                trafficSourceForOriginalVisit__c = context.Request.Form["trafficSourceForOriginalVisit"]
            };


            Salesforce.Force.ForceClient salesforceClient = null;
            SuccessResponse successResponse = null;

            Task.Run(async () => { salesforceClient = await DataHelper.GetSalesforceConnection(); }).Wait();
            Task.Run(async () =>
                    {
                        try {
                            successResponse = await salesforceClient.CreateAsync("Lead", leadDataForSF);
                        }
                        catch (Exception) {
                        }
                    }).Wait();
  

        context.Response.ContentType = "application/json";
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            context.Response.Write(JsonConvert.SerializeObject(new
            {
                submitted = (successResponse == null ? false : successResponse.Success)
            }, Formatting.Indented));
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}