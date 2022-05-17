using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using DB2Excel.DAL.Enum;
using DB2Excel.DAL.Obj;
using DB2Excel.Helpers;
using SMTPHelper;

namespace SendAmmortisation
{
    class Program
    {
        static void Main(string[] args)
        {
            string date = DateTime.Today.Year + "-" + DateTime.Today.Month + "-1";
            var Operations = new[]
            {
                new Operation(OperationType.SPROC, "dbo.AgreementsWithRepaymentAnomalies","Agreements W Repaymnt Anomalies"),
                new Operation(OperationType.SPROC, "dbo.Ammortisation", "Ammortisation", new List<string> { "'"+DateTime.Today.Year + "-" + DateTime.Today.Month + "-1"+"'" }),
                new Operation(OperationType.TABLE, "dbo.Ammortisation_Report", "Amortisation"),
                new Operation(OperationType.TABLE, "dbo.Ammortisation_ReportSettledLastMonth", "Amort. Settled Last Month"),
                new Operation(OperationType.TABLE, "dbo.Ammortisation_ReportSettledWithAmortisation", "Amort. Settled With Amort."),
                new Operation(OperationType.TABLE, "dbo.Ammortisation_ReportAllDefaulted", "Amort. All Defaulted"),
                new Operation(OperationType.TABLE, "dbo.Ammortisation_ReportDefaultedLastMonth", "Amort. Defaulted Last Month"),
                //new Operation(OperationType.TABLE, "dbo.Ammortisation_ReportDefaulteWithAmortisation", "Amort. Defaulted With Amort."),
                new Operation(OperationType.TABLE, "dbo.Ammortisation_Summary", "Amort. Summary"),
            };

            var fileName = ConfigurationManager.AppSettings["ExcelFilePath"].Replace(".", "-" + DateTime.Today.Month + "-" + DateTime.Today.Year + ".");

            ExcelHelper.ExportTables(fileName, null, Operations.Select(SqlHelper.RunOperation).ToArray());

            Console.WriteLine("Sending Email");

            Helpers.CreateEmailSender(out SmtpClient client, out email email, " - " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Today.Month) + "-" + DateTime.Today.Year);
            Helpers.SendEmail(client, email, fileName);

            Console.WriteLine("Email Sent");

            Environment.Exit(1);
        }
    }
}
