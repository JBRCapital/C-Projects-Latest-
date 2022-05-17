using System.Collections.Generic;
using System.Data;
using static DocxFromTemplateGenerator.GenerateDocument;
using word = Microsoft.Office.Interop.Word;

namespace AutoDocHelper
{
    public static class DocTemplateFunctions
    {

        public static void PostDocumentFunctions(DocTemplateHandler handler, Dictionary<string,DataTable> records)
        { 
            if (records.ContainsKey("transactions")) { insertTransactionHistory(handler, records["transactions"]); }
            if (records.ContainsKey("schedule")) { insertSchedule(handler, records["schedule"]); }
        }

        private static void insertTransactionHistory(DocTemplateHandler handler,DataTable transactions)
        {
            if (transactions == null) return;

            handler.InsertTableAtBookmark(transactions, new[]
            {
                new ColumnProperties
                {
                    Alignment = word.WdParagraphAlignment.wdAlignParagraphLeft,
                    Width = 77.04f
                },
                new ColumnProperties
                {
                    Alignment = word.WdParagraphAlignment.wdAlignParagraphLeft,
                    Width = 218.16f
                },
                new ColumnProperties
                {
                    Alignment = word.WdParagraphAlignment.wdAlignParagraphRight,
                    Width = 76.32f
                },
                new ColumnProperties
                {
                    Alignment = word.WdParagraphAlignment.wdAlignParagraphRight,
                    Width = 76.32f
                },
                new ColumnProperties
                {
                    Alignment = word.WdParagraphAlignment.wdAlignParagraphRight,
                    Width = 82.08f
                }
            });
        }

        private static void insertSchedule(DocTemplateHandler handler, DataTable schedule)
        {
            if (schedule == null) return;

            handler.InsertTableAtBookmark(schedule, new[]
            {
                new ColumnProperties
            {
                Alignment = word.WdParagraphAlignment.wdAlignParagraphLeft,
                Width = 113.76f
            },
                new ColumnProperties
            {
                Alignment = word.WdParagraphAlignment.wdAlignParagraphRight,
                Width = 120.0f
            }
            });
        }

    }
}
