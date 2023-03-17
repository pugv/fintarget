using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ExchangeHelpers;
using Newtonsoft.Json;
using NLib.Http;

namespace QRCApi
{
    public class QRCApi
    {
        private readonly HttpRequester client;

        public QRCApi()
        {
            // for moq
        }

        public QRCApi(string host, int port)
        {
            client = new HttpRequester(host, port, 30);
        }

        public virtual async Task<Dictionary<string, ClientPortfolio[]>> GetLimits(string[] clientCodes)
        {
            var response = await client.RequestAsync("limits", JsonConvert.SerializeObject(clientCodes));
            var res = JsonConvert.DeserializeObject<StandardResponse<Dictionary<string, ClientPortfolio[]>>>(response);
            if (res?.Success != true) return null;
            return res.Result;
        }

        public virtual async Task<bool> CheckActiveOrders(ActiveOrdersRequestDto request)
        {
            var response = await client.RequestAsync("orders/has_active", JsonConvert.SerializeObject(request));
            var result = JsonConvert.DeserializeObject<StandardResponse<bool>>(response);
            if (result.Success)
                return result.Result;
            throw new Exception($"CheckActiveOrders: {result.Message}");
        }
    }
}