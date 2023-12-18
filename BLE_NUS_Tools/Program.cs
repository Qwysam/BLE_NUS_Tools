using System.Text.Json;

namespace BLE
{
    internal class Program
    {

        static async Task Main(string[] args)
        {
            //variables to set up launch without parameters
            string custom_ip = "null";
            int custom_port = -1;
            if (args.Length == 1)
                custom_ip = args[0];
            if (args.Length == 2)
            {
                custom_ip = args[0];
                int.TryParse(args[1], out custom_port);
            }
            connectionManager Manager = new connectionManager();
            if(! await Manager.initializeManager(custom_ip,custom_port)){
                Console.WriteLine("Initialization failed. Press enter to close the app");
                Console.ReadLine();
                return;
            }
            //Manager.socketManager.send(Commands.formatCommand(socketStream.Internal,sentCommands.sucess));
            while (true) {
                JsonDocument tmp = Manager.socketManager.recieveInput().Result;
                if (tmp != null)
                {
                    await Manager.handleInput(tmp);
                    tmp = null;
                }
            }
        }  
    }
}
