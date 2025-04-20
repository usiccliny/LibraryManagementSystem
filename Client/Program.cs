using Client.Forms;
using Client.Models;
using Client.Communication;

namespace Client
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string instanceId = Guid.NewGuid().ToString();
            string ipAddress = NetworkHelper.GetLocalIPAddress();

            var roleForm = new RoleSelectionForm();
            Application.Run(roleForm);

            string role = roleForm.SelectedRole;

            if (!string.IsNullOrEmpty(role))
            {
                switch (role)
                {
                    case "major_server":
                        var majorServer = new MajorServer(
                            "major_server",
                            ipAddress,
                            5000);
                        majorServer.Start();
                        break;

                    case "minor_server":
                        var udpDiscoveryForMinor = new UdpDiscoveryClient(9999);
                        var (dispatcherIp, dispatcherPort) = udpDiscoveryForMinor.DiscoverDispatcher();

                        var minorServer = new MinorServer(instanceId, ipAddress, dispatcherIp, dispatcherPort);
                        minorServer.Start();
                        break;
                }
            }

            var mainForm = new MainForm(role);
            Application.Run(mainForm);
        }
    }
}