using Client.Communication;
using System;
using System.Net.Sockets;
using System.Text;

namespace Client.Models
{
    public class ClientС
    {
        private string majorServerIp;
        private int majorServerPort;

        public ClientС(string majorServerIp, int majorServerPort)
        {
            this.majorServerIp = majorServerIp;
            this.majorServerPort = majorServerPort;
        }

        public void RegisterClient(string instanceId)
        {
            var socketClient = new SocketClient(majorServerIp, majorServerPort);
            string message = $"REGISTER_CLIENT|{instanceId}";
            string response = socketClient.SendMessageAndGetResponse(message);
        }

        public List<(string IpAddress, int Port)> DiscoverServers(string role)
        {
            var servers = new List<(string IpAddress, int Port)>();
            var socketClient = new SocketClient(majorServerIp, majorServerPort);
            string message = $"GET_SERVERS_WITH_ROLE|{role}";
            string response = socketClient.SendMessageAndGetResponse(message);

            if (string.IsNullOrEmpty(response))
            {
                return servers;
            }

            servers = response.Split('|')
                .Where(s => !string.IsNullOrEmpty(s))
                .Select(s =>
                {
                    var parts = s.Split(':');
                    return (IpAddress: parts[0], Port: int.Parse(parts[1]));
                })
                .ToList();

            return servers;
        }
    }
}