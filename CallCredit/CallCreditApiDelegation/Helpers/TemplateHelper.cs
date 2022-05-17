using System.IO;
using System.Web.Configuration;
using CallCreditWrapper;

namespace CallCreditApiDelegation.Helpers
{
    public static class TemplateHelper
    {
        public static string PopulateTemplate(CallCreditModel ccModel, CallCreditResult callCreditScoreResult,
                                              string scoreTemplate)
        {
            var body = File.ReadAllText($"C:\\JBR\\CallCreditEmailTemplates\\{scoreTemplate}");
            body = body.Replace("[title] [first name] [last name]", $"{ccModel.title} {ccModel.forename} {ccModel.surname}")
                       .Replace("[host]", WebConfigurationManager.AppSettings["currentHost"])
                       .Replace("[creditScoreText]", callCreditScoreResult.creditScoreText)
                       .Replace("[creditScoreExplanation]", callCreditScoreResult.creditScoreExplanation)
                       ;
            return body;
        }
    }
}