using System;
using System.ComponentModel;

namespace Gijima.IOBM.MobileManager.Common.Structs
{
    /// <summary>
    /// The <see cref="SecurityRole"/> enumeration lists of
    /// application security roles.
    /// </summary>
    public enum SecurityRole
    {
        Administrator = 1,
        Supervisor = 2,
        User = 3,
        ReadOnly = 4
    }

    /// <summary>
    /// The <see cref="ActionCompleted"/> enumeration lists of
    /// actions that completed.
    /// </summary>
    public enum ActionCompleted
    {
        ReadContractSimCards
    }

    /// <summary>
    /// The <see cref="StatusLink"/> enumeration lists of
    /// option status entites can be linked to.
    /// </summary>
    public enum StatusLink
    {
        All = 0,
        Contract = 1,
        Device = 2,
        Sim = 3,
        ContractDevice = 4,
        ContractSim = 5,
        DeviceSim = 6
    }

    /// <summary>
    /// The <see cref="SearchEntity"/> enumeration lists of entities
    /// the user can search on.
    /// </summary>
    public enum SearchEntity
    {
        ClientID,
        EmployeeNumber,
        PrimaryCellNumber,
        IDNumber,
        Email,
        CompanyName,
        PackageName,
        CellNumber,
        AccountNumber,
        Other
    }


    /// <summary>
    /// The <see cref="BillingProcess"/> enumeration a list of 
    /// billing processes.
    /// </summary>
    public enum BillingExecutionState
    {
        Started = 1,
        DataValidation = 2,
        DataImport = 3
    }

    /// <summary>
    /// The <see cref="DataBaseEntity"/> enumeration a list of 
    /// data update column options.
    /// </summary>
    public enum DataBaseEntity
    {
        [Description("-- Please Select --")]
        None = 0,
        [Description("Clients")]
        Client = 1,
        [Description("Client Billing")]
        ClientBilling = 2,
        [Description("Client Locations")]
        ClientLocation = 3,
        [Description("Companies")]
        Company = 4,
        [Description("Company Billing Levels")]
        CompanyBillingLevel = 5,
        [Description("Contracts")]
        Contract = 6,
        [Description("Devices")]
        Device = 7,
        [Description("Device Makes")]
        DeviceMake = 8,
        [Description("Device Models")]
        DeviceModel = 9,
        [Description("Packages")]
        Package = 10,
        [Description("Client Package Setup")]
        PackageSetup = 11,
        [Description("Provinces")]
        Province = 12,
        [Description("Service Providers")]
        ServiceProvider = 13,
        [Description("Sim Cards")]
        SimCard = 14,
        [Description("Suburbs")]
        Suburb = 15,
        [Description("Cities")]
        City = 16,
    }

    #region Types

    /// <summary>
    /// The <see cref="PackageType"/> enumeration lists of package types.
    /// </summary>
    public enum PackageType
    {
        NONE = 0,
        VOICE = 1,
        DATA = 2
    }

    /// <summary>
    /// The <see cref="CostType"/> enumeration lists of package cost types.
    /// </summary>
    public enum CostType
    {
        NONE = 0,
        STANDARD = 1,
        AMORTIZED = 2
    }

    #endregion

    #region Data Activity

    /// <summary>
    /// The <see cref="ActivityProcess"/> enumeration a list of administartion
    /// options that activity logs can be filtered on.
    /// </summary>
    public enum ActivityProcess
    {
        Administration = 1,
        Maintenance,
        Configuartion
    }

    /// <summary>
    /// The <see cref="AdminActivityFilter"/> enumeration a list of administartion
    /// options that activity logs can be filtered on.
    /// </summary>
    public enum AdminActivityFilter
    {
        None,
        Client,
        Contract,
        ClientBilling,
        Device,
        SimCard,
        PackageSetup,
        Manual
    }

    /// <summary>
    /// The <see cref="MaintActivityFilter"/> enumeration a list of maintenance
    /// options that activity logs can be filtered on.
    /// </summary>
    public enum MaintActivityFilter
    {
        None,
        City,
        ClientLocation,
        Company,
        DeviceMake,
        DeviceModel,
        Package,
        Province,
        ServiceProvider,
        Status,
        Suburb,
        CompanyBillingLevel
    }

    /// <summary>
    /// The <see cref="ConfigActivityFilter"/> enumeration a list of configuration
    /// options that activity logs can be filtered on.
    /// </summary>
    public enum ConfigActivityFilter
    {
        None,
    }

    #endregion

    #region Data Validation

    /// <summary>
    /// The <see cref="DataValidationProcess"/> enumeration a list of 
    /// data validation processes on.
    /// </summary>
    public enum DataValidationProcess
    {
        [Description("-- Please Select --")]
        None = 0,
        [Description("System Data")]
        System = 1,
        [Description("Billing Data")]
        Billing = 2,
        [Description("External Data")]
        External = 3
    }

    /// <summary>
    /// The <see cref="DataValidationGroupName"/> enumeration a list of 
    /// data entities to validate on.
    /// </summary>
    public enum DataValidationGroupName
    {
        [Description("-- Please Select --")]
        None = 0,
        [Description("Client Data")]
        Client = 1,
        [Description("Company Data")]
        Company = 2,
        [Description("Company Client Data")]
        CompanyClient = 3,
        [Description("Package Data")]
        Package = 4,
        [Description("Package Client Data")]
        PackageClient = 5,
        [Description("Device Data")]
        Device = 6,
        [Description("Simcard Data")]
        SimCard = 7,
        [Description("Status Client Data")]
        StatusClient = 8
    }

