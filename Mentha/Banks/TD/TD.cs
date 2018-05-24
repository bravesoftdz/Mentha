using Mentha.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mentha.Banks.TD {
    class TD : BaseBank {
        public override string Id { get { return "TD"; } }

        public override async Task<List<Account>> GetAccountsAsync(Profile profile) {
            using (var Form = new BrowserForm(profile.CardNumber, profile.Password)) {
                switch (Form.ShowDialog()) {
                    case DialogResult.Abort:
                        // Maybe the Abort takes place between scraping and logging out, so check for accounts before throwing an exception
                        if (Form.Accounts.Any()) {
                            return Form.Accounts;
                        } else {
                            throw new Exception("Query aborted by the user");
                        }

                    case DialogResult.OK:
                        return Form.Accounts;
                }
            }

            // Shouldn't get here, but just in case...
            return new List<Account>();
        }
    }
}
