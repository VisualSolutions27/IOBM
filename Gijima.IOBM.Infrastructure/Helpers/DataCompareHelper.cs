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
                string[] compareToValues = valueToCompareTo.Split(';');

                foreach (string value in compareToValues)
                {
                    switch (stringOperator)
                    {
                        case StringOperator.Contains:
                            if (valueToCompare.ToUpper().Trim().Contains(value.ToUpper().Trim()))
                                return true;
                            break;
                        case StringOperator.Equal:
                            if (string.Equals(valueToCompare.ToUpper().Trim(), value.ToUpper().Trim()))
                                return true;
                            break;
                       case StringOperator.Format:
                            return false;
                        case StringOperator.LenghtSmaller:
                            if (valueToCompare.Length < Convert.ToInt32(value))
                                return true;
                            break;
                        case StringOperator.LengthEqual:
                            if (valueToCompare.Length == Convert.ToInt32(value))
                                return true;
                            break;
                        case StringOperator.LengthGreater:
                            if (valueToCompare.Length > Convert.ToInt32(value))
                                return true;
                            break;
                        case StringOperator.PostFix:
                            if (valueToCompare.EndsWith(value))
                                return true;
                            break;
                        case StringOperator.PreFix:
                            if (valueToCompare.StartsWith(value))
                                return true;
                            break;
                        default:
                            return false;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
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
                string[] compareToValues = valueToCompareTo.Split(';');

                foreach (string value in compareToValues)
                {
                    int intToCompare = Convert.ToInt32(valueToCompare);
                    int intToCompareTo = Convert.ToInt32(value);

                    switch (numericOperator)
                    {
                        case NumericOperator.Equal:
                            if (intToCompare == intToCompareTo)
                                return true;
                            break;
                        case NumericOperator.Greater:
                            if (intToCompare > intToCompareTo)
                                return true;
                            break;
                        case NumericOperator.GreaterEqual:
                            if (intToCompare >= intToCompareTo)
                                return true;
                            break;
                        case NumericOperator.Smaller:
                            if (intToCompare < intToCompareTo)
                                return true;
                            break;
                        case NumericOperator.SmallerEqual:
                            if (intToCompare <= intToCompareTo)
                                return true;
                            break;
                        default:
                            return false;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
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
                string[] compareToValues = valueToCompareTo.Split(';');

                foreach (string value in compareToValues)
                {
                    decimal intToCompare = Convert.ToDecimal(valueToCompare);
                    decimal intToCompareTo = Convert.ToDecimal(value);

                    switch (decimalOperator)
                    {
                        case NumericOperator.Equal:
                            if (intToCompare == intToCompareTo)
                                return true;
                            break;
                        case NumericOperator.Greater:
                            if (intToCompare > intToCompareTo)
                                return true;
                            break;
                        case NumericOperator.GreaterEqual:
                            if (intToCompare >= intToCompareTo)
                                return true;
                            break;
                        case NumericOperator.Smaller:
                            if (intToCompare < intToCompareTo)
                                return true;
                            break;
                        case NumericOperator.SmallerEqual:
                            if (intToCompare <= intToCompareTo)
                                return true;
                            break;
                        default:
                            return false;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                _eventAggregator.GetEvent<ApplicationMessageEvent>().Publish(null);
                return false;
            }
        }
    }
}
