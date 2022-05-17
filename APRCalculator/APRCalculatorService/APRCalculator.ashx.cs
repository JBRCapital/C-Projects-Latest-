using System.Web;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using APRCalculator;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace APRCalculatorService
{
    /// <summary>
    /// Summary description for APRCalculator
    /// </summary>
    public class APRCalculatorService : IHttpHandler
    {

        private decimal GetTargetAPRFromLookup(string carType, string creditProfile)
        {
            return decimal.Parse(Properties.Resources.ResourceManager.GetString("APR" + carType + creditProfile));
        }

        public void ProcessRequest(HttpContext context)
        {
            Utils.AllowsCORS(context);

            bool hastargetAPR = Utils.ContainsKey(context.Request.Form, "targetAPR");
            bool hasCarTypeAndCreditProfile = Utils.ContainsKey(context.Request.Form, "carType") && Utils.ContainsKey(context.Request.Form, "creditProfile");

            if (
                !Utils.ContainsKey(context.Request.Form, "costOfVehicle") ||
                !Utils.ContainsKey(context.Request.Form, "deposit") ||
                !Utils.ContainsKey(context.Request.Form, "term") ||
                !Utils.ContainsKey(context.Request.Form, "balloon") ||
                (!hastargetAPR && !hasCarTypeAndCreditProfile) ||
                !APRCalculator.Utils.Utils.IsNumeric(context.Request.Form["costOfVehicle"]) ||
                !APRCalculator.Utils.Utils.IsNumeric(context.Request.Form["deposit"]) ||
                !APRCalculator.Utils.Utils.IsNumeric(context.Request.Form["term"]) ||
                !APRCalculator.Utils.Utils.IsNumeric(context.Request.Form["balloon"]) ||
                (hastargetAPR && !APRCalculator.Utils.Utils.IsNumeric(context.Request.Form["targetAPR"]))
                )
            {
                context.Response.Status = "500 Bad Input";
                context.Response.StatusCode = 500;
                context.ApplicationInstance.CompleteRequest();
            }

            var TargetAPR = hastargetAPR ? decimal.Parse(context.Request.Form["targetAPR"]) : GetTargetAPRFromLookup(context.Request.Form["carType"], context.Request.Form["creditProfile"]);

            bool includeVATOnYearlyAdminFee = Properties.Resources.IncludeVATOnYearlyAdminFee == "Yes";

            context.Response.ContentType = "application/json";
            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            var APRFinders = new List<Task<APRFinder>>();
            APRFinders.Add(GetAPRFinder(context, TargetAPR, true));
            APRFinders.Add(GetAPRFinder(context, TargetAPR, false));

            Task t = Task.WhenAll(APRFinders);
            try
            {
                t.Wait();
            }
            catch { }
            
            //if (t.Status == TaskStatus.RanToCompletion)
            //    Console.WriteLine("All ping attempts succeeded.");
            //else if (t.Status == TaskStatus.Faulted)
            //    Console.WriteLine("{0} ping attempts failed", failed);

            //APRCalculator.APRFinder APRFinder = GetNewAPRFinder(context, TargetAPR, true);
            //APRCalculator.APRFinder APRFinderWithoutBalloon = GetNewAPRFinder(context, TargetAPR, false);

            //APRFinder.Setup(null);
            //var monthlyRepayment = APRFinder.calculateMonthlyPayment();

            //APRFinderWithoutBalloon.Setup(monthlyRepayment - 2);
            //APRFinderWithoutBalloon.calculateMonthlyPayment();

            var APRFinderWithBalloon = APRFinders[0].Result;
            var APRFinderWithoutBalloon = APRFinders[1].Result;

            var cashPrice = APRFinderWithBalloon.CostOfVehicle;
            var deposit = APRFinderWithBalloon.Deposit;
            var totalAmountOfCredit = APRFinderWithBalloon.Advance;

            var totalAdminFees = APRFinderWithBalloon.TotalYearlyAdminFee + APRFinderWithBalloon.DocFee + APRFinderWithBalloon.OptionToPurchaseFeeWithVAT;
            var totalChargeForCredit = APRFinderWithBalloon.TotalMonthlyRepayment + (double)totalAdminFees + APRFinderWithBalloon.Balloon - APRFinderWithBalloon.Advance;
            
            var durationOfAgreement = APRFinderWithBalloon.Term + " Months";
            var flatRate = (((APRFinderWithoutBalloon.TotalMonthlyRepayment - APRFinderWithoutBalloon.Advance) / APRFinderWithoutBalloon.Advance) / (APRFinderWithoutBalloon.Term / 12))*100;
            var monthlyFlatRate = flatRate / 12;
            var representativeAPR = APRFinderWithBalloon.TargetAPR;
            var totalAmountPayable = APRFinderWithBalloon.Deposit + APRFinderWithBalloon.TotalMonthlyRepayment + APRFinderWithBalloon.Balloon + APRFinderWithBalloon.TotalYearlyAdminFee + APRFinderWithBalloon.DocFee + (double)APRFinderWithBalloon.OptionToPurchaseFeeWithVAT;
                
            context.Response.Write(JsonConvert.SerializeObject(new
            {
                totalMonthlyRepayment = APRFinderWithBalloon.TotalMonthlyRepayment,
                totalYearlyAdminFee = APRFinderWithBalloon.TotalYearlyAdminFee,
                docFee = APRFinderWithBalloon.DocFee,
                optionToPurchaseFee = APRFinderWithBalloon.OptionToPurchaseFeeWithVAT,
                advance = APRFinderWithBalloon.Advance,
                startPosition = APRFinderWithBalloon.StartPosition, 
                //Excel.FinancialFunctions.Financial.Rate(APRFinderWithBalloon.Term, (((APRFinderWithBalloon.Advance - APRFinderWithBalloon.Balloon) / APRFinderWithBalloon.Term + ((double)(APRFinderWithBalloon.TargetAPR/2)) / 12 * (APRFinderWithBalloon.Advance - APRFinderWithBalloon.Balloon))), -(APRFinderWithBalloon.Advance - APRFinderWithBalloon.Balloon),0, Excel.FinancialFunctions.PaymentDue.EndOfPeriod)*12,
                //startPosition = ((APRFinderWithBalloon.Advance - APRFinderWithBalloon.Balloon)) / (System.Math.Pow(1.08, APRFinderWithBalloon.Term)/ APRFinderWithBalloon.Term),

                APRReturned = APRFinderWithBalloon.APRReturned,
                //monthlyRepaymentGuesses = APRFinder.MonthlyRepaymentGuesses.Count,
                balloon = APRFinderWithBalloon.Balloon,
                monthlyRepayment = string.Format("£{0:n0}", APRFinderWithBalloon.MonthlyRepayment),
                cashPrice = string.Format("£{0:n0}", cashPrice),
                deposit = string.Format("£{0:n0}", deposit),
                totalAmountOfCredit = string.Format("£{0:n0}", totalAmountOfCredit),
                totalChargeForCredit = string.Format("£{0:n0}", totalChargeForCredit),
                //totalAdminFees = string.Format("£{0:n0}", totalAdminFees),
                totalAmountPayable = string.Format("£{0:n0}", totalAmountPayable),
                durationOfAgreement = durationOfAgreement,
                flatRate = string.Format("{0:n2}%", flatRate),
                monthlyFlatRate = string.Format("{0:n2}%", monthlyFlatRate),
                representativeAPR = string.Format("{0:n1}%", representativeAPR),
                //payProfile = APRCalculator.APRFinder.ConvertToMonths(APRFinder.Calculator.CashFlows)
            }, Formatting.Indented));
        }


        public static async Task<APRFinder> GetAPRFinder(HttpContext context, decimal TargetAPR, bool includeBalloon)
        {
            var APRFinder = new APRFinder()
            {
                CostOfVehicle = int.Parse(context.Request.Form["costOfVehicle"]),
                Deposit = int.Parse(context.Request.Form["deposit"]),
                Term = int.Parse(context.Request.Form["term"]),
                Balloon = includeBalloon ? int.Parse(context.Request.Form["balloon"]) : 0,
                TargetAPR = TargetAPR,
                IncludeYearlyAdminFee = Properties.Resources.IncludeYearlyAdminFee == "Yes",
                YearlyAdminFee = int.Parse(Properties.Resources.YearlyAdminFee),

                IncludeVAT = Properties.Resources.IncludeVATOnYearlyAdminFee == "Yes",
                VAT = int.Parse(Properties.Resources.VATPercent),

                DocFee = int.Parse(Properties.Resources.DocFee),
                OptionToPurchaseFee = int.Parse(Properties.Resources.OptionToPurchaseFee)
            };

            APRFinder.Setup(null);
            //APRFinder.StartPosition = 700m;

            return await Task.FromResult<APRFinder>(APRFinder.calculateMonthlyPayment());
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