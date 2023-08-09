package no.bankaxept.epayment.client.test.wallet;

import static no.bankaxept.epayment.client.test.Verifier.verifyBadRequest;

import java.net.MalformedURLException;
import java.net.URL;
import java.util.UUID;
import no.bankaxept.epayment.client.wallet.WalletClient;
import no.bankaxept.epayment.client.wallet.bankaxept.PaymentRequest;
import org.junit.jupiter.api.Test;

public class WalletClientIT {

  private WalletClient t1Client() throws MalformedURLException {
    return new WalletClient(
        new URL("https://t1-api.techcloud0dev.net/bankaxept-epayment/access-token-api/v1/accesstoken"),
        new URL("https://t1-api.techcloud0dev.net/bankaxept-epayment/wallet-api"),
        System.getenv("APIM_KEY"),
        System.getenv("CLIENT_ID"),
        System.getenv("CLIENT_SECRET")
    );
  }

  private WalletClient testClient() throws MalformedURLException {
    return new WalletClient(
        new URL("https://epp.stoetest.cloud/access-token/v1/accesstoken"),
        new URL("https://epp.stoetest.cloud/wallet"),
        System.getenv("CLIENT_ID"),
        System.getenv("CLIENT_SECRET")
    );
  }

  @Test
  public void paymentRequest() throws MalformedURLException {
    paymentRequest(t1Client());
    paymentRequest(testClient());
  }

  private void paymentRequest(WalletClient client) {
    var correlationId = UUID.randomUUID().toString();
    System.out.println("Correlation id: " + correlationId);
    verifyBadRequest(client.requestPayment(new PaymentRequest(), correlationId));
  }

}