using System.Collections.Concurrent;
using OQ.MineBot.PluginBase;
using OQ.MineBot.PluginBase.Base.Plugin.Tasks;

namespace WhereAmIPlugin.Tasks
{
    public class Register : ITask
    {
        private readonly ConcurrentQueue<IPlayer> queue;

        public Register(ConcurrentQueue<IPlayer> queue) {
            this.queue = queue;
        }

        public override bool Exec() {
            return true;
        }

        public override void Start() {
            queue.Enqueue(player);    
        }
    }
}