using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ExchangeHelpers;
using ExchangeHelpers.TS;
using NLib.Http;
using NLib.ParsersFormatters;

namespace TradeService.ApiClient
{
    public class Api
        : BaseHttpClient
    {
        public Api(string serverUrl, int serverPort)
            : base(serverUrl, serverPort, TimeSpan.FromSeconds(30), new JsonFormatterFactory(), new JsonParser())
        {
        }

        public async Task CancelOrders(IEnumerable<OrderCancellationRequest> requests, CancellationToken token)
        {
            var result = await PostRequestAsync<StandardResponse>("orders/cancel", requests, cancellationToken: token);
            if (!result.Success)
            {
                throw new Exception(result.Message);
            }
        }
        public async Task<IEnumerable<OrdersCancellationStatusResponse>> QueryOrdersCancellationStatus(IEnumerable<Guid> requestIds, 
            CancellationToken token)
        {
            var result =
                await PostRequestAsync<StandardResponse<OrdersCancellationStatusResponse[]>>(
                    "orders/cancellationStatus", requestIds, cancellationToken: token);
            if (!result.Success)
            {
                throw new Exception(result.Message);
            }

            return result.Result;
        }
    }
}