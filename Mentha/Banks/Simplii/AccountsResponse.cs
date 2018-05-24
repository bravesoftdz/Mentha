using System.Collections.Generic;

namespace Mentha.Banks.Simplii {
    class AccountsResponse {
        public List<AccountsResponse_Account> Accounts { get; set; }
    }



    class AccountsResponse_Account {
        public string Nickname { get; set; }
        public AccountsResponse_Account_Product Product { get; set; }
        public AccountsResponse_Account_Balance Balance { get; set; }

        public double GetBalance() {
            // Simplii returns negative numbers when you have a balance owing on a credit account, so no adjustment needed
            return Balance.Amount;
        }

        public string GetName() {
            if (string.IsNullOrWhiteSpace(Nickname)) {
                return Product.Category + " - " + Product.Type;
            } else {
                return Nickname;
            }
        }
    }



    class AccountsResponse_Account_Balance {
        public string Currency { get; set; }
        public double Amount { get; set; }
    }



    class AccountsResponse_Account_Product {
        public string Category { get; set; }
        public string Type { get; set; }
    }
}
