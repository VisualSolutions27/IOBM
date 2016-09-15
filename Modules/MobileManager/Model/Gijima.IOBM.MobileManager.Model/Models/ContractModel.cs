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
    public class ContractModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;
        private AuditLogModel _activityLogger = null;
        private DataActivityHelper _dataActivityHelper = null;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        public ContractModel()
        {
        }

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public ContractModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            _activityLogger = new AuditLogModel(_eventAggregator);
            _dataActivityHelper = new DataActivityHelper(_eventAggregator);
        }

        /// <summary>
        /// Update an existing client contract entity in the database
        /// </summary>
        /// <param name="searchEntity">The contract data to search on.</param>
        /// <param name="searchCriteria">The contract search criteria to search for.</param>
        /// <param name="updateColumn">The contract entity property (column) to update.</param>
        /// <param name="updateValue">The value to update contract entity property (column) with.</param>
        /// <param name="companyGroup">The company group the client is linked to.</param>
        /// <param name="errorMessage">OUT The error message.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateContract(SearchEntity searchEntity, string searchCriteria, string updateColumn, object updateValue, CompanyGroup companyGroup, out string errorMessage)
        {
            errorMessage = string.Empty;
            Contract existingContract = null;
            Contract contractToUpdate = null;
            Type propertyType = null;
            bool result = false;

            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    switch (searchEntity)
                    {
                        case SearchEntity.CellNumber:
                            existingContract = db.Contracts.Where(p => p.CellNumber == searchCriteria).FirstOrDefault();
                            break;
                        case SearchEntity.AccountNumber:
                            existingContract = db.Contracts.Where(p => p.AccountNumber == searchCriteria).FirstOrDefault();
                            break;
                    }

                    if (existingContract == null)
                    {
                        errorMessage = string.Format("Contract not found for {0} {1}.", searchEntity.ToString(), searchCriteria);
                        return false;
                    }
                }

                if (existingContract != null)
                {
                    using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                    {
                        using (var db = MobileManagerEntities.GetContext())
                        {
                            contractToUpdate = db.Contracts.Where(p => p.pkContractID == existingContract.pkContractID).FirstOrDefault();

                            // Get the contract table properties (Fields)
                            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(Contract));

                            foreach (PropertyDescriptor property in properties)
                            {
                                // Find the data column (property) to update
                                if (property.Name == updateColumn)
                                {
                                    // Get the property type for nullable and non-nullable properties
                                    if (property.PropertyType.IsGenericType && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                        propertyType = property.PropertyType.GetGenericArguments()[0];
                                    else
                                        propertyType = property.PropertyType;

                                    // Update the property value based on the data type
                                    if (propertyType == typeof(DateTime))
                                    {
                                        property.SetValue(contractToUpdate, Convert.ToDateTime(updateValue));
                                    }
                                    else if (propertyType == typeof(int))
                                    {
                                        property.SetValue(contractToUpdate, Convert.ToInt32(updateValue));
                                    }
                                    else if (propertyType == typeof(bool))
                                    {
                                        property.SetValue(contractToUpdate, Convert.ToBoolean(updateValue));
                                    }
                                    else if (propertyType == typeof(string))
                                    {
                                        property.SetValue(contractToUpdate, updateValue);
                                    }
                                    else
                                    {
                                        errorMessage = string.Format("Data type {0) not found for {1}.", property.PropertyType, updateColumn);
                                        return false;
                                    }

                                    // Add the data activity log
                                    result = _activityLogger.CreateDataChangeAudits<Contract>(_dataActivityHelper.GetDataChangeActivities<Contract>(existingContract, contractToUpdate, contractToUpdate.pkContractID, db));

                                    db.SaveChanges();
                                }
                            }
                        }

                        // Commit changes
                        tc.Complete();
                    }
                }
                else
                {
                    errorMessage = string.Format("Client contract not found for {0} {1}.", searchEntity.ToString(), searchCriteria);
                    return false;
                }

                return result;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                errorMessage = string.Format("Error: {0} {1}.", ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty);
                return false;
            }
        }
    }
}

