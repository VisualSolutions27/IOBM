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
                    switch (((DataValidationEntityName)Enum.Parse(typeof(DataValidationEntityName), SelectedValidationEntity)))
                    {
                        case DataValidationEntityName.Client:
                            EntityDisplayName = "PrimaryCellNumber";
                            SelectedValidationData = new Client();
                            SelectedValidationData = ClientCollection != null && value.DataValidationEntityID > 0 ? ClientCollection.First(p => p.pkClientID == value.DataValidationEntityID) :
                                                                                                                    ClientCollection != null ? ClientCollection.First(p => p.pkClientID == 0) : null;
                            break;
                        case DataValidationEntityName.Company:
                            EntityDisplayName = "CompanyName";
                            foreach (Company entity in ValidationDataCollection)
                            {
                                if (entity.pkCompanyID == value.DataValidationEntityID)
                                {
                                    SelectedValidationData = entity;
                                    break;
                                }
                            }
                            break;
                    }

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
        /// The entity display name based on the selected category
        /// </summary>
        public string EntityDisplayName
        {
            get { return _entityDisplayName; }
            set { SetProperty(ref _entityDisplayName, value); }
        }
        private string _entityDisplayName = string.Empty;

        /// <summary>
        /// The entity label based on the selected category
        /// </summary>
        public string ValidationDataLabel
        {
            get { return _validationDataLabel; }
            set { SetProperty(ref _validationDataLabel, value); }
        }
        private string _validationDataLabel = "Data item:";

        ///// <summary>
        ///// The selected billing process
        ///// </summary>
        //public BillingProcess SelectedBillingProcess
        //{
        //    get { return _selectedBillingProcess; }
        //    set { SetProperty(ref _selectedBillingProcess, value); }
        //}
        //private BillingProcess _selectedBillingProcess = BillingProcess.DataValidation;

        /// <summary>
        /// The collection of validation rules from the database
        /// </summary>
        public ObservableCollection<BillingProcess> BillingProcessCollection
        {
            get { return _billingProcessCollection; }
            set { SetProperty(ref _billingProcessCollection, value); }
        }
        private ObservableCollection<BillingProcess> _billingProcessCollection = null;

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
        /// The collection of validation entities from the DataValidationEntity enum
        /// </summary>
        public ObservableCollection<string> ValidationEntityCollection
        {
            get { return _validationEntityCollection; }
            set { SetProperty(ref _validationEntityCollection, value); }
        }
        private ObservableCollection<string> _validationEntityCollection = null;

        /// <summary>
        /// The collection of data items for the selected entity from the database
        /// </summary>
        public ObservableCollection<object> ValidationDataCollection
        {
            get { return _validationDataCollection; }
            set { SetProperty(ref _validationDataCollection, value); } 
        }
        private ObservableCollection<object> _validationDataCollection = null;

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
        /// The collection of validation rule data from the database
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
        /// The selected selected validation entity
        /// </summary>
        public string SelectedValidationEntity
        {
            get { return _selectedValidationEntity; }
            set
            {
                SetProperty(ref _selectedValidationEntity, value);

                if (value != null && value != "-- Please Select --")
                {
                    Task.Run(() => ReadEntityDataAsync());
                    Task.Run(() => ReadEnityPropertiesAsync());
                    Task.Run(() => ReadDataValidationRulesAsync());
                }
                else
                {
                    ValidationDataCollection = null;
                    DataPropertyCollection = null;
                    OperatorCollection = null;
                    SelectedValidationValue = string.Empty;
                }
            }
        }
        private string _selectedValidationEntity = string.Empty;

        /// <summary>
        /// The selected validation entity data item
        /// </summary>
        public object SelectedValidationData
        {
            get { return _selectedValidationData; }
            set { SetProperty(ref _selectedValidationData, value); }
        }
        private object _selectedValidationData = null;

        /// <summary>
        /// The selected data validation property
        /// </summary>
        public DataValidationProperty SelectedDataProperty
        {
            get { return _selectedDataProperty; }
            set
            {
                SetProperty(ref _selectedDataProperty, value);

                if (value != null)
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
        public Brush ValidValidationEntity
        {
            get { return _validValidationEntity; }
            set { SetProperty(ref _validValidationEntity, value); }
        }
        private Brush _validValidationEntity = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidValidationData
        {
            get { return _validValidationData; }
            set { SetProperty(ref _validValidationData, value); }
        }
        private Brush _validValidationData = Brushes.Red;

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
                    case "SelectedValidationEntity":
                        ValidValidationEntity = string.IsNullOrEmpty(SelectedValidationEntity) || SelectedValidationEntity == "-- Please Select --" ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedValidationData":
                        if (SelectedValidationEntity != null && SelectedValidationEntity != "-- Please Select --")
                            switch (EnumHelper.GetEnumFromDescription<DataValidationEntityName>(SelectedValidationEntity))
                            {
                                case DataValidationEntityName.Client:
                                    ValidValidationData = SelectedValidationData == null || ((Client)SelectedValidationData).pkClientID < 1 ? Brushes.Red : Brushes.Silver; break;
                                case DataValidationEntityName.Company:
                                    ValidValidationData = SelectedValidationData == null || ((Company)SelectedValidationData).pkCompanyID < 1 ? Brushes.Red : Brushes.Silver; break;
                            }
                        else
                            ValidValidationData = Brushes.Red;
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
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecuteCancel).ObservesProperty(() => SelectedValidationEntity);
            AddCommand = new DelegateCommand(ExecuteAdd, CanExecuteAdd);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave).ObservesProperty(() => SelectedValidationEntity)
                                                                          .ObservesProperty(() => SelectedValidationData)
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

            // Add the default items
            ValidationEntityCollection = new ObservableCollection<string>();
            ValidationEntityCollection.Add(EnumHelper.GetDescriptionFromEnum(DataValidationEntityName.None));
            ValidationEntityCollection = new ObservableCollection<string>();
            ValidationEntityCollection.Add(EnumHelper.GetDescriptionFromEnum(DataValidationEntityName.None));
            //SelectedValidationData = null;
            //ValidDataProperty = null;
            //SelectedOperator = null;
            //SelectedValidationRule = new DataValidationRule();
            //SelectedValidationValue = string.Empty;
        }

        /// <summary>
        /// Load all the validation rules based on the selected group from the database
        /// </summary>
        private async Task ReadDataValidationRulesAsync()
        {
            try
            {
                ValidationRuleCollection = await Task.Run(() => _model.ReadDataValidationRules(EnumHelper.GetEnumFromDescription<DataValidationEntityName>(SelectedValidationEntity)));
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
                BillingProcessCollection = new ObservableCollection<BillingProcess>();

                foreach (BillingProcess process in Enum.GetValues(typeof(BillingProcess)))
                {
                    BillingProcessCollection.Add(process);
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
                ValidationEntityCollection = new ObservableCollection<string>();

                foreach (DataValidationEntityName source in Enum.GetValues(typeof(DataValidationEntityName)))
                {
                    ValidationEntityCollection.Add(EnumHelper.GetDescriptionFromEnum(source));
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

                switch (EnumHelper.GetEnumFromDescription<DataValidationEntityName>(SelectedValidationEntity))
                {
                    case DataValidationEntityName.Client:
                        EntityDisplayName = "PrimaryCellNumber";
                        ValidationDataCollection = new ObservableCollection<object>(new ClientModel(_eventAggregator).ReadClients(true));
                        break;
                    case DataValidationEntityName.Company:
                        EntityDisplayName = "CompanyName";
                        ValidationDataCollection = new ObservableCollection<object>(new CompanyModel(_eventAggregator).ReadCompanies(true));
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
                short entityID = ((DataValidationEntityName)Enum.Parse(typeof(DataValidationEntityName), SelectedValidationEntity)).Value();
                DataPropertyCollection = new ObservableCollection<DataValidationProperty>(new DataValidationPropertyModel(_eventAggregator).ReadDataValidationProperties(true)
                                                                                                                                           .Where(p => p.enDataValidationEntity == entityID));
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
                if (SelectedDataProperty.pkDataValidationPropertyID > 0)
                {
                    OperatorCollection = new ObservableCollection<string>();

                    switch ((DataTypeName)SelectedDataProperty.enDataType)
                    {
                        case DataTypeName.String:
                            foreach (StringOperator source in Enum.GetValues(typeof(StringOperator)))
                            {
                                OperatorCollection.Add(source.ToString());
                            }
                            break;
                        case DataTypeName.Bool:
                            OperatorCollection.Add("Equal");
                            break;
                        default:
                            foreach (NumericOperator source in Enum.GetValues(typeof(NumericOperator)))
                            {
                                if (source != NumericOperator.None)
                                    OperatorCollection.Add(source.ToString());
                            }
                            break;
                    }
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
            return SelectedValidationEntity != "-- Please Select --";
        }

        /// <summary>
        /// Execute when the cancel command button is clicked 
        /// </summary>
        private void ExecuteCancel()
        {
            SelectedValidationEntity = "-- Please Select --";
            ValidationRuleCollection = null;
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
            SelectedValidationRule = new DataValidationRule();
        }

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExecuteSave()
        {
            return ValidValidationEntity == Brushes.Silver && ValidValidationData == Brushes.Silver && ValidDataProperty == Brushes.Silver && ValidOperator == Brushes.Silver;
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

            SelectedValidationRule.enDataValidationEntity = ((DataValidationEntityName)Enum.Parse(typeof(DataValidationEntityName), SelectedValidationEntity)).Value();
            SelectedValidationRule.fkDataValidationPropertyID = SelectedDataProperty.pkDataValidationPropertyID;
            SelectedValidationRule.DataValidationValue = SelectedValidationValue.ToUpper();

            switch (((DataValidationEntityName)Enum.Parse(typeof(DataValidationEntityName), SelectedValidationEntity)))
            {
                case DataValidationEntityName.Client:
                    SelectedValidationRule.DataValidationEntityID = ((Client)SelectedValidationData).pkClientID;
                    break;
                case DataValidationEntityName.Company:
                    SelectedValidationRule.DataValidationEntityID = ((Company)SelectedValidationData).pkCompanyID;
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
