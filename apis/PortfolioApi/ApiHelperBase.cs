using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using NLib.Authorization;
using NLib.AuxTypes;
using NLib.Config;
using NLib.Logger;

namespace PortfolioApi
{
    public class ApiHelperBase : IDisposable
    {
        private readonly IOAuthManager authManager;
        private readonly HttpClient httpClient;
        protected readonly ILogger logger;
        private readonly TimeSpan timeOut;
        private readonly bool traceRawResponses;

        public ApiHelperBase(string url, IOAuthManager mgr, TimeSpan timeOut, ILogger logger, bool traceRawResponses = false)
        {
            this.logger = logger;
            authManager = mgr;
            httpClient = new HttpClient();
            ApiUrl = url;
            httpClient.BaseAddress = new Uri(url);
            httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PortfolioApiHelper", "1.0"));
            this.timeOut = timeOut;
            this.traceRawResponses = traceRawResponses;
        }

        public ApiHelperBase(IConfig config, IOAuthManager mgr, ILogger logger)
        {
            this.logger = logger;
            authManager = mgr;
            httpClient = new HttpClient();

            var typedConfig = new ApiHelperConfig(config, BaseUrlConfigParamName);

            ApiUrl = typedConfig.ApiHelperBaseAddress;
            httpClient.BaseAddress = new Uri(typedConfig.ApiHelperBaseAddress);
            timeOut = typedConfig.ApiHelperRequestTimeout;
            traceRawResponses = typedConfig.TraceRawResponses;
            logger.Trace($"Staring {GetType().Name} on {ApiUrl}");
        }

        public string ApiUrl { get; }

        protected virtual string BaseUrlConfigParamName => throw new NotImplementedException();

        public void Dispose()
        {
            httpClient?.Dispose();
        }

        protected T Get<T>(string method, /*KeyValuePair<string, string>[] */ string queryParams, Func<string, T> responseParser)
        {
            var uri = method;
            if (!string.IsNullOrEmpty(queryParams)) uri += '?' + queryParams;

            logger?.Trace($"Invoking GET {uri}");
            if (!(authManager?.IsAuthorized ?? true))
                throw new ApiException(ErrorStage.Channel, "Not Authorized", ApiException.NotAuthorized);

            try
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri);
                httpRequestMessage.Headers.Authorization = authManager != null ? new AuthenticationHeaderValue("Bearer", authManager.GetTokenValue()) : null;

                var response = httpClient.SendAsync(httpRequestMessage);
                if (!response.Wait((int)timeOut.TotalMilliseconds))
                    //throw new ApiException("GET", "Request timeout");
                    throw new UserVisibleException("Превышено время ожидания ответа от информационной системы");

                var message = response.Result.Content.ReadAsStringAsync().Result;
                if (traceRawResponses) logger?.Trace($"Response:\n{message}");

                if (!response.Result.IsSuccessStatusCode)
                {
                    if (response.Result.StatusCode == HttpStatusCode.GatewayTimeout)
                        throw new UserVisibleException("Превышено время ожидания ответа от информационной системы");
                    throw new ApiException(ErrorStage.Channel,
                        $"Response code={response.Result.StatusCode} message=|{message ?? "empty"}|rq={httpClient.BaseAddress}{uri}",
                        ApiException.BadHttpStatus);
                }

