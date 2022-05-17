using System;
using System.Threading.Tasks;

namespace ClickSend
{
    class Program
    {
        static void Main(string[] args)
        {
            //try
            //    {

                    Console.WriteLine(string.Concat("Started - ", DateTime.Now.ToString()));
                    //file.WriteLine(string.Concat("Started - ", DateTime.Now.ToString()));

                    Salesforce.Force.ForceClient salesforceClient = null;

                    Task.Run(async () =>
                    {

                        salesforceClient = await DataHelper.GetSalesforceConnection();

                    }).Wait(5600000);

                    Antifraud.sendAntiFraudLetters(salesforceClient, null);

                    if (salesforceClient != null) { salesforceClient.Dispose(); }

                    Console.WriteLine(string.Concat("Finished - ", DateTime.Now.ToString()));
                    //file.WriteLine(string.Concat("Finished - ", DateTime.Now.ToString()));
                    //Console.ReadKey();

                //}
                //catch (Exception ex)
                //{
                //    using (System.IO.StreamWriter file =
                //    new System.IO.StreamWriter(string.Concat(@"C:\Windows\Temp\", Guid.NewGuid(), ".txt")))
                //    {
                //        file.WriteLine("Message :" + ex.Message + "<br/>" + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                //        Environment.NewLine + "Date :" + DateTime.Now.ToString());
                //    }

                //}
                //finally
                //{
                //    Environment.Exit(1);
                //}

            Environment.Exit(1);
        }
    }
}
