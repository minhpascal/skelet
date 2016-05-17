using System.Collections.Generic;

namespace Robot
{
	using Ecng.Common;

	using StockSharp.Algo;
	using StockSharp.Algo.Strategies;
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
	public class BaseShellStrategy : BaseStrategy
	{
		public BaseShellStrategyProperties Params { set; get; }
	    public bool IsClosePositionsOnStop { set; get; }
          private bool _isFirstStart;

          public BaseShellStrategy()
       {
           _isFirstStart = true;

       }

       protected void SaveStateToParams()
       {

           foreach (var order in Orders)
           {
               var transactionId = order.TransactionId.ToString();
               if (Params.OrdersByTransactionId.ContainsKey(transactionId))
               {
                   Params.OrdersByTransactionId[transactionId] = order;
               }
               else
               {
                   Params.OrdersByTransactionId.Add(transactionId, order);
               }

               var myTrades = order.GetTrades();
               List<Trade> trades = new List<Trade>();
               foreach (var myTrade in myTrades)
               {
                   trades.Add(myTrade.Trade);
               }
               if (Params.TradesByTransactionId.ContainsKey(transactionId))
               {
                   Params.TradesByTransactionId[transactionId] = trades;
               }
               else
               {
                   Params.TradesByTransactionId.Add(transactionId, trades);
               }

               if (Params.SecuritiesByTransactionId.ContainsKey(transactionId))
               {
                   Params.SecuritiesByTransactionId[transactionId] = order.Security;
               } 
               else
               {
                   Params.SecuritiesByTransactionId.Add(transactionId, order.Security);
               }
               if (Params.ExchangesByTransactionId.ContainsKey(transactionId))
               {
                   Params.ExchangesByTransactionId[transactionId] = order.Security.Board.Exchange;
               }
               else
               {
				   Params.ExchangesByTransactionId.Add(transactionId, order.Security.Board.Exchange);
               }
               if (Params.PortfoliosByTransactionId.ContainsKey(transactionId))
               {
                   Params.PortfoliosByTransactionId[transactionId] = order.Portfolio;
               }
               else
               {
                   Params.PortfoliosByTransactionId.Add(transactionId, order.Portfolio);

               }
              
           }
       }

       protected void LoadStateFromParams()
       {
               foreach (var transactionId in Params.OrdersByTransactionId.Keys)
           {
               var order =(Order) Params.OrdersByTransactionId[transactionId];
               if (order != null)
               {
				   order.Connector = Connector;
                   var security =(Security) Params.SecuritiesByTransactionId[transactionId];
                   if (security != null)
                   {

                       var exchange = (Exchange)Params.ExchangesByTransactionId[transactionId];
					   if (exchange != null) security.Board.Exchange = exchange;

					   //security.Connector = Connector;
                       order.Security = security;

                   }

                   var portfolio = (Portfolio)Params.PortfoliosByTransactionId[transactionId];
                   if (portfolio != null)
                   {
					   Portfolio.Connector = Connector;
                       order.Portfolio = portfolio;
                   }

                   var myTrades = new List<MyTrade>();
                   var trades =(IEnumerable<Trade>) Params.TradesByTransactionId[transactionId];

                   if (trades != null)
                       foreach (var trade in trades)
                       {
                           var myTrade = new MyTrade();
                           myTrade.Order = order;
                           myTrade.Trade = trade;
                           myTrades.Add(myTrade);

                       }
                   if (myTrades.Count > 0)
                   {
                       AttachOrder(order, myTrades);
                   }
                    
               }
           }

       }

       protected  override  void OnStarted()
       {
           base.OnStarted();

         if(_isFirstStart && Params.RestorePositionsOnStart)
         {
             _isFirstStart = false;
             LoadStateFromParams();

         }

       }

       protected override void OnStopped()
       {
           SaveStateToParams();
           
           base.OnStopped();
       }

		    
		public virtual void StopAndClose()
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
