using AutoFishPlugin.Tasks;
using OQ.MineBot.PluginBase.Base;
using OQ.MineBot.PluginBase.Base.Plugin;
using OQ.MineBot.PluginBase.Bot;

namespace AutoFishPlugin
{
    [Plugin(1, "Auto fish", "Gets you level 99 in fishing.")]
    public class PluginCore : IStartPlugin
    {
        public override void OnLoad(int version, int subversion, int buildversion) {
            Setting.Add(new BoolSetting("Keep rotation", "Should the bot not change it's head rotation?", false));
            Setting.Add(new ComboSetting("Sensitivity", null, new string[] {"High", "Medium", "Low"}, 1));
            Setting.Add(new ComboSetting("Reaction speed", null, new string[] {"Fast", "Medium", "Slow"}, 1));
            Setting.Add(new BoolSetting("Diconnect on TNT detect", "Should the bot disconnect if it detects tnt nearby (mcmmo plugin)", false));
        }
        public override PluginResponse OnEnable(IBotSettings botSettings) {
            if(!botSettings.loadEntities || !botSettings.loadMobs) return new PluginResponse(false, "'Load entities' & 'Load mobs' must be enabled.");
            return new PluginResponse(true);
        }
        public override void OnStart() {
            RegisterTask(new Fish(Setting.At(0).Get<bool>(), Setting.At(1).Get<int>(), Setting.At(2).Get<int>()));
            if(Setting.At(3).Get<bool>()) RegisterTask(new TNTDetector());
        }
    }
}