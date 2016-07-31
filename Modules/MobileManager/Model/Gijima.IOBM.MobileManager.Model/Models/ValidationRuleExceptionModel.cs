using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class ValidationRuleExceptionModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public ValidationRuleExceptionModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Save all the data rule exceptions to the database
        /// </summary>
        /// <param name="validationRuleExceptions">List of validation rule exceptions to save.</param>
        /// <returns>True if successfull</returns>
        public bool CreateValidationRuleExceptions(IEnumerable<ValidationRuleException> validationRuleExceptions)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    string billingPeriod = validationRuleExceptions.First().BillingPeriod;

                    IEnumerable<ValidationRuleException> exceptionsToDelete = db.ValidationRuleExceptions.Where(p => p.BillingPeriod == billingPeriod).ToList();

                    // First delete all the current exceptions 
                    // for this billing period
                    // There should never be exceptions for more than
                    // one billing process for the same billing period 
                    if (exceptionsToDelete.Count() > 0)
                    {
                        foreach (ValidationRuleException exception in exceptionsToDelete)
                        {
                            db.ValidationRuleExceptions.Remove(exception);
                        }
                        db.SaveChanges();
                    }

                    // Add the new exceptions for the billing period
                    foreach (ValidationRuleException exception in validationRuleExceptions)
                    {
                        db.ValidationRuleExceptions.Add(exception);
                        //db.SaveChanges();
                    }
                    db.SaveChanges();

                    return true;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return false;
            }
        }

        /// <summary>
        /// Read all the validation rule exceptions for the specified billing period from the database
        /// </summary>
        /// <param name="billingPeriod">The billing period to read exceptions for.</param>
        /// <returns>Collection of ValidationRuleException</returns>
        public ObservableCollection<ValidationRuleException> ReadValidationRuleExceptions(string billingPeriod)
        {
            try
            {
                IEnumerable<ValidationRuleException> validationRuleExceptions = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    validationRuleExceptions = ((DbQuery<ValidationRuleException>)(from ruleException in db.ValidationRuleExceptions
                                                                                   where ruleException.BillingPeriod == billingPeriod
                                                                                   select ruleException)).ToList();

                    return new ObservableCollection<ValidationRuleException>(validationRuleExceptions);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return null;
            }
        }
    }
}
