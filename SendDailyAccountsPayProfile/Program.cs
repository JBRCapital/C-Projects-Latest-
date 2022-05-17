using System;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Mail;
using ClosedXML.Excel;
using DB2Excel.DAL.Enum;
using DB2Excel.DAL.Obj;
using DB2Excel.Helpers;
using SMTPHelper;

namespace SendDailyAccountsPayProfile
{
    class Program
    {
        static void Main(string[] args)
        {

            var fileName = ConfigurationManager.AppSettings["ExcelFilePath"].Replace(".", " " + DateTime.Today.Day + "-" + DateTime.Today.Month + "-" + DateTime.Today.Year + ".");
            var Operations = new[]
            {
                new Operation(OperationType.FUNCTION, "dbo.Accounts_AgreementDataPayProfile","Agreement Pay Profiles"),
            };

            ExcelHelper.ExportTables(fileName, postProcess, Operations.Select(SqlHelper.RunOperation).ToArray());

            Console.WriteLine("Sending Email");

            SMTPHelper.Helpers.CreateEmailSender(out SmtpClient client, out email email, " - " + DateTime.Today.Day + "-" + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Today.Month) + "-" + DateTime.Today.Year);
            SMTPHelper.Helpers.SendEmail(client, email, fileName);

            Console.WriteLine("Email Sent");

            Environment.Exit(1);
        }

        static void postProcess(IXLWorksheet ws)
        {
            //ws.Column("D").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);
        }
    }
}
