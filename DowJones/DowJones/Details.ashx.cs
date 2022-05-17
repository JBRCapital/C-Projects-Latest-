using System.Web;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DowJones
{
    /// <summary>
    /// Summary description for GetFullSearch
    /// </summary>
    public class Details : HttpTaskAsyncHandler
    {

        public async override Task ProcessRequestAsync(HttpContext context)
        {
            await DowjonesCaller.CallDowJones(context, string.Concat("https://", 
                                              Properties.Resources.dowJonesURL, 
                                              Properties.Resources.urlId), 
                                              Properties.Resources.UrlIdRestfulName);
        }

        public override bool IsReusable { get { return false; } }
    }
}


