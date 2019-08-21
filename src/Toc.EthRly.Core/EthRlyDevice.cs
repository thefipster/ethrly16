using System;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Toc.EthRly.Core.Exceptions;
using Toc.EthRly.Core.Extensions;
using Toc.EthRly.Core.Models;

namespace Toc.EthRly.Core
{
    public class EthRlyDevice : IDisposable
    {
        private readonly Options _options;
        private readonly Tranceiver _tranceiver;

        public EthRlyDevice(string hostname)
            : this(new Options(hostname)) { }

        public EthRlyDevice(string hostname, int port)
            : this(new Options(hostname, port)) { }

        public EthRlyDevice(string hostname, int port, int timeoutInMs)
            : this(new Options(hostname, port, TimeSpan.FromMilliseconds(timeoutInMs))) { }

        public EthRlyDevice(string hostname, int port, int timeoutInMs, CancellationToken token)
            : this(new Options(hostname, port, TimeSpan.FromMilliseconds(timeoutInMs)), token) { }
        
        public EthRlyDevice(Options options)
        {
            var token = getCancellation();
            _options = options;
            _tranceiver = new Tranceiver(_options, token);
        }

        public EthRlyDevice(Options options, CancellationToken token)
        {
            _options = options;
            _tranceiver = new Tranceiver(_options, token);
        }

        public async Task<bool> GetRelayStateAsync(int relayIndex)
        {
            throwExceptionOnBadIndex(relayIndex);
            var states = await GetRelaysStatesAsync();
            return states[relayIndex];
        }

        public async Task<bool[]> GetRelaysStatesAsync()
        {
            var stateMap = await tranceive(Commands.GetRelayStates);
            var states = stateMap.Payload.ToStateArray(_options.RelayCount);
            return states;
        }

        public async Task SetRelayStateAsync(int relayIndex, bool newState)
        {
            throwExceptionOnBadIndex(relayIndex);
            var packet = newState
                ? new Packet(Commands.RelayOn.AsByte() + relayIndex)
                : new Packet(Commands.RelayOff.AsByte() + relayIndex);

            await _tranceiver.TranceiveAsync(packet);
        }

        public async Task SetAllRelaysAsync(bool newState)
        {
            var packet = newState
                ? new Packet(Commands.AllRelaysOn)
                : new Packet(Commands.AllRelaysOff);

            await _tranceiver.TranceiveAsync(packet);
        }

        public async Task<bool> ToggleRelayStateAsync(int relayIndex)
        {
            throwExceptionOnBadIndex(relayIndex);
            var state = await GetRelayStateAsync(relayIndex);
            await SetRelayStateAsync(relayIndex, !state);
            return !state;
        }

        public async Task<double> GetVoltageAsync()
        {
            var voltage = await tranceive(Commands.GetInputVoltage);
            return voltage.Payload.ToVoltage();
        }

        public async Task<int> GetFirmwareVersionAsync()
        {
            var firmware = await tranceive(Commands.GetFirmwareVersion);
            return firmware.Payload.ToFirmwareVersion();
        }

        public async Task<byte[]> GetMacAddressAsync()
        {
            var macAddress = await tranceive(Commands.GetMacAddress);
            return macAddress.Payload;
        }

        private async Task<Packet> tranceive(Commands command)
        {
            var request = new Packet(Commands.GetFirmwareVersion);
            return await _tranceiver.TranceiveAsync(request);
        }

        private void throwExceptionOnBadIndex(int relayIndex)
        {
            if (relayIndex < 0 || relayIndex >= _options.RelayCount)
                throw new InvalidRelayAddressException(relayIndex);
        }

        private CancellationToken getCancellation()
        {
            var tokenSource = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += context => tokenSource.Cancel();
            return tokenSource.Token;
        }

        public void Dispose()
            => _tranceiver.Dispose();
    }
}
