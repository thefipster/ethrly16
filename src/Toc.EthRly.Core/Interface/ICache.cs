using System;
using System.Threading.Tasks;

namespace Toc.EthRly.Core.Interface
{
    public interface ICache : IDisposable
    {
        void Enqueue(Packet packet);
        Task<Packet> DequeueAsync();
        Task EnqueueAsync(Packet request, Packet response);
        Task<Packet> DequeueAsync(Packet request);
    }
}
