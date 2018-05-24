using Mentha.Code;
using System;
using System.Windows.Forms;

namespace Mentha.Forms {
    public partial class EnterMasterPasswordForm : Form {
        public EnterMasterPasswordForm() {
            InitializeComponent();
        }

        private void cmdOK_Click(object sender, EventArgs e) {
            // Validate inputs
            if (txtMasterPassword.Text.Trim() == string.Empty) {
                MessageBox.Show("Please enter a Master Password");
                txtMasterPassword.Focus();
                return;
            }

            // Inputs OK, update the global variable
            Globals.MasterPassword = txtMasterPassword.Text.Trim();
            DialogResult = DialogResult.OK;
        }
    }
}
