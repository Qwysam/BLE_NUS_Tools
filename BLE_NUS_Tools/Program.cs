﻿using System;
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

namespace BLE
{
    internal class Program
    {
        private static GattLocalCharacteristic txCharacteristic;
        private static GattLocalCharacteristic rxCharacteristic;

        static async Task Main(string[] args)
        {
            GattServiceProvider serviceProvider = null;
            // BT_Code: Initialize and starting a custom GATT Service using GattServiceProvider.
            GattServiceProviderResult serviceResult = await GattServiceProvider.CreateAsync(Constants.nordicServiceUuid);
            if (serviceResult.Error == BluetoothError.Success)
            {
                serviceProvider = serviceResult.ServiceProvider;
                Console.WriteLine("Initialization successful");
            }
            else
            {
                Console.WriteLine("Initialization failed");
            }

            GattLocalCharacteristicResult result = await serviceProvider.Service.CreateCharacteristicAsync(Constants.nordicRXCharacteristicUuid, Constants.gattNUSRX);
            if (result.Error == BluetoothError.Success)
            {
                rxCharacteristic = result.Characteristic;
            }
            else
            {
                Console.WriteLine($"Could not create RX characteristic: {result.Error}");

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

            }
            txCharacteristic.SubscribedClientsChanged += txCharacteristic_SubscribersChangedAsync;
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

            while (true) { }
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
        }
    }
}
