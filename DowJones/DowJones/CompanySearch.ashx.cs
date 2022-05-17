using System.Web;
using System.Threading.Tasks;

namespace DowJones
{
    /// <summary>
    /// Summary description for GetFullSearch
    /// </summary>
    public class CompanySearch : HttpTaskAsyncHandler
    {

        public async override Task ProcessRequestAsync(HttpContext context)
        {
            await DowjonesCaller.CallDowJones(context, string.Concat("https://", Properties.Resources.dowJonesURL, Properties.Resources.urlCompany));
        }

        public override bool IsReusable { get { return false; } }
    }
}