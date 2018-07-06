using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OQ.MineBot.PluginBase;
using OQ.MineBot.PluginBase.Base;
using OQ.MineBot.PluginBase.Base.Plugin;
using OQ.MineBot.PluginBase.Bot;
using OQ.MineBot.PluginBase.Classes;
using OQ.MineBot.PluginBase.Classes.Base;
using OQ.MineBot.PluginBase.Classes.Entity.Mob;
using OQ.MineBot.PluginBase.Classes.Entity.Player;
using OQ.MineBot.PluginBase.Utility;
using OQ.MineBot.Protocols.Classes.Base;
using RaidAlertsPlugin.Tasks;

namespace RaidAlertsPlugin
{
    [Plugin(1, "Raid alerts", "Notifies the user on discord when an explosion occurs/mobs appear/players get close.")]
    public class PluginCore : IStartPlugin
    {
        public override void OnLoad(int version, int subversion, int buildversion) {
            /*
            Setting[0] = new StringSetting("User or Channel ID", "Enable developer mode: Settings->Appearance->Developer mode. Copy id: right click channel and click 'Copy ID'.", "");
            Setting[1] = new BoolSetting("Local notifications", "", true);
            Setting[2] = new BoolSetting("Explosion notifications", "", true);
            Setting[3] = new BoolSetting("Wither notifications", "", true);
            Setting[4] = new BoolSetting("Creeper notifications", "", true);
            Setting[5] = new BoolSetting("Player notifications", "", true);
            Setting[6] = new StringSetting("Friendly uuid(s)/name(s)", "Uuids/name(s) split by space.", "");
            Setting[7] = new StringSetting("Lamp coordinates", "Coordinates in the [X Y Z] format, split by a space", "[-1 -1 -1] [0 0 0] [1 1 1]");
            Setting[8] = new ComboSetting("Mode", "Notification mode", new []{"none", "@everyone", "@everyone + tts"}, 1);
            Setting[9] = new LinkSetting("Add bot", "Adds the bot to your discord channel (you must have administrator permissions).", "https://discordapp.com/oauth2/authorize?client_id=299708378236583939&scope=bot&permissions=6152");
            Setting[10] = new BoolSetting("Detect falling blocks", "Should the bot detected falling sand and falling tnt", true);
            */

            Setting.Add(new StringSetting("User or Channel ID", "Enable developer mode: Settings->Appearance->Developer mode. Copy id: right click channel and click 'Copy ID'.", ""));
            Setting.Add(new BoolSetting("Local notifications", "", true));

            var notificationGroup = new GroupSetting("Notifications", "Select which notifications you wish to get here.");
                notificationGroup.Add(new BoolSetting("Explosion notifications", "", true));
                notificationGroup.Add(new BoolSetting("Wither notifications", "", true));
                notificationGroup.Add(new BoolSetting("Creeper notifications", "", true));
                notificationGroup.Add(new BoolSetting("Player notifications", "", true));
                notificationGroup.Add(new BoolSetting("Detect falling blocks", "Should the bot detected falling sand and falling tnt", true));
            Setting.Add(notificationGroup);

            var otherGroup = new GroupSetting("Miscellaneous", "");
                otherGroup.Add(new StringListSetting("Friendly uuid(s)/name(s)", "Uuids/name(s) split by space.", ""));
                otherGroup.Add(new StringListSetting("Lamp coordinates", "Coordinates in the [X Y Z] format, split by a space", "[-1 -1 -1] [0 0 0] [1 1 1]"));
                otherGroup.Add(new ComboSetting("Mode", "Notification mode", new[] { "none", "@everyone", "@everyone + tts" }, 1));
            Setting.Add(otherGroup);

            Setting.Add(new LinkSetting("Add bot", "Adds the bot to your discord channel (you must have administrator permissions).", "https://discordapp.com/oauth2/authorize?client_id=299708378236583939&scope=bot&permissions=6152"));
        }

        public override PluginResponse OnEnable(IBotSettings botSettings) {

            var notificationsGroup = (IParentSetting)Setting.Get("Notifications");
            var miscellaneousGroup = (IParentSetting)Setting.Get("Miscellaneous");

            if (!botSettings.loadEntities || !botSettings.loadPlayers) return new PluginResponse(false, "'Load entities & load players' must be enabled.");
            if(!botSettings.loadWorld && !string.IsNullOrWhiteSpace(miscellaneousGroup.GetValue<string>("Lamp coordinates"))) return new PluginResponse(false, "'Load worlds' must be enabled.");

            try {
                if(string.IsNullOrWhiteSpace(Setting.At(0).Get<string>())) return new PluginResponse(false, "Could not parse discord id.");
                ulong.Parse(Setting.At(0).Get<string>());
            }
            catch (Exception ex) {
                return new PluginResponse(false, "Could not parse discord id.");
            }

            // Do warnings.
            if(string.IsNullOrWhiteSpace(miscellaneousGroup.GetValue<string>("Lamp coordinates")) && (!botSettings.loadWorld ||botSettings.staticWorlds)) DiscordHelper.Error("[RaidAlerts] 'Load worlds' should be enabled, 'Shared worlds' should be disabled.", 584);
            if (notificationsGroup.GetValue<bool>("Detect falling blocks") && (!botSettings.loadEntities || !botSettings.loadMobs)) DiscordHelper.Error("[RaidAlerts] 'Load entities' & 'Load mobs' should be enabled.", 585);

            return new PluginResponse(true);
        }

        public override void OnStart() {

            var notificationsGroup = (IParentSetting)Setting.Get("Notifications");
            var miscellaneousGroup = (IParentSetting)Setting.Get("Miscellaneous");

            //Parse the lamp coordinates.
            var lampLocations = new List<ILocation>();
            var splitReg = new Regex(@"\[(.*?)\]");
            var split = splitReg.Matches(miscellaneousGroup.GetValue<string>("Lamp coordinates"));
            foreach (var match in split) {
                //Split into numbers only.
                var numbers = match.ToString().Replace("[", "").Replace("]", "").Split(' ');
                if(numbers.Length != 3) continue;

                //Try-catch in case the user
                //entered an invalid character.
                try {
                    int x = int.Parse(numbers[0]);
                    int y = int.Parse(numbers[1]);
                    int z = int.Parse(numbers[2]);

                    lampLocations.Add(new Location(x, y, z));
                }
                catch { }
            }

            // Add listening tasks.
            RegisterTask(new Alerts(
                            ulong.Parse(Setting.At(0).Get<string>()),
                            Setting.At(1).Get<bool>(),
                            notificationsGroup.GetValue<bool>("Explosion notifications"), notificationsGroup.GetValue<bool>("Wither notifications"), notificationsGroup.GetValue<bool>("Creeper notifications"), notificationsGroup.GetValue<bool>("Player notifications"),
                            miscellaneousGroup.GetValue<string>("Friendly uuid(s)/name(s)"), lampLocations.ToArray(), (DiscordHelper.Mode)miscellaneousGroup.GetValue<int>("Mode"), notificationsGroup.GetValue<bool>("Detect falling blocks")
                        ));
        }
    }
}
