using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OQ.MineBot.PluginBase;
using OQ.MineBot.PluginBase.Base;
using OQ.MineBot.PluginBase.Base.Plugin;
using OQ.MineBot.PluginBase.Bot;
using OQ.MineBot.PluginBase.Classes.Base;
using OreMinerPlugin.Tasks;

namespace OreMinerPlugin
{
    [Plugin(1, "Ore miner", "Mines ores using xray.")]
    public class PluginCore : IStartPlugin
    {
        public override void OnLoad(int version, int subversion, int buildversion) {
            Setting.Add(new StringSetting("Macro on inventory full", "Starts the macro when the bots inventory is full.", ""));
            Setting.Add(new BlockCollectionSetting("IDs", "What ids will the miner look for.", "56:0 129:0 15:0 14:0 73:0 74:0 21:0 16:0", false));
        }

        public override PluginResponse OnEnable(IBotSettings botSettings) {
            if (!botSettings.loadWorld) return new PluginResponse(false, "'Load world' must be enabled.");
            if (botSettings.staticWorlds) return new PluginResponse(false, "'Shared worlds' should be disabled.");

            return new PluginResponse(true);
        }

        public override void OnStart() {
            var macro = new MacroSync();

            RegisterTask(new Mine(Setting.At(1).Get<BlockIdCollection>(), macro));
            RegisterTask(new InventoryMonitor(Setting.At(0).Get<string>(), macro));
        }

        public override void OnStop() {
            Mine.beingMined.Clear();
        }
    }

    public class MacroSync
    {
        private Task macroTask;

        public bool IsMacroRunning() {
            //Check if there is an instance of the task.
            if (macroTask == null) return false;
            //Check completion state.
            return !macroTask.IsCompleted && !macroTask.IsCanceled && !macroTask.IsFaulted;
        }

        public void Run(IPlayer player, string name) {
            macroTask = player.functions.StartMacro(name);
        }
    }
}
