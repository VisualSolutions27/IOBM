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
    public class DataValidationPropertyModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public DataValidationPropertyModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new data validation property in the database
        /// </summary>
        /// <param name="validationProperty">The data validation property to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateDataValidationProperty(DataValidationProperty validationProperty)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.DataValidationProperties.Any(p => p.enDataValidationEntity == validationProperty.enDataValidationEntity &&
                                                              p.enDataValidationProperty == validationProperty.enDataValidationProperty))
                    {
                        db.DataValidationProperties.Add(validationProperty);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        _eventAggregator.GetEvent<MessageEvent>().Publish(string.Format("The data validation property {0} already exist.", 
                                                                                        ((DataValidationPropertyName)validationProperty.enDataValidationProperty)).ToString());
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
        /// Read all or active only data validation properties from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <returns>Collection of DataValidationProperties</returns>
        public ObservableCollection<DataValidationProperty> ReadDataValidationProperties(bool activeOnly)
        {
            try
            {
                IEnumerable<DataValidationProperty> validationRulesData = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    validationRulesData = ((DbQuery<DataValidationProperty>)(from validationProperty in db.DataValidationProperties
                                                                             where activeOnly ? validationProperty.IsActive : true
                                                                             select validationProperty)).ToList();

                    return new ObservableCollection<DataValidationProperty>(validationRulesData);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return null;
            }
        }

        /// <summary>
        /// Update an existing data validation property in the database
        /// </summary>
        /// <param name="validationProperty">The data validation property to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateDataValidationProperty(DataValidationProperty validationProperty)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    DataValidationProperty existingDataValidationProperty = db.DataValidationProperties.Where(p => p.enDataValidationEntity == validationProperty.enDataValidationEntity &&
                                                                                                                   p.enDataValidationProperty == validationProperty.enDataValidationProperty).FirstOrDefault();

                    // Check to see if the data validation rule property already exist for another entity 
                    if (existingDataValidationProperty != null && existingDataValidationProperty.pkDataValidationPropertyID != validationProperty.pkDataValidationPropertyID)
                    {
                        _eventAggregator.GetEvent<MessageEvent>().Publish(string.Format("The data validation property {0} already exist.", 
                                                                                        ((DataValidationPropertyName)validationProperty.enDataValidationProperty)).ToString());
                        return false;
                    }
                    else
                    {
                        if (existingDataValidationProperty != null)
                        {
                            existingDataValidationProperty.enDataValidationEntity = validationProperty.enDataValidationEntity;
                            existingDataValidationProperty.enDataValidationProperty = validationProperty.enDataValidationProperty;
                            existingDataValidationProperty.ModifiedBy = validationProperty.ModifiedBy;
                            existingDataValidationProperty.ModifiedDate = validationProperty.ModifiedDate;
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
    }
}
