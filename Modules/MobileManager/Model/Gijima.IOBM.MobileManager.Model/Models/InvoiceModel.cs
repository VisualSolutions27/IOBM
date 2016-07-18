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
    public class InvoiceModel
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public InvoiceModel(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Create a new invoice entity in the database
        /// </summary>
        /// <param name="invoice">The invoice entity to add.</param>
        /// <param name="servicePrefix">The selected service to create a invoice for.</param>
        /// <returns>True if successfull</returns>
        public bool CreateInvoice(Invoice invoice, string servicePrefix)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    if (!db.Invoices.Any(p => p.fkClientID == invoice.fkClientID &&
                                              p.fkServiceID == invoice.fkServiceID &&
                                              p.InvoicePeriod == invoice.InvoicePeriod &&
                                              p.PrivateDue == invoice.PrivateDue &&
                                              p.CompanyDue == invoice.CompanyDue))
                    {
                        // Create the invoice entity before any invoice items can be created
                        invoice.InvoiceNumber = GenerateInvoiceNumber(servicePrefix);
                        invoice.InvoiceTotal = invoice.PrivateDue + invoice.CompanyDue;
                        invoice.IsPeriodClosed = false;
                        db.Invoices.Add(invoice);
                        db.SaveChanges();

                        // Create the invoice detail
                        CreateInvoiceItems(invoice.pkInvoiceID, invoice.InvoiceDetails, db);

                        return true;
                    }
                    else
                    {
                        _eventAggregator.GetEvent<MessageEvent>().Publish("The invoice already exist.");
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
        /// Create a new invoice item entity in the database
        /// </summary>
        /// <param name="invoiceID">The invoice ID linked to the invoice items.</param>
        /// <param name="invoiceItems">The list of invoice item entity to add.</param>
        /// <param name="db">The current data context.</param>
        /// <returns>True if successfull</returns>
        private bool CreateInvoiceItems(int invoiceID, IEnumerable<InvoiceDetail> invoiceItems, MobileManagerEntities db)
        {
            try
            {
                foreach (InvoiceDetail item in invoiceItems)
                {
                    item.fkInvoiceID = invoiceID;
                    db.InvoiceDetails.Add(item);
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
        /// Read all invoices from the database
        /// </summary>
        /// <param name="clientID">The client ID to read accounts for.</param>
        /// <param name="accPeriodFilter">The selected account period to filter on..</param>
        /// <param name="serviceFilter">The selected serive to filter on.</param>
        /// <returns>Collection of Invoices</returns>
        public ObservableCollection<Invoice> ReadInvoicesForClient(int clientID, string accPeriodFilter = "None", int serviceFilter = 0)
        {
            try
            {
                IEnumerable<Invoice> invoices = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    invoices = ((DbQuery<Invoice>)(from invoice in db.Invoices
                                                   where invoice.fkClientID == clientID
                                                   select invoice)).Include("Client")
                                                                   .Include("Client.Company")
                                                                   .Include("Service1")
                                                                   .OrderByDescending(p => p.InvoicePeriod).ToList();

                    if (accPeriodFilter != "None")
                        invoices = invoices.Where(p => p.InvoicePeriod == accPeriodFilter);

                    if (serviceFilter > 0)
                        invoices = invoices.Where(p => p.fkServiceID == serviceFilter);

                    return new ObservableCollection<Invoice>(invoices);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return null;
            }
        }

        /// <summary>
        /// Read all invoice item for the specified invoice from the database
        /// </summary>
        /// <param name="invoiceID">The invoice ID linked to the invoice items.</param>
        /// <returns>Collection of Invoice Items</returns>
        public ObservableCollection<InvoiceDetail> ReadInvoiceItems(int invoiceID)
        {
            try
            {
                IEnumerable<InvoiceDetail> invoiceItems = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    invoiceItems = ((DbQuery<InvoiceDetail>)(from items in db.InvoiceDetails
                                                             where items.fkInvoiceID == invoiceID
                                                             select items)).Include("ServiceProvider").ToList();

                    return new ObservableCollection<InvoiceDetail>(invoiceItems);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return null;
            }
        }

        /// <summary>
        /// Read all or active only services from the database
        /// </summary>
        /// <param name="activeOnly">Flag to load all or active only entities.</param>
        /// <param name="excludeDefault">Flag to include or exclude the default entity.</param>
        /// <returns>Collection of Packages</returns>
        public ObservableCollection<Service> ReadServices(bool activeOnly, bool excludeDefault = false)
        {
            try
            {
                IEnumerable<Service> services = null;

                using (var db = MobileManagerEntities.GetContext())
                {
                    services = ((DbQuery<Service>)(from service in db.Services
                                                   where activeOnly ? service.IsActive : true &&
                                                         excludeDefault ? service.pkServiceID > 0 : true
                                                   select service)).OrderBy(p => p.ServiceDescription).ToList();

                    return new ObservableCollection<Service>(services);
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return null;
            }
        }

        /// <summary>
        /// Read the invoice data for the selected invoice
        /// </summary>
        /// <param name="invoiceID">The primary key for the selected invoice.</param>
        public List<sp_report_Invoice_Result> ReadInvoiceData(int invoiceID)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    return db.sp_report_Invoice(invoiceID).ToList();
                }
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Update an existing invoice entity in the database
        /// </summary>
        /// <param name="invoice">The invoice entity to update.</param>
        /// <returns>True if successfull</returns>
        public bool UpdateInvoice(Invoice invoice)
        {
            try
            {
                using (var db = MobileManagerEntities.GetContext())
                {
                    Invoice existingInvoice = db.Invoices.Where(p => p.pkInvoiceID == invoice.pkInvoiceID).FirstOrDefault();

                    if (existingInvoice != null)
                    {
                        existingInvoice.InvoicePeriod = invoice.InvoicePeriod;
                        existingInvoice.InvoiceDate = invoice.InvoiceDate;
                        existingInvoice.PrivateDue = invoice.PrivateDue;
                        existingInvoice.CompanyDue = invoice.CompanyDue;
                        existingInvoice.InvoiceTotal = invoice.PrivateDue + invoice.CompanyDue;
                        existingInvoice.ModifiedBy = invoice.ModifiedBy;
                        existingInvoice.ModifiedDate = invoice.ModifiedDate;
                        db.SaveChanges();

                        // Update the invoice detail
                        UpdateInvoiceItems(invoice.pkInvoiceID, invoice.InvoiceDetails, db);

                        return true;
                    }

                    return false;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return false;
            }
        }

        /// <summary>
        /// Update an existing invoice item entity in the database
        /// </summary>
        /// <param name="invoiceID">The invoice ID linked to the invoice items.</param>
        /// <param name="invoiceItems">The list of invoice item entity to update.</param>
        /// <param name="db">The current data context.</param>
        /// <returns>True if successfull</returns>
        private void UpdateInvoiceItems(int invoiceID, IEnumerable<InvoiceDetail> invoiceItems, MobileManagerEntities db)
        {
            try
            {
                IEnumerable<InvoiceDetail> existingItems = db.InvoiceDetails.Where(p => p.fkInvoiceID == invoiceID).ToList();

                foreach (InvoiceDetail item in existingItems)
                {
                    InvoiceDetail itemToUpdate = invoiceItems.Where(p => p.pkInvoiceItemID == item.pkInvoiceItemID).FirstOrDefault();

                    if (itemToUpdate != null)
                    {
                        item.fkServiceProviderID = itemToUpdate.fkServiceProviderID;
                        item.ItemDescription = itemToUpdate.ItemDescription;
                        item.ReferenceNumber = itemToUpdate.ReferenceNumber;
                        item.ItemAmount = itemToUpdate.ItemAmount;
                        item.IsPrivate = itemToUpdate.IsPrivate;
                        item.ModifiedBy = itemToUpdate.ModifiedBy;
                        item.ModifiedDate = itemToUpdate.ModifiedDate;
                    }
                    else
                    {
                        db.InvoiceDetails.Remove(item);
                    }
                }

                db.SaveChanges();
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
            }
        }

        /// <summary>
        /// Generate the next invoice number for the selected service
        /// </summary>
        /// <param name="servicePreFix">The selected service prefix.</param>
        /// <returns>Invoice number</returns>
        private string GenerateInvoiceNumber(string servicePreFix)
        {
            try
            {
                string invoiceNo = string.Empty;

                using (var db = MobileManagerEntities.GetContext())
                {
                    var latestInvoice = ((DbQuery<string>)(from invoice in db.Invoices
                                                           where invoice.InvoiceNumber.Substring(0, 3) == servicePreFix
                                                           select invoice.InvoiceNumber.Substring(3))).Max();

                    return string.Format("{0}{1}", servicePreFix, (Convert.ToInt32(latestInvoice) + 1).ToString().PadLeft(9, '0'));
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
