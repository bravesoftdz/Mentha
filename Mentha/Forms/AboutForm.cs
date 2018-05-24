using System.Diagnostics;
using System.Windows.Forms;

namespace Mentha.Forms {
    public partial class AboutForm : Form {
        public AboutForm() {
            InitializeComponent();
        }

        private void rtbAbout_LinkClicked(object sender, LinkClickedEventArgs e) {
            Process.Start(e.LinkText);
        }
    }
}
