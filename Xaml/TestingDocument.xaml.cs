namespace Robot
{
	using StockSharp.Logging;
	using StockSharp.Xaml;

    public partial class TestingDocument
    {
        private readonly LogManager _logManager = new LogManager();
        private BaseShellStrategy _strategy;

        public TestingDocument()
        {
            InitializeComponent();

            _logManager.Listeners.Add(new GuiLogListener(logControl));
			_logManager.Listeners.Add(new DebugLogListener());
        }

        public BaseShellStrategy Strategy
        {
			set
			{
				_strategy = value;
				ProperyGridStrategy.SelectedObject = _strategy.Params;
				_logManager.Sources.Add(_strategy);
			}

			get { return _strategy; }
		}
    }
}
