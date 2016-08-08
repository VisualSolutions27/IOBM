using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gijima.DataImport.MSOffice
{
    public class MSOfficeHelper
    {
        /// <summary>
        /// Retrieve all the workbook sheet names from the specfied excel workbook
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
                WorkSheetInfo sheetInfo = null;

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
        /// Retrieve all the workbook sheet from the specfied excel workbook
        /// </summary>
        /// <param name="workBookName">The Excel file to retrieve the sheet from.</param>
        /// <returns>List of workbook sheets</returns>
        public IEnumerable<Sheet> ReadSheetsFromExcel(string workBookName)
        {
            // Open excel
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

        private WorkSheetInfo ReadSheetInfoFromExcel(string workBook, Sheet workSheet)
        {
            WorkSheetInfo sheetInfo = new WorkSheetInfo();

            try
            {
                List<string> columnNames = new List<string>();
                DateTime startTime = DateTime.Now;
                DataTable dt = ReadExcelIntoDataTable(workBook, workSheet.Name);
                DateTime endTime = DateTime.Now;
                TimeSpan duration = endTime - startTime;

                foreach (DataColumn column in dt.Columns)
                {
                    columnNames.Add(column.ColumnName.ToString());
                }

                sheetInfo.WorkBookName = workBook;
                sheetInfo.ColumnCount = dt.Columns.Count;
                sheetInfo.RowCount = dt.Rows.Count;
                sheetInfo.ImportDuration = (int)Math.Round(duration.TotalSeconds, 0);
                sheetInfo.ColumnNames = columnNames.ToList();

                return sheetInfo;
            }
            catch (Exception e)
            {
                throw new Exception(e.InnerException.Message);
            }
        }

        //public void ReadSheetInfoFromExcel(string fileName, string excelSheetName)
        //{
        //    bool isVersionConfirmed = false, isColumnConfirmed = false;
        //    int unconfirmedColumns = 0;

        //    // Initialise the loop
        //    while (true)
        //    {
        //        isColumnConfirmed = false;
        //        unconfirmedColumns = 0;

        //        using (SpreadsheetDocument document = SpreadsheetDocument.Open(fileName, false))
        //        {
        //            // Get the work book
        //            WorkbookPart workbookPart = document.WorkbookPart;
        //            Workbook workbook = workbookPart.Workbook;

        //            // Get the sheet - for the name
        //            Sheet sheet = workbook.Sheets.OfType<Sheet>().First((s) => s.Name == importer.Sheetname);
        //            WorksheetPart worksheetPart = workbookPart.GetPartById(sheet.Id) as WorksheetPart;

        //            // Get the workseet and its data - for the sheet
        //            Worksheet worksheet = worksheetPart.Worksheet;
        //            SheetData sheetData = worksheet.Elements<SheetData>().First();

        //            // Get the used dimensions of the worksheet
        //            int columnCount = worksheet.Elements<Columns>().First().Count();
        //            IEnumerable<Row> rows = sheetData.Elements<Row>();
        //            foreach (Row r in rows)
        //            {
        //                List<object> values = new List<object>();
        //                foreach (Cell cell in r.Elements<Cell>())
        //                {
        //                    string stringValue = cell.InnerText;
        //                    object value = stringValue;

        //                    if (cell.DataType != null && cell.DataType.HasValue)
        //                    {
        //                        switch (cell.DataType.Value)
        //                        {
        //                            case CellValues.Boolean:
        //                                value = stringValue == "0" ? false : true;
        //                                break;

        //                            case CellValues.Date:
        //                                value = DateTime.FromOADate(double.Parse(stringValue));
        //                                break;

        //                            case CellValues.Error:
        //                                break;

        //                            case CellValues.String:
        //                            case CellValues.InlineString:
        //                                value = stringValue;
        //                                break;

        //                            case CellValues.Number:
        //                                value = decimal.Parse(stringValue);
        //                                break;

        //                            case CellValues.SharedString:
        //                                // For shared strings, look up the value in the shared strings table.
        //                                var stringTable = workbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();

        //                                // If the shared string table is missing, something is wrong. Return the index that is in the cell.
        //                                // Otherwise, look up the correct text in  the table.
        //                                if (stringTable != null)
        //                                {
        //                                    value = stringTable.SharedStringTable.ElementAt(int.Parse(stringValue)).InnerText;
        //                                }
        //                                break;
        //                        }
        //                    }

        //                    values.Add(value);
        //                }

        //                object[] dataRow = values.ToArray();

        //                //switch (importer.currentSection)
        //                //{
        //                //	default:
        //                //		break;
        //                //}

        //                //switch (importer.State)
        //                //{
        //                //    default:
        //                //    case VolumeImportState.VolumeHeader1:
        //                //        // First, we expect to find a volume header row #1
        //                //        // If the line is empty, ignore it
        //                //        if (importer.IsEmptyLine(dataRow))
        //                //            break;

        //                //        // Get the content of columns 0, 1, and 3, which together somehow determine the spreadsheet version
        //                //        // It doesnt get more senseless than that!
        //                //        if (importer.ColumnOffset == 0)
        //                //        {
        //                //            // We are at the first column - we need to confirm the version (and dont allow any column offset)
        //                //            if (!isVersionConfirmed)
        //                //            {
        //                //                isVersionConfirmed = importer.CheckVersion(dataRow);
        //                //                if (!isVersionConfirmed)
        //                //                {
        //                //                    // Try an arbritrary 10 rows to confirm the version
        //                //                    if (importer.RowNo < 10)
        //                //                        break;

        //                //                    // If after 10 rows we still haven't found a volume header, give up for good, throw an exception
        //                //                    throw new ApplicException("{0}: Line {1} is not a first volume header #1", importer.Sheetname, importer.RowNo);
        //                //                }
        //                //            }
        //                //        }
        //                //        else
        //                //        {
        //                //            // We are at a subsequent column, and the version is already confirmed - we need to find the start of the next column
        //                //            if (!isColumnConfirmed)
        //                //            {
        //                //                isColumnConfirmed = importer.CheckVersion(dataRow);
        //                //                if (!isColumnConfirmed)
        //                //                    break;
        //                //            }
        //                //        }

        //                //        // We have a first header, reset the type, index, and year collections
        //                //        importer.ResetCollections();

        //                //        importer.ReadVolumeHeader(1, dataRow);
        //                //        break;

        //                //    case VolumeImportState.VolumeHeader2:
        //                //        // We expect to find a volume header row #2
        //                //        importer.ReadVolumeHeader(2, dataRow);
        //                //        break;

        //                //    case VolumeImportState.VolumeHeader3:
        //                //        // We expect to find a volume header row #3
        //                //        importer.ReadVolumeHeader(3, dataRow);
        //                //        break;

        //                //    case VolumeImportState.VolumeHeader4:
        //                //        // We expect to find a volume header row #4
        //                //        importer.ReadVolumeHeader(4, dataRow);
        //                //        break;

        //                //    case VolumeImportState.VolumeHeader5:
        //                //        // We expect to find a volume header row #5
        //                //        importer.ReadVolumeHeader(5, dataRow);
        //                //        break;

        //                //    case VolumeImportState.VolumeHeader6:
        //                //        // We expect to find a volume header row #6
        //                //        importer.ReadVolumeHeader(6, dataRow);
        //                //        break;

        //                //    case VolumeImportState.VolumeHeader7:
        //                //        // We expect to find a volume header row #7
        //                //        importer.ReadVolumeHeader(7, dataRow);
        //                //        break;

        //                //    case VolumeImportState.Document:
        //                //        // We expect to find a document row (or a footer)
        //                //        importer.ReadDocumentOrFooter(dataRow);
        //                //        break;
        //                }

        //                //importer.RowNo++;

        //                //// Try an arbritrary 10 rows to confirm the column
        //                //if (importer.ColumnOffset > 0 && !isColumnConfirmed && importer.RowNo >= 10)
        //                //{
        //                //    // If after 10 rows we still haven't found a volume header, give up with this column, increment by one
        //                //    break;
        //                //}
        //            }

        //            //// If we could not confirm the column, try the next one - trying and arbritrary 5 columns
        //            //if (importer.ColumnOffset > 0 && !isColumnConfirmed)
        //            //{
        //            //    unconfirmedColumns++;
        //            //    if (unconfirmedColumns > 5)
        //            //        throw new ApplicException("Unable to confirm a new column from columns {0} to {1}", importer.ColumnOffset - unconfirmedColumns, importer.ColumnOffset);

        //            //    // Test if we do have space for another column
        //            //    if (columnCount < importer.ColumnOffset + importer.ColumnsRequired)
        //            //        break;

        //            //    // Try the next column, maybe we can confirm it
        //            //    importer.ColumnOffset++;
        //            //}

        //            // Check if we have another column of data to process
        //            //else
        //            //{
        //            //    // Advance to the next column
        //            //    importer.ColumnOffset += importer.ColumnsRequired;

        //            //    // Test if we do have space for this next column
        //            //    if (importer.ColumnsRequired == 0 || columnCount < importer.ColumnOffset + importer.ColumnsRequired)
        //            //        break;
        //            //}

        //            //if (importer.State != VolumeImportState.VolumeHeader1)
        //            //    importer.Errors.Add(string.Format("{0}: Last line {1}/{2} was not a volume footer.", importer.Sheetname, importer.RowNo, importer.ColumnOffset));
        //        }
        //    }

        public DataTable ReadExcelIntoDataTable(string workBook, string sheetName)
        {
            try
            {
                using (OleDbConnection dbConnection = new OleDbConnection())
                {
                    DataTable dt = new DataTable();
                    dbConnection.ConnectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + workBook + ";" + "Extended Properties='Excel 12.0 Xml;HDR=YES;'";

                    using (OleDbCommand dbCommand = new OleDbCommand())
                    {
                        dbCommand.CommandText = "Select * from [" + sheetName + "$]";
                        dbCommand.Connection = dbConnection;

                        using (OleDbDataAdapter da = new OleDbDataAdapter())
                        {
                            da.SelectCommand = dbCommand;
                            da.Fill(dt);
                            return dt;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.InnerException.Message);
            }
        }
    }
}
