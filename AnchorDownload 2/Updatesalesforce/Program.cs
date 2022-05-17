
using SMTPHelper;
using System;
using System.Configuration;
using System.Globalization;
using System.Net.Mail;
using System.Text;

namespace UpdateSalesforceData
{
    public class Program
    {

        //ConnectionString

        public static StringBuilder EmailErrorLog = new StringBuilder();
        public static StringBuilder EmailTransactionLog = new StringBuilder();

        static void Main(string[] args)
        {
            try
            {
                LogHelper.Logger.WriteOutput(string.Concat("Started - ", DateTime.Now.ToString()), Program.EmailTransactionLog);

                AgreementUpdater.Run();
                
                #region full update

                //if (args.ToList()
                //    .Any(arg => new[] { "fullupdate", "-f", "f", "full" }.Any(flag => string.Equals(arg, flag, StringComparison.CurrentCultureIgnoreCase))))
                //{
                ProposalUpdater.Run();

                CustomerUpdater.Run();
                CustomerCompanyUpdater.Run();

                //Do not run this unless you know what you are doing, this can cost JBR a lot of money
                //Do not run this unless you know what you are doing, this can cost JBR a lot of money
                //Do not run this unless you know what you are doing, this can cost JBR a lot of money
                //    //VehicleUpdater.UpdateVehicleData();
                //Do not run this unless you know what you are doing, this can cost JBR a lot of money
                //Do not run this unless you know what you are doing, this can cost JBR a lot of money
                //Do not run this unless you know what you are doing, this can cost JBR a lot of money

                IntroducerUpdater.Run();
                DealerUpdater.Run();

                PayProfileUpdater.Run();
                    TransactionUpdater.Run();

                    //AmortisationUpdater.UpdateAmortisationData();
                //}

               // if (!TriggerControlUpdater.ShouldTriggersRun) { TriggerControlUpdater.UpdateTriggerControlData(true); }

                #endregion

                SQLDataConnectionHelper.SqlConnection.Close();

                //if (salesforceClient != null) { salesforceClient.Dispose(); }

                //Console.ReadLine();
            }
            catch (Exception ex)
            {
                LogHelper.Logger.WriteOutput(string.Concat("Error:",Environment.NewLine, LogHelper.Logger.GetExceptionDetails(ex)), EmailErrorLog);
                //ExceptionLogging.ExceptionLogging.Write(ex);
            }
            finally
            {
                //If EmailErrorLog is not empty send an email with the error log 
                if (EmailErrorLog.Length > 0)
                {
                    Helpers.CreateEmailSender(out SmtpClient errorEmailClient, out email errorEmail, " - " + DateTime.Today.Day + "-" + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Today.Month) + "-" + DateTime.Today.Year);
                    errorEmail.body = EmailErrorLog.ToString();
                    errorEmail.subject = ConfigurationManager.AppSettings["SMTPErrorSubject"];
                    errorEmail.recipent = ConfigurationManager.AppSettings["SMTPErrorTo"];
                    errorEmail.recipentName = ConfigurationManager.AppSettings["SMTPErrorToName"];
                    Helpers.SendEmail(errorEmailClient, errorEmail, null);
                }

                LogHelper.Logger.WriteOutput(string.Concat("Finished - ", DateTime.Now.ToString()), EmailTransactionLog);

                Helpers.CreateEmailSender(out SmtpClient client, out email email, " - " + DateTime.Today.Day + "-" + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Today.Month) + "-" + DateTime.Today.Year);
                email.body = EmailTransactionLog.ToString();
                Helpers.SendEmail(client, email, null);

                Environment.Exit(1);
            }
        }
    }
}