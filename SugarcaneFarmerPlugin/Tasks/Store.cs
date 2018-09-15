﻿using System.Threading;
using System.Threading.Tasks;
using OQ.MineBot.PluginBase;
using OQ.MineBot.PluginBase.Base.Plugin.Tasks;
using OQ.MineBot.PluginBase.Pathfinding.Sub;

namespace SugarcaneFarmerPlugin.Tasks
{
    public class Store : ITask, ITickListener
    {
        private static readonly int[] FOOD = { 260, 297, 319, 320, 350, 357, 360, 364, 366, 391, 393, 400, 293 };

        private bool busy;
        private IChestMap chestMap;
        private MacroSync macroSync;

        private readonly bool nothing, store, macro;

        public Store(int mode, MacroSync macroSync) {
            this.macroSync = macroSync;

            nothing = mode == 0;
            store = mode == 1;
            macro = mode == 2;
        }

        public override bool Exec() {
            return !nothing && !status.entity.isDead && inventory.IsFull() && !status.eating &&
                   !busy && player.status.containers.GetWindow("minecraft:chest") == null &&
                   !macroSync.IsMacroRunning();
        }

        public void OnTick() {

            if (store) {

                // Check if this tick we should scan the
                // map for chests.
                if (chestMap == null) {
                    Scan();
                    return;
                }

                Deposite();
            }
            else if (macro) {
                macroSync.Run(player);   
            }
        }

        private void Deposite() {

            busy = true;
            ThreadPool.QueueUserWorkItem(obj => {
                var window = chestMap.Open(player, token);
                if (window != null) {
                    inventory.Deposite(window, FOOD);
                    player.tickManager.Register(3, () => {
                        actions.CloseContainer(window.id);
                        busy = false;
                    });
                } else {
                    player.tickManager.Register(3, () => { // Put a delay on chest open for 3 ticks.
                        busy = false;
                    });
                }
            });
        }

        private void Scan() {
            busy = true;
            chestMap = player.functions.CreateChestMap();
            chestMap.UpdateChestList(player, () => {
                busy = false;
            });
        }
    }
}
<<<<<<< HEAD

public class MacroSync
{
    private Task macroTask;
    private string name;

    public MacroSync() { }
    public MacroSync(string name) {
        this.name = name;
    }

    public bool IsMacroRunning()
    {
        //Check if there is an instance of the task.
        if (macroTask == null) return false;
        //Check completion state.
        return !macroTask.IsCompleted && !macroTask.IsCanceled && !macroTask.IsFaulted;
    }

    public void Run(IPlayer player) {
        macroTask = player.functions.StartMacro(name);
    }

    public void Run(IPlayer player, string name) {
        macroTask = player.functions.StartMacro(name);
    }
}
=======
>>>>>>> dba81812341b3c6b7afaec169ca0d5fd4a64bfd0
