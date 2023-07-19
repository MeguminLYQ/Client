using NLog;
using NLog.Targets;
using UnityEngine;
namespace WeCraft.Client.NLog
{
    [Target("UnityConsole")]
    public class UnityConsoleTarget: TargetWithLayout
    {
        protected override void Write(LogEventInfo logEvent)
        {
            string msg=RenderLogEvent(this.Layout, logEvent);
            bool isWarn=logEvent.Level == LogLevel.Warn;
            bool isErr = logEvent.Level == LogLevel.Error || logEvent.Level==LogLevel.Fatal;
            
            if (isErr)
            {
                Debug.LogError(msg);
                return;
            }

            if (isWarn)
            {
                Debug.LogWarning(msg);
                return;
            }
            Debug.Log(msg);
        }
    }
}