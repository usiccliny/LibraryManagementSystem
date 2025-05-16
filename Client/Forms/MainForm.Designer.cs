using System.Drawing;
using System.Windows.Forms;

namespace Client.Forms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private Button btnSearchBooks;
        private Button btnViewNotifications;
        private Button btnExit;
        private Label lblWelcome;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            btnSearchBooks = new Button();
            SuspendLayout();
            // 
            // btnSearchBooks
            // 
            btnSearchBooks.BackColor = Color.LightGreen;
            btnSearchBooks.Font = new Font("Segoe UI", 12F);
            btnSearchBooks.ForeColor = Color.Black;
            btnSearchBooks.Location = new Point(29, 73);
            btnSearchBooks.Name = "btnSearchBooks";
            btnSearchBooks.Size = new Size(238, 183);
            btnSearchBooks.TabIndex = 0;
            btnSearchBooks.Text = "Работа с объектами";
            btnSearchBooks.UseVisualStyleBackColor = false;
            // 
            // MainForm
            // 
            ClientSize = new Size(300, 350);
            Controls.Add(btnSearchBooks);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Главное меню";
            ResumeLayout(false);
        }
    }
}