using System;
using Toc.EthRly.Core.Extensions;

namespace Toc.EthRly.Core
{
    public class Packet
    {
        public Guid Id { get; private set; }

        public byte Command { get; set; }

        public byte[] Payload { get; set; }

        public bool ExpectsAnswer => 
            Command == Commands.GetRelayStates.AsByte()
            || Command == Commands.GetMacAddress.AsByte()
            || Command == Commands.GetInputVoltage.AsByte()
            || Command == Commands.GetFirmwareVersion.AsByte();

        private Packet(Guid id)
        {
            Id = id;
            Payload = new byte[6];
        }

        public Packet() : this(Guid.NewGuid()) { }
        public Packet(int command) : this((byte)command) { }
        public Packet(Commands command) : this((byte)command) { }
        public Packet(byte command) : this()
        {
            Command = command;
        }


        public byte[] ToByteArray()
        {
            var packet = new byte[Payload.Length + 1];

            packet[0] = Command;
            Payload.CopyTo(packet, 1);

            return packet;
        }

        public static Packet CreateResponseFor(Packet packet)
        {
            return new Packet(packet.Id);
        }
    }
}
