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

namespace BLE
{
    internal class Program
    {

        static async Task Main(string[] args)
        {
            byte[] test = new byte[3];
            test[0] = 0x00;
            test[1] = 0x00;
            test[2] = 0x12;
            byte[] res = Commands.cmdInfoConc(test, Encoding.UTF8.GetBytes("AT+HELLO Hello Sun"));
            connectionManager Manager = new connectionManager();
            if(! await Manager.initializeManager()|| ! await Manager.startAdvertising()){
                Console.WriteLine("Initialization failed. Press enter to close the app");
                Console.ReadLine();
                return;
            }
            Manager.handleInput(res);
            //Manager.socketManager.send(Commands.formatCommand(socketStream.Internal,sentCommands.sucess));
            //byte[] tmp;
            //while (true) {
            //    tmp = Manager.socketManager.recieveInput().Result;
            //    if (tmp != null)
            //    {
            //        Manager.handleInput(tmp);
            //        tmp = null;
            //    }
            //}
        }  
    }
}
