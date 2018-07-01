using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using OQ.MineBot.PluginBase.Base.Plugin.Tasks;
using OQ.MineBot.PluginBase.Classes;
using OQ.MineBot.PluginBase.Classes.Window;
using OQ.MineBot.PluginBase.Pathfinding.Sub;

namespace NetherwartFarmerPlugin.Tasks
{
    public class Store : ITask, ITickListener
    {
        public static readonly int[] FOOD = { 364, 412, 320, 424, 366, 393, 297 };

        private bool busy;
        private IChestMap chestMap;

        public override bool Exec() {
            return !status.entity.isDead && inventory.IsFull() && !status.eating &&
                   !busy && player.status.containers.GetWindow("minecraft:chest") == null;
        }

        public void OnTick() {

            // Check if this tick we should scan the
            // map for chests.
            if (chestMap == null) {
                Scan();
                return;
            }

            Deposite();
        }

        private void Deposite() {

            busy = true;
            ThreadPool.QueueUserWorkItem(obj => {
                var window = chestMap.Open(player, token);
                if (window != null) {
                    inventory.Deposite(window, FOOD);
                    player.tickManager.Register(3, () => {
                        CloseAllInventories(() => {
                            busy = false;
                        });
                    });
                } else {
                    player.tickManager.Register(3, () => { // Put a delay on chest open for 3 ticks.
                        CloseAllInventories(() => {
                            busy = false;
                        });
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

        private void CloseAllInventories(Action callback) {

            IStopToken token = null;
            token = player.tickManager.RegisterReocurring(3, () => {

                if(player.status.containers.openWindows.Count == 0) {

                    // Close player's inventory.
                    player.functions.CloseInventory();

                    token.Stop();
                    callback();
                    return;
                }

                var window = player.status.containers.openWindows.FirstOrDefault();
                if(!window.Equals(default(KeyValuePair<int, IWindow>))) player.status.containers.RemoveWindow(window.Key);
            });
        }
    }
}