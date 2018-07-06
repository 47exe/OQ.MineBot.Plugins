using System.Collections.Generic;
using System.Linq;
using OQ.MineBot.PluginBase.Base;
using OQ.MineBot.PluginBase.Base.Plugin;
using OQ.MineBot.PluginBase.Bot;
using OQ.MineBot.PluginBase.Classes.Entity.Lists;
using ShieldPlugin.Tasks;

namespace ShieldPlugin
{
    [Plugin(1, "Shield aura", "Follows the player and attacks anybody that gets close!")]
    public class PluginCore : IStartPlugin
    {
        public override void OnLoad(int version, int subversion, int buildversion) {
            /*
            Setting[0] = new StringSetting("Owner name/uuid", "Player that the bots will follow.", "");
            Setting[1] = new NumberSetting("Clicks per second", "How fast should the bot attack?", 5, 1, 60, 1);
            Setting[2] = new NumberSetting("Miss rate", "How often does the bot miss?", 15, 0, 100, 1);
            Setting[3] = new StringSetting("Friendly name(s)/uuid(s)", "Uuids of the user that own't be hit. Split by spaces'", "");
            Setting[4] = new BoolSetting("Auto equip best armor?", "Should the bot auto equip the best armor it has?", true);
            Setting[5] = new BoolSetting("Equip best weapon?", "Should the best item be auto equiped?", true);
            Setting[6] = new ComboSetting("Mode", null, new string[] {"Passive", "Aggressive"}, 0);
            */

            Setting.Add(new StringSetting("Owner name/uuid", "Player that the bots will follow.", ""));
            Setting.Add(new StringListSetting("Friendly name(s)/uuid(s)", "Uuids of the user that own't be hit. Split by spaces'", ""));
            Setting.Add(new ComboSetting("Mode", null, new string[] { "Passive", "Aggressive" }, 0));

            var clickGroup = new GroupSetting("Clicks", "");
                clickGroup.Add(new NumberSetting("Clicks per second", "How fast should the bot attack?", 5, 1, 60, 1));
                clickGroup.Add(new NumberSetting("Miss rate", "How often does the bot miss?", 15, 0, 100, 1));
            Setting.Add(clickGroup);

            var equipmentGroup = new GroupSetting("Equipment", "");
                equipmentGroup.Add(new BoolSetting("Auto equip best armor?", "Should the bot auto equip the best armor it has?", true));
                equipmentGroup.Add(new BoolSetting("Equip best weapon?", "Should the best item be auto equiped?", true));
            Setting.Add(equipmentGroup);
        }

        public override PluginResponse OnEnable(IBotSettings botSettings) {
            if (!botSettings.loadWorld) return new PluginResponse(false, "'Load world' must be enabled.");
            if (!botSettings.loadEntities || !botSettings.loadPlayers) return new PluginResponse(false, "'Load players' must be enabled.");
            if(string.IsNullOrWhiteSpace(this.Setting.At(0).Get<string>())) return new PluginResponse(false, "Invalid owner name/uuid.");
            return new PluginResponse(true);
        }

        public override void OnStart() {

            var clickGroup = (IParentSetting)Setting.Get("Clicks");
            var equipmentGroup = (IParentSetting)Setting.Get("Equipment");

            var names = new ResolvableNameCollection(Setting.GetValue<string>("Friendly name(s)/uuid(s)"));

            RegisterTask(new Follow(Setting.GetValue<string>("Owner name/uuid"), (Mode)Setting.GetValue<int>("Mode"), names));
            RegisterTask(new Attack(clickGroup.GetValue<int>("Clicks per second"), clickGroup.GetValue<int>("Miss rate"), equipmentGroup.GetValue<bool>("Equip best weapon?")));
            RegisterTask(new Equipment(equipmentGroup.GetValue<bool>("Auto equip best armor?")));
        }
    }

    public enum Mode
    {
        Passive,
        Aggresive
    }

    public class ResolvableNameCollection
    {
        public List<ResolvableName> Names = new List<ResolvableName>();
        private int m_unresolved = 0;

        public ResolvableNameCollection(string friendlyNames) {

            if (!string.IsNullOrWhiteSpace(friendlyNames)) {

                string[] friendlyArray;

                //Check if multiple.
                if (friendlyNames.Contains(' ')) friendlyArray = friendlyNames.Split(' ');
                else friendlyArray = new[] {friendlyNames};

                for (int i = 0; i < friendlyArray.Length; i++)
                    if (friendlyArray[i].Length == 32 || friendlyArray[i].Length == 36)
                        Add(new ResolvableName(friendlyArray[i].Replace("-", "")));
                    else
                        Add(new ResolvableName(friendlyArray[i], false));
            }
        }

        public void Add(ResolvableName Name) {
            if (Name == null || !Name.Resolved) {
                m_unresolved += 1;
                return; // Safety check.
            }

            this.Names.Add(Name);
            Targeter.IgnoreList.Add(Name.Uuid);
        }

        public bool HasUnresolved() {
            return m_unresolved > 0;
        }

        public void Resolve(IEntityList entities) {
            for (int i = 0; i < Names.Count; i++)
                if (!Names[i].Resolved) {
                    if (Names[i].Resolve(entities))
                        this.m_unresolved -= 1;
                }
        }
    }

    public class ResolvableName
    {
        public string Name;
        public string Uuid;
        public bool Resolved;

        public ResolvableName(string Name, bool Resolved)
        {
            this.Name = Name;
            this.Resolved = Resolved;
        }
        public ResolvableName(string Uuid)
        {
            this.Uuid = Uuid;
            this.Resolved = true;
        }

        public bool Resolve(IEntityList entities)
        {
            this.Uuid = entities.FindUuidByName(Name)?.Uuid;
            this.Resolved = this.Uuid != null;
            if (this.Resolved)
                Targeter.IgnoreList.Add(this.Uuid);
            return this.Resolved;
        }
    }
}
