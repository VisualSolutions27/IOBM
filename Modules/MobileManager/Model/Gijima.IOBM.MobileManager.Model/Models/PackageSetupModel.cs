using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.MobileManager.Common.Helpers;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.ComponentModel;
using System.Linq;
using System.Transactions;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class PackageSetupModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;
        private AuditLogModel _activityLogger = null;
        private DataActivityHelper _dataActivityHelper = null;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        public PackageSetupModel()
        {
        }

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public PackageSetupModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            _activityLogger = new AuditLogModel(_eventAggregator);
            _dataActivityHelper = new DataActivityHelper(_eventAggregator);
        }

        /// <summary>
        /// Update an existing package setup entity in the database
        /// </summary>
        /// <param name="searchEntity">The client data to search on.</param>
        /// <param name="searchCriteria">The client search criteria to search for.</param>
        /// <param name="updateColumn">The client entity property (column) to update.</param>
        /// <param name="updateValue">The value to update client entity property (column) with.</param>
        /// <param name="companyGroup">The company group the client is linked to.</param>
        /// <param name="errorMessage">OUT The error message.</param>
        /// <returns>True if successfull</returns>
        public bool UpdatePackageSetup(SearchEntity searchEntity, string searchCriteria, string updateColumn, object updateValue, CompanyGroup companyGroup, out string errorMessage)
        {
            errorMessage = string.Empty;
            Client existingClient = null;
            Contract clientContract = null;
            PackageSetup packageSetupCurrent = null;
            bool result = false;

            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    switch (searchEntity)
                    {
                        case SearchEntity.ClientID:
                            existingClient = new SearchEngineModel(_eventAggregator).SearchForClient(searchCriteria, SearchEntity.ClientID).FirstOrDefault();
                            break;
                        case SearchEntity.PrimaryCellNumber:
                            existingClient = new SearchEngineModel(_eventAggregator).SearchForClient(searchCriteria, SearchEntity.PrimaryCellNumber).FirstOrDefault();
                            break;
                        case SearchEntity.EmployeeNumber:
                            if (companyGroup == null)
                                existingClient = new SearchEngineModel(_eventAggregator).SearchForClient(searchCriteria, SearchEntity.EmployeeNumber).FirstOrDefault();
                            else
                                existingClient = new SearchEngineModel(_eventAggregator).SearchForClientByCompanyGroup(searchCriteria, SearchEntity.EmployeeNumber, companyGroup).FirstOrDefault();
                            break;
                        case SearchEntity.IDNumber:
                            existingClient = new SearchEngineModel(_eventAggregator).SearchForClient(searchCriteria, SearchEntity.IDNumber).FirstOrDefault();
                            break;
                    }

                    if (existingClient == null)
                    {
                        errorMessage = string.Format("Client not found for {0} {1}.", searchEntity.ToString(), searchCriteria);
                        return false;
                    }

                    clientContract = db.Contracts.Where(p => p.pkContractID == existingClient.fkContractID).FirstOrDefault();

                    if (clientContract != null)
                        packageSetupCurrent = db.PackageSetups.Where(p => p.pkPackageSetupID == clientContract.fkPackageSetupID).FirstOrDefault();
                }

                using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                {
                    using (var db = MobileManagerEntities.GetContext())
                    {
                        if (packageSetupCurrent != null)
                        {
                            PackageSetup packageSetupToUpdate = db.PackageSetups.Where(p => p.pkPackageSetupID == clientContract.fkPackageSetupID).FirstOrDefault();

                            // Get the client table properties (Fields)
                            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(PackageSetup));

                            foreach (PropertyDescriptor property in properties)
                            {
                                // Find the data column (property) to update
                                if (property.Name == updateColumn)
                                {
                                    // Update the property value based on the data type
                                    if (property.PropertyType == typeof(int))
                                    {
                                        property.SetValue(packageSetupToUpdate, Convert.ToInt32(updateValue));
                                    }
                                    else if (property.PropertyType == typeof(decimal))
                                    {
                                        property.SetValue(packageSetupToUpdate, Convert.ToDecimal(updateValue));
                                    }
                                    else
                                    {
                                        errorMessage = string.Format("Data type {0) not found for {1}.", property.PropertyType, updateColumn);
                                        return false;
                                    }

                                    // Add the data activity log
                                    result = _activityLogger.CreateDataChangeAudits<PackageSetup>(_dataActivityHelper.GetDataChangeActivities<PackageSetup>(packageSetupCurrent, packageSetupToUpdate, existingClient.fkContractID, db));

                                    db.SaveChanges();
                                }
                            }

                            // Commit changes
                            tc.Complete();
                        }
                        else
                        {
                            errorMessage = string.Format("Client package not found for {0} {1}.", searchEntity.ToString(), searchCriteria);
                            return false;
                        }
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                errorMessage = string.Format("Error: {0} {1}.", ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return false;
            }
        }
    }
}
