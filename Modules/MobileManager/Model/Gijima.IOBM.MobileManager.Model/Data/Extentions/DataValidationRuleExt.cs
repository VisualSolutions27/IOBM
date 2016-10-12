using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.Infrastructure.Structs;
using Gijima.IOBM.MobileManager.Common.Structs;

namespace Gijima.IOBM.MobileManager.Model.Data
{
    public partial class DataValidationRule
    {
        /// <summary>
        /// Returns a string representing the data description.
        /// </summary>
        /// <returns>Data Description</returns>
        public string DataDescription { get; set; }

        /// <summary>
        /// Returns the entity description from the DataValidationPropertyName enum.
        /// </summary>
        /// <returns></returns>
        public string EntityDescription { get; set; }

        /// <summary>
        /// Returns the property name from the DataValidationPropertyName enum.
        /// </summary>
        /// <returns></returns>
        public string PropertyName { get; set; }

        /// <summary>
        /// Returns the property description from the DataValidationPropertyName enum.
        /// </summary>
        /// <returns></returns>
        public string PropertyDescription { get; set; }

        /// <summary>
        /// Returns the package description from the packge entity.
        /// </summary>
        /// <returns></returns>
        public string PackageDescription { get; set; }

        /// <summary>
        /// Returns the data type description from the relevant DataTypeName enum.
        /// </summary>
        /// <returns></returns>
        public string DataTypeDescription { get; set; }

        /// <summary>
        /// Returns the operator description from the relevant operator enum.
        /// </summary>
        /// <returns></returns>
        public string OperatorDescription { get; set; }
    }
}
