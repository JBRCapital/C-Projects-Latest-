using IO.ClickSend.ClickSend.Api;
using IO.ClickSend.Client;
using IO.ClickSend.ClickSend.Model;
using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace ClicksendHelper
{
    public class ClickSendUploadFile
    {
        public static Boolean UploadFileToSendClick(ClickSendCredentials clickSendCredentials, string localpath, out string url)
        {
            url = string.Empty;

            var uploadApi = new UploadApi(new Configuration()
            {
                Username = clickSendCredentials.Username,
                Password = clickSendCredentials.Password
            });

            Byte[] bytes = File.ReadAllBytes(localpath);
            String file = Convert.ToBase64String(bytes);

           var response = JObject.Parse(uploadApi.UploadsPost(new UploadFile(file), "post"));
           
            if ((int)response.SelectToken("http_code") != 200)  return false; 

           url = (string)response.SelectToken("data._url");
           return true;
        }
    }
}
