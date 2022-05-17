using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Security.Permissions;
using System.Xml.Linq;
using word = Microsoft.Office.Interop.Word;

namespace Docx2ClickSend
{
    public class DocTemplateHandler
    {
        private word._Application oWord;
        private word._Document oDoc;
        private readonly object oMissing = System.Reflection.Missing.Value;
        private const bool isVisible = false, readOnly = true;

        public DocTemplateHandler(string oInput)
        {
            try
            {
                oWord = new word.Application { Visible = isVisible };
                oDoc = oWord.Documents.Open(oInput, oMissing, readOnly, oMissing,
                    oMissing, oMissing, oMissing, oMissing,
                    oMissing, oMissing, oMissing, isVisible,
                    oMissing, oMissing, oMissing, oMissing);
                oDoc.Activate();
            }
            catch (Exception e)
            {
                throw new Exception($"Could not open the document specified {e.Message}");
            }
        }

        public void InsertTableAtBookmark(DataTable dt,
            ColumnProperties[] columnProperties)
        {
            try
            {
                word.Table newTable;
                word.Range wrdRng = oDoc.Bookmarks.get_Item(dt.TableName).Range;
                newTable = oDoc.Tables.Add(wrdRng, 1, dt.Columns.Count,
                    oMissing, oMissing);
                newTable.Borders.InsideLineStyle = word.WdLineStyle.wdLineStyleSingle;
                newTable.Borders.OutsideLineStyle = word.WdLineStyle.wdLineStyleSingle;
                newTable.AllowAutoFit = true;
                newTable.AllowPageBreaks = false;

                for (int i = 1; i <= dt.Rows[0].ItemArray.Length; i++)
                {
                    newTable.Cell(newTable.Rows.Count, i).Range.Text =
                        dt.Columns[i - 1].ColumnName;
                    newTable.Cell(newTable.Rows.Count, i)
                            .Range.ParagraphFormat.Alignment =
                        columnProperties[i - 1].Alignment;
                }

                StyleHeader(newTable.Rows[newTable.Rows.Count]);

                newTable.Rows.Add();

                for (int i = 1; i <= dt.Rows.Count; i++)
                {
                    for (int j = 1; j <= dt.Rows[i - 1].ItemArray.Length; j++)
                    {
                        newTable.Cell(newTable.Rows.Count, j).Range.Text =
                            dt.Rows[i - 1].ItemArray[j - 1].ToString();
                        newTable.Cell(newTable.Rows.Count, j)
                                .Range.ParagraphFormat.Alignment =
                            columnProperties[j - 1].Alignment;
                    }
                    StyleRow(newTable.Rows[newTable.Rows.Count]);
                    if (i < dt.Rows.Count)
                        newTable.Rows.Add();
                }

                newTable.Range.Font.Size = 11f;

                for (var i = 0; i < newTable.Columns.Count; i++)
                {
                    newTable.Columns[i + 1].Cells.VerticalAlignment =
                        word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;
                    newTable.Columns[i + 1].SetWidth(columnProperties[i].Width, word.WdRulerStyle.wdAdjustNone);
                }

                newTable.Rows[1].HeadingFormat = -1;
                newTable.ApplyStyleHeadingRows = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Close();
            }
        }

        public void ReplaceValues(Dictionary<string, string> valuesToReplace)
        {
            foreach (var kvp in valuesToReplace)
                FindAndReplace(kvp.Key, kvp.Value);
        }

        public void StyleRow(word.Row row)
        {
            row.Alignment = word.WdRowAlignment.wdAlignRowLeft;
            row.Height = 20.88f;
            row.Range.Bold = 0;
            row.Range.Shading.BackgroundPatternColor = word.WdColor.wdColorWhite;
        }

        public void StyleHeader(word.Row row)
        {
            row.Alignment = word.WdRowAlignment.wdAlignRowLeft;
            row.Height = 20.88f;
            row.Range.Bold = 1;
            row.Range.Shading.BackgroundPatternColor = word.WdColor.wdColorGray05;
        }

        public void Save(string oOutput, word.WdSaveFormat format = word.WdSaveFormat.wdFormatPDF)
        {
            oDoc.SaveAs(oOutput, format, oMissing, oMissing, oMissing,
                 oMissing, oMissing, oMissing, oMissing,
                 oMissing, oMissing, oMissing, oMissing, oMissing,
                 oMissing, oMissing);
        }

        public void DeletePage(int pageNumber)
        {
            word.Section sec = oDoc.Sections.Last;
            if (sec.PageSetup.SectionStart == word.WdSectionStart.wdSectionNewPage)
                sec.PageSetup.SectionStart = word.WdSectionStart.wdSectionContinuous;

            oDoc.Application.Selection.GoTo(word.WdGoToItem.wdGoToPage,
                word.WdGoToDirection.wdGoToAbsolute, 0, 3);
            oDoc.Application.Selection.Bookmarks[@"\Page"].Select();
            oDoc.Application.Selection.Text = "";
            oDoc.Application.Selection.Delete();

        }

        private void FindAndReplace(object findText, object replaceWithText)
        {
            //options
            object matchCase = false;
            object matchWholeWord = true;
            object matchWildCards = false;
            object matchSoundsLike = false;
            object matchAllWordForms = false;
            object forward = true;
            object format = false;
            object matchKashida = false;
            object matchDiacritics = false;
            object matchAlefHamza = false;
            object matchControl = false;
            object read_only = false;
            object visible = true;
            object replace = 2;
            object wrap = 1;
            //execute find and replace
            oWord.Selection.Find.Execute(ref findText, ref matchCase, ref matchWholeWord,
                ref matchWildCards, ref matchSoundsLike, ref matchAllWordForms, ref forward, ref wrap, ref format, ref replaceWithText, ref replace,
                ref matchKashida, ref matchDiacritics, ref matchAlefHamza, ref matchControl);
        }

        public void Close()
        {
            oWord.Quit(word.WdSaveOptions.wdDoNotSaveChanges,
                oMissing, oMissing);
        }
    }

    public class ColumnProperties
    {
        public word.WdParagraphAlignment Alignment { get; set; }
        public float Width { get; set; }
    }

    public static class GenerateAgreement
    {
        private static string UnregulatedTemplate = "JBR_UnregulatedAgreementNew.docx",
            RegulatedTemplateWithSchedule = "JBR_RegulatedAgreementNewWithScheduleOfPayment.docx",
            RegulatedTemplateWithoutSchedule = "JBR_RegulatedAgreementNew.docx";

        private static string DetailsTableBookmark = "DetailsTable",
            ScheduleTableBookmark = "ScheduleTable";

        public static void GeneratedRegulated(string basePath, string outputName, 
            Dictionary<string, string> dic, DataTable transactions, DataTable schedule = null)
        {
            DocTemplateHandler handler;
            if (schedule == null)
            {
                handler = new DocTemplateHandler(
                    Path.Combine(basePath, RegulatedTemplateWithoutSchedule));
                handler.ReplaceValues(dic);
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
            else
            {
                handler = new DocTemplateHandler(
                    Path.Combine(basePath, RegulatedTemplateWithSchedule));
                handler.ReplaceValues(dic);
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
            handler.Save(outputName);
            handler.Close();
        }

        public static void GeneratedUnRegulated(string basePath, string outputName,
            Dictionary<string, string> dic, DataTable dt)
        {
            var handler = new DocTemplateHandler(
                Path.Combine(basePath, UnregulatedTemplate));
            handler.ReplaceValues(dic);
            handler.InsertTableAtBookmark(dt, new[]
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
            handler.Save(outputName);
            handler.Close();
        }
    }
}
