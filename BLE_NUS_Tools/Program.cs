using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Foundation;
using Windows.Storage.Streams;
using Microsoft.VisualBasic;
using System.Runtime.InteropServices.WindowsRuntime;
using static System.Runtime.InteropServices.JavaScript.JSType;
using BLE;
using System.Net;
using System.Net.Sockets;
using System.Text;
using BLE_NUS_Tools;
using System.Text.Json;

namespace BLE
{
    internal class Program
    {

        static async Task Main(string[] args)
        {
            connectionManager Manager = new connectionManager();
            if(! await Manager.initializeManager()|| ! await Manager.startAdvertising()){
                Console.WriteLine("Initialization failed. Press enter to close the app");
                Console.ReadLine();
                return;
            }
            //Manager.socketManager.send(Commands.formatCommand(socketStream.Internal,sentCommands.sucess));
            while (true) {
                JsonDocument tmp = Manager.socketManager.recieveInput().Result;
                if (tmp != null)
                {
                    Manager.handleInput(tmp);
                    tmp = null;
                }
            }
        }  
    }
}
