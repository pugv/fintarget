//using Google.Protobuf.WellKnownTypes;
//using Grpc.Core;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using NLib.Config;
using NLib.Logger;
using ProtoBuf.Grpc.Client;
using RMDS.Common;

namespace RMDS.ApiClient
{
    public class Api : IApi, IDisposable
    {
        private readonly IRealtimeMDService _client;
        private readonly ApiConfig _config;
        private readonly string _clientAppName;
        private readonly HashSet<string> _knownClientCodes = new HashSet<string>();
        private readonly HashSet<string> _knownFuturesCodes = new HashSet<string>();
        private readonly ILogger _logger;
        private readonly SemaphoreSlim _newRequestsAvailable = new SemaphoreSlim(0);
        private readonly ConcurrentQueue<RequestForPositionsRequest> _pendingRequests = new ConcurrentQueue<RequestForPositionsRequest>();
        private Func<PositionsResponse, Task> _positionsReceived;
        private readonly CancellationTokenSource _positionsStreamCTS = new CancellationTokenSource();

        private Guid _positionsStreamGuid;
        private Task _positionsSubscription;
        private bool _requestAllPositionsWasInvoked;
        private GrpcChannel _channel;
        private bool _disposed;

        public Api(IConfig config, string clientAppName = null)
        {
            _logger = LoggerFactory.ProduceLogger();

            _config = new ApiConfig(config);
            _clientAppName = clientAppName;

            GrpcClientFactory.AllowUnencryptedHttp2 = !_config.GrpcUseSSL;
            _channel = GrpcChannel.ForAddress($"{(_config.GrpcUseSSL ? "https" : "http")}://{_config.GrpcHost}:{_config.GrpcPort}");

            _client = _channel.CreateGrpcService<IRealtimeMDService>();
        }

        public event Action<Guid> PositionsStreamDisconnected;

        public void StartListeningForPositions(Func<PositionsResponse, Task> positionsReceived)
        {
            if (_positionsReceived != null) throw new Exception("StartListeningForPositions was already called");
            _positionsReceived = positionsReceived;
            _positionsSubscription = DoSubscriptionsLoop(_positionsStreamCTS.Token);
        }

        public Task SubscribeForPositionsAsync(string[] clientCodes, string[] futuresCodes)
        {
            foreach (var clientCode in clientCodes ?? Enumerable.Empty<string>()) _knownClientCodes.Add(clientCode);
            foreach (var futuresCode in futuresCodes ?? Enumerable.Empty<string>()) _knownFuturesCodes.Add(futuresCode);
            _pendingRequests.Enqueue(new RequestForPositionsRequest
                { StreamId = _positionsStreamGuid, ClientCodes = clientCodes, FuturesCodes = futuresCodes, Subscribe = true });
            _newRequestsAvailable.Release();
            return Task.CompletedTask;
        }

        public Task UnsubscribeForPositionsAsync(string[] clientCodes, string[] futuresCodes)
        {
            foreach (var clientCode in clientCodes ?? Enumerable.Empty<string>()) _knownClientCodes.Remove(clientCode);
            foreach (var futuresCode in futuresCodes ?? Enumerable.Empty<string>()) _knownFuturesCodes.Remove(futuresCode);
            _pendingRequests.Enqueue(new RequestForPositionsRequest
                { StreamId = _positionsStreamGuid, ClientCodes = clientCodes, FuturesCodes = futuresCodes, Subscribe = false });
            _newRequestsAvailable.Release();
            return Task.CompletedTask;
        }

        public Task SubscribeForAllPositionsAsync()
        {
            _pendingRequests.Enqueue(new RequestForPositionsRequest { AllPositions = true, Subscribe = true });
            _newRequestsAvailable.Release();
            _requestAllPositionsWasInvoked = true;
            return Task.CompletedTask;
        }

