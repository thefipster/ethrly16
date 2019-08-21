using System;
using System.Linq;

namespace Toc.EthRly.Core.Extensions
{
    public static class PacketExtensions
    {
        /// <summary>
        /// Insert the statemap. A byte where each bit represents a relay state. 
        /// Highest bit first
        /// </summary>
        /// <param name="stateMap">1 byte bitmap for relay states. 0 = off, 1 = on.</param>
        /// <param name="relayCount">Number of bits used</param>
        /// <returns>bool[] representing the bits as bools</returns>
        public static bool[] ToStateArray(this Packet packet, int relayCount)
        {
            byte stateMap = packet.Payload.FirstOrDefault();
            bool[] bitField = new bool[relayCount];
            byte bitMask = (byte)Math.Pow(2, relayCount - 1);

            for (int i = relayCount - 1; i >= 0; i--)
            {
                if (stateMap >= bitMask)
                {
                    bitField[i] = true;
                    stateMap = (byte)(stateMap - bitMask);
                }

                bitMask = (byte)(bitMask / 2);
            }

            return bitField;
        }

        public static double ToVoltage(this Packet packet)
        {
            var sigByte = packet.Payload.FirstOrDefault();
            return (double)sigByte / 10;
        }

        public static int ToFirmwareVersion(this Packet packet)
        {
            var firmwareByte = packet.Payload.FirstOrDefault();
            return firmwareByte;
        }
    }
}
