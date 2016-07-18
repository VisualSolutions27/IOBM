using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Events;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.MobileManager.Model.Models;
using Gijima.IOBM.MobileManager.Security;
using Gijima.IOBM.MobileManager.Views;
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
    public class ViewDeviceViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private DevicesModel _model = null;
        private IEventAggregator _eventAggregator;
        private SecurityHelper _securityHelper = null;
        private int _selectedContractID = 0;

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }
        public DelegateCommand DeviceMakeCommand { get; set; }
        public DelegateCommand DeviceModelCommand { get; set; }
        public DelegateCommand DeviceStatusCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the selected (current) device entity
        /// </summary>
        public Device SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                // Prevent circullar action
                if (value != null && value.pkDeviceID != _selectedDevice.pkDeviceID)
                {
                    // Device required fields
                    SelectedDeviceMake = DeviceMakeCollection != null ? value.fkDeviceMakeID != null ? 
                                         DeviceMakeCollection.Where(p => p.pkDeviceMakeID == value.fkDeviceMakeID).FirstOrDefault() :
                                         DeviceMakeCollection.Where(p => p.pkDeviceMakeID == 0).FirstOrDefault() : null;
                    SelectedDeviceModel = DeviceModelCollection != null ? DeviceModelCollection.Where(p => p.pkDeviceModelID == value.fkDeviceModelID).FirstOrDefault() : null;
                    SelectedSimmCard = SimmCardCollection != null ? value.fkSimmCardID != null ?
                                       SimmCardCollection.Where(p => p.pkSimmCardID == value.fkSimmCardID).FirstOrDefault() :
                                       SimmCardCollection.Where(p => p.pkSimmCardID == 0).FirstOrDefault() : null;
                    SelectedStatus = StatusCollection != null ? StatusCollection.Where(p => p.pkStatusID == value.fkStatusID).FirstOrDefault() : null;
                    SelectedReceivedDate = value.ReceiveDate != null ? value.ReceiveDate : DateTime.MinValue;
                    SelectedIMENumber = value.IMENumber;
                    DeviceState = value.IsActive;

                    // Insurance required fields
                    if (value.InsuranceCost != null && value.InsuranceValue != null)
                        SetDeviceInsurance(value.InsuranceCost.Value, value.InsuranceValue.Value);

                    SetProperty(ref _selectedDevice, value);

                    // Link the device to its simmcard
                    LinkDeviceToSimmCard();
                }
            }
        }
        private Device _selectedDevice = new Device();

        /// <summary>
        /// The selected simm card
        /// </summary>
        public SimmCard SelectedSimmCard
        {
            get { return _selectedSimmCard; }
            set { SetProperty(ref _selectedSimmCard, value); }
        }
        private SimmCard _selectedSimmCard;

        /// <summary>
        /// The selected device state
        /// </summary>
        public bool DeviceState
        {
            get { return _deviceState; }
            set { SetProperty(ref _deviceState, value); }
        }
        private bool _deviceState;

        /// <summary>
        /// The collection of devices from the database
        /// </summary>
        public ObservableCollection<Device> DeviceCollection
        {
            get { return _deviceCollection; }
            set { SetProperty(ref _deviceCollection, value); }
        }
        private ObservableCollection<Device> _deviceCollection = null;

        #region View Lookup Data Collections

        /// <summary>
        /// The collection of device makes from the database
        /// </summary>
        public ObservableCollection<DeviceMake> DeviceMakeCollection
        {
            get { return _deviceMakeCollection; }
            set { SetProperty(ref _deviceMakeCollection, value); }
        }
        private ObservableCollection<DeviceMake> _deviceMakeCollection = null;

        /// <summary>
        /// The collection of device models from the database
        /// </summary>
        public ObservableCollection<DeviceModel> DeviceModelCollection
        {
            get { return _deviceModelCollection; }
            set { SetProperty(ref _deviceModelCollection, value); }
        }
        private ObservableCollection<DeviceModel> _deviceModelCollection = null;

        /// <summary>
        /// The collection of contract simmcards from the database
        /// </summary>
        public ObservableCollection<SimmCard> SimmCardCollection
        {
            get { return _simmCardCollection; }
            set { SetProperty(ref _simmCardCollection, value); }
        }
        private ObservableCollection<SimmCard> _simmCardCollection = null;

        /// <summary>
        /// The collection of statuses from the database
        /// </summary>
        public ObservableCollection<Status> StatusCollection
        {
            get { return _statusCollection; }
            set { SetProperty(ref _statusCollection, value); }
        }
        private ObservableCollection<Status> _statusCollection = null;

        #endregion

        #region Required Fields

        /// <summary>
        /// The selected device make
        /// </summary>
        public DeviceMake SelectedDeviceMake
        {
            get { return _selectedDeviceMake; }
            set
            {
                SetProperty(ref _selectedDeviceMake, value);
                ReadDeviceMakeModelsAsync();
            }
        }
        private DeviceMake _selectedDeviceMake;

        /// <summary>
        /// The selected device model
        /// </summary>
        public DeviceModel SelectedDeviceModel
        {
            get { return _selectedDeviceModel; }
            set { SetProperty(ref _selectedDeviceModel, value); }
        }
        private DeviceModel _selectedDeviceModel;

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
        /// The selected received date
        /// </summary>
        public DateTime SelectedReceivedDate
        {
            get { return _selectedReceivedDate; }
            set { SetProperty(ref _selectedReceivedDate, value); }
        }
        private DateTime _selectedReceivedDate;

        /// <summary>
        /// The entered IME number
        /// </summary>
        public string SelectedIMENumber
        {
            get { return _selectedIMENumber; }
            set { SetProperty(ref _selectedIMENumber, value); }
        }
        private string _selectedIMENumber;

        /// <summary>
        /// Indicate if a device has insurance
        /// </summary>
        public bool DeviceInsuranceYes
        {
            get { return _deviceInsuranceYes; }
            set { SetProperty(ref _deviceInsuranceYes, value); }
        }
        private bool _deviceInsuranceYes;

        /// <summary>
        /// Indicate if a device has insurance
        /// </summary>
        public bool DeviceInsuranceNo
        {
            get { return _deviceInsuranceNo; }
            set { SetProperty(ref _deviceInsuranceNo, value); }
        }
        private bool _deviceInsuranceNo;

        /// <summary>
        /// The entered insurance value
        /// </summary>
        public decimal SelectedInsuranceValue
        {
            get { return _selectedInsuranceValue; }
            set { SetProperty(ref _selectedInsuranceValue, value); }
        }
        private decimal _selectedInsuranceValue;

        /// <summary>
        /// The entered insurance cost
        /// </summary>
        public decimal SelectedInsuranceCost
        {
            get { return _selectedInsuranceCost; }
            set { SetProperty(ref _selectedInsuranceCost, value); }
        }
        private decimal _selectedInsuranceCost;

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidDeviceMake
        {
            get { return _validDeviceMake; }
            set { SetProperty(ref _validDeviceMake, value); }
        }
        private Brush _validDeviceMake = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidDeviceModel
        {
            get { return _validDeviceModel; }
            set { SetProperty(ref _validDeviceModel, value); }
        }
        private Brush _validDeviceModel = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidIMENumber
        {
            get { return _validIMENumber; }
            set { SetProperty(ref _validIMENumber, value); }
        }
        private Brush _validIMENumber = Brushes.Red;

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
        public Brush ValidReceivedDate
        {
            get { return _validReceivedDate; }
            set { SetProperty(ref _validReceivedDate, value); }
        }
        private Brush _validReceivedDate = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidInsurance
        {
            get { return _validInsurance; }
            set { SetProperty(ref _validInsurance, value); }
        }
        private Brush _validInsurance = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidInsuranceValue
        {
            get { return _validInsuranceValue; }
            set { SetProperty(ref _validInsuranceValue, value); }
        }
        private Brush _validInsuranceValue = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidInsuranceCost
        { 
            get { return _validInsuranceCost; }
            set { SetProperty(ref _validInsuranceCost, value); }
        }
        private Brush _validInsuranceCost = Brushes.Red;

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
                    case "SelectedDeviceMake":
                        ValidDeviceMake = SelectedDeviceMake != null && SelectedDeviceMake.pkDeviceMakeID < 1 ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedDeviceModel":
                        ValidDeviceModel = SelectedDeviceModel == null || SelectedDeviceModel.pkDeviceModelID < 1 ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedStatus":
                        ValidStatus = SelectedStatus == null || SelectedStatus.pkStatusID < 1 ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedReceivedDate":
                        ValidReceivedDate = SelectedReceivedDate.Date == DateTime.MinValue.Date ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedIMENumber":
                        ValidIMENumber = string.IsNullOrEmpty(SelectedIMENumber) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedInsuranceValue":
                        ValidInsuranceValue = DeviceInsuranceYes && SelectedInsuranceValue < 1 ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedInsuranceCost":
                        ValidInsuranceCost = DeviceInsuranceYes && SelectedInsuranceCost < 1 ? Brushes.Red : Brushes.Silver; break;
                    case "DeviceInsuranceYes":
                        ValidInsuranceCost = DeviceInsuranceYes && SelectedInsuranceCost < 1 ? Brushes.Red : Brushes.Silver;
                        ValidInsuranceValue = DeviceInsuranceYes && SelectedInsuranceValue < 1 ? Brushes.Red : Brushes.Silver;
                        ValidInsurance = !DeviceInsuranceYes && !DeviceInsuranceNo ? Brushes.Red : Brushes.Silver; break;
                    case "DeviceInsuranceNo":
                        if (DeviceInsuranceNo)
                        {
                            ValidInsuranceValue = ValidInsuranceCost = Brushes.Silver;
                            SelectedInsuranceValue = SelectedInsuranceCost = 0;
                        }
                        ValidInsurance = !DeviceInsuranceYes && !DeviceInsuranceNo ? Brushes.Red : Brushes.Silver; break;
                }
                return result;
            }
        }

        #endregion

        #endregion

        #endregion

        #region Event Handlers

        /// <summary>
        /// Load all the device linked to the selected contract from the database
        /// </summary>
        /// <param name="sender">The selected contractID.</param>
        private async void ReadContractDevicesAsync_Event(int sender)
        {
            try
            {
                _selectedContractID = sender;
                await ReadContractSimmCardsAsync();

                if (_selectedContractID > 0)
                {
                    DeviceCollection = await Task.Run(() => new DevicesModel(_eventAggregator).ReadDevicesForContract(_selectedContractID));

                    if (DeviceCollection != null && DeviceCollection.Count > 0)
                    {
                        SelectedDevice = DeviceCollection.Where(p => p.IsActive).FirstOrDefault();
                        ReadDeviceMakeModelsAsync();
                    }
                }
                else
                {
                    ExecuteCancel();
                    DeviceCollection = null;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Call the process based on the sender's completed action.
        /// </summary>
        /// <param name="sender">The completed action.</param>
        private void ActionCompleted_Event(ActionCompleted sender)
        {
            switch (sender)
            {
                case ActionCompleted.ReadContractSimmCards:
                    LinkDeviceToSimmCard();
                    break;
            }
        }

        /// <summary>
        /// Link the selected simmcard on the simmcard view to its device
        /// </summary>
        /// <param name="sender">The selected simm card id.</param>
        private void LinkSimmCardToDevice_Event(int sender)
        {
            if (DeviceCollection != null && DeviceCollection.Count > 0)
            {
                if (sender > 0 && DeviceCollection.Any(p => p.fkSimmCardID.Value == sender))
                    SelectedDevice = DeviceCollection.Where(p => p.fkSimmCardID.Value == sender).FirstOrDefault();
                else
                    ExecuteCancel();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        public ViewDeviceViewModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            InitialiseDeviceView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseDeviceView()
        {
            _model = new DevicesModel(_eventAggregator);
            _securityHelper = new SecurityHelper(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecuteCancel).ObservesProperty(() => SelectedDeviceMake);
            AddCommand = new DelegateCommand(ExecuteAdd, CanExecuteAdd).ObservesProperty(() => SelectedDevice);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave).ObservesProperty(() => SelectedDeviceMake)
                                                                          .ObservesProperty(() => SelectedDeviceModel)
                                                                          .ObservesProperty(() => SelectedIMENumber)                                                                       
                                                                          .ObservesProperty(() => SelectedReceivedDate)
                                                                          .ObservesProperty(() => ValidInsurance)
                                                                          .ObservesProperty(() => SelectedStatus)
                                                                          .ObservesProperty(() => DeviceInsuranceYes)
                                                                          .ObservesProperty(() => DeviceInsuranceNo)
                                                                          .ObservesProperty(() => SelectedInsuranceCost)
                                                                          .ObservesProperty(() => SelectedInsuranceValue);
            DeviceMakeCommand = new DelegateCommand(ExecuteShowMakeView, CanExecuteMaintenace);
            DeviceModelCommand = new DelegateCommand(ExecuteShowModelView, CanExecuteMaintenace);
            DeviceStatusCommand = new DelegateCommand(ExecuteShowStatusView, CanExecuteMaintenace);

            // Subscribe to this event to read devices linked to selectede contract
            _eventAggregator.GetEvent<ReadDevicesEvent>().Subscribe(ReadContractDevicesAsync_Event, true);

            // Subscribe to this event to link the first active device to its simmcard
            _eventAggregator.GetEvent<ActionCompletedEvent>().Subscribe(ActionCompleted_Event, true);

            // Subscribe to this event to link the selected simmcard to its device
            _eventAggregator.GetEvent<LinkSimmCardDeviceEvent>().Subscribe(LinkSimmCardToDevice_Event, true);

            // Load the view data
            await ReadDeviceMakesAsync();
            await ReadStatusesAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            SelectedDevice = new Device();
            SelectedReceivedDate = DateTime.Now;
            DeviceInsuranceYes = DeviceInsuranceNo = false;
            DeviceState = true;
        }

        /// <summary>
        /// Set the options for device insurance
        /// </summary>
        /// <param name="hasInsurance">Indicator if a device has insurance.</param>
        private void SetDeviceInsurance(decimal insuranceCost, decimal insuranceValue)
        {
            if (insuranceCost > 0)
            {
                DeviceInsuranceYes = true;
                DeviceInsuranceNo = false;
                SelectedInsuranceCost = insuranceCost;
                SelectedInsuranceValue = insuranceValue;
            }
            else
            {
                DeviceInsuranceYes = false;
                DeviceInsuranceNo = true;
                SelectedInsuranceCost = SelectedInsuranceValue = 0;
            }
        }

        /// <summary>
        /// Link the selected device to its simmcard on the simmcard view
        /// </summary>
        private void LinkDeviceToSimmCard()
        {
            // Publish the event so the simmcard view can link the simmcard to the device
            if (SelectedDevice.fkSimmCardID != null)
                _eventAggregator.GetEvent<LinkDeviceSimmCardEvent>().Publish(SelectedDevice.fkSimmCardID.Value);
            else
                _eventAggregator.GetEvent<LinkDeviceSimmCardEvent>().Publish(0);
        }

        #region Lookup Data Loading

        /// <summary>
        /// Load all the device makes from the database
        /// </summary>
        private async Task ReadDeviceMakesAsync()
        {
            try
            {
                DeviceMakeCollection = await Task.Run(() => new DevicesMakeModel(_eventAggregator).ReadDeviceMakes(true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Load all the device models from the database
        /// </summary>
        private async void ReadDeviceMakeModelsAsync()
        {
            try
            {
                if (SelectedDeviceMake != null && SelectedDeviceMake.pkDeviceMakeID > 0)
                    DeviceModelCollection = await Task.Run(() => new DevicesModelModel(_eventAggregator).ReadDeviceMakeModels(SelectedDeviceMake.pkDeviceMakeID, true));

                if (DeviceModelCollection != null && SelectedDevice != null)
                    SelectedDeviceModel = DeviceModelCollection.Where(p => p.pkDeviceModelID == SelectedDevice.fkDeviceModelID).FirstOrDefault();
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
                StatusCollection = await Task.Run(() => new StatusModel(_eventAggregator).ReadStatuses(StatusLink.Device, true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Load all the simm cards linked to the selected contract from the database
        /// </summary>
        private async Task ReadContractSimmCardsAsync()
        {
            try
            {
                if (_selectedContractID > 0)
                    SimmCardCollection = await Task.Run(() => new SimmCardModel(_eventAggregator).ReadSimmCardsForContract(_selectedContractID));

                SimmCard defaultItem = new SimmCard();
                defaultItem.pkSimmCardID = 0;
                defaultItem.CellNumber = "-- Please Select --";
                SimmCardCollection.Add(defaultItem);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
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
            return SelectedDeviceMake != null ? SelectedDeviceMake.pkDeviceMakeID > 0 : false;
        }

        /// <summary>
        /// Execute when the cancel process
        /// </summary>
        private void ExecuteCancel()
        {
            InitialiseViewControls();
        }

        /// <summary>
        /// Validate if the cancel process can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteAdd()
        {
            // Validate if the logged-in user can administrate the company the client is linked to
            return SelectedDevice != null && SelectedDevice.pkDeviceID > 0 && 
                   (MobileManagerEnvironment.ClientCompanyID == 0 || _securityHelper.IsUserInCompany(MobileManagerEnvironment.ClientCompanyID) ? true : false);
        }

        /// <summary>
        /// Execute the add new device process
        /// </summary>
        private void ExecuteAdd()
        {
            InitialiseViewControls();
        }

        /// <summary>
        /// Validate if the save device data can be executed
        /// </summary>
        /// <returns>True if can execute</returns>
        private bool CanExecuteSave()
        {
            bool result = false;

            // Validate if the logged-in user can administrate the company the client is linked to
            result = MobileManagerEnvironment.ClientCompanyID == 0 || _securityHelper.IsUserInCompany(MobileManagerEnvironment.ClientCompanyID) ? true : false;

            if (result && SelectedDeviceMake != null && SelectedDeviceModel != null)
                result = SelectedDeviceMake.pkDeviceMakeID > 0 && SelectedDeviceModel.pkDeviceModelID > 0 && !string.IsNullOrEmpty(SelectedIMENumber) &&
                         SelectedReceivedDate.Date > DateTime.MinValue.Date && (DeviceInsuranceYes || DeviceInsuranceNo) && SelectedStatus.pkStatusID > 0 &&
                         (DeviceInsuranceYes ? SelectedInsuranceCost > 0 && SelectedInsuranceValue > 0 : true);
            else
                result = false;

            return result;
        }

        /// <summary>
        /// Execute when the save command button is clicked 
        /// </summary>
        private void ExecuteSave()
        {
            bool result = false;
            SelectedDevice.fkContractID = _selectedContractID;
            SelectedDevice.fkDeviceMakeID = SelectedDeviceMake.pkDeviceMakeID;
            SelectedDevice.fkDeviceModelID = SelectedDeviceModel.pkDeviceModelID;
            SelectedDevice.fkStatusID = SelectedStatus.pkStatusID;
            SelectedDevice.fkSimmCardID = SelectedSimmCard.pkSimmCardID;
            SelectedDevice.IMENumber = SelectedIMENumber.ToUpper();
            SelectedDevice.SerialNumber = SelectedDevice.SerialNumber;
            SelectedDevice.ReceiveDate = SelectedReceivedDate;
            SelectedDevice.InsuranceCost = SelectedInsuranceCost;
            SelectedDevice.InsuranceValue = SelectedInsuranceValue;
            SelectedDevice.ModifiedBy = SecurityHelper.LoggedInDomainName;
            SelectedDevice.ModifiedDate = DateTime.Now;
            SelectedDevice.IsActive = DeviceState;

            if (SelectedDevice.pkDeviceID == 0)
                result = _model.CreateDevice(SelectedDevice);
            else
                result = _model.UpdateDevice(SelectedDevice);

            if (result)
                ReadContractDevicesAsync_Event(_selectedContractID);
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
        /// Execute show device make maintenance view
        /// </summary>
        private async void ExecuteShowMakeView()
        {
            PopupWindow popupWindow = new PopupWindow(new ViewDeviceMake(), "Device Make Maintenance", PopupWindow.PopupButtonType.Close);
            popupWindow.ShowDialog();
            await ReadDeviceMakesAsync();
        }

        /// <summary>
        /// Execute show device model maintenance view
        /// </summary>
        private async void ExecuteShowModelView()
        {
            PopupWindow popupWindow = new PopupWindow(new ViewDeviceModel(), "Device Model Maintenance", PopupWindow.PopupButtonType.Close);
            popupWindow.ShowDialog();
            await ReadDeviceMakesAsync();
            ReadDeviceMakeModelsAsync();
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

        #endregion

        #endregion
    }
}
