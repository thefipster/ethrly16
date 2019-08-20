using System;
using System.Linq;
using System.Runtime.Loader;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Toc.EthRly.Core;
using Toc.EthRly.Core.Models;
using uPLibrary.Networking.M2Mqtt;

namespace Toc.EthRly.MqttRelay.Models
{
    public class ServiceState
    {
        private const string AppSettingsFile = "appSettings.json";
        private CancellationTokenSource _tokenSource;

        public ServiceState()
        {
            setupConfig();
            setupCancellation();
            setupEthrly();
            setupMqtt();
            RelayState = Enumerable.Repeat(false, Config.Ethrly.RelayCount).ToArray();
        }

        public EthRlyDevice Ethrly { get; private set; }

        public MqttClient Mqtt { get; private set; }

        public bool[] RelayState { get; set; }

        public CancellationToken Token => _tokenSource.Token;

        public Config Config { get; private set; }

        public void Exit() => _tokenSource.Cancel();

        private void setupConfig()
        {
            Config = new Config();
            var config = new ConfigurationBuilder()
                .AddJsonFile(AppSettingsFile, true, true)
                .Build();

            config.GetSection(EthrlyConfig.SectionName).Bind(Config.Ethrly);
            config.GetSection(MqttConfig.SectionName).Bind(Config.Mqtt);
        }

        private void setupCancellation()
        {
            _tokenSource = new CancellationTokenSource();
            AssemblyLoadContext.Default.Unloading += context => _tokenSource.Cancel();
        }

        private void setupEthrly()
        {
            var options = new Options(Config.Ethrly.Hostname, Config.Ethrly.Port)
            {
                Timeout = Config.Ethrly.Timeout,
                DelayBetweenCommands = Config.Ethrly.DelayBetweenCommands,
                CommandQueueLength = Config.Ethrly.CommandQueueLength,
                RelayCount = Config.Ethrly.RelayCount
            };

            Ethrly = new EthRlyDevice(options, Token);
        }

        private void setupMqtt()
        {
            Mqtt = new MqttClient(Config.Mqtt.Hostname);
            Mqtt.Connect(Config.Mqtt.ClientId, null, null, false, 60);
            Mqtt.ConnectionClosed += onMqttConnectionClosed;
        }

        private async void onMqttConnectionClosed(object sender, EventArgs e)
        {
            var retryCount = 0;
            while (!Mqtt.IsConnected && retryCount < 3)
            {
                try
                {
                    Mqtt.Connect(Config.Mqtt.ClientId, null, null, false, 60);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Reconnect failed: " + exception.Message);
                }

                await Task.Delay(TimeSpan.FromSeconds(1), Token);
            }
        }
    }
}
