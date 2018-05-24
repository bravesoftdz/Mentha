using System.Collections.Generic;

namespace Mentha.Banks.Tangerine {
    class AccountsResponse {
        public AccountsResponse_ResponseStatus response_status { get; set; }
        public List<AccountsResponse_Account> accounts { get; set; }
    }



    class AccountsResponse_Account {
        public double account_balance { get; set; }
        public string nickname { get; set; }
        public string description { get; set; }
        public string display_name { get; set; }
        public string type { get; set; }

        public double GetBalance() {
            // TODOX Don't have a CREDIT account with Tangerine, so not sure if they need the sign flipped like CIBC
            return account_balance;
        }

        public string GetName() {
            if (!string.IsNullOrWhiteSpace(nickname)) {
                return nickname;
            } else if (!string.IsNullOrWhiteSpace(description)) {
                return description;
            } else {
                return $"{type} - {display_name}";
            }
        }
    }



    class AccountsResponse_ResponseStatus {
        public string status_code { get; set; }
    }
}
