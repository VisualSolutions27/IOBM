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
        private DataValidationProcess _dataValidationProcess = DataValidationProcess.SingleSystemEntity;
        private DataValidationEntityName _dataValidationEntity = DataValidationEntityName.None;

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
                    switch (_dataValidationEntity)
                    {
                        case DataValidationEntityName.Client:
                            DataItemDisplayName = "PrimaryCellNumber";
                            foreach (Client entity in DataItemCollection)
                            {
                                if (entity.pkClientID == value.DataValidationEntityID)
                                {
                                    SelectedDataItem = entity;
                                    break;
                                }
                            }
                            break;
                        case DataValidationEntityName.Company:
                            DataItemDisplayName = "CompanyName";
                            foreach (Company entity in DataItemCollection)
                            {
                                if (entity.pkCompanyID == value.DataValidationEntityID)
                                {
                                    SelectedDataItem = entity;
                                    break;
                                }
                            }
                            break;
                    }

                    DataItemLabel = SelectedEntity.ToString();
                    SelectedDataProperty = DataPropertyCollection != null ? DataPropertyCollection.First(p => p.pkDataValidationPropertyID == value.fkDataValidationPropertyID) :
                                           DataPropertyCollection != null ? DataPropertyCollection.First(p => p.pkDataValidationPropertyID == 0) : null;

                    if (value.DataValidationProperty != null)
                    {
                        switch (((DataTypeName)value.DataValidationProperty.enDataType))
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
                    SetProperty(ref _selectedValidationRule, value);
                }
            }
        }
        private DataValidationRule _selectedValidationRule = new DataValidationRule();

        /// <summary>
        /// The data item display name based on the selected entity
        /// </summary>
        public string DataItemDisplayName
        {
            get { return _dataItemDisplayName; }
            set { SetProperty(ref _dataItemDisplayName, value); }
        }
        private string _dataItemDisplayName = string.Empty;

        /// <summary>
        /// The data item label based on the selected entity
        /// </summary>
        public string DataItemLabel
        {
            get { return _dataItemLabel; }
            set { SetProperty(ref _dataItemLabel, value); }
        }
        private string _dataItemLabel = "Data item:";

        /// <summary>
        /// The collection of validation rules from the database
        /// </summary>
        public ObservableCollection<DataValidationRule> ValidationRuleCollection
        {
            get { return _validationRuleCollection; }
            set { SetProperty(ref _validationRuleCollection, value); }
        }
        private ObservableCollection<DataValidationRule> _validationRuleCollection = null;

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
        private ObservableCollection<string> _processCollection = null;

        /// <summary>
        /// The collection of validation entities from the DataValidationEntity enum
        /// </summary>
        public ObservableCollection<string> EntityCollection
        {
            get { return _entityCollection; }
            set { SetProperty(ref _entityCollection, value); }
        }
        private ObservableCollection<string> _entityCollection = null;

        /// <summary>
        /// The collection of data items for the selected entity from the database
        /// </summary>
        public ObservableCollection<object> DataItemCollection
        {
            get { return _dataItemCollection; }
            set { SetProperty(ref _dataItemCollection, value); } 
        }
        private ObservableCollection<object> _dataItemCollection = null;

        /// <summary>
        /// The collection of company entities from the database
        /// </summary>
        public ObservableCollection<Company> CompanyCollection
        {
            get { return _companyCollection; }
            set { SetProperty(ref _companyCollection, value); }
        }
        private ObservableCollection<Company> _companyCollection = null;

        /// <summary>
        /// The collection of client entities from the database
        /// </summary>
        public ObservableCollection<Client> ClientCollection
        {
            get { return _clientCollection; }
            set { SetProperty(ref _clientCollection, value); }
        }
        private ObservableCollection<Client> _clientCollection = null;

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
        private ObservableCollection<DataValidationProperty> _dataPropertyCollection = null;

        /// <summary>
        /// The collection of operators types from the string, numeric
        /// and date OperatorType enum's
        /// </summary>
        public ObservableCollection<string> OperatorCollection
        {
            get { return _operatorCollection; }
            set { SetProperty(ref _operatorCollection, value); }
        }
        private ObservableCollection<string> _operatorCollection = null;

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
                _dataValidationProcess = EnumHelper.GetEnumFromDescription<DataValidationProcess>(value);

                //if (_dataValidationEntity != DataValidationEntityName.None)
                //{
                //    Task.Run(() => ReadEntityDataAsync());
                //    Task.Run(() => ReadEnityPropertiesAsync());
                //    Task.Run(() => ReadDataValidationRulesAsync());
                //}
                //else
                //{
                //    InitialiseViewControls();
                //}
            }
        }
        private string _selectedProcess = EnumHelper.GetDescriptionFromEnum(DataValidationProcess.SingleSystemEntity);

        /// <summary>
        /// The selected selected validation entity
        /// </summary>
        public string SelectedEntity
        {
            get { return _selectedEntity; }
            set
            {
                SetProperty(ref _selectedEntity, value);
                _dataValidationEntity = EnumHelper.GetEnumFromDescription<DataValidationEntityName>(value);

                if (_dataValidationEntity != DataValidationEntityName.None)
                {
                    Task.Run(() => ReadEntityDataAsync());
                    Task.Run(() => ReadEnityPropertiesAsync());
                    Task.Run(() => ReadDataValidationRulesAsync());
                }
                else
                {
                    InitialiseViewControls();
                }
            }
        }
        private string _selectedEntity = string.Empty;

        /// <summary>
        /// The selected validation entity data item
        /// </summary>
        public object SelectedDataItem
        {
            get { return _selectedDataItem; }
            set { SetProperty(ref _selectedDataItem, value); }
        }
        private object _selectedDataItem = null;

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
        public Brush ValidEntity
        {
            get { return _validEntity; }
            set { SetProperty(ref _validEntity, value); }
        }
        private Brush _validEntity = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidDataItem
        {
            get { return _validDataItem; }
            set { SetProperty(ref _validDataItem, value); }
        }
        private Brush _validDataItem = Brushes.Red;

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
                    case "SelectedEntity":
                        ValidEntity = string.IsNullOrEmpty(SelectedEntity) || SelectedEntity == "-- Please Select --" ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedDataItem":
                        if (SelectedEntity != null && SelectedEntity != "-- Please Select --")
                            switch (EnumHelper.GetEnumFromDescription<DataValidationEntityName>(SelectedEntity))
                            {
                                case DataValidationEntityName.Client:
                                    ValidDataItem = SelectedDataItem == null || ((Client)SelectedDataItem).pkClientID < 1 ? Brushes.Red : Brushes.Silver; break;
                                case DataValidationEntityName.Company:
                                    ValidDataItem = SelectedDataItem == null || ((Company)SelectedDataItem).pkCompanyID < 1 ? Brushes.Red : Brushes.Silver; break;
                            }
                        else
                            ValidDataItem = Brushes.Red;
                        break;
                    case "SelectedDataProperty":
                        ValidDataProperty = SelectedDataProperty == null || SelectedDataProperty.pkDataValidationPropertyID < 1 ? Brushes.Red : Brushes.Silver; break;
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
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecuteCancel).ObservesProperty(() => SelectedEntity);
            AddCommand = new DelegateCommand(ExecuteAdd, CanExecuteAdd);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave).ObservesProperty(() => SelectedEntity)
                                                                          .ObservesProperty(() => SelectedDataItem)
                                                                          .ObservesProperty(() => ValidDataProperty)
                                                                          .ObservesProperty(() => ValidOperator);

            // Load the view data
            ReadBillingProcesses();
            ReadDataValidationEntities();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            ValidationRuleCollection = null;
            SelectedDataItem = null;
            SelectedDataProperty = null;
            SelectedOperator = null;
            SelectedValidationValue = string.Empty;
        }

        /// <summary>
        /// Load all the validation rules based on the selected group from the database
        /// </summary>
        private async Task ReadDataValidationRulesAsync()
        {
            try
            {
                ValidationRuleCollection = await Task.Run(() => _model.ReadDataValidationRules(EnumHelper.GetEnumFromDescription<DataValidationEntityName>(SelectedEntity)));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
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
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Load all the validation rule groups from the database
        /// </summary>
        private void ReadDataValidationEntities()
        {
            try
            {
               EntityCollection = new ObservableCollection<string>();

                foreach (DataValidationEntityName source in Enum.GetValues(typeof(DataValidationEntityName)))
                {
                    EntityCollection.Add(EnumHelper.GetDescriptionFromEnum(source));
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Load all the lookup data for the selected entity from the database
        /// </summary>
        private void ReadEntityDataAsync()
        {
            try
            {
                ClientCollection = null;
                CompanyCollection = null;

                switch (_dataValidationEntity)
                {
                    case DataValidationEntityName.Client:
                        DataItemDisplayName = "PrimaryCellNumber";
                        DataItemCollection = new ObservableCollection<object>(new ClientModel(_eventAggregator).ReadClients(true));
                        break;
                    case DataValidationEntityName.Company:
                        DataItemDisplayName = "CompanyName";
                        DataItemCollection = new ObservableCollection<object>(new CompanyModel(_eventAggregator).ReadCompanies(true));
                        break;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
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
                                                                                                                                           .Where(p => p.enDataValidationEntity == _dataValidationEntity.Value()));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
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
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
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
            return SelectedEntity != "-- Please Select --";
        }

        /// <summary>
        /// Execute when the cancel command button is clicked 
        /// </summary>
        private void ExecuteCancel()
        {
            SelectedEntity = "-- Please Select --";
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
            return ValidEntity == Brushes.Silver && ValidDataItem == Brushes.Silver && ValidDataProperty == Brushes.Silver && ValidOperator == Brushes.Silver;
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

            SelectedValidationRule.enDataValidationEntity = _dataValidationEntity.Value();
            SelectedValidationRule.fkDataValidationPropertyID = SelectedDataProperty.pkDataValidationPropertyID;
            SelectedValidationRule.DataValidationValue = SelectedValidationValue.ToUpper();

            switch (_dataValidationEntity)
            {
                case DataValidationEntityName.Client:
                    SelectedValidationRule.DataValidationEntityID = ((Client)SelectedDataItem).pkClientID;
                    break;
                case DataValidationEntityName.Company:
                    SelectedValidationRule.DataValidationEntityID = ((Company)SelectedDataItem).pkCompanyID;
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
