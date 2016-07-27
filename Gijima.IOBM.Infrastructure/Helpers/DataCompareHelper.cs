using Gijima.IOBM.Infrastructure.Events;
using Gijima.IOBM.Infrastructure.Structs;
using Prism.Events;
using System;

namespace Gijima.IOBM.Infrastructure.Helpers
{
    public class DataCompareHelper
    {
        #region Properties and Attributes

        private IEventAggregator _eventAggregator;

        #endregion

        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="eventAggreagator"></param>
        public DataCompareHelper(IEventAggregator eventAggreagator)
        {
            _eventAggregator = eventAggreagator;
        }

        /// <summary>
        /// Compare string values based on the specified operator
        /// </summary>
        /// <param name="stringOperator">The string operator to use in the validation.</param>
        /// <param name="valueToCompare">The value to compare.</param>
        /// <param name="valueToCompareTo">The value to compare to.</param>
        /// <returns>True if successfull</returns>
        public bool CompareStringValues(StringOperator stringOperator, string valueToCompare, string valueToCompareTo)
        {
            try
            {
                switch (stringOperator)
                {
                    case StringOperator.Contains:
                        return valueToCompare.ToUpper().Trim().Contains(valueToCompareTo.ToUpper().Trim());
                    case StringOperator.Equal:
                        return string.Equals(valueToCompare.ToUpper().Trim(), valueToCompareTo.ToUpper().Trim());
                    case StringOperator.Format:
                        return false;
                    case StringOperator.LenghtSmaller:
                        return valueToCompare.Length < Convert.ToInt32(valueToCompare);
                    case StringOperator.LengthEqual:
                        return valueToCompare.Length == Convert.ToInt32(valueToCompareTo);
                    case StringOperator.LengthGreater:
                        return valueToCompare.Length > Convert.ToInt32(valueToCompareTo);
                    case StringOperator.PostFix:
                        return valueToCompare.EndsWith(valueToCompareTo);
                    case StringOperator.PreFix:
                        return valueToCompare.StartsWith(valueToCompareTo);
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return false;
            }
        }

        /// <summary>
        /// Compare numeric values based on the specified operator
        /// </summary>
        /// <param name="numericOperator">The numeric operator to use in the validation.</param>
        /// <param name="valueToCompare">The value to compare.</param>
        /// <param name="valueToCompareTo">The value to compare to.</param>
        /// <returns>True if successfull</
        public bool CompareNumericValues(NumericOperator numericOperator, string valueToCompare, string valueToCompareTo)
        {
            try
            {
                int intToCompare = Convert.ToInt32(valueToCompare);
                int intToCompareTo = Convert.ToInt32(valueToCompareTo);

                switch (numericOperator)
                {
                    case NumericOperator.Equal:
                        return intToCompare == intToCompareTo;
                    case NumericOperator.Greater:
                        return intToCompare > intToCompareTo;
                    case NumericOperator.GreaterEqual:
                        return intToCompare >= intToCompareTo;
                    case NumericOperator.Smaller:
                        return intToCompare < intToCompareTo;
                    case NumericOperator.SmallerEqual:
                        return intToCompare <= intToCompareTo;
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return false;
            }
        }

        /// <summary>
        /// Compare decimal values based on the specified operator
        /// </summary>
        /// <param name="decimalOperator">The decimal operator to use in the validation.</param>
        /// <param name="valueToCompare">The value to compare.</param>
        /// <param name="valueToCompareTo">The value to compare to.</param>
        /// <returns>True if successfull</returns>
        public bool CompareDecimalValues(NumericOperator decimalOperator, string valueToCompare, string valueToCompareTo)
        {
            try
            {
                decimal intToCompare = Convert.ToDecimal(valueToCompare);
                decimal intToCompareTo = Convert.ToDecimal(valueToCompareTo);

                switch (decimalOperator)
                {
                    case NumericOperator.Equal:
                        return intToCompare == intToCompareTo;
                    case NumericOperator.Greater:
                        return intToCompare > intToCompareTo;
                    case NumericOperator.GreaterEqual:
                        return intToCompare >= intToCompareTo;
                    case NumericOperator.Smaller:
                        return intToCompare < intToCompareTo;
                    case NumericOperator.SmallerEqual:
                        return intToCompare <= intToCompareTo;
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<MessageEvent>().Publish(ex);
                return false;
            }
        }
    }
}
