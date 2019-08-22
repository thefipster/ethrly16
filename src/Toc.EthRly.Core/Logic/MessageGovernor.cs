using System;
using System.Threading.Tasks;
using Toc.EthRly.Core.Exceptions;
using Toc.EthRly.Core.Interface;
using Toc.EthRly.Core.Models;

namespace Toc.EthRly.Core
{
    public class MessageGovernor : IDisposable
    {
        private readonly ICache _cache;
        private readonly ITransceiver _transceiver;
        private readonly Options _options;

        public MessageGovernor(ICache cache, ITransceiver transceiver, Options options)
        {
            _cache = cache;
            _transceiver = transceiver;
            _options = options;

            Task.Run(() => tranceiveAsync());
        }

        public void Transmit(Commands command) 
            => Transmit(new Packet(command));

        public void Transmit(Packet request) 
            => _cache.Enqueue(request);

        public async Task<Packet> TransceiveAsync(Commands command)
            => await TransceiveAsync(new Packet(command));

        public async Task<Packet> TransceiveAsync(Packet request)
        {
            _cache.Enqueue(request);
            return await _cache.DequeueAsync(request);
        }

        private async Task tranceiveAsync()
        {
            while (!_options.Token.IsCancellationRequested)
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
                finally
                {
                    await Task.Delay(_options.DelayBetweenCommands);
                }
            }
        }

        private async Task tranceivePacketAsync()
        {
            var request = await _cache.DequeueAsync();

            if (request.ExpectsAnswer)
            {
                var response = await _transceiver.TransceiveAsync(request);
                await _cache.EnqueueAsync(request, response);
            }
            else
            {
                _transceiver.Transmit(request);
            }
        }

        public void Dispose()
        {
            _cache.Dispose();
            _transceiver.Dispose();
        }
    }
}
