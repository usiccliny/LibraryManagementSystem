using Client.Communication;
using Client.Models;
using System;
using System.Windows.Forms;

namespace Client.Forms
{
    public partial class MainForm : Form
    {
        private System.Windows.Forms.Timer sessionCheckTimer;
        private string role;
        private string majorServerIp;
        private int majorServerPort;
        string instanceId = Guid.NewGuid().ToString();

        public MainForm(string role)
        {
            this.role = role;
            UpdateFormBasedOnRole();
        }

        private void UpdateFormBasedOnRole()
        {
            this.Controls.Clear();

            switch (role)
            {
                case "client":
                    InitializeComponent();
                    this.lblCurrentRole.Text = "Текущая роль: Клиент";
                    btnSearchBooks.Click += BtnSearchBooks_Click;
                    break;
                case "minor_server":
                    InitializeMinorServerInterface();
                    break;
                case "major_server":
                    InitializeMajorServerInterface();
                    break;
            }
        }

        private void BtnSearchBooks_Click(object sender, EventArgs e)
        {
            var udpDiscoveryForClient = new UdpDiscoveryClient(9999);
            (majorServerIp, majorServerPort) = udpDiscoveryForClient.DiscoverDispatcher();

            MessageBox.Show($"Мажорный сервер найден: IP={majorServerIp}, Port={majorServerPort}");

            var client = new ClientС(majorServerIp, majorServerPort);
            client.RegisterClient(instanceId);

            var servers = client.DiscoverServers("crud");

            if (servers.Count == 0)
            {
                MessageBox.Show("Нет доступных минорных серверов с ролью CRUD.");
                return;
            }

            var selectedServer = servers[0];

            var crudForm = new CrudForm(selectedServer.IpAddress, selectedServer.Port);
            crudForm.ShowDialog();
        }
    }
}