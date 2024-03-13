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
1) The repository needs to be cloned, using the git clone command.
2) The source code needs to be compiled, using a standalone .Net compiler or an IDE.
3) The application should be launched through the terminal with or without the launch parameters(custom IP adress and custom port)

## 📨 Data exchange format
  Board-App Exchange
Data exchanges between the board and the app happens in a byte stream format with first two bytes of the first chunk representing the length of the whole message in the Big Endian format. This is done because payload is usually bigger then the MTU(Maximum Transmission Unit) and needs to be sent in several chunks, which are accumulated and concatenated by the app. The information from the board gets passed to the server application(not included in the repository) after formatting. The application in this repository is not responsble for encoding the data sent to the board, but will format the data recieved from the board to a UTF-8 JSON string with a "payload" attribute before sending it to the server.

  App-Server Exchange
Data exchange between the server and the app also happens in a UTF-8 JSON format. There are two tags a server can use: "internal" to transfer commands and "datapipe" to transfer data directly to the board. To each command the app will generate and send to the server information in the "response" tag.

Supported Command Table

| Request                                        | Payload                           | Possible response  | Payload |
| ---------------------------------------------- | --------------------------------- | ------------------ | ------- |
| [SRV] Start advertising                        | “startadv”                        | [CLN] Started OK   | 0       |
||| [CLN] Periph mode not supported                | 2                                 |
| [SRV] Stop advertising                         | “stopadv”                         | [CLN] Stopped OK   | 0       |
| [CLN] Subscribers count changed                | “subscribers N” where N is number | [SRV] Acknowledged | 0       |
||| [SRV] Wrong subscribers’ number (generic FAIL) | 1                                 |
| [SRV] Ask for negotiated MTU                   | “mtu?”                            | [CLN] MTU Value    | 1-N     |
||| [CLN] FAIL (for example, no active connection) | 0                                 |
| [SRV] Hello (general comms test)               | “hello”                           | [CLN] Hello        | “0”     |

## 📃 Licensing
The project is licensed under MIT License. Refer to the LICENSE file included in this repository for more details.
