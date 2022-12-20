package no.bankaxept.epayment.client.accesstoken;

import no.bankaxept.epayment.client.base.accesstoken.AccessTokenPublisher;
import no.bankaxept.epayment.client.base.accesstoken.AccessTokenSubscriber;
import no.bankaxept.epayment.client.base.accesstoken.ScheduledAccessTokenPublisher;
import no.bankaxept.epayment.client.webflux.WebFluxClient;

import java.time.Duration;

public class AccessTokenRetriever {

    private final AccessTokenPublisher publisher;

    public AccessTokenRetriever(String uri, String id, String secret, String scope, String grantType) {
        publisher = new ScheduledAccessTokenPublisher(uri, id, secret, scope, grantType, new WebFluxClient(null));
    }

    public String get(Duration timeout) {
        return new AccessTokenSubscriber(publisher).get(timeout);
    }
}
