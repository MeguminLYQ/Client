 
using NLog;
using WeCraft.Core.Mod;
using WeCraft.Foundation;

namespace Script.Mod
{
    public class ModManager: IModManager
    {
        private Logger _logger;
        private WeCraftClient Client;
        public void CallPreInitialization()
        {
            throw new System.NotImplementedException();
        }

        public void CallInitialization()
        {
            throw new System.NotImplementedException();
        }

        public void CallPostInitialization()
        {
            throw new System.NotImplementedException();
        }

        public ModBase[] Mods { get; protected set; }

        public ModManager(WeCraftClient server)
        {
            _logger = LogManager.GetCurrentClassLogger();
            this.Client = server; 
        }

        public void Load()
        {
            Mods = LoadAllMod();
            Client.LoggerImpl.Info("开始加载MOD");
            foreach (var pluginBase in Mods)
            {
                Client.LoggerImpl.Info($"开始加载{pluginBase.Name}");
                pluginBase.OnEnable();
            }
            
            Client.LoggerImpl.Info("MOD加载完成");
        }
        protected ModBase[] LoadAllMod()
        {
            return new ModBase[] { new FoundationMod() ,new ClientEmbeddedMod()};
        }
    }
}