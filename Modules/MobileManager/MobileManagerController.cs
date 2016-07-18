﻿using static Gijima.IOBM.Infrastructure.Structs.ApplicationInfo;
using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Security;
using Gijima.IOBM.MobileManager.ViewModels;
using Gijima.IOBM.MobileManager.Views;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Regions;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Gijima.IOBM.MobileManager
{
    public class MobileManagerController
    {
        #region Properties & Attributes

        private IUnityContainer _container = null;
        private IRegionManager _regionManager = null;
        private IEventAggregator _eventAggregator = null;

        #region Properties
        #endregion

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="container"></param>
        public MobileManagerController(IUnityContainer container)
        {
            _container = container;
            _regionManager = _container.Resolve<IRegionManager>();
            _eventAggregator = _container.Resolve<IEventAggregator>();

            // Close the application is the user
            // do not authenticate
            if (AuthenticateUser())
            {
                ReadPublishedApplicationVersionAsync();
                ReadConnectionInfoAsync();

                ApplicationInfo connectionInfo = new ApplicationInfo();
                connectionInfo.ApplicationInfoSource = InfoSource.UserInfo;

                // Publish the event to update the IOBM shell
                _eventAggregator.GetEvent<ApplicationInfoEvent>().Publish(connectionInfo);
            }

            //ShowMobileManager();
            //_eventAggregator.GetEvent<ShowMobileManagerEvent>().Subscribe(ShowMobileManager, true);
            //_eventAggregator.GetEvent<HideMessageBoxEvent>().Subscribe(HideMessageBox, true);
        }

        /// <summary>
        /// Authenticate the user against the MobileManager database users
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

        /// <summary>
        /// Read the selected application version in the IOBM solution
        /// </summary>
        private async void ReadPublishedApplicationVersionAsync()
        {
            try
            {
                ApplicationInfo Application = new ApplicationInfo();
                FileVersionInfo fvi = null;
                Assembly assembly = Assembly.GetExecutingAssembly();
                await Task.Run(() => fvi = FileVersionInfo.GetVersionInfo(assembly.Location));
                Application.ApplicationInfoSource = InfoSource.ApplicationVersion;
                Application.PublisedApplicationVersion = fvi.FileVersion;

                // Publish the event to update the IOBM shell
                _eventAggregator.GetEvent<ApplicationInfoEvent>().Publish(Application);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Read the database server name and database name
        /// </summary>
        private async void ReadConnectionInfoAsync()
        {
            try
            {
                ApplicationInfo Application = new ApplicationInfo();
                string serverName = string.Empty;
                string databaseName = string.Empty;

                await Task.Run(() => new SecurityHelper(_eventAggregator).ReadConnectionInfo(out serverName, out databaseName));
                Application.ApplicationInfoSource = InfoSource.ConnectionInfo;
                Application.ServerName = serverName;
                Application.DatabaseName = databaseName;

                // Publish the event to update the IOBM shell
                _eventAggregator.GetEvent<ApplicationInfoEvent>().Publish(Application);
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        private void ShowMobileManager()
        {
            var region = _regionManager.Regions[RegionaNames.MainRegion];
            var view = _container.Resolve<ViewMobileManager>();
            var viewModel = _container.Resolve<ViewMobileManagerViewModel>();

            if (!region.Views.Contains(view))
                region.Add(view, typeof(ViewMobileManager).Name);

            view.DataContext = viewModel;
            region.Activate(view);
        }

        #endregion
    }
}
