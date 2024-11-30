using System;
using System.Reactive.Linq;
using BankAxept.Epayment.Client.Base;
using BankAxept.Epayment.Client.TokenRequestor;
using BankAxept.Epayment.Client.TokenRequestor.Model;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace BankAxept.Epayment.Client.Test.TokenRequestor {
    
    [TestFixture]
    public class TokenRequestorTest {


        public required TokenRequestorClient tokenRequestorClient;
        private static readonly string encryptedExampleData = "eyJlbmMiOiJBMjU2Q0JDLUhTNTEyIiwiYWxnIjoiUlNBLU9BRVAtMjU2In0.f5nkE6FuGYkoa4usRQ1MhUJY34pYi31xgSiApiR1uP4tSXV3DNnY3N5Zq9Bnt1OucN2nJxAqCcND4G8TpGw9kofFcLcs5kXHg7nmIgjI8ZXTYx7GuZ_w6YxVTzCmjT5dpSlOQFkuCfJn2VdKnF4PjaqiKW9fWluOKorUZdsjsDl5PjIjf3ndqCtGEma6TBpKxLX0FnCZzvsVATCBcxqwKLvkAYFdFFtLfxe5OvW0PFsy4OjasODW3Kk55e58v5xXB8bP9hzr5S7sXFlzX2TG583MLLXG3K1E3XG0R262vs2cGgSA1B6zmujvmkpR4lLofwgahpO-ZrhGZtXE0-wFJw.NDB8Ln7XCf1q1p6ddRvnSw.PTBsKUkN5stmSQwrQ-jQLA.ece3W1q3AiMdg5QQbAd1tq_nQWLRkyNnk2mL1TP8fpQ";
        private static readonly string messageIdExample = "74313af1-e2cc-403f-85f1-6050725b01b6";
        private static readonly string tokenRequestorIdExample = "19474920408";
        
        [SetUp]
        public void Setup() {
            tokenRequestorClient =
                new TokenRequestorClient(
                    Helpers.EnvVarOrDefault("AUTH_SERVER_URL", "http://localhost:8001/v1/accesstoken"),
                    Helpers.EnvVarOrDefault("RESOURCE_SERVER_URL", "http://localhost:8004/token-requestor"),
                    Helpers.EnvVarOrThrow("AUTH_SERVER_CLIENT_ID"),
                    Helpers.EnvVarOrThrow("AUTH_SERVER_CLIENT_SECRET")
                );
        }


        [Test]
        public async Task EnrolmentSuccessful() {
            var result = await tokenRequestorClient.EnrolCardAsync(CreateEnrolmentRequest(), Helpers.CorrelationId());
            
            Assert.That(result, Is.EqualTo(RequestStatus.Accepted));
        }

        private EnrolCardRequest CreateEnrolmentRequest() {
            return new EnrolCardRequest (
                messageId: Helpers.MessageId(),
                tokenRequestorId: tokenRequestorIdExample,
                tokenRequestorReference: "",
                encryptedCardholderAuthenticationData: encryptedExampleData
            );
        }
    }
}