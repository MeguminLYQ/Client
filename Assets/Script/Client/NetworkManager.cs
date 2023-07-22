using System;
using System.Net;
using System.Net.Sockets; 
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Riptide;
using Riptide.Utils;
using UnityEngine;
using UnityEngine.Profiling;
using WeCraft.Core;
using WeCraft.Core.Util;
using Logger = NLog.Logger;

namespace WeCraft.Client
{
    [Serializable]
    public class NetworkManager
    {

        [SerializeField]
        private string _remoteAddress="127.0.0.1";

        public string RemoteAddress
        {
            get => _remoteAddress;
            set
            {
                _remoteAddress = value; 
            }
        }
        [SerializeField]
        private int _remotePort=25575;

        public int RemotePort
        {
            get => _remotePort;
            set
            {
                if (value > 65535 || value < 1)
                {
                    Logger.Error($"错误,{value}不是一个合法的端口范围");
                    return;
                }
                _remotePort = value; 
            }
        }
        [field: SerializeField]
        public int LocalPort { get; set; }= 25576; 
        
        private Riptide.Client EmbeddedClient;

        public Logger Logger { get; private set; }

        public CancellationTokenSource CancelToken = new CancellationTokenSource();

        public bool IsConnected => EmbeddedClient is { IsConnected: true };

        private WeCraftClient _client;

        public void Initialize(WeCraftClient client)
        {
            _client = client;
            Logger = LogManager.GetCurrentClassLogger(); 
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
            EmbeddedClient.Connected += (a,b) =>
            {
                Logger.Info($"连接成功 {_remoteAddress}:{_remotePort}");
            };
            
            EmbeddedClient.Disconnected += (a,b) =>
            {
                Logger.Info($"连接成功 {_remoteAddress}:{_remotePort}");
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
                await Task.Delay(10,this.CancelToken.Token);
                Profiler.EndSample();
            }

            EmbeddedClient.MessageReceived -= OnMessageReceive;
            EmbeddedClient?.Disconnect();
            EmbeddedClient = null;
        }

        private void OnMessageReceive(object sender, MessageReceivedEventArgs e)
        {
            var message=e.Message;
            var clientId = e.FromConnection.Id;
            this._client.Core.Handler.HandleMessage(clientId,message.GetUShort(),message.GetUShort(),message.GetBytes());

        }

        public void Send(ushort chanId,ushort id,object data,bool reliable=true)
        {
            if (!IsConnected)
            {
                Logger.Debug("网络未连接");    
                return;
            }
            var message = CreateMessage(chanId, id, data, reliable);
            EmbeddedClient.Send(message);
        }

        public void Send(ushort chanId, PackId id, object data,bool reliable=true)
        {
            Send(chanId,(ushort)id,data,reliable);
        }
        
        private Message CreateMessage(ushort chanId, ushort packId, object data, bool reliable = true)
        {
            Message message = reliable
                ? Message.Create(MessageSendMode.Reliable,0)
                : Message.Create(MessageSendMode.Unreliable,0);
            message.AddUShort(chanId);
            message.AddUShort(packId);
            message.AddBytes(PBUtil.Serialize(data));
             return message;
        }
    }
}