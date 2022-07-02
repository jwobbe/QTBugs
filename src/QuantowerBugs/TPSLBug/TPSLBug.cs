// Copyright QUANTOWER LLC. © 2017-2021. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using TradingPlatform.BusinessLayer;

namespace TPSLBug
{
   /// <summary>
   /// An example of strategy for working with one symbol. Add your code, compile it and run via Strategy Runner panel in the assigned trading terminal.
   /// Information about API you can find here: http://api.quantower.com
   /// </summary>
   public class TPSLBug : Strategy, ICurrentAccount, ICurrentSymbol
   {
      private HistoricalData _historicalData;

      [InputParameter("Symbol", 10)]
      public Symbol CurrentSymbol { get; set; }

      [InputParameter("Account", 20)]
      public Account CurrentAccount {get;set;}

      public override string[] MonitoringConnectionsIds => new string[] { CurrentSymbol?.ConnectionId };

      public TPSLBug()
          : base()
      {
         // Defines strategy's name and description.
         Name = "TPSLBug";
         Description = "My strategy's annotation";
      }

      /// <summary>
      /// This function will be called after creating a strategy
      /// </summary>
      protected override void OnCreated()
      {
      }

      /// <summary>
      /// This function will be called after running a strategy
      /// </summary>
      protected override void OnRun()
      {
         if (CurrentSymbol == null || CurrentAccount == null || CurrentSymbol.ConnectionId != CurrentAccount.ConnectionId)
         {
            Log("Incorrect input parameters... Symbol or Account are not specified or they have diffent connectionID.", StrategyLoggingLevel.Error);
            return;
         }

         CurrentSymbol = Core.GetSymbol(CurrentSymbol?.CreateInfo());
         if (CurrentSymbol == null)
            return;

         _historicalData = CurrentSymbol.GetHistory(new HistoryRequestParameters
         {
            Symbol = CurrentSymbol,
            FromTime = default,
            ToTime = default,
            Aggregation = new HistoryAggregationTime(Period.MIN1),
            //SessionsContainer = sessionsContainer,
            HistoryType = CurrentSymbol.HistoryType
         });

         _historicalData.NewHistoryItem += HistoricalData_NewHistoryItem;

         Core.TradeAdded += Core_TradeAdded;
      }


      /// <summary>
      /// This function will be called after stopping a strategy
      /// </summary>
      protected override void OnStop()
      {
         if (_historicalData != null)
         {
            _historicalData.NewHistoryItem -= HistoricalData_NewHistoryItem;
            _historicalData.Dispose();
         }
         
      }

      /// <summary>
      /// This function will be called after removing a strategy
      /// </summary>
      protected override void OnRemove()
      {
         CurrentSymbol = null;
         CurrentAccount = null;
      }

      

      private void HistoricalData_NewHistoryItem(object sender, HistoryEventArgs e)
      {
         var currentTime = _historicalData[0].TimeLeft;
         if (currentTime == new DateTime(2022, 6, 15, 14, 12, 0))
            PlaceStopOrder(CurrentSymbol, Side.Buy, 3794.25, 21, 31, 5);
      }

      private void Core_TradeAdded(Trade trade)
      {
         var positionSize = Core.Positions.FirstOrDefault()?.Quantity;
         var currentTime = _historicalData[0].TimeLeft;

         if (trade.PositionImpactType == PositionImpactType.Open)
            Log($"{currentTime} - Trade opened by order {trade.OrderId} to {trade.Side} {trade.Quantity} {trade.Symbol} for {trade.Price}.  Position size {positionSize}.", StrategyLoggingLevel.Trading);
         else
            Log($"{currentTime} - Trade closed by order {trade.OrderId} to {trade.Side} {trade.Quantity} {trade.Symbol} for {trade.Price}. Position size {positionSize}.  Net PL: {trade.GrossPnl}.", StrategyLoggingLevel.Trading);
      }


      protected virtual TradingOperationResult PlaceStopOrder(Symbol symbol, Side side, double orderPrice, double risk, double reward, double quantity)
      {

         var result = Core.PlaceOrder(new PlaceOrderRequestParameters
         {
            Account = CurrentAccount,
            Symbol = symbol,
            Side = side,
            Quantity = quantity,
            OrderTypeId = OrderType.Stop,
            TriggerPrice = orderPrice,
            StopLoss = SlTpHolder.CreateSL(risk, PriceMeasurement.Offset),
            TakeProfit = SlTpHolder.CreateTP(reward, PriceMeasurement.Offset)
         });

         return result;
      }

   }
}
