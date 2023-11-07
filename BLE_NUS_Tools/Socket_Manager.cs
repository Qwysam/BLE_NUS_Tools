using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BLE
{
    public class socketManager
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        string ip = "127.0.0.1";
        int port = 1337;
        public socketManager(){
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1337);
            try { socket.Connect(ipEndPoint); }
            catch (Exception ex) {
            Console.WriteLine(ex.Message);
            }
        }  
        public async void send(byte[] info){
            socket.Send(info);
            _ = await socket.SendAsync(info, SocketFlags.None);
        }
        public async Task recieveInput()
        {
            var buffer = new byte[1_024];
            await socket.ReceiveAsync(buffer);
            Console.WriteLine("Message from socket:");
            string res = Encoding.UTF8.GetString(buffer).Trim('\0');
            Console.WriteLine(res);
            res = res.Trim('\n');
            if (res == "Hello")
                await socket.SendAsync(Encoding.UTF8.GetBytes("Please kill me"));
            if(res =="Fuck you")
                await socket.SendAsync(Encoding.UTF8.GetBytes("No, fuck you"));
        }
        ~socketManager(){
            socket.Shutdown(SocketShutdown.Both);
        }
    }
}
