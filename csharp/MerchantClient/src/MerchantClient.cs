using System;
using Newtonsoft.Json;
using BankAxept.Epayment.Client.Base;
using BankAxept.Epayment.Client.Merchant.Model;
using System.Reactive.Linq;
using Newtonsoft.Json.Serialization;
using System.Threading.Tasks;

namespace BankAxept.Epayment.Client.Merchant {

    public class MerchantClient {

        private readonly BaseClient baseClient;
        private readonly JsonSerializerSettings jsonSettings;
        
        public MerchantClient(BaseClient client) {
            baseClient = client;
            jsonSettings = new JsonSerializerSettings {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
        }

        public MerchantClient(
            string authorizationServerUrl,
            string resourceServerUrl,
            string clientId,
            string clientSecret
        ) : this(
            new BaseClient.Builder(resourceServerUrl)
                .WithScheduledToken(authorizationServerUrl, clientId, clientSecret)
                .Build()
        ) {}

        public IObservable<RequestStatus> RequestPayment(PaymentRequest request, string correlationId) {
            var json = JsonConvert.SerializeObject(request, jsonSettings);
            return Observable.FromAsync(() => baseClient.Post("/v1/payments", json, correlationId));
        }

        public Task<RequestStatus> RequestPaymentAsync(PaymentRequest request, string correlationId) {
            var json = JsonConvert.SerializeObject(request, jsonSettings);
            return baseClient.Post("/v1/payments", json, correlationId);
        }

        public IObservable<RequestStatus> RollbackPayment(string correlationId, string messageId) {
            return Observable.FromAsync(() => baseClient.Delete($"/v1/payments/messages/{messageId}", correlationId));
        }

        public Task<RequestStatus> RollbackPaymentAsync(string correlationId, string messageId) {
            return baseClient.Delete($"/v1/payments/messages/{messageId}", correlationId);
        }

        public IObservable<RequestStatus> CapturePayment(string paymentId, CaptureRequest request, string correlationId) {
            var json = JsonConvert.SerializeObject(request, jsonSettings);
            return Observable.FromAsync(() => baseClient.Post($"/v1/payments/{paymentId}/captures", json, correlationId));
        }

        public Task<RequestStatus> CapturePaymentAsync(string paymentId, CaptureRequest request, string correlationId) {
            var json = JsonConvert.SerializeObject(request, jsonSettings);
            return baseClient.Post($"/v1/payments/{paymentId}/captures", json, correlationId);
        }

        public IObservable<RequestStatus> CancelPayment(string paymentId, string correlationId) {
            return Observable.FromAsync(() => baseClient.Post($"/v1/payments/{paymentId}/cancellation", "", correlationId));
        }

        public Task<RequestStatus> CancelPaymentAsync(string paymentId, string correlationId) {
            return baseClient.Post($"/v1/payments/{paymentId}/cancellation", "", correlationId);
        }

        public IObservable<RequestStatus> RefundPayment(string paymentId, RefundRequest request, string correlationId) {
            var json = JsonConvert.SerializeObject(request, jsonSettings);
            return Observable.FromAsync(() => baseClient.Post($"/v1/payments/{paymentId}/refunds", json, correlationId));
        }

        public Task<RequestStatus> RefundPaymentAsync(string paymentId, RefundRequest request, string correlationId) {
            var json = JsonConvert.SerializeObject(request, jsonSettings);
            return baseClient.Post($"/v1/payments/{paymentId}/refunds", json, correlationId);
        }

        public IObservable<RequestStatus> CutOff(
            string merchantId, CutOffRequest request,
            string batchNumber, string correlationId
        ) {
            var json = JsonConvert.SerializeObject(request, jsonSettings);
            return Observable.FromAsync(() => baseClient.Put($"/v1/settlements/{merchantId}/{batchNumber}", json, correlationId));
        }

        public Task<RequestStatus> CutOffAsync(
            string merchantId, CutOffRequest request,
            string batchNumber, string correlationId
        ) {
            var json = JsonConvert.SerializeObject(request, jsonSettings);
            return baseClient.Put($"/v1/settlements/{merchantId}/{batchNumber}", json, correlationId);
        }

        public IObservable<RequestStatus> RollbackRefund(string paymentId, string messageId, string correlationId) {
            return Observable.FromAsync(() => baseClient.Delete($"/v1/payments/{paymentId}/refunds/messages/{messageId}", correlationId));
        }

        public Task<RequestStatus> RollbackRefundAsync(string paymentId, string messageId, string correlationId) {
            return baseClient.Delete($"/v1/payments/{paymentId}/refunds/messages/{messageId}", correlationId);
        }
    }
}