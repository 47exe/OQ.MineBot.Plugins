using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OQ.MineBot.PluginBase;
using OQ.MineBot.PluginBase.Base;
using OQ.MineBot.PluginBase.Base.Plugin;
using OQ.MineBot.PluginBase.Bot;
using WhereAmIPlugin.Tasks;

namespace WhereAmIPlugin
{
    [Plugin(1, "Where am I?", "Saves each bots position in a text file.")]
    public class PluginCore : IStartPlugin
    {
        private bool stopped;

        public override void OnLoad(int version, int subversion, int buildversion) {
            Setting.Add(new PathSetting("Where to save", "Path of a .txt file, which will hold all the saved locations.", ""));
        }
        public override PluginResponse OnEnable(IBotSettings botSettings) {

            toSaveQueue = new ConcurrentQueue<IPlayer>();
            stopped = false;
            new Thread(SaveLoop).Start();

            return new PluginResponse(true);
        }
        public override void OnDisable() {
            stopped = true;
        }

        public override void OnStart() {
            RegisterTask(new Register(toSaveQueue));
        }

        private ConcurrentQueue<IPlayer> toSaveQueue;
        private void SaveLoop() {
            while (!stopped) {

                if (toSaveQueue.IsEmpty) {
                    Thread.Sleep(50);
                    continue;
                }

                using (var stream = File.OpenWrite(Setting.At(0).Get<string>()))
                    while (!toSaveQueue.IsEmpty) {
                        IPlayer player;
                        if (!toSaveQueue.TryDequeue(out player)) continue;

                        var bytes = Encoding.UTF8.GetBytes(Environment.NewLine + player.status.username + " - " + player.status.entity.location.ToLocation(0));
                        stream.Write(bytes, 0, bytes.Length);
                    }
            }
        }
    }
}
