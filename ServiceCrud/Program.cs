using Client.Communication;

namespace ServiceCrud
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string instanceId = Guid.NewGuid().ToString();
            string ipAddress = NetworkHelper.GetLocalIPAddress();

            var udpDiscoveryForMinor = new UdpDiscoveryClient(9999);
            var (dispatcherIp, dispatcherPort) = udpDiscoveryForMinor.DiscoverDispatcher();

            var minorServer = new Server(instanceId, ipAddress, dispatcherIp, dispatcherPort);
            minorServer.Start();
        }
    }
}
