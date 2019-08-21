using System;

namespace Toc.EthRly.Core.Exceptions
{
    public class QueueIsFullException : Exception
    {
        public QueueIsFullException(string queueName) 
            : base($"{queueName} is full.") { }
    }
}
