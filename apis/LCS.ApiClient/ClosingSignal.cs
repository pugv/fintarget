using System;

namespace LifeCycleService.ApiClient
{
    public class ClosingSignal
    {
        public Guid StrategyId { get; set; }

        public int ManagerId { get; set; }

        public string SecurityKey { get; set; }
        public string Comment { get; set; }
    }
}