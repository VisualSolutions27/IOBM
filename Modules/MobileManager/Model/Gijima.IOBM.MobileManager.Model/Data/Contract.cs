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
    
    public partial class Contract
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Contract()
        {
            this.Clients = new HashSet<Client>();
            this.Devices = new HashSet<Device>();
            this.SimCards = new HashSet<SimCard>();
        }
    
        public int pkContractID { get; set; }
        public Nullable<short> enCostType { get; set; }
        public string CellNumber { get; set; }
        public int fkStatusID { get; set; }
        public int fkPackageID { get; set; }
        public Nullable<int> fkPackageSetupID { get; set; }
        public string AccountNumber { get; set; }
        public Nullable<System.DateTime> ContractStartDate { get; set; }
        public Nullable<System.DateTime> ContractUpgradeDate { get; set; }
        public Nullable<System.DateTime> ContractEndDate { get; set; }
        public Nullable<System.DateTime> PaymentCancelDate { get; set; }
        public string ModifiedBy { get; set; }
        public System.DateTime ModifiedDate { get; set; }
        public bool IsActive { get; set; }
    
        public virtual Package Package { get; set; }
        public virtual PackageSetup PackageSetup { get; set; }
        public virtual Status Status { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Client> Clients { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Device> Devices { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<SimCard> SimCards { get; set; }
    }
}
