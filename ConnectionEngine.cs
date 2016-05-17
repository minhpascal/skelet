namespace Robot
{
	using System;
	using System.ComponentModel;

	using Ecng.Common;

	using Stateless;

	using StockSharp.AlfaDirect;
	using StockSharp.Algo;
	using StockSharp.Algo.Testing;
	using StockSharp.BusinessEntities;
	using StockSharp.Logging;
	using StockSharp.Messages;
	using StockSharp.Quik;

	class ConnectionEngine : ILogReceiver, INotifyPropertyChanged
	{
		internal enum States
		{
			Disconnected,
			Connecting,
			Connected,
			Disconnecting
		}

		internal enum Triggers
		{
			Connect,
			Connected,
			Disconnect,
			Disconnected
		}

		StateMachine<States, Triggers> _fsm;
		States _state = States.Disconnected;

		private static ConnectionEngine _instance;

		//public ConnectionProperties Properties { set; get; }

		public IConnector Trader
		{
			get { return _trader; }
			set
			{
				_trader = value;
				OnPropertyChanged("Trader");
			}
		}

		public States State
		{
			get { return _state; }
			set 
			{ 
				_state = value;
				OnPropertyChanged("State");
			}
		}

		public bool IsConnected { set; get; }

		private ConnectionEngine()
		{
			InitFsm();

			ThreadingHelper.Timer(OnTimeChanged).Interval(TimeSpan.FromSeconds(30));
		}

		/// <summary>
		/// Configures final state machine.
		/// </summary>
		private void InitFsm()
		{
			_fsm = new StateMachine<States, Triggers>(() => _state, SetState);

			_fsm.Configure(States.Disconnected).Permit(Triggers.Connect, States.Connecting);
			_fsm.Configure(States.Connecting).OnEntry(FsmOnEntryConnecting);
			_fsm.Configure(States.Connecting).Permit(Triggers.Connected, States.Connected);
			_fsm.Configure(States.Connected).Permit(Triggers.Disconnect, States.Disconnecting);
			_fsm.Configure(States.Disconnecting).OnEntry(FsmOnEntryDisconnecting);
			_fsm.Configure(States.Disconnecting).Permit(Triggers.Disconnected, States.Disconnected);
			_fsm.Configure(States.Disconnected).OnEntry(FsmOnEntryDisconnected);
		}

		/// <summary>
		/// Makes transition to another state.
		/// </summary>
		void SetState(States state)
		{
			if (State != state)
			{
				this.AddWarningLog("{0} -> {1}", State, state);

				State = state;
			}
		}

		private void FsmOnEntryDisconnecting()
		{
			if (Trader != null)
			{
				Trader.Disconnect();
			}
		}

		private void FsmOnEntryDisconnected()
		{
			if (Trader != null)
			{
				Trader.Dispose();
				Trader = null;
			}
		}

		private void FsmOnEntryConnecting()
		{
			this.AddWarningLog("FsmOnEntryConnecting");

			switch (SettingsEngine.Instance.Properties.ConnectionType)
			{
				case SettingsProperties.Type.Alfa:
				{
					CreateAlfaTrader();
					break;
				}
				case SettingsProperties.Type.Quik:
				{
					CreateQuikTrader();
					break;
				}
				// TODO: add plaza
			}

			Trader.ConnectionError += error =>
			{
				this.AddErrorLog("Ошибка подключения: {0}", error);

				IsConnected = false;

				// TODO: handle connection errors in fsm
			};

			Trader.Disconnected += () =>
			{
				this.AddInfoLog("Trader Disconnected");

				IsConnected = false;

				Fire(Triggers.Disconnected);
			};

			Trader.Connected += () =>
			{
				this.AddInfoLog("Trader Connected");

				IsConnected = true;

				Fire(Triggers.Connected);

				Trader.StartExport();
			};

			try
			{
				Trader.Connect();
			}
			catch (Exception ex)
			{
				this.AddErrorLog("Ошибка подключения: {0}", ex.Message);
			}
		}

		public static ConnectionEngine Instance
		{
			get 
			{
				if (_instance == null)
				{
					_instance = new ConnectionEngine();
				}
				return _instance;
			}
		}

		private void OnTimeChanged()
		{
//			var time = DateTime.Now.AddHours(-1 * Properties.MarketTimeOffset).TimeOfDay;

//			if(! Properties.SchedulerEnabled)
//			{
//				 return;
//			}
//
//			if(Helper.Contains(Properties.Schedule, time))
//			{
//				Fire(Triggers.Connect);
//			}
//			else
//			{
//				Fire(Triggers.Disconnect);
//			}
		}

		public void Connect()
		{
			Fire(Triggers.Connect);
		}

		public void Disconnect()
		{
			Fire(Triggers.Disconnect);
		}

		private void Fire(Triggers trigger)
		{
			if (_fsm.CanFire(trigger))
			{
				_fsm.Fire(trigger);
			}
		}

		private void CreateAlfaTrader()
		{
			this.AddInfoLog("Conecting to AD");

			var trader = new AlfaTrader();

			if (SettingsEngine.Instance.Properties.Emulation)
			{
				this.AddWarningLog("Using real-time emulation");

                // тестовый портфель
                var portfolio = new Portfolio
                {
                    Name = "test account",
                    BeginValue = 1000000,
                };

                Trader = new RealTimeEmulationTrader<AlfaTrader>(trader);

				Trader.Connected += () =>
				{
					// передаем первоначальное значение размера портфеля в эмулятор
					((BaseEmulationConnector)Trader).TransactionAdapter.SendInMessage(portfolio.ToMessage());
					((BaseEmulationConnector)Trader).TransactionAdapter.SendInMessage(new PortfolioChangeMessage
					{
						PortfolioName = portfolio.Name
					}.Add(PositionChangeTypes.BeginValue, portfolio.BeginValue));
				};
			}
			else
			{
				Trader = trader;
			}
		}

		private void CreateQuikTrader()
		{
			this.AddInfoLog("Conecting to Quik");

			var settings = SettingsEngine.Instance.Properties;

			var trader = new QuikTrader(settings.QuikPath)
			{
				IsCommonMonetaryPosition = true // TODO: move to settings
			};

			if (!trader.Terminal.IsLaunched)
			{
				trader.Terminal.Launch();
				trader.Terminal.Login(settings.QuikLogin, settings.QuikPassword);
			}

			if (settings.Emulation)
			{
				this.AddWarningLog("Using real-time emulation");

                // тестовый портфель
                var portfolio = new Portfolio
                {
                    Name = "test account",
                    BeginValue = 1000000,
                };

				Trader = new RealTimeEmulationTrader<QuikTrader>(trader);

				Trader.Connected += () =>
				{
					// передаем первоначальное значение размера портфеля в эмулятор
					((BaseEmulationConnector)Trader).TransactionAdapter.SendInMessage(portfolio.ToMessage());
					((BaseEmulationConnector)Trader).TransactionAdapter.SendInMessage(new PortfolioChangeMessage
					{
						PortfolioName = portfolio.Name
					}.Add(PositionChangeTypes.BeginValue, portfolio.BeginValue));
				};
			}
			else
			{
				Trader = trader;
			}
		}

        #region Logging
        private readonly Guid _id = Guid.NewGuid();
		private IConnector _trader;

		public Guid Id
        {
            get { return _id; }
        }

        string ILogSource.Name
        {
            get { return "ConnectionEngine"; }
        }

        ILogSource ILogSource.Parent
        {
            get { return null; }
			set { }
        }

		public LogLevels LogLevel { get; set; }

		public DateTimeOffset CurrentTime { get; private set; }

        public event Action<LogMessage> Log;

        public void AddLog(LogMessage message)
        {
            Log.SafeInvoke(message);
        }

        #endregion

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string name)
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(name));
			}
		}

		public void Dispose()
		{
			//throw new NotImplementedException();
		}
	}
}
