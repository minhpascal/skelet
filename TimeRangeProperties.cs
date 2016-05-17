namespace Robot
{
	using System;
	using System.ComponentModel;
	using System.Xml;
	using System.Xml.Serialization;

	using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

	[DisplayName(@"Интервал")]
	public class TimeRangeProperties : INotifyPropertyChanged
	{
		private TimeSpan _from;
		private TimeSpan _to;

		[DisplayName(@"Начало")]
		[Description(@"Время начала временного интервала")]
		[PropertyOrder(0)]
		[XmlIgnore]
		public TimeSpan From
		{
			get { return _from; }
			set
			{
				_from = value;
				OnPropertyChanged("From");
			}
		}

		[DisplayName(@"Конец")]
		[Description(@"Время окончания временного интервала")]
		[PropertyOrder(1)]
		[XmlIgnore]
		public TimeSpan To
		{
			get { return _to; }
			set
			{
				_to = value;
				OnPropertyChanged("To");
			}
		}

		[Browsable(false)]
		[XmlElement(DataType = "duration", ElementName = "From")]
		public string FromXmlString
		{
			get
			{
				return XmlConvert.ToString(From);
			}
			set
			{
				From = string.IsNullOrEmpty(value) ? TimeSpan.Zero : XmlConvert.ToTimeSpan(value);
			}
		}

		[Browsable(false)]
		[XmlElement(DataType = "duration", ElementName = "To")]
		public string ToXmlString
		{
			get
			{
				return XmlConvert.ToString(To);
			}
			set
			{
				To = string.IsNullOrEmpty(value) ? TimeSpan.Zero : XmlConvert.ToTimeSpan(value);
			}
		}

		[Browsable(false)]
		public static string DisplayName
		{
			get { return @"Интервал"; }
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}
	}
}
