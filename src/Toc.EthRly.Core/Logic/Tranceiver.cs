using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Toc.EthRly.Core.Exceptions;
using Toc.EthRly.Core.Models;

namespace Toc.EthRly.Core
{
    public class Tranceiver : IDisposable
    {
        private readonly Options _options;
        private readonly PacketCache _cache;
        private readonly TcpConnection _connection;

        public Tranceiver(Options options, CancellationToken token)
        {
            _options = options;
            _cache = new PacketCache(options);
            _connection = new TcpConnection(options);

            Task.Run(() => tranceiveAsync());
        }

        public void Trasmit(Packet request) => _cache.Enqueue(request);

        public async Task<Packet> TranceiveAsync(Packet request)
        {
            _cache.Enqueue(request);

            if (request.ExpectsAnswer)
                return await _cache.DequeueAsync(request);

            return null;
        }

        private async Task tranceiveAsync()
        {
            while (true)
            {
                await tryTranceiveAsync();
                await Task.Delay(_options.DelayBetweenCommands);
            }
        }

        private async Task tryTranceiveAsync()
        {
            try
            {
                await tranceivePacketAsync();
            }
            catch (QueueIsEmptyException)
            {
                // too bad, let's try again
            }
            catch (TimeoutException)
            {
                // this happens... let's continue
            }
        }

        private async Task tranceivePacketAsync()
        {
            var socket = _connection.EnsureSocket();
            var request = await _cache.DequeueAsync();

            transmit(socket, request);

            if (request.ExpectsAnswer)
                await receiveAsync(socket, request);
        }

        private void transmit(Socket socket, Packet request)
        {
            var buffer = request.ToByteArray();
            socket.Send(buffer);
        }

        private async Task receiveAsync(Socket socket, Packet request)
        {
            var response = Packet.CreateResponseFor(request);
            while (socket.Receive(response.Payload) == 0)
                await Task.Delay(20);

            await _cache.EnqueueAsync(request, response);
        }

        public void Dispose()
        {
            _cache.Dispose();
            _connection.Dispose();
        }
    }
}
