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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

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

                    if (_dataValidationGroup != DataValidationGroupName.None)
                    {
                        Task.Run(() => ReadDataEntitiesAsync());
                        Task.Run(() => ReadEnityPropertiesAsync());
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
                    case "SelectedEntityGroup":
                        ValidEntityGroup = string.IsNullOrEmpty(SelectedEntityGroup) || SelectedEntityGroup == "-- Please Select --" ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedDataEntity":
                        if (SelectedEntityGroup != null && SelectedEntityGroup != "-- Please Select --")
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
            ReadBillingProcesses();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            ValidationRuleCollection = null;
            SelectedDataEntity = null;
            SelectedDataProperty = null;
            SelectedOperator = null;
            SelectedValidationValue = string.Empty;
            DataEntityCollection = null;
            DataPropertyCollection = null;
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

        #region Lookup Data Loading

        /// <summary>
        /// Load all the billing processes from the  
        /// billing process enum's
        /// </summary>
        private void ReadBillingProcesses()
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
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
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

                foreach (DataValidationGroupName source in Enum.GetValues(typeof(DataValidationGroupName)))
                {
                    EntityGroupCollection.Add(EnumHelper.GetDescriptionFromEnum(source));
                }

                SelectedEntityGroup = EnumHelper.GetDescriptionFromEnum(DataValidationPropertyName.None);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
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
                    case DataValidationGroupName.Device:
                        DataEntityDisplayName = "MakeDescription";
                        //DataEntityCollection = new ObservableCollection<object>(new PackageModel(_eventAggregator).ReadPackages(true, true));
                        break;
                    case DataValidationGroupName.SimCard:
                        DataEntityDisplayName = "CellNumber";
                        DataEntityCollection = new ObservableCollection<object>(new SimCardModel(_eventAggregator).ReadSimCard(true, true));
                        break;
                    case DataValidationGroupName.StatusClient:
                        DataEntityDisplayName = "StatusDescription";
                        DataEntityCollection = new ObservableCollection<object>(new StatusModel(_eventAggregator).ReadStatuses(StatusLink.Contract, true, true));
                        break;
                }

                SelectedDataEntity = null;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
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
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
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
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
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
            return SelectedEntityGroup != "-- Please Select --";
        }

        /// <summary>
        /// Execute when the cancel command button is clicked 
        /// </summary>
        private void ExecuteCancel()
        {
            SelectedEntityGroup = "-- Please Select --";
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
