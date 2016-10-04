﻿using Prism.Mvvm;
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
        private bool _deviceDataSaved = false;
        private bool _simCardDataSaved = false;

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
                try
                {
                    SetProperty(ref _selectedClient, value);

                    if (value != null && value.pkClientID > 0)
                    {
                        // Set the client properties
                        if (CompanyCollection != null)
                            SelectedCompany = value.fkCompanyID > 0 ? CompanyCollection.Where(p => p.pkCompanyID == value.fkCompanyID).FirstOrDefault()
                                                                    : CompanyCollection.Where(p => p.pkCompanyID == 0).FirstOrDefault();
                        if (ClientLocationCollection != null)
                            SelectedClientLocation = value.fkClientLocationID > 0 ? ClientLocationCollection.Where(p => p.pkClientLocationID == value.fkClientLocationID).FirstOrDefault()
                                                                                  : ClientLocationCollection.Where(p => p.pkClientLocationID == 0).FirstOrDefault();
                        if (SuburbCollection != null)
                            SelectedSuburb = value.fkSuburbID > 0 ? SuburbCollection.Where(p => p.pkSuburbID == value.fkSuburbID).FirstOrDefault()
                                                                  : SuburbCollection.Where(p => p.pkSuburbID == 0).FirstOrDefault();
                        SelectedCellNumber = value.PrimaryCellNumber;
                        SelectedClientName = value.ClientName;
                        SelectedClientIDNumber = value.IDNumber;
                        SelectedClientAdminFee = value.AdminFee > 0 ? value.AdminFee.ToString() : SelectedCompany != null ? SelectedCompany.AdminFee.ToString() : "0";
                        SelectedClientAddressLine = value.AddressLine1;
                        SelectedClientWBSNumber = !string.IsNullOrWhiteSpace(value.WBSNumber) ? value.WBSNumber : SelectedCompany != null ? SelectedCompany.WBSNumber : null;
                        SelectedClientCostCode = !string.IsNullOrWhiteSpace(value.CostCode) ? value.CostCode : SelectedCompany != null ? SelectedCompany.CostCode : null;
                        SelectedClientIPAddress = !string.IsNullOrWhiteSpace(value.IPAddress) ? value.IPAddress : SelectedCompany != null ? SelectedCompany.IPAddress : null;
                        SelectedClientState = value.IsActive;
                        CompanyClient = value.IsPrivate ? false : true;
                        PrivateClient = value.IsPrivate ? true : false;
                        SaIDNumber = value.IsSaIDNumber ? true : false;
                        OtherIDNumber = value.IsSaIDNumber ? false : true;

                        // Set the contract properties
                        SelectedContract = value.Contract != null ? value.Contract : null;
                        if (StatusCollection != null)
                            SelectedStatus = value.Contract != null ? StatusCollection.Where(p => p.pkStatusID == value.Contract.fkStatusID).FirstOrDefault()
                                                                    : StatusCollection.Where(p => p.pkStatusID == 0).FirstOrDefault();
                        if (PackageCollection != null)
                            SelectedPackage = value.Contract != null ? PackageCollection.Where(p => p.pkPackageID == value.Contract.fkPackageID).FirstOrDefault()
                                                                     : PackageCollection.Where(p => p.pkPackageID == 0).FirstOrDefault();
                        SelectedCostType = value.Contract != null ? ((CostType)value.Contract.enCostType).ToString() : "NONE";
                        SelectedPackageType = SelectedPackage != null ? ((PackageType)SelectedPackage.enPackageType).ToString() : "NONE";
                        SelectedContractAccNumber = value.Contract != null ? value.Contract.AccountNumber : null;
                        SelectedPackageCost = value.Contract != null && value.Contract.PackageSetup != null ? value.Contract.PackageSetup.Cost.ToString() : "0";
                        SelectedPackageRandValue = value.Contract != null && value.Contract.PackageSetup != null ? value.Contract.PackageSetup.RandValue.ToString() : "0";
                        SelectedPackageMBData = value.Contract != null && value.Contract.PackageSetup != null ? value.Contract.PackageSetup.MBData.ToString() : "0";
                        SelectedPackageSMSNumber = value.Contract != null && value.Contract.PackageSetup != null ? value.Contract.PackageSetup.SMSNumber.ToString() : "0";
                        SelectedPackageTalkTime = value.Contract != null && value.Contract.PackageSetup != null ? value.Contract.PackageSetup.TalkTimeMinutes.ToString() : "0";
                        SelectedPackageSPULValue = value.Contract != null && value.Contract.PackageSetup != null ? value.Contract.PackageSetup.SPULValue.ToString() : "0";
                        SelectedContractStartDate = value.Contract != null && value.Contract.ContractStartDate != null ? value.Contract.ContractStartDate.Value : DateTime.MinValue;
                        SelectedContractEndDate = value.Contract != null && value.Contract.ContractEndDate.Value != null ? value.Contract.ContractEndDate.Value : DateTime.MinValue;

                        // Set billing properties
                        SetClientBilling(value.ClientBilling);

                        // Set the delete button to delete or un-delete client
                        DeleteButtonImage = value.IsActive ? "278.png" : "delete.png";
                        DeleteButtonToolTip = value.IsActive ? "in-active" : "active";

                        // The the global application properties
                        MobileManagerEnvironment.ClientID = value.pkClientID;
                        MobileManagerEnvironment.ClientCompanyID = value.fkCompanyID;
                        MobileManagerEnvironment.ClientContractID = value.fkContractID;
                        MobileManagerEnvironment.ClientPrimaryCell = value.PrimaryCellNumber;

                        // Publish these event to populate the devices, Simcards, accounts
                        // abd activity logs linked to the contract
                        if (value.Contract != null)
                        {
                            _activityLogInfo.EntityID = value.fkContractID;
                            _eventAggregator.GetEvent<SetActivityLogProcessEvent>().Publish(_activityLogInfo);
                            _eventAggregator.GetEvent<ReadSimCardsEvent>().Publish(value.fkContractID);
                            _eventAggregator.GetEvent<ReadDevicesEvent>().Publish(value.fkContractID);
                            _eventAggregator.GetEvent<ReadInvoicesEvent>().Publish(value.pkClientID);
                        }

                        // Publish this event to set the admin tab as default tab
                        _eventAggregator.GetEvent<NavigationEvent>().Publish(0);
                    }
                }
                catch (Exception ex)
                {
                    _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                    .Publish(new ApplicationMessage("ViewCellularViewModel",
                                                                    string.Format("Error! {0}, {1}.",
                                                                    ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                    "SelectedClient",
                                                                    ApplicationMessage.MessageTypes.SystemError));
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
        /// Indicate if its a company client
        /// </summary>
        public bool CompanyClient
        {
            get { return _companyClient; }
            set
            {
                SetProperty(ref _companyClient, value);
                if (value == true)
                    ValidAdminFee = !string.IsNullOrWhiteSpace(SelectedClientAdminFee) && Convert.ToDecimal(SelectedClientAdminFee) < 1 ? Brushes.Red : Brushes.Silver;
            }
        }
        private bool _companyClient = true;

        /// <summary>
        /// Indicate if its a private client
        /// </summary>
        public bool PrivateClient
        {
            get { return _privateClient; }
            set
            {
                SetProperty(ref _privateClient, value);
                if (value == true)
                    ValidAdminFee = Brushes.Silver;
            }
        }
        private bool _privateClient;

        /// <summary>
        /// Indicate if its a south african ID number
        /// </summary>
        public bool SaIDNumber
        {
            get { return _saIDNumber; }
            set
            {
                SetProperty(ref _saIDNumber, value);
                if (value == true)
                    ValidClientIDNumber = SelectedClientIDNumber == null || SelectedClientIDNumber.Length != 13 ? Brushes.Red : Brushes.Silver;
            }
        }
        private bool _saIDNumber = true;

        /// <summary>
        /// Indicate if its a other ID number
        /// </summary>
        public bool OtherIDNumber
        {
            get { return _otherIDNumber; }
            set
            {
                SetProperty(ref _otherIDNumber, value);
                if (value == true)
                    ValidClientIDNumber = Brushes.Silver;
            }
        }
        private bool _otherIDNumber;

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

        /// <summary>
        /// The entered package cost
        /// </summary>
        public string SelectedPackageCost
        {
            get { return _selectedPackageCost; }
            set { SetProperty(ref _selectedPackageCost, value); }
        }
        private string _selectedPackageCost = "0";

        /// <summary>
        /// The entered package rand value
        /// </summary>
        public string SelectedPackageRandValue
        {
            get { return _selectedPackageRandValue; }
            set { SetProperty(ref _selectedPackageRandValue, value); }
        }
        private string _selectedPackageRandValue = "0";

        /// <summary>
        /// The entered package MB data
        /// </summary>
        public string SelectedPackageMBData
        {
            get { return _selectedPackageMBData; }
            set { SetProperty(ref _selectedPackageMBData, value); }
        }
        private string _selectedPackageMBData = "0";

        /// <summary>
        /// The entered package SPUL value
        /// </summary>
        public string SelectedPackageSPULValue
        {
            get { return _selectedPackageSPULValue; }
            set { SetProperty(ref _selectedPackageSPULValue, value); }
        }
        private string _selectedPackageSPULValue = "0";

        /// <summary>
        /// The entered package talk time
        /// </summary>
        public string SelectedPackageTalkTime
        {
            get { return _selectedPackageTalkTime; }
            set { SetProperty(ref _selectedPackageTalkTime, value); }
        }
        private string _selectedPackageTalkTime = "0";

        /// <summary>
        /// The entered package SMS number
        /// </summary>
        public string SelectedPackageSMSNumber
        {
            get { return _selectedPackageSMSNumber; }
            set { SetProperty(ref _selectedPackageSMSNumber, value); }
        }
        private string _selectedPackageSMSNumber = "0";

        /// <summary>
        /// The delete button active/in-active image
        /// </summary>
        public string DeleteButtonImage
        {
            get { return string.Format("/Gijima.IOBM.MobileManager;component/Assets/Images/{0}", _deleteImage); }
            set { SetProperty(ref _deleteImage, value); }
        }
        private string _deleteImage = "278.png";

        /// <summary>
        /// The delete button active/in-active toooltip
        /// </summary>
        public string DeleteButtonToolTip
        {
            get { return string.Format("Set the selected user as {0}", _deleteButtonToolTip); }
            set { SetProperty(ref _deleteButtonToolTip, value); }
        }
        private string _deleteButtonToolTip = "active";

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
        /// The collection of company billing levels from the database
        /// </summary>
        public ObservableCollection<CompanyBillingLevel> BillingLevelCollection
        {
            get { return _billingLevelCollection; }
            set { SetProperty(ref _billingLevelCollection, value); }
        }
        private ObservableCollection<CompanyBillingLevel> _billingLevelCollection = null;

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
            set
            {
                SetProperty(ref _selectedCompany, value);
                if (value != null && value.pkCompanyID > 0)
                {
                    SetCompanyDefaults();
                    ReadCompanyBillingLevelsAsync();
                }
            }
        }
        private Company _selectedCompany;
        
        /// <summary>
        /// The selected company billing level
        /// </summary>
        public CompanyBillingLevel SelectedBillingLevel
        {
            get { return _selectedBillingLevel; }
            set
            {
                SetProperty(ref _selectedBillingLevel, value);

                if (SelectedBillingLevel != null)
                {
                    if ((SplitBilling && SelectedPackageType != null) || (value != null && value.pkCompanyBillingLevelID > 0))
                    {
                        if (((PackageType)Enum.Parse(typeof(PackageType), SelectedPackageType)) == PackageType.DATA)
                        {
                            AllowVoiceAllowance = false;
                            AllowWDPAllowance = true;
                            SelectedWDPAllowance = value != null ? value.Amount.ToString() : "0";
                        }
                        else if (SplitBilling && ((PackageType)Enum.Parse(typeof(PackageType), SelectedPackageType)) == PackageType.VOICE)
                        {
                            AllowVoiceAllowance = true;
                            AllowWDPAllowance = false;
                            SelectedVoiceAllowance = value != null ? value.Amount.ToString() : "0";
                        }
                    }
                    else
                    {
                        AllowVoiceAllowance = AllowWDPAllowance = false;
                    }
                }
            }
        }
        private CompanyBillingLevel _selectedBillingLevel;

        /// <summary>
        /// Indicated if company billing level can be selected
        /// </summary>
        public bool AllowBillingLevels
        {
            get { return _allowBillingLevels; }
            set { SetProperty(ref _allowBillingLevels, value); }
        }
        private bool _allowBillingLevels;

        /// <summary>
        /// Indicated if a WDP allowance must be entered
        /// </summary>
        public bool AllowWDPAllowance
        {
            get { return _allowWDPAllowance; }
            set { SetProperty(ref _allowWDPAllowance, value); }
        }
        private bool _allowWDPAllowance;

        /// <summary>
        /// Indicated if a Voice allowance must be entered
        /// </summary>
        public bool AllowVoiceAllowance
        {
            get { return _allowVoiceAllowance; }
            set { SetProperty(ref _allowVoiceAllowance, value); }
        }
        private bool _allowVoiceAllowance;

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
            set
            {
                SetProperty(ref _selectedPackage, value);
                SetPackageDefaults();
            }
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
            set { SetProperty(ref _selectedPackageType, value);}
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
            set
            {
                SetProperty(ref _splitBilling, value);

                if (BillingLevelCollection != null)
                {
                    AllowBillingLevels = value;
                    SelectedBillingLevel = BillingLevelCollection != null ? BillingLevelCollection.Where(p => p.pkCompanyBillingLevelID == 0).FirstOrDefault() : null;
                }
                else
                {
                    if (((PackageType)Enum.Parse(typeof(PackageType), SelectedPackageType)) == PackageType.DATA)
                    {
                        AllowVoiceAllowance = false;
                        AllowWDPAllowance = value;
                        SelectedWDPAllowance = "0";
                    }
                    else if (SplitBilling && ((PackageType)Enum.Parse(typeof(PackageType), SelectedPackageType)) == PackageType.VOICE)
                    {
                        AllowVoiceAllowance = value;
                        AllowWDPAllowance = false;
                        SelectedVoiceAllowance = "0";
                    }
                }
            }
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
                SetProperty(ref _noSplitBilling, value);

                if (value == true)
                {
                    SelectedClientBilling = new ClientBilling();
                    SelectedVoiceAllowance = SelectedWDPAllowance = "0";
                    SelectedBillingLevel = BillingLevelCollection != null ? BillingLevelCollection.Where(p => p.pkCompanyBillingLevelID == 0).FirstOrDefault() : null; 
                    SelectedIntRoaming = false;
                }
            }
        }
        private bool _noSplitBilling;
       
        /// <summary>
        /// The entered voice allowance
        /// </summary>
        public string SelectedVoiceAllowance
        {
            get { return _selectedVoiceAllowance; }
            set { SetProperty(ref _selectedVoiceAllowance, value); }
        }
        private string _selectedVoiceAllowance;

        /// <summary>
        /// The entered WDP allowance
        /// </summary>
        public string SelectedWDPAllowance
        {
            get { return _selectedWDPAllowance; }
            set { SetProperty(ref _selectedWDPAllowance, value); }
        }
        private string _selectedWDPAllowance;

        /// <summary>
        /// The entered SP allowance
        /// </summary>
        public string SelectedSPAllowance
        {
            get { return _selectedSPAllowance; }
            set { SetProperty(ref _selectedSPAllowance, value); }
        }
        private string _selectedSPAllowance;

        /// <summary>
        /// The entered allowance limit
        /// </summary>
        public string SelectedAllowanceLimit
        {
            get { return _selectedAllowanceLimit; }
            set { SetProperty(ref _selectedAllowanceLimit, value); }
        }
        private string _selectedAllowanceLimit;

        /// <summary>
        /// Indicate if client use int dailing
        /// </summary>
        public bool SelectedIntDailing
        {
            get { return _selectedIntDailing; }
            set
            {
                SetProperty(ref _selectedIntDailing, value);
                if (value == false)
                    SelectedPermanentDailing = value;
            }
        }
        private bool _selectedIntDailing;

        /// <summary>
        /// Indicate if client use permanent int dailing
        /// </summary>
        public bool SelectedPermanentDailing
        {
            get { return _selectedPermanentDailing; }
            set { SetProperty(ref _selectedPermanentDailing, value); }
        }
        private bool _selectedPermanentDailing;

        /// <summary>
        /// Indicate if client use roaming
        /// </summary>
        public bool SelectedIntRoaming
        {
            get { return _selectedIntRoaming; }
            set
            {
                SetProperty(ref _selectedIntRoaming, value);
                SelectedRoamingCountry = string.Empty;
                SelectedRoamingFromDate = SelectedRoamingToDate = value == true ? DateTime.Now : DateTime.MinValue;
                ValidSelectedRoaming = value;
                if (value == false)
                    SelectedPermanentRoaming = value;
            }
        }
        private bool _selectedIntRoaming;

        /// <summary>
        /// Indicate if client use permanent roaming
        /// </summary>
        public bool SelectedPermanentRoaming
        {
            get { return _selectedPermanentRoaming; }
            set
            {
                SetProperty(ref _selectedPermanentRoaming, value);
                if (value == true)
                {
                    SelectedRoamingCountry = string.Empty;
                    SelectedRoamingFromDate = SelectedRoamingToDate = DateTime.MinValue;
                    ValidSelectedRoaming = false;
                    ValidRoamingCountry = Brushes.Silver;
                }
                else
                {
                    SelectedRoamingCountry = string.Empty;
                    SelectedRoamingFromDate = SelectedRoamingToDate = SelectedIntRoaming ? DateTime.Now : DateTime.MinValue;
                    ValidSelectedRoaming = SelectedIntRoaming ? true : false;
                }
            }
        }
        private bool _selectedPermanentRoaming;

        /// <summary>
        /// Indicate if roaming dates must be enable/disabled
        /// </summary>
        public bool ValidSelectedRoaming
        {
            get { return _validSelectedRoaming; }
            set { SetProperty(ref _validSelectedRoaming, value); }
        }
        private bool _validSelectedRoaming;

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
            set
            {
                SetProperty(ref _selectedRoamingFromDate, value);

                if (value.Date > SelectedRoamingToDate.Date)
                    SelectedRoamingToDate = value;
            }
        }
        private DateTime _selectedRoamingFromDate;

        /// <summary>
        /// Indicate if client has split billing
        /// </summary>
        public DateTime SelectedRoamingToDate
        {
            get { return _selectedRoamingToDate; }
            set
            {
                SetProperty(ref _selectedRoamingToDate, value);

                if (SelectedRoamingFromDate.Date > value.Date)
                    SelectedRoamingFromDate = value;
            }
        }
        private DateTime _selectedRoamingToDate;

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
        public Brush ValidLocation
        {
            get { return _validLocation; }
            set { SetProperty(ref _validLocation, value); }
        }
        private Brush _validLocation = Brushes.Red;

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
        public Brush ValidBillingLevel
        {
            get { return _validBillingLevel; }
            set { SetProperty(ref _validBillingLevel, value); }
        }
        private Brush _validBillingLevel = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidWDPAllowance
        {
            get { return _validWDPAllowance; }
            set { SetProperty(ref _validWDPAllowance, value); }
        }
        private Brush _validWDPAllowance = Brushes.Red;

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
                        ValidClientIDNumber = string.IsNullOrEmpty(SelectedClientIDNumber) || (SaIDNumber && SelectedClientIDNumber.Length != 13) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedCompany":
                        ValidCompany = SelectedCompany != null && SelectedCompany.pkCompanyID < 1 ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedClientLocation":
                        ValidLocation = SelectedClientLocation != null && SelectedClientLocation.pkClientLocationID < 1 ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedClientWBSNumber":
                        ValidWBSNumber = string.IsNullOrEmpty(SelectedClientWBSNumber) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedClientCostCode":
                        ValidCostCode = string.IsNullOrEmpty(SelectedClientCostCode) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedClientAdminFee":
                        ValidAdminFee = !string.IsNullOrWhiteSpace(SelectedClientAdminFee) && (CompanyClient && Convert.ToDecimal(SelectedClientAdminFee) < 1) ? Brushes.Red : Brushes.Silver; break;
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
                                               SelectedContractEndDate.Date < SelectedContractStartDate.Date ||
                                               SelectedContractEndDate.Date == DateTime.MinValue.Date ? Brushes.Red : Brushes.Silver; break;
                    case "SplitBilling":
                        ValidSplitBilling = !SplitBilling && !NoSplitBilling ? Brushes.Red : Brushes.Silver;
                        ValidRoamingCountry = SelectedIntRoaming && string.IsNullOrEmpty(SelectedRoamingCountry) ? Brushes.Red : Brushes.Silver;
                        ValidRoamingFromDate = SplitBilling && SelectedIntRoaming && SelectedRoamingFromDate.Date == DateTime.MinValue.Date ? Brushes.Red : Brushes.Silver;
                        ValidRoamingToDate = SplitBilling && SelectedIntRoaming && (SelectedRoamingToDate.Date < SelectedRoamingFromDate.Date ||
                                             SelectedRoamingToDate.Date == DateTime.MinValue.Date) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedBillingLevel":
                        ValidBillingLevel = SplitBilling && BillingLevelCollection != null && BillingLevelCollection.Count > 0 && SelectedBillingLevel != null && SelectedBillingLevel.pkCompanyBillingLevelID < 1 ? Brushes.Red : Brushes.Silver; break;
                    case "NoSplitBilling":
                        if (NoSplitBilling)
                        {
                            ValidSplitBilling = ValidBillingLevel = ValidVoiceAllowance = ValidWDPAllowance = ValidRoamingCountry = ValidRoamingFromDate = ValidRoamingToDate = Brushes.Silver;
                            AllowVoiceAllowance = AllowWDPAllowance = AllowBillingLevels = false;
                        }
                        ValidSplitBilling = !SplitBilling && !NoSplitBilling ? Brushes.Red : Brushes.Silver; break;
                        //ValidBillingLevel = SplitBilling && !NoSplitBilling && BillingLevelCollection != null &&? Brushes.Red : Brushes.Silver; break;
                    case "SelectedIntRoaming":
                        ValidRoamingCountry = SelectedIntRoaming && string.IsNullOrEmpty(SelectedRoamingCountry) ? Brushes.Red : Brushes.Silver;
                        ValidRoamingFromDate = SplitBilling && SelectedIntRoaming && (SelectedRoamingFromDate == null ||
                                               SelectedRoamingFromDate.Date == DateTime.MinValue.Date) ? Brushes.Red : Brushes.Silver;
                        ValidRoamingToDate = SplitBilling && SelectedIntRoaming && (SelectedRoamingToDate == null ||
                                             SelectedRoamingToDate.Date < SelectedRoamingFromDate.Date ||
                                             SelectedRoamingToDate.Date == DateTime.MinValue.Date) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedPermanentRoaming":
                        ValidRoamingCountry = !SelectedPermanentRoaming && string.IsNullOrEmpty(SelectedRoamingCountry) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedVoiceAllowance":
                        ValidVoiceAllowance = SplitBilling && AllowVoiceAllowance && ((PackageType)Enum.Parse(typeof(PackageType), SelectedPackageType)) == PackageType.VOICE &&
                                              (string.IsNullOrWhiteSpace(SelectedVoiceAllowance) || 
                                               Convert.ToDecimal(SelectedVoiceAllowance) < 1) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedWDPAllowance":
                        ValidWDPAllowance = SplitBilling && AllowWDPAllowance && ((PackageType)Enum.Parse(typeof(PackageType), SelectedPackageType)) == PackageType.DATA &&
                                            (string.IsNullOrWhiteSpace(SelectedWDPAllowance) || 
                                             Convert.ToDecimal(SelectedWDPAllowance) < 1) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedRoamingCountry":
                        ValidRoamingCountry = SelectedIntRoaming && string.IsNullOrEmpty(SelectedRoamingCountry) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedRoamingFromDate":
                        ValidRoamingFromDate = SplitBilling && SelectedIntRoaming && !SelectedPermanentRoaming && SelectedRoamingFromDate.Date == DateTime.MinValue.Date ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedRoamingToDate":
                        ValidRoamingToDate = SplitBilling && SelectedIntRoaming && !SelectedPermanentRoaming && (SelectedRoamingToDate.Date < SelectedRoamingFromDate.Date ||
                                             SelectedRoamingToDate.Date == DateTime.MinValue.Date) ? Brushes.Red : Brushes.Silver; break;
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

        /// <summary>
        /// Call the process based on the sender's completed action.
        /// </summary>
        /// <param name="sender">The completed action.</param>
        private void ActionCompleted_Event(ActionCompleted sender)
        {
            switch (sender)
            {
                case ActionCompleted.SaveContractDevices:
                    _deviceDataSaved = true; 
                    break;
                case ActionCompleted.SaveContractSimCards:
                    _simCardDataSaved = true;
                    break;
            }

            if (_deviceDataSaved && _simCardDataSaved)
            {
                _deviceDataSaved = _simCardDataSaved = false;
                InitialiseViewControls();
            }
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

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecuteCancel).ObservesProperty(() => SelectedCellNumber);
            AddCommand = new DelegateCommand(ExecuteAdd, CanExecuteAdd).ObservesProperty(() => SelectedClient);
            DeleteCommand = new DelegateCommand(ExecuteDelete, CanExecuteDelete).ObservesProperty(() => SelectedClient)
                                                                                .ObservesProperty(() => SelectedStatus);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave).ObservesProperty(() => SelectedCellNumber)
                                                                          .ObservesProperty(() => SelectedClientName)
                                                                          .ObservesProperty(() => SelectedClientIDNumber)
                                                                          .ObservesProperty(() => CompanyClient)
                                                                          .ObservesProperty(() => PrivateClient)
                                                                          .ObservesProperty(() => SaIDNumber)
                                                                          .ObservesProperty(() => OtherIDNumber)
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
                                                                          .ObservesProperty(() => SelectedBillingLevel)
                                                                          .ObservesProperty(() => SelectedWDPAllowance)
                                                                          .ObservesProperty(() => SelectedVoiceAllowance)
                                                                          .ObservesProperty(() => SelectedIntRoaming)
                                                                          .ObservesProperty(() => SelectedPermanentRoaming)
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

            // Subscribe to this event to be notified when the device and simcard data was saved
            _eventAggregator.GetEvent<ActionCompletedEvent>().Subscribe(ActionCompleted_Event, true);

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
            SelectedClient = null;
            SelectedContract = null;
            SelectedCompany = CompanyCollection != null ? CompanyCollection.Where(p => p.pkCompanyID == 0).FirstOrDefault() : null;
            SelectedClientLocation = ClientLocationCollection != null ? ClientLocationCollection.Where(p => p.pkClientLocationID == 0).FirstOrDefault() : null;
            SelectedSuburb = SuburbCollection != null ? SuburbCollection.Where(p => p.pkSuburbID == 0).FirstOrDefault() : null;
            SelectedStatus = StatusCollection != null ? StatusCollection.Where(p => p.pkStatusID == 0).FirstOrDefault() : null;
            SelectedPackage = PackageCollection != null ? PackageCollection.Where(p => p.pkPackageID == 0).FirstOrDefault() : null;
            SetClientBilling(null);
            SelectedCellNumber = SelectedClientName = SelectedClientIDNumber = SelectedClientAddressLine = SelectedContractAccNumber = string.Empty;
            SelectedClientWBSNumber = SelectedClientCostCode = SelectedClientIPAddress = string.Empty;
            SelectedContractStartDate = SelectedContractEndDate = DateTime.MinValue;
            SelectedClientAdminFee = "0";
            SelectedClientState = SaIDNumber = CompanyClient = true;
            SelectedCostType = SelectedPackageType = "NONE";
            DeleteButtonImage = "278.png";
            DeleteButtonToolTip = "active";
            MobileManagerEnvironment.ClientID = 0;
            MobileManagerEnvironment.ClientCompanyID = 0;
            MobileManagerEnvironment.ClientContractID = 0;
            MobileManagerEnvironment.ClientPrimaryCell = string.Empty;

            // Publish the event to clear the device view
            _eventAggregator.GetEvent<ReadDevicesEvent>().Publish(0);

            // Publish the event to clear the Sim card view
            _eventAggregator.GetEvent<ReadSimCardsEvent>().Publish(0);

            // Publish the event to clear the Accounts view
            _eventAggregator.GetEvent<ReadInvoicesEvent>().Publish(0);

            // Publish this event to set the admin tab as default tab
            _eventAggregator.GetEvent<NavigationEvent>().Publish(0);
        }

        /// <summary>
        /// Read client data from the database
        /// </summary>
        /// <param name="clientID">The client ID.</param>
        private async Task ReadClientAsync(int clientID)
        {
            try
            {
                InitialiseViewControls();
                SelectedClient = await Task.Run(() => _model.ReadClient(clientID));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewCellularViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadClientAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }
       
        /// <summary>
        /// Set the company defaults for the client 
        /// </summary>
        private void SetCompanyDefaults()
        {
            try
            {
                if (SelectedCompany != null)
                {
                    SelectedClientWBSNumber = SelectedCompany.WBSNumber != null ? SelectedCompany.WBSNumber : string.Empty;
                    SelectedClientCostCode = SelectedCompany.CostCode != null ? SelectedCompany.CostCode : string.Empty;
                    SelectedClientIPAddress = SelectedCompany.IPAddress != null ? SelectedCompany.IPAddress : string.Empty;
                    SelectedClientAdminFee = SelectedCompany.AdminFee.ToString();
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                .Publish(new ApplicationMessage("ViewCellularViewModel",
                                                string.Format("Error! {0}, {1}.",
                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                "SetCompanyDefaults",
                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Set the package defaults for the client 
        /// </summary>
        private void SetPackageDefaults()
        {
            try
            {
                if (SelectedPackage != null)
                {
                    SelectedPackageType = ((PackageType)SelectedPackage.enPackageType).ToString();
                    SelectedPackageCost = SelectedPackage.Cost.ToString();
                    SelectedPackageRandValue = SelectedPackage.RandValue.ToString();
                    SelectedPackageMBData = SelectedPackage.MBData.ToString();
                    SelectedPackageSMSNumber = SelectedPackage.SMSNumber.ToString();
                    SelectedPackageTalkTime = SelectedPackage.TalkTimeMinutes.ToString();
                    SelectedPackageSPULValue = SelectedPackage.SPULValue.ToString();
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                .Publish(new ApplicationMessage("ViewCellularViewModel",
                                                string.Format("Error! {0}, {1}.",
                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                "SetPackageDefaults",
                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Set the options for client billing
        /// </summary>
        /// <param name="clientBilling">The client billing entity.</param>
        private void SetClientBilling(ClientBilling clientBilling)
        {
            try
            { 
                SelectedClientBilling = clientBilling;

                if (clientBilling != null && clientBilling.IsSplitBilling)
                {
                    SplitBilling = true;
                    NoSplitBilling = false;
                    SelectedBillingLevel = BillingLevelCollection != null ? BillingLevelCollection.Where(p => p.pkCompanyBillingLevelID == clientBilling.fkCompanyBillingLevelID).FirstOrDefault() : null;
                    SelectedVoiceAllowance = clientBilling.VoiceAllowance.ToString();
                    SelectedWDPAllowance = clientBilling.WDPAllowance.ToString();
                    SelectedSPAllowance = clientBilling.SPLimit.ToString();
                    SelectedAllowanceLimit = clientBilling.AllowanceLimit.ToString();
                    SelectedIntRoaming = clientBilling.InternationalRoaming;
                    SelectedRoamingCountry = clientBilling.CountryVisiting;
                    SelectedRoamingFromDate = clientBilling.RoamingFromDate != null ? clientBilling.RoamingFromDate.Value : DateTime.MinValue.Date;
                    SelectedRoamingToDate = clientBilling.RoamingToDate != null ? clientBilling.RoamingToDate.Value : DateTime.MinValue.Date;
                }
                else
                {
                    SplitBilling = false;
                    NoSplitBilling = true;
                    SelectedBillingLevel = null;
                    SelectedVoiceAllowance = SelectedWDPAllowance = "0";
                    AllowVoiceAllowance = AllowWDPAllowance = false;
                    SelectedIntRoaming = false;
                    SelectedRoamingCountry = string.Empty;
                    SelectedRoamingFromDate = DateTime.MinValue.Date;
                    SelectedRoamingToDate = DateTime.MinValue.Date;

                    if (clientBilling != null)
                    {
                        SelectedSPAllowance = clientBilling.SPLimit.ToString();
                        SelectedAllowanceLimit = clientBilling.AllowanceLimit.ToString();
                    }
                    else
                    {
                        SelectedSPAllowance =  SelectedAllowanceLimit = "0";
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                .Publish(new ApplicationMessage("ViewCellularViewModel",
                                                string.Format("Error! {0}, {1}.",
                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                "SetClientBilling",
                                                ApplicationMessage.MessageTypes.SystemError));
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
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewCellularViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadCompaniesAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
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
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewCellularViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadClientLocationsAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
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
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewCellularViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadSuburbsAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
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
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewCellularViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadStatusesAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
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
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewCellularViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadPackagesAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
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

        /// <summary>
        /// Populate the company billing levels from the selected company from the database
        /// </summary>
        private async void ReadCompanyBillingLevelsAsync()
        {
            try
            {
                BillingLevelCollection = null;

                if (SelectedCompany != null && SelectedCompany.pkCompanyID > 0 && SelectedCompany.fkCompanyGroupID != null)
                    BillingLevelCollection = await Task.Run(() => new CompanyBillingLevelModel(_eventAggregator).ReadCompanyBillingLevels(SelectedCompany.fkCompanyGroupID.Value));

                if (BillingLevelCollection != null && BillingLevelCollection.Count > 1)
                {
                    AllowBillingLevels = SplitBilling ? true : false;
                    SelectedBillingLevel = BillingLevelCollection != null && SelectedClientBilling != null ? 
                                           BillingLevelCollection.Where(p => p.pkCompanyBillingLevelID == SelectedClientBilling.fkCompanyBillingLevelID).FirstOrDefault() : null;

                    // If company changed and the new company have billing levels then set the
                    // seletced billing level to the default
                    if (BillingLevelCollection != null && BillingLevelCollection.Count > 1 && SelectedBillingLevel == null)
                        SelectedBillingLevel = BillingLevelCollection.Where(p => p.pkCompanyBillingLevelID == 0).FirstOrDefault();
                }
                else
                {
                    BillingLevelCollection = null;
                    SelectedBillingLevel = null;
                    AllowBillingLevels = false;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewCellularViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadCompanyBillingLevelsAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
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
        }

        /// <summary>
        /// Validate if the save client data can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteAdd()
        {
            return true;
        }

        /// <summary>
        /// Execute the add new client process
        /// </summary>
        private void ExecuteAdd()
        {
            InitialiseViewControls();
            SelectedClient = new Client();
            SelectedClientState = true;
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
                                                                        SelectedStatus.StatusDescription == "REALLOCATED" ||
                                                                        SelectedStatus.StatusDescription == "TRANSFERED") ? true : false;
        }

        /// <summary>
        /// Execute the delete new client process
        /// </summary>
        private void ExecuteDelete()
        {
            SelectedClientState = SelectedClientState ? false : true;
            DeleteButtonImage = SelectedClientState ? "278.png" : "delete.png";
            DeleteButtonToolTip = SelectedClientState ? "in-active" : "active";
        }

        /// <summary>
        /// Validate if the save client data can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteSave()
        {
            bool result = false;

            result = SelectedClient != null ? true : false;

            // Validate if the logged-in user can administrate the company the client is linked to
            if (result)
                result = SelectedCompany != null && SelectedCompany.pkCompanyID > 0 && _securityHelper.IsUserInCompany(SelectedCompany.pkCompanyID) ? true : false;

            // Validate client data
            if (result)
                result = SelectedClient != null && SelectedClientLocation != null && SelectedClientLocation.pkClientLocationID > 0 && 
                         SelectedCompany != null && SelectedCompany.pkCompanyID > 0 && SelectedSuburb != null && SelectedSuburb.pkSuburbID > 0 &&
                         !string.IsNullOrEmpty(SelectedCellNumber) && SelectedCellNumber.Length == 10 && !string.IsNullOrEmpty(SelectedClientName) &&
                         !string.IsNullOrEmpty(SelectedClientIDNumber) && (SaIDNumber ? SelectedClientIDNumber.Length == 13 : true) &&  
                         !string.IsNullOrEmpty(SelectedClientWBSNumber) &&
                         !string.IsNullOrEmpty(SelectedClientCostCode) && !string.IsNullOrEmpty(SelectedClientAddressLine) &&
                         !string.IsNullOrWhiteSpace(SelectedClientAdminFee) && (CompanyClient ? Convert.ToDecimal(SelectedClientAdminFee) > 0 : true);

            // Validate contract data
            if (result)
                result = SelectedPackage != null && SelectedStatus != null && SelectedCostType != CostType.NONE.ToString() &&
                         !string.IsNullOrEmpty(SelectedContractAccNumber) && SelectedContractStartDate.Date > DateTime.MinValue.Date &&
                         SelectedContractEndDate.Date > DateTime.MinValue.Date;

            // Validate billing data
            if (result && SplitBilling)
                result = SelectedClientBilling != null && (SplitBilling || NoSplitBilling) && 
                         (SplitBilling && BillingLevelCollection == null ? true :
                          SplitBilling && BillingLevelCollection.Count > 1 && SelectedBillingLevel != null ? SelectedBillingLevel.pkCompanyBillingLevelID > 0 : false) && 
                         (SplitBilling ? (!string.IsNullOrWhiteSpace(SelectedWDPAllowance) && Convert.ToDecimal(SelectedWDPAllowance) > 0 ||
                                          !string.IsNullOrWhiteSpace(SelectedVoiceAllowance) && Convert.ToDecimal(SelectedVoiceAllowance) > 0) : false) && 
                         (SelectedIntRoaming && !SelectedPermanentRoaming ? !string.IsNullOrEmpty(SelectedRoamingCountry) : true) &&
                         (SplitBilling && SelectedIntRoaming && !SelectedPermanentRoaming ? SelectedRoamingFromDate.Date > DateTime.MinValue.Date : true) &&
                         (SplitBilling && SelectedIntRoaming && !SelectedPermanentRoaming ? SelectedRoamingToDate.Date > SelectedRoamingFromDate.Date && SelectedRoamingToDate.Date > DateTime.MinValue.Date : true);

            return result;
        }

        /// <summary>
        /// Execute when the save command button is clicked 
        /// </summary>
        private void ExecuteSave()
        {
            bool result = false;

            try
            {
                // Client Data
                SelectedClient.PrimaryCellNumber = SelectedCellNumber.Trim();
                SelectedClient.ClientName = SelectedClientName.ToUpper().Trim();
                SelectedClient.IsSaIDNumber = SaIDNumber;
                SelectedClient.IDNumber = SelectedClientIDNumber;
                SelectedClient.fkCompanyID = SelectedCompany.pkCompanyID;
                SelectedClient.fkClientLocationID = SelectedClientLocation.pkClientLocationID;
                SelectedClient.WBSNumber = SelectedClientWBSNumber.ToUpper().Trim();
                SelectedClient.CostCode = SelectedClientCostCode.ToUpper().Trim();
                SelectedClient.IsPrivate = PrivateClient;
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
                SelectedClient.Contract.ContractUpgradeDate = SelectedContract != null ? SelectedContract.ContractUpgradeDate : null;
                SelectedClient.Contract.PaymentCancelDate = SelectedContract != null ? SelectedContract.PaymentCancelDate : null;
                SelectedClient.Contract.ModifiedBy = SecurityHelper.LoggedInUserFullName;
                SelectedClient.Contract.ModifiedDate = DateTime.Now;
                SelectedClient.Contract.IsActive = SelectedClientState;
                // Package Setup Data
                if (SelectedClient.Contract.PackageSetup == null)
                    SelectedClient.Contract.PackageSetup = new PackageSetup();
                SelectedClient.Contract.PackageSetup.Cost = !string.IsNullOrEmpty(SelectedPackageCost) ? Convert.ToDecimal(SelectedPackageCost) : 0;
                SelectedClient.Contract.PackageSetup.TalkTimeMinutes = !string.IsNullOrEmpty(SelectedPackageTalkTime) ? Convert.ToInt32(SelectedPackageTalkTime) : 0;
                SelectedClient.Contract.PackageSetup.SMSNumber = !string.IsNullOrEmpty(SelectedPackageSMSNumber) ? Convert.ToInt32(SelectedPackageSMSNumber) : 0;
                SelectedClient.Contract.PackageSetup.MBData = !string.IsNullOrEmpty(SelectedPackageMBData) ? Convert.ToInt32(SelectedPackageMBData) : 0;
                SelectedClient.Contract.PackageSetup.RandValue = !string.IsNullOrEmpty(SelectedPackageRandValue) ? Convert.ToDecimal(SelectedPackageRandValue) : 0;
                SelectedClient.Contract.PackageSetup.SPULValue = !string.IsNullOrEmpty(SelectedPackageSPULValue) ? Convert.ToDecimal(SelectedPackageSPULValue) : 0;
                SelectedClient.Contract.PackageSetup.ModifiedBy = SecurityHelper.LoggedInUserFullName;
                SelectedClient.Contract.PackageSetup.ModifiedDate = DateTime.Now;
                SelectedClient.Contract.PackageSetup.IsActive = SelectedClientState;
                // Billing Data
                if (SelectedClient.ClientBilling == null)
                    SelectedClient.ClientBilling = new ClientBilling();
                SelectedClient.ClientBilling.IsSplitBilling = SplitBilling;
                SelectedClient.ClientBilling.fkCompanyBillingLevelID = SelectedBillingLevel != null ? SelectedBillingLevel.pkCompanyBillingLevelID : (Int32?)null;
                SelectedClient.ClientBilling.WDPAllowance = Convert.ToDecimal(SelectedWDPAllowance);
                SelectedClient.ClientBilling.VoiceAllowance = Convert.ToDecimal(SelectedVoiceAllowance);
                SelectedClient.ClientBilling.SPLimit = !string.IsNullOrEmpty(SelectedSPAllowance) ? Convert.ToDecimal(SelectedSPAllowance) : 0; 
                SelectedClient.ClientBilling.AllowanceLimit = !string.IsNullOrEmpty(SelectedAllowanceLimit) ? Convert.ToDecimal(SelectedAllowanceLimit) : 0;
                SelectedClient.ClientBilling.InternationalDailing = SelectedClientBilling.InternationalDailing;
                SelectedClient.ClientBilling.PermanentIntDailing = SelectedPermanentDailing;
                SelectedClient.ClientBilling.InternationalRoaming = SelectedIntRoaming;
                SelectedClient.ClientBilling.PermanentIntRoaming = SelectedPermanentRoaming;
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

                // Auto save the device and sim card data
                if (result)
                {
                    _eventAggregator.GetEvent<SaveDeviceEvent>().Publish(SelectedContract.pkContractID);
                    _eventAggregator.GetEvent<SaveSimCardEvent>().Publish(SelectedContract.pkContractID);
                }

                // Publish the event to read the administration activity logs
                if (SelectedContract != null)
                {
                    _activityLogInfo.EntityID = SelectedContract.pkContractID;
                    _eventAggregator.GetEvent<SetActivityLogProcessEvent>().Publish(_activityLogInfo);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                .Publish(new ApplicationMessage("ViewCellularViewModel",
                                                string.Format("Error! {0}, {1}.",
                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                "ExecuteSave",
                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Validate if the maintenace functionality can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteMaintenace()
        {
            return _securityHelper.IsUserInRole(SecurityRole.Administrator.Value()) || _securityHelper.IsUserInRole(SecurityRole.DataManager.Value());
        }

        /// <summary>
        /// Execute show company maintenace view
        /// </summary>
        private async void ExecuteShowCompanyView()
        {
            int selectedCompanyID = SelectedCompany.pkCompanyID;
            PopupWindow popupWindow = new PopupWindow(new ViewCompany(), "Company Maintenance", PopupWindow.PopupButtonType.Close);
            popupWindow.ShowDialog();
            await ReadCompaniesAsync();
            SelectedCompany = CompanyCollection.Where(p => p.pkCompanyID == selectedCompanyID).FirstOrDefault();
        }

        /// <summary>
        /// Execute show status maintenace view
        /// </summary>
        private async void ExecuteShowStatusView()
        {
            int selectedStatusID = SelectedStatus.pkStatusID;
            PopupWindow popupWindow = new PopupWindow(new ViewStatus(), "Status Maintenance", PopupWindow.PopupButtonType.Close);
            popupWindow.ShowDialog();
            await ReadStatusesAsync();
            SelectedStatus = StatusCollection.Where(p => p.pkStatusID == selectedStatusID).FirstOrDefault();
        }

        /// <summary>
        /// Execute show client location maintenace view
        /// </summary>
        private async void ExecuteShowClientSiteView()
        {
            int selectedSiteID = SelectedClientLocation.pkClientLocationID;
            PopupWindow popupWindow = new PopupWindow(new ViewClientSite(), "Client Location Maintenance", PopupWindow.PopupButtonType.Close);
            popupWindow.ShowDialog();
            await ReadClientLocationsAsync();
            SelectedClientLocation = ClientLocationCollection.Where(p => p.pkClientLocationID == selectedSiteID).FirstOrDefault();
        }

        /// <summary>
        /// Execute show package maintenace view
        /// </summary>
        private async void ExecuteShowPackageView()
        {
            int selectedPackageID = SelectedPackage.pkPackageID;
            PopupWindow popupWindow = new PopupWindow(new ViewPackage(), "Package Maintenance", PopupWindow.PopupButtonType.Close);
            popupWindow.ShowDialog();
            await ReadPackagesAsync();
            SelectedPackage = PackageCollection.Where(p => p.pkPackageID == selectedPackageID).FirstOrDefault();
        }

        /// <summary>
        /// Execute show suburb maintenace view
        /// </summary>
        private async void ExecuteShowSuburbView()
        {
            int selectedSuburbID = SelectedSuburb.pkSuburbID;
            PopupWindow popupWindow = new PopupWindow(new ViewSuburb(), "Suburb Maintenance", PopupWindow.PopupButtonType.Close);
            popupWindow.ShowDialog();
            await ReadSuburbsAsync();
            SelectedSuburb = SuburbCollection.Where(p => p.pkSuburbID == selectedSuburbID).FirstOrDefault();
        }

        #endregion

        #endregion
    }
}
