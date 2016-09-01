namespace Gijima.IOBM.Infrastructure.Structs
{
    public static class MobileManagerEnvironment
    {
        #region Properties & Attributes

        /// <summary>
        /// The selected client ID.
        /// </summary>
        public static int SelectedClientID { get; set; }

        /// <summary>
        /// The selected contract ID.
        /// </summary>
        public static int SelectedContractID { get; set; }

        /// <summary>
        /// The selected client company ID.
        /// </summary>
        public static int ClientCompanyID { get; set; }

        /// <summary>
        /// The current billing period.
        /// </summary>
        public static string BillingPeriod { get; set; }

        /// <summary>
        /// Indicate if the billing period is open.
        /// </summary>
        public static bool IsBillingPeriodOpen { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Constructor
        /// </summary>
        static MobileManagerEnvironment()
        {
        }

        #endregion
    }
}
