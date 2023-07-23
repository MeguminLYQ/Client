using System;
using WeCraft.Client;
using WeCraft.Core.Network;

namespace Script.Mod
{
    public class EmbeddedNetworkHandler
    {
        public ClientEmbeddedMod mod;
        public EmbeddedNetworkHandler(ClientEmbeddedMod mod)
        {
            this.mod = mod;
        }
        
        public void OnConnectedToServer()
        {
            var playerProfile = mod.GameManager.PlayerProfile;
            if (string.IsNullOrEmpty(playerProfile.Guid))
            {
                playerProfile.Guid = Guid.NewGuid().ToString();
            }
            mod.Network.SendToServer(mod.DefaultChannel.Id,PackId.C2S_PlayerProfile,playerProfile);
        }

    }
}