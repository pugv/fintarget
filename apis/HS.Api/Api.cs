using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExchangeHelpers.HS;
using NLib.AuxTypes;
using NLib.Http;
using NLib.ParsersFormatters;

namespace HS.ApiClient
{
    public class Api
        : BaseHttpClient, IApi
    {
        public Api(string serverUrl, int serverPort, TimeSpan? timeout = null)
            : base(serverUrl, serverPort, timeout ?? TimeSpan.FromSeconds(30), new JsonFormatterFactory(), new JsonParser())
        {
        }

        public async Task<SecurityRecord[]> UpdateShortList(string[] securityKeys)
        {
            return await PostRequestAsync<SecurityRecord[]>("securities/list", securityKeys);
        }

        public async Task<SecurityRecord[]> UpdateFullList()
        {
            return await PostRequestAsync<SecurityRecord[]>("securities/all", string.Empty);
        }

        public async Task<ShortsResult[]> ShortsAllowed(string[] securityKeys)
        {
            var result = await PostRequestAsync<StandardResponse<ShortsResult[]>>("securities/shortsallowed", securityKeys);

            if (result.Success)
                return result.Result;

            throw new Exception(result.Message);
        }

        public async Task<Dictionary<string, PriceItem[]>> EndOfDay(string[] keys, Range<DateTime> dateRange, bool fillEmptyDays, bool calcSplitPrice=false, bool dontConvertFuturesPrices=false)
        {
            return await PostRequestAsync<Dictionary<string, PriceItem[]>>
            ("prices/endofday", new
            {
                Securities = keys,
                DateRange = dateRange,
                FillEmptyDays = fillEmptyDays,
                CalcSplitPrice = calcSplitPrice,
                DontConvertFuturesPrices = dontConvertFuturesPrices
            });
        }


        public async Task<Dictionary<(string From, string To), (string Key, string KeySmall)>> GetCurrencies()
        {
            var result = await PostRequestAsync<StandardResponse<Dictionary<string, (string, string)>>>("currencies", string.Empty);
            if (result.Success)
                return result.Result.ToDictionary(c => (c.Key.Split(',')[0], c.Key.Split(',')[1]), c => c.Value);

            throw new Exception(result.Message);
        }

        public async Task<Dictionary<string, (string Currency, decimal Coeff)>> GetFutures()
        {
            var result = await PostRequestAsync<StandardResponse<Dictionary<string, (string, decimal)>>>("futures", string.Empty);
            if (result.Success)
                return result.Result;

            throw new Exception(result.Message);
        }

        public async Task<decimal> GetExchangeRate(string curIn, string curOut)
        {
            return await GetRequestAsync<decimal>($"currencies/exchange/{curIn}/{curOut}");
        }

        public async Task<TradeTimeInfo[]> GetTradingTimesAsync(TradeTimeRequest request)
        {
            return await PostRequestAsync<TradeTimeInfo[]>("times/tradeschedule", request);
        }

        public async Task<SecurityRecord[]> GetSecurities(IEnumerable<string> securityKeys)
        {
            return await PostRequestAsync<SecurityRecord[]>("securities/list", securityKeys);
        }
        
        public async Task<SecurityRecord[]> GetAllSecurities()
        {
            return await GetRequestAsync<SecurityRecord[]>("securities/all");
        }
    }
}