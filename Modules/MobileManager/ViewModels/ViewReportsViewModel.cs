﻿using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.MobileManager.Model.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewReportsViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private ReportModel _model = null;
        private IEventAggregator _eventAggregator;
        private string _defaultItem = "-- Please Select --";

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand CityCommand { get; set; }
        public DelegateCommand ProvinceCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// The selected report description
        /// </summary>
        public string SelectedReportDescription
        {
            get { return _selectedReportDescription; }
            set { SetProperty(ref _selectedReportDescription, value); }
        }
        private string _selectedReportDescription = string.Empty;       

        /// <summary>
        /// Set the paramater view as selected 
        /// </summary>
        public bool ParamaterViewSelected
        {
            get { return _paramaterViewSelected; }
            set { SetProperty(ref _paramaterViewSelected, value); }
        }
        private bool _paramaterViewSelected = false;

        /// <summary>
        /// Set the report view as selected 
        /// </summary>
        public bool ReportViewSelected
        {
            get { return _reportViewSelected; }
            set { SetProperty(ref _reportViewSelected, value); }
        }
        private bool _reportViewSelected = true;

        /// <summary>
        /// Hide or show the billing period paramater 
        /// </summary>
        public Visibility BillingPeriodVisible
        {
            get { return _billingPeriodVisible; }
            set { SetProperty(ref _billingPeriodVisible, value); }
        }
        private Visibility _billingPeriodVisible = Visibility.Collapsed;

        /// <summary>
        /// Hide or show the invoice number paramater 
        /// </summary>
        public Visibility InvoiceNumberVisible
        {
            get { return _invoiceNumberVisible; }
            set { SetProperty(ref _invoiceNumberVisible, value); }
        }
        private Visibility _invoiceNumberVisible = Visibility.Collapsed;

        /// <summary>
        /// Hide or show the company paramater 
        /// </summary>
        public Visibility CompanyVisible
        {
            get { return _companyVisible; }
            set { SetProperty(ref _companyVisible, value); }
        }
        private Visibility _companyVisible = Visibility.Collapsed;

        /// <summary>
        /// Hide or show the usage limit paramater 
        /// </summary>
        public Visibility UsageLimitVisible
        {
            get { return _usageLimitVisible; }
            set { SetProperty(ref _usageLimitVisible, value); }
        }
        private Visibility _usageLimitVisible = Visibility.Collapsed;

        #region Required Fields

        /// <summary>
        /// The selected report category
        /// </summary>
        public string SelectedReportCategory
        {
            get { return _selectedReportCategory; }
            set
            {
                SetProperty(ref _selectedReportCategory, value);

                if (value != _defaultItem)
                    ReadReportsAsync();
            }
        }
        private string _selectedReportCategory = string.Empty;

        /// <summary>
        /// The selected report
        /// </summary>
        public Report SelectedReport
        {
            get { return _selectedReport; }
            set
            {
                SetProperty(ref _selectedReport, value);
                BillingPeriodVisible = InvoiceNumberVisible = CompanyVisible = UsageLimitVisible = Visibility.Collapsed;
                SelectedReportDescription = value != null && value.pkReportID > 0 ? value.ReportDescription : string.Empty;
                ReportViewSelected = false;
                ParamaterViewSelected = true;

                if (SelectedReport != null)
                {
                    switch (SelectedReport.ReportName)
                    {
                        case "Invoice":
                            BillingPeriodVisible = InvoiceNumberVisible = Visibility.Visible;
                            break;
                        case "APN Usage and Cost":
                        case "Daily APN Usage and Cost":
                            BillingPeriodVisible = CompanyVisible = Visibility.Visible;
                            ReadCompaniesAsync();
                            break;
                        case "Daily IPAD Usage and Cost":
                            BillingPeriodVisible = CompanyVisible = UsageLimitVisible = Visibility.Visible;
                            ReadCompaniesAsync();
                            break;
                    }

                    if( SelectedReport.ReportName != _defaultItem)
                        ReadBillingPeriodsAsync();
                }
            }
        }
        private Report _selectedReport;

        /// <summary>
        /// The selected billing period
        /// </summary>
        public string SelectedBillingPeriod
        {
            get { return _selectedBillingPeriod; }
            set
            {
                SetProperty(ref _selectedBillingPeriod, value);

                switch (SelectedReport.ReportName)
                {
                    case "Invoice":
                        ReadInvoiceNumbersAsync();
                        break;
                }
            }
        }
        private string _selectedBillingPeriod;

        /// <summary>
        /// The selected invoice number
        /// </summary>
        public string SelectedInvoiceNumber
        {
            get { return _selectedInvoiceNumber; }
            set { SetProperty(ref _selectedInvoiceNumber, value); }
        }
        private string _selectedInvoiceNumber;

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
        /// The selected usage limit
        /// </summary>
        public int SelectedUsageLimit
        {
            get { return _selectedUsageLimit; }
            set { SetProperty(ref _selectedUsageLimit, value); }
        }
        private int _selectedUsageLimit;

        #endregion

        #region View Lookup Data Collections

        /// <summary>
        /// The collection of report catgories from ReportCatgory enum
        /// </summary>
        public ObservableCollection<string> ReportCategoryCollection
        {
            get { return _reportCategoryCollection; }
            set { SetProperty(ref _reportCategoryCollection, value); }
        }
        private ObservableCollection<string> _reportCategoryCollection = null;

        /// <summary>
        /// The collection of report from the database
        /// </summary>
        public ObservableCollection<Report> ReportCollection
        {
            get { return _reportCollection; }
            set { SetProperty(ref _reportCollection, value); }
        }
        private ObservableCollection<Report> _reportCollection = null;

        /// <summary>
        /// The collection of billing periods from the database
        /// </summary>
        public List<string> BillingPeriodCollection
        {
            get { return _billingPeriodCollection; }
            set { SetProperty(ref _billingPeriodCollection, value); }
        }
        private List<string> _billingPeriodCollection = null;

        /// <summary>
        /// The collection of invoice numbers from the database
        /// </summary>
        public List<string> InvoiceNumberCollection
        {
            get { return _invoiceNumberCollection; }
            set { SetProperty(ref _invoiceNumberCollection, value); }
        }
        private List<string> _invoiceNumberCollection = null;

        /// <summary>
        /// The collection of companies from the database
        /// </summary>
        public ObservableCollection<Company> CompanyCollection
        {
            get { return _companyCollection; }
            set { SetProperty(ref _companyCollection, value); }
        }
        private ObservableCollection<Company> _companyCollection = null;

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidReportCategory
        {
            get { return _validReportCategory; }
            set { SetProperty(ref _validReportCategory, value); }
        }
        private Brush _validReportCategory = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidReport
        {
            get { return _validReport; }
            set { SetProperty(ref _validReport, value); }
        }
        private Brush _validReport = Brushes.Red;

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
                    case "SelectedReportCategory":
                        ValidReportCategory = string.IsNullOrEmpty(SelectedReportCategory) || SelectedReportCategory == _defaultItem ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedReport":
                        ValidReport = SelectedReport != null && SelectedReport.pkReportID < 1 ? Brushes.Red : Brushes.Silver; break;
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
        public ViewReportsViewModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            InitialiseReportView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private void InitialiseReportView()
        {
            _model = new ReportModel(_eventAggregator);

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecuteCancel).ObservesProperty(() => SelectedReportCategory);
            //AddCommand = new DelegateCommand(ExecuteAdd);
            //SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave).ObservesProperty(() => SelectedCityName)
            //                                                              .ObservesProperty(() => SelectedProvince);

            // Load the view data
            ReadReportCategories();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            SelectedReport = null;
            SelectedUsageLimit = 1;
        }

        #region Lookup Data Loading

        /// <summary>
        /// Load all the report categories from the ReportCategory enum
        /// </summary>
        private void ReadReportCategories()
        {
            try
            {
                ReportCategoryCollection = new ObservableCollection<string>();

                foreach (ReportType process in Enum.GetValues(typeof(ReportType)))
                {
                    ReportCategoryCollection.Add(EnumHelper.GetDescriptionFromEnum(process));
                }

                SelectedReportCategory = _defaultItem;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewReportsViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadReportCategories",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Load all the report categories from the ReportCategory enum
        /// </summary>
        private async void ReadReportsAsync()
        {
            try
            {
                ReportCollection = await Task.Run(() => _model.ReadReports(EnumHelper.GetEnumFromDescription<ReportType>(SelectedReportCategory)));
                SelectedReport = ReportCollection.Where(p => p.pkReportID == 0).FirstOrDefault();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewReportsViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadReportsAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Load all the billing periods from the database
        /// </summary>
        private async void ReadBillingPeriodsAsync()
        {
            try
            {
                BillingPeriodCollection = await Task.Run(() => new InvoiceModel(_eventAggregator).ReadBillingPeriods());
                SelectedBillingPeriod = BillingPeriodCollection.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewReportsViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadBillingPeriodsAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Load all the invoice numbers from the database
        /// </summary>
        private async void ReadInvoiceNumbersAsync()
        {
            try
            {
                InvoiceNumberCollection = await Task.Run(() => new InvoiceModel(_eventAggregator).ReadInvoiceNumbers(SelectedBillingPeriod));
                SelectedInvoiceNumber = InvoiceNumberCollection.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewReportsViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadInvoiceNumbersAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        /// <summary>
        /// Load all the companies from the database
        /// </summary>
        private async void ReadCompaniesAsync()
        {
            try
            {
                CompanyCollection = await Task.Run(() => new CompanyModel(_eventAggregator).ReadCompanies(true, true));
                SelectedCompany = CompanyCollection.FirstOrDefault();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("ViewReportsViewModel",
                                                                string.Format("Error! {0}, {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadCompaniesAsync",
                                                                ApplicationMessage.MessageTypes.SystemError));
            }
        }

        #endregion

        #region Command Execution

        /// <summary>
        /// Validate if the cancel functionality can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteCancel()
        {
            return !string.IsNullOrWhiteSpace(SelectedReportCategory);
        }

        /// <summary>
        /// Execute when the cancel command button is clicked 
        /// </summary>
        private void ExecuteCancel()
        {
            InitialiseViewControls();
        }

        /// <summary>
        /// Validate if the save functionality can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        //private bool CanExecuteSave()
        //{
        //    return !string.IsNullOrWhiteSpace(SelectedCityName) && SelectedProvince.pkProvinceID > 0;
        //}

        ///// <summary>
        ///// Execute when the save command button is clicked 
        ///// </summary>
        //private async void ExecuteSave()
        //{
        //    bool result = false;

        //    SelectedCity.CityName = SelectedCityName.ToUpper();
        //    SelectedCity.fkProvinceID = SelectedProvince.pkProvinceID;
        //    SelectedCity.ModifiedBy = SecurityHelper.LoggedInDomainName;
        //    SelectedCity.ModifiedDate = DateTime.Now;
        //    SelectedCity.IsActive = CityState;

        //    if (SelectedCity.pkCityID == 0)
        //        result = _model.CreateCity(SelectedCity);
        //    else
        //        result = _model.UpdateCity(SelectedCity);

        //    if (result)
        //    {
        //        InitialiseViewControls();
        //        await ReadCitysAsync();
        //    }
        //}

        /// <summary>
        /// Execute when the add command button is clicked 
        /// </summary>
        private void ExecuteAdd()
        {
            InitialiseViewControls();
        }

        #endregion

        #endregion
    }
}
