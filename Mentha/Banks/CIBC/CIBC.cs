using Mentha.Code;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Mentha.Banks.CIBC {
    class CIBC : BaseBank {
        public override string Id { get { return "CIBC"; } }

        public string AccountsUrl { get; set; } = "https://www.cibconline.cibc.com/ebm-ai/api/v2/json/accounts";
        public string RefererUrl { get; set; } = "https://www.cibconline.cibc.com/ebm-resources/public/banking/cibc/client/web/index.html";
        public string SessionsUrl { get; set; } = "https://www.cibconline.cibc.com/ebm-anp/api/v1/json/sessions";

        public bool SendBrandHeader { get; set; } = true;
        public bool SendClientTypeHeader { get; set; } = true;

        public override async Task<List<Account>> GetAccountsAsync(Profile profile) {
            var Accounts = new List<Account>();

            using (var HC = new HttpClient()) {
                HC.DefaultRequestHeaders.Add("Accept", "application/vnd.api+json");
                HC.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                HC.DefaultRequestHeaders.Add("Accept-Language", "en");
                if (SendBrandHeader) {
                    HC.DefaultRequestHeaders.Add("brand", "cibc");
                }
                if (SendClientTypeHeader) {
                    HC.DefaultRequestHeaders.Add("Client-Type", "default_web");
                }
                HC.DefaultRequestHeaders.Add("Connection", "keep-alive");
                HC.DefaultRequestHeaders.Add("Cookie", "");
                HC.DefaultRequestHeaders.Add("Referer", RefererUrl);
                HC.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:60.0) Gecko/20100101 Firefox/60.0");
                HC.DefaultRequestHeaders.Add("WWW-Authenticate", "CardAndPassword");
                HC.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");

                // Start a session
                string PostData = $"{{\"card\":{{\"value\":\"{profile.CardNumber}\",\"description\":\"\",\"encrypted\":false,\"encrypt\":true}},\"password\":\"{profile.Password}\"}}";
                var Response = await HC.PostAsync(SessionsUrl, new StringContent(PostData, Encoding.UTF8, "application/vnd.api+json"));
                if (Response.IsSuccessStatusCode) {
                    var XAuthToken = Response.Headers.GetValues("X-Auth-Token").First();
                    if (!string.IsNullOrWhiteSpace(XAuthToken)) {
                        HC.DefaultRequestHeaders.Add("X-Auth-Token", XAuthToken);

                        // Retrieve a list of accounts
                        var ResponseText = await GetAsync(HC, AccountsUrl);
                        Accounts.AddRange(ParseAccounts(ResponseText));

                        // Destroy the session
                        ResponseText = await DeleteAsync(HC, SessionsUrl);
                    }
                } else {
                    throw new Exception($"Unexpected response status: '{Response.StatusCode}'");
                }
            }

            return Accounts;
        }

        protected virtual List<Account> ParseAccounts(string accountsResponseText) {
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
