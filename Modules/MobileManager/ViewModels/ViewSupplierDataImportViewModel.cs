using Gijima.DataImport.MSOffice;
using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.MobileManager.Common.Structs;
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
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewSupplierDataImportViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        //private ValidationRuleModel _model = null;
        private IEventAggregator _eventAggregator;
        private SecurityHelper _securityHelper = null;
        private MSOfficeHelper _officeHelper = null;
        private string _billingPeriod = string.Format("{0}{1}", DateTime.Now.Month.ToString().PadLeft(2, '0'), DateTime.Now.Year);

        #region Commands

        public DelegateCommand OpenDataFileCommand { get; set; }
        public DelegateCommand ApplyRuleFixCommand { get; set; }
        public DelegateCommand ManualFixCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// The data import progessbar description
        /// </summary>
        public string DataImportDescription
        {
            get { return _dataImportDescription; }
            set { SetProperty(ref _dataImportDescription, value); }
        }
        private string _dataImportDescription;

        /// <summary>
        /// The data import progessbar value
        /// </summary>
        public int DataImportProgress
        {
            get { return _dataImportProgress; }
            set { SetProperty(ref _dataImportProgress, value); }
        }
        private int _dataImportProgress;

        /// <summary>
        /// The page description to display on billing wizard page
        /// </summary>
        public string BillingWizardPageDescription
        {
            get { return _billingWizardPageDescription; }
            set { SetProperty(ref _billingWizardPageDescription, value); }
        }
        private string _billingWizardPageDescription = string.Empty;

        /// <summary>
        /// The instruction to display on billing wizard page
        /// </summary>
        public string BillingWizardPageInstruction
        {
            get { return _billingWizardPageInstruction; }
            set { SetProperty(ref _billingWizardPageInstruction, value); }
        }
        private string _billingWizardPageInstruction = string.Empty;

        /// <summary>
        /// The number of import data rows
        /// </summary>
        public int DataImportCount
        {
            get { return _dataImportCount; }
            set { SetProperty(ref _dataImportCount, value); }
        }
        private int _dataImportCount;

        /// <summary>
        /// The number of import rows that passed validation
        /// </summary>
        public int DataImportsPassed
        {
            get { return _dataImportPassed; }
            set { SetProperty(ref _dataImportPassed, value); }
        }
        private int _dataImportPassed;

        /// <summary>
        /// The number of import rows that failed validation
        /// </summary>
        public int DataImportsFailed
        {
            get { return _dataImportFailed; }
            set { SetProperty(ref _dataImportFailed, value); }
        }
        private int _dataImportFailed;

        /// <summary>
        /// The selected exceptions
        /// </summary>
        public string SelectedExceptions
        {
            get { return _selectedExceptions; }
            set
            {
                SetProperty(ref _selectedExceptions, value);
                //PopulateSelectedExceptions(value);
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

        /// <summary>
        /// Indicate if the current billing process completed
        /// </summary>
        public bool BillingProcessCompleted
        {
            get { return _billingProcessCompleted; }
            set { SetProperty(ref _billingProcessCompleted, value); }
        }
        private bool _billingProcessCompleted = false;

        ///// <summary>
        ///// The selected exceptions to fix
        ///// </summary>
        //private ObservableCollection<ValidationRuleException> ExceptionsToFix
        //{
        //    get { return _exceptionsToFix; }
        //    set { SetProperty(ref _exceptionsToFix, value); }
        //}
        //private ObservableCollection<ValidationRuleException> _exceptionsToFix = null;

        ///// <summary>
        ///// The collection of data validation exception Info
        ///// </summary>
        //public ObservableCollection<ValidationRuleException> ValidationErrorCollection
        //{
        //    get { return _validationErrorCollection; }
        //    set { SetProperty(ref _validationErrorCollection, value); }
        //}
        //private ObservableCollection<ValidationRuleException> _validationErrorCollection = null;

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
        /// The collection of imported excel data
        /// </summary>
        public DataTable ImportedDataCollection
        {
            get { return _importDataCollection; }
            set { SetProperty(ref _importDataCollection, value); }
        }
        private DataTable _importDataCollection = null;

        /// <summary>
        /// The collection of data import exception Info
        /// </summary>
        public ObservableCollection<string> DataImportErrorCollection
        {
            get { return _dataImportErrorCollection; }
            set { SetProperty(ref _dataImportErrorCollection, value); }
        }
        private ObservableCollection<string> _dataImportErrorCollection = null;

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
        /// Check if the data import was valid
        /// </summary>
        public bool ValidDataImport
        {
            get { return _validDataImport; }
            set { SetProperty(ref _validDataImport, value); }
        }
        private bool _validDataImport = false;

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
        /// Input validate error message
        /// </summary>
        public string Error
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public object Dispatcher { get; private set; }

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
                        ValidImportFile = string.IsNullOrEmpty(SelectedImportFile) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedDataSheet":
                        ValidDataSheet = SelectedDataSheet == null || SelectedDataSheet.SheetName == "-- Please Select --" ? Brushes.Red : Brushes.Silver;
                        ValidSelectedDataSheet = SelectedDataSheet == null || SelectedDataSheet.SheetName == "-- Please Select --" ? false : true; break;
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
            //Application.Current.Dispatcher.Invoke(() => { UpdateProgressBarValues(sender); });
        }

        /// <summary>
        /// This event gets received to display the validation result info
        /// </summary>
        /// <param name="sender">The error message.</param>
        private void DataValiationResult_Event(object sender)
        {
            //Application.Current.Dispatcher.Invoke(() => { DisplayDataValidationResults(sender); });
        }

        /// <summary>
        /// This event gets received to enable the next button 
        /// when the process completed
        /// </summary>
        /// <param name="sender">The error message.</param>
        private void BillingProcessCompleted_Event(BillingExecutionState sender)
        {
            if (sender == BillingExecutionState.DataValidation)
                BillingProcessCompleted = true;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewSupplierDataImportViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _securityHelper = new SecurityHelper(eventAggregator);
            InitialiseSupplierDataImportView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private void InitialiseSupplierDataImportView()
        {
            //_model = new ValidationRuleModel(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands
            OpenDataFileCommand = new DelegateCommand(ExecuteOpenDataFile, CanOpenDataFile).ObservesProperty(() => BillingProcessCompleted);
            //ApplyRuleFixCommand = new DelegateCommand(ExecuteApplyRuleFix, CanApplyRuleFix).ObservesProperty(() => ExceptionsToFix);
            //ManualFixCommand = new DelegateCommand(ExecuteManualFix, CanManualFix).ObservesProperty(() => ExceptionsToFix);

            //// Subscribe to this event to update the progressbar
            //_eventAggregator.GetEvent<ProgressBarInfoEvent>().Subscribe(ProgressBarInfo_Event, true);

            //// Subscribe to this event to display the data validation errors
            //_eventAggregator.GetEvent<DataValiationResultEvent>().Subscribe(DataValiationResult_Event, true);

            //// Subscribe to this event to lock the completed process
            //// and enable functionality to move to the process completed
            //_eventAggregator.GetEvent<BillingProcessCompletedEvent>().Subscribe(BillingProcessCompleted_Event, true);

            // Load the view data
            //ReadDataValidationEntitys();
            //await ReadValidationRuleExceptionsAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            DataImportDescription = string.Format("Importing - {0} of {1}", 0, 0);
            DataImportProgress = DataImportCount = DataImportsPassed = DataImportsFailed = 0;
            WorkSheetInfo defaultInfo = new WorkSheetInfo();
            defaultInfo.SheetName = EnumHelper.GetDescriptionFromEnum(DataUpdateColumn.None);
            DataSheetCollection = new ObservableCollection<WorkSheetInfo>();
            DataSheetCollection.Add(defaultInfo);
            //ValidationErrorCollection = null;
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
                    DataImportDescription = string.Format("Importing - {0} of {1}", 0, 0);
                    DataImportProgress = DataImportsPassed = DataImportsFailed = 0;
                    DataImportCount = SelectedDataSheet.RowCount;

                    if (_officeHelper == null)
                        _officeHelper = new MSOfficeHelper();

                    // Import the worksheet data
                    sheetData = _officeHelper.ReadExcelIntoDataTable(SelectedDataSheet.WorkBookName, SelectedDataSheet.SheetName);
                    string formula = "AmountEx + vatamt";
                    int total = 0;

                    // This is to fake the progress bar for importing
                    for (int i = 1; i <= SelectedDataSheet.RowCount; i++)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            DataImportProgress = i;
                            DataImportsPassed = i;
                            DataImportDescription = string.Format("Importing - {0} of {1}", DataImportProgress, SelectedDataSheet.RowCount);


                        });
                    }

                    ImportedDataCollection = sheetData;
                    ValidDataImport = true;
                }
            }
            catch (Exception ex)
            {
                if (DataImportErrorCollection == null)
                    DataImportErrorCollection = new ObservableCollection<string>();

                ++DataImportsFailed;
                DataImportErrorCollection.Add(string.Format("Error importing data sheet {0} - {1} {2}.", SelectedDataSheet.SheetName, ex.Message, ex.InnerException.Message));
            }
        }

        ///// <summary>
        ///// Load all the validation rule exceptions if any from the database
        ///// </summary>
        //private async Task ReadValidationRuleExceptionsAsync()
        //{
        //    try
        //    {
        //        ValidationErrorCollection = await Task.Run(() => new ValidationRuleExceptionModel(_eventAggregator).ReadValidationRuleExceptions(_billingPeriod));
        //    }
        //    catch (Exception ex)
        //    {
        //        _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
        //    }
        //}

        ///// <summary>
        ///// Add the data validation error to the validation exception listview
        ///// </summary>
        ///// <param name="resultInfo">The data validation result info.</param>
        //private void DisplayDataValidationResults(object validationResultInfo)
        //{
        //    try
        //    {
        //        if (ValidationErrorCollection == null)
        //            ValidationErrorCollection = new ObservableCollection<ValidationRuleException>();

        //        ValidationRuleException resultInfo = (ValidationRuleException)validationResultInfo;

        //        if (resultInfo.Result == true)
        //        {
        //            ValidationRuleEntitiesPassed++;
        //        }
        //        else
        //        {
        //            ValidationRuleEntitiesFailed++;
        //            ValidationErrorCollection.Add(resultInfo);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
        //    }
        //}

        ///// <summary>
        ///// Update the progressbar values
        ///// </summary>
        ///// <param name="progressBarInfo">The progressbar info.</param>
        //private void UpdateProgressBarValues(object progressBarInfo)
        //{
        //    try
        //    {
        //        ProgressBarInfo progressInfo = (ProgressBarInfo)progressBarInfo;
        //        ValidationRuleEntityCount = progressInfo.MaxValue;
        //        ValidationRuleEntityProgress = progressInfo.CurrentValue;
        //        ValidationRuleEntityDescription = string.Format("Validating data rule entity - {0} of {1}", ValidationRuleEntityProgress,
        //                                                                                                    progressInfo.MaxValue);
        //    }
        //    catch (Exception ex)
        //    {
        //        _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
        //    }
        //}

        ///// <summary>
        ///// Populate the selected exceptions to fix collection
        ///// </summary>
        ///// <param name="selectedExceptions">The Xceed CheckListBox selected items string.</param>
        //private void PopulateSelectedExceptions(string selectedExceptions)
        //{
        //    try
        //    {
        //        ObservableCollection<ValidationRuleException> resultInfosCollection = new ObservableCollection<ValidationRuleException>();
        //        ValidationRuleException resultInfo = null;

        //        // The Xceed CheckListBox return all the selected items in
        //        // a comma delimeted string, so get the number of selected 
        //        // items we need to split the string
        //        string[] exceptionsToFix = selectedExceptions.Split(',');

        //        // Find the last selected exception based on the exception message
        //        SelectedException = ValidationErrorCollection.Where(p => p.Message == exceptionsToFix.Last()).FirstOrDefault();

        //        // Convert all the string values to DataValidationResultInfo
        //        // and populate the ValidationErrorCollection
        //        foreach (string exception in exceptionsToFix)
        //        {
        //            resultInfo = ValidationErrorCollection.Where(p => p.Message == exception).FirstOrDefault();

        //            if (resultInfo != null)
        //            {
        //                resultInfosCollection.Add(resultInfo);
        //            }
        //        }

        //        ExceptionsToFix = new ObservableCollection<ValidationRuleException>(resultInfosCollection);
        //    }
        //    catch (Exception ex)
        //    {
        //        _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
        //    }
        //}

        ///// <summary>
        ///// Create a new billing process history entry
        ///// </summary>
        //private async Task CreateBillingProcessHistoryAsync()
        //{
        //    try
        //    {
        //        bool result = await Task.Run(() => new BillingProcessModel(_eventAggregator).CreateBillingProcessHistory(BillingExecutionState.DataValidation));

        //        // Publish this event to update the billing process history on the wizard's Info content
        //        _eventAggregator.GetEvent<BillingProcessHistoryEvent>().Publish(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
        //    }
        //}

        ///// <summary>
        ///// Create a new billing process history entry
        ///// </summary>
        ///// <param name="billingProcess">The billing process to complete.</param>/// 
        //private async Task CompleteBillingProcessHistoryAsync(BillingExecutionState billingProcess)
        //{
        //    try
        //    {
        //        bool result = await Task.Run(() => new BillingProcessModel(_eventAggregator).CompleteBillingProcessHistory(billingProcess, true));

        //        if (result)
        //        {
        //            // Publish this event to update the billing process history on the wizard's Info content
        //            _eventAggregator.GetEvent<BillingProcessHistoryEvent>().Publish(result);

        //            // Publish this event to lock the completed process and enable 
        //            // functinality to move to the next process
        //            if (billingProcess == BillingExecutionState.DataValidation)
        //                _eventAggregator.GetEvent<BillingProcessCompletedEvent>().Publish(BillingExecutionState.DataValidation);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
        //    }
        //}

        ///// <summary>
        ///// Save all the data rule exceptions to the database
        ///// </summary>
        //private async Task CreateValidationRuleExceptionsAsync()
        //{
        //    try
        //    {
        //        bool result = await Task.Run(() => new ValidationRuleExceptionModel(_eventAggregator).CreateValidationRuleExceptions(ValidationErrorCollection));
        //    }
        //    catch (Exception ex)
        //    {
        //        _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
        //    }
        //}

        #region Lookup Data Loading

        #endregion

        #region Command Execution

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanOpenDataFile()
        {
            return BillingProcessCompleted == false ? true : false;
        }

        /// <summary>
        /// Execute when the open file command button is clicked 
        /// </summary>
        private void ExecuteOpenDataFile()
        {
            try
            {
                System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
                dialog.Filter = "Excel files (*.xlsx)|*.xlsx";
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();
                ValidDataImport = false;

                if (result.ToString() == "OK")
                {
                    MSOfficeHelper officeHelper = new MSOfficeHelper();
                    SelectedImportFile = dialog.FileName;

                    // Get all the excel workbook sheets
                    List<WorkSheetInfo> workSheets = officeHelper.ReadWorkBookInfoFromExcel(dialog.FileName).ToList();

                    // Add all the workbook sheets
                    foreach (WorkSheetInfo sheetInfo in workSheets)
                    {
                        DataSheetCollection.Add(sheetInfo);
                    }

                    SelectedDataSheet = DataSheetCollection[0];
                    ValidDataFile = true;
                }
            }
            catch (Exception ex)
            { }
        }

        /// <summary>
        /// Execute when the start command button is clicked 
        /// </summary>
        private async void ExecuteStartDataImport()
        {
            InitialiseViewControls();

            //// Set the previous data validation process as complete
            //await CompleteBillingProcessHistoryAsync(BillingExecutionState.Started);

            //// Create a new history entry everytime the process get started
            //await CreateBillingProcessHistoryAsync();

            //// Set the group progressbar max value
            //ValidationGroupCount = ValidationGroupCollection.Count;

            //// Update the process progress values on the wizard's Info content
            //_eventAggregator.GetEvent<BillingProcessEvent>().Publish(BillingExecutionState.DataValidation);

            //// Disable the next buttton when the process gets started
            //_eventAggregator.GetEvent<BillingProcessStartedEvent>().Publish(BillingExecutionState.DataValidation);

            //foreach (string group in ValidationGroupCollection)
            //{
            //    // Read the validation rules for the specified
            //    // group from the database
            //    await ReadValidationRulesAsync(group);

            //    // Set the validation group progresssbar description
            //    ++ValidationGroupProgress;
            //    ValidationGroupDescription = string.Format("Validating data group {0} - {1} of {2}", group.ToUpper(),
            //                                                                                         ValidationGroupProgress,
            //                                                                                         ValidationGroupCollection.Count);
            //    if (ValidationRuleCollection.Count > 0)
            //    {
            //        int entityCount = ValidationRuleCollection.GroupBy(p => p.EntityID).Count();
            //        int entityID = 0;

            //        // Set the entity and data rule progressbars max values
            //        ValidationEntityCount = entityCount;
            //        ValidationDataRuleCount = ValidationRuleCollection.Count;

            //        foreach (ValidationRule rule in ValidationRuleCollection)
            //        {
            //            // entityID = rule.EntityID;

            //            // Validate the data rule and update
            //            // the progress values accodingly
            //            if (await Task.Run(() => _model.ValidateDataRule(rule)))
            //                ValidationDataRulesPassed++;
            //            else
            //                ValidationDataRulesFailed++;

            //            // Update progress values when the
            //            // group entity change
            //            if (entityID != rule.EntityID)
            //            {
            //                entityID = rule.EntityID;

            //                // Set the validation entity progresssbar description
            //                ++ValidationEntityProgress;
            //                ValidationEntityDescription = string.Format("Validating {0} {1} - {2} of {3}", group.ToLower(),
            //                                                                                               rule.EntityName.ToUpper(),
            //                                                                                               ValidationEntityProgress,
            //                                                                                               entityCount);
            //                // Update the group entity progress values
            //                if (ValidationDataRulesFailed == 0)
            //                    ValidationEntitiesPassed++;
            //                else
            //                    ValidationEntitiesFailed++;
            //            }

            //            // Set the data rule progresssbar description
            //            ++ValidationDataRuleProgress;
            //            ValidationDataRuleDescription = string.Format("Validating data rule {0} - {1} of {2}", rule.RuleDataName.ToUpper(),
            //                                                                                                   ValidationDataRuleProgress,
            //                                                                                                   ValidationRuleCollection.Count);
            //        }

            //        // Update the validation group 
            //        // progress values accodingly
            //        if (ValidationEntitiesFailed == 0)
            //            ValidationGroupsPassed++;
            //        else
            //            ValidationGroupsFailed++;

            //        // if NO validations exceptions found the set 
            //        // the data validation process as complete
            //        // else save the exceptions to the database
            //        if (ValidationErrorCollection.Count == 0)
            //            await CompleteBillingProcessHistoryAsync(BillingExecutionState.DataValidation);
            //        else
            //            await CreateValidationRuleExceptionsAsync();


            //    }
            //}
        }

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanApplyRuleFix()
        {
            return true;// ExceptionsToFix != null && ExceptionsToFix.Any(p => p.CanApplyRule == true) && !ExceptionsToFix.Any(p => p.CanApplyRule == false);
        }

        /// <summary>
        /// Execute when the apply rule command button is clicked 
        /// </summary>
        private async void ExecuteApplyRuleFix()
        {
            //try
            //{
            //    // Apply the data rule to each exception and remove the fixed
            //    // exceptions to the exceptions collection                  
            //    foreach (ValidationRuleException exception in ExceptionsToFix)
            //    {
            //        if (await Task.Run(() => _model.ApplyDataRule(exception)))
            //            ValidationErrorCollection.Remove(exception);
            //    }
            //}
            //catch (Exception ex)
            //{
            //    _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            //}
        }

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanManualFix()
        {
            return true;// ExceptionsToFix != null && ExceptionsToFix.Count == 1 && !ExceptionsToFix.Any(p => p.CanApplyRule == true);
        }

        /// <summary>
        /// Execute when the manual fix command button is clicked 
        /// </summary>
        private void ExecuteManualFix()
        {
            //if (_selectedException != null)
            //    _eventAggregator.GetEvent<SearchResultEvent>().Publish(_selectedException.EntityID);
        }

        #endregion

        #endregion
    }
}
