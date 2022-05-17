using APRCalculator;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace TestAPRCalculator
{
    class Program
    {

        public static async Task<APRFinder> GetAPRFinder(bool calcBalloon)
        {
            var APRFinder = new APRFinder()
            {
                CostOfVehicle = 3000000,
                Deposit = 300000,
                Term = 48,
                Balloon = (calcBalloon ? 135000 : 0),
                TargetAPR = 8.60m,
                IncludeYearlyAdminFee = true,
                YearlyAdminFee = 50,

                IncludeVAT = true,
                VAT = 20,

                DocFee = 295,
                OptionToPurchaseFee = 250
            };

            APRFinder.Setup(null);
            //APRFinder.StartPosition = 700m;
             
            return await Task.FromResult(APRFinder.calculateMonthlyPayment());
            //return monthlyRepayment;
        }

        public static async Task<APRFinder[]> GetAPRFinders()
        {
            var getUserTasks = new List<Task<APRFinder>>();
            getUserTasks.Add(GetAPRFinder(true));
            getUserTasks.Add(GetAPRFinder(false));
            //getUserTasks.Add(GetAPRFinder());
            //getUserTasks.Add(GetAPRFinder());

            return await Task.WhenAll(getUserTasks);
        }


        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();

            var APRFinders = await GetAPRFinders();

            //var APRFinder = new APRCalculator.APRFinder()
            //{
            //    CostOfVehicle = 3000000,
            //    Deposit = 300000,
            //    Term = 48,
            //    Balloon = 135000,
            //    TargetAPR = 8.60m,
            //    IncludeYearlyAdminFee = true,
            //    YearlyAdminFee = 50,

            //    IncludeVAT = true,
            //    VAT = 20,

            //    DocFee = 295,
            //    OptionToPurchaseFee = 250
            //};

            //APRFinder.Setup(null);
            ////APRFinder.StartPosition = 700m;
            //var monthlyRepayment = APRFinder.calculateMonthlyPayment();

            //var APRFinderWithoutBaloon = new APRCalculator.APRFinder()
            //{
            //    CostOfVehicle = APRFinder.CostOfVehicle,
            //    Deposit = APRFinder.Deposit,
            //    Term = APRFinder.Term,
            //    //Balloon = 0,
            //    TargetAPR = APRFinder.TargetAPR,
            //    IncludeYearlyAdminFee = APRFinder.IncludeYearlyAdminFee,
            //    YearlyAdminFee = APRFinder.YearlyAdminFee,

            //    IncludeVAT = APRFinder.IncludeVAT,
            //    VAT = APRFinder.VAT,

            //    DocFee = APRFinder.DocFee,
            //    OptionToPurchaseFee = APRFinder.OptionToPurchaseFee,

            //    //StartPosition = monthlyRepayment

            //    // MonthlyRepayment = monthlyRepayment
            //};

            //APRFinderWithoutBaloon.Setup(monthlyRepayment);


            //APRFinderWithoutBaloon.calculateMonthlyPayment();

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            //var cashPrice = APRFinder.CostOfVehicle;
            //var deposit = APRFinder.Deposit;
            //var totalAmountOfCredit = APRFinder.Advance;
            //var totalChargeForCredit = APRFinder.TotalMonthlyRepayment + APRFinder.TotalYearlyAdminFee + APRFinder.DocFee + APRFinder.OptionToPurchaseFee - APRFinder.Advance;
            //var totalAdminFees = APRFinder.TotalYearlyAdminFee + APRFinder.DocFee + APRFinder.OptionToPurchaseFee;
            //var durationOfAgreement = APRFinder.Term + " Months";
            //var flatRate = ((APRFinderWithoutBaloon.TotalMonthlyRepayment - APRFinderWithoutBaloon.Advance) / APRFinderWithoutBaloon.Advance) / (APRFinderWithoutBaloon.Term / 12);
            //var monthlyFlatRate = flatRate / 12;
            //var representativeAPR = APRFinder.TargetAPR;
            //var totalAmountPayable = APRFinder.TotalMonthlyRepayment + APRFinder.Balloon + APRFinder.TotalYearlyAdminFee + APRFinder.DocFee + APRFinder.OptionToPurchaseFee;

            ////////////////


            Console.Write(JsonConvert.SerializeObject(new
            {
                timeTaken = elapsedMs,
                balloon = APRFinders[0].Balloon,
                advance = APRFinders[0].Advance,
                targetAPR = APRFinders[0].TargetAPR,
                APROutput = APRFinders[0].APRReturned,
                monthlyRepayment = APRFinders[0].MonthlyRepayment,
                ////monthlyRepaymentGuesses = APRFinders[0]..MonthlyRepaymentGuesses,
                numberOfAttempts = APRFinders[0].NoOfAttempts,
                //payProfile = APRCalculator.APRFinders[0].ConvertToMonths(APRFinder.Calculator.CashFlows)
            }, Formatting.Indented));
            
            Console.ReadKey();
        }


    }
}
