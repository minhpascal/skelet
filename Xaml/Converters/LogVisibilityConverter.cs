using System;
using StockSharp.Logging;
using System.Globalization;
using System.Windows.Data;

namespace Robot
{
    class LogVisibilityConverter : IMultiValueConverter
	{
		#region Implementation of IValueConverter

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
		    var msg = (LogMessage)values[0];
		    var isErrorChecked = (bool) values[1];
            var isWarnChecked = (bool)values[2];
            var isInfoChecked = (bool)values[3];

		    switch (msg.Level)
		    {
                case LogLevels.Error:
		            return isErrorChecked;
                case LogLevels.Warning:
		            return isWarnChecked;
                case LogLevels.Info:
		            return isInfoChecked;
                default:
		            return true;
		    }
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
