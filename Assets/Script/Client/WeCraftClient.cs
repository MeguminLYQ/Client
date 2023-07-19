using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets.Kcp.Simple;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using UnityEngine;
using WeCraft.Client;
using WeCraft.Core;
using WeCraft.Core.C2S;
using WeCraft.Core.S2C;
using WeCraft.Core.Util;
using Logger = NLog.Logger;

public class WeCraftClient : MonoBehaviour
{
 
    public WeCraftCore Core { get; private set; }
    [field: SerializeField]
    public NetworkManager NetworkManager { get; private set; } 
    private void Awake()
    {
        Logger logger = LogManager.GetCurrentClassLogger();
        Core = new WeCraftCore();
        NetworkManager.Initialize(this);
    }

    private void Start()
    {
        NetworkManager.Connect();
        var chan=Core.Handler.GetDefaultChannel();
        NetworkManager.Send(chan.Id, PackId.C2S_Ping, new C2S_Ping() { msg = "HelloWorld" });
        chan.RegisterHandler(PackId.S2C_Pong, (data =>
        {
            Debug.Log("--------------------------------");
            Debug.Log(PBUtil.Deserialize<S2C_Pong>(data).msg);
        }));
    }

    private void OnDestroy()
    {
        NetworkManager.Disconnect();
        LogManager.Shutdown();
    }
}