                return responseParser(message);
            }
            catch (Exception e) when (!(e is ApiException))
            {
                logger?.Error(e, $"Get|{method}|{e.Message}");

                throw new ApiException(ErrorStage.Channel, e.Message, ApiException.Unknown, e);
            }
        }

        protected T Post<T>(string method, string requestJson, Func<string, T> responseParser)
        {
            logger?.Trace($"Invoking POST {method}");

            if (!(authManager?.IsAuthorized ?? true))
                throw new ApiException(ErrorStage.Channel, "Not Authorized", ApiException.NotAuthorized);

            try
            {
                var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, method);
                httpRequestMessage.Headers.Authorization = authManager != null ? new AuthenticationHeaderValue("Bearer", authManager.GetTokenValue()) : null;
                httpRequestMessage.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

                var response = httpClient.SendAsync(httpRequestMessage);
                if (!response.Wait((int)timeOut.TotalMilliseconds))
                    //throw new ApiException("POST", "Request Timeout");
                    throw new UserVisibleException("Превышено время ожидания ответа от информационной системы");

                var payload = response.Result.Content.ReadAsStringAsync().Result;
                if (traceRawResponses)
                {
                    logger?.Trace($"Request:\n{requestJson}");
                    logger?.Trace($"Response:\n{payload}");
                }

                if (!response.Result.IsSuccessStatusCode)
                {
                    if (response.Result.StatusCode == HttpStatusCode.GatewayTimeout)
                        throw new UserVisibleException("Превышено время ожидания ответа от информационной системы");
                    if (response.Result.StatusCode == (HttpStatusCode)422)
                        try
                        {
                            var pl = JsonConvert.DeserializeObject<AutofollowApiResponse>(payload);
                            throw new UserVisibleException(pl.userMsg);
                        }
                        catch (Exception ex) when (!(ex is UserVisibleException))
                        {
                        }

                    throw new ApiException(ErrorStage.Channel,
                        $"Response code={response.Result.StatusCode} message=|{payload ?? "empty"}|", ApiException.BadHttpStatus);
                }

                if (responseParser != null)
                    return responseParser(payload);
                return default;
            }
            catch (Exception e) when (!(e is UserVisibleException || e is ApiException))
            {
                logger?.Error(e, $"POST|{method}|{e.Message}");

                throw new ApiException(ErrorStage.Channel, e.Message, ApiException.Unknown, e);
            }
        }

        protected T Patch<T>(string method, string requestJson, Func<string, T> responseParser)
        {
            logger.Trace($"Invoking PATCH {method}");

            if (!(authManager?.IsAuthorized ?? true))
                throw new ApiException(ErrorStage.Channel, "Not Authorized", ApiException.NotAuthorized);

            try
            {
                var httpMethod = new HttpMethod("PATCH");
                var request = new HttpRequestMessage(httpMethod, method)
                {
                    Content = new StringContent(requestJson, Encoding.UTF8, "application/json")
                };
                request.Headers.Authorization = authManager != null ? new AuthenticationHeaderValue("Bearer", authManager.GetTokenValue()) : null;

                //var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
                var response = httpClient.SendAsync(request);

                if (!response.Wait((int)timeOut.TotalMilliseconds))
                    //throw new ApiException("PATCH", "Request Timeout");
                    throw new UserVisibleException("Превышено время ожидания ответа от информационной системы");

                var payload = response.Result.Content.ReadAsStringAsync().Result;
                if (traceRawResponses)
                {
                    logger?.Trace($"Request:\n{requestJson}");
                    logger?.Trace($"Response:\n{payload}");
                }

                if (!response.Result.IsSuccessStatusCode)
                {
                    if (response.Result.StatusCode == HttpStatusCode.GatewayTimeout)
                        throw new UserVisibleException("Превышено время ожидания ответа от информационной системы");
                    throw new ApiException(ErrorStage.Channel,
                        $"Response code={response.Result.StatusCode} message=|{payload ?? "empty"}|", ApiException.BadHttpStatus);
                }

                return responseParser(payload);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception e)
            {
                logger?.Error(e, $"PATCH|{method}|{e.Message}");

                throw new ApiException(ErrorStage.Channel, e.Message, ApiException.Unknown, e);
            }
        }

        protected void Delete(string method, string requestJson)
        {
            logger.Trace($"Invoking DELETE {method}");

            if (!(authManager?.IsAuthorized ?? true))
                throw new ApiException(ErrorStage.Channel, "Not Authorized", ApiException.NotAuthorized);

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Delete, method);
                if (!string.IsNullOrEmpty(requestJson)) request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                request.Headers.Authorization = authManager != null ? new AuthenticationHeaderValue("Bearer", authManager.GetTokenValue()) : null;

                var response = httpClient.SendAsync(request);
                if (!response.Wait((int)timeOut.TotalMilliseconds))
                    throw new UserVisibleException("Превышено время ожидания ответа от информационной системы");
                //throw new ApiException("DELETE", "Request Timeout");

                var payload = response.Result.Content.ReadAsStringAsync().Result;
                if (traceRawResponses)
                {
                    logger?.Trace($"Request:\n{requestJson}");
                    logger?.Trace($"Response:\n{payload}");
                }

                if (!response.Result.IsSuccessStatusCode)
                {
                    if (response.Result.StatusCode == HttpStatusCode.GatewayTimeout)
                        throw new UserVisibleException("Превышено время ожидания ответа от информационной системы");
                    if (response.Result.StatusCode == (HttpStatusCode)422)

                        try
                        {
                            var pl = JsonConvert.DeserializeObject<AutofollowApiResponse>(payload);
                            throw new UserVisibleException($"Payload: {pl.userMsg}");
                        }
                        catch (Exception ex) when (!(ex is UserVisibleException))
                        {
                        }

                    throw new ApiException(ErrorStage.Channel,
                        $"Response code={response.Result.StatusCode} message=|{payload ?? "empty"}|rq={httpClient.BaseAddress}{method}",
                        ApiException.BadHttpStatus);
                }
            }
            catch (Exception e) when (!(e is UserVisibleException || e is ApiException))
            {
                logger?.Error(e, $"DELELE|{method}|{e.Message}");

                throw new ApiException(ErrorStage.Channel, e.Message, ApiException.Unknown, e);
            }
        }

        public T SimpleParser<T>(string response)
        {
            return JsonConvert.DeserializeObject<T>(response);
        }

        public T ResponseParser<T>(string response, JsonSerializerSettings settings)
        {
            if (string.IsNullOrEmpty(response))
                throw new ApiException(ErrorStage.Application, "Empty response", ApiException.EmptyResponse);

            try
            {
                var result = JsonConvert.DeserializeObject<T>(response, settings);

                return result;
            }
            catch (JsonException e)
            {
                throw new ApiException(ErrorStage.Application, e.Message, ApiException.Unknown, e);
            }
        }

        public T ResponseParser<T>(string response)
        {
            return ResponseParser<T>(response, null);
        }

        protected void AddArgIfNotEmpty(List<string> args, string argName, object argValue)
        {
            if (argValue != null && argValue.ToString() != "") args.Add($"{argName}={HttpUtility.UrlEncode(argValue.ToString())}");
        }

        public class AutofollowApiResponse
        {
            public string techMsg { get; set; }
            public string userMsg { get; set; }
            public string timestamp { get; set; }
        }
    }

    internal class ApiHelperConfig
    {
        public ApiHelperConfig(IConfig config, string baseUrlConfigParamName)
        {
            ApiHelperRequestTimeout = (TimeSpan)config.Get(nameof(ApiHelperRequestTimeout));
            ApiHelperBaseAddress = (string)config.Get(baseUrlConfigParamName);
            TraceRawResponses = (bool)config.Get(nameof(TraceRawResponses));
        }

        public bool TraceRawResponses { get; set; }
        public TimeSpan ApiHelperRequestTimeout { get; set; }
        public string ApiHelperBaseAddress { get; set; }
    }
}