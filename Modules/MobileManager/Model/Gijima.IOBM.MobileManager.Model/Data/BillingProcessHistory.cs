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
    
    public partial class BillingProcessHistory
    {
        public int pkBillingProcessHistoryID { get; set; }
        public int fkBillingProcessID { get; set; }
        public string BillingPeriod { get; set; }
        public System.DateTime ProcessStartDate { get; set; }
        public Nullable<System.DateTime> ProcessEndDate { get; set; }
        public bool ProcessCurrent { get; set; }
        public Nullable<bool> ProcessResult { get; set; }
        public Nullable<double> ProcessDuration { get; set; }
        public byte[] BillingComment { get; set; }
        public string ModifiedBy { get; set; }
        public System.DateTime DateModified { get; set; }
    
        public virtual BillingProcess BillingProcess { get; set; }
    }
}
