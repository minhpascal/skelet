using System;
using System.ComponentModel;
using System.Linq;

using Ecng.Common;
using StockSharp.Algo;
using StockSharp.Algo.Strategies;
using StockSharp.BusinessEntities;
using StockSharp.Logging;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Robot.Strategies
{
	using StockSharp.Messages;

	public class HustleEveryDayStrategyProperties :BaseShellStrategyProperties
    {

        private decimal _takeProfit1 = 10000000;
        [DisplayName(@"1-й тейк профит")]
        [Description(@"Цена для 1-й цели")]
        [Category(@"Параметры")]
        [PropertyOrder(0)]
        public decimal TakeProfit1
        {
            get { return _takeProfit1; }
            set
            {
                _takeProfit1 = value;
                OnPropertyChanged("TakeProfit1");
            }
        }



        private decimal _takeProfit2 = 10000000;
        [DisplayName(@"2-й тейк профит")]
        [Description(@"Цена для 2-й цели")]
        [Category(@"Параметры")]
        [PropertyOrder(0)]
        public decimal TakeProfit2
        {
            get { return _takeProfit2; }
            set
            {
                _takeProfit2 = value;
                OnPropertyChanged("TakeProfit2");
            }
        }
        private decimal _takeProfit3 = 10000000;
        [DisplayName(@"3-й тейк профит")]
        [Description(@"Цена для 3-й цели")]
        [Category(@"Параметры")]
        [PropertyOrder(0)]
        public decimal TakeProfit3
        {
            get { return _takeProfit3; }
            set
            {
                _takeProfit3 = value;
                OnPropertyChanged("TakeProfit3");
            }
        }

        private decimal _stoploss = 10000000;
        [DisplayName(@"стоп лосс")]
        [Description(@"Цена для стоп лосса")]
        [Category(@"Параметры")]
        [PropertyOrder(0)]
        public decimal Stoploss
        {
            get { return _stoploss; }
            set
            {
                _stoploss = value;
                OnPropertyChanged("Stoploss");
            }
        }
        private DateTime _closeTime = new DateTime(2000,1,1,1,1,1);
        [DisplayName(@"Время закрытия")]
        [Description(@"Время закрытия сделки")]
        [Category(@"Параметры")]
        [PropertyOrder(0)]
        public DateTime CloseTime
        {
            get { return _closeTime; }
            set
            {
                _closeTime = value;
                OnPropertyChanged("CloseTime");
            }
        }

		private Sides _orderDirection = Sides.Buy;
        [DisplayName(@"Направление сделки")]
        [Description(@"Направление сделки")]
        [Category(@"Параметры")]
        [PropertyOrder(0)]
		public Sides OrderDirection
        {
            get { return _orderDirection; }
            set
            {
                _orderDirection = value;
                OnPropertyChanged("OrderDirection");
            }
        }


    }

    public class  HustleEveryDayStrategyTestingProperties:BaseShellTestingProperties
    {
        

    }


   public class HustleEveryDayStrategy:BaseShellStrategy
    {
       protected override void OnStarted()
       {

           base.OnStarted();

		   var currentTime = Connector.CurrentTime;
           this.AddInfoLog("Текущее рыночное время: {0}.".Put(currentTime));

           var currentClosePositionsTime = ((HustleEveryDayStrategyProperties) Params).CloseTime;
           this.AddInfoLog("Текущее время проверки выхода по тайм-стопу: {0}.".Put(currentClosePositionsTime));
           if (currentTime > currentClosePositionsTime)
           {
               var newClosePositionsTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day,
                                                        currentClosePositionsTime.Hour, currentClosePositionsTime.Minute,
                                                        currentClosePositionsTime.Second);
               this.AddInfoLog("Новое время проверки выхода по тайм-стопу: {0}.".Put(newClosePositionsTime));
               ((HustleEveryDayStrategyProperties) Params).CloseTime = newClosePositionsTime;
           }

           
           Security
			 .WhenTimeCome(Connector, ((HustleEveryDayStrategyProperties)Params).CloseTime)
             .Do(CheckForExitByTimeTargets)
             .Apply(this);

           Security
                   .WhenChanged(Connector)
                   .Do(CheckForExitByPriceTargets)
                   .Apply(this);

           SubscriptionEngine.Instance.RegisterSecurity(this, Security);

		   if (((HustleEveryDayStrategyProperties)Params).OrderDirection == Sides.Buy)
           {
               PlaceBuyMarketOrder(Security,Volume);

           }
           else
           {
               PlaceSellMarketOrder(Security, Volume);
           }

       }

       private  void CheckForExitByPriceTargets()
       {

         this.AddInfoLog("Проверка выхода по целям.");
       
         if( PositionManager.Positions.Any(p=> p.Security.Code==Security.Code))
         {
             var position = PositionManager.Positions.First(p => p.Security.Code == Security.Code);
             if (position.CurrentValue > 0)
             {
                 if (Security.LastTrade.Price <= ((HustleEveryDayStrategyProperties)Params).Stoploss)
                 { 
                    PlaceSellMarketOrder(position.Security, position.CurrentValue);
                    return;
                 }

                 if (Security.LastTrade.Price >= ((HustleEveryDayStrategyProperties)Params).TakeProfit3)
                 {
                     PlaceSellMarketOrder(position.Security, position.CurrentValue);
                     return;
                 }

                 if (Security.LastTrade.Price >= ((HustleEveryDayStrategyProperties)Params).TakeProfit2)
                 {
                     PlaceSellMarketOrder(position.Security,decimal.Floor(0.5m*position.CurrentValue));
                     ((HustleEveryDayStrategyProperties)Params).Stoploss =
                         ((HustleEveryDayStrategyProperties)Params).TakeProfit1;

                     return;
                 }
                 if (Security.LastTrade.Price >= ((HustleEveryDayStrategyProperties)Params).TakeProfit1)
                 {
                   
                     PlaceSellMarketOrder(position.Security, decimal.Floor(0.33m * position.CurrentValue));
                   
                     if(MyTrades.Any(t=>t.Trade.Security.Code==Security.Code))
                     {
                        var myTradesOrderedByDate = MyTrades.ToList().OrderBy(t => t.Trade.Time);
                        var trade = myTradesOrderedByDate.Last(t => t.Trade.Security.Code == Security.Code);
                        var entryPrice = trade.Trade.Price;

                         ((HustleEveryDayStrategyProperties) Params).Stoploss = entryPrice;

                     }
                     return;
                 }




             }
             else
             {
                 if (position.CurrentValue < 0)
                 {
                     if (Security.LastTrade.Price >= ((HustleEveryDayStrategyProperties)Params).Stoploss)
                     {
                         PlaceBuyMarketOrder(position.Security, position.CurrentValue);
                         return;
                     }

                     if (Security.LastTrade.Price <= ((HustleEveryDayStrategyProperties)Params).TakeProfit3)
                     {
                         PlaceBuyMarketOrder(position.Security, position.CurrentValue);
                         return;
                     }

                     if (Security.LastTrade.Price <= ((HustleEveryDayStrategyProperties)Params).TakeProfit2)
                     {
                         PlaceBuyMarketOrder(position.Security, decimal.Floor(0.5m * position.CurrentValue));
                         ((HustleEveryDayStrategyProperties)Params).Stoploss =
                             ((HustleEveryDayStrategyProperties)Params).TakeProfit1;

                         return;
                     }
                     if (Security.LastTrade.Price <= ((HustleEveryDayStrategyProperties)Params).TakeProfit1)
                     {

                         PlaceBuyMarketOrder(position.Security, decimal.Floor(0.33m * position.CurrentValue));

                         if (MyTrades.Any(t => t.Trade.Security.Code == Security.Code))
                         {
                             var myTradesOrderedByDate = MyTrades.ToList().OrderBy(t => t.Trade.Time);
                             var trade = myTradesOrderedByDate.Last(t => t.Trade.Security.Code == Security.Code);
                             var entryPrice = trade.Trade.Price;

                             ((HustleEveryDayStrategyProperties)Params).Stoploss = entryPrice;

                         }
                         return;
                     }




                 }
             }
          
              
         }
          

       }


       private  void CheckForExitByTimeTargets()
       {

           this.AddInfoLog("Проверка выхода по тайм-стопу");
       
             foreach (var position in PositionManager.Positions)
             {
                 if(position.CurrentValue>0)
                 {
                     PlaceSellMarketOrder(position.Security,position.CurrentValue);

                 }
           
                 if (position.CurrentValue < 0)
                 {
                     PlaceBuyMarketOrder(position.Security, -position.CurrentValue);


                 }
                  

             }

       }

       protected void PlaceBuyMarketOrder(Security security, decimal volume)
       {
           var order = this.BuyAtMarket(volume);
           order.Security = security;

           order
                          .WhenChanged()
                          .Do(() => this.AddInfoLog("Изменилось остояние заявки"))
                          .Once()
                          .Apply(this);



           order
               .WhenRegistered()
               .Do(() => this.AddInfoLog("Заявка успешно зарегестрирована"))
               .Once()
               .Apply(this);

           order
               .WhenRegisterFailed()
               .Do(() => this.AddInfoLog("Заявка не принята биржей"))
               .Once()
               .Apply(this);

           order
               .WhenMatched()
               .Do(() =>
               {

                   this.AddInfoLog("Заявка полностью исполнена");
               })
               .Once()
               .Apply(this);

           // регистрирация заявки

           RegisterOrder(order);

       }
       protected void PlaceSellMarketOrder(Security security, decimal volume)
       {
           var order = this.SellAtMarket(volume);
           order.Security = security;
           order
                          .WhenChanged()
                          .Do(() => this.AddInfoLog("Изменилось остояние заявки"))
                          .Once()
                          .Apply(this);



           order
               .WhenRegistered()
               .Do(() => this.AddInfoLog("Заявка успешно зарегестрирована"))
               .Once()
               .Apply(this);

           order
               .WhenRegisterFailed()
               .Do(() => this.AddInfoLog("Заявка не принята биржей"))
               .Once()
               .Apply(this);

           order
               .WhenMatched()
               .Do(() =>
               {

                   this.AddInfoLog("Заявка полностью исполнена");
               })
               .Once()
               .Apply(this);

           // регистрирация заявки

           RegisterOrder(order);

       }
   

    }
}
