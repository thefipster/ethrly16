using System.Threading.Tasks;
using Toc.EthRly.MqttRelay.Models;
using Toc.EthRly.MqttRelay.Tasks;

namespace Toc.EthRly.MqttRelay.Client
{
    class Program
    {
        static void Main()
        {
            var state = new ServiceState();

            Task.WaitAll(
                new MqttListenerTask(state).Run(),
                new SyncRelayStateTask(state).Run()
            );
        }
    }
}
