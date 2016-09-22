using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class CompanyBillingLevelModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public CompanyBillingLevelModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new company billing level entity in the database
        /// </summary>
        /// <param name="companyID">The companyID the billing level is linked to.</param>
        /// <param name="companyBillingLevel">The company billing level entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateCompanyBillingLevel(int companyID, CompanyBillingLevel companyBillingLevel)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.CompanyBillingLevels.Any(p => p.fkCompanyGroupID == companyBillingLevel.fkCompanyGroupID &&
                                                          p.fkBillingLevelID == companyBillingLevel.fkBillingLevelID))
                    {
                        db.CompanyBillingLevels.Add(companyBillingLevel);
                        db.SaveChanges();

                        return new CompanyModel(_eventAggregator).UpdateCompanyBillingLevelGroup(companyID, companyBillingLevel.fkCompanyGroupID);
                    }
                    else
                    {
                        //_eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(string.Format("The {0} company billing level already exist.", companyBillingLevel.BillingLevel.LevelDescription));
                        return false;
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
        /// Read all the company billing levels from the database
        /// </summary>
        /// <param name="companyGroupID">The company group linked to the billing levels.</param>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of CompanyBillingLevels</returns>
        public ObservableCollection<CompanyBillingLevel> ReadCompanyBillingLevels(int companyGroupID, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<CompanyBillingLevel> companyBillingLevels = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    companyBillingLevels = ((DbQuery<CompanyBillingLevel>)(from companyLevel in db.CompanyBillingLevels
                                                                           join billingLevel in db.BillingLevels on companyLevel.fkBillingLevelID equals billingLevel.pkBillingLevelID
                                                                           where companyLevel.fkCompanyGroupID == 0 ||
                                                                                 companyLevel.fkCompanyGroupID == companyGroupID
                                                                           select companyLevel)).Include("BillingLevel")
                                                                                                .Include("CompanyGroup")
                                                                                                .OrderBy(p => p.BillingLevel.LevelDescription).ToList();

                    if (excludeDefault)
                        companyBillingLevels = companyBillingLevels.Where(p => p.pkCompanyBillingLevelID > 0);

                    return new ObservableCollection<CompanyBillingLevel>(companyBillingLevels);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        /// <summary>
        /// Update an existing company billing level entity in the database
        /// </summary>
        /// <param name="companyID">The companyID the billing level is linked to.</param>
        /// <param name="companyBillingLevel">The company billing level entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateCompanyBillingLevel(int companyID, CompanyBillingLevel companyBillingLevel)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    CompanyBillingLevel existingBillingLevel = db.CompanyBillingLevels.Where(p => p.pkCompanyBillingLevelID == companyBillingLevel.pkCompanyBillingLevelID).FirstOrDefault();

                    // Check to see if the existing billing level description already exist for another entity 
                    if (existingBillingLevel != null && existingBillingLevel.pkCompanyBillingLevelID != companyBillingLevel.pkCompanyBillingLevelID)
                    {
                        //_eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(string.Format("The {0} company billing level already already exist.", companyBillingLevel.BillingLevel.LevelDescription));
                        return false;
                    }
                    else
                    {
                        // Prevent primary key confilcts when using attach property
                        if (existingBillingLevel != null)
                            db.Entry(existingBillingLevel).State = System.Data.Entity.EntityState.Detached;

                        db.CompanyBillingLevels.Attach(companyBillingLevel);
                        db.Entry(companyBillingLevel).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        return new CompanyModel(_eventAggregator).UpdateCompanyBillingLevelGroup(companyID, companyBillingLevel.fkCompanyGroupID);
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
        /// Delete an existing company billing level entity from the database
        /// </summary>
        /// <param name="companyBillingLevelID">The ID of the company billing level entity to delete.</param>
        /// <returns>True if successfull</returns>
        public bool DeleteCompanyBillingLevel(int companyBillingLevelID)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    CompanyBillingLevel billingLevelToDelete = db.CompanyBillingLevels.Where(p => p.pkCompanyBillingLevelID == companyBillingLevelID).FirstOrDefault();

                    if (billingLevelToDelete != null)
                    {
                        db.CompanyBillingLevels.Remove(billingLevelToDelete);
                        db.SaveChanges();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return false;
            }
        }
    }
}
