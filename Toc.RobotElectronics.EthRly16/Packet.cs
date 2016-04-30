namespace Toc.RobotElectronics.EthRly16
{
    public class Packet
    {
        /// <summary>
        /// The command for the relay card.
        /// </summary>
        public byte Command { get; set; }


        /// <summary>
        /// Data to add to the command. E.g. if you want to set a relay, you have to add the relay address to the payload.
        /// </summary>
        public byte[] Payload { get; set; }

        /// <summary>
        /// A packet that can be send to a Robot Electronics EthRly16 card
        /// </summary>
        public Packet()
        {
            Payload = new byte[6];
        }

        /// <summary>
        /// Creates the real bytes that can be send over the wire.
        /// </summary>
        /// <returns></returns>
        public byte[] ToByteArray()
        {
            byte[] packet = new byte[Payload.Length + 1];

            packet[0] = Command;
            Payload.CopyTo(packet, 1);

            return packet;
        }
    }
}
