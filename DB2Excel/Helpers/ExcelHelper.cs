using System;
using System.Data;
using ClosedXML.Excel;

namespace DB2Excel.Helpers
{
    public static class ExcelHelper
    {
  
        public static void ExportTables(string destination, Action<IXLWorksheet> postProcess, params DataTable[] dts)
        {
            var wb = new XLWorkbook();
  
            foreach (var table in dts)
            {
                if (table == null) continue;
                
                var ws = wb.Worksheets.Add(table.TableName);

                var rngTable = ws.Range(1, 1, 1, table.Columns.Count).Style.Font.SetBold();
                ws.SheetView.FreezeRows(1);

                foreach (DataColumn column in table.Columns)
                {
                    ws.Cell(1, column.Ordinal+1).Value = column.ColumnName;
                }

                int rowCounter = 2;
                foreach (DataRow row in table.Rows)
                {
                   foreach (DataColumn column in table.Columns)
                   {  ws.Cell(rowCounter, column.Ordinal + 1).Value = row[column.ColumnName].ToString(); }
                    
                    rowCounter++;
                }


                foreach (DataColumn column in table.Columns)
                {
                    ws.Column(column.Ordinal + 1).AdjustToContents();
                }

                ws.RangeUsed().SetAutoFilter();
                
                postProcess?.Invoke(ws);
            }
            
            wb.SaveAs(destination);
        }
    }
}
