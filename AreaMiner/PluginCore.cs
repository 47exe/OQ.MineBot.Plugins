using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AreaMiner.Tasks;
using OQ.MineBot.PluginBase;
using OQ.MineBot.PluginBase.Base;
using OQ.MineBot.PluginBase.Base.Plugin;
using OQ.MineBot.PluginBase.Bot;
using OQ.MineBot.PluginBase.Classes;
using OQ.MineBot.PluginBase.Classes.Base;
using OQ.MineBot.Protocols.Classes.Base;

namespace AreaMiner
{
    [Plugin(1, "Area miner", "Mines the area that is selected by the user.", "https://www.youtube.com/watch?v=Z0VB4PElvRY&t=129s")]
    public class PluginCore : IStartPlugin
    {
        private static readonly ShareManager shares = new ShareManager();

        public override void OnLoad(int version, int subversion, int buildversion) {

            this.Setting.Add(new LocationSetting("Start x y z", ""));
            this.Setting.Add(new LocationSetting("End x y z", ""));
            this.Setting.Add(new StringSetting("Macro on inventory full", "Starts the macro when the bots inventory is full.", ""));
            this.Setting.Add(new ComboSetting("Speed mode", null, new string[] { "Accurate", "Fast" }, 0));
            this.Setting.Add(new ComboSetting("Path mode", null, new string[] { "Advanced (mining & building)", "Basic" }, 0));

            var blockGroup = new GroupSetting("Blocks", "Block related settings can be found here.");
            blockGroup.Add(new BlockCollectionSetting("Ignore ids", "What blocks should be ignored.", "", true));
            this.Setting.Add(blockGroup);

            /*
            this.Setting.Add(new LocationSetting("--TEMP", "--DESC", new Location(0, 0, 0)));
            this.Setting.Add(new StringListSetting("--TEMP2", "--DESC2", ""));
            this.Setting.Add(new BlockCollectionSetting("--TEMP3", "--DESC3", "", true));

            var group = new GroupSetting("Testing groups", "group description");
            group.Add(new StringSetting("wow", "dab", "haters"));

            var temp = new ComboSetting("TEST SETTING", null, new string[] {"TEST 1", "TEST 2"}, 0);
            temp.Add(0, new StringSetting("1.1", "What blocks should be ignored.", "1.1"));
            temp.Add(0, new StringSetting("1.2", "What blocks should be ignored.", "1.2"));
            temp.Add(0, new StringSetting("1.3", "What blocks should be ignored.", "1.3"));
            temp.Add(1, new StringSetting("1.1", "What blocks should be ignored.", "1.1"));
            group.Add(temp);

            this.Setting.Add(group);
            */
        }

        public override PluginResponse OnEnable(IBotSettings botSettings) {
            if (!botSettings.loadWorld) return new PluginResponse(false, "'Load world' must be enabled.");
            if(Setting.GetValue<ILocation>("Start x y z").Compare(new Location(0, 0, 0)) && Setting.GetValue<ILocation>("End x y z").Compare(new Location(0, 0, 0))) return new PluginResponse(false, "No coordinates have been entered.");
            return new PluginResponse(true);
        }
        public override void OnDisable() { shares?.Clear(); Mine.broken?.Clear(); }

        public override void OnStart() {
            
            shares.SetArea(new IRadius(Setting.GetValue<ILocation>("Start x y z"), Setting.GetValue<ILocation>("End x y z")));

            var blocks = Setting.Get("Blocks") as IParentSetting; // Get blocks group.

            var macro = new MacroSync();
            RegisterTask(new InventoryMonitor(Setting.GetValue<string>("Macro on inventory full"), macro));
            RegisterTask(new Path(shares, (PathMode) Setting.GetValue<int>("Path mode"), macro ));
            RegisterTask(new Mine(shares, (Mode)Setting.GetValue<int>("Speed mode"), (PathMode)Setting.GetValue<int>("Path mode"), blocks.GetValue<BlockIdCollection>("Ignore ids").collection.Select(x=>x.id).Distinct().ToArray(), macro));
        }
    }
}

public class ShareManager
{
    private readonly ConcurrentDictionary<IPlayer, SharedRadiusState> zones = new ConcurrentDictionary<IPlayer, SharedRadiusState>();
    private IRadius  radius;

    public void SetArea(IRadius radius) {
        this.radius = radius;
    }
    public void Add(IPlayer player) {
        zones.TryAdd(player, new SharedRadiusState(radius));
        Calculate();
    }
    public void Clear() {
        zones.Clear();
    }

    public IRadius Get(IPlayer player) {
        SharedRadiusState state;
        if (!zones.TryGetValue(player, out state)) return null;
        return state.radius;
    }

    public void RegisterReached(IPlayer player) {
        SharedRadiusState state;
        if (!zones.TryGetValue(player, out state)) return;
        state.reached = true;
    }

    public void Calculate() {
        
        var zones = this.zones.ToArray();
        var count = zones.Length;

        int x, z;
        int l;

        if (radius.xSize > radius.zSize) {
            x = (int)Math.Ceiling((double)radius.xSize / (double)count);
            l = radius.xSize;
            z = radius.zSize;

            for (int i = 0; i < zones.Length; i++) {
                zones[i].Value.Update(new Location(radius.start.x + x*i, radius.start.y, radius.start.z),
                                      new Location(radius.start.x + (x*(i + 1)) + (i == zones.Length - 1 ? l - (x*(i + 1)) : 0), radius.start.y + radius.height, radius.start.z + z));
            }
        }
        else {
            x = radius.xSize;
            z = (int)Math.Ceiling((double)radius.zSize / (double)count);
            l = radius.zSize;
            for (int i = 0; i < zones.Length; i++)
                zones[i].Value.Update(new Location(radius.start.x, radius.start.y, radius.start.z + z * i),
                                      new Location(radius.start.x + x, radius.start.y + radius.height, radius.start.z + z * (i + 1) + (i == zones.Length - 1 ? l - (z * (i + 1)) : 0)));
        }
    }

    public bool AllReached() {

        var temp= zones.ToArray();
        for(int i = 0; i < temp.Length; i++)
            if (!temp[i].Value.reached) return false;
        return true;
    }
    public bool MeReached(IPlayer player) {

        SharedRadiusState state;
        if (!zones.TryGetValue(player, out state)) return false;
        return state.reached;
    }
}

public class SharedRadiusState
{
    public bool    reached = false;
    public IRadius radius  = null;

    public SharedRadiusState(IRadius radius) {
        this.radius = radius;
    }

    public void Update(ILocation loc, ILocation loc2) {
        reached = false;
        radius = new IRadius(loc, loc2);
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