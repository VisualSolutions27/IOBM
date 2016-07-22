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

        public DelegateCommand AcceptCommand { get; set; }
        public DelegateCommand NextCommand { get; set; }
        public DelegateCommand BackCommand { get; set; }
        public DelegateCommand FinishCommand { get; set; }

        #endregion

        #region Properties

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
        /// The number of pages in the billing wizard
        /// </summary>
        public int BillingWizardProgress
        {
            get { return _billingWizardProgress; }
            set { SetProperty(ref _billingWizardProgress, value); }
        }
        private int _billingWizardProgress = 1;

        /// <summary>
        /// The number of selected billing processes
        /// </summary>
        public int BillingProcessProgress
        {
            get { return _billingProcessProgress; }
            set { SetProperty(ref _billingProcessProgress, value); }
        }
        private int _billingProcessProgress = 0;

        /// <summary>
        /// The billing wizard progress description
        /// </summary>
        public string BillingWizardDescription
        {
            get { return _billingWizardDescription; }
            set { SetProperty(ref _billingWizardDescription, value); }
        }
        private string _billingWizardDescription = string.Format("Billing step - {0} of {1}", 1, 1);

        /// <summary>
        /// The billing wizard progress description
        /// </summary>
        public string BillinProcessDescription
        {
            get { return _billinProcessDescription; }
            set { SetProperty(ref _billinProcessDescription, value); }
        }
        private string _billinProcessDescription = string.Format("Executing Process - {0} of {1}", 0, 0);

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
        /// Indicate if the billing run started
        /// </summary>
        public bool BillingRunStarted
        {
            get { return _billingRunStarted; }
            set { SetProperty(ref _billingRunStarted, value); }
        }
        private bool _billingRunStarted = false;

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
        #endregion

        #region Required Fields
        #endregion

        #region Input Validation

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
            AcceptCommand = new DelegateCommand(ExecuteAccept, CanExecuteAccept).ObservesProperty(() => BillingRunStarted);

            // Load the view data
            ReadBillingProcesses();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            BillingWizardPageCount = 4;
            BillingWizardProgress = BillingProcessProgress = 0;
            SelectedBillingMonth = DateTime.Now.Month;
            SelectedBillingYear = DateTime.Now.Year;
            BillingWizardDescription = string.Format("Billing step - {0} of {1}", 1, BillingWizardPageCount);
            BillingRunStarted = false;
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

                BillingProcessCount = BillingProcessCollection.Count;
                BillinProcessDescription = string.Format("Executing Process - {0} of {1}", 0, BillingProcessCount);
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
        /// Validate is the accecpt button is avaliable 
        /// </summary>
        /// <returns>True if valid</returns>
        private bool CanExecuteAccept()
        {
            return BillingRunStarted ? false : true;
        }

        /// <summary>
        /// Execute when the accept command button is clicked 
        /// </summary>
        private void ExecuteAccept()
        {
            BillingRunStarted = true;
            SelectedBillingPeriod = string.Format("{0} {1}", SelectedBillingMonth.ToString().PadLeft(2, '0'), SelectedBillingYear);
        }

        #endregion

        #endregion
    }
}
