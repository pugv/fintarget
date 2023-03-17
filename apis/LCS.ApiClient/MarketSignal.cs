using System;

namespace LifeCycleService.ApiClient
{
    public class MarketSignal
    {
        public Guid StrategyId { get; set; }
        public int ManagerId { get; set; }
        public string Symbol { get; set; }
        public string ClassCode { get; set; }
        public string Board { get; set; }
        public decimal Weight { get; set; }
        public string Comment { get; set; }
        public decimal? StopLoss { get; set; }
        public decimal? TakeProfit { get; set; }
    }
}