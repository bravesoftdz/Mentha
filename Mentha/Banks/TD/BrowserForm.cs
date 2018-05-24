using Mentha.Code;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Mentha.Banks.TD {
    public partial class BrowserForm : Form {
        public List<Account> Accounts = new List<Account>();

        private enum ScraperState {
            WaitingForLoginForm,
            WaitingForAccountInformation,
            WaitingForLogout,
            LoggedOut
        }

        private string _CardNumber = null;
        private string _Password = null;
        private ScraperState _ScraperState = ScraperState.WaitingForLoginForm;

        public BrowserForm(string cardNumber, string password) {
            InitializeComponent();

            _CardNumber = cardNumber;
            _Password = password;
        }

        private void CheckForLogoutCompleted() {
            // Check if the frame we're waiting for has loaded
            var Frames = WebBrowser.Document.GetElementsByTagName("frame");
            foreach (HtmlElement Frame in Frames) {
                if (Frame.Name.ToLower() == "tddetails") {
                    // We found our frame, check for the sub-frame (seriously TD, frames within frames?!?)
                    HtmlWindow DetailsFrame = Frame.Document.Window.Frames["tddetails"];
                    var SubFrames = DetailsFrame.Document.GetElementsByTagName("frame");
                    foreach (HtmlElement SubFrame in SubFrames) {
                        if (SubFrame.Name.ToLower() == "logoutframe") {
                            // We found our subframe, so logout must have succeeded
                            _ScraperState = ScraperState.LoggedOut;
                            DialogResult = DialogResult.OK;
                            return;
                        }
                    }
                }
            }
        }

        private void ParseAccounts(HtmlElement div, bool flipSign) {
            var Rows = div.GetElementsByTagName("tr");
            foreach (HtmlElement Row in Rows) {
                if (string.IsNullOrWhiteSpace(Row.GetAttribute("className")) || (Row.GetAttribute("className") == "td-table-alt-row")) {
                    string Name = Row.GetElementsByTagName("th")[0].InnerText;

                    // Sometimes balance is something like "USD&nbsp;$0.00", so try to parse out just the dollar amount
                    var BalanceText = Row.GetElementsByTagName("td")[0].InnerText;
                    Match M = Regex.Match(BalanceText, "([$][0-9,]*[.][0-9]{2})");
                    if (M.Success) {
                        BalanceText = M.Groups[1].Value; 
                    }

                    if (!double.TryParse(BalanceText.Replace("$", "").Replace(",", ""), out double Balance)) {
                        Balance = 999999999.99;
                    }
                    if (flipSign) {
                        Balance *= -1;
                    }

                    Accounts.Add(new Account() {
                        Balance = Balance,
                        Name = Name,
                    });
                }
            }
        }

        // TODOX If the auto-login fails and the user manually logs in, it'll be stuck at WaitingForLoginForm.  So maybe these checks should be adjusted
        //       so TryToScrapeAccountInformation gets run all the time, so then if the user manually logs in it'll still get the data it wants
        private void tmrScrape_Tick(object sender, EventArgs e) {
            tmrScrape.Enabled = false;

            if ((WebBrowser != null) && (WebBrowser.Document != null)) {
                switch (_ScraperState) {
                    case ScraperState.WaitingForLoginForm:
                        TryToSubmitLoginForm();
                        break;

                    case ScraperState.WaitingForAccountInformation:
                        TryToScrapeAccountInformation();
                        break;

                    case ScraperState.WaitingForLogout:
                        CheckForLogoutCompleted();
                        break;
                }
            }

            if (_ScraperState != ScraperState.LoggedOut) {
                tmrScrape.Enabled = true;
            }
        }

        private void TryToScrapeAccountInformation() {
            // Check if the frame we're waiting for has loaded
            var Frames = WebBrowser.Document.GetElementsByTagName("frame");
            foreach (HtmlElement Frame in Frames) {
                if (Frame.Name.ToLower() == "tddetails") {
                    bool FoundAccountDivs = false;

                    // We found our frame, check for the account details divs
                    HtmlWindow DetailsFrame = Frame.Document.Window.Frames["tddetails"];
                    var Divs = DetailsFrame.Document.GetElementsByTagName("div");
                    foreach (HtmlElement Div in Divs) {
                        switch (Div.GetAttribute("className")) {
                            case "td-target-banking":
                                // Banking accounts
                                ParseAccounts(Div, false);
                                FoundAccountDivs = true;
                                break;
                            case "td-target-creditcards":
                                // Credit accounts, so flip the sign on the balance
                                ParseAccounts(Div, true);
                                FoundAccountDivs = true;
                                break;
                            case "td-target-investing":
                                // Investment accounts
                                ParseAccounts(Div, false);
                                FoundAccountDivs = true;
                                break;
                        }
                    }

                    if (FoundAccountDivs) {
                        // And now that we've parsed the account details, let's log out
                        var Anchors = DetailsFrame.Document.GetElementsByTagName("a");
                        foreach (HtmlElement Anchor in Anchors) {
                            if (!string.IsNullOrWhiteSpace(Anchor.InnerText) && (Anchor.InnerText.Trim().ToLower() == "logout")) {
                                Anchor.InvokeMember("click");
                            }
                        }

                        // Presumably we found a logout button above, if not we'll still switch states and hopefully
                        // the user will manually click the Logout button after waiting a few seconds
                        _ScraperState = ScraperState.WaitingForLogout;
                    }

                    break;
                }
            }
        }

        private void TryToSubmitLoginForm() {
            HtmlElement LoginForm = WebBrowser.Document.GetElementById("loginForm");
            if (LoginForm != null) {
                HtmlElement UsernameInput = WebBrowser.Document.GetElementById("username100");
                if (UsernameInput != null) {
                    HtmlElement PasswordInput = WebBrowser.Document.GetElementById("password");
                    if (PasswordInput != null) {
                        var Buttons = LoginForm.GetElementsByTagName("button");
                        foreach (HtmlElement Button in Buttons) {
                            if (Button.InnerText.Trim().ToLower() == "login") {
                                // Have all the inputs we need, try injecting the username and password
                                WebBrowser.Focus();

                                UsernameInput.Focus();
                                SendKeys.SendWait(_CardNumber);

                                PasswordInput.Focus();
                                SendKeys.SendWait(_Password);

                                Button.InvokeMember("click");

                                _ScraperState = ScraperState.WaitingForAccountInformation;
                                return;
                            }
                        }
                    }
                }
            }
        }

        private void WebBrowser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e) {
            txtUrl.Text = WebBrowser.Url.ToString();
        }
    }
}
