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
        private readonly Options options;
        private readonly PaketCache _paketCache;
        private readonly TcpConnection connection;

        public Tranceiver(Options options, CancellationToken token)
        {
            this.options = options;
            this._paketCache = new PaketCache(options);
            this.connection = new TcpConnection(options);

            Task.Run(() => tranceiveAsync());
        }

        public void Trasmit(Packet request) => _paketCache.Enqueue(request);

        public async Task<Packet> TranceiveAsync(Packet request)
        {
            _paketCache.Enqueue(request);

            if (request.ExpectsAnswer)
                return await _paketCache.DequeueAsync(request);

            return null;
        }

        private async Task tranceiveAsync()
        {
            while (true)
            {
                await tryTranceiveAsync();
                await Task.Delay(options.DelayBetweenCommands);
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
            var socket = connection.EnsureSocket();
            var request = await _paketCache.DequeueAsync();

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

            await _paketCache.EnqueueAsync(request, response);
        }

        public void Dispose()
        {
            _paketCache.Dispose();
            connection.Dispose();
        }
    }
}
