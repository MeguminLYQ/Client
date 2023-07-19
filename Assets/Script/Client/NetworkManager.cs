using System;
using System.Net;
using System.Net.Sockets;
using System.Net.Sockets.Kcp.Simple;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using UnityEngine;
using WeCraft.Core;
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
                RemoteIp.Address = IPAddress.Parse(_remoteAddress);
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
                RemoteIp.Port = _remotePort;
            }
        }
        [field: SerializeField]
        public int LocalPort { get; set; }= 25576;

        public IPEndPoint RemoteIp { get; private set; }
        
        private SimpleKcpClient KcpConnection;

        public Logger Logger { get; private set; }

        public CancellationTokenSource CancelToken = new CancellationTokenSource();

        public bool IsConnected => KcpConnection != null;

        private WeCraftClient _client;

        public void Initialize(WeCraftClient client)
        {
            _client = client;
            Logger = LogManager.GetCurrentClassLogger();

            RemoteIp = new IPEndPoint(IPAddress.Parse(_remoteAddress),_remotePort);
        }
        public void Connect()
        {
            Disconnect();
            try
            {
                KcpConnection = new SimpleKcpClient(LocalPort, RemoteIp);
                CancelToken = new CancellationTokenSource();
                Task.Run(UpdateKcp, CancelToken.Token);
                Task.Run(Receive, CancelToken.Token);
            }
            catch (SocketException e)
            {
                Logger.Error(e);
                Disconnect();
                return;
            } 
            Logger.Info($"连接成功 {_remoteAddress}:{_remotePort}");
        }
        public void Disconnect()
        {
            CancelToken.Cancel();
            try
            {
                KcpConnection?.Close();
            }
            catch (Exception)
            {
                // ignored
            }
            KcpConnection = null;
        }

        private async void UpdateKcp()
        {
            while (true)
            {
                KcpConnection.kcp.Update(DateTimeOffset.UtcNow);
                await Task.Delay(10,this.CancelToken.Token);
            }
        }

        private async void Receive()
        {
            while (true)
            {   
                byte[] res=await KcpConnection.ReceiveAsync(); 
                _client.Core.Handler.HandleBytes(res);
            }
        }
        
        
        public void Send(uint chanId,uint id,object data)
        {
            if (!IsConnected)
            {
                Logger.Debug("网络未连接");    
                return;
            }
            var bytes = _client.Core.Handler.GetSendBytes(chanId, id, data);
            KcpConnection.SendAsync(bytes,bytes.Length);
        }

        public void Send(uint chanId, PackId id, object data)
        {
            Send(chanId,(uint)id,data);
        }
    }
}