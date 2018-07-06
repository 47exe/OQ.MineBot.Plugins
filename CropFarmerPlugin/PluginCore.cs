using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CropFarmerPlugin.Tasks;
using OQ.MineBot.GUI.Protocol.Movement.Maps;
using OQ.MineBot.PluginBase;
using OQ.MineBot.PluginBase.Base;
using OQ.MineBot.PluginBase.Base.Plugin;
using OQ.MineBot.PluginBase.Bot;
using OQ.MineBot.PluginBase.Classes;
using OQ.MineBot.PluginBase.Classes.Base;
using OQ.MineBot.PluginBase.Classes.Blocks;
using OQ.MineBot.PluginBase.Classes.Entity;
using OQ.MineBot.PluginBase.Classes.Entity.Player;
using OQ.MineBot.PluginBase.Pathfinding;
using OQ.MineBot.PluginBase.Pathfinding.Sub;
using OQ.MineBot.Protocols.Classes.Base;

namespace CropFarmerPlugin
{
    [Plugin(1, "Crop farmer", "Auto farms crops.")]
    public class PluginCore : IStartPlugin
    {
        public override void OnLoad(int version, int subversion, int buildversion) {
            Setting.Add(new NumberSetting("Radius (crop, x-radius):", "Radius around the initial bot spawn position that it will look around.", 64, 1, 1000, 1));
            Setting.Add(new NumberSetting("Radius (crop, y-radius):", "What can be the Y difference for the bot for it to find valid crops.", 4, 1, 256, 1));
            Setting.Add(new ComboSetting("Speed mode", null, new string[] {"Accurate", "Fast"}, 0));
        }
        public override PluginResponse OnEnable(IBotSettings botSettings) {
            if (!botSettings.loadWorld) return new PluginResponse(false, "'Load world' must be enabled.");
            return new PluginResponse(true);
        }
        public override void OnStart() {
            RegisterTask(new Farm(Setting.At(0).Get<int>(), Setting.At(1).Get<int>(), (Mode)Setting.At(2).Get<int>()));
            RegisterTask(new Store());
        }
        public override void OnDisable() {
            Farm.beingMined.Clear();
        }
    }
}
