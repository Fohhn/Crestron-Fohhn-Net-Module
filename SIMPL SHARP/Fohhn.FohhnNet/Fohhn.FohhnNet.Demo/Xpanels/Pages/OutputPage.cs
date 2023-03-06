using System;
using Fohhn.FohhnNet.Outputs;
using Fohhn.FohhnNet.Routing;

namespace Fohhn.FohhnNet.Demo.Xpanels.Pages
{
    public class OutputPage : Page
    {
        private readonly int _index;
        private readonly FohhnNetDeviceOutput _output;

        public OutputPage(PanelBase panel, int index, FohhnNetDevice device)
            : base(panel, (uint)(Join.Output.BPage + index * Join.Output.Offset))
        {
            _index = index;
            _output = device.Outputs[index];

            Panel.OnDigitalPress(GetJoin(Join.Output.BVolumeUp), s => SetVolume(_output.VolumeDb + 2));
            Panel.OnDigitalPress(GetJoin(Join.Output.BVolumeDown), s => SetVolume(_output.VolumeDb + -2));
            Panel.OnDigitalPress(GetJoin(Join.Output.BVolumeMute), s => _output.SetMute(!_output.IsMuted));

            _output.Events += OutputOnEvents;

            for (uint i = 0; i < 4; i++)
            {
                var routeIndex = i;
                var route = _output.InputRoutes[i];
                Panel.OnDigitalPress(GetJoin(Join.Output.BInput1GainUp) + i * 5, s => SetRouteVolume(route, route.GainDb + 2));
                Panel.OnDigitalPress(GetJoin(Join.Output.BInput1GainDown) + i * 5, s => SetRouteVolume(route, route.GainDb - 2));
                Panel.OnDigitalPress(GetJoin(Join.Output.BInput1Mute) + i * 5, s => SetRouteMute(route, !route.IsMuted));

                route.Events += (sender, args) =>
                {
                    switch (args.EventType)
                    {
                        case FohhnNetDeviceRoutePointEventArgs.FohhnNetDeviceRoutePointEventTypes.Gain:
                            Panel.SetAnalog(GetJoin(Join.Output.BInput1Gain) + routeIndex * 5, (short) (args.DoubleValue * 10));
                            break;
                        case FohhnNetDeviceRoutePointEventArgs.FohhnNetDeviceRoutePointEventTypes.Muted:
                            Panel.SetDigital(GetJoin(Join.Output.BInput1Mute) + routeIndex * 5, args.BoolValue);
                            break;
                    }
                };
            }
        }

        private void SetVolume(double volume)
        {
            _output.SetVolumeAndMute(GetSafeVolume(volume), _output.IsMuted);
        }
        private void SetRouteVolume(FohhnNetDeviceRoutePoint route, double volume)
        {
            route.SetGainAndMute(GetSafeVolume(volume), route.IsMuted);
        }
        private void SetRouteMute(FohhnNetDeviceRoutePoint route, bool mute)
        {
            route.SetGainAndMute(route.GainDb, mute);
        }


        private double GetSafeVolume(double volume)
        {
            if (volume < -80)
                return -80;
            if (volume > 12)
                return 12;
            return volume;
        }

        private uint GetJoin(uint join)
        {
            return (uint)(join + _index * Join.Output.Offset);
        }


        private void OutputOnEvents(object sender, FohhnNetDeviceOutputEventArgs args)
        {
            switch (args.EventType)
            {
                case FohhnNetDeviceOutputEventArgs.FohhnNetDeviceOutputEventTypes.Volume:
                    Panel.SetAnalog(GetJoin(Join.Output.UVolume), (short)(args.DoubleValue * 10));
                    break;
                case FohhnNetDeviceOutputEventArgs.FohhnNetDeviceOutputEventTypes.Muted:
                    Panel.SetDigital(GetJoin(Join.Output.BVolumeMute), args.BoolValue);
                    break;
            }
        }
    }
}