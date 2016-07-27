using System;

namespace Gijima.IOBM.MobileManager.Common.Structs
{
    public class DataValidationResultInfo
    {
        public enum ResultType
        {
            Passed,
            Failed
        }
        public ResultType Result { get; set; }
        public object ValidationRule { get; set; }
        public Type EntityType { get; set; }
        public int EntityID { get; set; }
        public string PropertyName { get; set; }
        public string Message { get; set; }
    }
}
