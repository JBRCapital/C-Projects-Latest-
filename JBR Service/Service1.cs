using System;
using System.ServiceProcess;
using System.IO;
using System.Threading;
using System.Configuration;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using System.IO.Pipes;
using Salesforce.Common;
using Salesforce.Force;

namespace WindowsServiceCS
{
    //  .\JBRServiceUser
    //  JBRServiceBlah129

    public partial class Service1 : ServiceBase
    {
        public static string SecurityToken = ConfigurationManager.AppSettings["SecurityToken"];
        public static string ConsumerKey = ConfigurationManager.AppSettings["ConsumerKey"];
        public static string ConsumerSecret = ConfigurationManager.AppSettings["ConsumerSecret"];
        public static string Username = ConfigurationManager.AppSettings["Username"];
        public static string Password = ConfigurationManager.AppSettings["Password"];
        public class AnchorWebService__c { public string id { get; set; } }
        public const int delayBetweenRecordSyncs = 10000;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            WriteToFile("Simple Service started");
            Thread t = new Thread(new ThreadStart(this.ScheduleService));
            t.Start();
            //this.ScheduleService();
        }

        protected override void OnStop()
        {
            WriteToFile("Simple Service stopped");
            //this.Schedular.Dispose();
        }

        //private Timer Schedular;

        public void ScheduleService()
        {
            var concurrentQueue = new ConcurrentQueue<string>();

            Task.Run(async () =>
            {
                while (true)
                {
                    //string message = 
                    await AsyncPipeServer.Listen(concurrentQueue);
                    //Console.WriteLine("hi");
                }
            });//.Wait();

            Task.Run(async () =>
            {
                int counter = 0;
                while (true)
                {

                    if (!concurrentQueue.IsEmpty)
                    {
                        string proposalData;
                        while (concurrentQueue.TryDequeue(out proposalData))
                        {
                            string result = await processUpdate(proposalData);
                            WriteToFile(string.Concat(counter, ": ", result));
                            counter++;
                            if (counter > 99999999) { counter = 0; }
                            Thread.Sleep(delayBetweenRecordSyncs);
                        }
                    }
                    Thread.Sleep(500);
                }
            }).Wait();
        }


        static async Task<string> processUpdate(string proposalData)
        {
            var proposalDataArray = proposalData.Split(new string[] { "**" }, StringSplitOptions.None);
            var eventType__c = proposalDataArray[0];
            var eventData__c = proposalDataArray[1];
            var eventDate__c = proposalDataArray[2];
            var clientApplicationReference__c = proposalDataArray[3];

            var eventRecord = new
            {
                eventType__c = eventType__c,
                eventData__c = eventData__c,
                eventDate__c = eventDate__c,
                isRequestMade__c = false,
                WasRequestSuccessful__c = false,
                Response__c = string.Empty
            };

            // Authenticate with Salesforce             
            //sb.Append("Authenticating with Salesforce");
            //var url = "https://login.salesforce.com/services/oauth2/token";

            var notSubmitted = true;

            ForceClient client = null;
            string response = string.Empty;

            while (notSubmitted)
            {
                try
                {
                    var auth = new AuthenticationClient();
                    await auth.UsernamePasswordAsync(ConsumerKey, ConsumerSecret, Username, Password + SecurityToken); //,, url);
                    client = new ForceClient(auth.InstanceUrl, auth.AccessToken, auth.ApiVersion);
                     
                    var queryString = string.Concat("SELECT id FROM AnchorWebService__c WHERE clientApplicationReference__c = '", clientApplicationReference__c, "'");
                    var anchorWebServices = await client.QueryAsync<AnchorWebService__c>(queryString);

                    foreach (var anchorWebService in anchorWebServices.Records)
                    {
                        await client.DeleteAsync("AnchorWebService__c", anchorWebService.id);
                    }

                    var successResponse = await client.UpsertExternalAsync("AnchorWebService__c", "clientApplicationReference__c", clientApplicationReference__c, eventRecord);

                    response = (successResponse.Success ? "Success" : "Failed") + Environment.NewLine;
                    response += proposalData + Environment.NewLine;

                    if (!successResponse.Success)
                    {
                        Thread.Sleep(delayBetweenRecordSyncs);
                    }

                    notSubmitted = !successResponse.Success;
                }
                catch (Exception ex)
                {
                    WriteToFile(ex.ToString());
                    Thread.Sleep(2000);
                }
                finally
                {
                    if (client != null) { client.Dispose(); }
                }
            }
            
            return response;
        }

        private static void WriteToFile(string text)
        {
            string path = string.Concat("C:\\JBR\\Logs\\ServiceLog", DateTime.Now.ToString("-dd-MM-yyyy"),".txt");
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(string.Concat( DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"), " - ", text));
                writer.Close();
            }
        }
    }

    class AsyncPipeServer
    {
        public static async Task Listen(ConcurrentQueue<string> concurrentQueue)
        {
            var BUFFER_SIZE = 4096 * 4;
            PipeSecurity ps = new PipeSecurity();
            System.Security.Principal.SecurityIdentifier sid = new System.Security.Principal.SecurityIdentifier(System.Security.Principal.WellKnownSidType.WorldSid, null);
            PipeAccessRule par = new PipeAccessRule(sid, PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow);
            ps.AddAccessRule(par);

            using (NamedPipeServerStream pipeServer = new NamedPipeServerStream("AWS_Pipe_For_Panther", PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous, BUFFER_SIZE, BUFFER_SIZE, ps))
            {

                await Task.Factory.FromAsync(pipeServer.BeginWaitForConnection, pipeServer.EndWaitForConnection, null);

                using (StreamReader reader = new StreamReader(pipeServer))
                {
                    concurrentQueue.Enqueue(await reader.ReadToEndAsync());
                }
            }
        }
    }
}