        public Task UnsubscribeForAllPositionsAsync()
        {
            _pendingRequests.Enqueue(new RequestForPositionsRequest { AllPositions = true, Subscribe = true });
            _newRequestsAvailable.Release();
            _requestAllPositionsWasInvoked = false;
            return Task.CompletedTask;
        }

        public async Task<IEnumerable<BidAsk>> RequestMarketDataAsync(string[] securityList)
        {
            var request = new MarketDataRequest { Securities = securityList };
            var result = await _client.RequestMarketDataAsync(request);
            return result.BidAsks;
        }

        public async Task<IEnumerable<BidAsk>> PollMarketDataAsync(string[] securityList)
        {
            var request = new MarketDataRequest { Securities = securityList };
            var result = await _client.PollMarketDataAsync(request);
            return result.BidAsks;
        }

        public async Task<(DateTime Start, DateTime End, IEnumerable<Hloc> Candles)> GetCandlesAsync(DateTime start, string[] securityList)
        {
            var result = await _client.GetCandlesAsync(new GetCandlesRequest { Start = start, Securities = securityList });
            return (result.Start, result.End, result.Candles);
        }

        public async Task<(DateTime Start, DateTime End, IEnumerable<Hloc> BidCandles, IEnumerable<Hloc> AskCandles)> GetBidAskCandlesAsync(DateTime start,
            string[] securityList)
        {
            var result = await _client.GetBidAskCandlesAsync(new GetCandlesRequest { Start = start, Securities = securityList });
            return (result.Start, result.End, result.BidCandles, result.AskCandles);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            _positionsStreamCTS?.Cancel();
            _positionsSubscription?.Wait();
            _logger.Info("Channel is shutting down");
            var t = new Thread(async () => await _channel?.ShutdownAsync());
            t.Start();
            t.Join();
            _logger.Info("Channel is shut down");
        }

        private async Task DoSubscriptionsLoop(CancellationToken token)
        {
            while (true)
            {
                try
                {
                    _positionsStreamGuid = default;
                    var options = new CallOptions(cancellationToken: token);

                    //restore subscriptions
                    if (_knownClientCodes.Any() || _knownFuturesCodes.Any())
                        _pendingRequests.Enqueue(new RequestForPositionsRequest
                        {
                            StreamId = _positionsStreamGuid,
                            ClientCodes = _knownClientCodes.ToArray(),
                            FuturesCodes = _knownFuturesCodes.ToArray(),
                            Subscribe = true
                        });
                    if (_requestAllPositionsWasInvoked)
                        _pendingRequests.Enqueue(new RequestForPositionsRequest
                        {
                            AllPositions = true,
                            Subscribe = true
                        });
                    _newRequestsAvailable.Release();

                    await foreach (var positions in _client.SubscribeToPositions2Async(YieldPositionsRequest(token),
                        new ProtoBuf.Grpc.CallContext(new CallOptions(new Metadata { { "X-App-Name", _clientAppName } }))))
                    {
                        // _logger.Info($"Received {positions}");
                        await _positionsReceived(positions);
                    }
                    //_logger.Info($"Processed {positions}");
                    _logger.Info("Reports stream closed by other side");
                }
                catch (Exception ex)
                {
                    if (ex is RpcException && ((RpcException)ex).StatusCode == StatusCode.Cancelled && token.IsCancellationRequested)
                    {
                        _logger.Info("Reports stream closed by us");
                        return;
                    }

                    _logger.Error(ex, "Reports stream failed, retrying...");
                }

                _logger.Info("Disconnected");
                PositionsStreamDisconnected?.Invoke(_positionsStreamGuid);
                await Task.Delay(_config.GrpcReconnectTimeout, token);
            }
        }

        private async IAsyncEnumerable<RequestForPositionsRequest> YieldPositionsRequest([EnumeratorCancellation] CancellationToken token)
        {
            while (true)
            {
                await _newRequestsAvailable.WaitAsync(token);
                while (_pendingRequests.TryDequeue(out var request)) yield return request;
            }
        }
    }
}