using System;
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
        public bool send(byte[] info){
            client.Send(messageBytes);
            _ = await client.SendAsync(messageBytes, SocketFlags.None);
        }
        ~socketManager(){
            client.Shutdown(SocketShutdown.Both);
        }
    }
}
