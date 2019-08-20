using System;

namespace Toc.EthRly.MqttRelay.Models
{
    public class EthrlyConfig
    {
        public const string SectionName = "Ethrly";

        public string Hostname { get; set; }
        public int Port { get; set; }
        public int SyncIntervalInSecs { get; set; }
        public int TimeoutInMs { get; set; }
        public int DelayBetweenCommandsInMs { get; set; }
        public int RelayCount { get; set; }
        public int CommandQueueLength { get; set; }

        public TimeSpan Timeout => TimeSpan.FromMilliseconds(TimeoutInMs);
        public TimeSpan SyncInterval => TimeSpan.FromSeconds(SyncIntervalInSecs);
        public TimeSpan DelayBetweenCommands => TimeSpan.FromMilliseconds(DelayBetweenCommandsInMs);
    }
}
