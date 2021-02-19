using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace CSaN_1
{
    internal class Program
    {
        [DllImport("iphlpapi.dll", ExactSpelling = true)]
        private static extern int SendARP(int DestIP, int SrcIP, byte[] pMacAddr, ref uint PhyAddrLen);

        private static Dictionary<int, Boolean> nodeAddresses;
        private static void ProcessNetworkInterface(NetworkInterface networkInterface)
        {
            Console.WriteLine("--------------");
            Console.WriteLine("Network Interface: " + networkInterface.Description);
            var ipv4Mask = networkInterface.GetIPProperties().UnicastAddresses[1].IPv4Mask.GetAddressBytes();
            var ipAddress = networkInterface.GetIPProperties().UnicastAddresses[1].Address.GetAddressBytes();
            byte[] nodeAddress = GetNodeAddress(ipv4Mask, ipAddress);
            Console.Write("Address of node: ");
            foreach (var b in nodeAddress)
            {
                Console.Write(b + ".");
            }
            Console.WriteLine();
            if (nodeAddress[0] != 192 || nodeAddress[1] != 168 || nodeAddress[2] != 2 || nodeAddress[3] != 0) return;
            if (nodeAddresses.ContainsKey(AddressToInt(nodeAddress))) return;
            nodeAddresses.Add(AddressToInt(nodeAddress), true);
            CheckField(nodeAddress);
        }

        private static void CheckField(byte[] nodeAddress)
        {
            bool isEndpoint = true;
            for (int i = 0; i < 4; i++)
            {
                if (nodeAddress[i] == 0)
                {
                    for (byte j = 1; j < 255; j++)
                    {
                        byte[] address = nodeAddress;
                        address[i] = j;
                        CheckField(address);
                        isEndpoint = false;
                    }
                }
            }
            if (isEndpoint) CheckAddress(nodeAddress);
        }

        private static void CheckAddress(byte[] address)
        {
            Console.Write("ip:");
            foreach (var b in address)
            {
                Console.Write(b + ".");
            }
            Console.Write(": ");
            byte[] macAddress = new byte[6];
            uint macAddressLength = (uint) macAddress.Length;
            if (SendARP(AddressToInt(address), 0, macAddress, ref macAddressLength) == 0)
            {
                Console.Write("Device: ");
                for (int b = 0; b < 6; b++)
                {
                    Console.Write(Convert.ToString(macAddress[b], 16) + ":");
                }
            }
            Console.WriteLine();
        }

        private static int AddressToInt(byte[] address)
        {
            int result = 0;
            for (int i = 3; i > -1; i--)
            {
                result |= ((int) address[i] << ((3 - i) * 8));
            }
            return result;
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
            nodeAddresses = new Dictionary<int, Boolean>();
            
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var networkInterface in networkInterfaces)
            {
                ProcessNetworkInterface(networkInterface);
            }
        }
    }
}