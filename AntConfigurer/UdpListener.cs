using AntConfigurer.Objects;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Linq;
using System.Text;
using System.Net;
using System;

namespace AntConfigurer
{
    class UdpListener
    {
        protected static IPEndPoint RemoteIpEndPoint;
        protected static UdpClient UdpClient;
        
        protected static bool Listening = false;
        protected static String[] Data;
        
        public static FormDevice Winform;
        public static TextBox Log;
        
        protected static List<AsicDevice> Devices = new List<AsicDevice>();

        public static void Start()
        {
            if (UdpListener.Listening)
                return;

            UdpListener.Data = new String[] { "", "" };

            if (UdpListener.RemoteIpEndPoint == null)
            {
                UdpListener.RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
                UdpListener.UdpClient = new UdpClient(14235);
            }

            // Reset list of devices
            UdpListener.Devices.Clear();

            UdpListener.UdpClient.BeginReceive(new AsyncCallback(UdpListener.ReceiveCallback), null);
            UdpListener.Listening = true;
        }

        public static void Stop()
        {
            if (!UdpListener.Listening)
                return;

            UdpListener.Listening = false;
        }

        public static void ReceiveCallback(IAsyncResult result)
        {
            if (!UdpListener.Listening)
                return;

            UdpListener.UdpClient.BeginReceive(new AsyncCallback(UdpListener.ReceiveCallback), null);

            Byte[] receiveBytes = UdpClient.EndReceive(result, ref UdpListener.RemoteIpEndPoint);
            string returnData = Encoding.ASCII.GetString(receiveBytes).ToString();

            Console.WriteLine("Recived UDP Datagram '{0}'", returnData);

            Byte[] ok = new Byte[] { 0x4F, 0x4B, 0x00, 0x00, 0x73, 0x65, 0x6E, 0x64, 0x5F, 0x61 };

            var endpoint = UdpListener.RemoteIpEndPoint;
            var remoteIp = endpoint.Address.ToString();
            
            // var log = (TextBox)(Winform.Controls.Find("log_box", true)[0]);
            // TextBox log = (TextBox)(Winform.Controls.Find("log_box", true)[0]);            

            Log.Invoke(new Action(() => Log.AppendText("Received data UDP datagram from " + remoteIp + "\r\n")));

            if (receiveBytes.SequenceEqual(ok))
            {                
                var foundDevices = UdpListener.Devices.Where(x => x.IpAddr == remoteIp).ToList();
               
                // We need to check if this is a malformed request from device
                // If device count is 1, then everything is OK
                if (foundDevices.Count == 1)
                {
                    var item = UdpListener.Devices.Single(a => a.IpAddr == remoteIp);
                    item.Confirmed = true;                    
                   
                    Log.Invoke(new Action(() => Log.AppendText("ASIC with IP " + remoteIp + " was confirmed\r\n")));

                    return;
                } 
                else Log.Invoke(new Action(() => Log.AppendText("Received malformed request from IP " + remoteIp + "\r\n")));
            } 
            else
            {
                String[] data = returnData.ToString().Split(',');

                // Is it a malformed request?
                if (data.Length != 2)
                {
                    // UdpListener.udpClient.BeginReceive(new AsyncCallback(UdpListener.ReceiveCallback), null);
                    return; // Incorrect data
                } 
                else
                {
                    // Could it be that ASIC send request one more time?
                    var item = UdpListener.Devices.Where(x => x.IpAddr == remoteIp).ToList();
                    
                    if (item.Count == 0)
                    {
                        var device = new AsicDevice();
                        device.IpAddr = remoteIp;
                        device.MacAddr = data[1];
                        device.RealIpAddr = data[0];

                        UdpListener.Devices.Add(device);

                        IPAddress serverAddr = IPAddress.Parse(data[0]);
                        IPEndPoint endPoint = new IPEndPoint(serverAddr, UdpListener.RemoteIpEndPoint.Port);
                        
                        byte[] sendBuffer = Encoding.ASCII.GetBytes(data[1]);
                        UdpListener.UdpClient.Send(sendBuffer, sendBuffer.Length, endPoint);
                    } 
                    else Console.WriteLine("Duplicate");

                    // UdpListener.udpClient.BeginReceive(new AsyncCallback(UdpListener.ReceiveCallback), null);
                    return;
                }
            }

            // UdpListener.udpClient.BeginReceive(new AsyncCallback(UdpListener.ReceiveCallback), null);
        }
        
        public static Boolean IsListening()
        {
            return UdpListener.Listening;
        }
        
        public static List<AsicDevice> GetDevices()
        {
            return UdpListener.Devices;
        }
    }
}