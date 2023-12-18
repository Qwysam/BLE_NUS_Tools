using BLE_NUS_Tools;
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
        static int MTU;
        static int currentReadCycle = 0, totalReadCycles = 0;
        static string readBuffer = "";

        public socketManager socketManager;
        public async Task<bool> initializeManager(string custom_ip, int custom_port){
            //check for launch parameters
            if (custom_ip == "null")
            {
                if (custom_port == -1)
                    socketManager = new socketManager();
                else
                    socketManager = new socketManager(custom_port);
            }
            else
            {
                if (custom_port != -1)
                    socketManager = new socketManager(custom_ip, custom_port);
                else
                    socketManager = new socketManager(custom_ip);
            }

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
        public async Task<bool> stopAdvertising()
        {
            serviceProvider.StopAdvertising();
            Console.WriteLine("Stopped advertising");
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

        // method to collect data from the board and then send it via a port
        private async void rxCharacteristic_WriteRequestedAsync(GattLocalCharacteristic sender, GattWriteRequestedEventArgs args)
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
                //first chunk of the package
                if(currentReadCycle == 0)
                {
                    reader.ByteOrder = ByteOrder.LittleEndian;
                    byte[] bytes = new byte[requestLength];
                    reader.ReadBytes(bytes);
                    int length = BitConverter.ToInt16(bytes.Take(2).ToArray(), 0);
                    int packageSize = MTU - 3;
                    totalReadCycles = (int)Math.Ceiling((double)length / packageSize);
                    readBuffer += Encoding.UTF8.GetString(bytes.Skip(2).ToArray());
                    currentReadCycle++;
                    return;
                }
                //last chunk of the package
                if (currentReadCycle == totalReadCycles - 1)
                {
                    byte[] bytes = new byte[requestLength];
                    reader.ReadBytes(bytes);
                    readBuffer += Encoding.UTF8.GetString(bytes);
                    currentReadCycle = 0;
                    totalReadCycles = 0;
                    socketManager.send(Encoding.UTF8.GetBytes(readBuffer));
                    return;
                }

                //standart chunk reading
                byte[] chunk = new byte[requestLength];
                reader.ReadBytes(chunk);
                readBuffer += Encoding.UTF8.GetString(chunk);
                currentReadCycle++;
                //data display for debugging
                //await txCharacteristic.NotifyValueAsync(arr.AsBuffer());
                //Console.WriteLine(val);
            }
        }

        private async void txCharacteristic_SubscribersChangedAsync(GattLocalCharacteristic sender, object args)
        {
            Console.WriteLine($"Now there are {sender.SubscribedClients.Count} subscribers");
            JsonDocument subscriberNotification = JsonDocument.Parse($"{{\"internal\": subscribers {sender.SubscribedClients.Count}}}");
            socketManager.send(Encoding.UTF8.GetBytes(subscriberNotification.RootElement.GetRawText()));
            if (sender.SubscribedClients.Count >= 1)
            {
                GattSession tmpSession = sender.SubscribedClients[sender.SubscribedClients.Count - 1].Session;
                if (tmpSession != null)
                {
                    Console.WriteLine($"{tmpSession.MaxPduSize} - max MTU size for device {tmpSession.DeviceId.Id}");
                    MTU = tmpSession.MaxPduSize;
                }
            }
        }

        public async Task handleInput(JsonDocument doc)
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
                await handleInputCommandAsync(command);
            }
        }
        private async Task handleInputCommandAsync(JsonElement command)
        {
            JsonDocument jsonResponse;
            switch (command.GetString())
            {
                case "mtu?":
                    //return MTU - 3 service bytes
                    jsonResponse = JsonDocument.Parse($"{{\"response\": {MTU-3}}}");
                    socketManager.send(Encoding.UTF8.GetBytes(jsonResponse.RootElement.GetRawText()));
                    break;
                case "hello":
                    jsonResponse = JsonDocument.Parse("{\"response\": 0}");
                    socketManager.send(Encoding.UTF8.GetBytes(jsonResponse.RootElement.GetRawText()));
                    break;
                case "startadv":
                    if (!await startAdvertising())
                    {
                        jsonResponse = JsonDocument.Parse("{\"response\": 2}");
                        socketManager.send(Encoding.UTF8.GetBytes(jsonResponse.RootElement.GetRawText()));
                    }
                    else
                    {
                        jsonResponse = JsonDocument.Parse("{\"response\": 0}");
                        socketManager.send(Encoding.UTF8.GetBytes(jsonResponse.RootElement.GetRawText()));
                    }
                    break;
                case "stopadv":
                    if (!await stopAdvertising())
                    {
                        //Handle error response?
                    }
                    else
                    {
                        jsonResponse = JsonDocument.Parse("{\"response\": 0}");
                        socketManager.send(Encoding.UTF8.GetBytes(jsonResponse.RootElement.GetRawText()));
                    }
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
            if (MTU == 0)
                return;
            // TODO add condition for Base64 encoding
            JsonElement payload;
            if(data.TryGetProperty("payload", out payload))
            {
                int packageSize = MTU - 3;
                byte[] received = Encoding.UTF8.GetBytes(payload.GetRawText());
                received = received.Skip(1).Take(received.Length - 2).ToArray();
                byte[] transfer = new byte[received.Length+2];
                byte[] sizeEncoding = lengthToBigEndian(transfer.Length-2);
                Array.Copy(sizeEncoding,transfer, sizeEncoding.Length);
                Array.Copy(received, 0, transfer,2, received.Length);
                int numberOfChunks = (int)Math.Ceiling((double)transfer.Length / packageSize);

                for (int i = 0; i < numberOfChunks; i++)
                {
                    int offset = i * packageSize;
                    int chunkSize = Math.Min(packageSize, transfer.Length - offset);
                    byte[] chunk = new byte[chunkSize];
                    System.Buffer.BlockCopy(transfer, offset, chunk, 0, chunkSize);

                    await txCharacteristic.NotifyValueAsync(chunk.AsBuffer());
                }
            }
        }

        public JsonDocument recieveInput()
        {
            return socketManager.recieveInput().Result;
        }
        private static byte[] lengthToBigEndian(int length)
        {
            byte[] byteArray = BitConverter.GetBytes((ushort)length);
            Array.Reverse(byteArray);
            return byteArray;
        }
    }
}
