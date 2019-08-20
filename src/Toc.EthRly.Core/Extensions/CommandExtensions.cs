namespace Toc.EthRly.Core.Extensions
{
    public static class CommandExtensions
    {
        public static byte AsByte(this Commands command)
        {
            return (byte)command;
        }
    }
}
