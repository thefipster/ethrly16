using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Toc.EthRly.Core.Interface;
using Toc.EthRly.Core.Models;

namespace Toc.EthRly.Core
{
    public class TcpTransceiver : ITransceiver
    {
        private readonly Options _options;
        private TcpClient _tcp;

        public TcpTransceiver(Options options) => _options = options;

        private Socket Socket => _tcp?.Client;

        public bool IsConnected => _tcp?.Client != null
            && _tcp.Connected
            && _tcp.Client.Connected;

        public void Connect()
        {
            _tcp = new TcpClient
            {
                ExclusiveAddressUse = false,
                ReceiveTimeout = (int)_options.Timeout.TotalMilliseconds,
                SendTimeout = (int)_options.Timeout.TotalMilliseconds
            };

            _tcp?.Connect(_options.Endpoint);
        }

        public void Disconnect()
        {
            _tcp?.Client?.Disconnect(true);
            _tcp?.Close();
        }

        public void Transmit(Packet packet)
        {
            var buffer = packet.ToByteArray();
            EnsureSocket().Send(buffer);
        }

        public async Task<Packet> TransceiveAsync(Packet packet)
        {
            Transmit(packet);
            return await receiveAsync(packet);
        }

        public void Dispose()
        {
            Disconnect();
            _tcp?.Dispose();
        }

        private async Task<Packet> receiveAsync(Packet request)
        {
            var response = Packet.CreateResponseFor(request);
            var socket = EnsureSocket();

            while (socket.Receive(response.Payload) == 0)
                await Task.Delay(20);

            return response;
        }

        private Socket EnsureSocket()
        {
            if (IsConnected)
                return Socket;

            Dispose();
            Connect();

            return Socket;
        }
    }
}
