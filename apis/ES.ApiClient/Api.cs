using System;
using System.Collections.Generic;
using ES.ApiClient.Dto;
using ExchangeHelpers;
using NLib.Http;
using NLib.ParsersFormatters;

namespace ES.ApiClient
{
    public class QueryClientBranchInfoDto
    {
        public Guid? AgreementId { get; set; }

        public string ClientCode { get; set; }

        public Guid? StrategyId { get; set; }
    }

    public class Api : BaseHttpClient
    {
        public Api(string serverHost, int serverPort) : base(serverHost, serverPort, TimeSpan.FromSeconds(30), new JsonFormatterFactory(), new JsonParser())
        {
        }

        public void PostStrategy(StrategyInfoTO strategy)
        {
            var result = PostRequest<StandardResponse<StrategyInfoTO[]>>("strategy/info", new[] { strategy });

            if (!result.Success)
                throw new Exception(result.Message);
        }

        public StrategyResultDto[] PostStrategy(StrategyInfoTO[] strategy)
        {
            var result = PostRequest<StandardResponse<StrategyResultDto[]>>("strategy/info", strategy);

            if (!result.Success && result.Result == null)
                throw new Exception(result.Message);

            return result.Result;
        }

        public void PostStrategyManagers(StrategyManagerTO[] managers)
        {
            var result = PostRequest<StandardResponse<StrategyManagerTO[]>>("strategy/managers", managers);
            if (!result.Success) throw new Exception(result.Message);
        }

        public void PostPortfolios(PortfolioInfoTO[] portfolios)
        {
            var result = PostRequest<StandardResponse>("portfolio/list", portfolios);

            if (!result.Success)
                throw new Exception(result.Message);
        }

        public void PostPortfolioExecutions(PortfolioExecutionTO[] executions)
        {
            var result = PostRequest<StandardResponse>("portfolio/execution", executions);

            if (!result.Success)
                throw new Exception(result.Message);
        }

        public void PostPortfolioLeftovers(IEnumerable<PortfolioExecutionLeftoversTO> leftovers)
        {
            var result = PostRequest<StandardResponse>("portfolio/leftovers", leftovers);

            if (!result.Success)
                throw new Exception(result.Message);
        }

        public Guid? GetClientTariff(Guid agreementId)
        {
            var result = PostRequest<StandardResponse<Guid?>>("exports/tariffid", agreementId);
            if (!result.Success)
                //throw new Exception(result.Message);
                return null;

            return result.Result;
        }

        public BranchInfoTO GetClientAccoutReportData(string clientCode, Guid? agreementId, Guid? strategyId)
        {
            var response = PostRequest<StandardResponse<BranchInfoTO>>("clients/branchinfo",
                new QueryClientBranchInfoDto
                {
                    ClientCode = clientCode,
                    AgreementId = agreementId,
                    StrategyId = strategyId
                });

            if (!response.Success)
                throw new Exception(response.Message);

            return response.Result;
        }

        public MessageHistoryTO GetMessageHistory(Guid agreementId, MessageHistoryTO.MessageType msgType)
        {
            var result = PostRequest<StandardResponse<MessageHistoryTO>>("/history/diffs",
                new MessageHistoryTO { AgreementId = agreementId, Type = msgType });
            if (!result.Success)
                throw new Exception(result.Message);

            return result.Result;
        }

        public MessageHistoryTO GetClientInfo(string contractNum, Guid? agreementId = null)
        {
            if (string.IsNullOrEmpty(contractNum) || !agreementId.HasValue)
                throw new ArgumentNullException("agreementId or contractNum");

            var result = PostRequest<StandardResponse<MessageHistoryTO>>("/history/info",
                new MessageHistoryTO { AgreementId = agreementId, Contract = contractNum });
            if (!result.Success)
                throw new Exception(result.Message);

            return result.Result;
        }
    }
}