using OQ.MineBot.PluginBase.Base.Plugin.Tasks;

namespace AreaMiner.Tasks
{
    public class RestockMonitor: ITask, ITickListener
    {
        private readonly MacroSync macro;
        private readonly string    macroName;

        private readonly int[] pickIds = new int[] {257, 270, 274, 278, 285};

        public RestockMonitor(string name, MacroSync macro) {
            this.macro     = macro;
            this.macroName = name;
        }

        public override bool Exec() {
            return !string.IsNullOrWhiteSpace(macroName) &&  !status.entity.isDead && !status.eating && !macro.IsMacroRunning() && 
                !inventory.IsFull() && inventory.FindId(pickIds) == -1;
        }

        public void OnTick() {
            macro.Run(player, macroName);
        }
    }
}