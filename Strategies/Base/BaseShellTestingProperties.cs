using System;
using System.ComponentModel;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Robot
{
	using Ecng.Xaml;

	public class BaseShellTestingProperties : BaseShellStrategyProperties
    {
        private DateTime _startDate = new DateTime(2012, 07, 05, 10, 00, 00);
        private DateTime _stopDate = new DateTime(2012, 07, 05, 23, 50, 00);
        private string _path = @"..\..\..\HistoryQuotes\";
        private decimal _minStepSize = 1m;
		private decimal _minStepPrice = 1m;
		private decimal _portfolioAmount = 100000m;
	    private decimal _margin = 1100m;

        [DisplayName(@"Шаг цены")]
        [Description(@"Шаг цены тестируемого инструмента")]
        [Category(@"Основные")]
        //[CategoryOrder(0)]
        [PropertyOrder(2)]
        public decimal MinStepSize
        {
            get { return _minStepSize; }
            set
            {
                _minStepSize = value;
                OnPropertyChanged("MinStepSize");
            }
        }

		[DisplayName(@"Ст. шага цены")]
		[Description(@"Стоимость шага цены тестируемого инструмента")]
		[Category(@"Основные")]
		//[CategoryOrder(0)]
		[PropertyOrder(3)]
		public decimal MinStepPrice
		{
			get { return _minStepPrice; }
			set
			{
				_minStepPrice = value;
				OnPropertyChanged("MinStepPrice");
			}
		}

		[DisplayName(@"Депозит")]
		[Description(@"Размер депозита для тестирования")]
		[Category(@"Основные")]
		//[CategoryOrder(0)]
		[PropertyOrder(5)]
		public decimal PortfolioAmount
		{
			get { return _portfolioAmount; }
			set
			{
				_portfolioAmount = value;
				OnPropertyChanged("PortfolioAmount");
			}
		}

		[DisplayName(@"ГО")]
		[Description(@"Гарантийное обеспечения на один контракт для тестирования")]
		[Category(@"Основные")]
		//[CategoryOrder(0)]
		[PropertyOrder(3)]
		public decimal Margin
		{
			get { return _margin; }
			set
			{
				_margin = value;
				OnPropertyChanged("Margin");
			}
		}


        [DisplayName(@"Начало")]
        [Description(@"Дата начала исторических данных")]
        [Category(@"Тестирование")]
        //[CategoryOrder(7)]
        [PropertyOrder(0)]
        public DateTime StartDate
        {
            get { return _startDate; }
            set
            {
                _startDate = value;
                OnPropertyChanged("StartDate");
            }
        }

        [DisplayName(@"Конец")]
        [Description(@"Дата окончания исторических данных")]
        [Category(@"Тестирование")]
        //[CategoryOrder(7)]
        [PropertyOrder(1)]
        public DateTime StopDate
        {
            get { return _stopDate; }
            set
            {
                _stopDate = value;
                OnPropertyChanged("StopDate");
            }
        }

        [DisplayName(@"Путь")]
        [Description(@"Путь к историческим данным")]
        [Category(@"Тестирование")]
        //[CategoryOrder(7)]
        [PropertyOrder(2)]
		[Editor(typeof(FolderBrowserEditor), typeof(FolderBrowserEditor))]
        public string Path
        {
            get { return _path; }
            set
            {
                _path = value;
                OnPropertyChanged("Path");
            }
        }
    }
}
