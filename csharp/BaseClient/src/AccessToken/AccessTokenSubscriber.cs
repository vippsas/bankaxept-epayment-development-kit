using System;
using System.Threading.Tasks;

namespace BankAxept.Epayment.Client.Base.AccessToken {
    public class AccessTokenSubscriber : IObserver<string> {

        private readonly TaskCompletionSource<string> token = new TaskCompletionSource<string>();

        public AccessTokenSubscriber(IObservable<string> tokenPublisher) {
            tokenPublisher.Subscribe(this);
        }

        public void OnCompleted() {}

        public void OnError(Exception error) {
            token.TrySetException(error);
        }

        public void OnNext(string value) {
            token.TrySetResult(value);
        }

        public string Get(TimeSpan timeout) {
            try {
                if (token.Task.Wait(timeout)) {
                    return token.Task.Result;
                } else {
                    throw new AccessFailed(new TimeoutException("Timeout waiting for token"));
                }
            }
            catch (AggregateException e) {
                throw new AccessFailed(e.InnerException);
            }
            catch (Exception e){
                throw new AccessFailed(e);
            }
        }
    }
}