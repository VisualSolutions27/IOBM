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
    
    public partial class InvoiceDetail
    {
        public int pkInvoiceItemID { get; set; }
        public int fkInvoiceID { get; set; }
        public int fkServiceProviderID { get; set; }
        public string ItemDescription { get; set; }
        public string ReferenceNumber { get; set; }
        public decimal ItemAmount { get; set; }
        public bool IsPrivate { get; set; }
        public string ModifiedBy { get; set; }
        public System.DateTime ModifiedDate { get; set; }
    
        public virtual Invoice Invoice { get; set; }
        public virtual ServiceProvider ServiceProvider { get; set; }
    }
}
