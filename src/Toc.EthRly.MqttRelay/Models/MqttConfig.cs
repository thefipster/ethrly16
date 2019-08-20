using System.Text;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace Toc.EthRly.MqttRelay.Models
{
    public class MqttConfig
    {
        public const string SectionName = "Mqtt";

        public string Hostname { get; set; }
        public int Port { get; set; }
        public string ClientId { get; set; }
        public string SetTemplate { get; set; }
        public string GetTemplate { get; set; }

        public Encoding Encoding => Encoding.UTF8;
        public byte Qos => MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE;
    }
}
