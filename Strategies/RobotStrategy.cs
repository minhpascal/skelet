namespace Robot
{
	using System;
	using Ecng.Collections;
	using Ecng.Common;
	using Ecng.ComponentModel;
	using StockSharp.Algo;
	using StockSharp.Algo.Strategies;
	using StockSharp.Algo.Strategies.Quoting;
	using StockSharp.BusinessEntities;
	using StockSharp.Logging;
	using StockSharp.Messages;

	/// <summary>
    /// Тестовая стратегия.
    /// </summary>
    /// <remarks>
    /// Выставляются симмертричные заявки по обоим сторонам спреда с учетом отсупа
    /// и минимального размера спреда. Позиции закрываются либо при исполнении обоих
    /// заявок, либо по истечении времени.
    /// </remarks>
	public sealed class RobotStrategy : BaseShellStrategy
	{
		 
		//public bool IsClosePositionsOnStop { set; get; }

		private LimitQuotingStrategy _bidOrder;
		private LimitQuotingStrategy _askOrder;

        private decimal _lastTradePrice;
		private MarketRule<IConnector, IConnector> _stopRule;

		public RobotStrategy()
		{
			UnrealizedPnLInterval = TimeSpan.FromSeconds(1);
			CancelOrdersWhenStopping = false;
		}

		public decimal LastTradePrice
		{
			get { return _lastTradePrice; }
			set
			{
				if(_lastTradePrice != value)
				{
					_lastTradePrice = value;
					this.Notify("LastTradePrice");
				}
			}
		}

		protected override void OnStarted()
        {
			base.OnStarted();

		    Security.WhenNewTrades(Connector)
				.Do(() =>
				{
					if(Security.LastTrade != null)
					{
                        LastTradePrice = Security.LastTrade.Price;
					}
				})
				.Apply(this);

			Security.WhenMarketDepthChanged(Connector)
				.Do(OnMarketDepthChanged)
				.Apply(this);

		    Volume = Params.Volume;

			SubscriptionEngine.Instance.RegisterTrades(this, Security);
			SubscriptionEngine.Instance.RegisterMarketDepth(this, Security);
			SubscriptionEngine.Instance.RegisterSecurity(this, Security);

			Connector.MarketTimeChanged += OnMarketTimeChanged;
        }

		private void OnMarketDepthChanged()
		{
			//this.AddWarningLog("quote changed");

            if (Security == null || Security.BestAsk == null || Security.BestBid == null)
			{
				return;
			}

			var ask = Security.BestAsk.Price;
			var bid = Security.BestBid.Price;

			lock (this)
			{
				if (_bidOrder == null && _askOrder == null)
				{
					// проверяем на сигнал на вход
					var spread = (ask - bid) / Security.PriceStep;

					if (spread >= ((RobotStrategyProperties)Params).Spread)
					{
						this.AddWarningLog("Сигнал на вход. Bid {0}, Ask {1}", bid, ask);

						var offset = ((RobotStrategyProperties)Params).Offset * Security.PriceStep;
						var bidPrice = bid - offset;
						var askPrice = ask + offset;

						_bidOrder = new LimitQuotingStrategy(Sides.Buy, Volume, bidPrice);
						_askOrder = new LimitQuotingStrategy(Sides.Sell, Volume, askPrice);

						_bidOrder.WhenStopped().Do(() =>
						{
							if(_bidOrder.LeftVolume == 0)
							{
                                if(_askOrder.ProcessState == ProcessStates.Stopped && _askOrder.LeftVolume ==0)
                                {
                                    this.AddWarningLog("Trade complete.");
                                    _bidOrder = _askOrder = null; 
  
									if(_stopRule != null)
									{
										_stopRule.Dispose();
										_stopRule = null;
									}
                                }
                                else
                                {
	                                CreateStopRule();
                                }
							}
						}).Apply(this);

						_askOrder.WhenStopped().Do(() =>
						{
                            if (_askOrder.LeftVolume == 0)
                            {
                                if (_bidOrder.ProcessState == ProcessStates.Stopped && _bidOrder.LeftVolume == 0)
                                {
                                    this.AddWarningLog("Trade complete.");
                                    _bidOrder = _askOrder = null;

									if (_stopRule != null)
									{
										_stopRule.Dispose();
										_stopRule = null;
									}
                                }
                                else
                                {
									CreateStopRule();
								}
                            }
						}).Apply(this);

						ChildStrategies.AddRange(new [] {_bidOrder, _askOrder});
					}
				}
			}
		}

		private void CreateStopRule()
		{
			_stopRule = Connector
                .WhenIntervalElapsed(TimeSpan.FromSeconds(((RobotStrategyProperties)Params).Stop))
				.Do(() =>
			{
				_bidOrder.Stop();
				_askOrder.Stop();

				if(Position != 0)
				{
					this.AddWarningLog("Stop time elapsed, closing position {0}", Position);

					var direction = Position > 0 ? Sides.Sell : Sides.Buy;

					var quoting = new MarketQuotingStrategy(direction, Position.Abs());

					quoting.WhenStopped().Do(() =>
					{
						this.AddWarningLog("Trade complete by stop.");
						_bidOrder = _askOrder = null;
						_stopRule = null;
					}).Apply(this);

					ChildStrategies.Add(quoting);

				    _bidOrder.Stop();
				}
			})
			.Once()
			.Apply(this);
		}

//		private decimal CalculateVolume()
//		{
//			var total = Portfolio.CurrentValue;
//
//		    var margin = 15000m;
//
//            if(Security.Exchange == Exchange.Micex)
//            {
//                if(Security.BestAsk != null)
//                {
//                    margin = Security.BestAsk.Price;
//                }
//                else if(Security.LastTrade != null)
//                {
//                    margin = Security.LastTrade.Price;
//                }
//                else
//                {
//                    this.AddErrorLog("Невозможно определить ГО для инсрумента {0}", Security.Id);
//                    Stop();
//                }
//
//            }
//            else
//            {
//                margin = Math.Max(Security.MarginBuy, Security.MarginSell);
//            }
//
//			var money = total * Params.RiskAversion / 100m;
//			var volume = Math.Floor(money / margin);
//
//			this.AddInfoLog("Средств тек.: {0}, Риск: {1}, Средств на торговлю: {2}, ГО: {3}, Объем: {4}",
//				total, Params.RiskAversion, money, margin, volume);
//
//			if (volume == 0)
//			{
//				this.AddErrorLog("Отсутствуют средства для торговли!");
//				Stop();
//			}
//
//			return volume;
//		}
//
		private void OnMarketTimeChanged(TimeSpan time)
		{
			if (Connector != null)
			{
//				if(Trader.GetMarketTime(Security.Exchange).TimeOfDay > new TimeSpan(23, 45, 00))
//				{
//					Trader.MarketTimeChanged -= OnMarketTimeChanged;
//
//					this.AddWarningLog("Остановка стратегии в 23:45");
//					Stop();
//				}	
			}
		}

		public override void StopAndClose()
		{
			if(IsClosePositionsOnStop)
			{
				this.AddWarningLog("Остановка с закрытием позиций, остаток {0}", Position);

				if (PositionManager.Position != 0)
				{
					var order = PositionManager.Position > 0
						? MarketSell((int)PositionManager.Position.Abs())
						: MarketBuy((int)PositionManager.Position.Abs());

					RegisterOrder(order);
				}
			}

			Stop();
		}

		protected override void OnStopping()
        {
			base.OnStopping();

			this.AddInfoLog("Stoping strategy");

			Connector.MarketTimeChanged -= OnMarketTimeChanged;

			SubscriptionEngine.Instance.UnRegisterTrades(this, Security);
			SubscriptionEngine.Instance.UnRegisterMarketDepth(this, Security);
			SubscriptionEngine.Instance.UnRegisterSecurity(this, Security);

			Status = string.Empty;
        }

		private Order MarketSell(int volume)
		{
			return this.CreateOrder(Sides.Sell, Security.BestBid.Price - 100 * Security.PriceStep, volume);
		}

		private Order MarketBuy(int volume)
		{
			return this.CreateOrder(Sides.Buy, Security.BestAsk.Price + 100 * Security.PriceStep, volume);
		}
	}
}
