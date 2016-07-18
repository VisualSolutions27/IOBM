using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Security;
using Gijima.IOBM.MobileManager.Views;
using Prism.Events;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace Gijima.IOBM.MobileManager.ViewModels
{
    public class ViewConfigViewModel : BindableBase
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;
        private SecurityHelper _securityHelper;
        private int _selectedTabIndex = 0;

        #region Properties

        /// <summary>
        /// The selected tab item
        /// </summary>
        public TabItem SelectedTab
        {
            get { return _selectedTab; }
            set { SetProperty(ref _selectedTab, value); SetTabContent(value); }
        }
        private TabItem _selectedTab = null;

        /// <summary>
        /// The collection of tab pages for the tab control menu
        /// </summary>
        public ObservableCollection<TabItem> TabCollection
        {
            get { return _tabCollection; }
            set { SetProperty(ref _tabCollection, value); }
        }
        private ObservableCollection<TabItem> _tabCollection = null;

        #endregion

        #endregion

        public ViewConfigViewModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            _securityHelper = new SecurityHelper(eventAggreagator);

            // Add application functionality based on security roles
            TabCollection = new ObservableCollection<TabItem>();
            if (_securityHelper.IsUserInRole(SecurityRole.Administrator.Value()))
            {
                TabCollection.Add(new TabItem() { Header = "Security Configuration" });
                TabCollection.Add(new TabItem() { Header = "Billing Configuration" });
                TabCollection.Add(new TabItem() { Header = "System Configuration" });
            }
            TabCollection.Add(new TabItem() { Header = "Reference Data" });
        }

        /// <summary>
        /// When the user select a menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SetTabContent(TabItem tabItem)
        {
            //if (_selectedTabIndex != tabItem.TabIndex)
            //{
            //if (TabControlMainWindowMenu.SelectedItem != null)
            //{
            //_selectedTabItem = (TabItem)TabControlMainWindowMenu.SelectedItem;

            if (tabItem.Content == null)
            {
                switch (tabItem.Header.ToString())
                {
                    case "Security Configuration":
                        SelectedTab.Content = new ViewSecurityCF(_eventAggregator);
                        break;
                    case "Billing Configuration":
                        SelectedTab.Content = new ViewBillingCF();
                        break;
                    case "System Configuration":
                        //SelectedTab.Content = new ViewAccount(_eventAggregator);
                        break;
                    case "Reference Data":
                        //SelectedTab.Content = new ViewConfig();
                        break;
                }

            }

            //}
            //}

            _selectedTabIndex = tabItem.TabIndex;
        }
    }
}
