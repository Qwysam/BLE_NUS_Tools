using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLE_NUS_Tools
{
    //binary data stream codes
    public struct socketStream
    {
        public static byte Internal = 0x00;
        public static byte Data = 0x01;
    }
    //Commands to be transferred to the python app
    public struct sentCommands
    {
        public static byte[] sucess = Encoding.UTF8.GetBytes("AT+OK");
        public static byte[] failure = Encoding.UTF8.GetBytes("AT+FAIL");
        public static byte[] peripheralUnsupported = Encoding.UTF8.GetBytes("AT+NSUPP");
        public static byte[] MTUanswer = Encoding.UTF8.GetBytes("AT+MTU ");
        public static byte[] commsTest = Encoding.UTF8.GetBytes("AT+HELLO");
        public static byte[] subCount = Encoding.UTF8.GetBytes("AT+SUB");

    }
    //Commands to be received to the python app
    public struct receivedCommands 
    {
        public static byte[] sucess = Encoding.UTF8.GetBytes("AT+OK");
        public static byte[] commsTest = Encoding.UTF8.GetBytes("AT+HELLO");
        public static byte[] startAdv = Encoding.UTF8.GetBytes("AT+STRADV");
        public static byte[] stopAdv = Encoding.UTF8.GetBytes("AT+STPADV");
        public static byte[] MTUrequst = Encoding.UTF8.GetBytes("AT+MTU?");

        public static string sucessSTR = "AT+OK";
        public static string commTestSTR = "AT+HELLO";
        public static string startAdvSTR = "AT+STRADV";
        public static string stopAdvSTR = "AT+STPADV";
        public static string MTUreqestSTR = "AT+MTU?";
    }

    //class to handle all the necessary array operations
    public static class Commands
    {
        //transform payload length to a Big Endian byte array
       private static byte[] lengthToBigEndian(int length)
        {
            byte[] byteArray = BitConverter.GetBytes((ushort)length);
            Array.Reverse(byteArray);
            return byteArray;
        }
        // Add stream identificator and payload length to the payload as specified in the documentation
       public static byte[] formatCommand(byte stream, byte[]info)
       {
            int infoLength = info.Length;
            byte[] res = new byte[infoLength+3];
            if(info.Length < 65535)
            {
                res[0] = stream;
                byte[] tmp = lengthToBigEndian(infoLength);
                res[1] = tmp[0];
                res[2] = tmp[1];
                Array.Copy(info,0,res,3,infoLength);
            }
            return res;
       }
        //Concat the rest of the info to the unique command identifier as seen in structs above
        public static byte[] cmdInfoConc(byte[] command, byte[] info) {
            return command.Concat(info).ToArray();
        }
    }
}
