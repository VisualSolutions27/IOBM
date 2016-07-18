using System;

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
    public enum BillingProcess
    {
        DataValidation = 1,
        DataImport
    }

    /// <summary>
    /// The <see cref="ValidationRuleGroup"/> enumeration a list of 
    /// data entity options to validate on.
    /// </summary>
    public enum ValidationRuleGroup
    {
        None = 0,
        Client = 1,
        Package,
        Company,
        Status
    }

    /// <summary>
    /// The <see cref="ValidationDataType"/> enumeration a list of 
    /// data types to validate on.
    /// </summary>
    public enum ValidationDataType
    {
        None = 0,
        String = 1,
        Integer,
        Decimal,
        Date,
        Bool
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
    }
}
