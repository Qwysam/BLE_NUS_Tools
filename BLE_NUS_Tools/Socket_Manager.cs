using System;
using System.Net;
using System.Net.Sockets;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BLE
{
    public class socketManager
    {
        Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        string ip = "127.0.0.1";
        int port = 1337;
        public socketManager(){
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1337);
            client.Connect(ipEndPoint);
            
        }  
        public async void send(byte[] info){
            client.Send(info);
            _ = await client.SendAsync(info, SocketFlags.None);
        }
        ~socketManager(){
            client.Shutdown(SocketShutdown.Both);
        }
    }
}
