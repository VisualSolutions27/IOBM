using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.MobileManager.Common.Helpers;
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
    public class CompanyModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;
        private AuditLogModel _activityLogger = null;
        private DataActivityHelper _dataActivityHelper = null;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public CompanyModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            _activityLogger = new AuditLogModel(_eventAggregator);
            _dataActivityHelper = new DataActivityHelper(_eventAggregator);
        }

        /// <summary>
        /// Create a new company entity in the database
        /// </summary>
        /// <param name="company">The company entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateCompany(Company company)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.Companies.Any(p => p.CompanyName.ToUpper() == company.CompanyName))
                    {
                        db.Companies.Add(company);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        //_eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(string.Format("The {0} company already exist.", company.CompanyName));
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
        /// Read all or active only companies from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of Companies</returns>
        public ObservableCollection<Company> ReadCompanies(bool activeOnly, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<Company> companies = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    companies = ((DbQuery<Company>)(from company in db.Companies
                                                    select company)).Include("CompanyGroup")
                                                                    .OrderBy(p => p.CompanyName).ToList();

                    if (activeOnly)
                        companies = companies.Where(p => p.IsActive);

                    if (excludeDefault)
                        companies = companies.Where(p => p.pkCompanyID > 0);

                    return new ObservableCollection<Company>(companies);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        /// <summary>
        /// Find a company by company name from the database
        /// </summary>
        /// <param name="companyName">The company name to search for.</param>
        /// <returns>Company</returns>
        public Company ReadCompany(string companyName)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    return ((DbQuery<Company>)(from company in db.Companies
                                               select company)).Include("CompanyGroup")
                                                               .Include("CompanyBillingLevels")
                                                               .Include("CompanyBillingLevels.BillingLevel").FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        /// <summary>
        /// Read all or active only company groups from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <returns>Collection of Company Groups</returns>
        public ObservableCollection<CompanyGroup> ReadCompanyGroups(bool activeOnly)
        {
            try
            {
                IEnumerable<CompanyGroup> companyGroups = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    companyGroups = db.CompanyGroups.OrderBy(p => p.GroupName);

                    if (activeOnly)
                        companyGroups = companyGroups.Where(p => p.IsActive);

                    return new ObservableCollection<CompanyGroup>(companyGroups);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return null;
            }
        }

        /// <summary>
        /// Update an existing company entity in the database
        /// </summary>
        /// <param name="company">The company entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateCompany(Company company)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    Company existingCompany = db.Companies.Where(p => p.CompanyName == company.CompanyName).FirstOrDefault();

                    // Check to see if the company name already exist for another entity 
                    if (existingCompany != null && existingCompany.pkCompanyID != company.pkCompanyID)
                    {
                        //_eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(string.Format("The {0} company already exist.", company.CompanyName));
                        return false;
                    }
                    else
                    {
                        // Prevent primary key confilcts when using attach property
                        if (existingCompany != null)
                            db.Entry(existingCompany).State = System.Data.Entity.EntityState.Detached;

                        db.Companies.Attach(company);
                        db.Entry(company).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        return true;
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
        /// Update an existing company entity in the database
        /// </summary>
        /// <param name="searchCriteria">The client search criteria to search for.</param>
        /// <param name="updateColumn">The client entity property (column) to update.</param>
        /// <param name="updateValue">The value to update client entity property (column) with.</param>
        /// <param name="companyGroup">The company group the client is linked to.</param>
        /// <param name="errorMessage">OUT The error message.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateCompany(string searchCriteria, string updateColumn, object updateValue, CompanyGroup companyGroup, out string errorMessage)
        {
            errorMessage = string.Empty;
            Company existingCompany = null;
            Company companyToUpdate = null;
            Type propertyType = null;
            bool result = false;

            try
            {
                existingCompany = ReadCompany(searchCriteria);

                if (existingCompany != null)
                {
                    using (TransactionScope tc = TransactionHelper.CreateTransactionScope())
                    {
                        using (var db = MobileManagerEntities.GetContext())
                        {
                            companyToUpdate = db.Companies.Where(p => p.pkCompanyID == existingCompany.pkCompanyID).FirstOrDefault();

                            // Get the company table properties (Fields)
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
                                        property.SetValue(companyToUpdate, Convert.ToDateTime(updateValue));
                                    }
                                    else if (propertyType == typeof(int))
                                    {
                                        property.SetValue(companyToUpdate, Convert.ToInt32(updateValue));
                                    }
                                    else if (propertyType == typeof(decimal))
                                    {
                                        property.SetValue(companyToUpdate, Convert.ToDecimal(updateValue));
                                    }
                                    else if (propertyType == typeof(bool))
                                    {
                                        property.SetValue(companyToUpdate, Convert.ToBoolean(updateValue));
                                    }
                                    else if (propertyType == typeof(string))
                                    {
                                        property.SetValue(companyToUpdate, updateValue);
                                    }
                                    else
                                    {
                                        errorMessage = string.Format("Data type {0) not found for {1}.", property.PropertyType, updateColumn);
                                        return false;
                                    }

                                    // Add the data activity log
                                    result = _activityLogger.CreateDataChangeAudits<Company>(_dataActivityHelper.GetDataChangeActivities<Company>(existingCompany, companyToUpdate, companyToUpdate.pkCompanyID, db));

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
                    errorMessage = string.Format("Company {0} not found.", searchCriteria);
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
