﻿using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace BLE
{
    public class socketManager
    {
        Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        string default_ip = "127.0.0.1";
        int default_port = 1337;
        public socketManager(){
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(default_ip), default_port);
            try { socket.Connect(ipEndPoint); }
            catch (Exception ex) {
            Console.WriteLine(ex.Message);
            }
        }

        public socketManager(string custom_ip)
        {
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(custom_ip), default_port);
            try { socket.Connect(ipEndPoint); }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public socketManager(int custom_port)
        {
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(default_ip), custom_port);
            try { socket.Connect(ipEndPoint); }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public socketManager(string custom_ip,int custom_port)
        {
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(custom_ip), custom_port);
            try { socket.Connect(ipEndPoint); }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public void send(byte[] info){
            socket.Send(info);
        }
        //Needs testing
        public async Task<JsonDocument> recieveInput()
        {
            var bufferSize = 65535;
            var buffer = new byte[bufferSize];

            // Read data from the socket
            int bytesRead = await socket.ReceiveAsync(buffer, SocketFlags.None);

            // Check if no data was received
            if (bytesRead == 0)
            {
                // Handle the case where the connection was closed
                // or no data was received
                return null;
            }

            // Create a new buffer to hold only the received bytes
            byte[] receivedData = new byte[bytesRead];
            Array.Copy(buffer, receivedData, bytesRead);

            // Parse the received JSON data
            JsonDocument result = JsonDocument.Parse(receivedData);

            // You can now use 'result' for further processing

            return result;

            //outdated code for byte operations
            //byte[] payloadSize = new byte[2];
            //payloadSize[0] = buffer[1];
            //payloadSize[1] = buffer[2];
            //byte[] result = new byte[ConvertBigEndianBytesToInt(payloadSize)+3];
            //Array.Copy(buffer,0,result,0,result.Length);

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
