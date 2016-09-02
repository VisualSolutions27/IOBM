using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Gijima.DataImport.MSOffice
{
    public class MSOfficeHelper
    {
        /// <summary>
        /// Retrieve all the workbook sheet from the specfied excel workbook
        /// </summary>
        /// <param name="workBookName">The Excel file to retrieve the sheet from.</param>
        /// <returns>List of workbook sheets</returns>
        public IEnumerable<Sheet> ReadSheetsFromExcel(string workBookName)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(workBookName, false))
            {
                WorkbookPart workbookPart = document.WorkbookPart;
                Workbook workbook = workbookPart.Workbook;
                List<Sheet> sheets = new List<Sheet>();

                foreach (Sheet sheet in workbook.Sheets)
                {
                    sheets.Add(sheet);
                }

                return sheets;
            }
        }

        /// <summary>
        /// Retrieve the sheet data for all the sheets in the specified workbook
        /// </summary>
        /// <param name="workBookName">The Excel file to retrieve the sheet from.</param>
        /// <returns>List of workbook sheets</returns>
        public IEnumerable<WorkSheetInfo> ReadWorkBookInfoFromExcel(string workBookName)
        {
            using (SpreadsheetDocument document = SpreadsheetDocument.Open(workBookName, false))
            {
                WorkbookPart workbookPart = document.WorkbookPart;
                Workbook workbook = workbookPart.Workbook;
                List<WorkSheetInfo> workSheets = new List<WorkSheetInfo>();
                WorkSheetInfo sheetInfo = new WorkSheetInfo();

                foreach (Sheet sheet in workbook.Sheets)
                {
                    sheetInfo = ReadSheetInfoFromExcel(workBookName, sheet);
                    sheetInfo.SheetName = sheet.Name;
                    workSheets.Add(sheetInfo);
                }

                return workSheets;
            }
        }

        /// <summary>
        /// Retrieve the sheet info for the specified sheets in the specified workbook
        /// (ColCount, RowCount, ColNames)
        /// </summary>
        /// <param name="workBook">The Excel file to retrieve the sheet from.</param>
        /// <param name="workSheet">The workbook sheet to read data from.</param>
        /// <returns>WorkSheetInfo</returns>
        private WorkSheetInfo ReadSheetInfoFromExcel(string workBook, Sheet workSheet)
        {
            WorkSheetInfo sheetInfo = new WorkSheetInfo();
            List<string> columnNames = new List<string>();
            DateTime startTime = DateTime.Now;
            DateTime endTime = DateTime.Now;
            TimeSpan duration = endTime - startTime;

            try
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(workBook, false))
                {
                    // Get the work book
                    WorkbookPart workbookPart = document.WorkbookPart;
                    Workbook workbook = workbookPart.Workbook;

                    // Get the sheet - for the name
                    WorksheetPart worksheetPart = workbookPart.GetPartById(workSheet.Id) as WorksheetPart;

                    // Get the workseet and its data - for the sheet
                    Worksheet worksheet = worksheetPart.Worksheet;
                    SheetData sheetData = worksheet.Elements<SheetData>().First();

                    // Get all the sheet info
                    if (sheetData.Count() > 0)
                    {
                        IEnumerable<Row> rows = sheetData.Elements<Row>();

                        foreach (Cell cell in rows.ElementAt(0))
                        {
                            columnNames.Add(GetCellValue(document, cell));
                        }

                        sheetInfo.WorkBookName = workBook;
                        sheetInfo.ColumnCount = worksheet.Elements<Columns>().First().Count();
                        sheetInfo.RowCount = sheetData.Elements<Row>().Count();
                        sheetInfo.ImportDuration = (int)Math.Round(duration.TotalSeconds, 0);
                        sheetInfo.ColumnNames = columnNames.ToList();
                    }
                }

                return sheetInfo;
            }
            catch (Exception e)
            {
                throw new Exception(e.InnerException.Message);
            }
        }

        /// <summary>
        /// Retrieve the sheet data for the specified sheets in the specified workbook
        /// </summary>
        /// <param name="workBook">The Excel file to retrieve the sheet from.</param>
        /// <param name="workSheet">The workbook sheet to read data from.</param>
        /// <returns>DataTable</returns>
        public DataTable ReadSheetDataIntoDataTable(string workBook, string sheetName)
        {
            DataTable dt = new DataTable();

            using (SpreadsheetDocument document = SpreadsheetDocument.Open(workBook, false))
            {
                // Get the work book
                WorkbookPart workbookPart = document.WorkbookPart;
                Workbook workbook = workbookPart.Workbook;

                // Get the sheet - for the name
                Sheet sheet = workbook.Sheets.OfType<Sheet>().First((s) => s.Name == sheetName);
                WorksheetPart worksheetPart = workbookPart.GetPartById(sheet.Id) as WorksheetPart;

                // Get the workseet and its data - for the sheet
                Worksheet worksheet = worksheetPart.Worksheet;
                SheetData sheetData = worksheet.Elements<SheetData>().First();

                if (sheetData.Count() > 0)
                {
                    // Get the used dimensions of the worksheet
                    int columnCount = worksheet.Elements<Columns>().First().Count();
                    IEnumerable<Row> rows = sheetData.Elements<Row>();

                    // Create the data table from the sheet columns
                    foreach (Cell cell in rows.ElementAt(0))
                    {
                        dt.Columns.Add(GetCellValue(document, cell));
                    }

                    // Read the sheet data into the data table
                    foreach (Row row in rows)
                    {
                        DataRow dataRow = dt.NewRow();
                        for (int i = 0; i < row.Descendants<Cell>().Count(); i++)
                        {
                            dataRow[i] = GetCellValue(document, row.Descendants<Cell>().ElementAt(i));
                        }

                        dt.Rows.Add(dataRow);
                    }

                    // Remove the header row
                    dt.Rows.RemoveAt(0);
                }
            }

            return dt;
        }

        /// <summary>
        /// Get the value of the specified cell
        /// </summary>
        /// <param name="document">The spreadsheet containing the cell.</param>
        /// <param name="cell">The cell to get value of.</param>
        /// <returns>Cell Value</returns>
        private string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            string value = cell.CellValue.InnerXml;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
            else
                return value;
        }

        //public DataTable ReadExcelIntoDataTable(string workBook, string sheetName)
        //{
        //    try
        //    {
        //        using (OleDbConnection dbConnection = new OleDbConnection())
        //        {
        //            DataTable dt = new DataTable();
        //            dbConnection.ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + workBook + "; Extended Properties =\"Excel 12.0;HDR=YES;IMEX=1\"";

        //            using (OleDbCommand dbCommand = new OleDbCommand())
        //            {
        //                dbCommand.CommandText = "Select * from [" + sheetName + "$]";
        //                dbCommand.Connection = dbConnection;

        //                using (OleDbDataAdapter da = new OleDbDataAdapter())
        //                {
        //                    da.SelectCommand = dbCommand;
        //                    da.Fill(dt);
        //                    return dt;
        //                }
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.InnerException.Message);
        //    }
        //}
    }
}
