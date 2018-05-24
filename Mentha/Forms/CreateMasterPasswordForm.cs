using Mentha.Code;
using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Mentha.Forms {
    public partial class CreateMasterPasswordForm : Form {
        public CreateMasterPasswordForm() {
            InitializeComponent();
        }

        private void cmdSave_Click(object sender, EventArgs e) {
            // Validate inputs
            if (txtMasterPassword.Text.Trim() == string.Empty) {
                MessageBox.Show("Please enter a Master Password");
                txtMasterPassword.Focus();
                return;
            }
            if (txtConfirmMasterPassword.Text.Trim() == string.Empty) {
                MessageBox.Show("Please confirm the Master Password");
                txtConfirmMasterPassword.Focus();
                return;
            }
            if (txtMasterPassword.Text.Trim() != txtConfirmMasterPassword.Text.Trim()) {
                MessageBox.Show("Master Passwords do not match");
                return;
            }

            // Inputs OK, update the global variable
            Globals.MasterPassword = txtMasterPassword.Text.Trim();
            DialogResult = DialogResult.OK;
        }

        private void rtbInfo_LinkClicked(object sender, LinkClickedEventArgs e) {
            Process.Start(e.LinkText);
        }
    }
}
