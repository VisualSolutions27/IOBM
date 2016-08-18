﻿using Gijima.IOBM.Infrastructure.Helpers;
using Gijima.IOBM.MobileManager.Common.Structs;

namespace Gijima.IOBM.MobileManager.Model.Data
{
    public partial class DataValidationProperty
    {
        /// <summary>
        /// Returns the property description from the DataValidationPropertyName enum.
        /// </summary>
        /// <returns></returns>
        public string PropertyDescription
        {
            get { return EnumHelper.GetDescriptionFromEnum((DataValidationPropertyName)enDataValidationProperty).ToString(); }
        }
    }
}
