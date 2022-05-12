package no.bankaxept.epayment.client.merchant;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.fasterxml.jackson.databind.SerializationFeature;
import com.fasterxml.jackson.datatype.jsr310.JavaTimeModule;
import no.bankaxept.epayment.client.base.*;

import java.util.List;
import java.util.Map;
import java.util.concurrent.Executor;
import java.util.concurrent.Executors;
import java.util.concurrent.Flow;

import static java.util.Collections.emptyMap;

public class MerchantClient {

    private final BaseClient baseClient;

    private final static String BASE_URL = "/bankaxept-epayment/merchant-api/v1";
    private final static String PAYMENTS_URL = BASE_URL + "/payments";

    private final ObjectMapper objectMapper = new ObjectMapper()
            .registerModule(new JavaTimeModule())
            .disable(SerializationFeature.WRITE_DATES_AS_TIMESTAMPS);

    private final Executor executor = Executors.newSingleThreadExecutor();

    public MerchantClient(BaseClient baseClient) {
        this.baseClient = baseClient;
    }

    public MerchantClient(String baseurl, String apimKey, String username, String password) {
        this.baseClient = new BaseClient(baseurl, apimKey, username, password);
    }

    public Flow.Publisher<RequestStatus> payment(PaymentRequest request, String correlationId, Map<String, List<String>> customHeaders) {
        try {
            return new MapOperator<>(baseClient.post(PAYMENTS_URL, new SinglePublisher<>(objectMapper.writeValueAsString(request), executor), correlationId, customHeaders), httpResponse -> httpResponse.getStatus().toResponse());
        } catch (JsonProcessingException e) {
            throw new RuntimeException(e);
        }
    }

    public Flow.Publisher<RequestStatus> payment(PaymentRequest request, String correlationId) {
        return payment(request, correlationId, emptyMap());
    }

}
