using System;
using System.Collections.Generic;
using System.IO;
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
            if (networkInterface.OperationalStatus != OperationalStatus.Up) return;
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
            for (int i = 0; i < 2; i++)
            {
                byte buf = nodeAddress[i];
                nodeAddress[i] = nodeAddress[3 - i];
                nodeAddress[3 - i] = buf;
            }
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
            byte[] macAddress = new byte[6];
            uint macAddressLength = (uint) macAddress.Length;
            if (SendARP(AddressToInt(address), 0, macAddress, ref macAddressLength) == 0)
            {
                Console.Write("ip:");
                for (int i = 3; i > 0; i--)
                {
                    Console.Write(address[i] + ".");
                }
                Console.Write(address[0] + ": ");
                Console.Write("Device: ");
                string mac = "";
                for (int b = 0; b < macAddressLength - 1; b++)
                {
                    mac += (Convert.ToString(macAddress[b], 16) + ":");
                }
                mac += Convert.ToString(macAddress[macAddressLength - 1], 16);
                Console.Write(mac + " ");

                try
                {
                    WebRequest request = WebRequest.Create("https://api.macvendors.com/" + mac);
                    using (WebResponse response = request.GetResponse())
                    {
                        using (Stream stream = response.GetResponseStream())
                        {
                            using (StreamReader streamReader = new StreamReader(stream))
                            {
                                string vendor = streamReader.ReadLine();
                                Console.Write("Vendor: " + vendor);
                            }
                        }
                    }
                }
                catch (WebException e)
                {
                    Console.Write("Vendor not found");
                }
                Console.WriteLine();
            }
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
            
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var networkInterface in networkInterfaces)
            {
                ProcessNetworkInterface(networkInterface);
            }
            
            
        }
    }
}