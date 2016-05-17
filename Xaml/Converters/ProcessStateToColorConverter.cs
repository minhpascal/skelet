namespace Robot
{
	using System;
	using System.Windows.Data;
	using System.Windows.Media;

	using StockSharp.Algo;

	class ProcessStateToColorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			var state = (ProcessStates)value;

			var color = state == ProcessStates.Started ? Colors.LightGreen : Colors.Transparent;

			return new SolidColorBrush(color);
		}

		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
