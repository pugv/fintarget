using System;
using System.Threading.Tasks;
using ExchangeHelpers;
using ExchangeHelpers.LS;
using NLib.Http;
using NLib.ParsersFormatters;

namespace LS.Api
{
    public class Client : BaseHttpClient
    {
        private const string BaseRoute = "api/v1/client/";

        public Client(string serverHost, int serverPort) : base(serverHost, serverPort, TimeSpan.FromSeconds(10), null,
            new JsonParser())
        {
        }

        public async Task<ClientAccountDto[]> GetClient(Guid id)
        {
            var response = await GetRequestAsync<StandardResponse<ClientAccountDto[]>>(BaseRoute + id);
            if (!response.Success) throw new Exception(response.Message);

            return response.Result;
        }

        public async Task<ClientAccountDto[]> ReloadClient(Guid id, ReloadType? reloadTypes = null)
        {
            var response = await GetRequestAsync<StandardResponse<ClientAccountDto[]>>(BaseRoute + $"{id}/reload?reloadTypes={(int)reloadTypes}");
            if (!response.Success) throw new Exception(response.Message);

            return response.Result;
        }
    }
}