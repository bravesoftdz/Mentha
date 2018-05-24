using Mentha.Code;
using System;
using System.Windows.Forms;

namespace Mentha.Forms {
    public partial class ProfileForm : Form {
        // Fields used for an add
        public string FileId { get; set; }

        // Fields used for an edit
        private Profile _Profile;

        public ProfileForm(string bank) {
            InitializeComponent();

            this.Text = bank;
        }

        public ProfileForm(Profile profile) {
            InitializeComponent();

            this._Profile = profile;
            txtCardNumber.Text = profile.CardNumber;
            txtDescription.Text = profile.Description;
            txtPassword.Text = profile.Password;
        }

        private void cmdSave_Click(object sender, EventArgs e) {
            // Validate
            if (txtDescription.Text.Trim() == string.Empty) {
                MessageBox.Show("Please enter a Description");
                txtDescription.Focus();
                return;
            }
            if (txtCardNumber.Text.Trim() == string.Empty) {
                MessageBox.Show("Please enter a Card number");
                txtCardNumber.Focus();
                return;
            }
            if (txtPassword.Text.Trim() == string.Empty) {
                MessageBox.Show("Please enter a Password");
                txtPassword.Focus();
                return;
            }

            // Check if we need to create a Master Password
            if (string.IsNullOrWhiteSpace(Globals.MasterPassword)) {
                using (var Form = new CreateMasterPasswordForm()) {
                    if (Form.ShowDialog() != DialogResult.OK) {
                        MessageBox.Show("A Master Password is required to save this profile");
                        return;
                    }
                }
            }

            // Check if we're adding a new profile
            if (_Profile == null) {
                // Generate a new file id
                FileId = Guid.NewGuid().ToString();

                // Setup the new profile
                _Profile = new Profile(FileId) {
                    Bank = this.Text,
                };
            }

            // Update the details and save the file
            _Profile.CardNumber = txtCardNumber.Text.Trim();
            _Profile.Description = txtDescription.Text.Trim();
            _Profile.Password = txtPassword.Text.Trim();
            _Profile.Save();

            // Let the main window know everything is good
            DialogResult = DialogResult.OK;
        }
    }
}
