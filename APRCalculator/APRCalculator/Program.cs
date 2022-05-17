using System;
using TridentGoalSeek;
using System.Diagnostics;

namespace APRCalculator
{
    internal class Program
    {
        public static void Main()
        {
            //Stopwatch stopwatch = new Stopwatch();
            //stopwatch.Start();

            var APRFinder = new APRFinder()
            {
                CostOfVehicle = 25000,
                Deposit = 0,
                Term = 24,
                Balloon = 0,
                TargetAPR = 8.2m
            };
            
            APRFinder.Setup();
            //Console.WriteLine(APRFinder.Calculate(2171.88m));

           // Console.ReadKey();
            var goalSeeker = new GoalSeek(APRFinder);
            var seekResult = goalSeeker.SeekResult(0.1M,1,1, APRFinder.StartPosition, 1,int.MaxValue,false, APRFinder.TargetAPR);

            Console.WriteLine(Math.Round(seekResult.InputVariable.Value,0));
            Console.WriteLine(seekResult.OutputValue);
            Console.WriteLine(seekResult.StabAttempts);
            Console.WriteLine(seekResult.WithinAcceptanceThreshold);
            
            //stopwatch.Stop();
            //Console.WriteLine("Time elapsed in Milliseconds: {0}", stopwatch.Elapsed.TotalMilliseconds);

            Console.ReadLine();

            //decimal target = 7.4M;

            //var costOfVehicle = 633750;
            //var deposit = 0;

            //var advance = costOfVehicle - deposit;
            
            //var term = 48;
            //var balloon = 500000; 
            //var calculator = NewtonRaphsonIRRCalculator.Instance;

            //var startPosition = (advance / term) * 1.07;

            //calculator.CashFlows = new List<double> { advance };



            


            
 //-633750.00,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //6940.25,
 //500000.00 };


            //calculator.OnDataPointGenerated += new EventHandler<IRRCalculatorEventArgs>(Calculator_OnDataPointGenerated);


        }

        //static decimal CalculateTotalWithCompoundInterest(decimal principal, decimal interestRate, int compoundingPeriodsPerYear, double yearCount)
        //{
        //    return principal * (decimal)Math.Pow((double)(1 + interestRate / compoundingPeriodsPerYear), compoundingPeriodsPerYear * yearCount);
        //}


    }
}