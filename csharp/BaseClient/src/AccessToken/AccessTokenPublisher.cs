using System;

namespace BankAxept.Epayment.Client.Base.AccessToken {
    public interface IAccessTokenPublisher : IObservable<string> {
        void ShutDown() {}
    }
}