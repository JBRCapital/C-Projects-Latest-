using Docx2ClickSend;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using word = Microsoft.Office.Interop.Word;


namespace poc
{
    class Program
    {
        static void Main(string[] args)
        {
            string templatePath = @"C:\Users\andre\Desktop\New folder (4)\ClickSend\poc\bin\Debug\";
            string outputPath = @"C:\Users\andre\Desktop\New folder (4)\ClickSend\poc\bin\Debug\";

            GenerateAgreement.GeneratedRegulated(templatePath, 
                Path.Combine(outputPath, "regulatedWith.pdf"),
                GenerateDummyDictionary(), GenerateDummyTransactionTable(),
                GenerateDummyScheduleTable());

            GenerateAgreement.GeneratedRegulated(templatePath, 
                Path.Combine(outputPath, "regulatedWithout.pdf"),
                GenerateDummyDictionary(), GenerateDummyTransactionTable());

            GenerateAgreement.GeneratedUnRegulated(templatePath, 
                Path.Combine(outputPath, "unregulated.pdf"),
                GenerateDummyDictionary(), GenerateDummyScheduleTable());
        }

        private static Dictionary<string, string> GenerateDummyDictionary() => new Dictionary<string, string>()
        {
            {"<Address>", "12 sdasdasd Avenue \rCairo \rEgypt"},
            {"<Date>", DateTime.Now.ToString("dd/MM/yyyy")},
            {"<FullName>", "sdasdasd2"},
            {"<AgreementType>", "sdasdasd3"},
            {"<AgreementReference>", "sdasdasd4"},
            {"<AccountHolders>", "sdasdasd5"},
            {"<StatementPeriodStart>", "sdasdasd6"},
            {"<StatementPeriodEnd>", "sdasdasd7"},
            {"<RegistrationPlates>", "sdasdasd8"},
            {"<VehicleDetails>", "sdasdasd9"},
            {"<StartDateOfAgreement>", "sdasdasd10"},
            {"<AgreementTermInMonths>", "sdasdasd11"},
            {"<Advance>", "sdasdasd12"},
            {"<APR>", "sdasdasd13"},
        };

        private static DataTable GenerateDummyScheduleTable()
        {
            var dt = new DataTable("ScheduleTable");
            dt.Columns.AddRange(new[] {
                new DataColumn("Payment Due Date"),
                new DataColumn("Payment Amount £")
            });
            dt.Rows.Add(DateTime.Now.ToString("dd/MM/yyyy"), "£ 1000.12");
            dt.Rows.Add(DateTime.Now.ToString("dd/MM/yyyy"), "£ 1000.13");
            dt.Rows.Add(DateTime.Now.ToString("dd/MM/yyyy"), "£ 1000.14");
            dt.Rows.Add(DateTime.Now.ToString("dd/MM/yyyy"), "£ 1000.15");
            dt.Rows.Add(DateTime.Now.ToString("dd/MM/yyyy"), "£ 1000.16");
            dt.Rows.Add(DateTime.Now.ToString("dd/MM/yyyy"), "£ 1000.17");
            dt.Rows.Add(DateTime.Now.ToString("dd/MM/yyyy"), "£ 1000.18");
            dt.Rows.Add(DateTime.Now.ToString("dd/MM/yyyy"), "£ 1000.19");
            dt.Rows.Add(DateTime.Now.ToString("dd/MM/yyyy"), "£ 1000.20");
            dt.Rows.Add(DateTime.Now.ToString("dd/MM/yyyy"), "£ 1000.21");
            return dt;
        }

        private static DataTable GenerateDummyTransactionTable()
        {
            var dt = new DataTable("DetailsTable");
            dt.Columns.AddRange(new[]
            {
                new DataColumn("Date"),
                new DataColumn("Description"),
                new DataColumn("Debit £"),
                new DataColumn("Credit £"),
                new DataColumn("Balance £"),
            });
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            dt.Rows.Add("qweqwe", "qweqwe", "qweqwe", "sfsdf", "dvdfgfg");
            return dt;
        }
    }
}
