using System.Threading.Tasks;
using Toc.EthRly.Core.Exceptions;
using Toc.EthRly.MqttRelay.Exceptions;
using Toc.EthRly.MqttRelay.Models;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Toc.EthRly.MqttRelay.Tasks
{
    class MqttListenerTask
    {
        private readonly ServiceState _state;

        public MqttListenerTask(ServiceState state)
        {
            _state = state;
            subscribe();
        }

        private void subscribe()
        {
            for (int i = 0; i < _state.Config.Ethrly.RelayCount; i++)
                _state.Mqtt.Subscribe(
                    new[] {string.Format(_state.Config.Mqtt.SetTemplate, i)},
                    new[] {_state.Config.Mqtt.Qos});
        }

        internal async Task Run()
        {
            _state.Mqtt.MqttMsgPublishReceived += onMqttMessageReceived;

            while (!_state.Token.IsCancellationRequested)
            {
                await Task.Delay(1000);
            }
        }

        private async void onMqttMessageReceived(object sender, MqttMsgPublishEventArgs e)
        {
            var relayAddress = getAddressFromTopic(e.Topic);
            var payload = _state.Config.Mqtt.Encoding.GetString(e.Message);

            if (!int.TryParse(payload, out var command))
                return;

            var newState = await execute(command, relayAddress);
            publish(newState, relayAddress);
        }

        private void publish(bool newState, int relayAddress)
        {
            var stateNumber = newState ? 1 : 0;
            var message = _state.Config.Mqtt.Encoding.GetBytes(stateNumber.ToString());
            var topic = string.Format(_state.Config.Mqtt.GetTemplate, relayAddress);

            _state.Mqtt.Publish(topic, message, _state.Config.Mqtt.Qos, true);
        }

        private async Task<bool> execute(int command, int address)
        {
            switch (command)
            {
                case 0:
                case 1:
                    _state.Ethrly.SetRelayState(address, command == 1);
                    return command == 1;
                case 2:
                    return await _state.Ethrly.ToggleRelayStateAsync(address);
                default:
                    throw new InvalidCommandException(command);
            }
        }

        private int getAddressFromTopic(string topic)
        {
            var parts = topic.Split('/');
            var address = parts[1];
            if (int.TryParse(address, out var index))
                return index;

            throw new InvalidRelayAddressException(address);
        }
    }
}
