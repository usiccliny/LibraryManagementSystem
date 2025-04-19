using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client.Communication
{
    public class UdpDiscoveryClient
    {
        private int broadcastPort;

        public UdpDiscoveryClient(int broadcastPort)
        {
            this.broadcastPort = broadcastPort;
        }

        /// <summary>
        /// Отправляет широковещательное сообщение для обнаружения мажорного сервера.
        /// </summary>
        /// <param name="message">Сообщение для отправки.</param>
        public void SendBroadcastMessage(List<string> deviceIps, string message)
        {
            using (UdpClient udpClient = new UdpClient())
            {
                byte[] data = Encoding.UTF8.GetBytes(message);

                foreach (var ip in deviceIps)
                {
                    try
                    {
                        IPEndPoint endpoint = new IPEndPoint(IPAddress.Parse(ip), broadcastPort);
                        udpClient.Send(data, data.Length, endpoint);
                        Console.WriteLine($"Отправлено сообщение на устройство: {ip}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка при отправке сообщения на устройство {ip}: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// Ожидает широковещательное сообщение от мажорного сервера.
        /// </summary>
        /// <returns>IP-адрес и порт мажорного сервера.</returns>
        public (string DispatcherIp, int DispatcherPort) DiscoverDispatcher()
        {
            using (UdpClient udpClient = new UdpClient())
            {
                udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpClient.ExclusiveAddressUse = false;
                udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, broadcastPort));

                udpClient.Client.ReceiveTimeout = 10000; // Таймаут 10 секунд

                try
                {
                    IPEndPoint remoteEndPoint = null;
                    byte[] data = udpClient.Receive(ref remoteEndPoint);
                    string message = Encoding.UTF8.GetString(data);

                    var parts = message.Split(':');
                    if (parts.Length == 2 && int.TryParse(parts[1], out int port))
                    {
                        return (parts[0], port);
                    }
                }
                catch (SocketException ex)
                {
                    MessageBox.Show($"Ошибка при получении широковещательного сообщения: {ex.Message}");
                }

                throw new Exception("Не удалось найти мажорный сервер.");
            }
        }
    }
}