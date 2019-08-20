namespace Toc.EthRly.Core
{
    public enum Commands : byte
    {
        GetFirmwareVersion = 90,
        GetRelayStates = 91,
        SetRelayState = 92,
        GetInputVoltage = 93,
        AllRelaysOn = 100,
        RelayOn = 101,
        AllRelaysOff = 110,
        RelayOff = 111,
        GetMacAddress = 119
    }
}
