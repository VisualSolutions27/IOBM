﻿using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.MobileManager.Common.Helpers;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Transactions;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class ClientModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;
        private AuditLogModel _activityLogger = null;
        private DataActivityHelper _dataActivityHelper = null;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        public ClientModel()
        {
        }

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public ClientModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            _activityLogger = new AuditLogModel(_eventAggregator);
            _dataActivityHelper = new DataActivityHelper(_eventAggregator);
        }

        /// <summary>
        /// Create a new client entity in the database
        /// </summary>
        /// <param name="client">The client entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateClient(Client client)
        {
            try
            {
                using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                {
                    using (var db = MobileManagerEntities.GetContext())
                    {
                        if (!db.Clients.Any(p => p.ClientName.ToUpper() == client.ClientName && p.PrimaryCellNumber == client.PrimaryCellNumber))
                        {
                            // Save the client entity data
                            client.IsActive = true;
                            db.Clients.Add(client);
                            db.SaveChanges();

                            // Commit changes
                            tc.Complete();

                            return true;
                        }
                        else
                        {
                            //_eventAggregator.GetEvent</*ApplicationMessageEvent*/>().Publish(string.Format("The {0} client already exist.", client.ClientName));
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return false;
            }
        }

        /// <summary>
        /// Read client data from the database
        /// </summary>
        /// <param name="clientID">The client ID to read data for.</param>
        /// <returns>Client Entity</returns>
        public Client ReadClient(int clientID)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    Client selectedClient = ((DbQuery<Client>)(from client in db.Clients
                                                               where client.pkClientID == clientID
                                                               select client)).Include("Contract")
                                                                              .Include("Contract.PackageSetup")
                                                                              .Include("ClientBilling").FirstOrDefault();

                    return selectedClient;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        /// <summary>
        /// Read all or active only clients from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <returns>Collection of Clients</returns>
        public ObservableCollection<Client> ReadClients(bool activeOnly)
        {
            try
            {
                IEnumerable<Client> clients = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    clients = ((DbQuery<Client>)(from client in db.Clients
                                                 where activeOnly ? client.IsActive : true
                                                 select client)).OrderBy(p => p.ClientName).ToList();

                    return new ObservableCollection<Client>(clients);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        /// <summary>
        /// Read all or active only clients linked to the specified company from the database
        /// </summary>
        /// <param name="companyID">The company ID the client is linked to.</param>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <returns>Collection of Clients</returns>
        public ObservableCollection<Client> ReadClientsForCompany(int companyID, bool activeOnly)
        {
            try
            {
                IEnumerable<Client> clients = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    clients = ((DbQuery<Client>)(from client in db.Clients
                                                 where client.fkCompanyID == companyID
                                                 select client)).OrderBy(p => p.ClientName).ToList();

                    if (activeOnly)
                        clients = clients.Where(p => p.IsActive);

                    return new ObservableCollection<Client>(clients);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        /// <summary>
        /// Count all the employees linked to the specified company ID
        /// </summary>
        /// <param name="companyID">The company ID the employees are linked to.</param>
        /// <returns>Employee Count</returns>
        public int EmployeeCountForCompany(int companyID)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    return db.Clients.Count(p => p.fkCompanyID == companyID && p.IsActive);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return 0;
            }
        }
        
        /// <summary>
        /// Update an existing client entity in the database
        /// </summary>
        /// <param name="client">The client entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateClient(Client client)
        {
            try
            {
                bool result = false;

                using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                {
                    using (var db = MobileManagerEntities.GetContext())
                    {
                        Client existingClient = ReadClient(client.pkClientID);

                        // Check to see if the client name already exist for another entity 
                        if (existingClient != null && existingClient.pkClientID != client.pkClientID)
                        {
                            //_eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(string.Format("The {0} client already exist.", client.ClientName));
                            result = false;
                        }
                        else
                        {
                            // Prevent primary key confilcts when using attach property
                            if (existingClient != null)
                                db.Entry(existingClient).State = System.Data.Entity.EntityState.Detached;

                            db.Clients.Attach(client);
                            db.Entry(client).State = System.Data.Entity.EntityState.Modified;

                            db.Contracts.Attach(client.Contract);
                            db.Entry(client.Contract).State = System.Data.Entity.EntityState.Modified;

                            db.PackageSetups.Attach(client.Contract.PackageSetup);
                            db.Entry(client.Contract.PackageSetup).State = System.Data.Entity.EntityState.Modified;

                            db.ClientBillings.Attach(client.ClientBilling);
                            db.Entry(client.ClientBilling).State = System.Data.Entity.EntityState.Modified;

                            db.SaveChanges();

                            _activityLogger.CreateDataChangeAudits<Client>(_dataActivityHelper.GetDataChangeActivities<Client>(existingClient, client, client.fkContractID, db));
                            _activityLogger.CreateDataChangeAudits<Contract>(_dataActivityHelper.GetDataChangeActivities<Contract>(existingClient.Contract, client.Contract, client.fkContractID, db));
                            _activityLogger.CreateDataChangeAudits<PackageSetup>(_dataActivityHelper.GetDataChangeActivities<PackageSetup>(existingClient.Contract.PackageSetup, client.Contract.PackageSetup, client.fkContractID, db));
                            _activityLogger.CreateDataChangeAudits<ClientBilling>(_dataActivityHelper.GetDataChangeActivities<ClientBilling>(existingClient.ClientBilling, client.ClientBilling, client.fkContractID, db));

                            // Commit changes
                            tc.Complete();

                            result = true;
                        }
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return false;
            }
        }

        /// <summary>
        /// Update an existing client entity in the database
        /// </summary>
        /// <param name="searchEntity">The client data to search on.</param>
        /// <param name="searchCriteria">The client search criteria to search for.</param>
        /// <param name="updateColumn">The client entity property (column) to update.</param>
        /// <param name="updateValue">The value to update client entity property (column) with.</param>
        /// <param name="companyGroup">The company group the client is linked to.</param>
        /// <param name="errorMessage">OUT The error message.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateClient(SearchEntity searchEntity, string searchCriteria, string updateColumn, object updateValue, CompanyGroup companyGroup, out string errorMessage)
        {
            errorMessage = string.Empty;
            Client existingClient = null;
            Client clientToUpdate = null;
            Type propertyType = null;
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
                }

                if (existingClient != null)
                {
                    using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                    {
                        using (var db = MobileManagerEntities.GetContext())
                        {
                            clientToUpdate = db.Clients.Where(p => p.pkClientID == existingClient.pkClientID).FirstOrDefault();

                            // Get the client table properties (Fields)
                            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(Client));

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
                                        property.SetValue(clientToUpdate, Convert.ToDateTime(updateValue));
                                    }
                                    else if (propertyType == typeof(int))
                                    {
                                        property.SetValue(clientToUpdate, Convert.ToInt32(updateValue));
                                    }
                                    else if (propertyType == typeof(decimal))
                                    {
                                        property.SetValue(clientToUpdate, Convert.ToDecimal(updateValue));
                                    }
                                    else if (propertyType == typeof(bool))
                                    {
                                        property.SetValue(clientToUpdate, Convert.ToBoolean(updateValue));
                                    }
                                    else if (propertyType == typeof(string))
                                    {
                                        property.SetValue(clientToUpdate, updateValue);
                                    }
                                    else
                                    {
                                        errorMessage = string.Format("Data type {0) not found for {1}.", property.PropertyType, updateColumn);
                                        return false;
                                    }

                                    // Add the data activity log
                                    result = _activityLogger.CreateDataChangeAudits<Client>(_dataActivityHelper.GetDataChangeActivities<Client>(existingClient, clientToUpdate, clientToUpdate.fkContractID, db));

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
                    errorMessage = string.Format("Client not found for {0} {1}.", searchEntity.ToString(), searchCriteria);
                    return false;
                }

                return result;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                errorMessage = string.Format("Error: {0} {1}.", ex.Message, ex.InnerException.Message);
                return false;
            }
        }
    }
}
