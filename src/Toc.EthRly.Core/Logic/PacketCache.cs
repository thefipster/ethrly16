using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;
using Toc.EthRly.Core.Exceptions;
using Toc.EthRly.Core.Models;

namespace Toc.EthRly.Core
{
    public class PacketCache : IDisposable
    {
        private readonly Options _options;

        private readonly ConcurrentQueue<Packet> _requests;
        private readonly ConcurrentDictionary<Packet, Packet> _responses;

        public PacketCache(Options options)
        {
            _options = options;
            _requests = new ConcurrentQueue<Packet>();
            _responses = new ConcurrentDictionary<Packet, Packet>();
        }

        /// <summary>
        /// Enqueues requests to the relay board.
        /// </summary>
        public void Enqueue(Packet packet)
        {
            if (_requests.Count >= _options.CommandQueueLength)
                throw new QueueIsFullException("RequestQueue");

            _requests.Enqueue(packet);
        }

        /// <summary>
        /// Dequeues requests to the relay board.
        /// </summary>
        public async Task<Packet> DequeueAsync()
        {
            if (_requests.IsEmpty)
                throw new QueueIsEmptyException("RequestQueue");

            var watch = new Stopwatch();
            watch.Start();

            while (true)
            {
                Packet request;
                if (_requests.TryDequeue(out request))
                    return request;

                if (watch.ElapsedMilliseconds > _options.Timeout.TotalMilliseconds)
                    throw new TimeoutException("Request dequeueing timed out.");

                await Task.Delay(_options.DelayBetweenCommands);
            }
        }

        /// <summary>
        /// Enqueues responses from the relay board.
        /// </summary>
        public async Task EnqueueAsync(Packet request, Packet response)
        {
            var watch = new Stopwatch();
            watch.Start();

            while (_responses.TryAdd(request, response))
            {
                if (watch.ElapsedMilliseconds > _options.Timeout.TotalMilliseconds)
                    throw new TimeoutException("Response enqueueing timed out.");

                await Task.Delay(_options.DelayBetweenCommands);
            }
        }

        /// <summary>
        /// Dequeues responses from the relay board.
        /// </summary>
        public async Task<Packet> DequeueAsync(Packet request)
        {
            var watch = new Stopwatch();
            watch.Start();

            while (true)
            {
                if (_responses.TryGetValue(request, out var answer))
                    return answer;

                if (watch.ElapsedMilliseconds > _options.Timeout.TotalMilliseconds)
                    throw new TimeoutException("Response dequeueing timed out.");

                await Task.Delay(_options.DelayBetweenCommands);
            }
        }

        public void Dispose()
        {
            _requests.Clear();
            _responses.Clear();
        }
    }
}
