using System;

namespace Gijima.IOBM.Infrastructure.Structs
{
    /// <summary>
    /// The <see cref="SolutionApplication"/> enumeration lists of
    /// applications in the IOBM solution.
    /// </summary>
    public enum SolutionApplication
    {
        MobileManager = 1
    }

    /// <summary>
    /// The <see cref="IOBMSecurityRole"/> enumeration lists of
    /// application security roles.
    /// </summary>
    public enum IOBMSecurityRole
    {
        Administrator = 1,
        Supervisor = 2,
        User = 3
    }

    /// <summary>
    /// The <see cref="StringOperator"/> enumeration a list of 
    /// string operator options to validate on.
    /// </summary>
    public enum StringOperator
    {
        None = 0,
        PreFix = 1,
        PostFix,
        Contains,
        Equal,
        Format,
        LengthEqual,
        LengthGreater,
        LenghtSmaller
    }

    /// <summary>
    /// The <see cref="NumericOperator"/> enumeration a list of 
    /// numeric operator options to validate on.
    /// </summary>
    public enum NumericOperator
    {
        None = 0,
        Equal = 1,
        Greater,
        Smaller,
        GreaterEqual,
        SmallerEqual
    }

    /// <summary>
    /// The <see cref="DateCompareValue"/> enumeration a list of 
    /// date comapre values to validate on.
    /// </summary>
    public enum DateCompareValue
    {
        None = 0,
        Current = 1,
        Min,
        Max,
        MonthStart,
        MonthEnd
    }

    /// <summary>
    /// The below extension class can be used for all Enum types defined herein.
    /// Please not that you'll have to define each extension method per Enum type
    /// to handle the correct return type.
    /// </summary>
    public static class EnumExtensions
    {
        public static short Value(this SolutionApplication type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this IOBMSecurityRole type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this StringOperator type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this NumericOperator type)
        {
            return Convert.ToInt16(type);
        }
        public static short Value(this DateCompareValue type)
        {
            return Convert.ToInt16(type);
        }
    }
}
