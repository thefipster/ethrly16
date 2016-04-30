# EthRly16
.Net Class Library to control the EthRly16 relay card made by Robot Electronics

More information at http://www.felixreisch.net/2016/04/30/robot-electronics-ethernet-relay-net-control-library/

## How to use it

```c#
EthRly16Card relayCard = new EthRly16Card("192.168.1.22");

double voltage = relayCard.GetInputVoltage();
byte[] mac = relayCard.GetMacAddress();
int version = relayCard.GetFirmwareVersion();
bool[] states = relayCard.GetRelaysStates();

relayCard.SetRelay(i, true);
relayCard.SetRelays(true);
