using Mentha.Code;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mentha.Banks.Tangerine {
    // 100% credit for API: https://github.com/kevinjqiu/tangerine
    class Tangerine : BaseBank {
        public override string Id { get { return "Tangerine"; } }

        public override async Task<List<Account>> GetAccountsAsync(Profile profile) {
            var Accounts = new List<Account>();

            var CookieContainer = new CookieContainer();
            using (var HCH = new HttpClientHandler() { CookieContainer = CookieContainer }) {
                using (var HC = new HttpClient(HCH)) {
                    HC.DefaultRequestHeaders.Add("Accept", "application/json");
                    HC.DefaultRequestHeaders.Add("x-web-flavour", "fbe");

                    // GET the login page
                    var ResponseText = await GetAsync(HC, "https://secure.tangerine.ca/web/InitialTangerine.html?command=displayLoginRegular&device=web&locale=en_CA");

                    // POST the card number
                    var PostData = new FormUrlEncodedContent(new[]
                        {
                        new KeyValuePair<string, string>("command", "PersonalCIF"),
                        new KeyValuePair<string, string>("ACN", profile.CardNumber),
                        new KeyValuePair<string, string>("locale", "en_CA"),
                        new KeyValuePair<string, string>("device", "web"),
                    });
                    ResponseText = await PostAsync(HC, "https://secure.tangerine.ca/web/Tangerine.html", PostData);

                    // GET the extra security question page
                    ResponseText = await GetAsync(HC, "https://secure.tangerine.ca/web/Tangerine.html?command=displayChallengeQuestion");

                    // Prompt the user for the answer (unless they previously answered and we saved it)
                    string ChallengeAnswer = string.Empty;
                    var ChallengeResponse = JsonConvert.DeserializeObject<DisplayChallengeQuestionResponse>(ResponseText);
                    string ChallengeQuestion = ChallengeResponse.MessageBody.Question;
                    var SavedQuestion = profile.SecurityQuestions.SingleOrDefault(x => x.Question.ToLower().Trim() == ChallengeQuestion.ToLower().Trim());
                    if (SavedQuestion == null) {
                        // Not previously answered, so prompt them now
                        using (var Form = new ChallengeQuestionForm(ChallengeQuestion)) {
                            Form.Text += $" ({profile.Description})";
                            if (Form.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                                ChallengeAnswer = Form.Answer;
                            } else {
                                throw new Exception("Security question not answered by user");
                            }
                        }
                    } else {
                        // Previously answered, so use the saved answer
                        ChallengeAnswer = SavedQuestion.Answer;
                    }

                    // POST the extra security answer
                    PostData = new FormUrlEncodedContent(new[] {
                        new KeyValuePair<string, string>("command", "verifyChallengeQuestion"),
                        new KeyValuePair<string, string>("Answer", ChallengeAnswer),
                        new KeyValuePair<string, string>("locale", "en_CA"),
                        new KeyValuePair<string, string>("device", "web"),
                        new KeyValuePair<string, string>("BUTTON", "Next"),
                        new KeyValuePair<string, string>("Next", "Next"),
                    });
                    ResponseText = await PostAsync(HC, "https://secure.tangerine.ca/web/Tangerine.html", PostData);

                    // Confirm answer was accepted before continuing
                    if (!ResponseText.ToLower().Contains("displaypin")) {
                        if (SavedQuestion == null) {
                            throw new Exception("User-provided security response not accepted");
                        } else {
                            throw new Exception("Previously-saved security response not accepted");
                        }
                    }

                    // GET the PIN entry page
                    ResponseText = await GetAsync(HC, "https://secure.tangerine.ca/web/Tangerine.html?command=displayPIN");

                    // POST the PIN
                    PostData = new FormUrlEncodedContent(new[] {
                        new KeyValuePair<string, string>("command", "validatePINCommand"),
                        new KeyValuePair<string, string>("PIN", profile.Password),
                        new KeyValuePair<string, string>("locale", "en_CA"),
                        new KeyValuePair<string, string>("device", "web"),
                        new KeyValuePair<string, string>("BUTTON", "Go"),
                        new KeyValuePair<string, string>("Go", "Next"),
                        new KeyValuePair<string, string>("callSource", "4"),
                    });
                    ResponseText = await PostAsync(HC, "https://secure.tangerine.ca/web/Tangerine.html", PostData);

                    // GET a couple more pages to complete the login
                    ResponseText = await GetAsync(HC, "https://secure.tangerine.ca/web/Tangerine.html?command=PINPADPersonal");
                    ResponseText = await GetAsync(HC, "https://secure.tangerine.ca/web/Tangerine.html?command=displayAccountSummary&fill=1");

                    // GET the list of accounts
                    ResponseText = await GetAsync(HC, "https://secure.tangerine.ca/web/rest/pfm/v1/accounts");
                    var AccountsResponse = JsonConvert.DeserializeObject<AccountsResponse>(ResponseText);
                    if (AccountsResponse.response_status.status_code.ToLower() == "success") {
                        // Check if we should save the security question/answer
                        if (SavedQuestion == null) {
                            // Yep, we should update the file and re-save
                            profile.SecurityQuestions.Add(new Account_SecurityQuestion() {
                                Answer = ChallengeAnswer,
                                Question = ChallengeQuestion,
                            });
                            profile.Save();
                        }

                        // Parse accounts
                        foreach (var Account in AccountsResponse.accounts) {
                            Accounts.Add(new Account() {
                                Balance = Account.GetBalance(),
                                Name = Account.GetName(),
                            });
                        }
                    } else {
                        throw new Exception($"Unexpected accounts response: '{AccountsResponse.response_status.status_code}'");
                    }

                    // GET the logout page
                    ResponseText = await GetAsync(HC, "https://secure.tangerine.ca/web/InitialTangerine.html?command=displayLogout&device=web&locale=en_CA");
                }
            }

            return Accounts;
        }
    }
}
