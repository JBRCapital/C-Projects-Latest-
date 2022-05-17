using System;
using System.Collections.Generic;
using APRCalculator.Domain;
using TridentGoalSeek;

namespace APRCalculator
{
    public class APRFinder : IGoalSeekAlgorithm
    {
        #region Fields

        public int CostOfVehicle;
        public int Deposit;
        public int Term;
        public int Balloon;
        public decimal TargetAPR;

        public int YearlyAdminFee;

        public bool IncludeYearlyAdminFee;
        public bool IncludeVAT;
        public decimal VAT;

        public decimal StartPosition;
        public int Advance;

        public int DocFee;
        public int OptionToPurchaseFee;
        public decimal OptionToPurchaseFeeWithVAT { get {
                return OptionToPurchaseFee * VatPercent;
        } }

        decimal VatPercent
        {
            get
            {
                decimal vatPercent = 1;
                if (IncludeVAT)
                {
                    vatPercent += VAT / 100;
                }

                return vatPercent;
            }
        }

    public int TotalYearlyAdminFee;
        public double TotalMonthlyRepayment = 0;

        public decimal MonthlyRepayment = 0;
        public int NoOfAttempts;

        public decimal APRReturned = 0;

        public class RepaymentGuess {
            public decimal Amount;
            public decimal APR;
        }
        //public List<RepaymentGuess> MonthlyRepaymentGuesses = new List<RepaymentGuess>();

        public ICalculator Calculator;

        #endregion

        public APRFinder() {}

        public static List<object> ConvertToMonths(List<double> cashFlows)
        {
            List<object> months = new List<object>();
            int monthCounter = 0;
            foreach (var cashflowMonth in cashFlows)
            {
                //if (monthCounter == 0) { monthCounter++; continue; }

                months.Add(new { month = monthCounter, payment = cashflowMonth });
                monthCounter++;
               
            }

            return months;
        }

        public void Setup(decimal? startPostion)
        {
            Advance = CostOfVehicle - Deposit;
            if (startPostion.HasValue)
            {
                StartPosition = startPostion.Value;
            }
            else
            {
                StartPosition = ((Advance - Balloon) / Term) * ((1+(TargetAPR/100))*1.3m);
            }
            
            Calculator = NewtonRaphsonIRRCalculator.Instance;
        }

        public decimal Calculate(decimal monthlyRepayment)
        {
            
            CreatePayProfile(monthlyRepayment);
            //double interestRate = Excel.FinancialFunctions.Financial.Irr(Calculator.CashFlows,(double)StartPosition)*12;
            double interestRate = Calculator.Execute() * 12;
            
            double interestRateTimes100 = interestRate * 100;

            double APR = (Math.Pow((1 + interestRate / 12), 12) - 1);
            decimal APRTimes100 = (decimal)APR * 100M;

            //MonthlyRepaymentGuesses.Add(new RepaymentGuess() { Amount = monthlyRepayment, APR = APRTimes100 } );
            APRReturned = decimal.Round(APRTimes100,2);
            // Console.WriteLine(APRTimes100);

            return APRTimes100;
        }

        private void CreatePayProfile(decimal monthlyRepayment)
        {
            TotalMonthlyRepayment = 0;
            TotalYearlyAdminFee = 0;

            decimal vatPercent = VatPercent;
            decimal optionToPurchaseFeeWithVAT = OptionToPurchaseFeeWithVAT;

            decimal adminFee = 0;
            if (IncludeYearlyAdminFee)
            {
                adminFee = YearlyAdminFee * vatPercent;
            }

            if (Calculator.CashFlows == null)
            {
                Calculator.CashFlows = new List<double>(Term) { };
                for (int monthCounter = 0; monthCounter <= Term; monthCounter++)
                {
                    Calculator.CashFlows.Add(0);
                }
            }
            for (int monthCounter = 0; monthCounter <= Term; monthCounter++)
            {
                Calculator.CashFlows[monthCounter] = ((double)decimal.Round((monthCounter != 0 ? monthlyRepayment : 0) +
                                                       (monthCounter != 0 ? 0 : -Advance) +
                                                       (monthCounter != 1 ? 0 : DocFee) +
                                                       ((monthCounter % 12) != 0 || monthCounter == 0 ? 0 : adminFee) +
                                                       (monthCounter != Term ? 0 : Balloon + optionToPurchaseFeeWithVAT)  //Include the Balloon if its the end of the term
                                                      ,2));

                if (monthCounter == 2) {
                    MonthlyRepayment = (int)Math.Round(monthlyRepayment, 0); ;
                }
                TotalMonthlyRepayment += (double)decimal.Round(monthlyRepayment);
                TotalYearlyAdminFee += (int)((monthCounter % 12) != 0 || monthCounter == 0 ? 0 : adminFee);
            }
        }

        public APRFinder calculateMonthlyPayment() {
            var goalSeeker = new GoalSeek(this);

            var seekResult = goalSeeker.SeekResult(0.1M, 1, 2, StartPosition, 10, int.MaxValue, true, TargetAPR);
            
            //Console.WriteLine(Math.Round(seekResult.InputVariable.Value, 0));
            //Console.WriteLine(seekResult.OutputValue);
            NoOfAttempts = seekResult.StabAttempts;
            //WithinAcceptanceThreshold = seekResult.WithinAcceptanceThreshold;

            MonthlyRepayment = (int)Math.Round(seekResult.InputVariable.Value, 0);

            return this;
        }


        static void Calculator_OnDataPointGenerated(object sender, Domain.IRRCalculatorEventArgs e)
        {
            //Plot results here
        }
    }
}