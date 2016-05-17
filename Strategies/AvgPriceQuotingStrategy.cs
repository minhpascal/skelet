namespace Robot
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;

	using Ecng.Common;

	using Stateless;

	using StockSharp.Algo;
	using StockSharp.Algo.Strategies;
	using StockSharp.BusinessEntities;
	using StockSharp.Logging;
	using StockSharp.Messages;

	/// <summary>
	/// Упрощенная стратегия маркет-котирования с возможностью получения средней цены исполнения.
	/// </summary>
	class AvgPriceQuotingStrategy : Strategy
	{
		/// <summary>
		/// Определяет переставлять ли заявки.
		/// При значении true выставляет только одну заявку.
		/// </summary>
		public bool IsLimitQuoting { set; get; }

		/// <summary>
		/// Направление котирования.
		/// </summary>
		public Sides QuotingDirection { get; private set; }

		/// <summary>
		/// Объем котирования.
		/// </summary>
		public decimal QuotingVolume { get; private set; }

		public TimeSpan ReRegisterTimeout { get; set; }

		enum States
		{
			Init,
			PlacingOrder,
			WaitingOrder,
			CancelingOrder,
			WaitingMyTrades,
			Done
		}

		enum Triggers
		{
			PlaceOrder,
			OrderActive,
			ReRegisterOrder,
			OrderDone,
			Done
		}

		volatile States _state;
		StateMachine<States, Triggers> _fsm;
		private Order _order;
		private decimal _price;
		private volatile bool _isInitialOrderPlaced;
		private DateTimeOffset _lastReRegisterTime;

		public AvgPriceQuotingStrategy(Sides quotingDirection, decimal quotingVolume, decimal priceOffset = 0)
		{
			QuotingDirection = quotingDirection;
			QuotingVolume = quotingVolume;
			PriceOffset = priceOffset;
			ReRegisterTimeout = TimeSpan.Zero;

			InitializeFsm();
		}

		private void InitializeFsm()
		{
			_fsm = new StateMachine<States, Triggers>(() => _state, SetState);

			_fsm.Configure(States.Init).Permit(Triggers.PlaceOrder, States.PlacingOrder);
			_fsm.Configure(States.PlacingOrder).OnEntry(FsmPlacingOrderOnEntry).Permit(Triggers.OrderActive, States.WaitingOrder);
			_fsm.Configure(States.WaitingOrder).Permit(Triggers.ReRegisterOrder, States.CancelingOrder);
			_fsm.Configure(States.CancelingOrder).OnEntry(FsmCancelOrderOnEntry);
			_fsm.Configure(States.CancelingOrder).Permit(Triggers.PlaceOrder, States.PlacingOrder);
			_fsm.Configure(States.WaitingOrder).Permit(Triggers.OrderDone, States.WaitingMyTrades);
			_fsm.Configure(States.PlacingOrder).Permit(Triggers.OrderDone, States.WaitingMyTrades);
			_fsm.Configure(States.CancelingOrder).Permit(Triggers.OrderDone, States.WaitingMyTrades);
			_fsm.Configure(States.WaitingMyTrades).OnEntry(FsmWaitingMyTradesOnEntry).Permit(Triggers.Done, States.Done);
			_fsm.Configure(States.Done).OnEntry(FsmDoneOnEntry);
		}

		void SetState(States state)
		{
			lock (this)
			{
				if (_state != state)
				{
					this.AddInfoLog(_state + " -> " + state);
					_state = state;
				}	
			}
		}

		/// <summary>
		/// Абсолютный сдвиг цены для ордеров.
		/// </summary>
		protected decimal PriceOffset { private set; get; }
		
		protected override void OnStarted()
		{
			base.OnStarted();

			this.AddInfoLog("Quoting {0} {1}.", QuotingDirection, QuotingVolume);

			Connector.OrdersChanged += OnTraderOrdersChanged;
			Connector.MarketDepthsChanged += OnTraderMarketDepthsChanged;
			Connector.NewMyTrades += OnTraderNewMyTrades;
			Connector.OrdersRegisterFailed += OnTraderOrderRegisterFailed;
		}

		protected override void OnStopping()
		{
			base.OnStopping();

			this.AddInfoLog("Quoting stoped. Left Volume: {0}", LeftVolume);

			Connector.OrdersChanged -= OnTraderOrdersChanged;
			Connector.MarketDepthsChanged -= OnTraderMarketDepthsChanged;
			Connector.NewMyTrades -= OnTraderNewMyTrades;
			Connector.OrdersRegisterFailed -= OnTraderOrderRegisterFailed;
		}

		private void OnTraderOrdersChanged(IEnumerable<Order> orders)
		{
			if(orders.Contains(_order))
			{
				if(_order.State == OrderStates.Active)
				{
					Fire(Triggers.OrderActive);
				}
				else if(_order.State == OrderStates.Done)
				{
					if (_order.Balance == 0)
					{
						this.AddInfoLog("*tr id {0} done, bal = 0", _order.TransactionId);
						Fire(Triggers.OrderDone);	
					}
					else
					{
						this.AddInfoLog("*tr id {0} done, bal = {1}", _order.TransactionId, _order.Balance);
						Fire(Triggers.PlaceOrder);							
					}
				}
			}
		}

		private void OnTraderMarketDepthsChanged(IEnumerable<MarketDepth> enumerable)
		{
			var price = GetCurrentPrice();

			if(price != 0)
			{
				if(_price == 0)
				{
					_price = QuotingDirection == Sides.Buy ? price + PriceOffset : price - PriceOffset;

					//this.AddInfoLog("price {0}, _price {1}", price, _price);
				}

				else if (! IsLimitQuoting)
				{
					var time = Connector.CurrentTime;

					//this.AddInfoLog("time {0}, _lastRegTim {1}, next time {2}", time, _lastReRegisterTime, _lastReRegisterTime + ReRegisterTimeout);

					if (QuotingDirection == Sides.Buy && _price < price + PriceOffset 
						&& time > _lastReRegisterTime + ReRegisterTimeout)
					{
						_lastReRegisterTime = time;
						_price = price + PriceOffset;
					}

					if (QuotingDirection == Sides.Sell && _price > price - PriceOffset
						&& time > _lastReRegisterTime + ReRegisterTimeout)
					{
						_lastReRegisterTime = time;
						_price = price - PriceOffset;
					}					
				}

				if(_order == null)
				{
					lock(this)
					{
						if (!_isInitialOrderPlaced)
						{
							_isInitialOrderPlaced = true;

							this.AddInfoLog("_order == null, placing initial order");
							Fire(Triggers.PlaceOrder);
						}						
					}
				}
				else
				{
					if(_order.State == OrderStates.Active && _state == States.PlacingOrder)
					{
						Fire(Triggers.OrderActive);
					}
					else if (_order.State != OrderStates.Done && _order.Price != _price)
					{
						Fire(Triggers.ReRegisterOrder);
					}

					//Trace.WriteLine(Trader.MarketTime + " " + _order.State);
				}
			}
		}

		private void OnTraderNewMyTrades(IEnumerable<MyTrade> myTrades)
		{
			if(_state == States.WaitingMyTrades)
			{
				CalculateAveragePrice();
			}
		}

		private void OnTraderOrderRegisterFailed(IEnumerable<OrderFail> orders)
		{
			foreach (var fail in orders)
			{
				if(fail.Order == _order)
				{
					this.AddErrorLog("Ошибки регистрации ордера id {0}, tr id {1}: {2}", _order.Id, _order.TransactionId, fail.Error);	
					Stop();
				}
			}
		}

		private void FsmPlacingOrderOnEntry()
		{
			_order = new Order
			{
				Portfolio = Portfolio,
				Security = Security,
				Direction = QuotingDirection,
				Price = _price,
				Volume = LeftVolume
			};

			RegisterOrder(_order);

			this.AddInfoLog("Registerging order tr id {0}, volume {1}, price {2}", _order.TransactionId, _order.Volume, _order.Price);
		}

		private void FsmCancelOrderOnEntry()
		{
			this.AddInfoLog("Canceling order id {0}, tr id {1}", _order.Id, _order.TransactionId);

			Connector.CancelOrder(_order);
		}

		private void FsmWaitingMyTradesOnEntry()
		{
			CalculateAveragePrice();
		}

		private void CalculateAveragePrice()
		{
			var price = 0m;

			foreach (var order in Orders)
			{
				var realizedVolume = order.Volume - order.Balance;

				if(realizedVolume != 0)
				{
					var o = order;
					var trades = MyTrades.Where(t => t.Order == o).ToList();

					var volume = trades.Sum(t => t.Trade.Volume);

					if(realizedVolume != volume)
					{
						this.AddInfoLog("Some MyTrades are missing for order id {0}, tr id {1}", order.Id, order.TransactionId);
						return;
					}

					price += trades.Sum(trade => (trade.Trade.Volume * trade.Trade.Price));
				}
			}

			price /= QuotingVolume;

			AveragePrice = price;

			this.AddWarningLog("Average price: {0}", AveragePrice);

			Fire(Triggers.Done);
		}

		private void FsmDoneOnEntry()
		{
			Stop();
		}

		private void Fire(Triggers trigger)
		{
			lock (this)
			{
				if (_fsm.CanFire(trigger))
				{
					_fsm.Fire(trigger);
				}
				else
				{
					Trace.WriteLine("Fsm: can't fire {0} from state {1}".Put(trigger, _state));
				}	
			}
		}

		private decimal GetCurrentPrice()
		{
			var quote = FilteredQuotes.FirstOrDefault();

			if (quote == null)
				return 0;

			if (_order == null)
				return quote.Price;

			if (_order.Direction == Sides.Buy)
			{
				if (_order.Price > quote.Price)
					return _order.Price;
			}
			else
			{
				if (_order.Price < quote.Price)
					return _order.Price;
			}

			return quote.Price;
		}

		public decimal LeftVolume
		{
			get
			{
				var realizedVolume = Orders.Sum(order => (order.Volume - order.Balance));

				return QuotingVolume - realizedVolume;
			}
		}

		protected List<Quote> FilteredQuotes
		{
			get
			{
				return Connector
					.GetFilteredMarketDepth(Security)
					.Where(q => q.OrderDirection == QuotingDirection)
					.ToList();
			}
		}

		public decimal AveragePrice { get; private set; }
	}
}
