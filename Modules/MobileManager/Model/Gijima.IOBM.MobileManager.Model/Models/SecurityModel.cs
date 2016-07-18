﻿using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.MobileManager.Model.Data;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity.Infrastructure;
using System.Linq;

namespace Gijima.IOBM.MobileManager.Model.Models
{
    public class SecurityModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public SecurityModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        #region Create Methods

        /// <summary>
        /// Create a new user entity in the database
        /// </summary>
        /// <param name="applicationUser">The user entity to add.</param>
        /// <returns>True if successfull</returns>
        public bool CreateApplicationUser(User applicationUser)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (db.Users.Any(p => p.UserName == applicationUser.UserName))
                    {
                        _eventAggregator.GetEvent<MessageEvent>().Publish(string.Format("User {0} already exists!", applicationUser.UserName));
                        return false;
                    }
                    else
                    {
                        applicationUser.LastActivityDate = DateTime.Now;
                        db.Users.Add(applicationUser);
                        db.SaveChanges();
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

        /// <summary>
        /// Save the application user role data
        /// </summary>
        /// <param name="applicationUserID">The application userID.</param>
        /// <param name="userRoles">The roles linked to the user.</param>
        /// <returns>True if successfull</returns>
        private bool CreateApplicationUserRoles(int applicationUserID, IEnumerable<UserInRole> userRoles)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    IEnumerable<UserInRole> exitingRoles = db.UserInRoles.Where(p => p.fkUserID == applicationUserID);

                    foreach (UserInRole userRole in exitingRoles)
                    {
                        db.UserInRoles.Remove(userRole);
                    }

                    db.SaveChanges();

                    foreach (UserInRole userRole in userRoles)
                    {
                        userRole.fkUserID = applicationUserID;
                        db.UserInRoles.Add(userRole);
                    }

                    db.SaveChanges();
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
        /// Save the application user company data
        /// </summary>
        /// <param name="applicationUserID">The application userID.</param>
        /// <param name="userCompanies">The companies linked to the user.</param>
        /// <returns>True if successfull</returns>
        private bool CreateApplicationUserCompanies(int applicationUserID, IEnumerable<UserInCompany> userCompanies)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    IEnumerable<UserInCompany> exitingCompanies = db.UserInCompanies.Where(p => p.fkUserID == applicationUserID);

                    foreach (UserInCompany userCompany in exitingCompanies)
                    {
                        db.UserInCompanies.Remove(userCompany);
                    }

                    db.SaveChanges();

                    foreach (UserInCompany userCompany in userCompanies)
                    {
                        userCompany.fkUserID = applicationUserID;
                        db.UserInCompanies.Add(userCompany);
                    }

                    db.SaveChanges();
                }

                return true;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return false;
            }
        }

        #endregion

        #region Read Methods

        /// <summary>
        /// Read the application user entityt and roles by user name.
        /// </summary>
        /// <param name="userName"> The user domain name to authenticate.</param>
        /// <returns> The user entity.</returns>
        public User ReadApplicationUser(string userName)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    IQueryable<User> query = from user in db.Users
                                             where string.Compare(user.UserName, userName, true) == 0 && user.IsActive
                                             select user;

                    return ((DbQuery<User>)query).Include("UserInRoles.Role")
                                                 .Include("UserInCompanies.Company").FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return null;
            }
        }

        /// <summary>
        /// Read all or active only users from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <returns>Collection of Users</returns>
        public ObservableCollection<User> ReadApplicationUsers(bool activeOnly)
        {
            try
            {
                IEnumerable<User> users = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    users = ((DbQuery<User>)(from user in db.Users
                                             where activeOnly ? user.IsActive : true
                                             select user)).OrderBy(p => p.UserFullName).ToList();

                    return new ObservableCollection<User>(users);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return null;
            }
        }

        /// <summary>
        /// Read all the application roles
        /// </summary>
        /// <returns>List of role entities</returns>
        public List<Role> ReadApplicationRoles()
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    return db.Roles.ToList();
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return null;
            }
        }

        /// <summary>
        /// Read all roles linked to the application user
        /// </summary>
        /// <param name="userID">The application userID to get roles for.</param>
        /// <returns>List of application user role entities</returns>
        public ObservableCollection<Role> ReadApplicationUserRoles(int userID)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    return new ObservableCollection<Role>(from role in db.Roles
                                                          join userRole in db.UserInRoles
                                                          on role.pkRoleID equals userRole.fkRoleID
                                                          where userRole.fkUserID == userID
                                                          select role);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return null;
            }
        }

        /// <summary>
        /// Read all companies linked to the application user
        /// </summary>
        /// <param name="userID">The application userID to get companies for.</param>
        /// <returns>List of application user company entities</returns>
        public ObservableCollection<Company> ReadApplicationUserCompanies(int userID)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    return new ObservableCollection<Company>(from company in db.Companies
                                                             join userCompany in db.UserInCompanies
                                                             on company.pkCompanyID equals userCompany.fkCompanyID
                                                             where userCompany.fkUserID == userID
                                                             select company);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return null;
            }
        }

        /// <summary>
        /// This is public method that encapsulate the functionality to get the server
        /// and database connection information.
        /// </summary>
        /// <param name="serverName">OUT - The server the user authenticate against.</param>
        /// <param name="databaseName">OUT - The databse the user authenticate against.</param> 
        public void ReadConnectionInfo(out string serverName, out string databaseName)
        {
            serverName = "Unknown";
            databaseName = "Unknown";

            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    serverName = db.Database.Connection.DataSource;
                    databaseName = db.Database.Connection.Database;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        #endregion

        #region Update Methods

        /// <summary>
        /// Update the specified application user data.
        /// </summary>
        /// <param name="applicationUser">The application user entity to save.</param>
        /// <returns>True is successfull</returns>
        public bool UpdateApplicationUser(User applicationUser)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    User existingUser = db.Users.Where(p => p.pkUserID == applicationUser.pkUserID).FirstOrDefault();

                    // Check to see if the device description already exist for another entity 
                    if (existingUser != null)
                    {
                        existingUser.UserFullName = applicationUser.UserFullName;
                        existingUser.IsActive = applicationUser.IsActive;
                        db.SaveChanges();

                        // Save the user roles
                        CreateApplicationUserRoles(applicationUser.pkUserID, applicationUser.UserInRoles);

                        // Save the user companies
                        CreateApplicationUserCompanies(applicationUser.pkUserID, applicationUser.UserInCompanies);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return false;
            }
        }

        #endregion
    }
}
