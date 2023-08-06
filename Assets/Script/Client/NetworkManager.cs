using System;
using System.Net;
using System.Net.Sockets; 
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NLog;
using Riptide;
using Riptide.Utils;
using Script;
using UnityEngine;
using UnityEngine.Profiling;
using WeCraft.Client.Event;
using WeCraft.Core.C2S;
using WeCraft.Core.Event;
using WeCraft.Core.Network;
using WeCraft.Core.Utility; 
using Channel = WeCraft.Core.Network.Channel;
using Logger = NLog.Logger;

namespace WeCraft.Client
{ 
    public class NetworkManager:INetworkManager
    { 
        private string _remoteAddress => _client.RemoteAddress;
 
        private ushort _remotePort=>_client.RemotePort;
 
        
        private Riptide.Client EmbeddedClient;

        public Logger Logger { get; private set; }

        public CancellationTokenSource CancelToken = new CancellationTokenSource();

        public bool IsConnected => EmbeddedClient is { IsConnected: true };

        private WeCraftClient _client;
        public Channel DefaultChannel => _client.NetworkHandlerImpl.GetDefaultChannel();
        public NetworkManager(WeCraftClient client)
        {
            _client = client;
            Logger = LogManager.GetLogger("WeCraftClient.Network");
            RiptideLogger.Initialize(Logger.Debug,Logger.Info,Logger.Warn,Logger.Error,false);  
        }
        public void Connect()
        {
            Disconnect(); 
            EmbeddedClient = new Riptide.Client();
            CancelToken = new CancellationTokenSource();
            Task.Run(StartEmbeddedClient);
        }
        public void Disconnect()
        {
            CancelToken.Cancel(); 
        }

        private async void StartEmbeddedClient()
        {
            try
            {
                EmbeddedClient.Connected += (a,b) =>
                {
                    Logger.Info($"连接成功 {_remoteAddress}:{_remotePort}");
                    // EventHandler.ExecuteEvent(ClientEventName.OnConnectedToServer);
                    EventBus.Post(new ConnectToServerEvent());
                }; 
                EmbeddedClient.Disconnected += (a,b) =>
                {
                    Logger.Info($"连接断开 {_remoteAddress}:{_remotePort}");
                };
                EmbeddedClient.ConnectionFailed += (a, b) =>
                {
                    Logger.Info($"连接失败 {_remoteAddress}:{_remotePort}");
                };
                EmbeddedClient.MessageReceived += OnMessageReceive; 

                EmbeddedClient.Connect($"{_remoteAddress}:{_remotePort}", 1, 0, null, false);
            
                while (!CancelToken.IsCancellationRequested)
                {
                    Profiler.BeginSample("WeCraft-Network-Update");
                    EmbeddedClient.Update();
                    await Task.Delay(10);
                    Profiler.EndSample();
                } 
                EmbeddedClient.MessageReceived -= OnMessageReceive;
                EmbeddedClient?.Disconnect();
                EmbeddedClient = null;
            }
            catch (Exception e)
            {
                Logger.Error(e);
            }
        }

  
        private Message CreateMessage(ushort defaultChannelId, PackId c2SPlayerProfile, C2S_PlayerProfile playerProfile, bool reliable)
        {
            return CreateMessage(defaultChannelId, (ushort)c2SPlayerProfile, playerProfile, reliable);
        }

        private void OnMessageReceive(object sender, MessageReceivedEventArgs e)
        {
            var message=e.Message;
            var clientId = e.FromConnection.Id;
            this._client.NetworkHandlerImpl.HandleMessage(clientId,message.GetUShort(),message.GetUShort(),message.GetBytes());

        }

        public void SendToServer(ushort chanId,ushort id,object data,bool reliable=true)
        {
            if (!IsConnected)
            {
                Logger.Debug("网络未连接");    
                return;
            }
            var message = CreateMessage(chanId, id, data, reliable);
            EmbeddedClient.Send(message);
        }


        public void SendToAllClient(ushort chanId, PackId id, object data, bool reliable = true)
        {
            throw new NotImplementedException();
        }

        public void SendToServer(ushort chanId, PackId id, object data,bool reliable=true)
        {
            SendToServer(chanId,(ushort)id,data,reliable);
        }
 
        public void SendToClient(ushort[] connid,ushort chanId,ushort id,object data,bool reliable=true)
        {
            
            Logger.Warn("客户端,尝试发送数据给客户端");
        }

        public void SendToClient(ushort[] connid,ushort chanId, PackId id, object data,bool reliable=true)
        {
            SendToClient(connid,chanId,(ushort)id,data,reliable);
        }

        public void SendToAllClient(ushort chanId, ushort id, object data, bool reliable = true)
        {
            throw new NotImplementedException();
        }


        private Message CreateMessage(ushort chanId, ushort packId, object data, bool reliable = true)
        {
            Message message = reliable
                ? Message.Create(MessageSendMode.Reliable,0)
                : Message.Create(MessageSendMode.Unreliable,0);
            message.AddUShort(chanId);
            message.AddUShort(packId);
            message.AddBytes(ProtobufUtility.Serialize(data));
             return message;
        }
 
    }
}