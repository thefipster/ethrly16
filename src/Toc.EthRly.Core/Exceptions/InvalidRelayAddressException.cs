using System;

namespace Toc.EthRly.Core.Exceptions
{
    public class InvalidRelayAddressException : Exception
    {
        public InvalidRelayAddressException(int relayAddress) 
            : base($"{relayAddress} is not a valid relay address.") { }

        public InvalidRelayAddressException(string relayAddress) 
            : base($"{relayAddress} is not a valid relay address.") { }
    }
}
