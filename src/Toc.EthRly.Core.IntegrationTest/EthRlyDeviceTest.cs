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

            await turnAllRelaysOff(device);
            await turnAllRelaysOnOneByOne(device);
            await turnAllRelaysOffOneByOne(device);
            await turnAllRelaysOn(device);
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

        private static async Task turnAllRelaysOn(EthRlyDevice device)
        {
            await device.SetAllRelaysAsync(true);
            var states = await device.GetRelaysStatesAsync();
            Assert.True(states.All(state => state));
        }

        private static async Task turnAllRelaysOffOneByOne(EthRlyDevice device)
        {
            for (int address = 0; address < 8; address++)
            {
                await device.SetRelayStateAsync(address, false);
                var state = await device.GetRelayStateAsync(address);
                Assert.False(state);
            }
        }

        private static async Task turnAllRelaysOnOneByOne(EthRlyDevice device)
        {
            for (int address = 0; address < 8; address++)
            {
                await device.SetRelayStateAsync(address, true);
                var state = await device.GetRelayStateAsync(address);
                Assert.True(state);
            }
        }

        private static async Task turnAllRelaysOff(EthRlyDevice device)
        {
            await device.SetAllRelaysAsync(false);
            var states = await device.GetRelaysStatesAsync();
            Assert.True(states.All(state => !state));
        }
    }
}
