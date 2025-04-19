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
        private Label lblCurrentRole;

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
            this.btnSearchBooks = new Button();
            this.btnViewNotifications = new Button();
            this.btnExit = new Button();
            this.lblWelcome = new Label();
            this.lblCurrentRole = new Label();

            // 
            // lblWelcome
            // 
            this.lblWelcome.AutoSize = true;
            this.lblWelcome.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point);
            this.lblWelcome.Location = new Point(10, 30);
            this.lblWelcome.Name = "lblWelcome";
            this.lblWelcome.Size = new Size(300, 38);
            this.lblWelcome.Text = "Добро пожаловать в библиотеку!";
            this.lblWelcome.ForeColor = Color.DarkBlue;

            // 
            // btnSearchBooks
            // 
            this.btnSearchBooks.Location = new Point(50, 100);
            this.btnSearchBooks.Name = "btnSearchBooks";
            this.btnSearchBooks.Size = new Size(200, 50);
            this.btnSearchBooks.Text = "Поиск книг";
            this.btnSearchBooks.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            this.btnSearchBooks.BackColor = Color.LightGreen;
            this.btnSearchBooks.ForeColor = Color.Black;
            //this.btnSearchBooks.Click += new EventHandler(this.btnSearchBooks_Click);

            // 
            // btnViewNotifications
            // 
            this.btnViewNotifications.Location = new Point(50, 170);
            this.btnViewNotifications.Name = "btnViewNotifications";
            this.btnViewNotifications.Size = new Size(200, 50);
            this.btnViewNotifications.Text = "Просмотр уведомлений";
            this.btnViewNotifications.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            this.btnViewNotifications.BackColor = Color.LightSkyBlue;
            this.btnViewNotifications.ForeColor = Color.Black;
            //this.btnViewNotifications.Click += new EventHandler(this.btnViewNotifications_Click);

            // 
            // btnExit
            // 
            this.btnExit.Location = new Point(50, 240);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new Size(200, 50);
            this.btnExit.Text = "Выход";
            this.btnExit.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            this.btnExit.BackColor = Color.LightCoral;
            this.btnExit.ForeColor = Color.Black;
            //this.btnExit.Click += new EventHandler(this.btnExit_Click);

            // 
            // MainForm
            // 
            this.ClientSize = new Size(300, 350);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnViewNotifications);
            this.Controls.Add(this.btnSearchBooks);
            this.Controls.Add(this.lblWelcome);
            this.Name = "MainForm";
            this.Text = "Главное меню";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            this.lblCurrentRole.AutoSize = true;
            this.lblCurrentRole.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.lblCurrentRole.Location = new System.Drawing.Point(50, 50);
            this.lblCurrentRole.Text = "Текущая роль: Не определена";

            this.Controls.Add(this.lblCurrentRole);
        }

        private void InitializeMinorServerInterface()
        {
            this.Text = "Минорный сервер";
            this.Size = new System.Drawing.Size(400, 300);

            var lblInfo = new Label
            {
                Text = "Интерфейс минорного сервера",
                Location = new System.Drawing.Point(50, 50),
                AutoSize = true
            };

            this.Controls.Add(lblInfo);
        }

        private void InitializeMajorServerInterface()
        {
            this.Text = "Мажорный сервер";
            this.Size = new System.Drawing.Size(600, 400);

            var lblInfo = new Label
            {
                Text = "Интерфейс мажорного сервера",
                Location = new System.Drawing.Point(50, 50),
                AutoSize = true
            };

            this.Controls.Add(lblInfo);
        }
    }
}