using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

public static class NetworkHelper
{
    public static string GetLocalIPAddress()
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            // Проверяем, что адрес IPv4 и соответствует формату ***.***.0.***
            if (ip.AddressFamily == AddressFamily.InterNetwork && IsThirdOctetZero(ip.ToString()))
            {
                return ip.ToString();
            }
        }
        throw new Exception("Не удалось найти IP-адрес устройства с третьим октетом равным 0.");
    }

    private static bool IsThirdOctetZero(string ipAddress)
    {
        var parts = ipAddress.Split('.');
        if (parts.Length == 4 && int.TryParse(parts[2], out int thirdOctet))
        {
            return thirdOctet == 0;
        }
        return false;
    }
}