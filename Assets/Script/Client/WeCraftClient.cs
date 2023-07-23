using NLog;
using Script.Mod;
using UnityEngine; 
using WeCraft.Client;
using WeCraft.Core;
using WeCraft.Core.Network; 
 
public class WeCraftClient : WeCraftCore
{
    public NetworkManager NetworkManager;
    public string RemoteAddress="127.0.0.1";
    public ushort RemotePort=25575;
    private static WeCraftClient Client;
    protected WeCraftClient()
    { 
        //init value 
        this.LoggerImpl = LogManager.GetLogger("WeCraftClient");  
        this.IsServer = true;
        //set up manager
        this.GameLogicImpl = null;
        this.NetworkManager = new NetworkManager(this);
        this.NetworkManagerImpl=this.NetworkManager;
        this.ModManagerImpl = new ModManager(this);
        this.NetworkHandlerImpl = new NetworkHandler(); 

        (this.ModManagerImpl as ModManager).Load();

    }

    public void Start()
    {
        NetworkManager.Connect();
         
    }

    public void Stop()
    {
        NetworkManager.Disconnect();
        LogManager.Shutdown();
    }

    public static WeCraftClient GetInstance()
    {
        if (Client == null)
        {
            Client = new WeCraftClient();
            CoreImpl = Client;
        }
        return Client;
    }
}
