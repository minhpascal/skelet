namespace Robot
{
	using System.ComponentModel;
	using System.Xml.Serialization;

	using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

	/// <summary>
	/// Параметры стратегии.
	/// </summary>
	[XmlInclude(typeof(OrderType))]
	[XmlInclude(typeof(TimeRangeProperties))]
	public class RobotStrategyProperties : BaseShellStrategyProperties
	{
	 
		private int _spread = 3;
		private int _offset = 3;
        private int _stop = 360;
		private OrderType _orderType;

		private int _quotingPriceOffset;
		private int _quotingTimeout = 60;
		private long _trailingOrderId;


		[DisplayName(@"Спред")]
		[Description(@"Минимальный размер спреда в шагах цены для выставления ордеров")]
		[Category(@"Параметры")]
		//[CategoryOrder(1)]
		[PropertyOrder(0)]
		public int Spread
		{
			get { return _spread; }
			set
			{
				_spread = value;
				OnPropertyChanged("Spread");
			}
		}

		[DisplayName(@"Отступ")]
		[Description(@"Отступ в шагах цены от края стакана для ордеров. > 0 - вдаль от спреда, < 0 - вгубь спреда")]
		[Category(@"Параметры")]
		//[CategoryOrder(1)]
		[PropertyOrder(0)]
		public int Offset
		{
			get { return _offset; }
			set
			{
				_offset = value;
				OnPropertyChanged("Offset");
			}
		}

        [DisplayName(@"Стоп (сек)")]
        [Description(@"Стоп в секундах сколько держать открытой позицию")]
        [Category(@"Параметры")]
        //[CategoryOrder(1)]
        [PropertyOrder(0)]
        public int Stop
        {
            get { return _stop; }
            set
            {
                _stop = value;
                OnPropertyChanged("Stop");
            }
        }

			   
		[DisplayName(@"Стоп-Ордер")]
		[Description(@"Тип ордеров закрытия позиций по стопу")]
		[Category(@"Параметры")]
		//[CategoryOrder(1)]
		[PropertyOrder(1)]
		[ItemsSource(typeof(OrderTypeItemsSource))]
		public OrderType OrderType
		{
			get { return _orderType; }
			set
			{
				_orderType = value;
				OnPropertyChanged("OrderType");
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

		[DisplayName("Сдвиг (шаг. цены)")]
		[Description(@"Сдвиг в шагах цены от края спреда в шагах цены. Для покупки при значении > 0 сдвигает внутрь спреда, при < 0 вдаль от спреда.")]
		[Category(@"Котирование")]
		//[CategoryOrder(4)]
		[PropertyOrder(0)]
		public int QuotingPriceOffset
		{
			get { return _quotingPriceOffset; }
			set
			{
				_quotingPriceOffset = value;
				OnPropertyChanged("QuotingPriceOffset");
			}
		}

		[DisplayName("Таймаут (сек)")]
		[Description(@"Интервал в секундах, не чаще которого можно переставлять заявки.")]
		[Category(@"Котирование")]
		//[CategoryOrder(4)]
		[PropertyOrder(0)]
		public int QuotingTimeout
		{
			get { return _quotingTimeout; }
			set
			{
				_quotingTimeout = value;
				OnPropertyChanged("QuotingTimeout");
			}
		}

		 

		 

		[Browsable(false)]
		public long TrailingOrderId
		{
			get { return _trailingOrderId; }
			set
			{
				_trailingOrderId = value;
				OnPropertyChanged("TrailingOrderId");
			}
		}
	}
}
