﻿using System;
using AutoEatPlugin.Tasks;
using OQ.MineBot.PluginBase.Base;
using OQ.MineBot.PluginBase.Base.Plugin;
using OQ.MineBot.PluginBase.Bot;

namespace AutoEatPlugin
{
    [Plugin(1, "Auto eater", "Eats food when hungry, eats gapples when needed.")]
    public class PluginCore : IStartPlugin
    {
        public override void OnLoad(int version, int subversion, int buildversion) {
            Setting.Add(new NumberSetting("Eat when hunger is below X", "When should the bot eat normal food (-1 if it shouldn't eat them).", -1, -1, 19, 1));
            Setting.Add(new NumberSetting("Eat gapples when below X hp", "When should the bot eat golden apples (-1 if it shouldn't eat them).", -1, -1, 19, 1));
            Setting.Add(new ComboSetting("Mode", null, new string[] { "Efficient", "Accurate" }, 1));
            Setting.Add(new BoolSetting("Soup", "Can the bot use soup for healing?", false));
        }
        public override PluginResponse OnEnable(IBotSettings botSettings) {
            if (!botSettings.loadInventory) return new PluginResponse(false, "'Load inventory' must be enabled.");
            return new PluginResponse(true);
        }
        public override void OnStart() {
            RegisterTask(new Eat(
                            Setting.At(1).Get<int>(), Setting.At(0).Get<int>(),
                            Setting.At(3).Get<bool>(),    // Can use soup.
                            Setting.At(2).Get<int>() == 1 // Determine mode.
                        ));
        }
    }
}
