using Toc.EthRly.Core.Extensions;
using Xunit;

namespace Toc.EthRly.Core.UnitTest.Extensions
{
    public class PacketExtensionTest
    {
        [Fact]
        public void ConvertEmptyPayloadToStateArrayTest()
        {
            var expectedStates = new []{ false, false, false, false, false, false, false, false };
            var relayCount = 8;
            var packet = new Packet();

            var actualStates = packet.ToStateArray(relayCount);

            Assert.Equal(expectedStates, actualStates);

        }
        [Theory]
        [InlineData((byte)0, new [] { false, false, false, false, false, false, false, false })]
        [InlineData((byte)1, new [] { true, false, false, false, false, false, false, false })]
        [InlineData((byte)2, new [] { false, true, false, false, false, false, false, false })]
        [InlineData((byte)4, new [] { false, false, true, false, false, false, false, false })]
        [InlineData((byte)8, new [] { false, false, false, true, false, false, false, false })]
        [InlineData((byte)16, new [] { false, false, false, false, true, false, false, false })]
        [InlineData((byte)32, new [] { false, false, false, false, false, true, false, false })]
        [InlineData((byte)64, new [] { false, false, false, false, false, false, true, false })]
        [InlineData((byte)127, new [] { true, true, true, true, true, true, true, false })]
        [InlineData((byte)128, new [] { false, false, false, false, false, false, false, true })]
        [InlineData((byte)254, new [] { false, true, true, true, true, true, true, true })]
        [InlineData((byte)255, new [] { true, true, true, true, true, true, true, true })]
        public void ConvertFromByteToStateArrayTest(byte byteMap, bool[] expectedStates)
        {
            var relayCount = 8;
            var packet = new Packet();
            packet.Payload = new [] { byteMap };

            var actualStates = packet.ToStateArray(relayCount);

            Assert.Equal(expectedStates, actualStates);
        }

        [Fact]
        public void ConvertEmptyPayloadToVoltage()
        {
            var expectedVoltage = 0;
            var packet = new Packet();

            var actualVoltage = packet.ToVoltage();

            Assert.Equal(expectedVoltage, actualVoltage);
        }

        [Theory]
        [InlineData((byte)0, 0)]
        [InlineData((byte)64, 6.4)]
        [InlineData((byte)127, 12.7)]
        [InlineData((byte)255, 25.5)]
        public void ConvertBytePayloadToVoltage(byte payload, double expectedVoltage)
        {
            var packet = new Packet();
            packet.Payload = new [] { payload };

            var actualVoltage = packet.ToVoltage();

            Assert.Equal(expectedVoltage, actualVoltage);
        }

        [Fact]
        public void ConvertEmptyPayloadToFirmwareVersion()
        {
            var expectedFirmware = 0;
            var packet = new Packet();

            var actualFirmware = packet.ToFirmwareVersion();

            Assert.Equal(expectedFirmware, actualFirmware);
        }

        [Theory]
        [InlineData((byte)0, 0)]
        [InlineData((byte)64, 64)]
        [InlineData((byte)127, 127)]
        [InlineData((byte)255, 255)]
        public void ConvertBytePayloadToFirmwareVersion(byte payload, double expectedFirmware)
        {
            var packet = new Packet();
            packet.Payload = new [] { payload };

            var actualFirmware = packet.ToFirmwareVersion();

            Assert.Equal(expectedFirmware, actualFirmware);
        }
    }
}
