using System;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BLE
{
    public class Constants
    {
        // BT_Code: Initializes custom local parameters w/ properties, protection levels as well as common descriptors like User Description. 
        public static readonly GattLocalCharacteristicParameters gattNUSRX = new GattLocalCharacteristicParameters
        {
            CharacteristicProperties = GattCharacteristicProperties.Write |
                                       GattCharacteristicProperties.WriteWithoutResponse,
            WriteProtectionLevel = GattProtectionLevel.Plain,
            UserDescription = "Operand Characteristic"
        };

        public static readonly GattLocalCharacteristicParameters gattNUSTX = new GattLocalCharacteristicParameters
        {
            CharacteristicProperties = GattCharacteristicProperties.Read |
                                       GattCharacteristicProperties.Notify,
            WriteProtectionLevel = GattProtectionLevel.Plain,
            UserDescription = "Operator Characteristic"
        };

        public static readonly Guid nordicServiceUuid = Guid.Parse("6E400001-B5A3-F393-E0A9-E50E24DCCA9E");

        public static readonly Guid nordicRXCharacteristicUuid = Guid.Parse("6E400002-B5A3-F393-E0A9-E50E24DCCA9E");
        public static readonly Guid nordicTXCharacteristicUuid = Guid.Parse("6E400003-B5A3-F393-E0A9-E50E24DCCA9E");
    }
}
