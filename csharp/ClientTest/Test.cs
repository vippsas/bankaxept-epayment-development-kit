using System;
using BankAxept.Epayment.Client.Wallet.Model;

namespace BankAxept.Epayment.Client.Test {
    

    public static class Helpers {

        public static string EnvVarOrDefault (string envVar, string defaultVal) {
            if (Environment.GetEnvironmentVariable(envVar)!= null ) {
                return Environment.GetEnvironmentVariable(envVar);
            }
            return defaultVal;
        }

        public static string EnvVarOrThrow(string envVar) {
            if (Environment.GetEnvironmentVariable(envVar)!= null ) {
                return Environment.GetEnvironmentVariable(envVar);
            }
            throw new NullReferenceException($"Could not read {envVar} from environment");
        }
        
        public static string CorrelationId() {
            return Guid.NewGuid().ToString();
        }

        public static string MessageId() {
            return Guid.NewGuid().ToString();
        }

        public static PaymentRequest CreatePaymentRequest() {
            var payment = new PaymentData(
                amount: new Amount(currency: "NOK", value: 12345),
                merchantName: "Nærbutikken",
                merchantDisplayName: "Nærbutikken",
                merchantReference: "7c26a49c-e489-4e57-b94e-dd188cb594f3"
            );
            return new PaymentRequest (
                messageId: MessageId(),
                merchantAggregatorId: "localhost-aggregator",
                paymentData: payment,
                tokenId: Guid.NewGuid(),
                multiClient: false

            );
        }
    }
}

