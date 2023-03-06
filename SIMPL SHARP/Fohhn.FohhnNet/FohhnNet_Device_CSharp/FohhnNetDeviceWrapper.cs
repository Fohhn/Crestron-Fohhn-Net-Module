using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Fohhn.FohhnNet;
using Fohhn.FohhnNet.Logging;
using Fohhn.FohhnNet.Outputs;
using Fohhn.FohhnNet.Routing;

namespace FohhnNet_Device_CSharp
{
    public delegate void EmptyDel();
    public delegate void StringDel(SimplSharpString data);
    public delegate void UShortDel(ushort value);
    public delegate void ShortWithIndexDel(ushort outputIndex, short value);
    public delegate void UShortWithIndexDel(ushort outputIndex, ushort value);
    public delegate void ShortWithIndexesDel(ushort outputIndex, ushort inputIndex, short value);
    public delegate void UShortWithIndexesDel(ushort outputIndex, ushort inputIndex, ushort value);

    public class FohhnNetDeviceWrapper
    {
        public StringDel SendSerial { get; set; }
        public UShortDel RespondingChanged { get; set; }
        public UShortDel PowerChanged { get; set; }
        public StringDel FirmwareVersionChanged { get; set; }
        public UShortDel TemperatureChanged { get; set; }
        public EmptyDel StatusBitsChanged { get; set; }
        public StringDel DeviceAliasChanged { get; set; }
        public ShortWithIndexDel OutputVolumeChanged { get; set; }
        public UShortWithIndexDel OutputMuteChanged { get; set; }
        public ShortWithIndexesDel OutputRouteGainChanged { get; set; }
        public UShortWithIndexesDel OutputRouteMuteChanged { get; set; }
        public StringDel CustomCommandResponse { get; set; }

        private FohhnNetDevice _device;
        private readonly Encoding _encoding = Encoding.GetEncoding("latin1");

        public ushort[] StatusBits { get; private set; }

        public FohhnNetDeviceWrapper()
        {
            CrestronEnvironment.ProgramStatusEventHandler += type =>
            {
                if (type == eProgramStatusEventType.Stopping)
                {
                    if (_device != null)
                        _device.Dispose();
                }
            };
        }

        public void InitUdp(ushort deviceId, ushort inputs, ushort outputs, ushort pollRate)
        {
            var config = new FohhnNetDeviceConfig((byte)deviceId, inputs, outputs);
            Init(new FohhnNetDevice(config, null), pollRate);
        }
        public void InitSerial(ushort deviceId, ushort inputs, ushort outputs, ushort pollRate)
        {
            var config = new FohhnNetDeviceConfig((byte)deviceId, inputs, outputs);
            Init(new FohhnNetDevice(config, data => SendSerial(data), null), pollRate);
        }
        private void Init(FohhnNetDevice device, ushort pollRate)
        {
            _device = device;
            _device.PollRateMs = pollRate * 10;
            _device.Events += DeviceOnEvents;
            foreach (var output in _device.Outputs)
            {
                output.Events += OutputOnEvents;
                foreach (var route in output.InputRoutes)
                    route.Events += RouteOnEvents;
            }

        }
        
        public void HandleSerialResponse(string data)
        {
            _device.HandleSerialResponse(data);
        }

        public void StartCommunication(string ip, ushort port)
        {
            _device.StartCommunication(ip, port);
        }
        public void StopCommunication()
        {
            _device.StopCommunication();
        }

        public void SetPower(ushort value)
        {
            _device.SetPower(value == 1);
        }

        public void RecallPreset(ushort number)
        {
            _device.RecallPreset(number);
        }

        public void SetOutputVolume(ushort outputIndex, ushort volumeDb)
        {
            if (outputIndex < _device.Outputs.Length)
                _device.Outputs[outputIndex].SetVolumeAndMute(volumeDb / 10.0, _device.Outputs[outputIndex].IsMuted);
        }
        public void SetOutputMute(ushort outputIndex, ushort mute)
        {
            if (outputIndex < _device.Outputs.Length)
                _device.Outputs[outputIndex].SetMute(mute == 1);
        }
        public void SetRouteGain(ushort outputIndex, ushort inputIndex, ushort volumeDb)
        {
            var input = GetRouteInput(outputIndex, inputIndex);
            if (input == null)
                return;
            
            input.SetGainAndMute(volumeDb / 10.0, input.IsMuted);
        }
        public void SetRouteMute(ushort outputIndex, ushort inputIndex, ushort mute)
        {
            var input = GetRouteInput(outputIndex, inputIndex);
            if (input == null)
                return;

            input.SetGainAndMute(input.GainDb, mute == 1);
        }
        private FohhnNetDeviceRoutePoint GetRouteInput(ushort outputIndex, ushort inputIndex)
        {
            if (outputIndex >= _device.Outputs.Length)
                return null;
            var output = _device.Outputs[outputIndex];
            if (inputIndex >= output.InputRoutes.Length)
                return null;

            return output.InputRoutes[inputIndex];
        }

