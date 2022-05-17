using System;
using System.Net.Mail;
using System.Web.Configuration;
using CallCreditWrapper;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace CallCreditApiDelegation.Helpers
{
    public static class EmailHelper
    {
        public static bool SendScoreEmail(CallCreditModel callCreditModel, CallCreditResult callCreditScoreResult)
        {
            var scoreTemplate = WebConfigurationManager.AppSettings["scoreTemplate"];
            var subject = WebConfigurationManager.AppSettings["subject"];
            var body = TemplateHelper.PopulateTemplate(callCreditModel, callCreditScoreResult, scoreTemplate);
            return SendEmail(callCreditModel.email, subject, body);
        }
        private static bool SendEmail(string to, string subject, string body)
        {
            try
            {
                //var email = WebConfigurationManager.AppSettings["emailuser"];
                //var email = "doNotReply@jbrcapital.com";
                //the email password used in the smtp
                //in case it needs one, like gmail smtp
                //ok, im using the local smtp, no pasword required. I can find these settings. 
                //one sec
                //var password = WebConfigurationManager.AppSettings["emailpassword"];
                //SmtpClient client = new SmtpClient
                //{
                //    Port = 587,
                //    Host = "smtp.gmail.com",
                //    EnableSsl = true,
                //    Timeout = 10000,
                //    DeliveryMethod = SmtpDeliveryMethod.Network,
                //    UseDefaultCredentials = false,
                //    Credentials = new System.Net.NetworkCredential(email, password)
                //};


                var msg = new MailMessage("doNotReply@jbrcapital.com",to)
                {
                    From = new MailAddress("doNotReply@jbrcapital.com", "JBR Captial Credit Scorer"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                var mySmtpClient = new SmtpClient { EnableSsl = false };
                mySmtpClient.Send(msg);
                //client.Send(msg);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }

    public static class SendGridHelper
    {
        public static bool SendScoreEmail(CallCreditModel to, CallCreditResult callCreditResult)
        {
            var scoreTemplate = WebConfigurationManager.AppSettings["scoreTemplate"];
            var subject = WebConfigurationManager.AppSettings["subject"];
            var body = TemplateHelper.PopulateTemplate(to, callCreditResult, scoreTemplate);
            return SendEmail(new EmailAddress(to.email), subject, body);
        }

        private static bool SendEmail(EmailAddress to, string subject, string body)
        {
            try
            {
                var client = new SendGridClient(WebConfigurationManager.AppSettings["sendgridapikey"]);
                var from = new EmailAddress(WebConfigurationManager.AppSettings["emailuser"]);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, body, body);
                var result = client.SendEmailAsync(msg);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}