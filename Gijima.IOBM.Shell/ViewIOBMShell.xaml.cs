using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Security;
using Gijima.IOBM.Shell.ViewModels;
using Prism.Events;
using System;
using System.Security.Principal;
using System.Windows;

namespace Gijima.IOBM.Shell
{
    /// <summary>
    /// Interaction logic for ViewIOBMShell.xaml
    /// </summary>
    public partial class ViewIOBMShell : Window
    {
        IEventAggregator _eventAggregator = null;

        public ViewIOBMShell(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;

            // Close the application is the user
            // do not authenticate
            if (!AuthenticateUser())
            {
                MessageBox.Show("Logon failed!", "User Authentication", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                Close();
            }

            InitializeComponent();
            DataContext = new ViewIOBMShellViewModel(_eventAggregator);
        }

        /// <summary>
        /// Authenticate the user against the IOBM database users
        /// </summary>
        /// <returns></returns>
        private bool AuthenticateUser()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();

            try
            {
                return new SecurityHelper(_eventAggregator).IsUserAuthenticated(identity.Name);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return false;
            }
        }
    }
}
