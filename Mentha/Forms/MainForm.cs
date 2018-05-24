// TODOX A debug log window -- all activities should log to a global list, which can then be populated in a richtextbox on demand
// TODOX Multi-column output so the window is wider and less tall?
// TODOX Don't allow Download All to be clicked while downloading is underway (presumably bad things will happen)

using Mentha.Banks;
using Mentha.Code;
using Microsoft.Win32;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
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

                            foreach (var Account in Accounts) {
                                var NewListItem = new ListViewItem(Account.Name, Group);
                                NewListItem.SubItems.Add(Account.Balance.ToString("C"));
                                lvProfiles.Items.Add(NewListItem);
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

        private void LoadProfile(string fileId) {
            // Check if we need to prompt for the master password
            if (string.IsNullOrWhiteSpace(Globals.MasterPassword)) {
                using (var Form = new EnterMasterPasswordForm()) {
                    if (Form.ShowDialog() != DialogResult.OK) {
                        MessageBox.Show("A Master Password is required to load your saved profiles\r\n\r\nMentha will now terminate.");
                        Application.Exit();
                        return;
                    }
                }
            }

            // Decrypt the file
            var Profile = Code.Profile.Load(fileId);

            // Add the group to the ListView
            var NewGroup = new ListViewGroup($"{Profile.Bank} - {Profile.Description}") {
                Name = fileId,
                Tag = Profile,
            };
            lvProfiles.Groups.Add(NewGroup);

            // Add a placeholder to the group
            var NewListItem = new ListViewItem("Double click to download accounts for this profile...", NewGroup);
            lvProfiles.Items.Add(NewListItem);
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

                // Create data directory, if it does not yet exist
                var DataDirectory = Globals.GetAppDataDirectory();
                if (!Directory.Exists(DataDirectory)) {
                    Directory.CreateDirectory(DataDirectory);
                }

                // Load saved profiles
                int CryptographicExceptions = 0;
                int FileCount = 0;
                int OtherExceptions = 0;
                foreach (var Filename in Directory.EnumerateFiles(DataDirectory)) {
                    FileCount += 1;

                    try {
                        LoadProfile(Path.GetFileNameWithoutExtension(Filename));
                    } catch (CryptographicException) {
                        CryptographicExceptions += 1;
                    } catch (Exception) {
                        OtherExceptions += 1;
                    }
                }
                SortGroups();

                // Check for exceptions while loading the profiles
                // TODOX If all exceptions are cryptographic exceptions, clear the master password and try again
                if (CryptographicExceptions + OtherExceptions > 0) {
                    MessageBox.Show($"Exceptions were encountered while loading your saved profile information:\r\n\r\nFiles Read: {FileCount}\r\nCryptographic Exceptions: {CryptographicExceptions}\r\nOther Exceptions: {OtherExceptions}\r\n\r\nCryptographic Exceptions are likely caused by entering an invalid Master Password, or corrupt data file.\r\nOther Exceptions are unexpected and unknown in nature.\r\n\r\nMentha will now terminate.");
                    Application.Exit();
                }
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

        private void mnuHelpDataDirectory_Click(object sender, EventArgs e) {
            Process.Start(Globals.GetAppDataDirectory());
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
                    LoadProfile(Form.FileId);
                    SortGroups();
                }
            }
        }

        private async void tsbDownloadAll_ClickAsync(object sender, EventArgs e) {
            foreach (ListViewGroup Group in lvProfiles.Groups) {
                await DownloadGroupAsync(Group);
            }
        }
    }
}
