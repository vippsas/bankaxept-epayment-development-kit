using System;
using System.Reactive.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using BankAxept.Epayment.Client.Base;
using BankAxept.Epayment.Client.TokenRequestor.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BankAxept.Epayment.Client.TokenRequestor {

    public class TokenRequestorClient {

        private readonly BaseClient baseClient;
        private readonly JsonSerializerSettings jsonSettings;

        public TokenRequestorClient(BaseClient client) {
            baseClient = client;
            jsonSettings = new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        public TokenRequestorClient (
            string authorizationServerUrl,
            string resourceServerUrl,
            string clientId,
            string clientSecret
        ) : this(
            new BaseClient.Builder(resourceServerUrl)
                .WithScheduledToken(authorizationServerUrl, clientId, clientSecret)
                .Build()
        ) {}

        public IObservable<RequestStatus> EnrolCard(EnrolCardRequest request, string correlationId) {
            var json = JsonConvert.SerializeObject(request, jsonSettings);
            return Observable.FromAsync(() => baseClient.Post("/v1/payment-tokens", json, correlationId));
        }

        public Task<RequestStatus> EnrolCardAsync(EnrolCardRequest request, string correlationId) {
            var json = JsonConvert.SerializeObject(request, jsonSettings);
            return baseClient.Post("/v1/payment-tokens", json, correlationId);
        }

        public IObservable<RequestStatus> DeleteToken(string tokenId, string correlationId) {
            return Observable.FromAsync(() => baseClient.Delete($"/v1/payment-tokens/{tokenId}/deletion", correlationId));
        }

        public Task<RequestStatus> DeleteTokenAsync(string tokenId, string correlationId) {
            return baseClient.Delete($"/v1/payment-tokens/{tokenId}/deletion", correlationId);
        }

        public IObservable<RequestStatus> EligibleBanks(List<string> bankIdentifiers) {
            var requestPath = $"/v1/eligible-banks?bankIdentifier={string.Join(",", bankIdentifiers)}";
            return Observable.FromAsync(() => baseClient.Get(requestPath));
        }

        public Task<RequestStatus> EligibleBanksAsync(List<string> bankIdentifiers) {
            var requestPath = $"/v1/eligible-banks?bankIdentifier={string.Join(",", bankIdentifiers)}";
            return baseClient.Get(requestPath);
        }
    }
}