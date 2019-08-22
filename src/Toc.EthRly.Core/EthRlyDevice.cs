using System;
using System.Threading.Tasks;
using Toc.EthRly.Core.Exceptions;
using Toc.EthRly.Core.Extensions;
using Toc.EthRly.Core.Models;

namespace Toc.EthRly.Core
{
    public class EthRlyDevice : IDisposable
    {
        private readonly Options _options;
        private readonly MessageGovernor _tranceiver;
        
        public EthRlyDevice(Options options)
        {
            _options = options;
            _tranceiver = new MessageGovernor(
                new PacketCache(options),
                new TcpTransceiver(options),
                _options);
        }

        public async Task<bool[]> GetRelaysStatesAsync() => (await _tranceiver
            .TransceiveAsync(Commands.GetRelayStates))
            .ToStateArray(_options.RelayCount);

        public async Task<bool> GetRelayStateAsync(int relayIndex)
        {
            throwExceptionOnBadIndex(relayIndex);
            var states = await GetRelaysStatesAsync();
            return states[relayIndex];
        }

        public void SetRelayState(int relayIndex, bool newState)
        {
            throwExceptionOnBadIndex(relayIndex);
            var packet = newState
                ? new Packet(Commands.RelayOn.AsByte() + relayIndex)
                : new Packet(Commands.RelayOff.AsByte() + relayIndex);

            _tranceiver.Transmit(packet);
        }

        public void SetAllRelays(bool newState)
        {
            var packet = newState
                ? new Packet(Commands.AllRelaysOn)
                : new Packet(Commands.AllRelaysOff);

            _tranceiver.Transmit(packet);
        }

        public async Task<bool> ToggleRelayStateAsync(int relayIndex)
        {
            throwExceptionOnBadIndex(relayIndex);
            var state = await GetRelayStateAsync(relayIndex);
            SetRelayState(relayIndex, !state);
            return !state;
        }

        public async Task<double> GetVoltageAsync() => (await _tranceiver
            .TransceiveAsync(Commands.GetInputVoltage))
            .ToVoltage();

        public async Task<int> GetFirmwareVersionAsync() =>  (await _tranceiver
            .TransceiveAsync(Commands.GetFirmwareVersion))
            .ToFirmwareVersion();

        public async Task<byte[]> GetMacAddressAsync() =>  (await _tranceiver
            .TransceiveAsync(Commands.GetMacAddress))
            .Payload;

        private void throwExceptionOnBadIndex(int relayIndex)
        {
            if (relayIndex < 0 || relayIndex >= _options.RelayCount)
                throw new InvalidRelayAddressException(relayIndex);
        }

        public void Dispose()
            => _tranceiver.Dispose();
    }
}
