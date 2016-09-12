using Gijima.Controls.WPF;
using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.MobileManager.Common.Events;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.MobileManager.Model.Models;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewSecurityCFViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private SecurityModel _model = null;
        private IEventAggregator _eventAggregator;
        private ADUserSearchUX _adUserSearch = null;

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the selected (current) user entity
        /// </summary>
        public User SelectedUser
        {
            get { return _selectedUser; }
            set
            {
                if (value != null)
                {
                    SelectedUserName = value.UserName;
                    SelectedUserFullName = value.UserFullName;
                    UserState = value.IsActive;
                    SetProperty(ref _selectedUser, value);
                }
            }
        }
        private User _selectedUser = new User();

        /// <summary>
        /// The collection of users from from the database
        /// </summary>
        public ObservableCollection<User> UserCollection
        {
            get { return _userCollection; }
            set { SetProperty(ref _userCollection, value); }
        }
        private ObservableCollection<User> _userCollection = null;

        #region Required Fields

        /// <summary>
        /// The entered user name
        /// </summary>
        public string SelectedUserName
        {
            get { return _userName; }
            set { SetProperty(ref _userName, value); }
        }
        private string _userName = string.Empty;

        /// <summary>
        /// Holds the selected user fullname
        /// </summary>
        public string SelectedUserFullName
        {
            get { return _selectedUserFullName; }
            set { SetProperty(ref _selectedUserFullName, value); }
        }
        private string _selectedUserFullName = string.Empty;

        /// <summary>
        /// The selected user state
        /// </summary>
        public bool UserState
        {
            get { return _userState; }
            set { SetProperty(ref _userState, value); }
        }
        private bool _userState;

        /// <summary>
        /// The collection of user roles from the database
        /// </summary>
        public ObservableCollection<Role> UserRoleCollection
        {
            get { return _userRoleCollection; }
            set { SetProperty(ref _userRoleCollection, value); }
        }
        private ObservableCollection<Role> _userRoleCollection = null;

        /// <summary>
        /// The collection of user companies from the database
        /// </summary>
        public ObservableCollection<Company> UserCompanyCollection
        {
            get { return _userCompanyCollection; }
            set { SetProperty(ref _userCompanyCollection, value); }
        }
        private ObservableCollection<Company> _userCompanyCollection = null;

        #endregion

        #region View Lookup Data Collections

        /// <summary>
        /// The collection of available roles from the database
        /// </summary>
        public List<Role> AvailableRoleCollection
        {
            get { return _availableRoleCollection; }
            set { SetProperty(ref _availableRoleCollection, value); }
        }
        private List<Role> _availableRoleCollection = null;

        /// <summary>
        /// The collection of available companies from the database
        /// </summary>
        public ObservableCollection<Company> AvailableCompanyCollection
        {
            get { return _availableCompanyCollection; }
            set { SetProperty(ref _availableCompanyCollection, value); }
        }
        private ObservableCollection<Company> _availableCompanyCollection = null;

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidUserName
        {
            get { return _validUserName; }
            set { SetProperty(ref _validUserName, value); }
        }
        private Brush _validUserName = Brushes.Red;

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidUserFullName
        {
            get { return _validUserFullName; }
            set { SetProperty(ref _validUserFullName, value); }
        }
        private Brush _validUserFullName = Brushes.Red;

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
                    case "SelectedUserName":
                        ValidUserName = string.IsNullOrEmpty(SelectedUserName) ? Brushes.Red : Brushes.Silver; break;
                    case "SelectedUserFullName":
                        ValidUserFullName = string.IsNullOrEmpty(SelectedUserFullName) ? Brushes.Red : Brushes.Silver; break;
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
        public ViewSecurityCFViewModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            InitialiseUserView();
        }

        /// <summary>
        /// Initialise all the view dependencies
        /// </summary>
        private async void InitialiseUserView()
        {
            _model = new SecurityModel(_eventAggregator);

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecuteCancel).ObservesProperty(() => SelectedUserName);
            AddCommand = new DelegateCommand(ExecuteAdd);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecuteSave).ObservesProperty(() => SelectedUserFullName)
                                                                          .ObservesProperty(() => SelectedUserName);

            // Load the view data
            await ReadUsersAsync();
            await ReadRolesAsync();
            await ReadCompaniesAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            SelectedUser = new User();
        }

        /// <summary>
        /// Load all the users from the database
        /// </summary>
        private async Task ReadUsersAsync()
        {
            try
            {
                UserCollection = await Task.Run(() => _model.ReadApplicationUsers(false));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
            }
        }

        #region Lookup Data Loading

        /// <summary>
        /// Load all the roles from the database
        /// </summary>
        private async Task ReadRolesAsync()
        {
            try
            {
                AvailableRoleCollection = await Task.Run(() => _model.ReadApplicationRoles());
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
            }
        }

        /// <summary>
        /// Load all the companies from the database
        /// </summary>
        private async Task ReadCompaniesAsync()
        {
            try
            {
                AvailableCompanyCollection = await Task.Run(() => new CompanyModel(_eventAggregator).ReadCompanies(true, true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
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
            return !string.IsNullOrWhiteSpace(SelectedUserName);
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
        private bool CanExecuteSave()
        {
            return !string.IsNullOrWhiteSpace(SelectedUserName) && !string.IsNullOrWhiteSpace(SelectedUserFullName);
        }

        /// <summary>
        /// Execute when the save command button is clicked 
        /// </summary>
        private async void ExecuteSave()
        {
            bool result = false;

            SelectedUser.UserName = SelectedUserName.ToUpper();
            SelectedUser.UserFullName = SelectedUserFullName.ToUpper();
            SelectedUser.LastActivityDate = DateTime.Now;
            SelectedUser.IsActive = UserState;

            if (SelectedUser.pkUserID == 0)
                result = _model.CreateApplicationUser(SelectedUser);
            else
                result = _model.UpdateApplicationUser(SelectedUser);

            if (result)
            {
                InitialiseViewControls();
                await ReadUsersAsync();

                // Publish this event to sync the application user with the system user
                _eventAggregator.GetEvent<MobileManagerSecurityEvent>().Publish(SelectedUser);
            }
        }

        /// <summary>
        /// Execute when the add command button is clicked 
        /// </summary>
        private async void ExecuteAdd()
        {
            _adUserSearch = new ADUserSearchUX();
            PopupWindow popupWindow = new PopupWindow(_adUserSearch, "User Search", PopupWindow.PopupButtonType.Close);
            popupWindow.ShowDialog();
            await ReadUsersAsync();
        }

        #endregion

        #endregion
    }
}
