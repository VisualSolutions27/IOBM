using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.MobileManager.Model.Data;
using Gijima.IOBM.MobileManager.Model.Models;
using Gijima.IOBM.MobileManager.Security;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewCompanyGroupViewModel : BindableBase, IDataErrorInfo
    {
        #region Properties & Attributes

        private CompanyGroupModel _model = null;
        private IEventAggregator _eventAggregator;

        #region Commands

        public DelegateCommand CancelCommand { get; set; }
        public DelegateCommand AddCommand { get; set; }
        public DelegateCommand SaveCommand { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Holds the selected (current) company group entity
        /// </summary>
        public CompanyGroup SelectedGroup
        {
            get { return _selectedGroup; }
            set
            {
                if (value != null)
                {
                    GroupName = value.GroupName;
                    GroupState = value.IsActive;
                    SetProperty(ref _selectedGroup, value);
                }
            }
        }
        private CompanyGroup _selectedGroup = new CompanyGroup();

        /// <summary>
        /// The selected company group state
        /// </summary>
        public bool GroupState
        {
            get { return _groupState; }
            set { SetProperty(ref _groupState, value); }
        }
        private bool _groupState;

        /// <summary>
        /// The collection of company groups from the database
        /// </summary>
        public ObservableCollection<CompanyGroup> GroupCollection
        {
            get { return _groupCollection; }
            set { SetProperty(ref _groupCollection, value); }
        }
        private ObservableCollection<CompanyGroup> _groupCollection = null;

        #region Required Fields

        /// <summary>
        /// The entered company group name
        /// </summary>
        public string GroupName
        {
            get { return _groupName; }
            set { SetProperty(ref _groupName, value); }
        }
        private string _groupName = string.Empty;

        #endregion

        #region Input Validation

        /// <summary>
        /// Set the required field border colour
        /// </summary>
        public Brush ValidGroup
        {
            get { return _requiredFieldColour; }
            set { SetProperty(ref _requiredFieldColour, value); }
        }
        private Brush _requiredFieldColour = Brushes.Red;

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
                    case "GroupName":
                        ValidGroup = string.IsNullOrEmpty(GroupName) ? Brushes.Red : Brushes.Silver; break;
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
        public ViewCompanyGroupViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            InitialiseCompanyGroupView();
        }

        private async void InitialiseCompanyGroupView()
        {
            _model = new CompanyGroupModel(_eventAggregator);
            InitialiseViewControls();

            // Initialise the view commands
            CancelCommand = new DelegateCommand(ExecuteCancel, CanExecute).ObservesProperty(() => GroupName);
            AddCommand = new DelegateCommand(ExecuteAdd);
            SaveCommand = new DelegateCommand(ExecuteSave, CanExecute).ObservesProperty(() => GroupName);

            // Load the view data
            await ReadCompanyGroupsAsync();
        }

        /// <summary>
        /// Set default values to view properties
        /// </summary>
        private void InitialiseViewControls()
        {
            SelectedGroup = new CompanyGroup();
            GroupState = true;
        }

        /// <summary>
        /// Load all the company groups from the database
        /// </summary>
        private async Task ReadCompanyGroupsAsync()
        {
            try
            {
                GroupCollection = await Task.Run(() => _model.ReadCompanyGroups(false, true));
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
            }
        }

        #region Lookup Data Loading
        #endregion

        #region Command Execution

        /// <summary>
        /// Set view command buttons enabled/disabled state
        /// </summary>
        /// <returns></returns>
        private bool CanExecute()
        {
            return !string.IsNullOrWhiteSpace(GroupName);
        }

        /// <summary>
        /// Execute when the cancel command button is clicked 
        /// </summary>
        private void ExecuteCancel()
        {
            InitialiseViewControls();
        }

        /// <summary>
        /// Execute when the add command button is clicked 
        /// </summary>
        private void ExecuteAdd()
        {
            InitialiseViewControls();
        }

        /// <summary>
        /// Execute when the save command button is clicked 
        /// </summary>
        private async void ExecuteSave()
        {
            bool result = false;
            SelectedGroup.GroupName = GroupName.ToUpper();
            SelectedGroup.ModifiedBy = SecurityHelper.LoggedInDomainName;
            SelectedGroup.ModifiedDate = DateTime.Now;
            SelectedGroup.IsActive = GroupState;

            if (SelectedGroup.pkCompanyGroupID == 0)
                result = _model.CreateCompanyGroup(SelectedGroup);
            else
                result = _model.UpdateCompanyGroup(SelectedGroup);

            if (result)
            {
                InitialiseViewControls();
                await ReadCompanyGroupsAsync();
            }
        }

        #endregion

        #endregion
    }
}
