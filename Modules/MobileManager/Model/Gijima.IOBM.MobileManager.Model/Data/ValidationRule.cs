//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    
    public partial class ValidationRule
    {
        public int pkValidationRuleID { get; set; }
        public short enValidationRuleGroup { get; set; }
        public int fkValidationRulesDataID { get; set; }
        public int EntityID { get; set; }
        public string ValidationDataValue { get; set; }
        public short enStringCompareType { get; set; }
        public short enNumericCompareType { get; set; }
        public short enDateCompareType { get; set; }
        public string ModifiedBy { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    
        public virtual ValidationRulesData ValidationRulesData { get; set; }
    }
}
