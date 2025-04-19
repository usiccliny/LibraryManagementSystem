using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client.Forms
{
    public partial class PasswordForm : Form
    {
        private string roleType;

        public PasswordForm(string roleType)
        {
            InitializeComponent();
            this.roleType = roleType;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            string correctPassword = roleType == "minor" ? "minor123" : "major123";

            if (txtPassword.Text == correctPassword)
            {
                this.DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("Неверный пароль", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
