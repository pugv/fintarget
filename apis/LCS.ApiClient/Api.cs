using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ExchangeHelpers;
using ExchangeHelpers.LCS;
using LCS.Kernel.BusinessModel;
using LCS.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLib.AuxTypes;
using NLib.Http;
using NLib.ParsersFormatters;

namespace LifeCycleService.ApiClient
{
    public class Api
        : BaseHttpClient
    {
        public Api(string serverUrl, int serverPort, TimeSpan? timeout = null)
            : base(serverUrl, serverPort, timeout ?? TimeSpan.FromSeconds(30), new JsonFormatterFactory(new JsonSerializerSettings
            {
                Culture = CultureInfo.InvariantCulture,
                Formatting = Formatting.Indented,
                ContractResolver = new DefaultContractResolver()
            }), new JsonParser())
        {
        }

        public async Task<Signal> NewSignal(MarketSignal signal)
        {
            var result = await PostRequestAsync<JsonResult<Signal>>("signals/new", signal);

            if (!result.Success)
                throw new UserVisibleException(result.Message);

            return result.Data;
        }

        public async Task<bool> NewSignals(MarketSignal[] signals)
        {
            var result = await PostRequestAsync<JsonResult>("signals/multiple", signals);

            if (!result.Success)
                throw new UserVisibleException(result.Message);

            return true;
        }

        public async Task<bool> ConstructStrategy(ConstructStrategy construct)
        {
            var result = await PostRequestAsync<JsonResult>("signals/construct", construct);

            if (!result.Success)
                throw new UserVisibleException(result.Message);

            return true;
        }

        /// <summary>
        ///     Split signal
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public async Task<Signal> NewChangeSignal(ChangeSignal signal)
        {
            var result = await PostRequestAsync<JsonResult<Signal>>("signals/newchange", signal);

            if (!result.Success)
                throw new UserVisibleException(result.Message);

            return result.Data;
        }

        /// <summary>
        ///     Replace position keeping weight
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public async Task<Signal> NewReplaceSignal(ChangeSignal signal)
        {
            var result = await PostRequestAsync<JsonResult<Signal>>("signals/newreplace", signal);

            if (!result.Success)
                throw new UserVisibleException(result.Message);

            return result.Data;
        }

        public async Task<DelayedSignalDTO> NewDelayedSignal(DelayedSignal signal)
        {
            var result = await PostRequestAsync<JsonResult<DelayedSignalDTO>>("signals/newdelayed", signal);

            if (!result.Success)
                throw new UserVisibleException(result.Message);

            return result.Data;
        }

        public async Task CloseDelayedSignal(long delayedSignalId)
        {
            var result = await PostRequestAsync<JsonResult>("signals/delete_delayed", new[] { delayedSignalId });

            if (!result.Success)
                throw new UserVisibleException(result.Message);
        }

        public async Task CloseSignal(ClosingSignal signal)
        {
            var result = await PostRequestAsync<JsonResult>("signals/close", signal);

            if (!result.Success)
                throw new UserVisibleException(result.Message);
        }

        public Strategy[] GetStrategies()
        {
            var result = PostRequest<JsonResult<Strategy[]>>("strategy/get", "");

            if (!result.Success)
                throw new Exception(result.Message);

            return result.Data;
        }


        public async Task<bool> SetStrategy(Strategy strat)
        {
            JsonResult result = await PostRequestAsync<JsonResult<Strategy[]>>("strategy/set",
                strat);

            return result.Success;
        }

        public async Task<bool> ReloadStrategy(Guid stratId)
        {
            var result = await PostRequestAsync<JsonResult>("strategy/reload", stratId.ToString());

            return result.Success;
        }

        public async Task<bool> BindTermPortfolio(Portfolio pf, CancellationToken cancellationToken)
        {
            var result = await PostRequestAsync<JsonResult>("portfolios/new", new TimedPortfolio
            {
                Id = pf.Id,
                AgreementId = pf.AgreementId,
                AllowCell = pf.AllowSell,
                ParentId = pf.PortfolioId,
                UserId = pf.ClientId,
                S = pf.Sum,
                SL = pf.SL,
                TP = pf.TP,
                CloseDate = pf.CloseDate,
                PortfolioPositions = pf.Positions.Select(pos =>
                    {
                        var split = pos.Security.Split(' ');
                        if (split.Length != 3) throw new Exception($"Invalid security key '{pos.Security}' when creating timed portfolio id {pf.Id}");

                        return new TimedPortfolioPosition
                        {
                            Security = new Security
                            {
                                Symbol = pos.Security.Split(' ')[0],
                                ClassCode = pos.Security.Split(' ')[1],
                                Board = pos.Security.Split(' ')[2]
                            },
                            Weight = pos.Weight
                        };
                    }
                ).ToList()
            }, cancellationToken: cancellationToken);

            if (result.Success)
                return result.Success;

            throw new Exception(result.Message);
        }

        public async Task<bool> CloseAllPortfolios(Guid agreementId, CancellationToken cancellationToken)
        {
            var result = await PostRequestAsync<JsonResult>("portfolios/closeall", agreementId, cancellationToken: cancellationToken);

            if (result.Success)
                return result.Success;

            throw new Exception(result.Message);
        }

        public async Task<bool> PushToCS(Guid strategyId)
        {
            var result = await PostRequestAsync<JsonResult>("signals/pushtocs", strategyId);

            if (result.Success)
                return result.Success;

            throw new Exception(result.Message);
        }

        public async Task<bool> SetUsers(User[] users)
        {
            var result = await PostRequestAsync<JsonResult>("users/set", users);

            if (result.Success)
                return result.Success;

            throw new Exception(result.Message);
        }

        public async Task<SecurityRecordDTO[]> QueryLastSignalAsync(Guid strategyId)
        {
            var result = await PostRequestAsync<StandardResponse<SecurityRecordDTO[]>>("signals/last/securities",
                strategyId);
            if (result.Success)
                return result.Result;

            throw new Exception(result.Message);
        }
    }
}