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

public class MerchantClient {

    private final BaseClient baseClient;

    private final static String SIMULATION_HEADER = "X-Simulation";

    private final static String PAYMENTS_URL = "/payments";

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

    private static Map<String, List<String>> findSimulationHeader(Object request) {
        if(request instanceof SimulationRequest) {
            return Map.of(SIMULATION_HEADER, ((SimulationRequest) request).getSimulationValues());
        }
        return Map.of();
    }

    public Flow.Publisher<RequestStatus> payment(PaymentRequest request, String correlationId) {
        try {
            return new MapOperator<>(baseClient.post(PAYMENTS_URL, new SinglePublisher<>(objectMapper.writeValueAsString(request), executor), correlationId, findSimulationHeader(request)), httpResponse -> httpResponse.getStatus().toResponse());
        } catch (JsonProcessingException e) {
            throw new RuntimeException(e);
        }
    }

}
