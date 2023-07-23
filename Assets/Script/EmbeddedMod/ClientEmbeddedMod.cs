using WeCraft.Client;
using WeCraft.Core.EventHandler;
using WeCraft.Core.Mod;
using WeCraft.Core.Network;

namespace Script.Mod
{
    public class ClientEmbeddedMod: ModBase
    {
        public NetworkManager Network { get; }= WeCraftClient.GetInstance().NetworkManager;
        public GameManager GameManager { get; }=GameManager.Instance;
        public Channel DefaultChannel { get; } = NetworkHandler.DefaultChannel;
        private EmbeddedNetworkHandler _handler;
        public ClientEmbeddedMod()
        {
            Name = "WeCraftClient";
            _handler = new EmbeddedNetworkHandler(this);
        }
        public override void OnLoad()
        {
            throw new System.NotImplementedException();
        }

        public override void OnEnable()
        { 
            EventHandler.RegisterEvent(ClientEventName.OnConnectedToServer,_handler.OnConnectedToServer);
        }

        public override void OnDisable()
        {
            throw new System.NotImplementedException();
        }
    }
}