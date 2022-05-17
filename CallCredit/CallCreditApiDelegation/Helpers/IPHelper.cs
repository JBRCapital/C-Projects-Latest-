using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CallCreditApiDelegation.Helpers
{
    public static class IPHelper
    {
        public static string GetIPAddress()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }
    }
}