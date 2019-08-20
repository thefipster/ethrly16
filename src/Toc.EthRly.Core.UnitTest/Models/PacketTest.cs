using Xunit;

namespace Toc.EthRly.Core.UnitTest.Models
{
    public class PacketTest
    {
        [Theory]
        [InlineData(Commands.GetFirmwareVersion)]
        [InlineData(Commands.GetInputVoltage)]
        [InlineData(Commands.GetMacAddress)]
        [InlineData(Commands.GetRelayStates)]
        public void ExpectsAnswerPositiveTest(Commands command)
        {
            var packet = new Packet(command);

            Assert.True(packet.ExpectsAnswer);
        }

        [Theory]
        [InlineData(Commands.AllRelaysOff)]
        [InlineData(Commands.AllRelaysOn)]
        [InlineData(Commands.RelayOff)]
        [InlineData(Commands.RelayOn)]  
        [InlineData(Commands.SetRelayState)]
        public void ExpectsAnswerNegativeTest(Commands command)
        {
            var packet = new Packet(command);

            Assert.False(packet.ExpectsAnswer);
        }

        [Fact]
        public void CreateResponseTest()
        {
            var requestPacket = new Packet();

            var responsePacket = Packet.CreateResponseFor(requestPacket);

            Assert.Equal(requestPacket.Id, responsePacket.Id);
        }
    }
}
