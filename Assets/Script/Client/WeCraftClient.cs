using NLog;
using UnityEngine;
using UnityEngine.InputSystem;
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

    private void Update()
    {
        if (Keyboard.current[Key.Space].wasPressedThisFrame)
        {
            var chan=Core.Handler.GetDefaultChannel();
            NetworkManager.Send(chan.Id, PackId.C2S_Ping, new C2S_Ping() { msg = "HelloWorld" });
        }
    }

    private void Start()
    {
        NetworkManager.Connect();
        var chan=Core.Handler.GetDefaultChannel(); 
        chan.RegisterHandler((ushort)PackId.S2C_Pong, ((id,data) =>
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
