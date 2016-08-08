using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.MobileManager.Common.Structs;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class ImportRuleDataModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public ImportRuleDataModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new import rule data entity in the database
        /// </summary>
        /// <param name="importRuleData">The import rule data entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateImportRule(ImportRuleData importRuleData)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.ImportRuleDatas.Any(p => p.enImportSource == importRuleData.enImportSource &&
                                                     p.enImportDestination == importRuleData.enImportDestination))
                    {
                        db.ImportRuleDatas.Add(importRuleData);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        _eventAggregator.GetEvent<MessageEvent>().Publish(string.Format("The data import rule source {0} and destination {1} already exist.", 
                                                                                       ((DataImportColumns)importRuleData.enImportSource).ToString(),
                                                                                       ((DataImportEntities)importRuleData.enImportDestination).ToString()));
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
        /// Read all or active only data import rule data from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <returns>Collection of ImportRuleData</returns>
        public ObservableCollection<ImportRuleData> ReadImportRuleData(bool activeOnly)
        {
            try
            {
                IEnumerable<ImportRuleData> importRuleData = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    importRuleData = ((DbQuery<ImportRuleData>)(from ruleData in db.ImportRuleDatas
                                                                where activeOnly ? ruleData.IsActive : true
                                                                select ruleData)).ToList();

                    return new ObservableCollection<ImportRuleData>(importRuleData);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return null;
            }
        }

        /// <summary>
        /// Update an existing validation rule data entity in the database
        /// </summary>
        /// <param name="validationRulesData">The validation rule data entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateValidationRulesData(ValidationRulesData validationRulesData)
        {
            try
            {
                //using (var db = MobileManagerEntities.GetContext())
                //{
                //    ValidationRulesData existingValidationRulesData = db.ValidationRulesDatas.Where(p => p.enValidationRuleGroup == validationRulesData.enValidationRuleGroup &&
                //                                                                                         p.ValidationDataName == validationRulesData.ValidationDataName).FirstOrDefault();

                //    // Check to see if the validation rule data name already exist for another entity 
                //    if (existingValidationRulesData != null && existingValidationRulesData.pkValidationRulesDataID != validationRulesData.pkValidationRulesDataID)
                //    {
                //        _eventAggregator.GetEvent<MessageEvent>().Publish(string.Format("The validation rule data {0} already exist.", validationRulesData.ValidationDataName));
                //        return false;
                //    }
                //    else
                //    {
                //        if (existingValidationRulesData != null)
                //        {
                //            existingValidationRulesData.enValidationRuleGroup = validationRulesData.enValidationRuleGroup;
                //            existingValidationRulesData.ValidationDataName = validationRulesData.ValidationDataName;
                //            existingValidationRulesData.ModifiedBy = validationRulesData.ModifiedBy;
                //            existingValidationRulesData.ModifiedDate = validationRulesData.ModifiedDate;
                //            db.SaveChanges();
                //            return true;
                //        }

                //        return false;
                //    }
                //}

                return true;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return false;
            }
        }
    }
}
