using System.Collections.Generic;

namespace Mentha.Banks.CIBC {
    class AccountsResponse {
        public List<AccountsResponse_Account> Accounts { get; set; }
    }



    class AccountsResponse_Account {
        public string Nickname { get; set; }
        public AccountsResponse_Account_Categorization Categorization { get; set; }
        public double Balance { get; set; }

        public double GetBalance() {
            // Simplii returns positive numbers when you have a balance owing on a credit account, so flip the sign
            if (Categorization.Category == "CREDIT") {
                return Balance * -1;
            } else {
                return Balance;
            }
        }

        public string GetName() {
            if (string.IsNullOrWhiteSpace(Nickname)) {
                return Categorization.Category + " - " + Categorization.SubCategory + (Categorization.ExtraSubCategory == null ? "" : " - " + Categorization.ExtraSubCategory);
            } else {
                return Nickname;
            }
        }
    }



    class AccountsResponse_Account_Categorization {
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string ExtraSubCategory { get; set; }
    }
}
