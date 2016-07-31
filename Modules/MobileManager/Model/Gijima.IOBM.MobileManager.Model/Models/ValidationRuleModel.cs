﻿using Gijima.IOBM.Infrastructure.Events;
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
    public class ValidationRuleModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;
        private DataCompareHelper _dataComparer;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public ValidationRuleModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            _dataComparer = new DataCompareHelper(_eventAggregator);
        }

        /// <summary>
        /// Create a new validation rule entity in the database
        /// </summary>
        /// <param name="validationRule">The validation rule entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateValidationRule(ValidationRule validationRule)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.ValidationRules.Any(p => p.fkValidationRulesDataID == validationRule.fkValidationRulesDataID &&
                                                     p.EntityID == validationRule.EntityID))
                    {
                        db.ValidationRules.Add(validationRule);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        _eventAggregator.GetEvent<MessageEvent>().Publish("The validation rule already exist.");
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
        /// Read all the validation rules for the specified group from the database
        /// </summary>
        /// <param name="validationGroup">The validation group linked to the rules.</param>
        /// <returns>Collection of ValidationRules</returns>
        public ObservableCollection<ValidationRule> ReadValidationRules(ValidationRuleGroup validationGroup)
        {
            try
            {
                List<ValidationRule> validationRules = new List<ValidationRule>();
                var rules = (dynamic)null;
                short groupID = validationGroup.Value();

                using (var db = MobileManagerEntities.GetContext())
                {
                    switch (validationGroup)
                    {
                        case ValidationRuleGroup.Client:
                            //EntityDisplayName = "PrimaryCellNumber";
                            //EntityCollection = new ObservableCollection<object>(new ClientModel(_eventAggregator).ReadClients(true));
                            break;
                        case ValidationRuleGroup.Company:
                            rules = (from validationRule in db.ValidationRules
                                     join validationRulesData in db.ValidationRulesDatas
                                     on validationRule.fkValidationRulesDataID equals validationRulesData.pkValidationRulesDataID
                                     join company in db.Companies
                                     on validationRule.EntityID equals company.pkCompanyID
                                     where validationRule.enValidationRuleGroup == groupID
                                     select new
                                     {
                                         pkValidationRuleID = validationRule.pkValidationRuleID,
                                         enValidationRuleGroup = validationRule.enValidationRuleGroup,
                                         fkValidationRulesDataID = validationRule.fkValidationRulesDataID,
                                         RuleDataName = validationRulesData.ValidationDataName,
                                         enRuleDataType = validationRulesData.enValidationDataType,
                                         EntityID = validationRule.EntityID,
                                         EntityName = company.CompanyName,
                                         ValidationDataValue = validationRule.ValidationDataValue,
                                         enStringCompareType = validationRule.enStringCompareType,
                                         enNumericCompareType = validationRule.enNumericCompareType,
                                         enDateCompareType = validationRule.enDateCompareType,
                                         ModifiedBy = validationRule.ModifiedBy,
                                         ModifiedDate = validationRule.ModifiedDate
                                     }).OrderBy(p => p.EntityName).ToList();

                            foreach (var rule in rules)
                            {
                                validationRules.Add(new ValidationRule
                                {
                                    pkValidationRuleID = rule.pkValidationRuleID,
                                    enValidationRuleGroup = rule.enValidationRuleGroup,
                                    fkValidationRulesDataID = rule.fkValidationRulesDataID,
                                    RuleDataName = rule.RuleDataName,
                                    RuleDataTypeName = ((ValidationDataType)rule.enRuleDataType).ToString(),
                                    EntityID = rule.EntityID,
                                    EntityName = rule.EntityName,
                                    ValidationDataValue = rule.ValidationDataValue,
                                    enStringCompareType = rule.enStringCompareType,
                                    enNumericCompareType = rule.enNumericCompareType,
                                    enDateCompareType = rule.enDateCompareType,
                                    ModifiedBy = rule.ModifiedBy,
                                    ModifiedDate = rule.ModifiedDate
                                });
                            }

                            break;
                    }

                    return new ObservableCollection<ValidationRule>(validationRules);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return null;
            }
        }

        /// <summary>
        /// Update an existing validation rule entity in the database
        /// </summary>
        /// <param name="validationRule">The validation rule entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateValidationRule(ValidationRule validationRule)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    ValidationRule existingValidationRule = db.ValidationRules.Where(p => p.pkValidationRuleID == validationRule.pkValidationRuleID).FirstOrDefault();

                    // Check to see if the validation rule name already exist for another entity 
                    if (existingValidationRule != null && existingValidationRule.pkValidationRuleID != validationRule.pkValidationRuleID)
                    {
                        _eventAggregator.GetEvent<MessageEvent>().Publish("The validationRule already exist.");
                        return false;
                    }
                    else
                    {
                        if (existingValidationRule != null)
                        {
                            existingValidationRule.fkValidationRulesDataID = validationRule.fkValidationRulesDataID;
                            existingValidationRule.EntityID = validationRule.EntityID;
                            existingValidationRule.ValidationDataValue = validationRule.ValidationDataValue;
                            existingValidationRule.enStringCompareType = validationRule.enStringCompareType;
                            existingValidationRule.enDateCompareType = validationRule.enDateCompareType;
                            existingValidationRule.enNumericCompareType = validationRule.enNumericCompareType;
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
        /// Update an existing validation rule entity in the database
        /// </summary>
        /// <param name="validationRule">The validation rule entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool ValidateDataRule(ValidationRule validationRule)
        {
            try
            {
                switch (((ValidationRuleGroup)validationRule.enValidationRuleGroup))
                {
                    case ValidationRuleGroup.Client:
                        break;
                    case ValidationRuleGroup.Company:
                        return ValidateCompanyDataRule(validationRule);
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
        public bool ApplyDataRule(ValidationRuleException validationExceptionInfo)
        {
            try
            {
                bool result = false;

                using (var db = MobileManagerEntities.GetContext())
                {
                    ValidationRule rule = ((DbQuery<ValidationRule>)(from validationRule in db.ValidationRules
                                                                     where validationRule.pkValidationRuleID == validationExceptionInfo.fkValidationRuleID
                                                                     select validationRule)).Include("ValidationRulesData").FirstOrDefault();

                    if (rule != null)
                    {
                        switch ((DataValidationEntity)validationExceptionInfo.enValidationEntity)
                        {
                            case DataValidationEntity.Client:
                                Client clientToUpdate = db.Clients.Where(p => p.pkClientID == validationExceptionInfo.EntityID).FirstOrDefault();

                                if (clientToUpdate != null)
                                {
                                    // Get the client table properties (Fields)
                                    PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(Client));

                                    foreach (PropertyDescriptor property in properties)
                                    {
                                        // Find the data column (property) to update
                                        if (rule.ValidationRulesData.ValidationDataName == property.Name)
                                        {
                                            // Update the property value based on the data type
                                            switch ((ValidationDataType)rule.ValidationRulesData.enValidationDataType)
                                            {
                                                case ValidationDataType.Date:
                                                    property.SetValue(clientToUpdate, Convert.ToDateTime(rule.ValidationRulesData.ValidationDataName));
                                                    break;
                                                case ValidationDataType.Integer:
                                                    property.SetValue(clientToUpdate, Convert.ToInt32(rule.ValidationRulesData.ValidationDataName));
                                                    break;
                                                case ValidationDataType.Decimal:
                                                    property.SetValue(clientToUpdate, Convert.ToDecimal(rule.ValidationRulesData.ValidationDataName));
                                                    break;
                                                case ValidationDataType.Bool:
                                                    property.SetValue(clientToUpdate, Convert.ToBoolean(rule.ValidationRulesData.ValidationDataName));
                                                    break;
                                                default:
                                                    property.SetValue(clientToUpdate, rule.ValidationRulesData.ValidationDataName);
                                                    break;
                                            }

                                            db.SaveChanges();
                                            result = true;
                                        }
                                    }
                                }
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
        /// Update an existing validation rule entity in the database
        /// </summary>
        /// <param name="validationRule">The validation rule entity to update.</param>
        /// <returns>True if successfull</returns>
        private bool ValidateCompanyDataRule(ValidationRule validationRule)
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
                    List<Client> companyClients = db.Clients.Where(p => p.fkCompanyID == validationRule.EntityID && p.IsActive).ToList();

                    if (companyClients.Count > 0)
                    {
                        // Get the Client table properties (Fields)
                        PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(Client));

                        foreach (PropertyDescriptor property in properties)
                        {
                            // Only validate when the rule's data name
                            // match the property name
                            if (validationRule.RuleDataName == property.Name)
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
                                    entityValue = db.Entry(client).Property(validationRule.RuleDataName).CurrentValue != null ? 
                                                  db.Entry(client).Property(validationRule.RuleDataName).CurrentValue.ToString() : string.Empty;

                                    switch (((ValidationDataType)Enum.Parse(typeof(ValidationDataType), validationRule.RuleDataTypeName)))
                                    {
                                        case ValidationDataType.String:
                                            ruleValue = string.Format("{0} {1} {2}", validationRule.RuleDataName.ToUpper(),
                                                                                     ((StringOperator)validationRule.enStringCompareType).ToString(),
                                                                                     validationRule.ValidationDataValue);
                                            result = _dataComparer.CompareStringValues((StringOperator)validationRule.enStringCompareType,
                                                                                       entityValue,
                                                                                       validationRule.ValidationDataValue);
                                            // If the operator is 'Equal' the the rule can be auto applied to fixed failures
                                            canApplyRule = (StringOperator)validationRule.enStringCompareType == StringOperator.Equal ? true : false;
                                            break;
                                        case ValidationDataType.Date:
                                            ruleValue = string.Format("{0} {1} {2}", validationRule.RuleDataName.ToUpper(),
                                                                                     ((StringOperator)validationRule.enStringCompareType).ToString(),
                                                                                     validationRule.ValidationDataValue);
                                            break;
                                        case ValidationDataType.Integer:
                                            ruleValue = string.Format("{0} {1} {2}", validationRule.RuleDataName.ToUpper(),
                                                                                     ((StringOperator)validationRule.enStringCompareType).ToString(),
                                                                                     validationRule.ValidationDataValue);
                                            result = _dataComparer.CompareNumericValues((NumericOperator)validationRule.enNumericCompareType,
                                                                                        entityValue,
                                                                                        validationRule.ValidationDataValue);
                                            // If the operator is 'Equal' the the rule can be auto applied to fixed failures
                                            canApplyRule = (NumericOperator)validationRule.enNumericCompareType == NumericOperator.Equal ? true : false;
                                            break;
                                        case ValidationDataType.Decimal:
                                            ruleValue = string.Format("{0} {1} {2}", validationRule.RuleDataName.ToUpper(),
                                                                                     ((StringOperator)validationRule.enStringCompareType).ToString(),
                                                                                     validationRule.ValidationDataValue);
                                            result = _dataComparer.CompareDecimalValues((NumericOperator)validationRule.enNumericCompareType,
                                                                                        entityValue,
                                                                                        validationRule.ValidationDataValue);
                                            // If the operator is 'Equal' the the rule can be auto applied to fixed failures
                                            canApplyRule = (NumericOperator)validationRule.enNumericCompareType == NumericOperator.Equal ? true : false;
                                            break;
                                        case ValidationDataType.Bool:
                                            ruleValue = string.Format("{0} {1} {2}", validationRule.RuleDataName.ToUpper(),
                                                                                     ((StringOperator)validationRule.enStringCompareType).ToString(),
                                                                                     validationRule.ValidationDataValue);
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
                                        _eventAggregator.GetEvent<DataValiationResultEvent>().Publish(new ValidationRuleException()
                                        {
                                            fkBillingProcessID = BillingExecutionState.DataValidation.Value(),
                                            fkValidationRuleID = validationRule.pkValidationRuleID,
                                            BillingPeriod = string.Format("{0}{1}", DateTime.Now.Month.ToString().PadLeft(2, '0'), DateTime.Now.Year),
                                            enValidationEntity = DataValidationEntity.Client.Value(),
                                            EntityID = client.pkClientID,
                                            Result = true
                                        });
                                    else
                                    {
                                        _eventAggregator.GetEvent<DataValiationResultEvent>().Publish(new ValidationRuleException()
                                        {
                                            fkBillingProcessID = BillingExecutionState.DataValidation.Value(),
                                            fkValidationRuleID = validationRule.pkValidationRuleID,
                                            BillingPeriod = string.Format("{0}{1}", DateTime.Now.Month.ToString().PadLeft(2, '0'), DateTime.Now.Year),
                                            enValidationEntity = DataValidationEntity.Client.Value(),
                                            EntityID = client.pkClientID,
                                            CanApplyRule = canApplyRule,
                                            Message = string.Format("{0} failed for {1} linked to company {2} - current value ({3}) - expected ({4}).",
                                                                    validationRule.RuleDataName.ToUpper(), entityName, validationRule.EntityName,
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
