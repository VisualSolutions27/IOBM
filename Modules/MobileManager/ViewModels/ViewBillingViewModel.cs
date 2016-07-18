using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Security;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewBillingViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        //private ValidationRuleModel _model = null;
        private IEventAggregator _eventAggregator;
        private SecurityHelper _securityHelper = null;

        #region Commands

        public DelegateCommand ResetCommand { get; set; }
        public DelegateCommand AcceptCommand { get; set; }
        public DelegateCommand NextCommand { get; set; }
        public DelegateCommand BackCommand { get; set; }
        public DelegateCommand FinishCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the selected (current) validation rule entity
        /// </summary>
        //public ValidationRule SelectedValidationRule
        //{
        //    get { return _selectedValidationRule; }
        //    set
        //    {
        //        SetProperty(ref _selectedValidationRule, value);

        //        if (value != null && value.enValidationRuleGroup > 0)
        //        {
        //            switch (((ValidationRuleGroup)Enum.Parse(typeof(ValidationRuleGroup), SelectedCategory)))
        //            {
        //                case ValidationRuleGroup.Client:
        //                    EntityDisplayName = "PrimaryCellNumber";
        //                    SelectedEntity = new Client();
        //                    SelectedEntity = ClientCollection != null && value.EntityID > 0 ? ClientCollection.First(p => p.pkClientID == value.EntityID) :
        //                                     ClientCollection != null ? ClientCollection.First(p => p.pkClientID == 0) : null;
        //                    break;
        //                case ValidationRuleGroup.Company:
        //                    EntityDisplayName = "CompanyName";
        //                    foreach (Company entity in EntityCollection)
        //                    {
        //                        if (entity.pkCompanyID == value.EntityID)
        //                        {
        //                            SelectedEntity = entity;
        //                            break;
        //                        }
        //                    }
        //                    break;
        //            }

        //            SelectedRuleData = RuleDataCollection != null ? RuleDataCollection.First(p => p.pkValidationRulesDataID == value.fkValidationRulesDataID) :
        //                               RuleDataCollection != null ? RuleDataCollection.First(p => p.pkValidationRulesDataID == 0) : null;

        //            if (value.RuleDataTypeName != null)
        //            {
        //                switch (((ValidationDataType)Enum.Parse(typeof(ValidationDataType), value.RuleDataTypeName)))
        //                {
        //                    case ValidationDataType.String:
        //                        SelectedOperator = value != null && value.enStringCompareType > 0 ? ((StringOperator)value.enStringCompareType).ToString() : StringOperator.None.ToString();
        //                        break;
        //                    case ValidationDataType.Bool:
        //                        SelectedOperator = "Equal";
        //                        break;
        //                    default:
        //                        SelectedOperator = value != null && value.enNumericCompareType > 0 ? ((NumericOperator)value.enStringCompareType).ToString() : StringOperator.None.ToString();
        //                        break;
        //                }
        //            }
        //            else if (OperatorCollection != null)
        //            {
        //                SelectedOperator = OperatorCollection.First();
        //            }

        //            SelectedValidationValue = value.ValidationDataValue;
        //            SetProperty(ref _selectedValidationRule, value);
        //        }
        //    }
        //}
        //private ValidationRule _selectedValidationRule = new ValidationRule();     

        /// <summary>
        /// The default billing period
        /// </summary>
        public DateTime DefaultBillingPeriod
        {
            get { return _defaultBillingPeriod; }
            set { SetProperty(ref _defaultBillingPeriod, value); }
        }
        private DateTime _defaultBillingPeriod = DateTime.Now;

        /// <summary>
        /// The selected billing period
        /// </summary>
        public string SelectedBillingPeriod
        {
            get { return _selectedBillingPeriod; }
            set { SetProperty(ref _selectedBillingPeriod, value); }
        }
        private string _selectedBillingPeriod = string.Format("{0} {1}", DateTime.Now.Month.ToString().PadLeft(2, '0'), DateTime.Now.Year);

        /// <summary>
        /// The number of pages in the billing wizard
        /// </summary>
        public int BillingWizardPageCount
        {
            get { return _billingWizardPageCount; }
            set { SetProperty(ref _billingWizardPageCount, value); }
        }
        private int _billingWizardPageCount = 1;

        /// <summary>
        /// The number of selected billing processes
        /// </summary>
        public int BillingProcessCount
        {
            get { return _billingProcessCount; }
            set { SetProperty(ref _billingProcessCount, value); }
        }
        private int _billingProcessCount = 0;
        
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
        /// The billing wizard progress description
        /// </summary>
        public string BillingWizardProgress
        {
            get { return _billingWizardProgress; }
            set { SetProperty(ref _billingWizardProgress, value); }
        }
        private string _billingWizardProgress = string.Format("Billing step - {0} of {1}", 1, 1);

        /// <summary>
        /// The billing wizard progress description
        /// </summary>
        public string BillinProcessProgress
        {
            get { return _billinProcessProgress; }
            set { SetProperty(ref _billinProcessProgress, value); }
        }
        private string _billinProcessProgress = string.Format("Executing Process - {0} of {1}", 0, 0);

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
        /// Indicate if the billing run started
        /// </summary>
        public bool BillingRunStarted
        {
            get { return _billingRunStarted; }
            set { SetProperty(ref _billingRunStarted, value); }
        }
        private bool _billingRunStarted = false;

        /// <summary>
        /// Indicate if the billing options can be changed
        /// </summary>
        public bool CanBillingOptionsChange
        {
            get { return _canBillingOptionsChange; }
            set { SetProperty(ref _canBillingOptionsChange, value); }
        }
        private bool _canBillingOptionsChange = true;
       
        /// <summary>
        /// The collection of billing processes from the database
        /// </summary>
        public ObservableCollection<BillingProcess> BillingProcessCollection
        {
            get { return _billingProcessCollection; }
            set { SetProperty(ref _billingProcessCollection, value); }
        }
        private ObservableCollection<BillingProcess> _billingProcessCollection = null;

        #region View Lookup Data Collections

        ///// <summary>
        ///// The collection of operators types from the string, numeric
        ///// and date OperatorType enum's
        ///// </summary>
        //public ObservableCollection<string> OperatorCollection
        //{
        //    get { return _operatorCollection; }
        //    set { SetProperty(ref _operatorCollection, value); }
        //}
        //private ObservableCollection<string> _operatorCollection = null;

        #endregion

        #region Required Fields

        /// <summary>
        /// The selected billing month
        /// </summary>
        public int SelectedBillingMonth
        {
            get { return _selectedBillingMonth; }
            set { SetProperty(ref _selectedBillingMonth, value); }
        }
        private int _selectedBillingMonth = DateTime.Now.Month;

        /// <summary>
        /// The selected billing year
        /// </summary>
        public int SelectedBillingYear
        {
            get { return _selectedBillingYear; }
            set { SetProperty(ref _selectedBillingYear, value); }
        }
        private int _selectedBillingYear = DateTime.Now.Year;

        /// <summary>
        /// The selected billing processes
        /// </summary>
        public string SelectedBillingProcesses
        {
            get { return _billingProcessSelected; }
            set { SetProperty(ref _billingProcessSelected, value); }
        }
        private string _billingProcessSelected;

        ///// <summary>
        ///// The selected validation rule data
        ///// </summary>
        //public ValidationRulesData SelectedRuleData
        //{
        //    get { return _selectedRuleData; }
        //    set
        //    {
        //        SetProperty(ref _selectedRuleData, value);

        //        if (value != null)
        //            ReadDataTypeOperators();
        //    }
        //}
        //private ValidationRulesData _selectedRuleData = null;

        ///// <summary>
        ///// The selected operator
        ///// </summary>
        //public string SelectedOperator
        //{
        //    get { return _selectedOperator; }
        //    set { SetProperty(ref _selectedOperator, value); }
        //}
        //private string _selectedOperator;

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the validation value
        /// </summary>
        public bool ValidBillingOptions
        {
            get { return _validBillingOptions; }
            set { SetProperty(ref _validBillingOptions, value); }
        }
        private bool _validBillingOptions = false;

        ///// <summary>
        ///// Set the required field border colour
        ///// </summary>
        //public Brush ValidEntity
        //{
        //    get { return _validEntity; }
        //    set { SetProperty(ref _validEntity, value); }
        //}
        //private Brush _validEntity = Brushes.Red;

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
                    case "SelectedBillingProcesses":
                        ValidBillingOptions = string.IsNullOrEmpty(SelectedBillingProcesses) ? false : true; break;
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
        public ViewBillingViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            _securityHelper = new SecurityHelper(eventAggregator);
            InitialiseBillingView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private void InitialiseBillingView()
        {
            //_model = new ValidationRuleModel(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands
            ResetCommand = new DelegateCommand(ExecuteReset);
            AcceptCommand = new DelegateCommand(ExecuteAccept, CanExecuteAccept).ObservesProperty(() => ValidBillingOptions);
            //SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave).ObservesProperty(() => ValidCategory)
            //                                                              .ObservesProperty(() => ValidEntity)
            //                                                              .ObservesProperty(() => ValidRuleData)
            //                                                              .ObservesProperty(() => ValidOperator);

            // Load the view data
            ReadBillingProcesses();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            BillingWizardPageCount = 4;
            BillingProcessCount = 0;
            SelectedBillingMonth = DateTime.Now.Month;
            SelectedBillingYear = DateTime.Now.Year;
            BillingWizardProgress = string.Format("Billing step - {0} of {1}", 1, BillingWizardPageCount);
            BillinProcessProgress = string.Format("Executing Process - {0} of {1}", 0, BillingProcessCount);

        }

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

        #region Lookup Data Loading

        #endregion

        #region Command Execution
        
        /// <summary>
        /// Execute when the reset command button is clicked 
        /// </summary>
        private void ExecuteReset()
        {

        }

        /// <summary>
        /// Validate if the billing option can be accepted
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteAccept()
        {
            return ValidBillingOptions;
        }

        /// <summary>
        /// Execute when the accept command button is clicked 
        /// </summary>
        private void ExecuteAccept()
        {
            BillingRunStarted = true;
            CanBillingOptionsChange = false;
            BillingProcessCount = SelectedBillingProcesses.Split(',').Count();
            SelectedBillingPeriod = string.Format("{0} {1}", SelectedBillingMonth.ToString().PadLeft(2, '0'), SelectedBillingYear);
            BillinProcessProgress = string.Format("Executing Process - {0} of {1}", 0, BillingProcessCount);

        }

        ///// <summary>
        ///// Set view command buttons enabled/disabled state
        ///// </summary>
        ///// <returns></returns>
        //private bool CanExecuteAdd()
        //{
        //    return _securityHelper.IsUserInRole(SecurityRole.Administrator.Value());
        //}

        ///// <summary>
        ///// Execute when the add command button is clicked 
        ///// </summary>
        //private void ExecuteAdd()
        //{
        //    SelectedValidationRule = new ValidationRule();
        //}

        ///// <summary>
        ///// Set view command buttons enabled/disabled state
        ///// </summary>
        ///// <returns></returns>
        //private bool CanExecuteSave()
        //{
        //    return ValidCategory == Brushes.Silver && ValidEntity == Brushes.Silver && ValidRuleData == Brushes.Silver && ValidOperator == Brushes.Silver;
        //}

        ///// <summary>
        ///// Execute when the save command button is clicked 
        ///// </summary>
        //private async void ExecuteSave()
        //{
        //    bool result = false;
        //    ValidationDataType dataType = (ValidationDataType)SelectedRuleData.enValidationDataType;

        //    if (SelectedValidationRule == null)
        //        SelectedValidationRule = new ValidationRule();

        //    SelectedValidationRule.enValidationRuleGroup = ((ValidationRuleGroup)Enum.Parse(typeof(ValidationRuleGroup), SelectedCategory)).Value();
        //    SelectedValidationRule.fkValidationRulesDataID = SelectedRuleData.pkValidationRulesDataID;
        //    SelectedValidationRule.ValidationDataValue = SelectedValidationValue.ToUpper();

        //    switch (((ValidationRuleGroup)Enum.Parse(typeof(ValidationRuleGroup), SelectedCategory)))
        //    {
        //        case ValidationRuleGroup.Client:
        //            SelectedValidationRule.EntityID = ((Client)SelectedEntity).pkClientID;
        //            break;
        //        case ValidationRuleGroup.Company:
        //            SelectedValidationRule.EntityID = ((Company)SelectedEntity).pkCompanyID;
        //            break;
        //    }

        //    SelectedValidationRule.enStringCompareType = SelectedValidationRule.enDateCompareType = SelectedValidationRule.enNumericCompareType = 0;
        //    if (dataType == ValidationDataType.String)
        //        SelectedValidationRule.enStringCompareType = ((StringOperator)Enum.Parse(typeof(StringOperator), SelectedOperator)).Value();
        //    else if (dataType == ValidationDataType.Date)
        //        SelectedValidationRule.enDateCompareType = ((DateCompareValue)Enum.Parse(typeof(DateCompareValue), SelectedOperator)).Value();
        //    else
        //        SelectedValidationRule.enNumericCompareType = ((NumericOperator)Enum.Parse(typeof(NumericOperator), SelectedOperator)).Value();

        //    SelectedValidationRule.ModifiedBy = SecurityHelper.LoggedInDomainName;
        //    SelectedValidationRule.ModifiedDate = DateTime.Now;

        //    if (SelectedValidationRule.pkValidationRuleID == 0)
        //        result = _model.CreateValidationRule(SelectedValidationRule);
        //    else
        //        result = _model.UpdateValidationRule(SelectedValidationRule);

        //    if (result)
        //    {
        //        InitialiseViewControls();
        //        await ReadValidationRulesAsync();
        //    }
        //}

        ///// <summary>
        ///// Set view command buttons enabled/disabled state
        ///// </summary>
        ///// <returns></returns>
        //private bool CanExecuteMaintenace()
        //{
        //    return true;
        //}

        #endregion

        #endregion
    }
}
