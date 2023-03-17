using System;

namespace LifeCycleService.ApiClient
{
    public class Portfolio
    {
        public int Id { get; set; }
        public Guid ClientId { get; set; }
        public Guid AgreementId { get; set; }
        public bool AllowSell { get; set; }
        public Guid PortfolioId { get; set; }
        public decimal Sum { get; set; }
        public decimal SL { get; set; }
        public decimal TP { get; set; }
        public DateTime CloseDate { get; set; }
        public Position[] Positions { get; set; }

        public class Position
        {
            public string Security { get; set; }
            public decimal Weight { get; set; }
        }
    }
}