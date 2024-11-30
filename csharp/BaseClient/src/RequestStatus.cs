using System.Net;
using System.Text.Json;

namespace BankAxept.Epayment.Client.Base {

    public record ProblemDetails (string Details){
        public static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    };
    public enum RequestStatus {
        Accepted,
        Repeated,
        Rejected,
        Conflicted,
        Failed
    }

    public static class RequestStatusParser {

        private static bool Is4xxClientError(HttpStatusCode statusCode) {
            return (int)statusCode >= 400 && (int)statusCode < 500;
        }

        public static RequestStatus Parse(HttpStatusCode statusCode, string body) {
            return statusCode switch {
                HttpStatusCode.OK => RequestStatus.Repeated,
                HttpStatusCode.Created => RequestStatus.Accepted,
                HttpStatusCode.Conflict => RequestStatus.Conflicted,
                HttpStatusCode.UnprocessableContent => RequestStatus.Rejected,
                _ when Is4xxClientError(statusCode) => throw ParseClientError(body),
                _ => RequestStatus.Failed
            };
        }

        private static ClientError ParseClientError(string body) {
            ProblemDetails details;
            try {
                details = JsonSerializer.Deserialize<ProblemDetails>(body, ProblemDetails.jsonOptions);
            } catch (JsonException) {
                details = null;
            }
            return details == null || string.IsNullOrEmpty(details.Details)
                ? new ClientError(body)
                : new ClientError(details.Details);
        }
    }
}