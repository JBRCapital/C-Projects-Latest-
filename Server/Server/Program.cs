using Salesforce.Common;
using Salesforce.Common.Models;
using Salesforce.Force;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

class PipeServer
{
    public static string SecurityToken = Server.Properties.Settings.Default.SecurityToken;
    public static string ConsumerKey = Server.Properties.Settings.Default.ConsumerKey;
    public static string ConsumerSecret = Server.Properties.Settings.Default.ConsumerSecret;
    public static string Username = Server.Properties.Settings.Default.Username;
    public static string Password = Server.Properties.Settings.Default.Password;


    public static bool IsSandboxUser = Server.Properties.Settings.Default.IsSandboxUser;

    static void Main()
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
                        Console.WriteLine(string.Concat(counter, ": ", DateTime.Now.ToString(), ": ", result));
                        counter++;
                        Thread.Sleep(3000);
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
                SuccessResponse successResponse = await client.UpsertExternalAsync("AnchorWebService__c", "clientApplicationReference__c", clientApplicationReference__c, eventRecord);

                response = (successResponse.Success ? "Success" : "Failed") + Environment.NewLine;
                response += proposalData + Environment.NewLine;

                if (!successResponse.Success)
                {
                    Thread.Sleep(3000);
                }

                notSubmitted = !successResponse.Success;
            }
            catch (Exception)
            {
                Thread.Sleep(2000);
            }
            finally
            {
                if (client != null) { client.Dispose(); }
            }
        }

        //sw.Stop();

        //response += string.Concat("Elapsed=", sw.Elapsed) + Environment.NewLine;

        

        return response;
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