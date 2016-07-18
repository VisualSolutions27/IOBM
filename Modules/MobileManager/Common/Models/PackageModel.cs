﻿using Gijima.IOBM.Common.Infrastructure;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class PackageModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public PackageModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new package entity in the database
        /// </summary>
        /// <param name="package">The package entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreatePackage(Package package)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.Packages.Any(p => p.PackageName.ToUpper() == package.PackageName))
                    {
                        db.Packages.Add(package);
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        _eventAggregator.GetEvent<MessageEvent>().Publish(string.Format("The {0} package already exist.", package.PackageName));
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
        /// Read all or active only packages from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of Packages</returns>
        public ObservableCollection<Package> ReadPackages(bool activeOnly, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<Package> packages = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    packages = ((DbQuery<Package>)(from package in db.Packages
                                                   where activeOnly ? package.IsActive : true &&
                                                         excludeDefault ? package.pkPackageID > 0 : true
                                                   select package)).Include("ServiceProvider")
                                                                   .OrderBy(p => p.PackageName).ToList();

                    return new ObservableCollection<Package>(packages);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return null;
            }
        }

        /// <summary>
        /// Update an existing package entity in the database
        /// </summary>
        /// <param name="package">The package entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdatePackage(Package package)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    Package existingPackage = db.Packages.Where(p => p.PackageName == package.PackageName).FirstOrDefault();

                    // Check to see if the package name already exist for another entity 
                    if (existingPackage != null && existingPackage.pkPackageID != package.pkPackageID)
                    {
                        _eventAggregator.GetEvent<MessageEvent>().Publish(string.Format("The {0} package already exist.", package.PackageName));
                        return false;
                    }
                    else
                    {
                        // Prevent primary key confilcts when using attach property
                        if (existingPackage != null)
                        {
                            existingPackage.fkServiceProviderID = package.fkServiceProviderID;
                            existingPackage.enPackageType = package.enPackageType;
                            existingPackage.PackageName = package.PackageName;
                            existingPackage.Cost = package.Cost;
                            existingPackage.TalkTimeMinutes = package.TalkTimeMinutes;
                            existingPackage.SMSNumber = package.SMSNumber;
                            existingPackage.RandValue = package.RandValue;
                            existingPackage.ModifiedBy = package.ModifiedBy;
                            existingPackage.ModifiedDate = package.ModifiedDate;
                            existingPackage.IsActive = package.IsActive;
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
