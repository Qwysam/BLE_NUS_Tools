using System;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BLE
{
    public class connectionManager
    {
        private  GattLocalCharacteristic txCharacteristic;
        private GattLocalCharacteristic rxCharacteristic;
        GattServiceProvider serviceProvider;
        public connectionManager(){
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
            }
        }
        public bool initializeManager(){
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
            

        }
        public bool startAdvertising(){
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

    }
}
