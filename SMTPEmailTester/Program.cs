using System;
using System.ComponentModel;
using System.Globalization;
using System.Net.Mail;
using SMTPHelper;

namespace SMTPEmailTester
{
    class Program
    { 

    //     //Call back function
    //public static void compEvent(object sender, AsyncCompletedEventArgs e)
    //{
    //    if (e.UserState != null)
    //        Console.Out.WriteLine(e.UserState.ToString());

    //    Console.Out.WriteLine("is it cancelled? " + e.Cancelled);

    //    if (e.Error != null)
    //        Console.Out.WriteLine("Error : " + e.Error.Message);
    //}

    static void Main(string[] args)
        {



            Helpers.CreateEmailSender(out SmtpClient client, out email email, " - " + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Today.Month) + "-" + DateTime.Today.Year);
            Helpers.SendEmail(client, email, null);
        }
    }
}
