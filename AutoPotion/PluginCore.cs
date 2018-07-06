using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoPotion.Tasks;
using OQ.MineBot.PluginBase;
using OQ.MineBot.PluginBase.Base;
using OQ.MineBot.PluginBase.Base.Plugin;
using OQ.MineBot.PluginBase.Bot;
using OQ.MineBot.PluginBase.Classes;
using OQ.MineBot.PluginBase.Classes.Base;
using OQ.MineBot.PluginBase.Classes.Entity;

namespace AutoPotion
{
    [Plugin(1, "Auto potion", "Drinks potions when the effects run out.")]
    public class PluginCore : IStartPlugin
    {
        public override void OnLoad(int version, int subversion, int buildversion) {
            Setting.Add(new BoolSetting("Strength", null, true));
            Setting.Add(new BoolSetting("Speed", null, true));
            Setting.Add(new BoolSetting("Fire resistance", null, true));
            Setting.Add(new NumberSetting("Health", "At how much health should the bot use health potions.", 10, -1, 20, 1));
        }
        public override PluginResponse OnEnable(IBotSettings botSettings) {
            if (!botSettings.loadInventory) return new PluginResponse(false, "'Load inventory' must be enabled.");
            return new PluginResponse(true);
        }
        public override void OnStart() {
            RegisterTask(new Drink(
                                Setting.At(0).Get<bool>(), Setting.At(1).Get<bool>(), Setting.At(2).Get<bool>(),
                                Setting.At(3).Get<int>()
                        ));
        }
    }
}