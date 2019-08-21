using System.Linq;
using System.Threading.Tasks;
using Toc.EthRly.Core.Models;
using Xunit;

namespace Toc.EthRly.Core.IntegrationTest
{
    public class EthRlyDeviceTest
    {
        [Fact]
        public async Task FullTest()
        {
            var expectedFirmwareVersion = 7;
            var expectedMacAddress = new byte[] { 0, 4, 163, 72, 186, 44 };

            var options = new Options("192.168.1.150");
            var device = new EthRlyDevice(options);

            await switchAllRelays(device, false);
            await switchAllRelaysOneByOne(device, true);
            await switchAllRelaysOneByOne(device, false);
            await switchAllRelays(device, true);
            await toggleAllRelays(device);

            var voltage = await device.GetVoltageAsync();
            var firmwareVersion = await device.GetFirmwareVersionAsync();
            var macAddress = await device.GetMacAddressAsync();
            var relayStates = await device.GetRelaysStatesAsync();

            Assert.Equal(expectedFirmwareVersion, firmwareVersion);
            Assert.True(voltage > 0);
            Assert.True(expectedMacAddress.SequenceEqual(macAddress));
            Assert.True(relayStates.All(state => !state));
        }

        private static async Task toggleAllRelays(EthRlyDevice device)
        {
            for (int address = 0; address < 8; address++)
            {
                var beforeState = await device.GetRelayStateAsync(address);
                await device.ToggleRelayStateAsync(address);
                var afterState = await device.GetRelayStateAsync(address);
                Assert.False(beforeState == afterState);
            }
        }

        private static async Task switchAllRelaysOneByOne(EthRlyDevice device, bool newState)
        {
            for (int address = 0; address < 8; address++)
            {
                await device.SetRelayStateAsync(address, newState);
                var state = await device.GetRelayStateAsync(address);
                Assert.False(state);
            }
        }

        private static async Task switchAllRelays(EthRlyDevice device, bool newState)
        {
            await device.SetAllRelaysAsync(newState);
            var states = await device.GetRelaysStatesAsync();
            Assert.True(states.All(state => !state));
        }
    }
}
