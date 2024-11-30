using System;
using NUnit.Framework;
using BankAxept.Epayment.Client.Wallet.Model;
using BankAxept.Epayment.Client.Wallet;
using BankAxept.Epayment.Client.Base;

namespace BankAxept.Epayment.Client.Test.Wallet {

    [TestFixture]
    public class WalletClientTest {

        public required WalletClient walletClient;
        
        [SetUp]
        public void Setup() {
            walletClient =
                new WalletClient(
                    Helpers.EnvVarOrDefault("AUTH_SERVER_URL", "http://localhost:8001/v1/accesstoken"),
                    Helpers.EnvVarOrDefault("RESOURCE_SERVER_URL", "http://localhost:8004/token-requestor"),
                    Helpers.EnvVarOrThrow("AUTH_SERVER_CLIENT_ID"),
                    Helpers.EnvVarOrThrow("AUTH_SERVER_CLIENT_SECRET")
                );
        }

        [Test]
        public async Task PaymentRequest() {
            var result = await walletClient.RequestPaymentAsync(Helpers.CreatePaymentRequest(), Helpers.CorrelationId());

            Assert.That(result, Is.EqualTo(RequestStatus.Accepted));
        }
    }
}