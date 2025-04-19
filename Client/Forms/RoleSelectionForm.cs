namespace Client.Forms
{
    public partial class RoleSelectionForm : Form
    {
        public string SelectedRole { get; private set; }

        public RoleSelectionForm()
        {
            InitializeComponent();
        }

        private void btnStartAsClient_Click(object sender, EventArgs e)
        {
            SelectedRole = "client";
            Close();
        }

        private void btnStartAsMinorServer_Click(object sender, EventArgs e)
        {
            var passwordForm = new PasswordForm("minor");
            if (passwordForm.ShowDialog() == DialogResult.OK)
            {
                SelectedRole = "minor_server";
                Close();
            }
        }

        private void btnStartAsMajorServer_Click(object sender, EventArgs e)
        {
            var passwordForm = new PasswordForm("major");
            if (passwordForm.ShowDialog() == DialogResult.OK)
            {
                SelectedRole = "major_server";
                Close();
            }
        }
    }
}