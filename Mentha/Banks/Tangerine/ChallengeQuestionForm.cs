using System;
using System.Windows.Forms;

namespace Mentha.Banks.Tangerine {
    public partial class ChallengeQuestionForm : Form {
        public string Answer { get; set; }

        public ChallengeQuestionForm(string question) {
            InitializeComponent();

            lblQuestion.Text = question;
        }

        private void cmdSubmit_Click(object sender, EventArgs e) {
            // Validate
            if (txtAnswer.Text.Trim() == string.Empty) {
                MessageBox.Show("Please enter an Answer");
                txtAnswer.Focus();
                return;
            }

            Answer = txtAnswer.Text.Trim();
            DialogResult = DialogResult.OK;
        }
    }
}