    /// <summary>
    /// The <see cref="DataValidationPropertyName"/> enumeration a list of 
    /// data properties to validate on.
    /// </summary>
    public enum DataValidationPropertyName
    {
        [Description("-- Please Select --")]
        None = 0,
        [Description("Cell Number")]
        PrimaryCellNumber = 1,
        [Description("Client Employee Number")]
        EmployeeNumber = 2,
        [Description("Client Name")]
        ClientName = 3,
        [Description("Client Land Line")]
        LandLine = 4,
        [Description("Client ID Number")]
        IDNumber = 5,
        [Description("Client EMail Address")]
        Email = 6,
        [Description("Cost Code")]
        CostCode = 7,
        [Description("WBS Number")]
        WBSNumber = 8,
        [Description("Admin Fee")]
        AdminFee = 9,
        [Description("IP Address")]
        IPAddress = 10,
        [Description("Client Street Address")]
        AddressLine1 = 11,
        [Description("Client Suburb")]
        fkSuburbID = 12,
        [Description("Is Client Billable")]
        IsBillable = 13,
        [Description("Has Split Billing")]
        IsSplitBilling = 14,
        [Description("Client Voice Allowance")]
        VoiceAllowance = 15,
        [Description("Client SP Limit")]
        SPLimit = 16,
        [Description("Company Name")]
        CompanyName = 17,
        [Description("Company Billing Levels")]
        CompanyBillingLevel = 18,
        [Description("Contract Account Number")]
        AccountNumber = 19,
        [Description("Contract Start Date")]
        ContractStartDate = 20,
        [Description("Contract End Date")]
        ContractEndDate = 21,
        [Description("Contract Upgrade Date")]
        ContractUpgradeDate = 22,
        [Description("Contract Payment Cancel Date")]
        PaymentCancelDate = 23,
        [Description("Device IME Number")]
        IMENumber = 24,
        [Description("Device Serial Number")]
        SerialNumber = 25,
        [Description("Package Name")]
        PackageName = 26,
        [Description("Package Cost")]
        Cost = 27,
        [Description("Package MB Data")]
        MBData = 28,
        [Description("Package Talk Time")]
        TalkTimeMinutes = 29,
        [Description("Package SMS Limit")]
        SMSNumber = 30,
        [Description("Package Rand Value")]
        RandValue = 31,
        [Description("Package SPUL Value")]
        SPULValue = 32,
        [Description("Sim Card Number")]
        CardNumber = 33,
        [Description("Sim Card PUK Number")]
        PUKNumber = 34,
        [Description("Sim Card Cell Number")]
        CellNumber = 35,
    }

    #endregion

    #region Data Update

    /// <summary>
    /// The <see cref="DataUpdateColumn"/> enumeration a list of 
    /// data update column options.
    /// </summary>
    public enum DataUpdateColumn
    {
        [Description("-- Please Select --")]
        None = 0,
        [Description("Employee Number")]
        EmployeeNumber = 1,
        [Description("Cost Code")]
        CostCode = 2,
        [Description("Client Name")]
        ClientName = 3,
        [Description("Client Land Line")]
        LandLine = 4,
        [Description("Client ID Number")]
        IDNumber = 5,
        [Description("Client EMail")]
        Email = 6,
        [Description("WBS Number")]
        WBSNumber = 7,
        [Description("IPAddress")]
        IPAddress = 8,
        [Description("Client Billable")]
        IsBillable = 9,
        [Description("Client Voice Allowance")]
        VoiceAllowance = 10,
        [Description("Supplier Limit")]
        SPLimit = 11,
        [Description("Stop Billing Period")]
        StopBilling = 12,
        [Description("Admin Fee")]
        AdminFee = 13,
        [Description("Company Billing Level")]
        CompanyBillingLevel = 14,
        [Description("Contract Payment Cancel Date")]
        PaymentCancelDate = 15,
        [Description("Package Name")]
        PackageName = 16,
        [Description("Package Cost")]
        Cost = 17,
        [Description("Package MB Data")]
        MBData = 18,
        [Description("Package Talk Time")]
        TalkTimeMinutes = 19,
        [Description("Package Rand Value")]
        RandValue = 20,
        [Description("Package SPUL Value")]
        SPULValue = 21
    }

    /// <summary>
    /// The <see cref="DataUpdateEntity"/> enumeration a list of 
    /// data import entities options.
    /// </summary>
    public enum DataUpdateEntity
    {
        [Description("-- Please Select --")]
        None = 0,
        [Description("Client Data")]
        Client = 1,
        [Description("Company Data")]
        Company = 2,
        [Description("Client Billing Data")]
        ClientBilling = 3,
        [Description("Billing Level Data")]
        CompanyBillingLevel = 4,
        [Description("Contract Data")]
        Contract = 5,
        [Description("Package Data")]
        Package = 6
    }

    #endregion

    /// <summary>
    /// The below extension class can be used for all Enum types defined herein.
    /// Please not that you'll have to define each extension method per Enum type
    /// to handle the correct return type.
    /// </summary>
    public static class EnumExtensions
    {
        public static short Value(this ActivityProcess type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this StatusLink type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this PackageType type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this CostType type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this SecurityRole type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this DataValidationGroupName type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this BillingExecutionState type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this DataUpdateColumn type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this DataUpdateEntity type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this DataValidationPropertyName type)
        {
            return Convert.ToInt16(type);
        }       
        public static short Value(this DataValidationProcess type)
        {
            return Convert.ToInt16(type);
        }
    }
}
