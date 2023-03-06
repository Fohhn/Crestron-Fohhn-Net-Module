using System;

namespace Fohhn.FohhnNet.Demo.Xpanels.Pages
{
    public class DevicePage : Page
    {
        private readonly FohhnNetDevice _device;

        public DevicePage(PanelBase panel, FohhnNetDevice device) 
            : base(panel, Join.Device.BPage)
        {
            _device = device;

            Panel.OnDigitalPress(Join.Device.BPowerOn, s => _device.SetPower(true));
            Panel.OnDigitalPress(Join.Device.BPowerOff, s => _device.SetPower(false));

            Panel.OnDigitalPress(Join.Device.BPreset3, s => _device.RecallPreset(3));
            Panel.OnDigitalPress(Join.Device.BPreset4, s => _device.RecallPreset(4));
            Panel.OnDigitalPress(Join.Device.BPreset5, s => _device.RecallPreset(5));
            Panel.OnDigitalPress(Join.Device.BPreset6, s => _device.RecallPreset(6));

            _device.Events += DeviceOnEvents;
        }

        private void DeviceOnEvents(object sender, FohhnNetDeviceEventArgs args)
        {
            switch (args.EventType)
            {
                case FohhnNetDeviceEventArgs.FohhnNetDeviceEventTypes.Power:
                    Panel.SetDigital(Join.Device.BPowerOn, args.BoolValue);
                    Panel.SetDigital(Join.Device.BPowerOff, !args.BoolValue);
                    break;
                case FohhnNetDeviceEventArgs.FohhnNetDeviceEventTypes.Temperature:
                    Panel.SetAnalog(Join.Device.UTemperature, (ushort)(args.DoubleValue * 10));
                    break;
                case FohhnNetDeviceEventArgs.FohhnNetDeviceEventTypes.StatusBits:
                    Panel.SetDigital(Join.Device.BStatusBit1, args.BoolArrayValue[0]);
                    Panel.SetDigital(Join.Device.BStatusBit2, args.BoolArrayValue[1]);
                    Panel.SetDigital(Join.Device.BStatusBit3, args.BoolArrayValue[2]);
                    Panel.SetDigital(Join.Device.BStatusBit4, args.BoolArrayValue[3]);
                    break;
            }
        }
    }
}