using System;
using System.Web;
using BalancerModels;
using Newtonsoft.Json;
using NLib.Authorization;
using NLib.Logger;
//using ServiceBase;

namespace PortfolioApi
{
    public class Api : ApiHelperBase
    {
        public Api(IOAuthManager auth, string apiBaseUrl, ILogger logger, bool traceRawResponses = false)
            : base(apiBaseUrl, auth, TimeSpan.FromSeconds(30), logger, traceRawResponses)
        {
        }

        public PortfolioDto[] GetPortfolios()
        {
            return Get("portfolio-service/v1/portfolios", null, ParseRequestResult<PortfolioDto[]>);
        }

        private T ParseRequestResult<T>(string c)
        {
            var result = JsonConvert.DeserializeObject<RequestResult<T>>(c);
            if (!result.Success) throw new ApiException(ErrorStage.Application, result.ErrorMessage, result.ErrorCode);
            return result.Result;
        }
        /*
        public Portfolio GetPortfolio(Guid portfolioId)
        {
            return Get<Portfolio>($"portfolio-service/v1/portfolio/{portfolioId}", null, ParseRequestResult<Portfolio>);
        }*/

        public BalancedPortfolio Balance(Guid portfolioId, decimal freeFunds)
        {
            return Get("balancer-service/v1/balance",
                FormattableString.Invariant($"portfolioId={portfolioId}&freeFunds={freeFunds}"),
                ParseRequestResult<BalancedPortfolio>);
        }

        public ShortClientAccount[] GetClientAccounts()
        {
            return Get("balancer-service/v1/clientAccounts/", null, ParseRequestResult<ShortClientAccount[]>);
        }

        public BalancedPortfolio BalanceWithWeights(Guid portfolioId, decimal freeFunds, decimal expextedValue, WeightedPosition[] positions)
        {
            return Post("balancer-service/v1/balance-weighted",
                JsonConvert.SerializeObject(
                    new WeightedBalanceRequest
                    {
                        PortfolioId = portfolioId,
                        FreeFunds = freeFunds,
                        ExpectedValue = expextedValue,
                        Positions = positions
                    }),
                ParseRequestResult<BalancedPortfolio>);
        }

        public BalancedPortfolio CalcWeighted(Guid portfolioId, decimal freeFunds, CalcWeightedPosition[] positions)
        {
            return Post("balancer-service/v1/calc-weighted",
                JsonConvert.SerializeObject(
                    new CalcWeightedRequest
                    {
                        PortfolioId = portfolioId,
                        FreeFunds = freeFunds,
                        Positions = positions
                    }),
                ParseRequestResult<BalancedPortfolio>);
        }

        public Guid CreateExecuteRequest(Guid portfolioId, string packetOrderId, Guid agreementId, ExecuteRequestPosition[] positions)
        {
            return Post("balancer-service/v1/create-execution-request",
                JsonConvert.SerializeObject(
                    new CreateExecutionRequest
                    {
                        PortfolioId = portfolioId,
                        AgreementId = agreementId,
                        Positions = positions
                    }),
                ParseRequestResult<Guid>);
        }

        public bool ResendSms(Guid executionRequestId)
        {
            return Post($"balancer-service/v1/resend-sms?executionRequestId={executionRequestId}",
                "",
                ParseRequestResult<bool>);
        }

        public void ConfirmExecuteRequest(Guid executionRequestId, string smsCode)
        {
            Post($"balancer-service/v1/confirm-execution-request?executionRequestId={executionRequestId}&smsCode={HttpUtility.UrlEncode(smsCode)}",
                "",
                ParseRequestResult<bool>);
        }

        public GenerateFormResponse GenerateForm(Guid executionRequestId, FormFormat formFormat)
        {
            return Post($"balancer-service/v1/generate-form?executionRequestId={executionRequestId}&formFormat={formFormat}",
                "",
                ParseRequestResult<GenerateFormResponse>);
        }

        public CheckExecutionReportResponse CheckExecutionRequest(Guid executionRequestId)
        {
            return Get($"balancer-service/v1/check-execution-request?executionRequestId={executionRequestId}",
                "",
                ParseRequestResult<CheckExecutionReportResponse>);
        }
    }

    public class RequestResult<T>
    {
        /// <summary>
        ///     Возвращаемое значение
        /// </summary>
        public T Result { get; set; }

        /// <summary>
        ///     Флаг успешной обработки, если true - возвращаемое значение в <see cref="Result" />, если false - текст ошибки в <see cref="ErrorMessage" />
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        ///     Текст сообщения об ошибке
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        ///     Код ошибки
        /// </summary>
        public int ErrorCode { get; set; }
    }
}