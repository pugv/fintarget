using System;

namespace QRCApi
{
    public class ClientPortfolio
    {
        public string ClientCode { get; set; }
        public string SecurityKey { get; set; }
        public long Qty { get; set; }
        public decimal AvgPrice { get; set; }
        public string Currency { get; set; }
        public string Depo { get; set; }
        public string Kind { get; set; } // TO, ...
        public string Tag { get; set; } // EQTV, ...
        public decimal Varmargin { get; set; }
        public decimal TotalVarmargin { get; set; }
        public decimal PositionValue { get; set; }
        public DateTime UpdateTime { get; set; }
        public bool Confirm { get; set; }
    }
}