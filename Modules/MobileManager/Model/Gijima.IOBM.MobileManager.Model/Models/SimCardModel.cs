using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class SimCardModel
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
        public SimCardModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
            _activityLogger = new AuditLogModel(_eventAggregator);
            _dataActivityHelper = new DataActivityHelper(_eventAggregator);
        }

        /// <summary>
        /// Create a new Sim card entity in the database
        /// </summary>
        /// <param name="simCard">The Sim card entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateSimCard(SimCard simCard)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.SimCards.Any(p => p.PUKNumber == simCard.PUKNumber))
                    {
                        db.SimCards.Add(simCard);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        _eventAggregator.GetEvent<MessageEvent>().Publish(string.Format("The {0}, {1} sim card already exist.", simCard.CellNumber, simCard.PUKNumber));
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
        /// Read all or active only Sim cards from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of SimCard</returns>
        public ObservableCollection<SimCard> ReadSimCard(bool activeOnly, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<SimCard> simCards = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    simCards = ((DbQuery<SimCard>)(from simCard in db.SimCards
                                                   where activeOnly ? simCard.IsActive : true &&
                                                         excludeDefault ? simCard.pkSimCardID > 0 : true
                                                   select simCard)).ToList();

                    return new ObservableCollection<SimCard>(simCards);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return null;
            }
        }

        /// <summary>
        /// Read all or active only Sim cards linked to the specified contract from the database
        /// </summary>
        /// <param name="contractID">The contract primary key linked to the SimCards.</param>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <returns>Collection of SimCard</returns>
        public ObservableCollection<SimCard> ReadSimCardsForContract(int contractID, bool activeOnly = false)
        {
            try
            {
                IEnumerable<SimCard> simCards = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    simCards = ((DbQuery<SimCard>)(from simCard in db.SimCards
                                                   where simCard.fkContractID == contractID
                                                   select simCard)).Include("Devices")
                                                                      .Include("Devices.DeviceMake")
                                                                      .Include("Devices.DeviceModel")
                                                                      .Include("Status").ToList();

                    if (activeOnly)
                        simCards = simCards.Where(p => p.IsActive);

                    return new ObservableCollection<SimCard>(simCards);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return null;
            }
        }

        /// <summary>
        /// Update an existing Sim card entity in the database
        /// </summary>
        /// <param name="simCard">The Sim card entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateSimCard(SimCard simCard)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    SimCard existingSimCard = db.SimCards.Where(p => p.pkSimCardID == simCard.pkSimCardID).FirstOrDefault();

                    // Check to see if the sim card already exist for another entity 
                    if (existingSimCard != null && existingSimCard.pkSimCardID != simCard.pkSimCardID && 
                        (existingSimCard.PUKNumber == simCard.PUKNumber || existingSimCard.CardNumber == simCard.CardNumber ||
                         existingSimCard.CellNumber == simCard.CellNumber))
                    {
                        _eventAggregator.GetEvent<MessageEvent>().Publish(string.Format("The {0}, {1} sim card already exist.", simCard.CellNumber, simCard.PUKNumber));
                        return false;
                    }
                    else
                    {
                        // Prevent primary key confilcts when using attach property
                        if (existingSimCard != null)
                            db.Entry(existingSimCard).State = System.Data.Entity.EntityState.Detached;

                        db.SimCards.Attach(simCard);
                        db.Entry(simCard).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        _activityLogger.CreateDataChangeAudits<SimCard>(_dataActivityHelper.GetDataChangeActivities<SimCard>(existingSimCard, simCard, simCard.fkContractID.Value, db));
                        return true;
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
