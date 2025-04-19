namespace Client.Forms
{
    partial class PasswordForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblPassword = new Label();
            this.txtPassword = new TextBox();
            this.btnSubmit = new Button();

            this.Text = "Введите пароль";
            this.Size = new System.Drawing.Size(300, 150);
            this.StartPosition = FormStartPosition.CenterScreen;

            this.lblPassword.Text = "Пароль:";
            this.lblPassword.Location = new System.Drawing.Point(50, 20);
            this.lblPassword.AutoSize = true;

            this.txtPassword.Location = new System.Drawing.Point(50, 50);
            this.txtPassword.Size = new System.Drawing.Size(200, 20);
            this.txtPassword.PasswordChar = '*';

            this.btnSubmit.Text = "Подтвердить";
            this.btnSubmit.Location = new System.Drawing.Point(100, 80);
            this.btnSubmit.Size = new System.Drawing.Size(100, 30);
            this.btnSubmit.Click += new EventHandler(this.btnSubmit_Click);

            this.Controls.Add(this.lblPassword);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.btnSubmit);
        }

        private Label lblPassword;
        private TextBox txtPassword;
        private Button btnSubmit;

        #endregion
    }
}