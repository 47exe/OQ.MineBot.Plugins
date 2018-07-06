﻿
using System.Linq;
using ChatTriggerPlugin.Tasks;
using OQ.MineBot.PluginBase.Base;
using OQ.MineBot.PluginBase.Base.Plugin;
using OQ.MineBot.PluginBase.Bot;

namespace ChatTriggerPlugin
{
    [Plugin(1, "Chat Trigger", "Runs a macro if the bot recieves a chat trigger. (Original author: ZerGo0)")]
    public class PluginCore : IStartPlugin
    {
        public override void OnLoad(int version, int subversion, int buildversion)
        {
            Setting.Add(new StringSetting("Authorized Users", "The bot will only hear to those users. (split with space)", ""));
			Setting.Add(new StringSetting("Trigger Keywords", "Keywords the bot should react to (You can also use phrases). (split with comma)", ""));
            Setting.Add(new StringSetting("Trigger Macro", "Macro name that will get triggered", ""));
        }
        public override PluginResponse OnEnable(IBotSettings botSettings)
        {
            if (!botSettings.loadChat) return new PluginResponse(false, "'Load chat' must be enabled.");
            if(string.IsNullOrWhiteSpace(Setting.At(0).Get<string>())) return new PluginResponse(false, "'Authorized Users' not set.");
            if (string.IsNullOrWhiteSpace(Setting.At(1).Get<string>())) return new PluginResponse(false, "'Trigger Keywords' not set.");
            if (string.IsNullOrWhiteSpace(Setting.At(2).Get<string>())) return new PluginResponse(false, "'Trigger Macro' not set.");

            return new PluginResponse(true);
        }
        public override void OnStart()
        {
            RegisterTask(new Chat(
                Setting.At(0).Get<string>().Split(' '),
                Setting.At(1).Get<string>().Split(',').Select(x=>x.First()==' '?x.Skip(1).ToString():x).ToArray(), // If user enters ", " instead of "," remove the space.
                Setting.At(2).Get<string>()));
        }
    }
}
