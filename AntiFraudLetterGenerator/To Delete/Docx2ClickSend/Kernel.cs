using Microsoft.Office.Interop.Word;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Xceed.Words.NET;

namespace Docx2ClickSend
{
    public class Kernel
    {
        string DocxTemplatePath { get; }
        string DocxFilePath { get; }
        string PdfFilePath { get; }
        string PdfInternetPath { get; }

        public Kernel(string DocxFilePath, string DocxOutputPath, string outputPath)
        {
            if (File.Exists(DocxFilePath))
            {
                string guid = Guid.NewGuid().ToString();
                string fileName = guid + ".pdf";

                this.DocxTemplatePath = DocxFilePath;
                
                this.DocxFilePath = DocxOutputPath + guid + ".docx";
                this.PdfFilePath = outputPath+ fileName;
                this.PdfInternetPath = "https://jbrcapital1.net/clickSendOutput/" + fileName;
            }
            else
                throw new FileNotFoundException();
        }

        public Kernel Replace(WordValues DocxValues)
        {
            using (DocX document = DocX.Load(DocxTemplatePath))
            {
                foreach (var item in DocxValues.AsDictionary())
                    document.ReplaceText($"<{item.Key}>", item.Value);
                document.SaveAs(this.DocxFilePath);
            }
            return this;
        }

        public Kernel Convert()
        {
            _Application oWord = new Application();
            oWord.Visible = false;
            object oMissing = System.Reflection.Missing.Value;
            object isVisible = true;
            object readOnly = false;
            object oInput = this.DocxFilePath;
            object oOutput = this.PdfFilePath;
            object oFormat = WdSaveFormat.wdFormatPDF;
            _Document oDoc = oWord.Documents.Open(ref oInput, ref oMissing, ref readOnly, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref isVisible,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing);
            oDoc.Activate();
            oDoc.SaveAs(ref oOutput, ref oFormat, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing,
                ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing, ref oMissing);
            oWord.Quit(ref oMissing, ref oMissing, ref oMissing);
            return this;
        }

        public async Task<ClickSendResult> Send(SendClickValues SendclickValues,
            ClickSendCredentials credentials)
        {
            //JObject APIdata = ConstructJSONobjectForRequestTEST(SendclickValues.AsDictionary(), @"http://unec.edu.az/application/uploads/2014/12/pdf-sample.pdf");
            //comment the above line and uncomment the below line to send the actual pdf
            //assuming that the directory you will add this to will be accessable for clicksend
            JObject APIdata = ConstructJSONobjectForRequest(SendclickValues.AsDictionary(), PdfInternetPath);
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://rest.clicksend.com/");
                var content = new StringContent(APIdata.ToString(), Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(
                        "Basic",
                        System.Convert.ToBase64String(
                            Encoding.ASCII.GetBytes(
                                string.Format("{0}:{1}", credentials.Username, credentials.Password))));
                var ApiResult = await client.PostAsync("v3/post/letters/send", content);
                string result = await ApiResult.Content.ReadAsStringAsync();
                
                return JsonConvert.DeserializeObject<ClickSendResult>(result);
            }
        }

        /// <summary>
        /// used to construct the json object using the data you pass to the API
        /// all elements other than the WordTemplateValues
        /// This test version use 11111 for the postal code which the API considers
        /// a test postal code
        /// </summary>
        /// <param name="ClickSendElements">all elements passed to the api to populate the API call to clicksend</param>
        /// <param name="Externalpath">the path of the pdf file, must be accessable for clicksend to fetch the file</param>
        /// <returns>returns the json object used in the ClickSend call</returns>
        private JObject ConstructJSONobjectForRequestTEST(Dictionary<string, string> ClickSendElements, string Externalpath)
        {
            return new JObject(
                        new JProperty("file_url", Externalpath),
                        ClickSendElements.GetElementOrDefaultValue("template_used", 1),
                        ClickSendElements.GetElementOrDefaultValue("colour", 0),
                        ClickSendElements.GetElementOrDefaultValue("duplex", 0),
                        ClickSendElements.GetElementOrDefaultValue("priority_post", 1),
                        new JProperty("recipients", new JArray(
                        new JObject(
                        ClickSendElements.GetElementOrDefaultValue("address_name", string.Empty),
                        ClickSendElements.GetElementOrDefaultValue("address_line_1", string.Empty),
                        ClickSendElements.GetElementOrDefaultValue("address_line_2", string.Empty),
                        ClickSendElements.GetElementOrDefaultValue("address_city", string.Empty),
                        ClickSendElements.GetElementOrDefaultValue("address_state", string.Empty),
                        new JProperty("address_postal_code", 11111),
                        ClickSendElements.GetElementOrDefaultValue("address_country", string.Empty),
                        ClickSendElements.GetElementOrDefaultValue("return_address_id", 65447),
                        ClickSendElements.GetElementOrDefaultValue("schedule", 0)
                            )
                        ))
                    );
        }

        /// <summary>
        /// used to construct the json object using the data you pass to the API
        /// all elements other than the WordTemplateValues
        /// </summary>
        /// <param name="ClickSendElements">all elements passed to the api to populate the API call to clicksend</param>
        /// <param name="Externalpath">the path of the pdf file, must be accessable for clicksend to fetch the file</param>
        /// <returns>returns the json object used in the ClickSend call</returns>
        private JObject ConstructJSONobjectForRequest(Dictionary<string, string> ClickSendElements, string Externalpath)
        {
            return new JObject(
                        new JProperty("file_url", Externalpath),
                        ClickSendElements.GetElementOrDefaultValue("template_used", 1),
                        ClickSendElements.GetElementOrDefaultValue("colour", 1),
                        ClickSendElements.GetElementOrDefaultValue("duplex", 0),
                        ClickSendElements.GetElementOrDefaultValue("priority_post", 1),
                        new JProperty("recipients", new JArray(
                              new JObject(
                                ClickSendElements.GetElementOrDefaultValue("address_name", string.Empty),
                                ClickSendElements.GetElementOrDefaultValue("address_line_1", string.Empty),
                                ClickSendElements.GetElementOrDefaultValue("address_line_2", string.Empty),
                                ClickSendElements.GetElementOrDefaultValue("address_city", string.Empty),
                                ClickSendElements.GetElementOrDefaultValue("address_state", string.Empty),
                                ClickSendElements.GetElementOrDefaultValue("address_postal_code", string.Empty),
                                ClickSendElements.GetElementOrDefaultValue("address_country", "United Kingdom"),
                                ClickSendElements.GetElementOrDefaultValue("return_address_id", 65447),
                                ClickSendElements.GetElementOrDefaultValue("schedule", 0)
                            )
                        ))
                    );
        }
    }
}
