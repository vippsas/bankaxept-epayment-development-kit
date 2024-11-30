using System;
using System.Reactive.Linq;
using Newtonsoft.Json;
using BankAxept.Epayment.Client.Wallet.Model;
using BankAxept.Epayment.Client.Base;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;

namespace BankAxept.Epayment.Client.Wallet {
    public class WalletClient {
        private readonly BaseClient baseClient;
        private readonly JsonSerializerSettings jsonSettings;

        private WalletClient(BaseClient client) {
            baseClient = client;
            jsonSettings = new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        public WalletClient(
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

        public IObservable<RequestStatus> DeleteToken(Guid tokenId, string correlationId) {
            return Observable.FromAsync(() => baseClient.Delete($"/v1/payment-tokens/{tokenId}", correlationId));
        }

        public Task<RequestStatus> DeleteTokenAsync(Guid tokenId, string correlationId) {
            return baseClient.Delete($"/v1/payment-tokens/{tokenId}", correlationId);
        }

        public IObservable<RequestStatus> RequestPayment(PaymentRequest request, string correlationId) {
            var json = JsonConvert.SerializeObject(request, jsonSettings);
            return Observable.FromAsync(() => baseClient.Post("/v1/payments", json, correlationId));
        }

        public Task<RequestStatus> RequestPaymentAsync(PaymentRequest request, string correlationId) {
            var json = JsonConvert.SerializeObject(request, jsonSettings);
            return baseClient.Post("/v1/payments", json, correlationId);
        }
    }
}