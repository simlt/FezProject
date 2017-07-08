using System;
using Microsoft.SPOT;
using Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;
using Gadgeteer.Networking;
using System.Net;

namespace GadgeteerApp
{
    class Network
    {
        private EthernetJ11D ethernet;
        public enum NetworkState { Down, Up };
        public delegate void NetworkEventHandler(Network sender, NetworkState status);
        public event NetworkEventHandler NetworkStateChange;

        // This is used to setup the physical network
        public void Setup(EthernetJ11D eth)
        {
            // Fez 11 MAC: 00-21-03-80-4A-49
            ethernet = eth;
            ethernet.DebugPrintEnabled = true;
            ethernet.NetworkUp += Ethernet_NetworkUp;
            ethernet.NetworkDown += Ethernet_NetworkDown;

            ethernet.UseStaticIP("192.168.10.2", "255.255.255.0", "192.168.10.1");
            ethernet.UseThisNetworkInterface();
            //ethernet.NetworkInterface.Open();
            //ethernet.NetworkSettings.RenewDhcpLease();
        }

        private void Ethernet_NetworkUp(Module.NetworkModule sender, Module.NetworkModule.NetworkState state)
        {
            Debug.Print("# Network up");
            ListNetworkInterfaces();
            NetworkStateChange(this, NetworkState.Up);
        }

        private void Ethernet_NetworkDown(Module.NetworkModule sender, Module.NetworkModule.NetworkState state)
        {
            Debug.Print("# Network down");
            NetworkStateChange(this, NetworkState.Down);
        }

        private void ListNetworkInterfaces()
        {
            var settings = ethernet.NetworkSettings;

            Debug.Print("------------------------------------------------");
            //Debug.Print("MAC: " + BitConverter.ToString(settings.PhysicalAddress));
            Debug.Print("IP Address:   " + settings.IPAddress);
            Debug.Print("DHCP Enabled: " + settings.IsDhcpEnabled);
            Debug.Print("Subnet Mask:  " + settings.SubnetMask);
            Debug.Print("Gateway:      " + settings.GatewayAddress);
            Debug.Print("------------------------------------------------");
        }
    }
}
