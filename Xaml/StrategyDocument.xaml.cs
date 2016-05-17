namespace Robot
{
	using StockSharp.Logging;
	using StockSharp.Xaml;

	public partial class StrategyDocument
	{
		private BaseShellStrategy _strategy;
		private readonly LogManager _logManager = new LogManager();

		public StrategyDocument()
		{
			InitializeComponent();

			_logManager.Listeners.Add(new GuiLogListener(logControl));
		}

		public BaseShellStrategy Strategy
		{
			set
			{
				_strategy = value;
				ProperyGridStrategy.SelectedObject = _strategy.Params;
				_logManager.Sources.Add(_strategy);

				_strategy.OrderRegistering += order => OrderGrid.Orders.Insert(0, order);
				_strategy.StopOrderRegistering += order => OrderGrid.Orders.Insert(0, order);

				_strategy.NewMyTrades += trades =>
				{
					foreach (var trade in trades)
					{
						_tradeGrid.Trades.Insert(0, trade.Trade);
					}
				};

				_strategyInfoGrid.ListViewStrategies.Items.Clear();
				_strategyInfoGrid.ListViewStrategies.Items.Add(_strategy);
			}

			get { return _strategy; }
		}
	}
}
