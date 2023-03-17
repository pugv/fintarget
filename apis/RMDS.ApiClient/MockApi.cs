using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RMDS.Common;

namespace RMDS.ApiClient
{
    public class MockApi : IApi, IDisposable
    {
        private readonly BidAsk ba = new BidAsk { Bid = 100, Ask = 100, LastPrice = 100 };

        public MockApi()
        {
        }

        public MockApi(BidAsk ret)
        {
            ba = ret;
        }

        public event Action<Guid> PositionsStreamConnected
        {
            add { }
            remove { }
        }

        public event Action<Guid> PositionsStreamDisconnected
        {
            add { }
            remove { }
        }

        public Task<(DateTime Start, DateTime End, IEnumerable<Hloc> BidCandles, IEnumerable<Hloc> AskCandles)> GetBidAskCandlesAsync(DateTime start,
            string[] securityList)
        {
            return Task.FromResult((start, DateTime.UtcNow, securityList.Select(c => new Hloc { Close = 100, Open = 100, High = 100, Low = 100, Security = c }),
                securityList.Select(c => new Hloc { Close = 100, Open = 100, High = 100, Low = 100, Security = c })));
        }

        public Task<(DateTime Start, DateTime End, IEnumerable<Hloc> Candles)> GetCandlesAsync(DateTime start, string[] securityList)
        {
            return Task.FromResult(
                (start, DateTime.UtcNow, securityList.Select(c => new Hloc { Close = 100, Open = 100, High = 100, Low = 100, Security = c })));
        }

        public Task<IEnumerable<BidAsk>> PollMarketDataAsync(string[] securityList)
        {
            return Task.FromResult(securityList.Select(c => new BidAsk
                { Security = c, Bid = ba.Bid, Ask = ba.Ask, LastPrice = ba.LastPrice, TimeStamp = DateTime.UtcNow }));
        }

        public Task<IEnumerable<BidAsk>> RequestMarketDataAsync(string[] securityList)
        {
            return Task.FromResult(securityList.Select(c => new BidAsk
                { Security = c, Bid = ba.Bid, Ask = ba.Ask, LastPrice = ba.LastPrice, TimeStamp = DateTime.UtcNow }));
        }

        public void StartListeningForPositions(Func<PositionsResponse, Task> positionsReceived)
        {
        }

        public Task SubscribeForAllPositionsAsync()
        {
            return Task.CompletedTask;
        }

        public Task SubscribeForPositionsAsync(string[] clientCodes, string[] futuresCodes)
        {
            return Task.CompletedTask;
        }

        public Task UnsubscribeForAllPositionsAsync()
        {
            return Task.CompletedTask;
        }

        public Task UnsubscribeForPositionsAsync(string[] clientCodes, string[] futuresCodes)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}