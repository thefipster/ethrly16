using System;
using System.Threading.Tasks;

namespace Toc.EthRly.Core.Interface
{
    public interface ITransceiver : IDisposable
    {
        void Connect();
        void Disconnect();
        bool IsConnected { get; }
        void Transmit(Packet packet);
        Task<Packet> TransceiveAsync(Packet packet);
    }
}
