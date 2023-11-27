# BLE_NUS_Tools

## 🔍 Software overview
This program is created to allow developers to turn their Windows PC into a BLE Peripheral device which advertises GATT [Nordic Uart Service](https://developer.nordicsemi.com/nRF_Connect_SDK/doc/latest/nrf/libraries/bluetooth_services/services/nus.html) with RX and TX characteristics according to the link above.

This can be very useful when Windows PC needs to act as a Peripheral device and **be** discovered by some other Central Devices. For example, this project is already succesfully working as a configuration software backbone for Nordicsemi nRF microcontrollers which are scanning for such peripherals and connect to them for the following data exchange.

Also, because this software is not a standalone program, it allows any developer with any programming language knowledge to use it thanks to the standardized JSON-based data transfer protocol and sockets. One just need to start their own software which will act as a socket server, then start BLE NUS TOOLS and begin working with it.

## ✨ Key features
- Native Windows BluetoothLE API without extra wrappers --> maximum performance and portability.
- Small and clean JSON-based data exchange protocol.
- Working with any custom software by using socket communication.

## 💿 Prerequisites
- Windows 10.0.20348.0 or later.
- Native Bluetooth 4.0+ adapter (Laptops) or quality BT dongle with advertisement support.
- Some custom-written software acting as a server and supporting BLE NUS TOOLS protocol.


## 🛠️ Installation and running


## 📨 Data exchange format


## 📃 Licensing
The project is licensed under MIT License. Refer to the LICENSE file included in this repository for more details.
