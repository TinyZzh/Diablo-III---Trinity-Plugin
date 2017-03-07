using System;
using System.Windows.Data;

namespace QuestTools.UI
{
    public class PercentConverter : IValueConverter
    {
        #region IValueConverter Members
        /// <summary>Converts a value.</summary>
        /// <param name="value">The value produced by the binding source.</param>
        /// <param name="targetType">The type of the binding target property.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is float)
            {
                return (float)Math.Round((float)value * 100,0);
            }
            if (value is double)
            {
                return Math.Round((double)value * 100,0);
            }
            return 0f;
        }

        /// <summary>Converts a value.</summary>
        /// <param name="value">The value that is produced by the binding target.</param>
        /// <param name="targetType">The type to convert to.</param>
        /// <param name="parameter">The converter parameter to use.</param>
        /// <param name="culture">The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            float pf;
            double pd;

            if (value is float)
            {
                return (float)Math.Round((float)value / 100, 2);
            }
            if (value is double)
            {
                return Math.Round((double)value / 100, 2);
            }
            if (float.TryParse(value.ToString(), out pf))
            {
                return (float)Math.Round(pf / 100, 2);
            }
            if (double.TryParse(value.ToString(), out pd))
            {
                return Math.Round(pd / 100, 2);
            }
            return 0f;
        }

        #endregion
    }
}
