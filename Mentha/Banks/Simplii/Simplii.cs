using Mentha.Code;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mentha.Banks.Simplii {
    class Simplii : CIBC.CIBC {
        public override string Id { get { return "Simplii"; } }

        // Simplii and CIBC are nearly identical -- just different URLs, and slightly different account parsing
        public override Task<List<Account>> GetAccountsAsync(Profile profile) {
            AccountsUrl = "https://online.simplii.com/ebm-ai/api/v1/json/accounts";
            RefererUrl = "https://online.simplii.com/ebm-resources/public/client/web/index.html";
            SessionsUrl = "https://online.simplii.com/ebm-anp/api/v1/json/sessions";
            SendBrandHeader = false;
            SendClientTypeHeader = false;

            return base.GetAccountsAsync(profile);
        }

        protected override List<Account> ParseAccounts(string accountsResponseText) {
            var Result = new List<Account>();

            var AccountsResponse = JsonConvert.DeserializeObject<AccountsResponse>(accountsResponseText);
            foreach (var Account in AccountsResponse.Accounts) {
                Result.Add(new Account() {
                    Balance = Account.GetBalance(),
                    Name = Account.GetName(),
                });
            }

            return Result;
        }
    }
}
