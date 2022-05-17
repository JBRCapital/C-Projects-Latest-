using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UpworkPlatesLookupAutomationDLL;

namespace ImplementationTest
{
    class Program
    {

        public static Dictionary<string, Result> Results = new Dictionary<string, Result>();

        [STAThread]
        static void Main(string[] args)
        {
            var lookUp = new PlateLookupAutomation();
            Result LookupResult = null;
            lookUp.Login();
            while (!lookUp.IsLoggedIn) Thread.Sleep(1000);

            lookUp.Lookup((result) => LookupResult = result, "LB54DZJ", "123", DateTime.Now);
            while (lookUp.IsLookingUp) Thread.Sleep(1000);

            Console.WriteLine(LookupResult.PolicyNumber);
            Console.ReadLine();
            Environment.Exit(1);

            //List<KeyValuePair<string, string>> Batchtest = new List<KeyValuePair<string, string>>()
            //{
            //    new KeyValuePair<string, string>("asdasd", "12ewe3"),
            //    new KeyValuePair<string, string>("LB54DZJ", "123"),
            //    new KeyValuePair<string, string>("N307DLV", "123"),
            //    new KeyValuePair<string, string>("LB54DZJ", "123"),
            //    new KeyValuePair<string, string>("N307DLV", "123"),
            //    new KeyValuePair<string, string>("asdasd", "12ewe3"),
            //};

            //foreach (var item in Batchtest)
            //{
            //    lookUp.Lookup((result) => LookupResult = result, item.Key, item.Value, DateTime.Now);
            //    Results.Add($"{item.Key}{item.Value}{new Random().Next()}", LookupResult);
            //    Thread.Sleep(500);
            //    while (lookUp.IsLookingUp) Thread.Sleep(1000);
            //}


        }
    }
}
