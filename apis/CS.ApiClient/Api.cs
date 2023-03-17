using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ExchangeHelpers;
using Newtonsoft.Json;
using NLib.Http;
using NLib.ParsersFormatters;

namespace CS.ApiClient
{
    public class Api
        : BaseHttpClient
    {
        public Api(string serverUrl, int serverPort)
            : base(serverUrl, serverPort, TimeSpan.FromSeconds(30), new JsonFormatterFactory(), new JsonParser())
        {
        }

        public void UpdateClientAccount(ClientAccountTO account)
        {
            var result = PostRequest<StandardResponse>("clients/update_account", account);

            if (!result.Success)
                throw new Exception(result.Message);
        }

        public async Task UpdateClientAccountAsync(ClientAccountTO account)
        {
            var result = await PostRequestAsync<StandardResponse>("clients/update_account", account);

            if (!result.Success)
                throw new Exception(result.Message);
        }

        public void UpdateClientAccountPositions(IEnumerable<ClientAccountPositionTO> positions)
        {
            var result = PostRequest<StandardResponse>("clients/update_account_positions", positions);

            if (!result.Success)
                throw new Exception(result.Message);
        }


        public async Task UpdateClientAccountPositionsAsync(IEnumerable<ClientAccountPositionTO> positions)
        {
            var result = await PostRequestAsync<StandardResponse>("clients/update_account_positions", positions);

            if (!result.Success)
                throw new Exception(result.Message);
        }

        public async Task<object> CalcRebalance(string ClientCode)
        {
            var result = await PostRequestAsync<StandardResponse<object>>($"clients/calcrebalance/{ClientCode}", "");

            if (!result.Success)
                throw new Exception(result.Message);

            return result.Result;
        }


        public async Task<ClientAccountFullTO[]> GetClientAccountByCodeAsync(string ClientCodeOrFuturesCode)
        {
            var result = await PostRequestAsync<StandardResponse<ClientAccountFullTO[]>>($"clients/account_info/{ClientCodeOrFuturesCode}", "");

            if (!result.Success)
                throw new Exception(result.Message);

            return result.Result;
        }

        public async Task<ClientAccountFullTO[]> GetClientAccountByGuidAsync(Guid id, bool Reload = false)
        {
            var result = Reload
                ? await PostRequestAsync<StandardResponse<ClientAccountFullTO[]>>($"clients/reload_info/{id}", "")
                : await PostRequestAsync<StandardResponse<ClientAccountFullTO[]>>($"clients/account_info/{id}", "");

            if (!result.Success)
                throw new Exception(result.Message);

            return result.Result;
        }

        public async Task<ClientAccountFullTO> FetchClientLimits(string clientCode)
        {
            var result = await PostRequestAsync<StandardResponse<ClientAccountFullTO>>($"clients/fetch/{clientCode}", "");

            if (!result.Success)
                throw new Exception(result.Message);

            return result.Result;
        }


        public async Task<ClientInfoTO> FetchClientProfile(string clientCode)
        {
            var result = await PostRequestAsync<StandardResponse<ClientInfoTO>>($"clients/fetchprofile/{clientCode}", "");

            if (!result.Success)
                throw new Exception(result.Message);

            return result.Result;
        }

        public async Task<bool> Rebalance()
        {
            var result = await GetRequestAsync<StandardResponse>("clients/rebalance");

            if (!result.Success)
                throw new Exception(result.Message);

            return result.Success;
        }

        public async Task<Dictionary<int,string>> ProblemClients()
        {
            var result = await GetRequestAsync<StandardResponse<Dictionary<int,string>>>("clients/problem");

            if (!result.Success)
                throw new Exception(result.Message);

            return result.Result;
        }

        public async Task<bool> Rebalance(string ClientCode)
        {
            var result = await PostRequestAsync<StandardResponse>("clients/rebalance_account", new { ClientCode });

            if (!result.Success)
                throw new Exception(result.Message);

            return result.Success;
        }

        /// <summary>
        ///     Блокирует клиента на время (throws on error)
        /// </summary>
        /// <param name="ClientCode"></param>
        /// <param name="time">На сколько блокировать</param>
        /// <returns>Был ли клиент реально заблокирован</returns>
        public async Task<bool> BlockClient(string ClientCode, TimeSpan time, CancellationToken cancellationToken)
        {
            var result = await PostRequestAsync<StandardResponse<bool>>("clients/block", new { ClientCode, Time = time }, cancellationToken: cancellationToken);

            if (!result.Success)
                throw new Exception(result.Message);

            return result.Result;
        }

        public async Task<bool> Exit()
        {
            return (await GetRequestAsync("exit")).Equals("Ok");
        }

        public async Task<string> Info()
        {
            return await GetRequestAsync("info");
        }

        public async Task<SecurityTO> Security(string key)
        {
            return JsonConvert.DeserializeObject<StandardResponse<SecurityHolder>>(await GetRequestAsync($"info/{key}")).Result.Security;
        }

        private class SecurityHolder
        {
            public SecurityTO Security { get; set; }
        }
    }
}