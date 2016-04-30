using System;
using System.Linq;
using System.Threading;

namespace Toc.RobotElectronics.EthRly16.ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            EthRly16Card relayCard = new EthRly16Card("192.168.1.22");

            var voltage = relayCard.GetInputVoltage();
            Console.WriteLine("Voltage: {0}V", voltage);
            var mac = relayCard.GetMacAddress();
            Console.WriteLine("MAC: {0}", BitConverter.ToString(mac));
            var version = relayCard.GetFirmwareVersion();
            Console.WriteLine("Firmware: V{0}", version);
            var statesStep = relayCard.GetRelaysStates();
            Console.WriteLine("Relay states: {0}", string.Join(" | ", statesStep.Select(x => x ? "ON" : "OFF")));

            for (int i = 1; i <= 8; i++)
            {
                relayCard.SetRelay(i, true);
                Thread.Sleep(150);

                statesStep = relayCard.GetRelaysStates();
                Console.WriteLine("Relay states: {0}", string.Join(" | ", statesStep.Select(x => x ? "ON" : "OFF")));
            }

            for (int i = 1; i <= 8; i++)
            {
                relayCard.SetRelay(i, false);
                Thread.Sleep(150);
            }

            relayCard.SetRelays(true);
            Thread.Sleep(250);

            relayCard.SetRelays(false);
            Thread.Sleep(250);

            Console.ReadKey();
        }
    }
}
