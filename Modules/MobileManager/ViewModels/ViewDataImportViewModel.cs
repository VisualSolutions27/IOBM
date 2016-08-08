using Gijima.DataImport.MSOffice;
using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Events;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.MobileManager.Model.Models;
using Gijima.IOBM.MobileManager.Security;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewDataImportViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private ImportRuleDataModel _model = null;
        private IEventAggregator _eventAggregator;
        private SecurityHelper _securityHelper = null;
        private IEnumerable<ImportRuleData> _importRules = null;
        private MSOfficeHelper _officeHelper = null;

        #region Commands

        public DelegateCommand OpenFileCommand { get; set; }
        public DelegateCommand UpdateCommand { get; set; }

        #endregion

        #region Properties       

        /// <summary>
        /// The data import/update exception description
        /// </summary>
        public string ExceptionDescription
        {
            get { return _exceptionDescription; }
            set { SetProperty(ref _exceptionDescription, value); }
        }
        private string _exceptionDescription;

        /// <summary>
        /// The data import/update progessbar description
        /// </summary>
        public string ImportUpdateDescription
        {
            get { return _importUpdateDescription; }
            set { SetProperty(ref _importUpdateDescription, value); }
        }
        private string _importUpdateDescription;

        /// <summary>
        /// The data import/update progessbar value
        /// </summary>
        public int ImportUpdateProgress
        {
            get { return _ImportUpdateProgress; }
            set { SetProperty(ref _ImportUpdateProgress, value); }
        }
        private int _ImportUpdateProgress;

        /// <summary>
        /// The number of import/update data items
        /// </summary>
        public int ImportUpdateCount
        {
            get { return _importUpdateCount; }
            set { SetProperty(ref _importUpdateCount, value); }
        }
        private int _importUpdateCount;

        /// <summary>
        /// The number of imported/updated data that passed validation
        /// </summary>
        public int ImportUpdatesPassed
        {
            get { return _importUpdatesPassed; }
            set { SetProperty(ref _importUpdatesPassed, value); }
        }
        private int _importUpdatesPassed;

        /// <summary>
        /// The number of imported/updated data that failed
        /// </summary>
        public int ImportUpdatesFailed
        {
            get { return _importUpdatesFailed; }
            set { SetProperty(ref _importUpdatesFailed, value); }
        }
        private int _importUpdatesFailed;

        /// <summary>
        /// The selected exceptions
        /// </summary>
        public string SelectedExceptions
        {
            get { return _selectedExceptions; }
            set
            {
                SetProperty(ref _selectedExceptions, value);
                PopulateSelectedExceptions(value);
            }
        }
        private string _selectedExceptions;

        ///// <summary>
        ///// The current selected exception
        ///// </summary>
        //private ValidationRuleException SelectedException
        //{
        //    get { return _selectedException; }
        //    set { SetProperty(ref _selectedException, value); }
        //}
        //private ValidationRuleException _selectedException = null;

        ///// <summary>
        ///// The selected exceptions to fix
        ///// </summary>
        //private ObservableCollection<ValidationRuleException> ExceptionsToFix
        //{
        //    get { return _exceptionsToFix; }
        //    set { SetProperty(ref _exceptionsToFix, value); }
        //}
        //private ObservableCollection<ValidationRuleException> _exceptionsToFix = null;

        /// <summary>
        /// The collection of imported excel data
        /// </summary>
        public DataTable ImportedDataCollection
        {
            get { return _importDataCollection; }
            set { SetProperty(ref _importDataCollection, value); }
        }
        private DataTable _importDataCollection = null;

        #region View Lookup Data Collections

        /// <summary>
        /// The collection of data sheets from the selected Excel file
        /// </summary>
        public ObservableCollection<WorkSheetInfo> DataSheetCollection
        {
            get { return _dataSheetCollection; }
            set { SetProperty(ref _dataSheetCollection, value); }
        }
        private ObservableCollection<WorkSheetInfo> _dataSheetCollection = null;

        /// <summary>
        /// The collection of data sheet columns from the selected Excel sheet
        /// </summary>
        public ObservableCollection<string> SourceColumnCollection
        {
            get { return _sourceColumnCollection; }
            set { SetProperty(ref _sourceColumnCollection, value); }
        }
        private ObservableCollection<string> _sourceColumnCollection = null;

        /// <summary>
        /// The collection of data columns to import to from the DataImportColumn enum
        /// </summary>
        public ObservableCollection<string> DestinationColumnCollection
        {
            get { return _destinationColumnCollection; }
            set { SetProperty(ref _destinationColumnCollection, value); }
        }
        private ObservableCollection<string> _destinationColumnCollection = null;

        /// <summary>
        /// The collection of data entities to import to from the DataImportEntities enum
        /// </summary>
        public ObservableCollection<string> DestinationEntityCollection
        {
            get { return _destinationEntityCollection; }
            set { SetProperty(ref _destinationEntityCollection, value); }
        }
        private ObservableCollection<string> _destinationEntityCollection = null;

        #endregion

        #region Required Fields

        /// <summary>
        /// The selected data import file
        /// </summary>
        public string SelectedImportFile
        {
            get { return _selectedImportFile; }
            set { SetProperty(ref _selectedImportFile, value); }
        }
        private string _selectedImportFile = string.Empty;

        /// <summary>
        /// The selected data import sheet
        /// </summary>
        public WorkSheetInfo SelectedDataSheet
        {
            get { return _selectedImportSheet; }
            set
            {
                SetProperty(ref _selectedImportSheet, value);
                Task.Run(() => ImportWorkSheetDataAsync());               
            }
        }
        private WorkSheetInfo _selectedImportSheet;
        
        /// <summary>
        /// The selected source sheet column to import from
        /// </summary>
        public string SelectedSourceColumn
        {
            get { return _selectedSourceColumn; }
            set { SetProperty(ref _selectedSourceColumn, value); }
        }
        private string _selectedSourceColumn;

        /// <summary>
        /// The selected destination column to import to
        /// </summary>
        public string SelectedDestinationColumn
        {
            get { return _selectedDestinationColumn; }
            set
            {
                SetProperty(ref _selectedDestinationColumn, value);
                FilterImportRuleData(value);

                if (value == EnumHelper.GetDescriptionFromEnum(DataImportColumns.None))
                    ImportUpdateDescription = string.Format("Importing {0} - {1} of {2}", "", 0, 0);
                else
                    ImportUpdateDescription = string.Format("Importing {0} - {1} of {2}", value, 0, 0);
            }
        }
        private string _selectedDestinationColumn;

        /// <summary>
        /// The selected destination entity to import to
        /// </summary>
        public string SelectedDestinationEntity
        {
            get { return _selectedDestinationEntity; }
            set { SetProperty(ref _selectedDestinationEntity, value); }
        }
        private string _selectedDestinationEntity;

        #endregion

        #region Input Validation  
             
        /// <summary>
        /// Check if a valid data file was selected
        /// </summary>
        public bool ValidDataFile
        {
            get { return _validDataFile; }
            set { SetProperty(ref _validDataFile, value); }
        }
        private bool _validDataFile = false;

        /// <summary>
        /// Check if a valid data file was selected
        /// </summary>
        public bool ValidSelectedDataSheet
        {
            get { return _validSelectedDataSheet; }
            set { SetProperty(ref _validSelectedDataSheet, value); }
        }
        private bool _validSelectedDataSheet = false;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidImportFile
        {
            get { return _validImportFile; }
            set { SetProperty(ref _validImportFile, value); }
        }
        private Brush _validImportFile = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidDataSheet
        {
            get { return _validDataSheet; }
            set { SetProperty(ref _validDataSheet, value); }
        }
        private Brush _validDataSheet = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidSourceColumn
        {
            get { return _validSourceColumn; }
            set { SetProperty(ref _validSourceColumn, value); }
        }
        private Brush _validSourceColumn = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidDestinationColumn
        {
            get { return _validDestinationColumn; }
            set { SetProperty(ref _validDestinationColumn, value); }
        }
        private Brush _validDestinationColumn = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidDestinationEntity
        {
            get { return _validDestinationEntity; }
            set { SetProperty(ref _validDestinationEntity, value); }
        }
        private Brush _validDestinationEntity = Brushes.Red;

        /// <summary>
        /// Input validate error message
        /// </summary>
        public string Error
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Input validation properties
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public string this[string columnName]
        {
            get
            {
                string result = string.Empty;

                switch (columnName)
                {
                    case "SelectedImportFile":
                        ValidImportFile = string.IsNullOrEmpty(SelectedImportFile) ? Brushes.Red : Brushes.Silver;
                        ValidDataFile = string.IsNullOrEmpty(SelectedImportFile) ? false : true; break;
                    case "SelectedDataSheet":
                        ValidDataSheet = SelectedDataSheet == null || SelectedDataSheet.SheetName == "-- Please Select --" ? Brushes.Red : Brushes.Silver;
                        ValidSelectedDataSheet = SelectedDataSheet == null || SelectedDataSheet.SheetName == "-- Please Select --" ? false : true; break;
                    case "SelectedSourceColumn":
                        ValidSourceColumn = string.IsNullOrEmpty(SelectedSourceColumn) || SelectedSourceColumn == "-- Please Select --" ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedDestinationColumn":
                        ValidDestinationColumn = string.IsNullOrEmpty(SelectedDestinationColumn) || SelectedDestinationColumn == "-- Please Select --" ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedDestinationEntity":
                        ValidDestinationEntity = string.IsNullOrEmpty(SelectedDestinationEntity) || SelectedDestinationEntity == "-- Please Select --" ? Brushes.Red : Brushes.Silver; break;
                }

                return result;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Event Handlers

        /// <summary>
        /// This event gets received to update the progressbar
        /// </summary>
        /// <param name="sender">The error message.</param>
        private void ProgressBarInfo_Event(object sender)
        {
            Application.Current.Dispatcher.Invoke(() => { UpdateProgressBarValues(sender); });
        }

        /// <summary>
        /// This event gets received to display the data import/update result info
        /// </summary>
        /// <param name="sender">The error message.</param>
        private void DataImportUpdateResult_Event(object sender)
        {
            Application.Current.Dispatcher.Invoke(() => { DisplayDataImportUpdateResults(sender); });
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewDataImportViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _securityHelper = new SecurityHelper(eventAggregator);
            InitialiseDataImportView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseDataImportView()
        {
            _model = new ImportRuleDataModel(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands
            OpenFileCommand = new DelegateCommand(ExecuteOpenFileCommand);
            UpdateCommand = new DelegateCommand(ExecuteUpdate, CanUpdate).ObservesProperty(() => SelectedDataSheet)
                                                                         .ObservesProperty(() => SelectedSourceColumn)
                                                                         .ObservesProperty(() => SelectedDestinationColumn)
                                                                         .ObservesProperty(() => SelectedDestinationEntity);

            // Subscribe to this event to update the progressbar
            _eventAggregator.GetEvent<ProgressBarInfoEvent>().Subscribe(ProgressBarInfo_Event, true);

            // Subscribe to this event to display the data validation errors
            _eventAggregator.GetEvent<DataImportUpdateResultEvent>().Subscribe(DataImportUpdateResult_Event, true);

            // Load the view data
            ReadDataDestinationColumns();
            await ReadImportRuleDataAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            ExceptionDescription = "Data Import Exceptions";
            SelectedImportFile = string.Empty;
            ImportUpdateCount = 1;
            ImportUpdateProgress = ImportUpdatesPassed = ImportUpdatesFailed = 0;

            // Add the default items
            DataSheetCollection = new ObservableCollection<WorkSheetInfo>();
            WorkSheetInfo defaultInfo = new WorkSheetInfo();
            defaultInfo.SheetName = EnumHelper.GetDescriptionFromEnum(DataImportColumns.None);
            DataSheetCollection.Add(defaultInfo);
            SourceColumnCollection = new ObservableCollection<string>();
            SourceColumnCollection.Add(EnumHelper.GetDescriptionFromEnum(DataImportColumns.None));
            DestinationEntityCollection = new ObservableCollection<string>();
            DestinationEntityCollection.Add(EnumHelper.GetDescriptionFromEnum(DataImportColumns.None));
            SelectedDestinationEntity = EnumHelper.GetDescriptionFromEnum(DataImportColumns.None);
        }

        /// <summary>
        /// Read all the active import rule data from the database
        /// </summary>
        private async Task ReadImportRuleDataAsync()
        {
            try
            {
                _importRules = await Task.Run(() => _model.ReadImportRuleData(true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Add the data import or update error to the exception listview
        /// </summary>
        /// <param name="importUpdateResultInfo">The data import or update result info.</param>
        private void DisplayDataImportUpdateResults(object importUpdateResultInfo)
        {
            try
            {
                //if (ValidationErrorCollection == null)
                //    ValidationErrorCollection = new ObservableCollection<ValidationRuleException>();

                //ValidationRuleException resultInfo = (ValidationRuleException)validationResultInfo;

                //if (resultInfo.Result == true)
                //{
                //    ValidationRuleEntitiesPassed++;
                //}
                //else
                //{
                //    ValidationRuleEntitiesFailed++;
                //    ValidationErrorCollection.Add(resultInfo);
                //}
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Update the progressbar values
        /// </summary>
        /// <param name="progressBarInfo">The progressbar info.</param>
        private void UpdateProgressBarValues(object progressBarInfo)
        {
            try
            {
                ProgressBarInfo progressInfo = (ProgressBarInfo)progressBarInfo;
                ImportUpdateCount = progressInfo.MaxValue;
                ImportUpdateProgress = progressInfo.CurrentValue;
                ImportUpdateDescription = string.Format("Importing {0} - {1} of {2}", "", progressInfo.CurrentValue,
                                                                                          progressInfo.MaxValue);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Populate the selected exceptions to fix collection
        /// </summary>
        /// <param name="selectedExceptions">The Xceed CheckListBox selected items string.</param>
        private void PopulateSelectedExceptions(string selectedExceptions)
        {
            try
            {
                //ObservableCollection<ValidationRuleException> resultInfosCollection = new ObservableCollection<ValidationRuleException>();
                //ValidationRuleException resultInfo = null;

                //// The Xceed CheckListBox return all the selected items in
                //// a comma delimeted string, so get the number of selected 
                //// items we need to split the string
                //string[] exceptionsToFix = selectedExceptions.Split(',');

                //// Find the last selected exception based on the exception message
                //SelectedException = ValidationErrorCollection.Where(p => p.Message == exceptionsToFix.Last()).FirstOrDefault();

                //// Convert all the string values to DataValidationResultInfo
                //// and populate the ValidationErrorCollection
                //foreach (string exception in exceptionsToFix)
                //{
                //    resultInfo = ValidationErrorCollection.Where(p => p.Message == exception).FirstOrDefault();

                //    if (resultInfo != null)
                //    {
                //        resultInfosCollection.Add(resultInfo);
                //    }
                //}

                //ExceptionsToFix = new ObservableCollection<ValidationRuleException>(resultInfosCollection);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        #region Lookup Data Loading

        /// <summary>
        /// Populate the data source column combobox from the selected data sheet
        /// </summary>
        private void ReadDataSourceColumns()
        {
            if (SelectedDataSheet != null && SelectedDataSheet.ColumnNames != null)
            {
                SourceColumnCollection = new ObservableCollection<string>();
                SourceColumnCollection.Add(EnumHelper.GetDescriptionFromEnum(DataImportColumns.None));

                foreach (string columnName in SelectedDataSheet.ColumnNames)
                {
                    SourceColumnCollection.Add(columnName);
                }

                SelectedSourceColumn = EnumHelper.GetDescriptionFromEnum(DataImportColumns.None);
            }
        }

        /// <summary>
        /// Populate the data destination column combobox from the DataImportColumns enum
        /// </summary>
        private void ReadDataDestinationColumns()
        {
            DestinationColumnCollection = new ObservableCollection<string>();

            foreach (DataImportColumns source in Enum.GetValues(typeof(DataImportColumns)))
            {
                DestinationColumnCollection.Add(EnumHelper.GetDescriptionFromEnum(source));
            }
        }

        /// <summary>
        /// Filter the import destinations based on selected import rule source
        /// </summary>
        private void FilterImportRuleData(string enumDescription)
        {
            try
            {
                if (_importRules != null)
                {
                    DestinationEntityCollection.Clear();

                    // Convert enum description back to the enum
                    int importSourceID = EnumHelper.GetEnumFromDescription<DataImportColumns>(enumDescription).Value();

                    // Filter the import destination data based on the import source
                    IEnumerable<ImportRuleData> destinations = _importRules.Where(p => p.enImportSource == importSourceID).ToList();

                    // Add the default enum description
                    DestinationEntityCollection.Add(EnumHelper.GetDescriptionFromEnum(DataImportEntities.None));

                    // Add all the import destination enum decriptions to the collection
                    foreach (ImportRuleData rule in destinations)
                    {
                        DestinationEntityCollection.Add(EnumHelper.GetDescriptionFromEnum((DataImportEntities)rule.enImportDestination));
                    }

                    // Set the default value;
                    SelectedDestinationEntity = EnumHelper.GetDescriptionFromEnum(DataImportEntities.None);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Import the data from the selected workbook sheet
        /// </summary>
        private void ImportWorkSheetDataAsync()
        {
            try
            {
                if (SelectedDataSheet != null && SelectedDataSheet.WorkBookName != null)
                {
                    DataTable sheetData = null;
                    ExceptionDescription = "Data Import Exceptions";
                    ImportUpdateDescription = string.Format("Importing - {0} of {1}", 0, 0);
                    ImportUpdateProgress = ImportUpdatesPassed = ImportUpdatesFailed = 0;
                    ImportUpdateCount = SelectedDataSheet.RowCount;

                    if (_officeHelper == null)
                        _officeHelper = new MSOfficeHelper();
                    
                    // Import the worksheet data
                    sheetData = _officeHelper.ReadExcelIntoDataTable(SelectedDataSheet.WorkBookName, SelectedDataSheet.SheetName);

                    // This is to fake the progress bar for importing
                    for (int i = 1; i <= SelectedDataSheet.RowCount; i++)
                    {
                        Application.Current.Dispatcher.Invoke(() => 
                        {
                            ImportUpdateProgress = i;
                            ImportUpdatesPassed = i;
                            ImportUpdateDescription = string.Format("Importing - {0} of {1}", ImportUpdateProgress, SelectedDataSheet.RowCount);

                            if (i % 2 == 0)
                                Thread.Sleep(1);
                        });
                    }

                    ImportedDataCollection = sheetData;

                    // Read the data columns of the selected worksheet
                    ReadDataSourceColumns();
                }
            }
            catch (Exception ex)
            {
                ++ImportUpdatesFailed;
            }
        }

        #endregion

        #region Command Execution

        /// <summary>
        /// Execute when the open file command button is clicked 
        /// </summary>
        private void ExecuteOpenFileCommand()
        {
            try
            {
                System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
                dialog.Filter = "Excel files (*.xlsx)|*.xlsx";
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result.ToString() == "OK")
                {
                    MSOfficeHelper officeHelper = new MSOfficeHelper();
                    SelectedImportFile = dialog.FileName;
                    ValidDataFile = true;

                    // Get all the excel workbook sheets
                    List<WorkSheetInfo> workSheets = officeHelper.ReadWorkBookInfoFromExcel(dialog.FileName).ToList();

                    // Add all the workbook sheets
                    foreach (WorkSheetInfo sheetInfo in workSheets)
                    {
                        DataSheetCollection.Add(sheetInfo);
                    }

                    SelectedDataSheet = DataSheetCollection[0];
                }
            }
            catch (Exception ex)
            { }
        }

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanUpdate()
        {
            return SelectedDataSheet != null && SelectedDataSheet.SheetName != "-- Please Select --" &&
                   !string.IsNullOrEmpty(SelectedSourceColumn) && SelectedSourceColumn != EnumHelper.GetDescriptionFromEnum(DataImportColumns.None) &&
                   !string.IsNullOrEmpty(SelectedDestinationColumn) && SelectedDestinationColumn != EnumHelper.GetDescriptionFromEnum(DataImportColumns.None) &&
                   !string.IsNullOrEmpty(SelectedDestinationEntity) && SelectedDestinationEntity != EnumHelper.GetDescriptionFromEnum(DataImportEntities.None);
        }

        /// <summary>
        /// Execute when the start command button is clicked 
        /// </summary>
        private void ExecuteUpdate()
        {
        }

        #endregion

        #endregion
    }
}
