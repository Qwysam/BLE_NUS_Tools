using System.Text.Json;

namespace BLE
{
    internal class Program
    {

        static async Task Main(string[] args)
        {
            connectionManager Manager = new connectionManager();
            if(! await Manager.initializeManager()){
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
