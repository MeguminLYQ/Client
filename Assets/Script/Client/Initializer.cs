using NLog;
using UnityEngine;
using WeCraft.Client.NLog;

namespace WeCraft.Client
{
    public static class Initializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void InitializeNLogger()
        {
            global::NLog.LogManager.Setup()
                .LoadConfiguration(builder =>
                {
                    builder.ForLogger().WriteTo(new UnityConsoleTarget());
                });
        }
    }
}