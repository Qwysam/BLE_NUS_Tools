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
        //Needs testing
        public async Task<byte[]> recieveInput()
        {
            var buffer = new byte[65535];
            await socket.ReceiveAsync(buffer);
            Console.WriteLine("Message from socket:");
            byte[] payloadSize = new byte[2];
            payloadSize[0] = buffer[1];
            payloadSize[1] = buffer[2];
            byte[] result = new byte[ConvertBigEndianBytesToInt(payloadSize)+3];
            Array.Copy(buffer,0,result,0,result.Length);
            return result;

        }
        ~socketManager(){
            socket.Shutdown(SocketShutdown.Both);
        }

        private static int ConvertBigEndianBytesToInt(byte[] bytes)
        {
            // Make a copy of the array and reverse it to little-endian order
            byte[] reversedBytes = new byte[bytes.Length];
            Array.Copy(bytes, reversedBytes, bytes.Length);
            Array.Reverse(reversedBytes);

            // Convert the reversed array to an integer
            int result = BitConverter.ToInt16(reversedBytes, 0);

            return result;
        }
    }
}
