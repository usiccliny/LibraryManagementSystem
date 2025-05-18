using System;
using System.Diagnostics;
using System.Collections.Generic;

public class ArpScanner
{
    public static List<string> GetActiveIPs()
    {
        var filteredIPs = new List<string>();

        // Выполняем команду arp -a
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = "arp",
            Arguments = "-a",
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(psi))
        {
            using (System.IO.StreamReader reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();

                var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length >= 1 && IsValidIP(parts[0]))
                    {
                        filteredIPs.Add(parts[0]); // Добавляем IP-адрес в список
                    }
                }
            }
        }

        return filteredIPs;
    }

    private static bool IsValidIP(string ip)
    {
        // Простая проверка формата IP-адреса
        return System.Net.IPAddress.TryParse(ip, out _);
    }
}