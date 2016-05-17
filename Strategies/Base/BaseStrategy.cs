namespace Robot
{
	using System;
	using System.Collections.Generic;
	using System.Threading;

	using Ecng.ComponentModel;

	using StockSharp.Algo;
	using StockSharp.Algo.Strategies;
	using StockSharp.BusinessEntities;

	public class BaseStrategy : Strategy
	{
		private bool _isReady;
		private Timer _workingTimeTimer;
		private TimeSpan _workingTime;
		private DateTime _timeStarted = DateTime.MinValue;
		private string _status;

		protected BaseStrategy()
		{
			UnrealizedPnLInterval = TimeSpan.FromSeconds(2);

			PnLChanged += () => this.Notify("PnL");
			PositionChanged += () => this.Notify("Position");
			SlippageChanged += () => this.Notify("Slippage");

			ProcessStateChanged += strategy =>
			{
				if(strategy.ProcessState == ProcessStates.Started)
				{
					_timeStarted = DateTime.Now;
					_workingTimeTimer = new Timer(state => this.Notify("WorkingTime"), null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
				}
				else if(strategy.ProcessState == ProcessStates.Stopped)
				{
					_workingTime += DateTime.Now - _timeStarted;
					_timeStarted = DateTime.MinValue;

					if(_workingTimeTimer != null)
					{
						_workingTimeTimer.Dispose();
					}
				}
			};
		}

		protected override void OnOrderRegistering(Order order)
		{
			base.OnOrderRegistering(order);
			this.Notify("Orders");
		}

		protected override void OnNewMyTrades(IEnumerable<MyTrade> trades)
		{
			base.OnNewMyTrades(trades);
			this.Notify("MyTrades");
		}

		public TimeSpan WorkingTime
		{
			get
			{
				if(_timeStarted != DateTime.MinValue)
				{
					return _workingTime + (DateTime.Now - _timeStarted);
				}
				else
				{
					return _workingTime;
				}
			}
		}

		/// <summary>
		/// Получены ли портфель и все инструменты для данной стратегии.
		/// </summary>
		public bool IsReady
		{
			get { return _isReady; }
			set
			{
				_isReady = value;
				this.Notify("IsReady");
			}
		}

		public string Status
		{
			get { return _status; }
			set
			{
				_status = value;
				this.Notify("Status");
			}
		}
	}
}