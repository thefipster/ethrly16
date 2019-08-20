using System;

namespace Toc.EthRly.Core.Exceptions
{
    public class QueueIsEmptyException : Exception
    {
        public QueueIsEmptyException(string queueName) 
            : base($"{queueName} is empty.") { }
    }
}
