using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;

public static class NetworkHelper
{
    /// <summary>
    /// Получает IPv4-адрес для указанного имени сети.
    /// </summary>
    /// <param name="networkName">Имя сети (например, "Беспроводная сеть").</param>
    /// <returns>IPv4-адрес, связанный с указанной сетью.</returns>
    public static string GetIPv4AddressByNetworkName(string networkName = "Беспроводная сеть")
    {
        var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

        foreach (var networkInterface in networkInterfaces)
        {
            if ((networkInterface.Name.Equals(networkName, StringComparison.OrdinalIgnoreCase) ||
                 networkInterface.Description.Equals(networkName, StringComparison.OrdinalIgnoreCase)) &&
                networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
            {
                var ipProperties = networkInterface.GetIPProperties();

                var ipv4Address = ipProperties.UnicastAddresses
                    .Where(addr => addr.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(addr => addr.Address.ToString())
                    .FirstOrDefault();

                if (!string.IsNullOrEmpty(ipv4Address))
                {
                    return ipv4Address;
                }
            }
        }

        throw new Exception($"Не удалось найти IPv4-адрес для сети с именем '{networkName}'.");
    }

    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork)
            {
                return ip.ToString();
            }
        }
        throw new Exception("Не удалось найти IP-адрес устройства.");
    }
}
