namespace Robot
{
	using System.ComponentModel;
	using System.Threading;
	using System.Windows.Input;
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;
	using System.Windows;
	using System.Windows.Media;
	using System.Diagnostics;
	using System.IO;

	using Ecng.Collections;
	using Ecng.Common;
	using Ecng.Xaml;

	using Xceed.Wpf.AvalonDock.Layout;
	using Robot.Strategies;

	using StockSharp.Algo.Strategies.Reporting;
	using StockSharp.Algo;
	using StockSharp.Algo.Storages;
	using StockSharp.Algo.Testing;
	using StockSharp.BusinessEntities;
	using StockSharp.Logging;
	using StockSharp.Xaml;
    using StockSharp.Xaml.Charting;

    public partial class MainWindow : ILogReceiver
	{
		public static RoutedCommand ConnectCommand = new RoutedCommand();
		public static RoutedCommand DisconnectCommand = new RoutedCommand();
		public static RoutedCommand AddStrategyCommand = new RoutedCommand();
		public static RoutedCommand RemoveStrategyCommand = new RoutedCommand();
		public static RoutedCommand StartStrategyCommand = new RoutedCommand();
		public static RoutedCommand StartAllStrategiesCommand = new RoutedCommand();
		public static RoutedCommand StopStrategyCommand = new RoutedCommand();
		public static RoutedCommand StopStrategyToCashCommand = new RoutedCommand();
		public static RoutedCommand ExitCommand = new RoutedCommand();
		public static RoutedCommand SettingsCommand = new RoutedCommand();
        public static RoutedCommand AddTestingStrategyCommand = new RoutedCommand();
        public static RoutedCommand RemoveTestingStrategyCommand = new RoutedCommand();
        public static RoutedCommand StartTestingStrategyCommand = new RoutedCommand();
        public static RoutedCommand StopTestingStrategyCommand = new RoutedCommand();
        public static RoutedCommand TestingReportCommand = new RoutedCommand();

        public static RoutedCommand AddHustleStrategyCommand = new RoutedCommand();

		private readonly LogManager _logManager = new LogManager();
		private decimal _newStrategyCount = 1;
        private decimal _newTestingCount = 1;
		private Timer _timer;
		private HistoryEmulationConnector _emulationTrader;

        private readonly Dictionary<BaseShellStrategy, LayoutDocument> _documents = new Dictionary<BaseShellStrategy, LayoutDocument>();
        private readonly Dictionary<BaseShellStrategy, LayoutDocument> _testingDocuments = new Dictionary<BaseShellStrategy, LayoutDocument>();

        private readonly List<HustleEveryDayStrategy> _hustleEveryDayStrategies = new List<HustleEveryDayStrategy>();
        private readonly List<HustleEveryDayStrategy> _hustleEveryDayTestingStrategies = new List<HustleEveryDayStrategy>();
	
        public static MainWindow Instance { set; get; }

	    public List<BaseShellStrategy> Strategies
	    {
		    get { return _documents.Keys.ToList(); }
	    }

	    public MainWindow()
	    {
		    Instance = this;

			DispatcherPropertyChangedEventManager.Init();

			InitializeComponent();
			InitializeLogging();
			InitializeConnectionEngine();
			InitializeConfiguration();

			SettingsEngine.Instance.Properties.PropertyChanged += (s, args) =>
			{
				if (args.PropertyName == "ConnectionType" || args.PropertyName == "Emulation")
				{
					UpdateConnectionText();
				}
			};

			ConnectionEngine.Instance.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "Trader")
				{
					if(ConnectionEngine.Instance.Trader != null)
					{
						_timer = ThreadingHelper.Timer(OnTimeChanged).Interval(TimeSpan.FromSeconds(30));	
					}
					else
					{
						_timer.Dispose();
						_timer = null;	
					}
				}
			};