        public void SetLogLevel(ushort level)
        {
            _device.LogLevel = (LogLevel) level;
        }

        public void PollRouting()
        {
            _device.PollRouting();
        }

        public void SendCustomCommand(string command)
        {
            _device.SendCustomCommand(_encoding.GetBytes(command), bytes =>
            {
                if (CustomCommandResponse != null)
                    CustomCommandResponse(new string(bytes.Select(b => (char) b).ToArray()));
            });
        }

        private void DeviceOnEvents(object sender, FohhnNetDeviceEventArgs args)
        {
            switch (args.EventType)
            {
                case FohhnNetDeviceEventArgs.FohhnNetDeviceEventTypes.Responding:
                    if (RespondingChanged != null)
                        RespondingChanged(GetUShort(args.BoolValue));
                    break;
                case FohhnNetDeviceEventArgs.FohhnNetDeviceEventTypes.Power:
                    if (PowerChanged != null)
                        PowerChanged(GetUShort(args.BoolValue));
                    break;
                case FohhnNetDeviceEventArgs.FohhnNetDeviceEventTypes.FirmwareVersion:
                    if (FirmwareVersionChanged != null)
                        FirmwareVersionChanged(args.StringValue);
                    break;
                case FohhnNetDeviceEventArgs.FohhnNetDeviceEventTypes.Temperature:
                    if (TemperatureChanged != null)
                        TemperatureChanged((ushort)(args.DoubleValue * 10));
                    break;
                case FohhnNetDeviceEventArgs.FohhnNetDeviceEventTypes.StatusBits:
                    StatusBits = GetUShorts(args.BoolArrayValue);
                    if (StatusBitsChanged != null)
                        StatusBitsChanged();
                    break;
                case FohhnNetDeviceEventArgs.FohhnNetDeviceEventTypes.DeviceAlias:
                    if (DeviceAliasChanged != null)
                        DeviceAliasChanged(args.StringValue);
                    break;
            }
        }
        private void OutputOnEvents(object sender, FohhnNetDeviceOutputEventArgs args)
        {
            var output = (FohhnNetDeviceOutput)sender;
            var outputIndex = (ushort)(output.Number - 1);
            switch (args.EventType)
            {
                case FohhnNetDeviceOutputEventArgs.FohhnNetDeviceOutputEventTypes.Volume:
                    if (OutputVolumeChanged != null)
                        OutputVolumeChanged(outputIndex, (short)(args.DoubleValue * 10));
                    break;
                case FohhnNetDeviceOutputEventArgs.FohhnNetDeviceOutputEventTypes.Muted:
                    if (OutputVolumeChanged != null)
                        OutputMuteChanged(outputIndex, GetUShort(args.BoolValue));
                    break;
            }
        }
        private void RouteOnEvents(object sender, FohhnNetDeviceRoutePointEventArgs args)
        {
            var route = (FohhnNetDeviceRoutePoint)sender;
            var outputIndex = (ushort)(route.Output.Number - 1);
            var inputIndex = (ushort)(route.Input.Number - 1);
            switch (args.EventType)
            {
                case FohhnNetDeviceRoutePointEventArgs.FohhnNetDeviceRoutePointEventTypes.Gain:
                    if (OutputRouteGainChanged != null)
                        OutputRouteGainChanged(outputIndex, inputIndex, (short)(args.DoubleValue * 10));
                    break;
                case FohhnNetDeviceRoutePointEventArgs.FohhnNetDeviceRoutePointEventTypes.Muted:
                    if (OutputRouteMuteChanged != null)
                        OutputRouteMuteChanged(outputIndex, inputIndex, GetUShort(args.BoolValue));
                    break;
            }
        }

        private ushort GetUShort(bool value)
        {
            return (ushort) (value ? 1 : 0);
        }
        private ushort[] GetUShorts(IEnumerable<bool> values)
        {
            return values.Select(x => GetUShort(x)).ToArray();
        }
    }
}