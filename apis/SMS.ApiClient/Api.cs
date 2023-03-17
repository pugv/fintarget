using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ExchangeHelpers;
using NLib.Http;
using NLib.ParsersFormatters;

namespace SMS.ApiClient
{
    public class Api
        : BaseHttpClient
    {
        public Api(string serverUrl, int serverPort)
            : base(serverUrl, serverPort, TimeSpan.FromSeconds(30), new JsonFormatterFactory(), new JsonParser())
        {
        }

        public Api(HttpClient client)
            : base(client, TimeSpan.FromSeconds(30), new JsonFormatterFactory(), new JsonParser())
        {
        }

        public Guid SendMessages(Message[] messages)
        {
            var result = PostRequest<StandardResponse<Guid>>("messages/send", messages);

            if (result.Success)
                return result.Result;

            throw new Exception(result.Message);
        }

        public async Task<Guid> SendMessagesAsync(Message[] messages, CancellationToken cancellationToken = default)
        {
            var result = await PostRequestAsync<StandardResponse<Guid>>("messages/send", messages, cancellationToken: cancellationToken);

            if (result.Success)
                return result.Result;

            throw new Exception(result.Message);
        }

        public async Task<Guid> SendEmailsAsync(Email[] messages, CancellationToken cancellationToken = default)
        {
            var result = await PostRequestAsync<StandardResponse<Guid>>("email/send", messages, cancellationToken: cancellationToken);

            if (result.Success)
                return result.Result;

            throw new Exception(result.Message);
        }

        public async Task<Guid> SendTemplateEmailAsync(TemplateEmailDto templateEmail, CancellationToken cancellationToken = default)
        {
            var result = await PostRequestAsync<StandardResponse<Guid>>("templateEmail/send", templateEmail, cancellationToken: cancellationToken);

            if (result.Success)
                return result.Result;

            throw new Exception(result.Message);
        }

        public async Task<string> GenerateIIRPdfBase64Async(long signalIdId, CancellationToken cancellationToken = default)
        {
            var result = await PostRequestAsync<StandardResponse<string>>("iir/generate_pdf", signalIdId, cancellationToken: cancellationToken);

            if (result.Success)
                return result.Result;

            throw new Exception(result.Message);
        }

        public async Task SetAccountSendingTypesAsync(string clientCode, SendTypes sendTypes, CancellationToken cancellationToken)
        {
            var result = await PostRequestAsync<StandardResponse<bool>>("account/setSendingTypes", new SetAccountSendTypesRequest
            {
                ClientCode = clientCode,
                SendTypes = sendTypes
            }, cancellationToken: cancellationToken);

            if (!result.Success)
                throw new Exception(result.Message);
        }
    }
}