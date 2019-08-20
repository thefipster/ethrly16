using System;
using System.Threading.Tasks;
using Toc.EthRly.MqttRelay.Models;

namespace Toc.EthRly.MqttRelay.Tasks
{
    class SyncRelayStateTask
    {
        private readonly ServiceState _state;

        public SyncRelayStateTask(ServiceState state) 
            => _state = state;

        internal async Task Run()
        {
            while (!_state.Token.IsCancellationRequested)
            {
                try
                {
                    await syncState();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    await Task.Delay(_state.Config.Ethrly.SyncInterval);
                }
            }
        }

        private async Task syncState()
        {
            var currentState = await _state.Ethrly.GetRelaysStatesAsync();

            for (int i = 0; i < _state.Config.Ethrly.RelayCount; i++)
            {
                if (currentState[i] != _state.RelayState[i])
                {
                    var topic = string.Format(_state.Config.Mqtt.GetTemplate, i);
                    var stateNumber = currentState[i] ? 1 : 0;
                    var message = _state.Config.Mqtt.Encoding.GetBytes(stateNumber.ToString());

                    _state.Mqtt.Publish(topic, message, _state.Config.Mqtt.Qos, true);
                    _state.RelayState[i] = currentState[i];
                }
            }
        }
    }
}
