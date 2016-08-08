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
        ReadContractSimmCards
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
        Simm = 3,
        ContractDevice = 4,
        ContractSimm = 5,
        DeviceSimm = 6
    }

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

    /// <summary>
    /// The <see cref="SearchEntity"/> enumeration lists of entities
    /// the user can search on.
    /// </summary>
    public enum SearchEntity
    {
        EmployeeNumber,
        CellNumber,
        IDNumber,
        Email,
        Other
    }

    /// <summary>
    /// The <see cref="ActivityProcess"/> enumeration a list of administartion
    /// options that activity logs can be filtered on.
    /// </summary>
    public enum ActivityProcess
    {
        Administration,
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
        SimmCard,
        PackageSetup
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
    /// The <see cref="ValidationRuleGroup"/> enumeration a list of 
    /// data entity options to validate on.
    /// </summary>
    public enum ValidationRuleGroup
    {
        None = 0,
        Client = 1,
        Company = 2,
        Package = 3,
        Device = 4,
        SimmCard = 5,
        Status = 6
    }

    /// <summary>
    /// The <see cref="ValidationDataType"/> enumeration a list of 
    /// data types to validate on.
    /// </summary>
    public enum ValidationDataType
    {
        None = 0,
        String = 1,
        Integer = 2,
        Decimal = 3,
        Date = 4,
        Bool = 5
    }

    /// <summary>
    /// The <see cref="DataValidationEntity"/> enumeration a list of 
    /// data entity options to validate on.
    /// </summary>
    public enum DataValidationEntity
    {
        None = 0,
        Client = 1,
        Company = 2
    }

    /// <summary>
    /// The <see cref="DataImportColumns"/> enumeration a list of 
    /// data import column options.
    /// </summary>
    public enum DataImportColumns
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
        [Description("Company Name")]
        CompanyName = 13,
        [Description("Admin Fee")]
        AdminFee = 14,
        [Description("Company Billing Level")]
        CompanyBillingLevel = 15,
        [Description("Contract Payment Cancel Date")]
        PaymentCancelDate = 16,
        [Description("Package Name")]
        PackageName = 17,
        [Description("Package Cost")]
        Cost = 18,
        [Description("Package MB Data")]
        MBData = 19,
        [Description("Package Talk Time")]
        TalkTimeMinutes = 20,
        [Description("Package Rand Value")]
        RandValue = 21,
        [Description("Package SPUL Value")]
        SPULValue = 22
    }

    /// <summary>
    /// The <see cref="DataImportEntities"/> enumeration a list of 
    /// data import entities options.
    /// </summary>
    public enum DataImportEntities
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
        [Description("Cantract Data")]
        Contract = 5,
        [Description("Package Data")]
        Package = 6
    }

    /// <summary>
    /// The below extension class can be used for all Enum types defined herein.
    /// Please not that you'll have to define each extension method per Enum type
    /// to handle the correct return type.
    /// </summary>
    public static class EnumExtensions
    {
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
        public static short Value(this ValidationRuleGroup type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this ValidationDataType type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this BillingExecutionState type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this DataValidationEntity type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this DataImportColumns type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this DataImportEntities type)
        {
            return Convert.ToInt16(type);
        }
    }
}
