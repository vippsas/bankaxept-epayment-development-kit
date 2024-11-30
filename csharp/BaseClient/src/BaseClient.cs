using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using BankAxept.Epayment.Client.Base.AccessToken;

namespace BankAxept.Epayment.Client.Base {

    public class BaseClient  {
        
        private static readonly TimeSpan tokenTimeout = TimeSpan.FromSeconds(10);
        private readonly HttpClient client;
        private readonly IAccessTokenPublisher tokenPublisher;
        
        public BaseClient(HttpClient httpClient, IAccessTokenPublisher tokenPublisher) {
            client = httpClient;
            this.tokenPublisher = tokenPublisher;
        }

        private HttpRequestHeaders FilterHeaders(HttpRequestHeaders headers, string correlationId, bool hasBody) {
            if (correlationId != null) {
                headers.Add("X-Correlation-Id", correlationId);
            }

            headers.Add("Authorization", $"Bearer {new AccessTokenSubscriber(tokenPublisher).Get(tokenTimeout)}");
            return headers;
        }

        public async Task<RequestStatus> Get(string uri) {
            var requestMessage = new HttpRequestMessage (HttpMethod.Get, uri);
            FilterHeaders(requestMessage.Headers, null, false);
            var response = await client.SendAsync(requestMessage);
            var body = await response.Content.ReadAsStringAsync();
            return RequestStatusParser.Parse(response.StatusCode, body);
        }

        public async Task<RequestStatus> Post(string uri, string body, string correlationId) {
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var requestMessage = new HttpRequestMessage {
                Method = HttpMethod.Post,
                RequestUri = new Uri($"{client.BaseAddress}{uri}"),
                Content = content
            };
            
            FilterHeaders(requestMessage.Headers, correlationId, true);
            var response = await client.SendAsync(requestMessage);
            var responseBody = await response.Content.ReadAsStringAsync();
            return RequestStatusParser.Parse(response.StatusCode, responseBody);
        }

        public async Task<RequestStatus> Put(string uri, string body, string correlationId) {
            var content = new StringContent(body, Encoding.UTF8, "application/json");
            var requestMessage = new HttpRequestMessage {
                Method = HttpMethod.Post,
                RequestUri = new Uri(uri, UriKind.Relative),
                Content = content
            };

            FilterHeaders(requestMessage.Headers, correlationId, true);
            var response = await client.SendAsync(requestMessage);
            var responseBody = await response.Content.ReadAsStringAsync();
            return RequestStatusParser.Parse(response.StatusCode, responseBody);
        }

        public async Task<RequestStatus> Delete(string uri, string correlationId) {
            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, uri);
            FilterHeaders(requestMessage.Headers, correlationId, false);
            var response = await client.SendAsync(requestMessage);
            var body = await response.Content.ReadAsStringAsync();
            return RequestStatusParser.Parse(response.StatusCode, body);
        }

        public class Builder {
            private readonly HttpClient client;
            private IAccessTokenPublisher tokenPublisher;

            public Builder(string resourceServerUrl) {
                var baseAddress = resourceServerUrl.TrimEnd('/');
                client = new HttpClient {
                    BaseAddress = new Uri(baseAddress)
                };
            }

            public Builder WithScheduledToken(string authorizationServerUrl, string clientId, string clientSecret) {
                tokenPublisher =
                    new ScheduledAccessTokenPublisher.Builder(new HttpClient())
                        .Url(new Uri(authorizationServerUrl))
                        .ClientCredentials(clientId, clientSecret)
                        .GrantType("client_credentials")
                        .Build();
                
                return this;
            }

            public BaseClient Build() {
                return new BaseClient(client, tokenPublisher);
            }

            //public Builder WithStaticToken(string token) {
            //    TokenPublisher = new StaticAccessTokenPublisher(token);
            //}

            // public Builder WithScheduledToken(Uri authorizationServerUrl, string id, string secret) {
            //     tokenPublisher = new ScheduledAccessTokenPublisher(client, authorizationServerUrl);
            //     return this;
            // }

            // public BaseClient Build() {
            //     return new BaseClient(Client, TokenPublisher);
            // }            
        }
    }
}