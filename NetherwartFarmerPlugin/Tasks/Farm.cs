using System;
using System.Collections.Concurrent;
using System.Linq;
using OQ.MineBot.GUI.Protocol.Movement.Maps;
using OQ.MineBot.PluginBase.Base.Plugin.Tasks;
using OQ.MineBot.PluginBase.Classes;

namespace NetherwartFarmerPlugin.Tasks
{
    public class Farm : ITask, ITickListener
    {
        private readonly MapOptions MO        = new MapOptions() { Look = false, Quality = SearchQuality.LOW };
        private static ushort[]     FARMABLE  = { 115 };
        private static ushort[]     PLANTABLE = { 372 };

        private int  x, y;
        private Mode mode;

        private bool scan;
        private bool scanning;
        private ILocation[] locations;
        private bool busy;

        public Farm(int x, int y, Mode mode) {
            this.x = x;
            this.y = y;
            this.mode = mode;
            this.scan = true;
        }

        public override bool Exec() {
            return !status.entity.isDead && !inventory.IsFull() && !status.eating &&
                   !scanning && !busy && player.status.containers.GetWindow("minecraft:chest") == null;
        }

        public override void Start() {
            player.events.onWorldReload += player1 => scan = true;
        }

        private int tick = 0;
        public void OnTick() {

            if (scan) {
                ScanArea();
                return;
            }

            tick++;
            if (tick > (mode == Mode.Accurate ? 6 : 3)) tick = 0;
            else return;

            if(ReplantHandler()) return;

            var location = FindNext();
            if (location == null) return;

            busy = true;
            beingMined.TryAdd(location, null);
            if (mode == Mode.Fast && player.world.InRange(status.entity.location, location)) {
                Mine(location);
            }
            else {
                var map = actions.AsyncMoveToLocation(location, token, MO);
                map.Completed += areaMap => {
                    Mine(location);
                };
                map.Cancelled += (areaMap, cuboid) => {
                    object obj; beingMined.TryRemove(location, out obj);
                    busy = false;
                };
                map.Start();
            }
        }

        private bool ReplantHandler() {

            if (inventory.FindId(PLANTABLE[0]) == -1) return false;
            var location = FindNextToReplant();
            if (location == null) return false;

            busy = true;
            beingMined.TryAdd(location, null);

            var map = actions.AsyncMoveToLocation(location, token, MO);
            map.Completed += areaMap => {
                Replant(location, FARMABLE[0]);
            };
            map.Cancelled += (areaMap, cuboid) => {
                object obj; beingMined.TryRemove(location, out obj);
                busy = false;
            };
            map.Start();

            return true;
        }

        private void Mine(ILocation location) {
            
            // Keep old data to know what to replant.
            var blockData = world.GetBlockId(location.x, (int)location.y, location.z);

            // Break the block.
            actions.LookAtBlock(location, true);
            player.tickManager.Register(1, () => {
                actions.BlockDig(location, action => {
                    Replant(location, blockData);
                });
            });
        }

        private void Replant(ILocation location, int oldBlockData) {

            // Create prioritization list.
            // aka: which plantable object should we find
            // in our inventory before the others.
            var priority = FarmableToPlantable(oldBlockData);
            var prioritizedList = PLANTABLE.ToList();
            if (priority != -1) prioritizedList.Insert(0, (ushort)priority);
            var prioritizedArray = prioritizedList.ToArray();

            if (inventory.Select(prioritizedArray) != -1) {
                actions.LookAtBlock(location);
                player.tickManager.Register((mode == Mode.Accurate ? 4 : 1), () =>
                {
                    var data = player.functions.FindValidNeighbour(location);
                    if (data != null)
                        actions.BlockPlaceOnBlockFace(data.location, data.face);
                    object obj; beingMined.TryRemove(location, out obj);
                    busy = false;
                });
            }
            else {
                object obj; beingMined.TryRemove(location, out obj);
                busy = false;
            }
        }

        private void ScanArea() {
            scanning = true;
            scan = false;

            world.FindAsync(player, status.entity.location.ToLocation(), x, y, FARMABLE, tempBlocks => {
                locations = tempBlocks;
                scanning = false;
            });
        }

        private ILocation FindNext() {

            ILocation nextMove = null;
            double distance = int.MaxValue;
            for (int i = 0; i < locations.Length; i++) {
                var block = player.world.GetBlock(locations[i].x, (int)locations[i].y, locations[i].z);
                if (FARMABLE.Contains((ushort) (block >> 4)) && (block & 15) >= 3) {
                    //Create the location.
                    var loc = locations[i];

                    //Check if already being mined.
                    if (beingMined.ContainsKey(loc)) continue;

                    //Check by difference.
                    double tempDistance = loc.Distance(player.status.entity.location.ToLocation(0));
                    if (nextMove == null) {
                        distance = tempDistance;
                        nextMove = loc;
                    }
                    else if (tempDistance < distance) {
                        distance = tempDistance;
                        nextMove = loc;
                    }
                }
            }
            return nextMove;
        }

        private ILocation FindNextToReplant() {

            ILocation nextMove = null;
            double distance = int.MaxValue;
            for (int i = 0; i < locations.Length; i++) {
                var block = player.world.GetBlockId(locations[i].x, (int)locations[i].y, locations[i].z);
                if (block == 0) {
                    //Create the location.
                    var loc = locations[i];

                    //Check if already being mined.
                    if (beingMined.ContainsKey(loc)) continue;

                    //Check by difference.
                    double tempDistance = loc.Distance(player.status.entity.location.ToLocation(0));
                    if (nextMove == null) {
                        distance = tempDistance;
                        nextMove = loc;
                    }
                    else if (tempDistance < distance) {
                        distance = tempDistance;
                        nextMove = loc;
                    }
                }
            }
            return nextMove;
        }

       private int FarmableToPlantable(int id) {
            
            //Loop trough the farmables and use the same
            //enumerator on the plantables as both arrays
            //are "linked".
            for(int i = 0;i < FARMABLE.Length; i++)
                if (FARMABLE[i] == id)
                    return PLANTABLE[i];
            return -1;
        }

        #region Shared work.

        /// <summary>
        /// Blocks that are taken already.
        /// </summary>
        public static ConcurrentDictionary<ILocation, object> beingMined = new ConcurrentDictionary<ILocation, object>();

        #endregion
    }

    public enum Mode
    {
        Accurate,
        Fast
    }
}