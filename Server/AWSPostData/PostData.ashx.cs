
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using System.IO.Pipes;

namespace AWSPostData
{
    public class PostData : HttpTaskAsyncHandler
    {
        private async Task<string> ProcessAsync(HttpContext context)
        {
            //Stopwatch sw = new Stopwatch();
            //sw.Start();

            //var xml = @"<?xml version='1.0'?>
            //            <EventQueueItem xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema'>
            //              <Type>ProposalSaved</Type>
            //              <ClientApplicationName>Proposal</ClientApplicationName>
            //              <Data>
            //                <KeyValue>
            //                  <Key>EventData</Key>
            //                  <Value>1234556</Value>
            //                </KeyValue>
            //                <KeyValue>
            //                  <Key>ClientApplicationReference</Key>
            //                  <Value>347</Value>
            //                </KeyValue>
            //                <KeyValue>
            //                  <Key>EventDate</Key>
            //                  <Value>2016-01-13T11:47:33</Value>
            //                </KeyValue>
            //              </Data>
            //          </EventQueueItem>";

            string postData = string.Empty;
            using (var reader = new StreamReader(context.Request.InputStream))
            {
                postData += reader.ReadToEnd();
            }

            //WriteToFile(postData);

            if (postData == string.Empty) { return "Error: Post String is Empty"; }

            var doc = XDocument.Load(new StringReader(postData));

            //var doc = XDocument.Load(new StreamReader(context.Request.InputStream));

            string eventType__c = doc.XPathSelectElement("/EventQueueItem/Type").Value;
            string eventData__c = string.Empty, clientApplicationReference__c = string.Empty, eventDate__c = string.Empty;

            var query = doc.XPathSelectElements("/EventQueueItem/Data/KeyValue");
            foreach (XElement el in query)
            {
                switch (el.XPathSelectElement("Key").Value)
                {
                    case "EventData":
                        eventData__c = el.XPathSelectElement("Value").Value;
                        break;
                    case "ClientApplicationReference":
                        clientApplicationReference__c = el.XPathSelectElement("Value").Value;
                        break;
                    case "EventDate":
                        eventDate__c = el.XPathSelectElement("Value").Value;
                        break;
                }
            }

            await SendAwait(string.Concat(eventType__c, "**", eventData__c, "**", eventDate__c, "**", clientApplicationReference__c),
                            "AWS_Pipe_For_Panther", 3000);

            return "Posted to local server";
        }

        public static async Task SendAwait(string SendStr, string PipeName, int TimeOut = 1000)
        {
            using (NamedPipeClientStream pipeStream = new NamedPipeClientStream(".", PipeName, PipeDirection.Out, PipeOptions.Asynchronous))
            {
                // The connect function will indefinitely wait for the pipe to become available
                // If that is not acceptable specify a maximum waiting time (in ms)
                pipeStream.Connect(TimeOut);

                using (StreamWriter sw = new StreamWriter(pipeStream))
                {
                    await sw.WriteAsync(SendStr);
                    // flush
                    await pipeStream.FlushAsync();
                }
            }
        }

        public async override Task ProcessRequestAsync(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write("Response...." + Environment.NewLine);
            context.Response.Write(await ProcessAsync(context));
        }

        public override bool IsReusable { get { return false; } }

        private static void WriteToFile(string text)
        {
            string path = string.Concat("C:\\JBR\\Logs\\PostData", DateTime.Now.ToString("-dd-MM-yyyy"), ".txt");
            using (StreamWriter writer = new StreamWriter(path, true))
            {
                writer.WriteLine(string.Concat(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt"), " - ", text));
                writer.Close();
            }
        }
    }
}