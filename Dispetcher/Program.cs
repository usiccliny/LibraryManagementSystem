using System.Net;

namespace Dispetcher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string ipAddress = NetworkHelper.GetIPv4AddressByNetworkName();

            var majorServer = new Server(
                "major_server",
                ipAddress,
                5000);
            majorServer.Start();
        }
    }
}
