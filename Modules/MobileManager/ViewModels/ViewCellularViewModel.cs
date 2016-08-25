using Prism.Mvvm;
using Prism.Commands;
using Gijima.IOBM.MobileManager.Views;
using Prism.Events;
using System;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Linq;
using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.MobileManager.Model.Models;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Security;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Common.Events;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewCellularViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private ClientModel _model = null;
        private IEventAggregator _eventAggregator;
        private SecurityHelper _securityHelper = null;
        private DataActivityLog _activityLogInfo = null;

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand DeleteCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand ClientCompanyCommand { get; set; }
        public DelegateCommand ClientSuburbCommand { get; set; }
        public DelegateCommand ClientUserSiteCommand { get; set; }
        public DelegateCommand ContractStatusCommand { get; set; }
        public DelegateCommand ContractPackageCommand { get; set; }
        public DelegateCommand ContractSuburbCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the selected (current) client entity
        /// </summary>
        public Client SelectedClient
        {
            get { return _selectedClient; }
            set
            {
                SetProperty(ref _selectedClient, value);

                if (value != null && value.pkClientID > 0)
                {
                    SelectedContract = value.Contract != null ? value.Contract : null;
                    if (CompanyCollection != null)
                        SelectedCompany = value.fkCompanyID > 0 ? CompanyCollection.Where(p => p.pkCompanyID == value.fkCompanyID).FirstOrDefault() 
                                                                : CompanyCollection.Where(p => p.pkCompanyID == 0).FirstOrDefault();
                    if (ClientLocationCollection != null)
                        SelectedClientLocation = value.fkClientLocationID > 0 ? ClientLocationCollection.Where(p => p.pkClientLocationID == value.fkClientLocationID).FirstOrDefault() 
                                                                              : ClientLocationCollection.Where(p => p.pkClientLocationID == 0).FirstOrDefault();
                    if (SuburbCollection != null)
                        SelectedSuburb = value.fkSuburbID > 0 ? SuburbCollection.Where(p => p.pkSuburbID == value.fkSuburbID).FirstOrDefault()
                                                              : SuburbCollection.Where(p => p.pkSuburbID == 0).FirstOrDefault();
                    if (StatusCollection != null)
                        SelectedStatus = value.Contract != null ? StatusCollection.Where(p => p.pkStatusID == value.Contract.fkStatusID).FirstOrDefault() 
                                                                : StatusCollection.Where(p => p.pkStatusID == 0).FirstOrDefault();
                    if (PackageCollection != null)
                        SelectedPackage = value.Contract != null ? PackageCollection.Where(p => p.pkPackageID == value.Contract.fkPackageID).FirstOrDefault()
                                                                 : PackageCollection.Where(p => p.pkPackageID == 0).FirstOrDefault();
                    SetClientBilling(value.ClientBilling);
                    SelectedCellNumber = value.PrimaryCellNumber;
                    SelectedClientName = value.ClientName;
                    SelectedClientIDNumber = value.IDNumber;
                    SelectedClientAdminFee = value.AdminFee > 0 ? value.AdminFee.ToString() : SelectedCompany != null ? SelectedCompany.AdminFee.ToString() : "0.00";
                    SelectedClientAddressLine = value.AddressLine1;
                    SelectedClientWBSNumber = !string.IsNullOrWhiteSpace(value.WBSNumber) ? value.WBSNumber : SelectedCompany != null ? SelectedCompany.WBSNumber : null;
                    SelectedClientCostCode = !string.IsNullOrWhiteSpace(value.CostCode) ? value.CostCode : SelectedCompany != null ? SelectedCompany.CostCode : null;
                    SelectedClientIPAddress = !string.IsNullOrWhiteSpace(value.IPAddress) ? value.IPAddress : SelectedCompany != null ? SelectedCompany.IPAddress : null;
                    SelectedClientState = value.IsActive;
                    SelectedCostType = value.Contract != null ? ((CostType)value.Contract.enCostType).ToString() : "NONE";
                    SelectedPackageType = SelectedPackage != null ? ((PackageType)SelectedPackage.enPackageType).ToString() : "NONE";
                    SelectedContractAccNumber = value.Contract != null ? value.Contract.AccountNumber : null;
                    SelectedContractStartDate = value.Contract != null && value.Contract.ContractStartDate != null ? value.Contract.ContractStartDate.Value : DateTime.MinValue;
                    SelectedContractEndDate = value.Contract != null && value.Contract.ContractEndDate.Value != null ? value.Contract.ContractEndDate.Value : DateTime.MinValue;

                    MobileManagerEnvironment.SelectedClientID = value.pkClientID;
                    MobileManagerEnvironment.ClientCompanyID = value.fkCompanyID;
                    MobileManagerEnvironment.SelectedContractID = value.fkContractID;

                    // Publish these event to populate the devices, Simcards and accounts linked to the contract
                    if (value.Contract != null)
                    {
                        _eventAggregator.GetEvent<ReadSimCardsEvent>().Publish(value.Contract.pkContractID);
                        _eventAggregator.GetEvent<ReadDevicesEvent>().Publish(value.Contract.pkContractID);
                        _eventAggregator.GetEvent<ReadInvoicesEvent>().Publish(value.pkClientID);
                    }

                    // Publish this event to set the admin tab as default tab
                    _eventAggregator.GetEvent<NavigationEvent>().Publish(0);
                }
            }
        }
        private Client _selectedClient = new Client();

        /// <summary>
        /// The selected client (Contract) state
        /// </summary>
        public bool SelectedClientState
        {
            get { return _selectedClientState; }
            set
            {
                SetProperty(ref _selectedClientState, value);
                ClientStateColour = value ? Brushes.Black : Brushes.Red;
            }
        }
        private bool _selectedClientState;

        /// <summary>
        /// Set the required field fore colour
        /// </summary>
        public Brush ClientStateColour
        {
            get { return _clientStateColour; }
            set { SetProperty(ref _clientStateColour, value); }
        }
        private Brush _clientStateColour;

        /// <summary>
        /// The entered IP Address
        /// </summary>
        public string SelectedClientIPAddress
        {
            get { return _selectedIPAddress; }
            set { SetProperty(ref _selectedIPAddress, value); }
        }
        private string _selectedIPAddress = string.Empty;

        #region View Lookup Data Collections

        /// <summary>
        /// The collection of companies from the database
        /// </summary>
        public ObservableCollection<Company> CompanyCollection
        {
            get { return _companyCollection; }
            set { SetProperty(ref _companyCollection, value); }
        }
        private ObservableCollection<Company> _companyCollection = null;

        /// <summary>
        /// The collection of client locations from the database
        /// </summary>
        public ObservableCollection<ClientLocation> ClientLocationCollection
        {
            get { return _clientLocationCollection; }
            set { SetProperty(ref _clientLocationCollection, value); }
        }
        private ObservableCollection<ClientLocation> _clientLocationCollection = null;

        /// <summary>
        /// The collection of suburb from the database
        /// </summary>
        public ObservableCollection<Suburb> SuburbCollection
        {
            get { return _suburbCollection; }
            set { SetProperty(ref _suburbCollection, value); }
        }
        private ObservableCollection<Suburb> _suburbCollection = null;

        /// <summary>
        /// The collection of statuses from the database
        /// </summary>
        public ObservableCollection<Status> StatusCollection
        {
            get { return _statusCollection; }
            set { SetProperty(ref _statusCollection, value); }
        }
        private ObservableCollection<Status> _statusCollection = null;

        /// <summary>
        /// The collection of packages from the database
        /// </summary>
        public ObservableCollection<Package> PackageCollection
        {
            get { return _packageCollection; }
            set { SetProperty(ref _packageCollection, value); }
        }
        private ObservableCollection<Package> _packageCollection = null;

        /// <summary>
        /// The collection of cost types frem the CostType enum
        /// </summary>
        public ObservableCollection<string> CostTypeCollection
        {
            get { return _costTypeCollection; }
            set { SetProperty(ref _costTypeCollection, value); }
        }
        private ObservableCollection<string> _costTypeCollection = null;

        #endregion

        #region Required Fields

        /// <summary>
        /// The entered primary cell number
        /// </summary>
        public string SelectedCellNumber
        {
            get { return _cellNumber; }
            set { SetProperty(ref _cellNumber, value); }
        }
        private string _cellNumber = string.Empty;

        /// <summary>
        /// The entered client name
        /// </summary>
        public string SelectedClientName
        {
            get { return _clientName; }
            set { SetProperty(ref _clientName, value); }
        }
        private string _clientName = string.Empty;

        /// <summary>
        /// The entered client ID number
        /// </summary>
        public string SelectedClientIDNumber
        {
            get { return _clientIDNumber; }
            set { SetProperty(ref _clientIDNumber, value); }
        }
        private string _clientIDNumber = string.Empty;

        /// <summary>
        /// The selected company
        /// </summary>
        public Company SelectedCompany
        {
            get { return _selectedCompany; }
            set { SetProperty(ref _selectedCompany, value); }
        }
        private Company _selectedCompany;

        /// <summary>
        /// The selected client location
        /// </summary>
        public ClientLocation SelectedClientLocation
        {
            get { return _selectedClientLocation; }
            set { SetProperty(ref _selectedClientLocation, value); }
        }
        private ClientLocation _selectedClientLocation;

        /// <summary>
        /// The entered wbs number
        /// </summary>
        public string SelectedClientWBSNumber
        {
            get { return _wbsNumber; }
            set { SetProperty(ref _wbsNumber, value); }
        }
        private string _wbsNumber;

        /// <summary>
        /// The entered cost code
        /// </summary>
        public string SelectedClientCostCode
        {
            get { return _costCode; }
            set { SetProperty(ref _costCode, value); }
        }
        private string _costCode;

        /// <summary>
        /// The entered admin fee
        /// </summary>
        public string SelectedClientAdminFee
        {
            get { return _adminFee; }
            set { SetProperty(ref _adminFee, value); }
        }
        private string _adminFee;

        /// <summary>
        /// The entered address line 1
        /// </summary>
        public string SelectedClientAddressLine
        {
            get { return _addressLine; }
            set { SetProperty(ref _addressLine, value); }
        }
        private string _addressLine;

        /// <summary>
        /// The selected suburb
        /// </summary>
        public Suburb SelectedSuburb
        {
            get { return _selectedSuburb; }
            set { SetProperty(ref _selectedSuburb, value); }
        }
        private Suburb _selectedSuburb;

        /// <summary>
        /// The selected client billing
        /// </summary>
        public ClientBilling SelectedClientBilling
        {
            get { return _selectedClientBilling; }
            set { SetProperty(ref _selectedClientBilling, value); }
        }
        private ClientBilling _selectedClientBilling;

        /// <summary>
        /// The selected client contract
        /// </summary>
        public Contract SelectedContract
        {
            get { return _selectedContract; }
            set { SetProperty(ref _selectedContract, value); }
        }
        private Contract _selectedContract;

        /// <summary>
        /// The selected status
        /// </summary>
        public Status SelectedStatus
        {
            get { return _selectedStatus; }
            set { SetProperty(ref _selectedStatus, value); }
        }
        private Status _selectedStatus;

        /// <summary>
        /// The selected package
        /// </summary>
        public Package SelectedPackage
        {
            get { return _selectedPackage; }
            set { SetProperty(ref _selectedPackage, value); }
        }
        private Package _selectedPackage;

        /// <summary>
        /// The selected cost type
        /// </summary>
        public string SelectedCostType
        {
            get { return _selectedCostType; }
            set { SetProperty(ref _selectedCostType, value); }
        }
        private string _selectedCostType;

        /// <summary>
        /// The selected package type
        /// </summary>
        public string SelectedPackageType
        {
            get { return _selectedPackageType; }
            set { SetProperty(ref _selectedPackageType, value); }
        }
        private string _selectedPackageType;

        /// <summary>
        /// The entered account number
        /// </summary>
        public string SelectedContractAccNumber
        {
            get { return _selectedAccountNumber; }
            set { SetProperty(ref _selectedAccountNumber, value); }
        }
        private string _selectedAccountNumber;
       
        /// <summary>
        /// The selected contract start date
        /// </summary>
        public DateTime SelectedContractStartDate
        {
            get { return _selectedContractStartDate; }
            set { SetProperty(ref _selectedContractStartDate, value); }
        }
        private DateTime _selectedContractStartDate;

        /// <summary>
        /// The selected contract end date
        /// </summary>
        public DateTime SelectedContractEndDate
        {
            get { return _selectedContractEndDate; }
            set { SetProperty(ref _selectedContractEndDate, value); }
        }
        private DateTime _selectedContractEndDate;

        /// <summary>
        /// Indicate if client has split billing
        /// </summary>
        public bool SplitBilling
        {
            get { return _splitBilling; }
            set { SetProperty(ref _splitBilling, value); }
        }
        private bool _splitBilling;

        /// <summary>
        /// Indicate if client do not have split billing
        /// </summary>
        public bool NoSplitBilling
        {
            get { return _noSplitBilling; }
            set
            {
                if (value == true)
                {
                    SelectedClientBilling = new ClientBilling();
                    SelectedVoiceAllowance = "0.00";
                    SelectedIntRoaming = false;
                }

                SetProperty(ref _noSplitBilling, value);
            }
        }
        private bool _noSplitBilling;
       
        /// <summary>
        /// Indicate if client has split billing
        /// </summary>
        public string SelectedVoiceAllowance
        {
            get { return _selectedVoiceAllowance; }
            set { SetProperty(ref _selectedVoiceAllowance, value); }
        }
        private string _selectedVoiceAllowance;

        /// <summary>
        /// Indicate if client has split billing
        /// </summary>
        public string SelectedRoamingCountry
        {
            get { return _selectedRoamingCountry; }
            set { SetProperty(ref _selectedRoamingCountry, value); }
        }
        private string _selectedRoamingCountry;

        /// <summary>
        /// Indicate if client has split billing
        /// </summary>
        public DateTime SelectedRoamingFromDate
        {
            get { return _selectedRoamingFromDate; }
            set { SetProperty(ref _selectedRoamingFromDate, value); }
        }
        private DateTime _selectedRoamingFromDate;

        /// <summary>
        /// Indicate if client has split billing
        /// </summary>
        public DateTime SelectedRoamingToDate
        {
            get { return _selectedRoamingToDate; }
            set { SetProperty(ref _selectedRoamingToDate, value); }
        }
        private DateTime _selectedRoamingToDate;

        /// <summary>
        /// Indicate if client has split billing
        /// </summary>
        public bool SelectedIntRoaming
        {
            get { return _selectedIntRoaming; }
            set
            {
                SelectedRoamingCountry = string.Empty;
                SelectedRoamingFromDate = SelectedRoamingToDate = value == true ? DateTime.Now : DateTime.MinValue; 
                SetProperty(ref _selectedIntRoaming, value);
            }
        }
        private bool _selectedIntRoaming;

        #endregion

        #region Input Validation
         
        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidCellNumber
        {
            get { return _validCellNumber; }
            set { SetProperty(ref _validCellNumber, value); }
        }
        private Brush _validCellNumber = Brushes.Red;

        MaskedTextProvider mtp = new MaskedTextProvider("###.###.###.###");

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidClientName
        {
            get { return _validClientName; }
            set { SetProperty(ref _validClientName, value); }
        }
        private Brush _validClientName = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidClientIDNumber
        {
            get { return _validClientIDNumber; }
            set { SetProperty(ref _validClientIDNumber, value); }
        }
        private Brush _validClientIDNumber = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidCompany
        {
            get { return _validCompany; }
            set { SetProperty(ref _validCompany, value); }
        }
        private Brush _validCompany = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidWBSNumber
        {
            get { return _validWBSNumber; }
            set { SetProperty(ref _validWBSNumber, value); }
        }
        private Brush _validWBSNumber = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidCostCode
        {
            get { return _validCostCode; }
            set { SetProperty(ref _validCostCode, value); }
        }
        private Brush _validCostCode = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidAdminFee
        {
            get { return _validAdminFee; }
            set { SetProperty(ref _validAdminFee, value); }
        }
        private Brush _validAdminFee = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidAddressLine
        {
            get { return _validAddressLine; }
            set { SetProperty(ref _validAddressLine, value);  }
        }
        private Brush _validAddressLine = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidSuburb
        {
            get { return _validSuburb; }
            set { SetProperty(ref _validSuburb, value);}
        }
        private Brush _validSuburb = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidStatus
        {
            get { return _validStatus; }
            set { SetProperty(ref _validStatus, value); }
        }
        private Brush _validStatus = Brushes.Red;
        
        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidPackage
        {
            get { return _validPackage; }
            set { SetProperty(ref _validPackage, value); }
        }
        private Brush _validPackage = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidAccountNumber
        {
            get { return _validAccountNumber; }
            set { SetProperty(ref _validAccountNumber, value); }
        }
        private Brush _validAccountNumber = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidCostType
        {
            get { return _validCostType; }
            set { SetProperty(ref _validCostType, value); }
        }
        private Brush _validCostType = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidContractStartDate
        {
            get { return _validContractStartDate; }
            set { SetProperty(ref _validContractStartDate, value);}
        }
        private Brush _validContractStartDate = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidContractEndDate
        {
            get { return _validContractEndDate; }
            set { SetProperty(ref _validContractEndDate, value);}
        }
        private Brush _validContractEndDate = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidSplitBilling
        {
            get { return _validSplitBilling; }
            set { SetProperty(ref _validSplitBilling, value); }
        }
        private Brush _validSplitBilling = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidVoiceAllowance
        {
            get { return _validVoiceAllowance; }
            set { SetProperty(ref _validVoiceAllowance, value); }
        }
        private Brush _validVoiceAllowance = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidRoamingCountry
        {
            get { return _validRoamingCountry; }
            set { SetProperty(ref _validRoamingCountry, value); }
        }
        private Brush _validRoamingCountry = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidRoamingFromDate
        {
            get { return _validRoamingFromDate; }
            set { SetProperty(ref _validRoamingFromDate, value); }
        }
        private Brush _validRoamingFromDate = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidRoamingToDate
        {
            get { return _validRoamingToDate; }
            set { SetProperty(ref _validRoamingToDate, value); }
        }
        private Brush _validRoamingToDate = Brushes.Red;

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
                    case "SelectedCellNumber":
                        ValidCellNumber = string.IsNullOrEmpty(SelectedCellNumber) || SelectedCellNumber.Length < 10 ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedClientName":
                        ValidClientName = string.IsNullOrEmpty(SelectedClientName) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedClientIDNumber":
                        ValidClientIDNumber = string.IsNullOrEmpty(SelectedClientIDNumber) || SelectedClientIDNumber.Length < 13 ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedCompany":
                        ValidCompany = SelectedCompany != null && SelectedCompany.pkCompanyID < 1 ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedClientWBSNumber":
                        ValidWBSNumber = string.IsNullOrEmpty(SelectedClientWBSNumber) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedClientCostCode":
                        ValidCostCode = string.IsNullOrEmpty(SelectedClientCostCode) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedClientAdminFee":
                        ValidAdminFee = !string.IsNullOrWhiteSpace(SelectedClientAdminFee) && Convert.ToDecimal(SelectedClientAdminFee) < 1 ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedContractAccNumber":
                        ValidAccountNumber = string.IsNullOrEmpty(SelectedContractAccNumber) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedClientAddressLine":
                        ValidAddressLine = string.IsNullOrEmpty(SelectedClientAddressLine) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedSuburb":
                        ValidSuburb = SelectedSuburb != null && SelectedSuburb.pkSuburbID < 1 ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedStatus":
                        ValidStatus = SelectedStatus != null && SelectedStatus.pkStatusID < 1 ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedPackage":
                        ValidPackage = SelectedPackage != null && SelectedPackage.pkPackageID < 1 ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedCostType":
                        ValidCostType = SelectedCostType == null || SelectedCostType == CostType.NONE.ToString() ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedContractStartDate":
                        ValidContractStartDate = SelectedContractStartDate == null || 
                                                 SelectedContractStartDate.Date == DateTime.MinValue.Date ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedContractEndDate":
                        ValidContractEndDate = SelectedContractEndDate == null || 
                                               SelectedContractEndDate < SelectedContractStartDate ||
                                               SelectedContractEndDate.Date == DateTime.MinValue.Date ? Brushes.Red : Brushes.Silver; break;
                    case "SplitBilling":
                        ValidSplitBilling = !SplitBilling && !NoSplitBilling ? Brushes.Red : Brushes.Silver;
                        ValidVoiceAllowance = SplitBilling && (string.IsNullOrWhiteSpace(SelectedVoiceAllowance) || Convert.ToDecimal(SelectedVoiceAllowance) < 1) ? Brushes.Red : Brushes.Silver;
                        ValidRoamingCountry = SelectedIntRoaming && string.IsNullOrEmpty(SelectedRoamingCountry) ? Brushes.Red : Brushes.Silver;
                        ValidRoamingFromDate = SplitBilling && (SelectedClientBilling == null ||
                                               SelectedClientBilling.RoamingFromDate == DateTime.MinValue) ? Brushes.Red : Brushes.Silver;
                        ValidRoamingToDate = SplitBilling && (SelectedClientBilling == null ||
                                             SelectedClientBilling.RoamingToDate < SelectedClientBilling.RoamingFromDate ||
                                             SelectedClientBilling.RoamingToDate == DateTime.MinValue) ? Brushes.Red : Brushes.Silver; break;
                    case "NoSplitBilling":
                        if (NoSplitBilling)
                            ValidSplitBilling = ValidVoiceAllowance = ValidRoamingCountry = ValidRoamingFromDate = ValidRoamingToDate = Brushes.Silver;
                        ValidSplitBilling = !SplitBilling && !NoSplitBilling ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedIntRoaming":
                        ValidRoamingCountry = SelectedIntRoaming && string.IsNullOrEmpty(SelectedRoamingCountry) ? Brushes.Red : Brushes.Silver;
                        ValidRoamingFromDate = SplitBilling && SelectedIntRoaming && (SelectedClientBilling == null || SelectedClientBilling.RoamingToDate == null ||
                                               SelectedClientBilling.RoamingFromDate == DateTime.MinValue) ? Brushes.Red : Brushes.Silver;
                        ValidRoamingToDate = SplitBilling && SelectedIntRoaming && (SelectedClientBilling == null || SelectedClientBilling.RoamingToDate == null ||
                                             SelectedClientBilling.RoamingToDate < SelectedClientBilling.RoamingFromDate ||
                                             SelectedClientBilling.RoamingToDate == DateTime.MinValue) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedVoiceAllowance":
                        ValidVoiceAllowance = SplitBilling && (string.IsNullOrWhiteSpace(SelectedVoiceAllowance) || Convert.ToDecimal(SelectedVoiceAllowance) < 1) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedRoamingCountry":
                        ValidRoamingCountry = SelectedIntRoaming && string.IsNullOrEmpty(SelectedRoamingCountry) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedRoamingFromDate":
                        ValidRoamingFromDate = SplitBilling && (SelectedClientBilling == null ||
                                               SelectedClientBilling.RoamingFromDate == DateTime.MinValue) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedRoamingToDate":
                        ValidRoamingToDate = SplitBilling && (SelectedClientBilling == null ||
                                             SelectedClientBilling.RoamingToDate < SelectedClientBilling.RoamingFromDate ||
                                             SelectedClientBilling.RoamingToDate == DateTime.MinValue) ? Brushes.Red : Brushes.Silver; break;
                }
                return result;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Event Handlers

        /// <summary>
        /// Read the client data for the client ID
        /// </summary>
        /// <param name="sender">The client ID.</param>
        private async void SearchResult_Event(int sender)
        {
            await ReadClientAsync(sender);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewCellularViewModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            InitialiseCellularView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseCellularView()
        {
            _model = new ClientModel(_eventAggregator);
            _securityHelper = new SecurityHelper(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecuteCancel).ObservesProperty(() => SelectedCellNumber);
            AddCommand = new DelegateCommand(ExecuteAdd, CanExecuteAdd).ObservesProperty(() => SelectedClient);
            DeleteCommand = new DelegateCommand(ExecuteDelete, CanExecuteDelete).ObservesProperty(() => SelectedClient)
                                                                                .ObservesProperty(() => SelectedStatus);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave).ObservesProperty(() => SelectedCellNumber)
                                                                          .ObservesProperty(() => SelectedClientName)
                                                                          .ObservesProperty(() => SelectedClientIDNumber)
                                                                          .ObservesProperty(() => SelectedCompany)
                                                                          .ObservesProperty(() => SelectedClientLocation)
                                                                          .ObservesProperty(() => SelectedClientWBSNumber)
                                                                          .ObservesProperty(() => SelectedClientCostCode)
                                                                          .ObservesProperty(() => SelectedClientAdminFee)
                                                                          .ObservesProperty(() => SelectedContractAccNumber)
                                                                          .ObservesProperty(() => SelectedClientAddressLine)
                                                                          .ObservesProperty(() => SelectedSuburb)
                                                                          .ObservesProperty(() => SplitBilling)
                                                                          .ObservesProperty(() => NoSplitBilling)
                                                                          .ObservesProperty(() => SelectedVoiceAllowance)
                                                                          .ObservesProperty(() => SelectedIntRoaming)
                                                                          .ObservesProperty(() => SelectedRoamingCountry)
                                                                          .ObservesProperty(() => SelectedRoamingFromDate)
                                                                          .ObservesProperty(() => SelectedRoamingToDate)
                                                                          .ObservesProperty(() => SelectedStatus)
                                                                          .ObservesProperty(() => SelectedPackage)
                                                                          .ObservesProperty(() => SelectedCostType)
                                                                          .ObservesProperty(() => SelectedContractStartDate)
                                                                          .ObservesProperty(() => SelectedContractEndDate);
            ClientCompanyCommand = new DelegateCommand(ExecuteShowCompanyView, CanExecuteMaintenace);
            ClientUserSiteCommand = new DelegateCommand(ExecuteShowClientSiteView);
            ClientSuburbCommand = new DelegateCommand(ExecuteShowSuburbView, CanExecuteMaintenace);
            ContractStatusCommand = new DelegateCommand(ExecuteShowStatusView, CanExecuteMaintenace);
            ContractPackageCommand = new DelegateCommand(ExecuteShowPackageView, CanExecuteMaintenace);

            // Subscribe to this event to read the client data based on the search results
            _eventAggregator.GetEvent<SearchResultEvent>().Subscribe(SearchResult_Event, true);

            // Initialise the data activity log info entity
            _activityLogInfo = new DataActivityLog();
            _activityLogInfo.ActivityProcess = ActivityProcess.Administration.Value();

            // Load the view data
            await ReadCompaniesAsync();
            await ReadClientLocationsAsync();
            await ReadSuburbsAsync();
            await ReadStatusesAsync();
            await ReadPackagesAsync();
            ReadCostTypes();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            SelectedClient = new Client();
            SelectedContract = new Contract();           
            SplitBilling = NoSplitBilling = false;
        }

        /// <summary>
        /// Read client data from the database
        /// </summary>
        /// <param name="clientID">The client ID.</param>
        private async Task ReadClientAsync(int clientID)
        {
            try
            {
                SelectedClient = await Task.Run(() => _model.ReadClient(clientID));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Set the options for client billing
        /// </summary>
        /// <param name="clientBilling">The client billing entity.</param>
        private void SetClientBilling(ClientBilling clientBilling)
        {
            SelectedClientBilling = clientBilling;

            if (clientBilling != null && clientBilling.IsSplitBilling)
            {
                SplitBilling = true;
                NoSplitBilling = false;
                SelectedVoiceAllowance = clientBilling.VoiceAllowance.ToString();
                SelectedIntRoaming = clientBilling.InternationalRoaming;
                SelectedRoamingCountry = clientBilling.CountryVisiting;
                SelectedRoamingFromDate = clientBilling.RoamingFromDate != null ? clientBilling.RoamingFromDate.Value : DateTime.MinValue.Date;
                SelectedRoamingToDate = clientBilling.RoamingToDate != null ? clientBilling.RoamingToDate.Value : DateTime.MinValue.Date;
            }
            else
            {
                SplitBilling = false;
                NoSplitBilling = true;
                SelectedVoiceAllowance = "0.00";
                SelectedIntRoaming = false;
                SelectedRoamingCountry = string.Empty;
                SelectedRoamingFromDate = DateTime.MinValue.Date;
                SelectedRoamingToDate = DateTime.MinValue.Date;
            }
        }

        #region Lookup Data Loading

        /// <summary>
        /// Load all the companies from the database
        /// </summary>
        private async Task ReadCompaniesAsync()
        {
            try
            {
                CompanyCollection = await Task.Run(() => new CompanyModel(_eventAggregator).ReadCompanies(true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Load all the client locations from the database
        /// </summary>
        private async Task ReadClientLocationsAsync()
        {
            try
            {
                ClientLocationCollection = await Task.Run(() => new ClientLocationModel(_eventAggregator).ReadClientLocations(true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Load all the suburbs from the database
        /// </summary>
        private async Task ReadSuburbsAsync()
        {
            try
            {
                SuburbCollection = await Task.Run(() => new SuburbModel(_eventAggregator).ReadSuburbes(true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Load all the statuses from the database
        /// </summary>
        private async Task ReadStatusesAsync()
        {
            try
            {
                StatusCollection = await Task.Run(() => new StatusModel(_eventAggregator).ReadStatuses(StatusLink.Contract, true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Load all the packages from the database
        /// </summary>
        private async Task ReadPackagesAsync()
        {
            try
            {
                PackageCollection = await Task.Run(() => new PackageModel(_eventAggregator).ReadPackages(true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Populate the package types combobox from the PackageType enum
        /// </summary>
        private void ReadCostTypes()
        {
            CostTypeCollection = new ObservableCollection<string>();

            foreach (CostType source in Enum.GetValues(typeof(CostType)))
            {
                CostTypeCollection.Add(source.ToString());
            }
        }

        #endregion

        #region Command Execution

        /// <summary>
        /// Validate if the cancel process can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteCancel()
        {
            return !string.IsNullOrWhiteSpace(SelectedCellNumber);
        }

        /// <summary>
        /// Execute when the cancel process
        /// </summary>
        private void ExecuteCancel()
        {
            InitialiseViewControls();

            // Publish the event to clear the device view
            _eventAggregator.GetEvent<ReadDevicesEvent>().Publish(0);

            // Publish the event to clear the Sim card view
            _eventAggregator.GetEvent<ReadSimCardsEvent>().Publish(0);
        }

        /// <summary>
        /// Validate if the save client data can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteAdd()
        {
            // Validate if the logged-in user can administrate the company the client is linked to
            return SelectedClient != null && SelectedClient.pkClientID > 0 && _securityHelper.IsUserInCompany(SelectedClient.fkCompanyID) ? true : false;
        }

        /// <summary>
        /// Execute the add new client process
        /// </summary>
        private void ExecuteAdd()
        {
            ExecuteCancel();
            SelectedContractStartDate = SelectedContractEndDate = DateTime.Now;
        }

        /// <summary>
        /// Validate if the delete client can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteDelete()
        {
            // Validate if the logged-in user can administrate the company the client is linked to
            return SelectedClient != null && SelectedStatus != null && (SelectedStatus.StatusDescription == "CANCELLED" ||
                                                                        SelectedStatus.StatusDescription == "TRANSFERED" ||
                                                                        SelectedStatus.StatusDescription == "REALLOCATED") ? true : false;
        }

        /// <summary>
        /// Execute the delete new client process
        /// </summary>
        private void ExecuteDelete()
        {
            SelectedClientState = false;
        }

        /// <summary>
        /// Validate if the save client data can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteSave()
        {
            bool result = false;

            // Validate if the logged-in user can administrate the company the client is linked to
            result = SelectedClient.fkCompanyID > 0 && _securityHelper.IsUserInCompany(SelectedClient.fkCompanyID) ? true : false;

            // Validate client data
            if (result)
                result = SelectedClient != null && SelectedClientLocation != null && SelectedClientLocation.pkClientLocationID > 0 && 
                         SelectedCompany != null && SelectedCompany.pkCompanyID > 0 && SelectedSuburb != null && SelectedSuburb.pkSuburbID > 0 &&
                         !string.IsNullOrEmpty(SelectedCellNumber) && SelectedCellNumber.Length == 10 && !string.IsNullOrEmpty(SelectedClientName) &&
                         !string.IsNullOrEmpty(SelectedClientIDNumber) && SelectedClientIDNumber.Length == 13 &&  !string.IsNullOrEmpty(SelectedClientWBSNumber) &&
                         !string.IsNullOrEmpty(SelectedClientCostCode) && !string.IsNullOrEmpty(SelectedClientAddressLine) &&
                         (!string.IsNullOrWhiteSpace(SelectedClientAdminFee) && Convert.ToDecimal(SelectedClientAdminFee) > 0);

            // Validate contract data
            if (result)
                result = SelectedContract != null && SelectedPackage != null && SelectedStatus != null && SelectedCostType != CostType.NONE.ToString() &&
                         !string.IsNullOrEmpty(SelectedContractAccNumber) && SelectedContractStartDate.Date > DateTime.MinValue.Date &&
                         SelectedContractEndDate.Date > DateTime.MinValue.Date;

            // Validate billing data
            if (result && SplitBilling)
                result = SelectedClientBilling != null && (SplitBilling || NoSplitBilling) && 
                         (SplitBilling ? (!string.IsNullOrWhiteSpace(SelectedVoiceAllowance) && Convert.ToDecimal(SelectedVoiceAllowance) > 0) : false) && (SelectedIntRoaming ? !string.IsNullOrEmpty(SelectedRoamingCountry) : true) &&
                         (SplitBilling && SelectedIntRoaming ? SelectedRoamingFromDate.Date > DateTime.MinValue.Date : true) &&
                         (SplitBilling && SelectedIntRoaming ? SelectedRoamingToDate.Date > SelectedRoamingFromDate.Date && SelectedRoamingToDate.Date > DateTime.MinValue.Date : true);

            return result;
        }

        /// <summary>
        /// Execute when the save command button is clicked 
        /// </summary>
        private void ExecuteSave()
        {
            bool result = false;
            // Client Data
            SelectedClient.PrimaryCellNumber = SelectedCellNumber.Trim();
            SelectedClient.ClientName = SelectedClientName.ToUpper().Trim();
            SelectedClient.IDNumber = SelectedClientIDNumber;
            SelectedClient.fkCompanyID = SelectedCompany.pkCompanyID;
            SelectedClient.fkClientLocationID = SelectedClientLocation.pkClientLocationID;
            SelectedClient.WBSNumber = SelectedClientWBSNumber.ToUpper().Trim(); 
            SelectedClient.CostCode = SelectedClientCostCode.ToUpper().Trim();
            SelectedClient.IPAddress = SelectedClientIPAddress;
            SelectedClient.AdminFee = Convert.ToDecimal(SelectedClientAdminFee);
            SelectedClient.AddressLine1 = SelectedClientAddressLine.ToUpper().Trim();
            SelectedClient.fkSuburbID = SelectedSuburb.pkSuburbID;
            SelectedClient.ModifiedBy = SecurityHelper.LoggedInUserFullName;
            SelectedClient.ModifiedDate = DateTime.Now;
            SelectedClient.IsActive = SelectedClientState;
            // Contract Data
            if (SelectedClient.Contract == null)
                SelectedClient.Contract = new Contract();
            SelectedClient.Contract.fkStatusID = SelectedStatus.pkStatusID;
            SelectedClient.Contract.fkPackageID = SelectedPackage.pkPackageID;
            SelectedClient.Contract.AccountNumber = SelectedContractAccNumber.ToUpper().Trim();
            SelectedClient.Contract.enCostType = ((CostType)Enum.Parse(typeof(CostType), SelectedCostType)).Value();
            SelectedClient.Contract.ContractStartDate = SelectedContractStartDate > DateTime.MinValue ? SelectedContractStartDate : (DateTime?)null;
            SelectedClient.Contract.ContractEndDate = SelectedContractEndDate > DateTime.MinValue ? SelectedContractEndDate : (DateTime?)null;
            SelectedClient.Contract.ContractUpgradeDate = SelectedContract.ContractUpgradeDate;
            SelectedClient.Contract.PaymentCancelDate = SelectedContract.PaymentCancelDate;
            SelectedClient.Contract.ModifiedBy = SecurityHelper.LoggedInUserFullName;
            SelectedClient.Contract.ModifiedDate = DateTime.Now;
            SelectedClient.Contract.IsActive = SelectedClientState;
            // Package Setup Data
            if (SelectedClient.Contract.PackageSetup == null)
                SelectedClient.Contract.PackageSetup = new PackageSetup();
            SelectedClient.Contract.PackageSetup.Cost = SelectedContract.PackageSetup.Cost;
            SelectedClient.Contract.PackageSetup.TalkTimeMinutes = SelectedContract.PackageSetup.TalkTimeMinutes;
            SelectedClient.Contract.PackageSetup.SMSNumber = SelectedContract.PackageSetup.SMSNumber;
            SelectedClient.Contract.PackageSetup.MBData = SelectedContract.PackageSetup.MBData;
            SelectedClient.Contract.PackageSetup.RandValue = SelectedContract.PackageSetup.RandValue;
            SelectedClient.Contract.PackageSetup.ModifiedBy = SecurityHelper.LoggedInUserFullName;
            SelectedClient.Contract.PackageSetup.ModifiedDate = DateTime.Now;
            SelectedClient.Contract.PackageSetup.IsActive = SelectedClientState;
            // Billing Data
            if (SelectedClient.ClientBilling == null)
                SelectedClient.ClientBilling = new ClientBilling();
            SelectedClient.ClientBilling.IsSplitBilling = SplitBilling;
            SelectedClient.ClientBilling.WDPAllowance = SelectedClientBilling.WDPAllowance;
            SelectedClient.ClientBilling.VoiceAllowance = Convert.ToDecimal(SelectedVoiceAllowance);
            SelectedClient.ClientBilling.SPLimit = SelectedClientBilling.SPLimit;
            SelectedClient.ClientBilling.InternationalDailing = SelectedClientBilling.InternationalDailing;
            SelectedClient.ClientBilling.InternationalRoaming = SelectedIntRoaming;
            SelectedClient.ClientBilling.CountryVisiting = SelectedRoamingCountry != null ? SelectedRoamingCountry.ToUpper().Trim() : null;
            SelectedClient.ClientBilling.RoamingFromDate = SelectedRoamingFromDate > DateTime.MinValue ? SelectedRoamingFromDate : (DateTime?)null;  
            SelectedClient.ClientBilling.RoamingToDate = SelectedRoamingToDate > DateTime.MinValue ? SelectedRoamingToDate : (DateTime?)null;
            SelectedClient.ClientBilling.StopBillingFromDate = SelectedClientBilling.StopBillingFromDate;
            SelectedClient.ClientBilling.StopBillingToDate = SelectedClientBilling.StopBillingToDate;
            SelectedClient.ClientBilling.ModifiedBy = SecurityHelper.LoggedInUserFullName;
            SelectedClient.ClientBilling.ModifiedDate = DateTime.Now;
            SelectedClient.ClientBilling.IsActive = SelectedClientState;

            if (SelectedClient.pkClientID == 0)
                result = _model.CreateClient(SelectedClient);
            else
                result = _model.UpdateClient(SelectedClient);

            // Publish the event to read the administartion activity logs
            _eventAggregator.GetEvent<SetActivityLogProcessEvent>().Publish(_activityLogInfo);

            if (result)
                ExecuteCancel();
        }

        /// <summary>
        /// Validate if the maintenace functionality can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteMaintenace()
        {
            return _securityHelper.IsUserInRole(SecurityRole.Administrator.Value()) || _securityHelper.IsUserInRole(SecurityRole.Supervisor.Value());
        }

        /// <summary>
        /// Execute show company maintenace view
        /// </summary>
        private async void ExecuteShowCompanyView()
        {
            PopupWindow popupWindow = new PopupWindow(new ViewCompany(), "Company Maintenance", PopupWindow.PopupButtonType.Close);
            popupWindow.ShowDialog();
            await ReadCompaniesAsync();
        }

        /// <summary>
        /// Execute show status maintenace view
        /// </summary>
        private async void ExecuteShowStatusView()
        {
            PopupWindow popupWindow = new PopupWindow(new ViewStatus(), "Status Maintenance", PopupWindow.PopupButtonType.Close);
            popupWindow.ShowDialog();
            await ReadStatusesAsync();
        }

        /// <summary>
        /// Execute show client location maintenace view
        /// </summary>
        private async void ExecuteShowClientSiteView()
        {
            PopupWindow popupWindow = new PopupWindow(new ViewClientSite(), "Client Location Maintenance", PopupWindow.PopupButtonType.Close);
            popupWindow.ShowDialog();
            await ReadClientLocationsAsync();
        }

        /// <summary>
        /// Execute show package maintenace view
        /// </summary>
        private async void ExecuteShowPackageView()
        {
            PopupWindow popupWindow = new PopupWindow(new ViewPackage(), "Package Maintenance", PopupWindow.PopupButtonType.Close);
            popupWindow.ShowDialog();
            await ReadPackagesAsync();
        }

        /// <summary>
        /// Execute show suburb maintenace view
        /// </summary>
        private async void ExecuteShowSuburbView()
        {
            PopupWindow popupWindow = new PopupWindow(new ViewSuburb(), "Suburb Maintenance", PopupWindow.PopupButtonType.Close);
            popupWindow.ShowDialog();
            await ReadSuburbsAsync();
        }

        #endregion

        #endregion
    }
}
