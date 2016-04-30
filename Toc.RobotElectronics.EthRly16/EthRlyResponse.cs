namespace Toc.RobotElectronics.EthRly16
{
    public class EthRlyResponse
    {
        /// <summary>
        /// The initial command of the request
        /// </summary>
        public byte Command { get; set; }

        /// <summary>
        /// The returned data from the relay card.
        /// </summary>
        public byte[] Payload { get; set; }
    }
}
