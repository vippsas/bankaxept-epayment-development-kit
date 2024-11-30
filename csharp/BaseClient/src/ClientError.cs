using System;

namespace BankAxept.Epayment.Client.Base {

    public class ClientError : Exception {
        public ClientError(string error) : base(error) {}
    }
}