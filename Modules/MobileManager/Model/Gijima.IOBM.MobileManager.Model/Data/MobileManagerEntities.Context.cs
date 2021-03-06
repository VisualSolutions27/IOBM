﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Gijima.IOBM.MobileManager.Model.Data
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class MobileManagerEntities : DbContext
    {
        public MobileManagerEntities()
            : base("name=MobileManagerEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<AppSetting> AppSettings { get; set; }
        public virtual DbSet<AuditLog> AuditLogs { get; set; }
        public virtual DbSet<BillingLevel> BillingLevels { get; set; }
        public virtual DbSet<BillingProcess> BillingProcesses { get; set; }
        public virtual DbSet<BillingProcessHistory> BillingProcessHistories { get; set; }
        public virtual DbSet<City> Cities { get; set; }
        public virtual DbSet<Client> Clients { get; set; }
        public virtual DbSet<ClientBilling> ClientBillings { get; set; }
        public virtual DbSet<ClientLocation> ClientLocations { get; set; }
        public virtual DbSet<Company> Companies { get; set; }
        public virtual DbSet<CompanyBillingLevel> CompanyBillingLevels { get; set; }
        public virtual DbSet<CompanyGroup> CompanyGroups { get; set; }
        public virtual DbSet<Contract> Contracts { get; set; }
        public virtual DbSet<DataImportRule> DataImportRules { get; set; }
        public virtual DbSet<DataUpdateRule> DataUpdateRules { get; set; }
        public virtual DbSet<DataValidationException> DataValidationExceptions { get; set; }
        public virtual DbSet<DataValidationProperty> DataValidationProperties { get; set; }
        public virtual DbSet<DataValidationRule> DataValidationRules { get; set; }
        public virtual DbSet<DeviceMake> DeviceMakes { get; set; }
        public virtual DbSet<DeviceModel> DeviceModels { get; set; }
        public virtual DbSet<ExternalBillingData> ExternalBillingDatas { get; set; }
        public virtual DbSet<Invoice> Invoices { get; set; }
        public virtual DbSet<InvoiceDetail> InvoiceDetails { get; set; }
        public virtual DbSet<Package> Packages { get; set; }
        public virtual DbSet<PackageSetup> PackageSetups { get; set; }
        public virtual DbSet<Province> Provinces { get; set; }
        public virtual DbSet<Report> Reports { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Service> Services { get; set; }
        public virtual DbSet<ServiceProvider> ServiceProviders { get; set; }
        public virtual DbSet<SimCard> SimCards { get; set; }
        public virtual DbSet<Status> Status { get; set; }
        public virtual DbSet<Suburb> Suburbs { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserInCompany> UserInCompanies { get; set; }
        public virtual DbSet<UserInRole> UserInRoles { get; set; }
        public virtual DbSet<Device> Devices { get; set; }
    
        public virtual ObjectResult<sp_report_Invoice_Result> sp_report_Invoice(Nullable<int> invoiceID)
        {
            var invoiceIDParameter = invoiceID.HasValue ?
                new ObjectParameter("invoiceID", invoiceID) :
                new ObjectParameter("invoiceID", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<sp_report_Invoice_Result>("sp_report_Invoice", invoiceIDParameter);
        }
    }
}
