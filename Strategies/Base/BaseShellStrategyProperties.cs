using Ecng.Serialization;
using StockSharp.BusinessEntities;

namespace Robot
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Xml.Serialization;

	using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

	/// <summary>
	/// Параметры стратегии.
	/// </summary>
	[XmlInclude(typeof(OrderType))]
	[XmlInclude(typeof(TimeRangeProperties))]
	public class BaseShellStrategyProperties : INotifyPropertyChanged
	{
		private string _name = "Новая стратегия";
        private decimal _volume = 1;
	    private bool _restorePositionsOnStart = true;
	 
		private bool _schedulerIsEnabled;
		private List<TimeRangeProperties> _schedule = new List<TimeRangeProperties>(10);

        private SettingsStorage _ordersByTransactionId = new SettingsStorage();
        private SettingsStorage _securitiesByTransactionId = new SettingsStorage();
        private SettingsStorage _portfoliosByTransactionId = new SettingsStorage();
        private SettingsStorage _tradesByTransactionId = new SettingsStorage();
        private SettingsStorage _exchangesByTransactionId = new SettingsStorage();


		[DisplayName(@"Название")]
		[Description(@"Название стратегии")]
		[Category(@"Основные")]
		//[CategoryOrder(0)]
		[PropertyOrder(0)]
		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				OnPropertyChanged("Name");
			}
		}

		private Security _security = new Security { Id = "SIU2@RTS" };

		[DisplayName(@"Инструмент")]
		[Description(@"Код торгового инструмента")]
		[Category(@"Основные")]
		//[CategoryOrder(0)]
		[PropertyOrder(1)]
		//[Editor(typeof(SecurityIdEditor), typeof(SecurityIdEditor))]
		public Security Security
		{
			get { return _security; }
			set
			{
				_security = value;
				OnPropertyChanged("Security");
			}
		}

		private Portfolio _portfolio = new Portfolio { Name = "SPBFUT00622" };

		[DisplayName(@"Портфель")]
		[Description(@"Имя портфеля")]
		[Category(@"Основные")]
		//[CategoryOrder(0)]
		[PropertyOrder(3)]
		//[Editor(typeof(PortfolioNameEditor), typeof(PortfolioNameEditor))]
		public Portfolio Portfolio
		{
			get { return _portfolio; }
			set
			{
				_portfolio = value;
				OnPropertyChanged("Portfolio");
			}
		}

        [DisplayName(@"Объем")]
        [Description(@"Объем для открытия позиций")]
        [Category(@"Основные")]
        //[CategoryOrder(0)]
        [PropertyOrder(4)]
        public decimal Volume
        {
            get { return _volume; }
            set
            {
                if (value < 0)
                {
                    return;
                }

                _volume = value;
                OnPropertyChanged("Volume");
            }
        }

        [DisplayName(@"Восстанавливать позиции")]
        [Description(@"Восстанавливать позиции при старте стратегии")]
        [Category(@"Основные")]
        //[CategoryOrder(0)]
        [PropertyOrder(4)]
       public bool RestorePositionsOnStart
		{
			get { return _restorePositionsOnStart; }
			set
			{
				_restorePositionsOnStart = value;
				OnPropertyChanged("Portfolio");
			}
		}
 

       

	 

//		[DisplayName(@"Тайм-Фрейм")]
//		[Description(@"Временной интервал для начального открытия позиции.")]
//		[Category(@"Параметры")]
//		[CategoryOrder(1)]
//		[PropertyOrder(2)]
//		[ItemsSource(typeof(TimeFramesItemsSource))]
//		[XmlIgnore]
//		public TimeFrames TimeFrame
//		{
//			get { return _timeFrame; }
//			set
//			{
//				_timeFrame = value;
//				OnPropertyChanged("TimeFrame");
//			}
//		}

//		[Browsable(false)]
//		public string TimeFrameValue
//		{
//			get { return ((TimeSpan)_timeFrame).ToString(); }
//			set { TimeFrame = TimeSpan.Parse(value); }
//		}

		 
 

		[Category(@"Планировщик")]
		//[CategoryOrder(5)]
		[DisplayName(@"Включен")]
		[Description(@"Включен ли запуск по расписанию для данной стратегии.")]
		[PropertyOrder(0)]
		public bool SchedulerIsEnabled
		{
			get { return _schedulerIsEnabled; }
			set
			{
				_schedulerIsEnabled = value;
				OnPropertyChanged("SchedulerIsEnabled");
			}
		}

		[Category(@"Планировщик")]
		//[CategoryOrder(5)]
		[DisplayName(@"Время работы")]
		[Description(@"Настройка времени автоматической работы стратегии.")]
		[PropertyOrder(1)]
		[Editor(typeof(ScheduleEditor), typeof(ScheduleEditor))]
		public List<TimeRangeProperties> Schedule
		{
			get { return _schedule; }
			set
			{
				_schedule = value;
				OnPropertyChanged("Schedule");
			}
		}

		[Browsable(false)]
        public SettingsStorage OrdersByTransactionId
        {
            get { return _ordersByTransactionId; }
            set
            {
                _ordersByTransactionId = value;
                OnPropertyChanged("OrdersByTransactionId");
            }
        }

		[Browsable(false)]
        public SettingsStorage SecuritiesByTransactionId
        {
            get { return _securitiesByTransactionId; }
            set
            {
                _securitiesByTransactionId = value;
                OnPropertyChanged("SecuritiesByTransactionId");
            }
        }

		[Browsable(false)]
        public SettingsStorage TradesByTransactionId
        {
            get { return _tradesByTransactionId; }
            set
            {
                _tradesByTransactionId = value;
                OnPropertyChanged("TradesByTransactionId");
            }
        }

		[Browsable(false)]
        public SettingsStorage PortfoliosByTransactionId
        {
            get { return _portfoliosByTransactionId; }
            set
            {
                _portfoliosByTransactionId = value;
                OnPropertyChanged("PortfoliosByTransactionId");
            }
        }

		[Browsable(false)]
        public SettingsStorage ExchangesByTransactionId
        {
            get { return _exchangesByTransactionId; }
            set
            {
                _exchangesByTransactionId = value;
                OnPropertyChanged("ExchangesByTransactionId");
            }
        }

	 

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged(string name)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}
	}
}
