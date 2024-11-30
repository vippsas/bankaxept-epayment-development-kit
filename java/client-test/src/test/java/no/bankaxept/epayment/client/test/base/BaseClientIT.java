package no.bankaxept.epayment.client.test.base;

import java.net.MalformedURLException;
import java.net.URL;
import java.text.ParseException;
import java.util.List;
import java.util.ServiceLoader;
import com.nimbusds.jwt.JWT;
import com.nimbusds.jwt.JWTClaimsSet;
import com.nimbusds.jwt.JWTParser;
import no.bankaxept.epayment.client.base.accesstoken.ScheduledAccessTokenPublisher;
import no.bankaxept.epayment.client.base.http.HttpClient;
import no.bankaxept.epayment.client.base.spi.HttpClientProvider;
import org.junit.jupiter.api.Test;
import reactor.adapter.JdkFlowAdapter;
import reactor.test.StepVerifier;

import static java.util.Optional.ofNullable;

public class BaseClientIT {

  @Test
  public void accessToken() throws MalformedURLException {
    HttpClient httpClient = ServiceLoader.load(HttpClientProvider.class)
        .findFirst()
        .map(httpClientProvider -> httpClientProvider.create(null))
        .orElseThrow();
    var publisher = JdkFlowAdapter.flowPublisherToFlux(
        new ScheduledAccessTokenPublisher.Builder()
            .httpClient(httpClient)
            .url(new URL(System.getenv("AUTHORIZATION_SERVER_URL")))
            .clientCredentials(System.getenv("CLIENT_ID_COMPLEX_SECRET"), System.getenv("CLIENT_SECRET_COMPLEX_SECRET"))
            .build()
    ).elementAt(0);
    StepVerifier.create(publisher)
        .expectNextMatches(s -> s.startsWith("eyJ"))
        .expectNextMatches(s -> ofNullable(getTokenAudiences(s)).map(List::size).orElse(0) > 0)
        .verifyComplete();
  }

  private List<String> getTokenAudiences(String accessToken) {
    try {
      JWT jwt = JWTParser.parse(accessToken);
      JWTClaimsSet claims = jwt.getJWTClaimsSet();
      return claims.getAudience();
    } catch (ParseException e) {
      return List.of();
    }
  }

}
