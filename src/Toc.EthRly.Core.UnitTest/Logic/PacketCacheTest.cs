using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Toc.EthRly.Core.Exceptions;
using Toc.EthRly.Core.Models;
using Xunit;

namespace Toc.EthRly.Core.UnitTest.Logic
{
    public class PacketCacheTest
    {
        [Fact]
        public async Task EnqueueAndDequeueRequestPacketTest()
        {
            var options = new Options("irrelevant")
            {
                CommandQueueLength = 1,
                Timeout = TimeSpan.FromMilliseconds(100)
            };

            var cache = new PacketCache(options);
            var expectedPacket = new Packet();

            cache.Enqueue(expectedPacket);
            var actualPacket = await cache.DequeueAsync();

            Assert.NotNull(actualPacket);
            Assert.Equal(expectedPacket.Id, actualPacket.Id);
        }

        [Fact]
        public async Task EnqueueAndDequeueResponsePacketTest()
        {
            var options = new Options("irrelevant")
            {
                CommandQueueLength = 1,
                Timeout = TimeSpan.FromMilliseconds(100)
            };

            var cache = new PacketCache(options);
            var expectedRequest = new Packet();
            var expectedResponse = Packet.CreateResponseFor(expectedRequest);

            await cache.EnqueueAsync(expectedRequest, expectedResponse);
            var actualResponse = await cache.DequeueAsync(expectedRequest);

            Assert.NotNull(actualResponse);
            Assert.Equal(expectedResponse.Id, actualResponse.Id);
        }

        [Fact]
        public async Task EnqueueAndDequeueResponseNotMatchingPacketTest()
        {
            var options = new Options("irrelevant")
            {
                CommandQueueLength = 1,
                Timeout = TimeSpan.FromMilliseconds(100)
            };

            var cache = new PacketCache(options);
            var wrongRequest = new Packet();
            var wrongResponse = Packet.CreateResponseFor(wrongRequest);
            var request = new Packet();

            await cache.EnqueueAsync(wrongRequest, wrongResponse);

            await Assert.ThrowsAsync<TimeoutException>(async () => await cache.DequeueAsync(request));
        }

        [Fact]
        public void OverfillRequestQueueTest()
        {
            var options = new Options("irrelevant")
            {
                CommandQueueLength = 1,
                Timeout = TimeSpan.FromMilliseconds(100)
            };

            var cache = new PacketCache(options);
            var expectedPacket = new Packet();

            cache.Enqueue(expectedPacket);

            Assert.Throws<QueueIsFullException>(() => cache.Enqueue(expectedPacket));
        }

        [Fact]
        public async Task DequeueFromEmptyRequestQueueThrowsExceptionTest()
        {
            var options = new Options("irrelevant")
            {
                CommandQueueLength = 1,
                Timeout = TimeSpan.FromMilliseconds(100)
            };

            var cache = new PacketCache(options);

            await Assert.ThrowsAsync<QueueIsEmptyException>(async () => await cache.DequeueAsync());
        }

        [Fact]
        public async Task DequeueFromEmptyResponseQueueThrowsExceptionTest()
        {
            var options = new Options("irrelevant")
            {
                CommandQueueLength = 1,
                Timeout = TimeSpan.FromMilliseconds(100)
            };

            var cache = new PacketCache(options);
            var request = new Packet();

            await Assert.ThrowsAsync<TimeoutException>(async () => await cache.DequeueAsync(request));
        }

        [Fact]
        public async Task DequeueFromEmptyResponseQueueTimeoutDurationTest()
        {
            var timeout = TimeSpan.FromMilliseconds(1000);
            var options = new Options("irrelevant")
            {
                CommandQueueLength = 1,
                Timeout = timeout
            };

            var cache = new PacketCache(options);
            var request = new Packet();
            var watch = new Stopwatch();

            try
            {
                watch.Start();
                await cache.DequeueAsync(request);
            }
            catch (TimeoutException)
            {
                watch.Stop();
            }
            catch (Exception)
            {
                throw;
            }

            Assert.True(watch.ElapsedMilliseconds > timeout.TotalMilliseconds);
        }
    }
}
