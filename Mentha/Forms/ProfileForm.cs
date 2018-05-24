using Mentha.Code;
using System;
using System.Windows.Forms;

namespace Mentha.Forms {
    public partial class ProfileForm : Form {
        public Profile Profile;

        public ProfileForm(string bank) {
            InitializeComponent();

            this.Text = bank;
        }

        public ProfileForm(Profile profile) {
            InitializeComponent();

            this.Profile = profile;
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
            if (Profile == null) {
                // Setup the new profile
                Profile = new Profile() {
                    Bank = this.Text,
                };
            }

            // Update the details and save the file
            Profile.CardNumber = txtCardNumber.Text.Trim();
            Profile.Description = txtDescription.Text.Trim();
            Profile.Password = txtPassword.Text.Trim();
            
            // Let the main window know everything is good
            DialogResult = DialogResult.OK;
        }
    }
}
