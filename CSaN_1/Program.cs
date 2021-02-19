using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace CSaN_1
{
    internal class Program
    {
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        private static extern int SendARP(int DestIP, int SrcIP, byte[] pMacAddr, ref uint PhyAddrLen);
        private static void ProcessNetworkInterface(NetworkInterface networkInterface)
        {
            Console.WriteLine("--------------");
            Console.WriteLine("Network Interface: " + networkInterface.Description);
            var ipv4Mask = networkInterface.GetIPProperties().UnicastAddresses[0].IPv4Mask.GetAddressBytes();
            var ipAddress = networkInterface.GetIPProperties().UnicastAddresses[0].Address.GetAddressBytes();
            byte[] nodeAddress = GetNodeAddress(ipv4Mask, ipAddress);
        }

        private static byte[] GetNodeAddress(byte[] mask, byte[] ipAddress)
        {
            byte[] result = new byte[4];
            for (int i = 0; i < 4; i++)
            {
                int buf = mask[i] & ipAddress[i];
                result[i] = (byte)buf;
            }
            return result;
        }

        public static void Main(string[] args)
        {
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            foreach (var networkInterface in networkInterfaces)
            {
                ProcessNetworkInterface(networkInterface);
            }
        }
    }
}