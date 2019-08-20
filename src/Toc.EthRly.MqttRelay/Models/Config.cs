namespace Toc.EthRly.MqttRelay.Models
{
    public class Config
    {
        public Config()
        {
            Ethrly = new EthrlyConfig();
            Mqtt = new MqttConfig();
        }

        public EthrlyConfig Ethrly { get; set; }
        public MqttConfig Mqtt { get; set; }
    }
}
