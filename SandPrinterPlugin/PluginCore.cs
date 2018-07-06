using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using OQ.MineBot.GUI.Protocol.Movement.Maps;
using OQ.MineBot.PluginBase;
using OQ.MineBot.PluginBase.Base;
using OQ.MineBot.PluginBase.Base.Plugin;
using OQ.MineBot.PluginBase.Bot;
using OQ.MineBot.PluginBase.Classes;
using OQ.MineBot.PluginBase.Classes.Base;
using OQ.MineBot.PluginBase.Classes.Blocks;
using OQ.MineBot.PluginBase.Classes.Items;
using OQ.MineBot.PluginBase.Classes.Materials;
using OQ.MineBot.Protocols.Classes.Base;
using SandPrinterPlugin.Tasks;

namespace SandPrinterPlugin
{
    [Plugin(1, "Sand printer", "Places sand in the specified area.")]
    public class PluginCore : IStartPlugin
    {
        public override void OnLoad(int version, int subversion, int buildversion) {
            Setting.Add(new LocationSetting("Start x y z", ""));
            Setting.Add(new LocationSetting("End x y z", ""));
            Setting.Add(new ComboSetting("Mode", null, new string[] {"Fast", "Accurate"}, 1));
            Setting.Add(new BoolSetting("Sand walking", "Can the bot walk on sand (might cause it falling off with multiple bots)", false));
            Setting.Add(new BoolSetting("No movement", "Should the bot place all the sand from the spot it is in?", false));
        }
        public override PluginResponse OnEnable(IBotSettings botSettings) {
            
            if(!botSettings.loadWorld) return new PluginResponse(false, "'Load world' must be enabled.");
            if (!botSettings.loadInventory) return new PluginResponse(false, "'Load inventory' must be enabled.");
            if (Setting.At(0).Get<ILocation>().Compare(new Location(0, 0, 0)) &&
                Setting.At(1).Get<ILocation>().Compare(new Location(0, 0, 0))  ) return new PluginResponse(false, "No coordinates have been entered.");

            return new PluginResponse(true);
        }
        public override void OnStart() {

            var radius = new IRadius(Setting.At(0).Get<ILocation>(),
                                     Setting.At(1).Get<ILocation>());

            RegisterTask(new Print((Mode)Setting.At(2).Get<int>(), radius,
                       Setting.At(3).Get<bool>(), Setting.At(4).Get<bool>())
                      );
        }
    }
}
