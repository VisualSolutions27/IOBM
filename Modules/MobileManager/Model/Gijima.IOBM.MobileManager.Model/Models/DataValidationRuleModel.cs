using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Events;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class DataValidationRuleModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;
        private DataCompareHelper _dataComparer;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public DataValidationRuleModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            _dataComparer = new DataCompareHelper(_eventAggregator);
        }

        /// <summary>
        /// Create a new data validation rule entity in the database
        /// </summary>
        /// <param name="validationRule">The data validation rule entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateDataValidationRule(DataValidationRule validationRule)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.DataValidationRules.Any(p => (p.enValidationProcess == validationRule.enValidationProcess &&
                                                          p.enDataValidationEntity == validationRule.enDataValidationEntity &&
                                                          p.DataValidationEntityID == validationRule.DataValidationEntityID &&
                                                          p.fkDataValidationPropertyID == validationRule.fkDataValidationPropertyID) &&
                                                         (validationRule.fkPackageID != null ? p.fkPackageID == validationRule.fkPackageID : true)))
                    {
                        db.DataValidationRules.Add(validationRule);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                        .Publish(new ApplicationMessage("DataValidationRuleModel",
                                                                        "The validation rule already exist.",
                                                                        "CreateDataValidationRule",
                                                                        ApplicationMessage.MessageTypes.Information));
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DataValidationRuleModel",
                                                                string.Format("Error! {0} {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "CreateDataValidationRule",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Read all the data validation rules for the specified group from the database
        /// </summary>
        /// <param name="validationProcess">The data validation process linked to the rules.</param>
        /// <param name="validationEntity">The data validation entity linked to the rules.</param>
        /// <returns>Collection of DataValidationRules</returns>
        public ObservableCollection<DataValidationRule> ReadDataValidationRules(DataValidationProcess validationProcess, DataValidationGroupName validationEntity)
        {
            try
            {
                List<DataValidationRule> validationRules = new List<DataValidationRule>();
                var rules = (dynamic)null;
                short processID = validationProcess.Value();
                short entityID = validationEntity.Value();
                DataValidationRule newDataRule = null;
                string propertyDescription = string.Empty;
                string packageDescription = string.Empty;

                using (var db = MobileManagerEntities.GetContext())
                {
                    switch (validationEntity)
                    {
                        case DataValidationGroupName.Client:
                            rules = (from validationRule in db.DataValidationRules
                                     join validationProperty in db.DataValidationProperties
                                     on validationRule.fkDataValidationPropertyID equals validationProperty.pkDataValidationPropertyID
                                     join client in db.Clients
                                     on validationRule.DataValidationEntityID equals client.pkClientID
                                     where validationRule.enValidationProcess == processID &&
                                           validationRule.enDataValidationEntity == entityID
                                     select new
                                     {
                                         pkDataValidationRuleID = validationRule.pkDataValidationRuleID,
                                         enValidationProcess = validationRule.enValidationProcess,
                                         enDataValidationEntity = validationRule.enDataValidationEntity,
                                         DataValidationEntityID = validationRule.DataValidationEntityID,
                                         DataDescription = client.PrimaryCellNumber,
                                         fkDataValidationPropertyID = validationRule.fkDataValidationPropertyID,
                                         fkPackageID = validationRule.fkPackageID,
                                         enDataValidationProperty = validationProperty.enDataValidationProperty,
                                         enDataType = validationProperty.enDataType,
                                         enDataValidationOperator = validationRule.enDataValidationOperator,
                                         DataValidationValue = validationRule.DataValidationValue,
                                         ModifiedBy = validationRule.ModifiedBy,
                                         ModifiedDate = validationRule.ModifiedDate
                                     }).ToList();
                            break;
                        case DataValidationGroupName.CompanyClient:
                            rules = (from validationRule in db.DataValidationRules
                                     join validationProperty in db.DataValidationProperties
                                     on validationRule.fkDataValidationPropertyID equals validationProperty.pkDataValidationPropertyID
                                     join company in db.Companies
                                     on validationRule.DataValidationEntityID equals company.pkCompanyID
                                     where validationRule.enValidationProcess == processID &&
                                           validationRule.enDataValidationEntity == entityID
                                     select new
                                     {
                                         pkDataValidationRuleID = validationRule.pkDataValidationRuleID,
                                         enValidationProcess = validationRule.enValidationProcess,
                                         enDataValidationEntity = validationRule.enDataValidationEntity,
                                         DataValidationEntityID = validationRule.DataValidationEntityID,
                                         DataDescription = company.CompanyName,
                                         fkDataValidationPropertyID = validationRule.fkDataValidationPropertyID,
                                         fkPackageID = validationRule.fkPackageID,
                                         enDataValidationProperty = validationProperty.enDataValidationProperty,
                                         enDataType = validationProperty.enDataType,
                                         enDataValidationOperator = validationRule.enDataValidationOperator,
                                         DataValidationValue = validationRule.DataValidationValue,
                                         ModifiedBy = validationRule.ModifiedBy,
                                         ModifiedDate = validationRule.ModifiedDate
                                     }).ToList();
                            break;
                        case DataValidationGroupName.Device:
                        case DataValidationGroupName.SimCard:
                            rules = (from validationRule in db.DataValidationRules
                                     join validationProperty in db.DataValidationProperties
                                     on validationRule.fkDataValidationPropertyID equals validationProperty.pkDataValidationPropertyID
                                     where validationRule.enValidationProcess == processID &&
                                           validationRule.enDataValidationEntity == entityID
                                     select new
                                     {
                                         pkDataValidationRuleID = validationRule.pkDataValidationRuleID,
                                         enValidationProcess = validationRule.enValidationProcess,
                                         enDataValidationEntity = validationRule.enDataValidationEntity,
                                         DataValidationEntityID = validationRule.DataValidationEntityID,
                                         DataDescription = "None",
                                         fkDataValidationPropertyID = validationRule.fkDataValidationPropertyID,
                                         fkPackageID = validationRule.fkPackageID,
                                         enDataValidationProperty = validationProperty.enDataValidationProperty,
                                         enDataType = validationProperty.enDataType,
                                         enDataValidationOperator = validationRule.enDataValidationOperator,
                                         DataValidationValue = validationRule.DataValidationValue,
                                         ModifiedBy = validationRule.ModifiedBy,
                                         ModifiedDate = validationRule.ModifiedDate
                                     }).ToList();
                            break;
                        case DataValidationGroupName.ExternalData:
                            rules = (from validationRule in db.DataValidationRules
                                     join externalData in db.ExternalBillingDatas
                                     on validationRule.DataValidationEntityID equals externalData.pkExternalBillingDataID
                                     where validationRule.enValidationProcess == processID &&
                                           validationRule.enDataValidationEntity == entityID
                                     select new
                                     {
                                         pkDataValidationRuleID = validationRule.pkDataValidationRuleID,
                                         enValidationProcess = validationRule.enValidationProcess,
                                         enDataValidationEntity = validationRule.enDataValidationEntity,
                                         DataValidationEntityID = validationRule.DataValidationEntityID,
                                         DataDescription = externalData.TableName,
                                         fkDataValidationPropertyID = validationRule.fkDataValidationPropertyID,
                                         fkPackageID = validationRule.fkPackageID,
                                         enDataValidationProperty = 0,
                                         ExtDataValidationProperty = validationRule.ExtDataValidationProperty,
                                         enDataValidationOperator = validationRule.enDataValidationOperator,
                                         enDataType = 1,
                                         DataValidationValue = validationRule.DataValidationValue,
                                         ModifiedBy = validationRule.ModifiedBy,
                                         ModifiedDate = validationRule.ModifiedDate
                                     });
                            break;
                    }

                    // If no rules found return null
                    if (rules == null)
                        return null;

                    foreach (var rule in rules)
                    {
                        if ((DataValidationGroupName)rule.enDataValidationEntity == DataValidationGroupName.ExternalData)
                        {
                            propertyDescription = rule.ExtDataValidationProperty;
                            if (rule.fkPackageID != null)
                                packageDescription = new PackageModel(_eventAggregator).ReadPackageName(rule.fkPackageID);
                        }
                        else
                        {
                            propertyDescription = EnumHelper.GetDescriptionFromEnum((DataValidationPropertyName)rule.enDataValidationProperty).ToString();
                            packageDescription = string.Empty;
                        }
                        newDataRule = new DataValidationRule();
                        newDataRule.pkDataValidationRuleID = rule.pkDataValidationRuleID;
                        newDataRule.enValidationProcess = rule.enValidationProcess;
                        newDataRule.enDataValidationEntity = rule.enDataValidationEntity;
                        newDataRule.EntityDescription = EnumHelper.GetDescriptionFromEnum((DataValidationGroupName)rule.enDataValidationEntity).ToString();
                        newDataRule.DataValidationEntityID = rule.DataValidationEntityID;
                        newDataRule.DataDescription = rule.DataDescription;
                        newDataRule.fkDataValidationPropertyID = rule.fkDataValidationPropertyID;
                        newDataRule.fkPackageID = rule.fkPackageID;
                        newDataRule.PackageDescription = packageDescription;
                        newDataRule.PropertyName = ((DataValidationPropertyName)rule.enDataValidationProperty).ToString();
                        newDataRule.PropertyDescription = propertyDescription;
                        newDataRule.enDataValidationOperator = rule.enDataValidationOperator;
                        newDataRule.DataTypeDescription = ((DataTypeName)rule.enDataType).ToString();
                        newDataRule.OperatorDescription = EnumHelper.GetOperatorFromDataTypeEnum((DataTypeName)rule.enDataType, rule.enDataValidationOperator).ToString();
                        newDataRule.DataValidationValue = rule.DataValidationValue;
                        newDataRule.ModifiedBy = rule.ModifiedBy;
                        newDataRule.ModifiedDate = rule.ModifiedDate;
                        validationRules.Add(newDataRule);
                    }

                    return new ObservableCollection<DataValidationRule>(validationRules.OrderBy(p => p.DataDescription).ThenBy(p => p.EntityDescription));
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DataValidationRuleModel",
                                                                string.Format("Error! {0} {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ReadDataValidationRules",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return null;
            }
        }

        /// <summary>
        /// Update an existing data validation rule entity in the database
        /// </summary>
        /// <param name="validationRule">The data validation rule entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateDataValidationRule(DataValidationRule validationRule)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    DataValidationRule existingValidationRule = db.DataValidationRules.Where(p => p.pkDataValidationRuleID == validationRule.pkDataValidationRuleID).FirstOrDefault();

                    // Check to see if the validation rule name already exist for another entity 
                    if (existingValidationRule != null && existingValidationRule.pkDataValidationRuleID != validationRule.pkDataValidationRuleID)
                    {
                        _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                        .Publish(new ApplicationMessage("DataValidationRuleModel",
                                                                        "The validation rule already exist.",
                                                                        "CreateDataValidationRule",
                                                                        ApplicationMessage.MessageTypes.Information));
                        return false;
                    }
                    else
                    {
                        if (existingValidationRule != null)
                        {
                            existingValidationRule.DataValidationEntityID = validationRule.DataValidationEntityID;
                            existingValidationRule.fkDataValidationPropertyID = validationRule.fkDataValidationPropertyID;
                            existingValidationRule.ExtDataValidationProperty = validationRule.ExtDataValidationProperty;
                            existingValidationRule.enDataValidationOperator = validationRule.enDataValidationOperator;
                            existingValidationRule.DataValidationValue = validationRule.DataValidationValue;
                            existingValidationRule.ModifiedBy = validationRule.ModifiedBy;
                            existingValidationRule.ModifiedDate = validationRule.ModifiedDate;
                            db.SaveChanges();
                            return true;
                        }

                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DataValidationRuleModel",
                                                                string.Format("Error! {0} {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "UpdateDataValidationRule",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Validate the data based on the specified data validation rule
        /// </summary>
        /// <param name="validationRule">The data validation rule to validate against.</param>
        /// <returns>True if successfull</returns>
        public bool ValidateDataValidationRule(DataValidationRule validationRule)
        {
            try
            {
                switch (((DataValidationGroupName)validationRule.enDataValidationEntity))
                {
                    case DataValidationGroupName.Client:
                        break;
                    case DataValidationGroupName.CompanyClient:
                        return ValidateCompanyClientData(validationRule);
                    case DataValidationGroupName.SimCard:
                        return ValidateCompanySimCardData(validationRule);
                }
                return true;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DataValidationRuleModel",
                                                                string.Format("Error! {0} {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ValidateDataValidationRule",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Update the relevant enetity based on the validation result info
        /// </summary>
        /// <param name="validationExceptionInfo">The validation exception info to apply.</param>
        /// <returns>True if successfull</returns>
        public bool ApplyDataRule(DataValidationException validationExceptionInfo)
        {
            try
            {
                bool result = false;
                string errorMessage = string.Empty;

                using (var db = MobileManagerEntities.GetContext())
                {
                    DataValidationRule rule = ((DbQuery<DataValidationRule>)(from validationRule in db.DataValidationRules
                                                                             where validationRule.pkDataValidationRuleID == validationExceptionInfo.fkDataValidationPropertyID
                                                                             select validationRule)).Include("DataValidationProperty").FirstOrDefault();

                    if (rule != null)
                    {
                        switch ((DataValidationGroupName)validationExceptionInfo.enDataValidationEntity)
                        {
                            case DataValidationGroupName.Client:
                                result = new ClientModel(_eventAggregator).UpdateClient(SearchEntity.ClientID, 
                                                                                        validationExceptionInfo.DataValidationEntityID.ToString(),
                                                                                        ((DataValidationPropertyName)rule.DataValidationProperty.enDataValidationProperty).ToString(),
                                                                                        rule.DataValidationValue,
                                                                                        null,
                                                                                        out errorMessage);

                                //Client clientToUpdate = db.Clients.Where(p => p.pkClientID == validationExceptionInfo.EntityID).FirstOrDefault();

                                //if (clientToUpdate != null)
                                //{
                                //    // Get the client table properties (Fields)
                                //    PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(Client));

                                //    foreach (PropertyDescriptor property in properties)
                                //    {
                                //        // Find the data column (property) to update
                                //        if (rule.DataValidationOption.ValidationDataName == property.Name)
                                //        {
                                //            // Update the property value based on the data type
                                //            switch ((ValidationDataType)rule.DataValidationOption.enValidationDataType)
                                //            {
                                //                case ValidationDataType.Date:
                                //                    property.SetValue(clientToUpdate, Convert.ToDateTime(rule.DataValidationOption.ValidationDataName));
                                //                    break;
                                //                case ValidationDataType.Integer:
                                //                    property.SetValue(clientToUpdate, Convert.ToInt32(rule.DataValidationOption.ValidationDataName));
                                //                    break;
                                //                case ValidationDataType.Decimal:
                                //                    property.SetValue(clientToUpdate, Convert.ToDecimal(rule.DataValidationOption.ValidationDataName));
                                //                    break;
                                //                case ValidationDataType.Bool:
                                //                    property.SetValue(clientToUpdate, Convert.ToBoolean(rule.DataValidationOption.ValidationDataName));
                                //                    break;
                                //                default:
                                //                    property.SetValue(clientToUpdate, rule.DataValidationOption.ValidationDataName);
                                //                    break;
                                //            }

                                //            db.SaveChanges();
                                //            result = true;
                                //        }
                                //    }
                                //}
                                break;
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DataValidationRuleModel",
                                                                string.Format("Error! {0} {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ApplyDataRule",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Validate the company data based on the specified data validation rule
        /// </summary>
        /// <param name="validationRule">The validation rule entity to update.</param>
        /// <returns>True if successfull</returns>
        private bool ValidateCompanyClientData(DataValidationRule validationRule)
        {
            try
            {
                DataValidationException validationResult = null;
                int entityID = 0;
                string entityName = string.Empty;
                string entityValue = string.Empty;
                string ruleValue = string.Empty;
                bool canApplyRule = false;
                bool result = true;

                using (var db = MobileManagerEntities.GetContext())
                {
                    // Get all the active clients link to the company
                    List<Client> companyClients = db.Clients.Where(p => p.fkCompanyID == validationRule.DataValidationEntityID && p.IsActive).ToList();

                    if (companyClients.Count > 0)
                    {
                        // Get the Client table properties (Fields)
                        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(Client));

                        foreach (PropertyDescriptor property in properties)
                        {
                            // Only validate when the rule's data name
                            // match the property name
                            if (validationRule.PropertyName == property.Name)
                            {
                                // Initialise the progress values
                                _eventAggregator.GetEvent<ProgressBarInfoEvent>().Publish(new ProgressBarInfo()
                                {
                                    CurrentValue = 1,
                                    MaxValue = companyClients.Count,
                                });

                                // Validate the data rule values for each           
                                // client linked the the company
                                foreach (Client client in companyClients)
                                {
                                    entityID = client.pkClientID;
                                    entityName = string.Format("{0} ({1})", client.ClientName, client.PrimaryCellNumber);
                                    entityValue = db.Entry(client).Property(validationRule.PropertyName).CurrentValue != null ?
                                                  db.Entry(client).Property(validationRule.PropertyName).CurrentValue.ToString() : string.Empty;

                                    switch (EnumHelper.GetEnumFromDescription<DataTypeName>(validationRule.DataTypeDescription))
                                    {
                                        case DataTypeName.String:
                                            ruleValue = string.Format("{0} {1} {2}", validationRule.PropertyName.ToUpper(),
                                                                                     ((StringOperator)validationRule.enDataValidationOperator).ToString(),
                                                                                     validationRule.DataValidationValue);
                                            result = _dataComparer.CompareStringValues((StringOperator)validationRule.enDataValidationOperator,
                                                                                       entityValue,
                                                                                       validationRule.DataValidationValue);
                                            // If the operator is 'Equal' the the rule can be auto applied to fixed failures
                                            canApplyRule = (StringOperator)validationRule.enDataValidationOperator == StringOperator.Equal ? true : false;
                                            break;
                                        case DataTypeName.DateTime:
                                            ruleValue = string.Format("{0} {1} {2}", validationRule.PropertyName.ToUpper(),
                                                                                     ((DateOperator)validationRule.enDataValidationOperator).ToString(),
                                                                                     validationRule.DataValidationValue);
                                            break;
                                        case DataTypeName.Integer:
                                            ruleValue = string.Format("{0} {1} {2}", validationRule.PropertyName.ToUpper(),
                                                                                     ((NumericOperator)validationRule.enDataValidationOperator).ToString(),
                                                                                     validationRule.DataValidationValue);
                                            result = _dataComparer.CompareNumericValues((NumericOperator)validationRule.enDataValidationOperator,
                                                                                        entityValue,
                                                                                        validationRule.DataValidationValue);
                                            // If the operator is 'Equal' the the rule can be auto applied to fixed failures
                                            canApplyRule = (NumericOperator)validationRule.enDataValidationOperator == NumericOperator.Equal ? true : false;
                                            break;
                                        case DataTypeName.Decimal:
                                            ruleValue = string.Format("{0} {1} {2}", validationRule.PropertyName.ToUpper(),
                                                                                     ((NumericOperator)validationRule.enDataValidationOperator).ToString(),
                                                                                     validationRule.DataValidationValue);
                                            result = _dataComparer.CompareDecimalValues((NumericOperator)validationRule.enDataValidationOperator,
                                                                                        entityValue,
                                                                                        validationRule.DataValidationValue);
                                            // If the operator is 'Equal' the the rule can be auto applied to fixed failures
                                            canApplyRule = (NumericOperator)validationRule.enDataValidationOperator == NumericOperator.Equal ? true : false;
                                            break;
                                        case DataTypeName.Bool:
                                            ruleValue = string.Format("{0} {1} {2}", validationRule.PropertyName.ToUpper(),
                                                                                     ((StringOperator)validationRule.enDataValidationOperator).ToString(),
                                                                                     validationRule.DataValidationValue);
                                            break;
                                        default:
                                            break;
                                    }

                                    // Update the progress values for each client
                                    _eventAggregator.GetEvent<ProgressBarInfoEvent>().Publish(new ProgressBarInfo()
                                    {
                                        CurrentValue = companyClients.IndexOf(client),
                                        MaxValue = companyClients.Count,
                                    });

                                    // Update the validation result values
                                    validationResult = new DataValidationException();
                                    if (result)
                                    {
                                        validationResult = new DataValidationException()
                                        {
                                            fkBillingProcessID = BillingExecutionState.InternalDataValidation.Value(),
                                            fkDataValidationPropertyID = validationRule.fkDataValidationPropertyID.Value,
                                            BillingPeriod = string.Format("{0}{1}", DateTime.Now.Month.ToString().PadLeft(2, '0'), DateTime.Now.Year),
                                            enDataValidationEntity = DataValidationGroupName.Client.Value(),
                                            DataValidationEntityID = client.pkClientID,
                                            Result = true
                                        };

                                        _eventAggregator.GetEvent<DataValiationResultEvent>().Publish(validationResult);
                                    }
                                    else
                                    {
                                        validationResult = new DataValidationException()
                                        {
                                            fkBillingProcessID = BillingExecutionState.InternalDataValidation.Value(),
                                            fkDataValidationPropertyID = validationRule.pkDataValidationRuleID,
                                            BillingPeriod = string.Format("{0}{1}", DateTime.Now.Month.ToString().PadLeft(2, '0'), DateTime.Now.Year),
                                            enDataValidationEntity = DataValidationGroupName.Client.Value(),
                                            DataValidationEntityID = client.pkClientID,
                                            CanApplyRule = canApplyRule,
                                            Message = string.Format("{0} failed for {1} linked to company {2} - current value ({3}) - expected ({4}).",
                                                                    validationRule.PropertyDescription.ToUpper(), entityName, validationRule.DataDescription,
                                                                    entityValue, ruleValue),
                                            Result = false
                                        };

                                        _eventAggregator.GetEvent<DataValiationResultEvent>().Publish(validationResult);
                                    }
                                }

                                // Update the progress values for the last client
                                _eventAggregator.GetEvent<ProgressBarInfoEvent>().Publish(new ProgressBarInfo()
                                {
                                    CurrentValue = companyClients.Count,
                                    MaxValue = companyClients.Count,
                                });
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DataValidationRuleModel",
                                                                string.Format("Error! {0} {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ValidateCompanyClientData",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }

        /// <summary>
        /// Validate the simcard data based on the specified data validation rule
        /// </summary>
        /// <param name="validationRule">The validation rule entity to update.</param>
        /// <returns>True if successfull</returns>
        private bool ValidateCompanySimCardData(DataValidationRule validationRule)
        {
            try
            {
                DataValidationException validationResult = null;
                string entityName = string.Empty;
                string entityValue = string.Empty;
                string ruleValue = string.Empty;
                bool canApplyRule = false;
                bool result = true;

                using (var db = MobileManagerEntities.GetContext())
                {
                    // Get all the active simcards
                    List<SimCard> simCards = db.SimCards.Where(p => p.IsActive).ToList();

                    if (simCards.Count > 0)
                    {
                        // Get the Client table properties (Fields)
                        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(SimCard));

                        foreach (PropertyDescriptor property in properties)
                        {
                            // Only validate when the rule's data name
                            // match the property name
                            if (validationRule.PropertyName == property.Name)
                            {
                                // Initialise the progress values
                                _eventAggregator.GetEvent<ProgressBarInfoEvent>().Publish(new ProgressBarInfo()
                                {
                                    CurrentValue = 1,
                                    MaxValue = simCards.Count,
                                });

                                // Validate the data rule values for each simcard         
                                foreach (SimCard sim in simCards)
                                {
                                    entityName = string.Format("{0}", sim.CellNumber);
                                    entityValue = db.Entry(sim).Property(validationRule.PropertyName).CurrentValue != null ?
                                                  db.Entry(sim).Property(validationRule.PropertyName).CurrentValue.ToString() : string.Empty;

                                    switch (EnumHelper.GetEnumFromDescription<DataTypeName>(validationRule.DataTypeDescription))
                                    {
                                        case DataTypeName.String:
                                            ruleValue = string.Format("{0} {1} {2}", validationRule.PropertyName.ToUpper(),
                                                                                     ((StringOperator)validationRule.enDataValidationOperator).ToString(),
                                                                                     validationRule.DataValidationValue);
                                            result = _dataComparer.CompareStringValues((StringOperator)validationRule.enDataValidationOperator,
                                                                                       entityValue,
                                                                                       validationRule.DataValidationValue);
                                            // If the operator is 'Equal' the the rule can be auto applied to fixed failures
                                            canApplyRule = (StringOperator)validationRule.enDataValidationOperator == StringOperator.Equal ? true : false;
                                            break;
                                        case DataTypeName.DateTime:
                                            ruleValue = string.Format("{0} {1} {2}", validationRule.PropertyName.ToUpper(),
                                                                                     ((DateOperator)validationRule.enDataValidationOperator).ToString(),
                                                                                     validationRule.DataValidationValue);
                                            break;
                                        case DataTypeName.Integer:
                                            ruleValue = string.Format("{0} {1} {2}", validationRule.PropertyName.ToUpper(),
                                                                                     ((NumericOperator)validationRule.enDataValidationOperator).ToString(),
                                                                                     validationRule.DataValidationValue);
                                            result = _dataComparer.CompareNumericValues((NumericOperator)validationRule.enDataValidationOperator,
                                                                                        entityValue,
                                                                                        validationRule.DataValidationValue);
                                            // If the operator is 'Equal' the the rule can be auto applied to fixed failures
                                            canApplyRule = (NumericOperator)validationRule.enDataValidationOperator == NumericOperator.Equal ? true : false;
                                            break;
                                        case DataTypeName.Decimal:
                                            ruleValue = string.Format("{0} {1} {2}", validationRule.PropertyName.ToUpper(),
                                                                                     ((NumericOperator)validationRule.enDataValidationOperator).ToString(),
                                                                                     validationRule.DataValidationValue);
                                            result = _dataComparer.CompareDecimalValues((NumericOperator)validationRule.enDataValidationOperator,
                                                                                        entityValue,
                                                                                        validationRule.DataValidationValue);
                                            // If the operator is 'Equal' the the rule can be auto applied to fixed failures
                                            canApplyRule = (NumericOperator)validationRule.enDataValidationOperator == NumericOperator.Equal ? true : false;
                                            break;
                                        case DataTypeName.Bool:
                                            ruleValue = string.Format("{0} {1} {2}", validationRule.PropertyName.ToUpper(),
                                                                                     ((StringOperator)validationRule.enDataValidationOperator).ToString(),
                                                                                     validationRule.DataValidationValue);
                                            break;
                                        default:
                                            break;
                                    }

                                    // Update the progress values for each simcard
                                    _eventAggregator.GetEvent<ProgressBarInfoEvent>().Publish(new ProgressBarInfo()
                                    {
                                        CurrentValue = simCards.IndexOf(sim),
                                        MaxValue = simCards.Count,
                                    });

                                    // Update the validation result values
                                    validationResult = new DataValidationException();
                                    if (result)
                                    {
                                        validationResult = new DataValidationException()
                                        {
                                            fkDataValidationPropertyID = validationRule.fkDataValidationPropertyID.Value,
                                            enDataValidationEntity = DataValidationGroupName.SimCard.Value(),
                                            DataValidationEntityID = sim.pkSimCardID,
                                            Result = true
                                        };

                                        _eventAggregator.GetEvent<DataValiationResultEvent>().Publish(validationResult);
                                    }
                                    else
                                    {
                                        validationResult = new DataValidationException()
                                        {
                                            fkDataValidationPropertyID = validationRule.pkDataValidationRuleID,
                                            enDataValidationEntity = DataValidationGroupName.Client.Value(),
                                            DataValidationEntityID = sim.pkSimCardID,
                                            CanApplyRule = canApplyRule,
                                            Message = string.Format("{0} failed for {1} - current value ({2}) - expected ({3}).",
                                                                    validationRule.PropertyDescription.ToUpper(), entityName, 
                                                                    entityValue, ruleValue),
                                            Result = false
                                        };

                                        _eventAggregator.GetEvent<DataValiationResultEvent>().Publish(validationResult);
                                    }
                                }

                                // Update the progress values for the last client
                                _eventAggregator.GetEvent<ProgressBarInfoEvent>().Publish(new ProgressBarInfo()
                                {
                                    CurrentValue = simCards.Count,
                                    MaxValue = simCards.Count,
                                });
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>()
                                .Publish(new ApplicationMessage("DataValidationRuleModel",
                                                                string.Format("Error! {0} {1}.",
                                                                ex.Message, ex.InnerException != null ? ex.InnerException.Message : string.Empty),
                                                                "ValidateSimCardData",
                                                                ApplicationMessage.MessageTypes.SystemError));
                return false;
            }
        }
    }
}
