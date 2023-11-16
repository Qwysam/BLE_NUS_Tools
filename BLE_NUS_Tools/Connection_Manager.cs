using BLE_NUS_Tools;
using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.Json;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace BLE
{
    public class connectionManager
    {
        private static GattLocalCharacteristic txCharacteristic;
        private static GattLocalCharacteristic rxCharacteristic;
        GattServiceProvider serviceProvider;

        public socketManager socketManager;
        public async Task<bool> initializeManager(){
            socketManager = new socketManager();
            GattServiceProviderResult serviceResult = await GattServiceProvider.CreateAsync(Constants.nordicServiceUuid);
            // BT_Code: Initialize and starting a custom GATT Service using GattServiceProvider.

            if (serviceResult.Error == BluetoothError.Success)
            {
                serviceProvider = serviceResult.ServiceProvider;
                Console.WriteLine("Initialization successful");
            }
            else
            {
                Console.WriteLine("Initialization failed");
                return false;
            }


            GattLocalCharacteristicResult result = await serviceProvider.Service.CreateCharacteristicAsync(Constants.nordicRXCharacteristicUuid, Constants.gattNUSRX);
            if (result.Error == BluetoothError.Success)
            {
                rxCharacteristic = result.Characteristic;
            }
            else
            {
                Console.WriteLine($"Could not create RX characteristic: {result.Error}");
                return false;

            }
            rxCharacteristic.WriteRequested += rxCharacteristic_WriteRequestedAsync;
            result = await serviceProvider.Service.CreateCharacteristicAsync(Constants.nordicTXCharacteristicUuid, Constants.gattNUSTX);
            if (result.Error == BluetoothError.Success)
            {
                txCharacteristic = result.Characteristic;
            }
            else
            {
                Console.WriteLine($"Could not create TX characteristic: {result.Error}");
                return false;

            }
            txCharacteristic.SubscribedClientsChanged += txCharacteristic_SubscribersChangedAsync;

            return true;
        }
        public async Task<bool> startAdvertising(){
            if(!await CheckPeripheralRoleSupportAsync())
            {
                Console.WriteLine("Peripheral Mode unsupported.");
                return false;
            }
                        // BT_Code: Indicate if your sever advertises as connectable and discoverable.
            GattServiceProviderAdvertisingParameters advParameters = new GattServiceProviderAdvertisingParameters
            {
                // IsConnectable determines whether a call to publish will attempt to start advertising and 
                // put the service UUID in the ADV packet (best effort)
                IsConnectable = true,

                // IsDiscoverable determines whether a remote device can query the local device for support 
                // of this service
                IsDiscoverable = true
            };
            serviceProvider.StartAdvertising(advParameters);
            Console.WriteLine("Started advertising");
            return true;
        }


        private static async Task<bool> CheckPeripheralRoleSupportAsync()
        {
            // BT_Code: New for Creator's Update - Bluetooth adapter has properties of the local BT radio.
            var localAdapter = await BluetoothAdapter.GetDefaultAsync();

            if (localAdapter != null)
            {
                return localAdapter.IsPeripheralRoleSupported;
            }
            else
            {
                // Bluetooth is not turned on 
                return false;
            }
        }

        private static async void rxCharacteristic_WriteRequestedAsync(GattLocalCharacteristic sender, GattWriteRequestedEventArgs args)
        {
            // BT_Code: Processing a write request.
            using (args.GetDeferral())
            {
                // Get the request information.  This requires device access before an app can access the device's request.
                GattWriteRequest request = await args.GetRequestAsync();
                if (request == null)
                {
                    // No access allowed to the device.  Application should indicate this to the user.
                    return;
                }
                //ProcessWriteCharacteristic(request, CalculatorCharacteristics.Operand1);
                uint requestLength = request.Value.Length;
                Console.WriteLine($"Write to RX requested of {requestLength} bytes.");
                var reader = DataReader.FromBuffer(request.Value);
                reader.ByteOrder = ByteOrder.LittleEndian;
                string val = reader.ReadString(requestLength);
                byte[] arr = System.Text.Encoding.UTF8.GetBytes(val);
                await txCharacteristic.NotifyValueAsync(arr.AsBuffer());
                Console.WriteLine(val);
            }
        }

        private static async void txCharacteristic_SubscribersChangedAsync(GattLocalCharacteristic sender, object args)
        {
            Console.WriteLine($"Now there are {sender.SubscribedClients.Count} subscribers");
            if (sender.SubscribedClients.Count >= 1)
            {
                GattSession tmpSession = sender.SubscribedClients[sender.SubscribedClients.Count - 1].Session;
                if (tmpSession != null)
                {
                    Console.WriteLine($"{tmpSession.MaxPduSize} - max MTU size for device {tmpSession.DeviceId.Id}");
                }
            }
        }

        public void handleInput(JsonDocument doc)
        {
            //parse only payload from byte array    later divide into a separate method
            //string payload = Encoding.UTF8.GetString(input, 3, input.Length-3);
            //if (input[0] == socketStream.Internal)
            //    handleInpputCommand(payload);
            //if (input[0] == socketStream.Data)
            //    handleInpputData(payload);

            JsonElement data, command;
            if(doc.RootElement.TryGetProperty("datapipe", out data))
            {
                Console.WriteLine("Datapipe detected");
                handleInputData(data);
            }
            if(doc.RootElement.TryGetProperty("internal", out command))
            {
                Console.WriteLine("Command detected");
                handleInputCommand(command);
            }
        }
        private void handleInputCommand(JsonElement command)
        {
            switch (command.GetString())
            {
                case "mtu?":
                    break;
                case "hello":
                    JsonDocument jsonDocument = JsonDocument.Parse("{\"response\": 0}");
                    socketManager.send(Encoding.UTF8.GetBytes(jsonDocument.RootElement.GetRawText()));
                    Console.WriteLine(jsonDocument.RootElement.GetRawText());
                    break;
            }
        }
        //Method to handle input commands from the server
        private void handleInputCommand(string payload)
        {
            //test comms
            if(payload.Contains(receivedCommands.commTestSTR)) 
            {
                //response test
                byte[] text = Encoding.UTF8.GetBytes("Hello Moon");
                //add command code to the text
                byte[] res = Commands.cmdInfoConc(sentCommands.commsTest, text);
                socketManager.send(Commands.formatCommand(socketStream.Internal, res));
            }
        }
        //Method to transfer data from server to bluetooth characteristic
        private async void handleInputData(JsonElement data)
        {
            JsonElement payload;
            if(data.TryGetProperty("payload", out payload))
            {
                byte[] arr = Encoding.UTF8.GetBytes(payload.GetRawText());
                await txCharacteristic.NotifyValueAsync(arr.AsBuffer());
            }
        }

    }
}
