using System;
using System.Net.Sockets;
using Toc.EthRly.Core.Models;

namespace Toc.EthRly.Core
{
    public class TcpConnection : IDisposable
    {
        private readonly Options _options;
        private TcpClient _tcp;

        public TcpConnection(Options options)
        {
            _options = options;
        }

        public Socket Socket => _tcp?.Client;

        public Socket EnsureSocket()
        {
            if (IsConnected)
                return Socket;

            Dispose();
            Connect();

            return Socket;
        }

        public bool IsConnected =>
            _tcp?.Client != null
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

        public void Dispose()
        {
            Disconnect();
            _tcp?.Dispose();
        }
    }
}
