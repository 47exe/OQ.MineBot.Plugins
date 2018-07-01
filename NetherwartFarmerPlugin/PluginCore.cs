using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetherwartFarmerPlugin.Tasks;
using OQ.MineBot.PluginBase.Base;
using OQ.MineBot.PluginBase.Base.Plugin;
using OQ.MineBot.PluginBase.Bot;
using OQ.MineBot.PluginBase.Utility;

namespace NetherwartFarmerPlugin
{
    [Plugin(1, "Netherwart farmer", "Auto farms netherwarts.")]
    public class PluginCore : IStartPlugin
    {
        public override void OnLoad(int version, int subversion, int buildversion) {
            this.Setting = new IPluginSetting[3];
            Setting[0] = new NumberSetting("Radius (crop, x-radius):", "Radius around the initial bot spawn position that it will look around.", 64, 1, 1000, 1);
            Setting[1] = new NumberSetting("Radius (crop, y-radius):", "What can be the Y difference for the bot for it to find valid crops.", 4, 1, 256, 1);
            Setting[2] = new ComboSetting("Speed mode", null, new string[] {"Accurate", "Fast"}, 0);
        }
        public override PluginResponse OnEnable(IBotSettings botSettings) {
            if (!botSettings.loadWorld) return new PluginResponse(false, "'Load world' must be enabled.");
            return new PluginResponse(true);
        }
        public override void OnStart() {
            RegisterTask(new Farm(Setting[0].Get<int>(), Setting[1].Get<int>(), (Mode)Setting[2].Get<int>()));
            RegisterTask(new Store());
        }
        public override void OnDisable() {
            Farm.beingMined.Clear();
        }
    }
}
