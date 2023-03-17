using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RMDS.Common;

namespace RMDS.ApiClient
{
    public interface IApi
    {
        event Action<Guid> PositionsStreamDisconnected;
        void StartListeningForPositions(Func<PositionsResponse, Task> positionsReceived);
        Task SubscribeForPositionsAsync(string[] clientCodes, string[] futuresCodes);
        Task UnsubscribeForPositionsAsync(string[] clientCodes, string[] futuresCodes);
        Task SubscribeForAllPositionsAsync();
        Task UnsubscribeForAllPositionsAsync();
        Task<IEnumerable<BidAsk>> PollMarketDataAsync(string[] securityList);
        Task<IEnumerable<BidAsk>> RequestMarketDataAsync(string[] securityList);
        Task<(DateTime Start, DateTime End, IEnumerable<Hloc> Candles)> GetCandlesAsync(DateTime start, string[] securityList);

        Task<(DateTime Start, DateTime End, IEnumerable<Hloc> BidCandles, IEnumerable<Hloc> AskCandles)> GetBidAskCandlesAsync(DateTime start,
            string[] securityList);
    }
}