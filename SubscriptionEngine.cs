namespace Robot
{
	using System;
	using System.Collections.Concurrent;
	using System.Collections.Generic;

	using Ecng.Collections;
	using Ecng.Common;

	using StockSharp.Algo.Strategies;
	using StockSharp.BusinessEntities;
	using StockSharp.Logging;

	class SubscriptionEngine : ILogReceiver
	{
		private static SubscriptionEngine _instance;
		private static readonly object _creationMutex = new object();

		private readonly ConcurrentDictionary<Security, List<Strategy>> _securitiesSubscriptions = new ConcurrentDictionary<Security, List<Strategy>>();
		private readonly ConcurrentDictionary<Security, List<Strategy>> _tradesSubscriptions = new ConcurrentDictionary<Security, List<Strategy>>();
		private readonly ConcurrentDictionary<Security, List<Strategy>> _quotesSubscriptions = new ConcurrentDictionary<Security, List<Strategy>>();

		private SubscriptionEngine()
		{
		}

		public static SubscriptionEngine Instance
		{
			get
			{
				if (_instance == null)
				{
					lock (_creationMutex)
					{
						if (_instance == null)
						{
							_instance = new SubscriptionEngine();
						}
					}
				}

				return _instance;
			}
		}

		/// <summary>
		/// Подписка на обновление инструмента
		/// </summary>
		/// <param name="strategy"> </param>
		/// <param name="security"></param>
		public void RegisterSecurity(Strategy strategy, Security security)
		{
			this.AddInfoLog("RegisterSecurity: strategy = {0}, security = {1}", strategy.Name, security.Id);

			if(! _securitiesSubscriptions.ContainsKey(security))
			{
				_securitiesSubscriptions[security] = new List<Strategy> {strategy};

				var trader = strategy.Connector;

				if(trader != null)
				{
					this.AddInfoLog("Trader.RegisterSecurity {0}", security.Id);
					trader.RegisterSecurity(security);
				}
			}
			else
			{
				_securitiesSubscriptions[security].Add(strategy);
			}
		}

		/// <summary>
		/// Отписка от обновления инструмента
		/// </summary>
		/// <param name="strategy"> </param>
		/// <param name="security"></param>
		public void UnRegisterSecurity(Strategy strategy, Security security)
		{
			this.AddInfoLog("UnRegisterSecurity: strategy = {0}, security = {1}", strategy.Name, security.Id);

			if (_securitiesSubscriptions.ContainsKey(security))
			{
				var strategies = _securitiesSubscriptions[security];

				strategies.Remove(strategy);
				
				if(strategies.IsEmpty())
				{
					this.AddInfoLog("Trader.UnRegisterSecurity {0}", security.Id);
					strategy.Connector.UnRegisterSecurity(security);					
				}
				else
				{
					_securitiesSubscriptions[security] = strategies;
				}
			}
		}

		/// <summary>
		/// Подписка на получение сделок по инструменту.
		/// </summary>
		/// <param name="strategy"> </param>
		/// <param name="security"></param>
		public void RegisterTrades(Strategy strategy, Security security)
		{
			this.AddInfoLog("RegisterTrades: strategy = {0}, security = {1}", strategy.Name, security.Id);

			if (!_tradesSubscriptions.ContainsKey(security))
			{
				_tradesSubscriptions[security] = new List<Strategy> { strategy };

				if (strategy.Connector != null)
				{
					this.AddInfoLog("Trader.RegisterTrades {0}", security.Id);
					strategy.Connector.RegisterTrades(security);
				}
			}
			else
			{
				_tradesSubscriptions[security].Add(strategy);
			}
		}

		/// <summary>
		/// Отписка от получения сделок по инструменту.
		/// </summary>
		/// <param name="strategy"> </param>
		/// <param name="security"></param>
		public void UnRegisterTrades(Strategy strategy, Security security)
		{
			this.AddInfoLog("UnRegisterTrades: strategy = {0}, security = {1}", strategy.Name, security.Id);

			if (_tradesSubscriptions.ContainsKey(security))
			{
				var strategies = _tradesSubscriptions[security];

				strategies.Remove(strategy);

				if (strategies.IsEmpty())
				{
					this.AddInfoLog("Trader.UnRegisterTrades( {0}", security.Id);
					strategy.Connector.UnRegisterTrades(security);
				}
				else
				{
					_tradesSubscriptions[security] = strategies;
				}
			}
		}
		
		/// <summary>
		/// Подписка на обновление стаканов по инструменту.
		/// </summary>
		public void RegisterMarketDepth(Strategy strategy, Security security)
		{
			this.AddInfoLog("RegisterMarketDepth: strategy = {0}, security = {1}", strategy.Name, security.Id);

			if (!_quotesSubscriptions.ContainsKey(security))
			{
				_quotesSubscriptions[security] = new List<Strategy> { strategy };

				if (strategy.Connector != null)
				{
					this.AddInfoLog("Trader.RegisterMarketDepth {0}", security.Id);
					strategy.Connector.RegisterMarketDepth(security);
				}
			}
			else
			{
				_quotesSubscriptions[security].Add(strategy);
			}
		}

		/// <summary>
		/// Отписка от обновления стаканов по инструменту.
		/// </summary>
		public void UnRegisterMarketDepth(Strategy strategy, Security security)
		{
			this.AddInfoLog("UnRegisterMarketDepth: strategy = {0}, security = {1}", strategy.Name, security.Id);

			if (_quotesSubscriptions.ContainsKey(security))
			{
				var strategies = _quotesSubscriptions[security];

				strategies.Remove(strategy);

				if (strategies.IsEmpty())
				{
					List<Strategy> value;
					_quotesSubscriptions.TryRemove(security, out value);

					this.AddInfoLog("Trader.UnRegisterMarketDepth: {0}", security.Id);

					strategy.Connector.UnRegisterMarketDepth(security);
				}
				else
				{
					_quotesSubscriptions[security] = strategies;
				}
			}
		}

		#region Logging
		private readonly Guid _id = Guid.NewGuid();

		public Guid Id
		{
			get { return _id; }
		}

		ILogSource ILogSource.Parent
		{
			get { return null; }
			set { }
		}

		string ILogSource.Name
		{
			get { return "SubscriptionEngine"; }
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

		public DateTimeOffset CurrentTime { get; private set; }

		public event Action<LogMessage> Log;

		public void AddLog(LogMessage message)
		{
			Log.SafeInvoke(message);
		}
		#endregion

		#region Implementation of IDisposable

		public void Dispose()
		{
			//throw new NotImplementedException();
		}

		#endregion
	}
}
