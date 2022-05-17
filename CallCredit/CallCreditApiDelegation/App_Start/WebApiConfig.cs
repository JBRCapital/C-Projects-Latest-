using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;

namespace CallCreditApiDelegation
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            //used to enable to CORS, the actual settings is at the web config
            //for now, only permitting the the callcredittest.tk domain
            config.EnableCors();

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}"
            );
        }
    }
}
