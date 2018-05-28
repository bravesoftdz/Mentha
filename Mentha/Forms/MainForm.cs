// TODOX A debug log window -- all activities should log to a global list, which can then be populated in a richtextbox on demand
// TODOX Edit/Delete profiles
// TODOX Load/Save Globals.Settings.Form*
// TODOX Download OFX files to allow easy import of transactions into MS Money

using Mentha.Banks;
using Mentha.Code;
using Microsoft.Win32;
using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mentha.Forms {
    public partial class MainForm : Form {
        private bool _FormPainted = false;

        public MainForm() {
            InitializeComponent();

            // Enable newer version of IE in WebBrowser control
            using (var Key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION", true)) {
                var ExeName = Process.GetCurrentProcess().ProcessName + ".exe";
                Key.SetValue(ExeName, 99999, RegistryValueKind.DWord);
            }
        }

        private void AddProfileToListView(Profile profile) {
            // Add the group to the ListView
            var NewGroup = new ListViewGroup($"{profile.Bank} - {profile.Description}") {
                Name = profile.Id,
                Tag = profile,
            };
            lvProfiles.Groups.Add(NewGroup);

            // Add a placeholder to the group
            var NewListItem = new ListViewItem("Double click to download accounts for this profile...", NewGroup);
            lvProfiles.Items.Add(NewListItem);
        }

        private async Task DownloadGroupAsync(ListViewGroup Group) {
            if (Group.Tag != null) {
                // Not null tag means it still needs to be downloaded
                var Profile = (Group.Tag as Profile);

                Group.Items[0].Text = "Downloading...";
                Group.Tag = null;

                // Determine which bank to download
                BaseBank Bank = Globals.Banks.SingleOrDefault(x => x.Id == Profile.Bank);
                if (Bank == null) {
                    Group.Items[0].Text = $"ERROR: Missing handler for '{Profile.Bank}'";
                } else {
                    try {
                        var Accounts = await Bank.GetAccountsAsync(Profile);
                        if (Accounts.Any()) {
                            lvProfiles.Items.Remove(Group.Items[0]);

                            int Index = 1;
                            foreach (var Account in Accounts) {
                                if (Index++ % 2 == 1) {
                                    // Odd index means start a new row
                                    var NewListItem = new ListViewItem(Account.Name, Group);
                                    NewListItem.SubItems.Add(Account.Balance.ToString("C"));
                                    lvProfiles.Items.Add(NewListItem);

                                } else {
                                    // Even index means add to last row
                                    var ExistingListItem = Group.Items[Group.Items.Count - 1];
                                    ExistingListItem.SubItems.Add(Account.Name);
                                    ExistingListItem.SubItems.Add(Account.Balance.ToString("C"));
                                }
                            }
                        } else {
                            Group.Items[0].Text = "No accounts found for this profile!";
                        }
                    } catch (Exception ex) {
                        Group.Items[0].Text = "EXCEPTION: " + ex.Message;
                        Group.Tag = Profile; // Re-add, so it can be retried later
                    }
                }
            }
        }

        private async void lvProfiles_DoubleClickAsync(object sender, EventArgs e) {
            if (lvProfiles.SelectedItems.Count > 0) {
                await DownloadGroupAsync(lvProfiles.SelectedItems[0].Group);
            }
        }

        private void MainForm_Paint(object sender, PaintEventArgs e) {
            if (!_FormPainted) {
                _FormPainted = true;

                // Add supported banks to the dropdown list
                foreach (var Bank in Globals.Banks.OrderBy(x => x.Id)) {
                    var DD = new ToolStripMenuItem(Bank.Id);
                    DD.Click += tsbAddProfile_Click;
                    tsbAddProfile.DropDownItems.Add(DD);
                }

                // Add saved profiles
                foreach (var Profile in Globals.Settings.Profiles) {
                    AddProfileToListView(Profile);
                }
                SortGroups();
            }
        }

        private void mnuFileExit_Click(object sender, EventArgs e) {
            Application.Exit();
        }

        private void mnuHelpAbout_Click(object sender, EventArgs e) {
            using (var Form = new AboutForm()) {
                Form.ShowDialog();
            }
        }

        private void SortGroups() {
            // Save and clear old groups
            var Groups = new ListViewGroup[lvProfiles.Groups.Count];
            lvProfiles.Groups.CopyTo(Groups, 0);
            lvProfiles.Groups.Clear();

            // Add groups back in, sorted ascending
            lvProfiles.Groups.AddRange(Groups.OrderBy(x => x.Header).ToArray());
        }

        private void tsbAddProfile_Click(object sender, EventArgs e) {
            var Bank = (sender as ToolStripMenuItem).Text;
            using (var Form = new ProfileForm(Bank)) {
                if (Form.ShowDialog() == DialogResult.OK) {
                    // Save new entry
                    Globals.Settings.Profiles.Add(Form.Profile);
                    Globals.Settings.Save();

                    // Update the listview
                    AddProfileToListView(Form.Profile);
                    SortGroups();
                }
            }
        }

        private async void tsbDownloadAll_ClickAsync(object sender, EventArgs e) {
            // Get and handle TD profiles first (stuffing input doesn't work well if a tangerine security question pops up over the browser window)
            // Must use 'await' so they're handled one at a time
            foreach (ListViewGroup Group in lvProfiles.Groups) {
                if ((Group.Tag != null) && ((Group.Tag as Profile).Bank == "TD")) {
                    await DownloadGroupAsync(Group);
                }
            }

            // Then handle non-TD all at once
            // No 'await' required, the Bank objects are thread-safe
            foreach (ListViewGroup Group in lvProfiles.Groups) {
                if ((Group.Tag != null) && ((Group.Tag as Profile).Bank != "TD")) {
                    DownloadGroupAsync(Group);
                }
            }
        }
    }
}
