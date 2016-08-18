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
                    if (!db.DataValidationRules.Any(p => p.fkDataValidationPropertyID == validationRule.fkDataValidationPropertyID &&
                                                         p.DataValidationEntityID == validationRule.DataValidationEntityID))
                    {
                        db.DataValidationRules.Add(validationRule);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        _eventAggregator.GetEvent<MessageEvent>().Publish("The data validation rule already exist.");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return false;
            }
        }

        /// <summary>
        /// Read all the data validation rules for the specified group from the database
        /// </summary>
        /// <param name="validationEntity">The data validation entity linked to the rules.</param>
        /// <returns>Collection of DataValidationRules</returns>
        public ObservableCollection<DataValidationRule> ReadDataValidationRules(DataValidationEntityName validationEntity)
        {
            try
            {
                List<DataValidationRule> validationRules = new List<DataValidationRule>();
                var rules = (dynamic)null;
                short entityID = validationEntity.Value();

                using (var db = MobileManagerEntities.GetContext())
                {
                    switch (validationEntity)
                    {
                        case DataValidationEntityName.Client:
                            rules = (from validationRule in db.DataValidationRules
                                     join validationProperty in db.DataValidationProperties
                                     on validationRule.fkDataValidationPropertyID equals validationProperty.pkDataValidationPropertyID
                                     join client in db.Clients
                                     on validationRule.DataValidationEntityID equals client.pkClientID
                                     where validationRule.enDataValidationEntity == entityID
                                     select new
                                     {
                                         pkDataValidationRuleID = validationRule.pkDataValidationRuleID,
                                         enValidationProcess = validationRule.enValidationProcess,
                                         enDataValidationEntity = validationRule.enDataValidationEntity,
                                         DataValidationEntityID = validationRule.DataValidationEntityID,
                                         DataDescription = client.PrimaryCellNumber,
                                         fkDataValidationPropertyID = validationRule.fkDataValidationPropertyID,
                                         enDataValidationProperty = validationProperty.enDataValidationProperty,
                                         enDataType = validationProperty.enDataType,
                                         enDataValidationOperator = validationRule.enDataValidationOperator,
                                         DataValidationValue = validationRule.DataValidationValue,
                                         ModifiedBy = validationRule.ModifiedBy,
                                         ModifiedDate = validationRule.ModifiedDate
                                     }).ToList();
                            break;
                        case DataValidationEntityName.Company:
                            rules = (from validationRule in db.DataValidationRules
                                     join validationRulesData in db.DataValidationProperties
                                     on validationRule.fkDataValidationPropertyID equals validationRulesData.pkDataValidationPropertyID
                                     join company in db.Companies
                                     on validationRule.DataValidationEntityID equals company.pkCompanyID
                                     where validationRule.enDataValidationEntity == entityID
                                     select new
                                     {
                                         pkDataValidationRuleID = validationRule.pkDataValidationRuleID,
                                         enDataValidationEntity = validationRule.enDataValidationEntity,
                                         fkDataValidationPropertyID = validationRule.fkDataValidationPropertyID,
                                         enDataValidationProperty = validationRulesData.enDataValidationProperty,
                                         enDataType = validationRulesData.enDataType,
                                         DataValidationEntityID = validationRule.DataValidationEntityID,
                                         DataDescription = company.CompanyName,
                                         DataValidationValue = validationRule.DataValidationValue,
                                         enDataValidationOperator = validationRule.enDataValidationOperator,
                                         ModifiedBy = validationRule.ModifiedBy,
                                         ModifiedDate = validationRule.ModifiedDate
                                     }).ToList();

                            break;
                    }

                    foreach (var rule in rules)
                    {
                        validationRules.Add(new DataValidationRule
                        {
                            pkDataValidationRuleID = rule.pkDataValidationRuleID,
                            enValidationProcess = rule.enValidationProcess,
                            enDataValidationEntity = rule.enDataValidationEntity,
                            EntityDescription = EnumHelper.GetDescriptionFromEnum((DataValidationEntityName)rule.enDataValidationEntity).ToString(),
                            DataValidationEntityID = rule.DataValidationEntityID,
                            DataDescription = rule.DataDescription,
                            fkDataValidationPropertyID = rule.fkDataValidationPropertyID,
                            PropertyName = ((DataValidationPropertyName)rule.enDataValidationProperty).ToString(),
                            DataTypeDescription = ((DataTypeName)rule.enDataType).ToString(),
                            PropertyDescription = EnumHelper.GetDescriptionFromEnum((DataValidationPropertyName)rule.enDataValidationProperty).ToString(),
                            enDataValidationOperator = rule.enDataValidationOperator,
                            OperatorDescription = EnumHelper.GetOperatorFromDataTypeEnum((DataTypeName)rule.enDataType, rule.enDataValidationOperator).ToString(),
                            DataValidationValue = rule.DataValidationValue,
                            ModifiedBy = rule.ModifiedBy,
                            ModifiedDate = rule.ModifiedDate,

                        });
                    }

                    return new ObservableCollection<DataValidationRule>(validationRules);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
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
                        _eventAggregator.GetEvent<MessageEvent>().Publish("The validationRule already exist.");
                        return false;
                    }
                    else
                    {
                        if (existingValidationRule != null)
                        {
                            existingValidationRule.fkDataValidationPropertyID = validationRule.fkDataValidationPropertyID;
                            existingValidationRule.DataValidationEntityID = validationRule.DataValidationEntityID;
                            existingValidationRule.DataValidationValue = validationRule.DataValidationValue;
                            existingValidationRule.enDataValidationOperator = validationRule.enDataValidationOperator;
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
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
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
                switch (((DataValidationEntityName)validationRule.enDataValidationEntity))
                {
                    case DataValidationEntityName.Client:
                        break;
                    case DataValidationEntityName.Company:
                        return ValidateCompanyData(validationRule);
                }
                return true;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
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
                        switch ((DataValidationEntityName)validationExceptionInfo.enDataValidationEntity)
                        {
                            case DataValidationEntityName.Client:
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
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return false;
            }
        }

        /// <summary>
        /// Validate the company data based on the specified data validation rule
        /// </summary>
        /// <param name="validationRule">The validation rule entity to update.</param>
        /// <returns>True if successfull</returns>
        private bool ValidateCompanyData(DataValidationRule validationRule)
        {
            try
            {
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

                                    switch (((DataTypeName)validationRule.DataValidationProperty.enDataType))
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
                                    if (result)
                                        _eventAggregator.GetEvent<DataValiationResultEvent>().Publish(new DataValidationException()
                                        {
                                            fkBillingProcessID = BillingExecutionState.DataValidation.Value(),
                                            fkDataValidationPropertyID = validationRule.fkDataValidationPropertyID,
                                            BillingPeriod = string.Format("{0}{1}", DateTime.Now.Month.ToString().PadLeft(2, '0'), DateTime.Now.Year),
                                            enDataValidationEntity = DataValidationEntityName.Client.Value(),
                                            DataValidationEntityID = client.pkClientID,
                                            Result = true
                                        });
                                    else
                                    {
                                        _eventAggregator.GetEvent<DataValiationResultEvent>().Publish(new DataValidationException()
                                        {
                                            fkBillingProcessID = BillingExecutionState.DataValidation.Value(),
                                            fkDataValidationPropertyID = validationRule.pkDataValidationRuleID,
                                            BillingPeriod = string.Format("{0}{1}", DateTime.Now.Month.ToString().PadLeft(2, '0'), DateTime.Now.Year),
                                            enDataValidationEntity = DataValidationEntityName.Client.Value(),
                                            DataValidationEntityID = client.pkClientID,
                                            CanApplyRule = canApplyRule,
                                            Message = string.Format("{0} failed for {1} linked to company {2} - current value ({3}) - expected ({4}).",
                                                                    validationRule.PropertyDescription.ToUpper(), entityName, validationRule.EntityDescription,
                                                                    entityValue, ruleValue),
                                            Result = false
                                        });
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
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return false;
            }
        }
    }
}
