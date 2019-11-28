using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Specto.Relay
{
    public static class Network
    {
        public class WiFiData
        {
            public string SSID { get; private set; }
            public string Password { get; set; }
            public bool IsProtected { get; private set; }

            public WiFiData(string ssid, bool isProtected)
            {
                SSID = ssid;
                IsProtected = isProtected;

                if (!IsProtected) 
                    Password = "";
            }

            public WiFiData(string ssid, string password, bool isProtected)
            {
                SSID = ssid;
                Password = password;
                IsProtected = isProtected;

                if (!IsProtected)
                    Password = "";
            }
        }

        public static IPAddress GetBroadcastAddress(this IPAddress address) 
            => GetBroadcastAddress(address, address.ReturnSubnetmask());

        public static IPAddress LocalIP()
        {
            string localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address.ToString();
            }
            return IPAddress.Parse(localIP);
        }

        private static IPAddress GetBroadcastAddress(IPAddress address, IPAddress subnetMask)
        {
            byte[] ipAdressBytes = address.GetAddressBytes();
            byte[] subnetMaskBytes = subnetMask.GetAddressBytes();

            if (ipAdressBytes.Length != subnetMaskBytes.Length)
                throw new ArgumentException("Lengths of IP address and subnet mask do not match.");

            byte[] broadcastAddress = new byte[ipAdressBytes.Length];
            for (int i = 0; i < broadcastAddress.Length; i++) 
                broadcastAddress[i] = (byte)(ipAdressBytes[i] | (subnetMaskBytes[i] ^ 255));
            
            return new IPAddress(broadcastAddress);
        }

        private static IPAddress ReturnSubnetmask(this IPAddress address)
        {
            string ipaddress = address.ToString();
            uint firstOctet = ReturnFirtsOctet(ipaddress);
            string subnet = "0.0.0.0";

            if (firstOctet >= 0 && firstOctet <= 127)
                subnet = "255.0.0.0";
            else if (firstOctet >= 128 && firstOctet <= 191)
                subnet = "255.255.0.0";
            else if (firstOctet >= 192 && firstOctet <= 223)
                subnet = "255.255.255.0";

            return IPAddress.Parse(subnet);
        }

        public static uint ReturnFirtsOctet(string ipAddress)
        {
            System.Net.IPAddress iPAddress = System.Net.IPAddress.Parse(ipAddress);
            byte[] byteIP = iPAddress.GetAddressBytes();
            uint ipInUint = (uint)byteIP[0];
            return ipInUint;
        }
    }
}
