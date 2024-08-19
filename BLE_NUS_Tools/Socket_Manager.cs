using System.Net;
using System.Net.Sockets;
using System.Text.Json;

namespace BLE
{
    //class for managing the socket connection, sending and recieving data
    public class socketManager
    {
        //socket setup
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
        //basic method for sending processed data
        public void send(byte[] info){
            socket.Send(info);
        }

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

        }
        //destructor to close the connection when object is no longer used
        ~socketManager(){
            socket.Shutdown(SocketShutdown.Both);
        }
    }
}
