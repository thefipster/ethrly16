using System;

namespace Toc.EthRly.MqttRelay.Exceptions
{
    public class InvalidCommandException : Exception
    {
        public InvalidCommandException(int command) 
            : base($"The command id '{command}' is not supported. Only 0 (off), 1 (on) and 2 (toggle) are allowed.") { }
    }
}
