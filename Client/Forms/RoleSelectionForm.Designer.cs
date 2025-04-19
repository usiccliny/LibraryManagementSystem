using System.Drawing;
using System.Windows.Forms;

namespace Client.Forms
{
    partial class RoleSelectionForm
    {
        private Button btnStartAsClient;
        private Button btnStartAsMinorServer;
        private Button btnStartAsMajorServer;

        private void InitializeComponent()
        {
            this.btnStartAsClient = new Button();
            this.btnStartAsMinorServer = new Button();
            this.btnStartAsMajorServer = new Button();

            this.Text = "Выбор роли";
            this.Size = new System.Drawing.Size(350, 250);
            this.StartPosition = FormStartPosition.CenterScreen;

            this.btnStartAsClient.Text = "Запустить как клиент";
            this.btnStartAsClient.Location = new System.Drawing.Point(75, 30);
            this.btnStartAsClient.Size = new System.Drawing.Size(200, 40);
            this.btnStartAsClient.Click += new EventHandler(this.btnStartAsClient_Click);

            this.btnStartAsMinorServer.Text = "Запустить как минорный сервер";
            this.btnStartAsMinorServer.Location = new System.Drawing.Point(75, 90);
            this.btnStartAsMinorServer.Size = new System.Drawing.Size(200, 40);
            this.btnStartAsMinorServer.Click += new EventHandler(this.btnStartAsMinorServer_Click);

            this.btnStartAsMajorServer.Text = "Запустить как мажорный сервер";
            this.btnStartAsMajorServer.Location = new System.Drawing.Point(75, 150);
            this.btnStartAsMajorServer.Size = new System.Drawing.Size(200, 40);
            this.btnStartAsMajorServer.Click += new EventHandler(this.btnStartAsMajorServer_Click);

            this.Controls.Add(this.btnStartAsClient);
            this.Controls.Add(this.btnStartAsMinorServer);
            this.Controls.Add(this.btnStartAsMajorServer);
        }
    }
}