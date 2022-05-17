using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;

namespace CallCreditApiDelegation.Helpers
{
    public static class HTTPHelpers
    {
        public static HttpResponseMessage getImageHttpResponseMessage(byte[] imgbytes)
        {
            HttpResponseMessage result = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new ByteArrayContent(imgbytes)
            };
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

            return result;
        }
    }
}