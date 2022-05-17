using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mail;

namespace SMTPHelper
{
    public static class Helpers
    {
        public static void SendEmail(SmtpClient client, email email, string attachmentPath, bool isBodyHtml = false)
        {
            var msg = new MailMessage { From = new MailAddress(email.sender, email.senderName) };

            var recepientEmails = email.recipent.Split(',');
            var recepientEmailNames = email.recipentName.Split(',');
            for (int recepientCounter = 0; recepientCounter < recepientEmails.Length; recepientCounter++)
            {
                msg.To.Add(new MailAddress(recepientEmails[recepientCounter].Trim(), recepientEmailNames[recepientCounter].Trim()));
            }
            for (int recepientCCCounter = 0; recepientCCCounter < email.CC.Count; recepientCCCounter++)
            {
                msg.CC.Add(new MailAddress(email.CC[recepientCCCounter].Trim(), email.CC[recepientCCCounter].Trim()));
            }
            for (int recepientBccCounter = 0; recepientBccCounter < email.BCC.Count; recepientBccCounter++)
            {
                msg.Bcc.Add(new MailAddress(email.BCC[recepientBccCounter].Trim(), email.BCC[recepientBccCounter].Trim()));
            }
            msg.Subject = email.subject;
            msg.Body = email.body;
            msg.IsBodyHtml = isBodyHtml;
            if (!string.IsNullOrEmpty(attachmentPath) && File.Exists(attachmentPath))
                msg.Attachments.Add(new Attachment(attachmentPath));
            try
            {
                client.Send(msg);
            }
            catch (Exception e)
            {
                //how would you like to handle email sending failure?
            }
            finally
            {
                foreach (Attachment attachment in msg.Attachments)
                {
                    attachment.Dispose();
                }
            }
        }

        public static void CreateEmailSender(out SmtpClient client, out email email, string subjectSuffix)
        {
            client = CreateSMTPClient();

            email = new email(
                              ConfigurationManager.AppSettings["SMTPTo"],
                              ConfigurationManager.AppSettings["SMTPToNAME"],
                              ConfigurationManager.AppSettings["SMTPSubject"] + subjectSuffix,
                              ConfigurationManager.AppSettings["SMTPBody"],
                              ConfigurationManager.AppSettings["SMTPSender"],
                              ConfigurationManager.AppSettings["SMTPSenderName"],
                              new List<string>(),
                              new List<string>()
            );
        }

        public static SmtpClient CreateSMTPClient()
{
            SmtpClient client;
            var user = new NetworkCredential(ConfigurationManager.AppSettings["SMTPUser"], System.Net.WebUtility.HtmlDecode(ConfigurationManager.AppSettings["SMTPPassword"]));
            client = new SmtpClient(ConfigurationManager.AppSettings["SMTPServer"], ushort.Parse(ConfigurationManager.AppSettings["SMTPPort"]))
            {
                EnableSsl = true,
                UseDefaultCredentials = true,
                Credentials = user
            };
            return client;
        }

        public static bool IsValidEmail(string email, bool emptyIsValid)
        {

            if (emptyIsValid && email.Length == 0) { return true; }
            if (!emptyIsValid && email.Length == 0) { return false; }
            if (email.EndsWith(".")) { return false; }

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }

    public class email
    {
        public string subject { get; set; }
        public string body { get; set; }
        public string recipent { get; set; }
        public string recipentName { get; set; }
        public string sender { get; set; }
        public string senderName { get; set; }
        public List<string> CC { get; set; }
        public List<string> BCC { get; set; }

        //public email(string recipent, string subject, string body)
        //{
        //    this.recipent = recipent;
        //    this.subject = subject;
        //    this.body = body;
        //}

        public email(string recipent, string recipentName, string subject, string body, string sender, string senderName, List<string> cc, List<string> bcc)
        {
            this.recipent = recipent;
            this.recipentName = recipentName;
            this.subject = subject;
            this.body = body;
            this.sender = sender;
            this.senderName = senderName;
            CC = cc;
            BCC = bcc;
        }
    }

}
