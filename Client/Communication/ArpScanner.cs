using System;
using System.Diagnostics;
using System.Collections.Generic;

public class ArpScanner
{
    public static List<string> GetActiveIPs()
    {
        var dynamicIPs = new List<string>();

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

                // Парсим вывод команды arp -a
                var lines = result.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    // Разделяем строку на части (IP, MAC, Type)
                    var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    // Проверяем, что строка содержит все три части: IP, MAC и Type
                    if (parts.Length >= 3 && IsValidIP(parts[0]) && parts[2].ToLower() == "динамический")
                    {
                        dynamicIPs.Add(parts[0]); // Добавляем IP-адрес в список
                    }
                }
            }
        }

        return dynamicIPs;
    }

    private static bool IsValidIP(string ip)
    {
        // Простая проверка формата IP-адреса
        return System.Net.IPAddress.TryParse(ip, out _);
    }
}