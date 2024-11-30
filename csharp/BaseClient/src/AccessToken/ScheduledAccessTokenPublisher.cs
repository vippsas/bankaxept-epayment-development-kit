using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text;
using System.Threading;

namespace BankAxept.Epayment.Client.Base.AccessToken {

    public class ScheduledAccessTokenPublisher : IAccessTokenPublisher, IObserver<HttpResponseMessage> {

        private string latestToken;
        private readonly ConcurrentBag<IObserver<string>> subscribers = new ConcurrentBag<IObserver<string>>();

        private bool shutDown;
        private readonly HttpClient httpClient;
        private readonly Uri uri;
        private readonly string body;

        private ScheduledAccessTokenPublisher(
            Uri uri,
            string body,
            HttpClient httpClient
        ) {
            this.uri = uri;
            this.body = body;
            this.httpClient = httpClient;
            ScheduleFetch(0);
        }

        private void ScheduleFetch(long seconds) {
            if (shutDown)
                return;
            Observable.Interval(TimeSpan.FromSeconds(seconds))
                .Take(1)
                .Subscribe(_ => FetchNewToken());
        }

        public async void FetchNewToken() {
            if (shutDown)
                return;
            
            try {
                var response = await httpClient.PostAsync(uri, new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded"));
                OnNext(response);
            } catch (Exception ex) {
                OnError(ex);
            }
        }

        public void OnCompleted() {

        }

        public void OnError(Exception ex) {
            ScheduleFetch(5);
            foreach (var subscriber in subscribers) {
                subscriber.OnError(ex);
            }
        }

        public async void OnNext(HttpResponseMessage msg) {
            var payload = await msg.Content.ReadAsStringAsync();
            if (!msg.IsSuccessStatusCode) {
                OnError(new HttpStatusException(msg.StatusCode, $"Could not fetch access token from {uri}. Status code: {msg.StatusCode}" ));
                return;
            }

            AccessToken token;
            try {
                token = AccessToken.Parse(payload);
            } catch (Exception e) {
                OnError(e);
                return;
            }

            latestToken = token.GetToken();

            foreach (var subscriber in subscribers) {
                subscriber.OnNext(token.GetToken());
            }

            ScheduleFetch(token.SecondsUntilTenPercentBeforeExpiry(DateTime.UtcNow));
        }

        public void ShutDown() {
            shutDown = true;
            try {
                //
            } catch (ThreadInterruptedException) {}
        }

        public IDisposable Subscribe(IObserver<string> observer) {
            if (!subscribers.Contains(observer)) {
                subscribers.Add(observer);
                if (!string.IsNullOrEmpty(latestToken)) {
                    observer.OnNext(latestToken);
                }
            }
            return new Unsubscriber(subscribers, observer);
        }

        private class Unsubscriber : IDisposable {
            private readonly ConcurrentBag<IObserver<string>> subscribers;
            private readonly IObserver<string> observer;

            public Unsubscriber(ConcurrentBag<IObserver<string>> subscribers, IObserver<string> observer) {
                this.subscribers = subscribers;
                this.observer = observer;
            }

            public void Dispose() {
                if(observer != null && subscribers.Contains(observer))
                    subscribers.TryTake(out _);
            }
        }

        public class Builder {
            private HttpClient httpClient;
            private Uri uri;
            private string grantType;
            private List<string> scopes = new List<string>();

            public Builder(HttpClient httpClient) {
                this.httpClient = httpClient;
            }

            public Builder HttpClient (HttpClient httpClient) {
                this.httpClient = httpClient;
                return this;
            }

            public Builder Url(Uri uri) {
                this.uri = uri;
                return this;
            }

            public Builder GrantType(string grantType) {
                this.grantType = grantType;
                return this;
            }

            public Builder ClientCredentials(string clientId, string clientSecret) {
                if (httpClient != null) {
                    var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {encodedCredentials}");
                }
                return this;
            }

            public Builder Scopes(List<string> scopes) {
                this.scopes = scopes;
                return this;
            }

            private string CreateBody() {
                var body = $"grant_type={grantType}";
                if (scopes.Count > 0) {
                    body = $"{body}&scope={System.Web.HttpUtility.UrlEncode(string.Join(' ', scopes))}";
                }
                return body;
            }

            public ScheduledAccessTokenPublisher Build() {
                if (string.IsNullOrEmpty(grantType)) {
                    throw new MissingFieldException("Grant type is not set");
                }
                return new ScheduledAccessTokenPublisher(
                    uri,
                    CreateBody(),
                    httpClient
                );
            }
        }
    }
}