using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.UI;
using Fohhn.FohhnNet.Demo.Xpanels.Pages;

namespace Fohhn.FohhnNet.Demo.Xpanels
{
    public class Xpanel : PanelBase
    {
        private readonly FohhnNetDevice _device;
        private string _ip;

        private readonly List<Page> _pages = new List<Page>();
        private readonly DevicePage _devicePage;
        private readonly OutputPage _output1Page;
        private readonly OutputPage _output2Page;
        private readonly OutputPage _output3Page;
        private readonly OutputPage _output4Page;


        public Xpanel(FohhnNetDevice device) 
            : base(new XpanelForSmartGraphics(0x03, ControlSystem.Instance))
        {
            _device = device;

            OnSerial(Join.SIp, s => _ip = s.StringValue);
            OnDigitalPress(Join.BConnect, s => _device.StartCommunication(_ip));
            OnDigitalPress(Join.BDisconnect, s => _device.StopCommunication());

            _pages.Add(_devicePage = new DevicePage(this, device));
            _pages.Add(_output1Page = new OutputPage(this, 0, device));
            _pages.Add(_output2Page = new OutputPage(this, 1, device));
            _pages.Add(_output3Page = new OutputPage(this, 2, device));
            _pages.Add(_output4Page = new OutputPage(this, 3, device));

            OnDigitalPress(Join.Device.BPage, s => SetPage(_devicePage));
            OnDigitalPress(Join.Output.BPage + Join.Output.Offset * 0, s => SetPage(_output1Page));
            OnDigitalPress(Join.Output.BPage + Join.Output.Offset * 1, s => SetPage(_output2Page));
            OnDigitalPress(Join.Output.BPage + Join.Output.Offset * 2, s => SetPage(_output3Page));
            OnDigitalPress(Join.Output.BPage + Join.Output.Offset * 3, s => SetPage(_output4Page));

            _device.Events += DeviceOnEvents;
        }

        public void SetPage(Page page)
        {
            foreach (var p in _pages)
                p.Hide();

            page.Show();
        }

        private void DeviceOnEvents(object sender, FohhnNetDeviceEventArgs args)
        {
            switch (args.EventType)
            {
                case FohhnNetDeviceEventArgs.FohhnNetDeviceEventTypes.Responding:
                    SetDigital(Join.BConnected, args.BoolValue);
                    break;
                case FohhnNetDeviceEventArgs.FohhnNetDeviceEventTypes.FirmwareVersion:
                    SetSerial(Join.SFirmwareVersion, args.StringValue);
                    break;
                case FohhnNetDeviceEventArgs.FohhnNetDeviceEventTypes.DeviceAlias:
                    SetSerial(Join.SDeviceAlias, args.StringValue);
                    break;
            }
        }
    }
}