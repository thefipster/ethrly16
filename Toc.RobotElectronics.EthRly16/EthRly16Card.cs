using System;
using System.Net.Sockets;
using System.Threading;

namespace Toc.RobotElectronics.EthRly16
{
    /// <summary>
    /// The RobotElectronics EthRly 16 relay card with 8 250V 16A relays.
    /// </summary>
    public class EthRly16Card
    {
        /// <summary>
        /// Number of relays on the card
        /// </summary>
        public const int RelayCount = 8;

        /// <summary>
        /// The hostname of the relay card. Usually the IP address.
        /// </summary>
        private readonly string host;

        /// <summary>
        /// The port of the relay card.
        /// </summary>
        private readonly int port;

        /// <summary>
        /// The receive timeout of network messages.
        /// </summary>
        private readonly int timeout;

        /// <summary>
        /// The network socket to the relay card.
        /// </summary>
        private Socket socket;

        /// <summary>
        /// Initializes a new instance of the <see cref="EthRly16Card"/> class.
        /// </summary>
        /// <param name="ipAddress">
        /// The ip address of the relay card.
        /// </param>
        /// <param name="port">
        /// The port of the relay card. Default is 17494.
        /// </param>
        /// <param name="timeout">
        /// The receive timeout for network messages. Default is 1000ms
        /// </param>
        public EthRly16Card(string ipAddress, int port = 17494, int timeout = 1000)
        {
            host = ipAddress;
            this.port = port;
            this.timeout = timeout;
        }

        /// <summary>
        /// Converts the returned byte of the relais card into an array with 8 bools indicating the status of each relais.
        /// The given byte represents a bitmask. Each bit defines one relay.
        /// </summary>
        /// <param name="states">The byte return value of the relais card.</param>
        /// <returns>An array with 8 bools indicating each relais state.</returns>
        private static bool[] StatesToArray(byte states)
        {
            bool[] bitField = new bool[8];

            byte bitMask = 128;

            for (int i = 7; i >= 0; i--)
            {
                if (states >= bitMask)
                {
                    bitField[i] = true;
                    states = (byte)(states - bitMask);
                }

                bitMask = (byte)(bitMask / 2);
            }

            return bitField;
        }

        /// <summary>
        /// Handles the sending and receiving of network messages.
        /// </summary>
        /// <param name="packet">
        /// The packet you want to send.
        /// </param>
        /// <returns>
        /// The payload of the answer.
        /// </returns>
        private EthRlyResponse DoWork(Packet packet)
        {
            lock (this)
            {
                TcpClient client = new TcpClient(host, port) { ReceiveTimeout = timeout };

                socket = client.Client;
                socket.Send(packet.ToByteArray());
                if (packet.Command == Commands.GetRelayStates ||
                    packet.Command == Commands.GetFirmwareVersion ||
                    packet.Command == Commands.GetInputVoltage ||
                    packet.Command == Commands.GetMacAddress)
                {
                    int receivedBytes = 0;
                    EthRlyResponse answer = new EthRlyResponse
                    {
                        Command = packet.Command,
                        Payload = new byte[6]
                    };

                    while (receivedBytes == 0)
                    {
                        receivedBytes = socket.Receive(answer.Payload);
                    }

                    client.Close();
                    return answer;
                }

                Thread.Sleep(10);
                client.Close();
                return null;
            }
        }

        /// <summary>
        /// Sets the relay to the state.
        /// </summary>
        /// <param name="relay">
        /// The relay number.
        /// </param>
        /// <param name="state">
        /// The state. true = on, false = off.
        /// </param>
        /// <exception cref="ArgumentException"> 
        /// If the relay number is not valid.
        /// </exception>
        public void SetRelay(int relay, bool state)
        {
            CheckRelay(relay);

            DoWork(new Packet
            {
                Command = state ? (byte)(Commands.AllRelaysOn + relay)
                                : (byte)(Commands.AllRelaysOff + relay)
            });
        }

        /// <summary>
        /// Toggles the state of a relay.
        /// </summary>
        /// <param name="relay">
        /// The relay number.
        /// </param>
        /// <returns>
        /// The state after the switch as a <see cref="bool"/>.
        /// </returns>
        /// <exception cref="ArgumentException"> 
        /// If the relay number is not valid.
        /// </exception>
        public bool ToggleRelay(int relay)
        {
            CheckRelay(relay);

            bool state = GetRelayState(relay);
            SetRelay(relay, !state);

            return !state;
        }

        /// <summary>
        /// Gets the state of all relays.
        /// </summary>
        /// <returns>
        /// The state of every relay. 0 = no data. true = on, false = off
        /// </returns>
        public bool[] GetRelaysStates()
        {
            EthRlyResponse result = DoWork(new Packet
            {
                Command = Commands.GetRelayStates
            });

            bool[] states = StatesToArray(result.Payload[0]);
            return states;
        }

        /// <summary>
        /// Gets the input voltage of the relay card in volts.
        /// </summary>
        /// <returns>Returns the input voltage of the relay card in volts.</returns>
        public double GetInputVoltage()
        {
            EthRlyResponse result = DoWork(new Packet
            {
                Command = Commands.GetInputVoltage
            });

            return result.Payload[0] / 10.0;
        }

        /// <summary>
        /// Gets the MAC address of the network interface from the relay card.
        /// </summary>
        /// <returns>The MAC address of the relay card.</returns>
        public byte[] GetMacAddress()
        {
            EthRlyResponse result = DoWork(new Packet
            {
                Command = Commands.GetMacAddress
            });

            return result.Payload;
        }

        /// <summary>
        /// Gets the firmware version installed on the relay card.
        /// </summary>
        /// <returns>Returns the firmware version of the relay card.</returns>
        public int GetFirmwareVersion()
        {
            EthRlyResponse result = DoWork(new Packet
            {
                Command = Commands.GetFirmwareVersion
            });

            return result.Payload[0];
        }

        /// <summary>
        /// Checks if the given relay address is valid otherwise an argument exception is thrown.
        /// </summary>
        /// <param name="address">Relay address</param>
        private void CheckRelay(int address)
        {
            if (address < 1 || address > RelayCount)
            {
                throw new ArgumentException("Relais address is not valid.", "relay");
            }
        }

        /// <summary>
        /// Gets the state of the relay
        /// </summary>
        /// <param name="relay">
        /// The relay number.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/> indicating the state of the relay. true = on, false = off.
        /// </returns>
        /// <exception cref="ArgumentException"> If the relay number is not valid.
        /// </exception>
        public bool GetRelayState(int relay)
        {
            CheckRelay(relay);

            bool[] states = GetRelaysStates();
            return states[relay - 1];
        }

        /// <summary>
        /// Sets all relays to the state
        /// </summary>
        /// <param name="state">
        /// The state to switch to. true = on, false = off.
        /// </param>
        public void SetRelays(bool state)
        {
            DoWork(new Packet
            {
                Command = state ? Commands.AllRelaysOn : Commands.AllRelaysOff
            });
        }
    }
}