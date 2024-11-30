using System;

namespace BankAxept.Epayment.Client.Base {
    public class AccessFailed : Exception {
        public AccessFailed(Exception e) : base("Access failed", e) {}
    }
}