			UpdateConnectionText();
	    }

		private void OnTimeChanged()
		{
			var time = ConnectionEngine.Instance.Trader.CurrentTime.TimeOfDay;

			foreach (var strategy in _documents.Keys.ToList().Where(s => s.Params.SchedulerIsEnabled))
			{
				var isContain = strategy.Params.Schedule.Any(property => time > property.From && time < property.To);

				switch (strategy.ProcessState)
				{
					case ProcessStates.Stopped:
						if (isContain)
							StartStrategy(strategy);
						break;
					case ProcessStates.Started:
						if (!isContain)
							StopStrategy(strategy, true);
						break;
				}
			}
		}

		private void InitializeConnectionEngine()
		{
			ConnectionEngine.Instance.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "State")
				{
					string state;

					switch (ConnectionEngine.Instance.State)
					{
						case ConnectionEngine.States.Disconnected:
							state = "Отключен";
							break;
						case ConnectionEngine.States.Connecting:
							state = "Подключаюсь";
							break;
						case ConnectionEngine.States.Connected:
							state = "Подключен";
							break;
						case ConnectionEngine.States.Disconnecting:
							state = "Отключаюсь";
							break;
						default:
							state = "Неизвестно";
							break;
					}

					this.GuiAsync(() => TextBlockConnectionState.Text = state);
				}
			};

			ConnectionEngine.Instance.PropertyChanged += (sender, args) =>
			{
				var trader = ConnectionEngine.Instance.Trader;

				if (args.PropertyName == "Trader")
				{
					foreach (var document in _documents)
					{
						document.Key.Connector = trader;
					}

					if (trader != null && !SettingsEngine.Instance.Properties.Emulation)
					{
						_logManager.Sources.Add(trader);
					}
				}
			};

		}

		private void InitializeLogging()
		{
			_logManager.Listeners.Add(new Log4NetLogListener("log4net.xml"));
			_logManager.Listeners.Add(new FileLogListener("strategy_log.txt"));
			_logManager.Listeners.Add(new DebugLogListener());
			_logManager.Listeners.Add(new GuiLogListener(LogControl));
			_logManager.Sources.Add(new UnhandledExceptionSource());

			_logManager.Sources.Add(this);
			_logManager.Sources.Add(SettingsEngine.Instance);
			_logManager.Sources.Add(ConnectionEngine.Instance);
		}

		private void InitializeConfiguration()
		{
			try
			{
				var strategies = SettingsEngine.Instance.LoadStrategies();

				foreach (var properties in strategies)
				{
					AddStrategy(properties);
				}

				if (!strategies.Any())
				{
					AddStrategy();
				}
			}
			catch (Exception ex)
			{
				ex.LogError();
			}

			try
			{
				var testingStrategies = SettingsEngine.Instance.LoadTestingStrategies();

				foreach (var properties in testingStrategies)
				{
					AddTestingStrategy(properties);
				}
			}
			catch (Exception ex)
			{
				ex.LogError();
			}

			try
			{
				var hustleProperties = SettingsEngine.Instance.LoadHustleEveryDayStrategiesProperties();

				foreach (var property in hustleProperties)
				{
					AddHustleEveryDayStrategy(property);
				}
			}
			catch (Exception ex)
			{
				ex.LogError();
			}

			try
			{
				var testingHustleProperties = SettingsEngine.Instance.LoadTestingHustleEveryDayStrategyProperties();

				foreach (var properties in testingHustleProperties)
				{
					AddTestingStrategy(properties);
				}
			}
			catch (Exception ex)
			{
				ex.LogError();
			}

			try
			{
				AllStrategies.ListViewStrategies.ItemsSource = _documents.Keys.ToList();

				if (_documents.Count > 0)
				{
					_documents.First().Value.IsActive = true;
				}
			}
			catch (Exception ex)
			{
				ex.LogError();
			}
		}

		private void UpdateConnectionText()
		{
			var properties = SettingsEngine.Instance.Properties;

			var text = properties.ConnectionType.ToString();

			if (properties.Emulation)
			{
				text += " (Эмуляция!)";
			}

			this.GuiAsync(() => TextBlockConnectionType.Text = text);
		}

		# region Command handlers
		private void ExecutedConnect(object sender, ExecutedRoutedEventArgs e)
		{
			var worker = new BackgroundWorker();

			worker.DoWork += (o, ea) => ConnectionEngine.Instance.Connect();

			worker.RunWorkerCompleted += (o, ea) =>
			{
			};

			worker.RunWorkerAsync();
		}

		private void CanExecuteConnect(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = !ConnectionEngine.Instance.IsConnected;
			e.Handled = true;
		}

		private void ExecutedDisconnect(object sender, ExecutedRoutedEventArgs e)
		{
			var worker = new BackgroundWorker();

			worker.DoWork += (o, ea) => ConnectionEngine.Instance.Disconnect();

			worker.RunWorkerCompleted += (o, ea) =>
			{
			};

			worker.RunWorkerAsync();
		}

		private void CanExecuteDisconnect(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = ConnectionEngine.Instance.IsConnected;
			e.Handled = true;
		}

		private void ExecutedAddStrategy(object sender, ExecutedRoutedEventArgs e)
		{
			AddStrategy();

			AllStrategies.ListViewStrategies.ItemsSource = _documents.Keys.ToList();
		}

		private void CanExecuteAddStrategy(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
			e.Handled = true;
		}

        private void ExecutedAddHustleStrategy(object sender, ExecutedRoutedEventArgs e)
        {
            AddHustleEveryDayStrategy();

            AllStrategies.ListViewStrategies.ItemsSource = _documents.Keys.ToList();
        }

        private void CanExecuteAddHustleStrategy(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
            e.Handled = true;
        }

		private void ExecutedRemoveStrategy(object sender, ExecutedRoutedEventArgs e)
		{
			var strategy = SelectedStrategy;

			var result = new MessageBoxBuilder().Owner(this.GetWindow()).YesNo()
				.Text("Вы действительно хотите удалить стратегию '{0}'?".Put(strategy.Params.Name))
				.Show();

			if (result == MessageBoxResult.Yes)
			{
				this.AddInfoLog("Удаление стратегии '{0}'", strategy.Params.Name);

				var doc = _documents[strategy];
				_documents.Remove(strategy);

				StrategiesDocumentPane.RemoveChild(doc);
			    _hustleEveryDayStrategies.RemoveAll(s => s.Params.Name == strategy.Params.Name);
				SaveStrategies();
			}

			AllStrategies.ListViewStrategies.ItemsSource = _documents.Keys.ToList();
		}

		private void CanExecuteRemoveStrategy(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (SelectedStrategy != null);
			e.Handled = true;
		}

		private void ExecutedStartStrategy(object sender, ExecutedRoutedEventArgs e)
		{
			var strategy = SelectedStrategy;

			StartStrategy(strategy);
		}

		private void CanExecuteStartStrategy(object sender, CanExecuteRoutedEventArgs e)
		{
			var strategy = SelectedStrategy;
			e.CanExecute = ConnectionEngine.Instance.IsConnected && strategy != null && strategy.ProcessState == ProcessStates.Stopped;
			e.Handled = true;
		}

		private void ExecutedStopStrategy(object sender, ExecutedRoutedEventArgs e)
		{
			var strategy = SelectedStrategy;

			StopStrategy(strategy);
		}

		private void CanExecuteStopStrategy(object sender, CanExecuteRoutedEventArgs e)
		{
			var strategy = SelectedStrategy;
			e.CanExecute = ConnectionEngine.Instance.IsConnected && strategy != null && strategy.ProcessState == ProcessStates.Started;
			e.Handled = true;
		}


		private void ExecutedStopStrategyToCash(object sender, ExecutedRoutedEventArgs e)
		{
			var strategy = SelectedStrategy;

			this.AddInfoLog("Stop strategy and close position '{0}'", strategy.Params.Name);

			strategy.IsClosePositionsOnStop = true;

			strategy.StopAndClose();
		}

		private void CanExecuteStopStrategyToCash(object sender, CanExecuteRoutedEventArgs e)
		{
			var strategy = SelectedStrategy;
			e.CanExecute = ConnectionEngine.Instance.IsConnected && strategy != null && strategy.ProcessState == ProcessStates.Started;
			e.Handled = true;
		}

		private void ExecutedSettings(object sender, ExecutedRoutedEventArgs e)
		{
			var window = new SettingsWindow();
			window.ShowModal(this);
		}

		private void CanExecuteSettings(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
			e.Handled = true;
		}

		private void ExecutedExit(object sender, ExecutedRoutedEventArgs e)
		{
			Application.Current.Shutdown(110);
		}

		private void CanExecuteExit(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
			e.Handled = true;
		}
		#endregion

		public void StartStrategy(BaseShellStrategy strategy)
		{
			strategy.AddInfoLog("Запуск стратегии '{0}'", strategy.Params.Name);

			if(ConnectionEngine.Instance.Trader == null)
			{
				this.AddErrorLog("Ошибка запуска стратегии {0}, нет подключения", strategy.Params.Name);
				return;
			}

			var security = ConnectionEngine.Instance.Trader.Securities.FirstOrDefault
				(s => strategy.Params.Security != null && s.Id == strategy.Params.Security.Id);

			if (security == null)
			{
				strategy.AddErrorLog("Не найден инструмент {0} для запуска стратегии", strategy.Params.Security);
				return;
			}
			else
			{
				strategy.Security = security;
			}

			var portfolio = ConnectionEngine.Instance.Trader.Portfolios.FirstOrDefault(p => strategy.Params.Portfolio != null && p.Name == strategy.Params.Portfolio.Name);

			if (portfolio == null)
			{
				strategy.AddErrorLog("Не найден портфель {0} для запуска стратегии", strategy.Params.Portfolio);
				return;
			}
			else
			{
				strategy.Portfolio = portfolio;
			}

			//strategy.Volume = strategy.Params.Volume;

			strategy.Start();
		}

		public void StopStrategy(BaseShellStrategy strategy, bool closePostions = false)
		{
			this.AddInfoLog("Остановка стратегии '{0}'", strategy.Params.Name);

			strategy.IsClosePositionsOnStop = closePostions;

			strategy.StopAndClose();
		}

        private void AddHustleEveryDayStrategy()
        {
            var properties = new HustleEveryDayStrategyProperties
            {
                // TODO: check if the stategy with same does exist
                Name = "Новая стратегия {0}".Put(_newStrategyCount++),
            };

            AddHustleEveryDayStrategy(properties);
            SaveStrategies();
        }

        private void AddHustleEveryDayStrategy(HustleEveryDayStrategyProperties properties)
		{
			var strategy = new HustleEveryDayStrategy
			{
				Params = properties,
				Connector = ConnectionEngine.Instance.Trader
			};

			_logManager.Sources.Add(strategy);

		    var doc = new LayoutDocument
		    {
                Title = strategy.Params.Name,
                Content = new StrategyDocument
				{
					Strategy = strategy
				},
		        CanClose = false
		    };

			strategy.Params.PropertyChanged += (s, a) =>
			{
				if (a.PropertyName == "Name")
				{
					doc.Title = strategy.Params.Name;
				}

				SaveStrategies();
			};

			_documents.Add(strategy, doc);
            _hustleEveryDayStrategies.Add(strategy);

			this.AddInfoLog("Добавлена стратегия '{0}'", strategy.Name);

			StrategiesDocumentPane.Children.Add(doc);
		}

        private void AddHustleEverydayTestingStrategy()
        {
            var properties = new HustleEveryDayStrategyTestingProperties
            {
                Name = "Тестирование {0}".Put(_newTestingCount++)
            };

            AddTestingStrategy(properties);
            SaveTestingStrategies();
        }

        private void AddHustleEverydayTestingStrategy(HustleEveryDayStrategyTestingProperties properties)
        {
            var strategy = new HustleEveryDayStrategy
            {
                Params = properties,
				Connector = ConnectionEngine.Instance.Trader
            };

            var doc = new LayoutDocument()
            {
                Title = strategy.Params.Name,
                Content = new TestingDocument
                {
                    Strategy = strategy
                },
                CanClose = false
            };

            strategy.Params.PropertyChanged += (s, a) =>
            {
                if (a.PropertyName == "Name")
                {
                    doc.Title = strategy.Params.Name;
                }

                SaveTestingStrategies();
            };

            _testingDocuments.Add(strategy, doc);
            _hustleEveryDayTestingStrategies.Add(strategy);
            this.AddInfoLog("Добавлена тестовая стратегия '{0}'", strategy.Name);

            StrategiesDocumentPane.Children.Add(doc);
        }


        private void AddStrategy()
        {
            var properties = new BaseShellStrategyProperties
            {
                // TODO: check if the stategy with same does exist
                Name = "Новая стратегия {0}".Put(_newStrategyCount++),
            };

            AddStrategy(properties);
            SaveStrategies();
        }

        private void AddStrategy(BaseShellStrategyProperties properties)
        {
            var strategy = new BaseShellStrategy
            {
                Params = properties,
				Connector = ConnectionEngine.Instance.Trader
            };

            _logManager.Sources.Add(strategy);

            var doc = new LayoutDocument
            {
                Title = strategy.Params.Name,
                Content = new StrategyDocument
                {
                    Strategy = strategy
                },
                CanClose = false
            };

            strategy.Params.PropertyChanged += (s, a) =>
            {
                if (a.PropertyName == "Name")
                {
                    doc.Title = strategy.Params.Name;
                }

                SaveStrategies();
            };

            _documents.Add(strategy, doc);

            this.AddInfoLog("Добавлена стратегия '{0}'", strategy.Name);

            StrategiesDocumentPane.Children.Add(doc);
        }

        private void AddTestingStrategy()
        {
            var properties = new BaseShellTestingProperties
            {
                Name = "Тестирование {0}".Put(_newTestingCount++)
            };

            AddTestingStrategy(properties);
            SaveTestingStrategies();
        }

        private void AddTestingStrategy(BaseShellTestingProperties properties)
        {
            var strategy = new BaseShellStrategy
            {
                Params = properties,
				Connector = ConnectionEngine.Instance.Trader
            };

            var doc = new LayoutDocument
            {
                Title = strategy.Params.Name,
                Content = new TestingDocument
                {
                    Strategy = strategy
                },
                CanClose = false
            };

            strategy.Params.PropertyChanged += (s, a) =>
            {
                if (a.PropertyName == "Name")
                {
                    doc.Title = strategy.Params.Name;
                }

                SaveTestingStrategies();
            };

            _testingDocuments.Add(strategy, doc);
            
            this.AddInfoLog("Добавлена тестовая стратегия '{0}'", strategy.Name);

			StrategiesDocumentPane.Children.Add(doc);			            
        }

		private void SaveStrategies()
		{
			

		    SettingsEngine.Instance.SaveHustleEveryDayStrategiesProperties(
		        _hustleEveryDayStrategies.Select(s => (HustleEveryDayStrategyProperties)s.Params).ToList());

            var otherStrategies = _documents.Keys.Where( s=>_hustleEveryDayStrategies.All(str => str.Params.Name != s.Params.Name)).Select(strategy => strategy.Params).ToList();
            SettingsEngine.Instance.SaveStrategies(otherStrategies);
		}

        private void SaveTestingStrategies()
        {
            SettingsEngine.Instance.SaveTestingHustleEveryDayStrategyProperties(
                _hustleEveryDayTestingStrategies.Select(s => (HustleEveryDayStrategyTestingProperties)s.Params).ToList());

            var strategies = _testingDocuments.Keys.Where( s=>_hustleEveryDayTestingStrategies.All(str => str.Params.Name != s.Params.Name)).Select(strategy => (BaseShellTestingProperties)strategy.Params).ToList();

            SettingsEngine.Instance.SaveTestingStrategies(strategies);
        }

		private BaseShellStrategy SelectedStrategy
		{
			get
			{
				BaseShellStrategy result = null;

				var doc = dockManager.ActiveContent;

				var document = doc as StrategyDocument;
				if (document != null)
				{
					var content = document;
					result = content.Strategy;
				}

				return result;	
			}
		}

		private TestingDocument SelectedTestingDocument
		{
			get
			{
				TestingDocument result = null;

				var doc = dockManager.ActiveContent;

				var document = doc as TestingDocument;
				if (document != null)
				{
					var content = document;
					result = content;
				}

				return result;
			}
		}


		#region Implementation of ILogSource
		private readonly Guid _id = Guid.NewGuid();


	    public Guid Id
		{
			get { return _id; }
		}

		/// <summary>
		/// Имя источника.
		/// </summary>
		string ILogSource.Name
		{
			get { return "Робот"; }
		}

	    public LogLevels LogLevel
	    {
		    get
		    {
				return LogLevels.Debug;
		    }
			set
			{
			}
	    }

		ILogSource ILogSource.Parent
		{
			get { return null; }
			set { }
		}

		public DateTimeOffset CurrentTime
		{
			get
			{
				return DateTimeOffset.Now.AddHours(SettingsEngine.Instance.Properties.MarketTimeOffset);
			}
		}

		public event Action<LogMessage> Log;

		public void AddLog(LogMessage message)
		{
			Log.SafeInvoke(message);
		}

		#endregion

		public void Dispose()
		{
			//throw new NotImplementedException();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			//var preferences = new WindowPreferences();

			//Height = preferences.WindowHeight;
			//Width = preferences.WindowWidth;
			//Top = preferences.WindowTop;
			//Left = preferences.WindowLeft;
			//WindowState = preferences.WindowState;

			var version = Assembly.GetExecutingAssembly().GetName().Version;
			Title += " - {0}.{1}.{2}".Put(version.Major, version.Minor, version.Build);

			this.AddInfoLog(Title);
		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			var preferences = new WindowPreferences
			{
				WindowHeight = Height,
				WindowWidth = Width,
				WindowTop = Top,
				WindowLeft = Left,
				WindowState = WindowState
			};

			preferences.Save();
		}

	    private void ExecutedAddTestingStrategy(object sender, ExecutedRoutedEventArgs e)
	    {
	        AddTestingStrategy();
	    }

	    private void CanExecuteAddTestingStrategy(object sender, CanExecuteRoutedEventArgs e)
	    {
	        e.CanExecute = true;
	        e.Handled = true;
	    }

	    private void ExecutedRemoveTestingStrategy(object sender, ExecutedRoutedEventArgs e)
	    {
            var strategy = SelectedTestingDocument.Strategy;

            var result = new MessageBoxBuilder().Owner(this.GetWindow()).YesNo()
                .Text("Вы действительно хотите удалить тестовую стратегию '{0}'?".Put(strategy.Params.Name))
                .Show();

            if (result == MessageBoxResult.Yes)
            {
                this.AddInfoLog("Удаление тестовой стратегии '{0}'", strategy.Params.Name);

				var doc = _testingDocuments[strategy];
				_testingDocuments.Remove(strategy);

				StrategiesDocumentPane.RemoveChild(doc);

                _testingDocuments.Remove(strategy);

                SaveTestingStrategies();
            }
	    }

	    private void CanExecuteRemoveTestingStrategy(object sender, CanExecuteRoutedEventArgs e)
	    {
            e.CanExecute = (SelectedTestingDocument != null);
            e.Handled = true;
	    }

	    private void ExecutedStartTestingStrategy(object sender, ExecutedRoutedEventArgs e)
	    {
			var doc = SelectedTestingDocument;

			StartTesingStrategy(doc);
	    }

	    private void CanExecuteStartTestingStrategy(object sender, CanExecuteRoutedEventArgs e)
	    {
			var doc = SelectedTestingDocument;

			if(doc != null)
			{
				var strategy = doc.Strategy;
				e.CanExecute = strategy != null && strategy.ProcessState == ProcessStates.Stopped;	
			}
			e.Handled = true;
	    }

	    private void ExecutedStopTestingStrategy(object sender, ExecutedRoutedEventArgs e)
	    {
            var doc = SelectedTestingDocument;

            StopTesingStrategy(doc);
	    }

        private void CanExecuteStopTestingStrategy(object sender, CanExecuteRoutedEventArgs e)
	    {
            var doc = SelectedTestingDocument;

            if (doc != null)
            {
                var strategy = doc.Strategy;
                e.CanExecute = strategy != null && strategy.ProcessState == ProcessStates.Started;
            }
            e.Handled = true;
	    }

        private void ExecutedTestingReport(object sender, ExecutedRoutedEventArgs e)
        {
            var doc = SelectedTestingDocument;

            const string report = "report.xlsx";

            new ExcelStrategyReport(doc.Strategy, report).Generate();

            // открыть отчет
            Process.Start(report);
        }

        private void CanExecuteTestingReport(object sender, CanExecuteRoutedEventArgs e)
        {
            var doc = SelectedTestingDocument;

            if (doc != null)
            {
                var strategy = doc.Strategy;
                e.CanExecute = strategy != null;
            }
            e.Handled = true;
        }

        private void StopTesingStrategy(TestingDocument doc)
        {
			// если процесс был запущен, то его останавливаем
            if (_emulationTrader != null && _emulationTrader.State != EmulationStates.Stopped)
			{
				doc.Strategy.Stop();
                _emulationTrader.Stop();
				//_logManager.Sources.Clear();
			}
        }

		/// <summary>
		/// Начать тестирование выбранной стратегии на исторических данных.
		/// </summary>
		private void StartTesingStrategy(TestingDocument doc)
		{
            // создаем новую стратегию и копируем настройки
		    var strategy = new BaseShellStrategy
		    {
                Params = doc.Strategy.Params
		    };

		    doc.Strategy = strategy;

			var parameters = (BaseShellTestingProperties)strategy.Params;

			if (parameters.Path.IsEmpty() || !Directory.Exists(parameters.Path))
			{
                this.AddErrorLog("Неправильный путь к историческим данным: {0}", parameters.Path);
				return;
			}

			var security = new Security
			{
				Id = parameters.Security.Id,
				PriceStep = parameters.MinStepSize,
                StepPrice = parameters.MinStepPrice,
				Board = ExchangeBoard.Forts,
				MarginBuy = parameters.Margin,
				MarginSell = parameters.Margin
			};

			// тестовый портфель
			var portfolio = new Portfolio
			{
				Name = "Backtest",
				BeginValue = parameters.PortfolioAmount,
				CurrentValue = parameters.PortfolioAmount
			};

			// хранилище, через которое будет производиться доступ к тиковой и котировочной базе
			var storage = new StorageRegistry();

			// изменяем путь, используемый по умолчанию
			((LocalMarketDataDrive)storage.DefaultDrive).Path = parameters.Path;

			_emulationTrader = new HistoryEmulationConnector(new[] { security }, new[] { portfolio })
			{
				StorageRegistry = storage,
			};

			_emulationTrader.MarketDataAdapter.SessionHolder.MarketTimeChangedInterval = TimeSpan.FromSeconds(1);

			_emulationTrader.RegisterMarketDepth(new TrendMarketDepthGenerator(security.ToSecurityId())
			{
				// стакан для инструмента в истории обновляется раз в секунду
				Interval = TimeSpan.FromSeconds(1),
				GenerateDepthOnEachTrade = true,
				MaxSpreadStepCount = 10,
				UseTradeVolume = true
			});

			strategy.Connector = _emulationTrader;
			strategy.Security = security;
			strategy.Portfolio = portfolio;
			//_strategy.Volume = 1;
			strategy.IsReady = true;

			// копируем параметры на визуальную панель
			doc.ParametersPanel.Parameters.Clear();
			doc.ParametersPanel.Parameters.AddRange(strategy.StatisticManager.Parameters);


			//doc.TestingPanel.CurveChart.Reset();

			var curve = doc.CurveChart.CreateCurve(strategy.Name, Colors.Green); 

			strategy.PnLChanged += () =>
			{
				var data = new EquityData
				{
					Time = strategy.Connector.CurrentTime,
					Value = strategy.PnL,
				};

				this.GuiAsync(() => curve.Add(data));
			};

		    var lastUpdateDate = (DateTimeOffset)parameters.StartDate;
			// и подписываемся на событие изменения времени, чтобы обновить ProgressBar
			_emulationTrader.MarketTimeChanged += mt =>
			{
				// в целях оптимизации обновляем ProgressBar раз в 10 минут
				if (strategy.Connector.CurrentTime.AddMinutes(-10) > lastUpdateDate)
				{
					lastUpdateDate = strategy.Connector.CurrentTime;
					this.GuiAsync(() => doc.ProgressBarTesting.Value = strategy.Connector.CurrentTime.Ticks);
				}
			};

			_emulationTrader.StateChanged += () =>
			{
				if (_emulationTrader.State == EmulationStates.Stopped)
				{
					this.GuiAsync(() => this.AddInfoLog("Остановка \"{0}\" по историческим данным", parameters.Name));

					strategy.Stop();
				}
				else if (_emulationTrader.State == EmulationStates.Started)
				{
                    this.AddInfoLog("Запуск \"{0}\" по историческим данным", parameters.Name);

					// запускаем стратегию когда эмулятор запустился
					strategy.Start();
				}
			};

			// устанавливаем в визуальный элемент ProgressBar максимальное количество итераций)
            doc.ProgressBarTesting.Minimum = parameters.StartDate.Ticks;
            doc.ProgressBarTesting.Maximum = parameters.StopDate.Ticks;
		    doc.ProgressBarTesting.Value = doc.ProgressBarTesting.Minimum;

			// соединяемся с трейдером и запускаем экспорт,
			// чтобы инициализировать переданными инструментами и портфелями необходимые свойства EmulationTrader
			_emulationTrader.Connect();
			_emulationTrader.StartExport();

			// запускаем эмуляцию, задавая период тестирования (startTime, stopTime).
			_emulationTrader.Start(parameters.StartDate, parameters.StopDate);
		}

	    private void ExecutedStartAllStrategies(object sender, ExecutedRoutedEventArgs e)
	    {
		    foreach (var strategy in _documents.Keys.ToList())
		    {
			    StartStrategy(strategy);
		    }
	    }

	    private void CanExecuteStartAllStrategies(object sender, CanExecuteRoutedEventArgs e)
	    {
		    e.CanExecute = ConnectionEngine.Instance.Trader != null;
		    e.Handled = true;
	    }


	}
}
