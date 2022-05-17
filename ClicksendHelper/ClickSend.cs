using ClicksendHelper;
using DictionaryHelper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ClickSendHelper
{
    public static class ClickSend
    {
        //static bool firstTime = false;
        public static bool SendClickSendLetter(string statementPath, ClickSendValues clickSendValues,
              string clicksendUserName, string clicksendPassword)
        {
            //; var tries = 0;
            //Task<ClickSendResult> asyncTaskResult;

            var clickSendCredentials = new ClickSendCredentials(clicksendUserName, clicksendPassword);

            ClickSendUploadFile.UploadFileToSendClick(clickSendCredentials, statementPath, out string url);
            Task<ClickSendResult> asyncTaskResult = SendClickSendPdf(clickSendValues, clickSendCredentials, url);
            asyncTaskResult.Wait();
            return asyncTaskResult.Result.response_code == "SUCCESS";

            //do
            //{
            //    tries++;

            //    asyncTaskResult = ClickSend.SendFile(
            //     string.Concat(ftpServer, '/', ftpPath),
            //     ftpUserName,
            //     ftpPassword,
            //     clicksendUserName,
            //     clicksendPassword,
            //     clickSendValues,
            //     statementPath,
            //     clicksendCollectFileURL);

            //    //Todo: This is temporary, remove this after going live. 
            //    if (asyncTaskResult == null) return true;

            //    asyncTaskResult.Wait();

            //} while (asyncTaskResult.Result.response_code != "SUCCESS" && tries < 3);

            
        }

        //public static Task<ClickSendResult> SendFile(string ftpAddress,
        //                                             string ftpUserName,
        //                                             string ftpPassword,
        //                                             string clickSendUserName,
        //                                             string clicksendPassword,
        //                                             ClickSendValues clickSendValues,
        //                                             string pdfFilePath, string clickSendURLPath)
        //{
        //    string pdfFileName = Path.GetFileName(pdfFilePath);
           
        //    writeAddressToFile(clickSendValues, pdfFilePath);
        //    FTPHelper.Helpers.UploadTheFileToFTP(ftpUserName, ftpPassword, 
        //                                           string.Concat(ftpAddress, '/' , pdfFileName),
        //                                           pdfFilePath);

        //    // return null;
        //    if (firstTime)
        //    {
        //        System.Threading.Thread.Sleep(10000);
        //        firstTime = false;
        //    }
        //    System.Threading.Thread.Sleep(5000);
        //    return SendClickSendPdf(clickSendValues, new ClickSendCredentials(clickSendUserName, clicksendPassword), string.Concat(clickSendURLPath,'/', pdfFileName));
        //}

        //private static void writeAddressToFile(ClickSendValues clickSendValues, string pdfFilePath)
        //{
        //    // Save to disk.
        //    File.WriteAllText(pdfFilePath + ".txt", clickSendValues.ToString());
        //}


        private static async Task<ClickSendResult> SendClickSendPdf(ClickSendValues SendclickValues, ClickSendCredentials credentials, string pdfPath)
        {
            //JObject APIdata = ConstructJSONobjectForRequestTEST(SendclickValues.AsDictionary(), @"http://unec.edu.az/application/uploads/2014/12/pdf-sample.pdf");
            //comment the above line and uncomment the below line to send the actual pdf
            //assuming that the directory you will add this to will be accessable for clicksend
            JObject APIdata = ConstructJSONobjectForRequest(SendclickValues.AsDictionary(), pdfPath);
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
        //private static JObject ConstructJSONobjectForRequestTEST(Dictionary<string, string> ClickSendElements, string Externalpath)
        //{
        //    return new JObject(
        //                new JProperty("file_url", Externalpath),
        //                ClickSendElements.GetElementOrDefaultValue("template_used", 1),
        //                ClickSendElements.GetElementOrDefaultValue("colour", 0),
        //                ClickSendElements.GetElementOrDefaultValue("duplex", 0),
        //                ClickSendElements.GetElementOrDefaultValue("priority_post", 1),
        //                new JProperty("recipients", new JArray(
        //                new JObject(
        //                ClickSendElements.GetElementOrDefaultValue("address_name", string.Empty),
        //                ClickSendElements.GetElementOrDefaultValue("address_line_1", string.Empty),
        //                ClickSendElements.GetElementOrDefaultValue("address_line_2", string.Empty),
        //                ClickSendElements.GetElementOrDefaultValue("address_city", string.Empty),
        //                ClickSendElements.GetElementOrDefaultValue("address_state", string.Empty),
        //                new JProperty("address_postal_code", 11111),
        //                ClickSendElements.GetElementOrDefaultValue("address_country", string.Empty),
        //                ClickSendElements.GetElementOrDefaultValue("return_address_id", 65447),
        //                ClickSendElements.GetElementOrDefaultValue("schedule", 0)
        //                    )
        //                ))
        //            );
        //}

        /// <summary>
        /// used to construct the json object using the data you pass to the API
        /// all elements other than the WordTemplateValues
        /// </summary>
        /// <param name="ClickSendElements">all elements passed to the api to populate the API call to clicksend</param>
        /// <param name="Externalpath">the path of the pdf file, must be accessable for clicksend to fetch the file</param>
        /// <returns>returns the json object used in the ClickSend call</returns>
        private static JObject ConstructJSONobjectForRequest(Dictionary<string, string> ClickSendElements, string Externalpath)
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

    internal class ClicksendOptions
    {
    }
}
