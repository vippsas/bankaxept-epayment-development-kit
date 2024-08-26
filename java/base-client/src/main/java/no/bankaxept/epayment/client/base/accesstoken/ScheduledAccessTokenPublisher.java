package no.bankaxept.epayment.client.base.accesstoken;

import java.net.URL;
import java.net.URLEncoder;
import java.time.Clock;
import java.util.ArrayList;
import java.util.Base64;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.Queue;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.Flow;
import java.util.concurrent.Flow.Subscription;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.ScheduledExecutorService;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.atomic.AtomicReference;
import no.bankaxept.epayment.client.base.SinglePublisher;
import no.bankaxept.epayment.client.base.http.HttpClient;
import no.bankaxept.epayment.client.base.http.HttpResponse;
import no.bankaxept.epayment.client.base.http.HttpStatusException;

import static java.nio.charset.StandardCharsets.UTF_8;

public class ScheduledAccessTokenPublisher implements AccessTokenPublisher, Flow.Subscriber<HttpResponse> {

  private final ScheduledExecutorService scheduler;
  private final ExecutorService fetchExecutor = Executors.newSingleThreadExecutor();
  private final Clock clock;

  private boolean shutDown;

  private final HttpClient httpClient;
  private final URL url;
  private final Map<String, List<String>> headers;
  private final String body;

  private final AtomicReference<AccessToken> atomicToken = new AtomicReference<>();

  private final Queue<Flow.Subscriber<? super String>> subscribers = new LinkedBlockingQueue<>();

  private ScheduledAccessTokenPublisher(
      URL url,
      Map<String, List<String>> headers,
      String body,
      Clock clock,
      ScheduledExecutorService scheduler,
      HttpClient httpClient
  ) {
    this.url = url;
    this.headers = headers;
    this.body = body;
    this.clock = clock;
    this.scheduler = scheduler;
    this.httpClient = httpClient;
    scheduleFetch(0);
  }


  private void scheduleFetch(long seconds) {
    if (shutDown)
      return;
    scheduler.schedule(this::fetchNewToken, seconds, TimeUnit.SECONDS);
  }

  private void fetchNewToken() {
    if (shutDown)
      return;
    printHeaders(url.toString(), headers);
    httpClient.post(url.toString(), new SinglePublisher<>(body, fetchExecutor), headers).subscribe(this);
  }

  public static void printHeaders(String url, Map<String, List<String>> headers) {
    for (Map.Entry<String, List<String>> entry : headers.entrySet()) {
      System.out.println("CLIENT_IT_Key: " + entry.getKey() + ", CLIENT_IT_Value: " + entry.getValue() + "CLIENT_URL_TARGET:" + url);
    }
  }

  @Override
  public void onSubscribe(Flow.Subscription subscription) {
    subscription.request(1);
  }

  @Override
  public void onNext(HttpResponse item) {
    if (!item.getStatus().is2xxOk()) {
      onError(new HttpStatusException(item.getStatus(), String.format(
              "Could not get access token from %s. HTTP status: %s. HTTP payload: %s",
              url.toString(),
              item.getStatus(),
              item.getBody()
          ))
      );
      return;
    }
    AccessToken token;
    try {
      token = AccessToken.parse(item.getBody(), clock);
    } catch (Exception e) {
      onError(e);
      return;
    }
    atomicToken.set(token);
    synchronized (subscribers) {
      subscribers.forEach(subscriber -> subscriber.onNext(token.getToken()));
      subscribers.clear();
    }
    scheduleFetch(token.secondsUntilTenSecondsBeforeExpiry(clock));
  }

  @Override
  public void onError(Throwable throwable) {
    scheduleFetch(5);
    synchronized (subscribers) {
      subscribers.forEach(subscriber -> subscriber.onError(throwable));
      subscribers.clear();
    }
  }

  @Override
  public void onComplete() {
  }

  public void shutDown() {
    shutDown = true;
    scheduler.shutdown();
    fetchExecutor.shutdown();
    try {
      while (!scheduler.awaitTermination(500, TimeUnit.MILLISECONDS) && !fetchExecutor.awaitTermination(
          500,
          TimeUnit.MILLISECONDS
      )) {
      }
    } catch (InterruptedException ignored) {
    }
  }

  @Override
  public void subscribe(Flow.Subscriber<? super String> subscriber) {
    subscriber.onSubscribe(new Subscription() {
      @Override
      public void request(long n) {}
      @Override
      public void cancel() {}
    });

    var token = atomicToken.get();
    if (token != null && token.getExpiry().isAfter(clock.instant()))
      subscriber.onNext(token.getToken());
    else {
      synchronized (subscribers) {
        subscribers.add(subscriber);
      }
    }
  }

  public static class Builder {

    private URL url;
    private HttpClient httpClient;
    private ScheduledExecutorService scheduler;
    private Clock clock = Clock.systemDefaultZone();

    private final Map<String, List<String>> headers = new HashMap<>(Map.of(
        "Content-type",
        List.of("application/x-www-form-urlencoded")
    ));
    private GrantType grantType;
    private List<String> scopes = new ArrayList<>();

    public Builder httpClient(HttpClient httpClient) {
      this.httpClient = httpClient;
      return this;
    }

    public Builder url(URL url) {
      this.url = url;
      return this;
    }

    public Builder clientCredentials(String id, String secret) {
      headers.put(
          "Authorization",
          List.of("Basic " + Base64.getEncoder().encodeToString(
              (URLEncoder.encode(id, UTF_8) + ":" + URLEncoder.encode(secret, UTF_8)).getBytes(UTF_8)
          ))
      );
      return grantType(GrantType.client_credentials);
    }

    private Builder grantType(GrantType grantType) {
      this.grantType = grantType;
      return this;
    }

    public Builder scopes(List<String> scopes) {
      this.scopes.addAll(scopes);
      return this;
    }

    public Builder clock(Clock clock) {
      this.clock = clock;
      return this;
    }

    public Builder scheduler(ScheduledExecutorService scheduler) {
      this.scheduler = scheduler;
      return this;
    }

    public ScheduledAccessTokenPublisher build() {
      if (grantType == null) {
        throw new IllegalArgumentException("Grant type is not set");
      }
      return new ScheduledAccessTokenPublisher(url, headers, createBody(), clock, getSchedulerOrDefault(), httpClient);
    }

    private ScheduledExecutorService getSchedulerOrDefault() {
      return scheduler == null ? Executors.newScheduledThreadPool(1) : scheduler;
    }

    private String createBody() {
      var body = new StringBuilder("grant_type=").append(grantType);
      if (!scopes.isEmpty()) {
        body.append("&").append("scope=").append(URLEncoder.encode(String.join(" ", scopes), UTF_8));
      }
      return body.toString();
    }
  }
}
