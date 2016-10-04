using Gijima.DataImport.MSOffice;
using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.MobileManager.Model.Models;
using Gijima.IOBM.MobileManager.Security;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Threading;
using System.Collections.Generic;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewDataValidationCFViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private DataValidationRuleModel _model = null;
        private IEventAggregator _eventAggregator;
        private SecurityHelper _securityHelper = null;
        private DataValidationProcess _dataValidationProcess = DataValidationProcess.System;
        private DataValidationGroupName _dataValidationGroup = DataValidationGroupName.None;
        private MSOfficeHelper _officeHelper = null;
        private string _defaultItem = "-- Please Select --";

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the selected (current) validation rule entity
        /// </summary>
        public DataValidationRule SelectedValidationRule
        {
            get { return _selectedValidationRule; }
            set
            {
                SetProperty(ref _selectedValidationRule, value);

                if (value != null && value.enDataValidationEntity > 0)
                {
                    switch (_dataValidationGroup)
                    {
                        case DataValidationGroupName.Client:
                            DataEntityDisplayName = "PrimaryCellNumber";
                            foreach (Client entity in DataEntityCollection)
                            {
                                if (entity.pkClientID == value.DataValidationEntityID)
                                {
                                    SelectedDataEntity = entity;
                                    break;
                                }
                            }
                            break;
                        case DataValidationGroupName.Company:
                        case DataValidationGroupName.CompanyClient:
                            DataEntityDisplayName = "CompanyName";
                            foreach (Company entity in DataEntityCollection)
                            {
                                if (entity.pkCompanyID == value.DataValidationEntityID)
                                {
                                    SelectedDataEntity = entity;
                                    break;
                                }
                            }
                            break;
                    }

                    SelectedDataProperty = DataPropertyCollection != null ? DataPropertyCollection.First(p => p.pkDataValidationPropertyID == value.fkDataValidationPropertyID) :
                                           DataPropertyCollection != null ? DataPropertyCollection.First(p => p.pkDataValidationPropertyID == 0) : null;

                    if (SelectedDataProperty != null)
                    {
                        switch (EnumHelper.GetEnumFromDescription<DataTypeName>(value.DataTypeDescription))
                        {
                            case DataTypeName.String:
                                SelectedOperator = value != null && value.enDataValidationOperator > 0 ? ((StringOperator)value.enDataValidationOperator).ToString() : StringOperator.None.ToString();
                                break;
                            case DataTypeName.DateTime:
                                SelectedOperator = value != null && value.enDataValidationOperator > 0 ? ((DateOperator)value.enDataValidationOperator).ToString() : DateOperator.None.ToString();
                                break;
                            case DataTypeName.Integer:
                            case DataTypeName.Decimal:
                                SelectedOperator = value != null && value.enDataValidationOperator > 0 ? ((NumericOperator)value.enDataValidationOperator).ToString() : StringOperator.None.ToString();
                                break;
                            default:
                                SelectedOperator = "Equal";
                                break;
                        }
                    }
                    else if (OperatorCollection != null)
                    {
                        SelectedOperator = OperatorCollection.First();
                    }

                    SelectedValidationValue = value.DataValidationValue;
                }
            }
        }
        private DataValidationRule _selectedValidationRule = new DataValidationRule();

        /// <summary>
        /// The data entity display name based on the selected group
        /// </summary>
        public string DataEntityDisplayName
        {
            get { return _dataEntityDisplayName; }
            set { SetProperty(ref _dataEntityDisplayName, value); }
        }
        private string _dataEntityDisplayName = string.Empty;

        /// <summary>
        /// The data import progessbar description
        /// </summary>
        public string ImportUpdateDescription
        {
            get { return _importUpdateDescription; }
            set { SetProperty(ref _importUpdateDescription, value); }
        }
        private string _importUpdateDescription;

        /// <summary>
        /// The data import progessbar value
        /// </summary>
        public int ImportUpdateProgress
        {
            get { return _ImportUpdateProgress; }
            set { SetProperty(ref _ImportUpdateProgress, value); }
        }
        private int _ImportUpdateProgress;

        /// <summary>
        /// The number of import data items
        /// </summary>
        public int ImportUpdateCount
        {
            get { return _importUpdateCount; }
            set { SetProperty(ref _importUpdateCount, value); }
        }
        private int _importUpdateCount;

        /// <summary>
        /// The number of importd data that passed validation
        /// </summary>
        public int ImportUpdatesPassed
        {
            get { return _importUpdatesPassed; }
            set { SetProperty(ref _importUpdatesPassed, value); }
        }
        private int _importUpdatesPassed;

        /// <summary>
        /// The number of importd data that failed
        /// </summary>
        public int ImportUpdatesFailed
        {
            get { return _importUpdatesFailed; }
            set { SetProperty(ref _importUpdatesFailed, value); }
        }
        private int _importUpdatesFailed;

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
        /// The collection of data import exceptions
        /// </summary>
        public ObservableCollection<string> ExceptionsCollection
        {
            get { return _exceptionsCollection; }
            set { SetProperty(ref _exceptionsCollection, value); }
        }
        private ObservableCollection<string> _exceptionsCollection = null;

        /// <summary>
        /// The collection of validation rules from the database
        /// </summary>
        public ObservableCollection<DataValidationRule> ValidationRuleCollection
        {
            get { return _validationRuleCollection; }
            set { SetProperty(ref _validationRuleCollection, value); }
        }
        private ObservableCollection<DataValidationRule> _validationRuleCollection;

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
        /// The collection of data validation processes from
        /// the DataValidationProcess enum
        /// </summary>
        public ObservableCollection<string> ProcessCollection
        {
            get { return _processCollection; }
            set { SetProperty(ref _processCollection, value); }
        }
        private ObservableCollection<string> _processCollection;

        /// <summary>
        /// The collection of validation group entities from the DataValidationGroupName enum
        /// </summary>
        public ObservableCollection<string> EntityGroupCollection
        {
            get { return _entityGroupCollection; }
            set { SetProperty(ref _entityGroupCollection, value); }
        }
        private ObservableCollection<string> _entityGroupCollection;

        /// <summary>
        /// The collection of data entities for the selected group from the database
        /// </summary>
        public ObservableCollection<object> DataEntityCollection
        {
            get { return _dataEntityCollection; }
            set { SetProperty(ref _dataEntityCollection, value); } 
        }
        private ObservableCollection<object> _dataEntityCollection;

        /// <summary>
        /// The collection of company entities from the database
        /// </summary>
        public ObservableCollection<Company> CompanyCollection
        {
            get { return _companyCollection; }
            set { SetProperty(ref _companyCollection, value); }
        }
        private ObservableCollection<Company> _companyCollection;

        /// <summary>
        /// The collection of client entities from the database
        /// </summary>
        public ObservableCollection<Client> ClientCollection
        {
            get { return _clientCollection; }
            set { SetProperty(ref _clientCollection, value); }
        }
        private ObservableCollection<Client> _clientCollection;

        /// <summary>
        /// The collection of entity properties from the 
        /// DataValidationPropertyName enum linked to the
        /// configuration in the database
        /// </summary>
        public ObservableCollection<DataValidationProperty> DataPropertyCollection
        {
            get { return _dataPropertyCollection; }
            set { SetProperty(ref _dataPropertyCollection, value); }
        }
        private ObservableCollection<DataValidationProperty> _dataPropertyCollection;

        /// <summary>
        /// The collection of operators types from the string, numeric
        /// and date OperatorType enum's
        /// </summary>
        public ObservableCollection<string> OperatorCollection
        {
            get { return _operatorCollection; }
            set { SetProperty(ref _operatorCollection, value); }
        }
        private ObservableCollection<string> _operatorCollection;

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
        /// The selected data validation process
        /// </summary>
        public string SelectedProcess
        {
            get { return _selectedProcess; }
            set
            {
                SetProperty(ref _selectedProcess, value);
                InitialiseViewControls();
                EntityGroupCollection = null;
                _dataValidationProcess = EnumHelper.GetEnumFromDescription<DataValidationProcess>(value);

                if (_dataValidationProcess != DataValidationProcess.None)
                {
                    ReadDataValidationGroups();
                }
            }
        }
        private string _selectedProcess = EnumHelper.GetDescriptionFromEnum(DataValidationProcess.System);

        /// <summary>
        /// The selected validation entity group
        /// </summary>
        public string SelectedEntityGroup
        {
            get { return _selectedEntityGroup; }
            set
            {
                SetProperty(ref _selectedEntityGroup, value);
                InitialiseViewControls();

                if (value != null)
                {
                    _dataValidationGroup = EnumHelper.GetEnumFromDescription<DataValidationGroupName>(value);
                    DataValidationProcess selectedProcess = SelectedProcess != null ?
                                        EnumHelper.GetEnumFromDescription<DataValidationProcess>(SelectedProcess) :
                                        DataValidationProcess.None;

                    if (_dataValidationGroup != DataValidationGroupName.None)
                    {
                        Task.Run(() => ReadDataEntitiesAsync());
                        if (selectedProcess != DataValidationProcess.ExternalBilling)
                            Task.Run(() => ReadEnityPropertiesAsync());
                        else
                            ReadDataSourceColumns();
                        Task.Run(() => ReadDataValidationRulesAsync());
                    }
                }
            }
        }
        private string _selectedEntityGroup = EnumHelper.GetDescriptionFromEnum(DataValidationGroupName.None);

        /// <summary>
        /// The selected validation data entity
        /// </summary>
        public object SelectedDataEntity
        {
            get { return _selectedDataEntity; }
            set { SetProperty(ref _selectedDataEntity, value); }
        }
        private object _selectedDataEntity = null;

        /// <summary>
        /// The selected data validation property
        /// </summary>
        public DataValidationProperty SelectedDataProperty
        {
            get { return _selectedDataProperty; }
            set
            {
                SetProperty(ref _selectedDataProperty, value);
                ReadDataTypeOperators();
            }
        }
        private DataValidationProperty _selectedDataProperty = null;

        /// <summary>
        /// The selected operator
        /// </summary>
        public string SelectedOperator
        {
            get { return _selectedOperator; }
            set { SetProperty(ref _selectedOperator, value); }
        }
        private string _selectedOperator;

        /// <summary>
        /// The entered validation value
        /// </summary>
        public string SelectedValidationValue
        {
            get { return _selectedValidationValue; }
            set { SetProperty(ref _selectedValidationValue, value); }
        }
        private string _selectedValidationValue = string.Empty;

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
        public bool ValidImportData
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
        /// Check if a valid data file was selected
        /// </summary>
        public bool ValidSelectedDestinationEntity
        {
            get { return _validSelectedDestinationEntity; }
            set { SetProperty(ref _validSelectedDestinationEntity, value); }
        }
        private bool _validSelectedDestinationEntity = false;

        /// <summary>
        /// Check if the data can be imported
        /// </summary>
        public bool CanImport
        {
            get { return _canImport; }
            set { SetProperty(ref _canImport, value); }
        }
        private bool _canImport = false;

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
        public Brush ValidEntityGroup
        {
            get { return _validEntityGroup; }
            set { SetProperty(ref _validEntityGroup, value); }
        }
        private Brush _validEntityGroup = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidDataEntity
        {
            get { return _validDataEntity; }
            set { SetProperty(ref _validDataEntity, value); }
        }
        private Brush _validDataEntity = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidDataProperty
        {
            get { return _validDataProperty; }
            set { SetProperty(ref _validDataProperty, value); }
        }
        private Brush _validDataProperty = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidOperator
        {
            get { return _validOperator; }
            set { SetProperty(ref _validOperator, value); }
        }
        private Brush _validOperator = Brushes.Red;

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
                        ValidImportFile = string.IsNullOrEmpty(SelectedImportFile) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedDataSheet":
                        ValidDataSheet = SelectedDataSheet == null || SelectedDataSheet.SheetName == _defaultItem ? Brushes.Red : Brushes.Silver;
                        ValidSelectedDataSheet = SelectedDataSheet == null || SelectedDataSheet.SheetName == _defaultItem ? false : true; break;
                    case "SelectedEntityGroup":
                        ValidEntityGroup = string.IsNullOrEmpty(SelectedEntityGroup) || SelectedEntityGroup == _defaultItem ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedDataEntity":
                        if (SelectedEntityGroup != null && SelectedEntityGroup != _defaultItem)
                            switch (EnumHelper.GetEnumFromDescription<DataValidationGroupName>(SelectedEntityGroup))
                            {
                                case DataValidationGroupName.Client:
                                    ValidDataEntity = SelectedDataEntity != null && ((Client)SelectedDataEntity).pkClientID > 0 ? Brushes.Silver : Brushes.Red; break;
                                case DataValidationGroupName.CompanyClient:
                                case DataValidationGroupName.Company:
                                    ValidDataEntity = SelectedDataEntity != null && ((Company)SelectedDataEntity).pkCompanyID > 0 ? Brushes.Silver : Brushes.Red; break;
                            }
                        else
                            ValidDataEntity = Brushes.Red;
                        break;
                    case "SelectedDataProperty":
                        ValidDataProperty = SelectedDataProperty != null && SelectedDataProperty.pkDataValidationPropertyID > 0 ? Brushes.Silver : Brushes.Red; break;
                    case "SelectedOperator":
                        ValidOperator = string.IsNullOrEmpty(SelectedOperator) ? Brushes.Red : Brushes.Silver; break;
                }
                return result;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewDataValidationCFViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _securityHelper = new SecurityHelper(eventAggregator);
            InitialiseValidationCFView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private void InitialiseValidationCFView()
        {
            _model = new DataValidationRuleModel(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecuteCancel).ObservesProperty(() => SelectedEntityGroup);
            AddCommand = new DelegateCommand(ExecuteAdd, CanExecuteAdd);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave).ObservesProperty(() => SelectedEntityGroup)
                                                                          .ObservesProperty(() => SelectedDataEntity)
                                                                          .ObservesProperty(() => ValidDataProperty)
                                                                          .ObservesProperty(() => ValidOperator);

            // Load the view data
            ReadDataValidationProcesses();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            DataSheetCollection = new ObservableCollection<WorkSheetInfo>();
            WorkSheetInfo defaultInfo = new WorkSheetInfo();
            defaultInfo.SheetName = _defaultItem;
            SelectedDataSheet = defaultInfo;
            DataSheetCollection.Add(defaultInfo);
            ValidationRuleCollection = null;
            SelectedDataEntity = null;
            SelectedDataProperty = null;
            SelectedOperator = null;
            SelectedValidationValue = string.Empty;
            DataEntityCollection = null;
            DataPropertyCollection = null;
            InitialiseImportControls();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseImportControls()
        {
            ImportUpdateDescription = string.Format("Importing - {0} of {1}", 0, 0);
            ImportUpdateCount = 1;
            ImportUpdateProgress = ImportUpdatesPassed = ImportUpdatesFailed = 0;
            ValidImportData = false;
            ExceptionsCollection = null;
            ImportedDataCollection = null;
        }

        /// <summary>
        /// Load all the validation rules based on the selected group from the database
        /// </summary>
        private async Task ReadDataValidationRulesAsync()
        {
            try
            {
                ValidationRuleCollection = await Task.Run(() => _model.ReadDataValidationRules(_dataValidationProcess, _dataValidationGroup));
                SelectedValidationRule = null;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
            }
        }

        /// <summary>
        /// Import the data from the selected workbook sheet
        /// </summary>
        private void ImportWorkSheetDataAsync()
        {
            try
            {
                InitialiseImportControls();

                if (SelectedDataSheet != null && SelectedDataSheet.WorkBookName != null)
                {
                    DataTable sheetData = null;
                    ImportUpdateDescription = string.Format("Reading - {0} of {1}", 0, 0);
                    ImportUpdateProgress = ImportUpdatesPassed = ImportUpdatesFailed = 0;
                    ImportUpdateCount = SelectedDataSheet.RowCount;

                    if (_officeHelper == null)
                        _officeHelper = new MSOfficeHelper();

                    // Update the worksheet data
                    sheetData = _officeHelper.ReadSheetDataIntoDataTable(SelectedDataSheet.WorkBookName, SelectedDataSheet.SheetName);

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
                    ValidImportData = true;

                    // Read the data columns of the selected worksheet
                    ReadDataSourceColumns();
                }
            }
            catch (Exception ex)
            {
                if (ExceptionsCollection == null)
                    ExceptionsCollection = new ObservableCollection<string>();

                ++ImportUpdatesFailed;
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataImportViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ImportWorkSheetDataAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
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
                List<string> sheetColumns = new List<string>();
                sheetColumns.Add(_defaultItem);

                foreach (string columnName in SelectedDataSheet.ColumnNames)
                {
                    sheetColumns.Add(columnName);
                }

                //DataPropertyCollection = new ObservableCollection<string>(sheetColumns);
                //Application.Current.Dispatcher.Invoke(() => { SelectedSourceProperty = _defaultItem; });
            }
        }

        /// <summary>
        /// Load all the data validation processes from the  
        /// DataValidationProcess enum's
        /// </summary>
        private void ReadDataValidationProcesses()
        {
            try
            {
                ProcessCollection = new ObservableCollection<string>();

                foreach (DataValidationProcess process in Enum.GetValues(typeof(DataValidationProcess)))
                {
                    ProcessCollection.Add(EnumHelper.GetDescriptionFromEnum(process));
                }

                SelectedProcess = EnumHelper.GetDescriptionFromEnum(DataValidationProcess.None);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationCFViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadDataValidationProcesses",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Load all the validation entity groups from the DataValidationGroupName enum
        /// </summary>
        private void ReadDataValidationGroups()
        {
            try
            {
                EntityGroupCollection = new ObservableCollection<string>();
                DataValidationProcess selectedProcess = SelectedProcess != null ?
                                                        EnumHelper.GetEnumFromDescription<DataValidationProcess>(SelectedProcess) :
                                                        DataValidationProcess.None;

                foreach (DataValidationGroupName source in Enum.GetValues(typeof(DataValidationGroupName)))
                {
                    if (selectedProcess == DataValidationProcess.ExternalBilling &&
                        (source == DataValidationGroupName.None || source == DataValidationGroupName.ServiceProvider))
                        EntityGroupCollection.Add(EnumHelper.GetDescriptionFromEnum(source));
                    else if (selectedProcess != DataValidationProcess.ExternalBilling && source != DataValidationGroupName.ServiceProvider)
                        EntityGroupCollection.Add(EnumHelper.GetDescriptionFromEnum(source));
                }

                SelectedEntityGroup = _defaultItem;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationCFViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadDataValidationGroups",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Load all the data entities for the selected group from the database
        /// </summary>
        private void ReadDataEntitiesAsync()
        {
            try
            {
                ClientCollection = null;
                CompanyCollection = null;

                switch (EnumHelper.GetEnumFromDescription<DataValidationGroupName>(SelectedEntityGroup))
                {
                    case DataValidationGroupName.Client:
                        DataEntityDisplayName = "PrimaryCellNumber";
                        DataEntityCollection = new ObservableCollection<object>(new ClientModel(_eventAggregator).ReadClients(true));
                        break;
                    case DataValidationGroupName.Company:
                    case DataValidationGroupName.CompanyClient:
                        DataEntityDisplayName = "CompanyName";
                        DataEntityCollection = new ObservableCollection<object>(new CompanyModel(_eventAggregator).ReadCompanies(true, true));
                        break;
                    case DataValidationGroupName.Package:
                    case DataValidationGroupName.PackageClient:
                        DataEntityDisplayName = "PackageName";
                        DataEntityCollection = new ObservableCollection<object>(new PackageModel(_eventAggregator).ReadPackages(true, true));
                        break;
                    case DataValidationGroupName.StatusClient:
                        DataEntityDisplayName = "StatusDescription";
                        DataEntityCollection = new ObservableCollection<object>(new StatusModel(_eventAggregator).ReadStatuses(StatusLink.Contract, true, true));
                        break;
                    case DataValidationGroupName.SimCard:
                    case DataValidationGroupName.Device:
                        ValidDataEntity = Brushes.Silver;
                        SelectedDataEntity = 0;
                        break;
                    case DataValidationGroupName.ServiceProvider:
                        DataEntityDisplayName = "ServiceProviderName";
                        DataEntityCollection = new ObservableCollection<object>(new ServiceProviderModel(_eventAggregator).ReadServiceProviders(true, true));
                        break;
                }

                SelectedDataEntity = null;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationCFViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadDataEntitiesAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Load all the properties for the selected entity
        /// </summary>
        private void ReadEnityPropertiesAsync()
        {
            try
            {
                OperatorCollection = null;
                DataPropertyCollection = new ObservableCollection<DataValidationProperty>(new DataValidationPropertyModel(_eventAggregator).ReadDataValidationProperties(true)
                                                                                                                                           .Where(p => p.enDataValidationEntity == _dataValidationGroup.Value()));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationCFViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadEnityPropertiesAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Load all the data type operators from the string, 
        /// numeric and date OperatorType enum's
        /// </summary>
        private void ReadDataTypeOperators()
        {
            try
            {
                OperatorCollection = new ObservableCollection<string>();

                switch ((DataTypeName)SelectedDataProperty.enDataType)
                {
                    case DataTypeName.String:
                        foreach (StringOperator source in Enum.GetValues(typeof(StringOperator)))
                        {
                            OperatorCollection.Add(source.ToString());
                        }
                        SelectedOperator = StringOperator.None.ToString();
                        break;
                    case DataTypeName.Bool:
                        OperatorCollection.Add("Equal");
                        SelectedOperator = "Equal";
                        break;
                    default:
                        foreach (NumericOperator source in Enum.GetValues(typeof(NumericOperator)))
                        {
                            OperatorCollection.Add(source.ToString());
                        }
                        SelectedOperator = NumericOperator.None.ToString();
                        break;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewDataValidationCFViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadDataTypeOperators",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #endregion

        #region Command Execution

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteCancel()
        {
            return SelectedEntityGroup != _defaultItem;
        }

        /// <summary>
        /// Execute when the cancel command button is clicked 
        /// </summary>
        private void ExecuteCancel()
        {
            SelectedEntityGroup = _defaultItem;
        }

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteAdd()
        {
            return _securityHelper.IsUserInRole(SecurityRole.Administrator.Value());
        }

        /// <summary>
        /// Execute when the add command button is clicked 
        /// </summary>
        private void ExecuteAdd()
        {
            ExecuteCancel();
            SelectedValidationRule = new DataValidationRule();
        }

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteSave()
        {
            return ValidEntityGroup == Brushes.Silver && ValidDataEntity == Brushes.Silver && ValidDataProperty == Brushes.Silver && ValidOperator == Brushes.Silver;
        }

        /// <summary>
        /// Execute when the save command button is clicked 
        /// </summary>
        private async void ExecuteSave()
        {
            bool result = false;
            DataTypeName dataType = (DataTypeName)SelectedDataProperty.enDataType;

            if (SelectedValidationRule == null)
                SelectedValidationRule = new DataValidationRule();

            SelectedValidationRule.enValidationProcess = _dataValidationProcess.Value();
            SelectedValidationRule.enDataValidationEntity = _dataValidationGroup.Value();
            SelectedValidationRule.fkDataValidationPropertyID = SelectedDataProperty.pkDataValidationPropertyID;
            SelectedValidationRule.DataValidationValue = SelectedValidationValue.ToUpper();

            switch (_dataValidationGroup)
            {
                case DataValidationGroupName.Client:
                    SelectedValidationRule.DataValidationEntityID = ((Client)SelectedDataEntity).pkClientID;
                    break;
                case DataValidationGroupName.CompanyClient:
                case DataValidationGroupName.Company:
                    SelectedValidationRule.DataValidationEntityID = ((Company)SelectedDataEntity).pkCompanyID;
                    break;
                case DataValidationGroupName.Device:
                case DataValidationGroupName.SimCard:
                    SelectedValidationRule.DataValidationEntityID = 0;
                    break;
            }

            SelectedValidationRule.enDataValidationOperator = 0;
            if (dataType == DataTypeName.String)
                SelectedValidationRule.enDataValidationOperator = ((StringOperator)Enum.Parse(typeof(StringOperator), SelectedOperator)).Value();
            else if (dataType == DataTypeName.DateTime)
                SelectedValidationRule.enDataValidationOperator = ((DateOperator)Enum.Parse(typeof(DateOperator), SelectedOperator)).Value();
            else
                SelectedValidationRule.enDataValidationOperator = ((NumericOperator)Enum.Parse(typeof(NumericOperator), SelectedOperator)).Value();

            SelectedValidationRule.ModifiedBy = SecurityHelper.LoggedInDomainName;
            SelectedValidationRule.ModifiedDate = DateTime.Now;

            if (SelectedValidationRule.pkDataValidationRuleID == 0)
                result = _model.CreateDataValidationRule(SelectedValidationRule);
            else
                result = _model.UpdateDataValidationRule(SelectedValidationRule);

            if (result)
            {
                InitialiseViewControls();
                await ReadDataValidationRulesAsync();
            }
        }

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteMaintenace()
        {
            return true;
        }

        #endregion

        #endregion
    }
}
