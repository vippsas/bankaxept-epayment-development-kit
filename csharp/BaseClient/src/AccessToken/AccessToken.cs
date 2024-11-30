using System;
using System.Text.RegularExpressions;

namespace BankAxept.Epayment.Client.Base.AccessToken {

    public class AccessToken {

        private readonly string token;
        private readonly DateTime expiry;
        
        public AccessToken(string token, long expiresIn, DateTime clock) {
            this.token = token;
            expiry = clock.AddSeconds(expiresIn);
        }

        public AccessToken(string token, long expiresIn) : this (token, expiresIn, DateTime.UtcNow) {}

        public string GetToken() {
            return token;
        }

        public DateTime GetExpiry() {
            return expiry;
        }

        public long SecondsUntilTenSecondsBeforeExpiry (DateTime clock) {
            return (long)(expiry.AddSeconds(-10) - clock).TotalSeconds;
        }

        public long SecondsUntilTenPercentBeforeExpiry (DateTime clock) {
            var remainingSeconds = (expiry - clock).TotalSeconds;
            return (long)(remainingSeconds * 0.9);
        }

        public static AccessToken Parse(string input) {
            return AccessTokenParser.Parse(input);
        }

        public static AccessToken Parse(string input, DateTime clock) {
            return AccessTokenParser.Parse(input, clock);
        }


        private static class AccessTokenParser {
            private static readonly Regex tokenPattern = new Regex("\"access_token\"\\s*:\\s*\"(.+?)\"");
            private static readonly Regex expiryPattern = new Regex("\"expires_in\"\\s*:\\s*(\\d+)");

            public static AccessToken Parse(string input) {
                return Parse(input, DateTime.UtcNow);
            }

            public static AccessToken Parse(string input, DateTime clock) {
                var tokenMatch = tokenPattern.Match(input);
                var expiryMatch = expiryPattern.Match(input);
                if (!tokenMatch.Success || !expiryMatch.Success) {
                    throw new ArgumentException("Could not parse token or expiry");
                }
                return new AccessToken(tokenMatch.Groups[1].Value, long.Parse(expiryMatch.Groups[1].Value), clock);
            }
        }
    }
}
