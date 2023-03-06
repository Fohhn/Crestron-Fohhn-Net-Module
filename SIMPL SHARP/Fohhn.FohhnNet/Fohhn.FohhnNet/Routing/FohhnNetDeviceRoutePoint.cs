using System;
using Fohhn.FohhnNet.Inputs;
using Fohhn.FohhnNet.Outputs;

namespace Fohhn.FohhnNet.Routing
{
    /// <summary>
    /// This class represents an output routing point on the device
    /// </summary>
    public class FohhnNetDeviceRoutePoint
    {
        /// <summary>
        /// This will be trigged when a property changes
        /// </summary>
        public event EventHandler<FohhnNetDeviceRoutePointEventArgs> Events;

        /// <summary>
        /// The input for this routing point
        /// </summary>
        public FohhnNetDeviceInput Input { get; private set; }
        /// <summary>
        /// The output for this routing point
        /// </summary>
        public FohhnNetDeviceOutput Output { get; private set; }

        private bool _isMuted;
        /// <summary>
        /// Returns true if the routing point is muted
        /// </summary>
        public bool IsMuted
        {
            get { return _isMuted; }
            private set
            {
                if (_isMuted == value)
                    return;
                _isMuted = value;
                TrigEvent(new FohhnNetDeviceRoutePointEventArgs(FohhnNetDeviceRoutePointEventArgs.FohhnNetDeviceRoutePointEventTypes.Muted, value));
            }
        }
        private double _gainDb;
        /// <summary>
        /// Returns the current gain of the routing point in dB (-80.0 to 12.0)
        /// </summary>
        public double GainDb
        {
            get { return _gainDb; }
            private set
            {
                if (Math.Abs(_gainDb - value) < .09)
                    return;
                _gainDb = value;
                TrigEvent(new FohhnNetDeviceRoutePointEventArgs(FohhnNetDeviceRoutePointEventArgs.FohhnNetDeviceRoutePointEventTypes.Gain, value));
            }
        }

        private readonly byte _deviceId;
        private readonly MessageHandler _messageHandler;

        internal FohhnNetDeviceRoutePoint(byte deviceId, FohhnNetDeviceOutput output, FohhnNetDeviceInput input, MessageHandler messageHandler)
        {
            _deviceId = deviceId;
            _messageHandler = messageHandler;
            Output = output;
            Input = input;
        }

        /// <summary>
        /// Sets gain and mute for this routing point
        /// </summary>
        /// <param name="gainDb">In decibels, -80.0 to 12.0</param>
        /// <param name="mute">Set true to mute this routing point</param>
        public void SetGainAndMute(double gainDb, bool mute)
        {
            var gainBytes = ((short)(gainDb * 10)).GetBytes();

            var request = new Request(
                _deviceId,
                0x81,
                new byte[] { (byte)Output.Number, (byte)Input.Number, gainBytes[0], gainBytes[1], (byte)(mute ? 0 : 1) },
                (req, res) =>
                {
                    if (res.IsAck)
                    {
                        IsMuted = mute;
                        GainDb = gainDb;
                    }
                }
            );

            _messageHandler.SendRequest(request);
        }

        /// <summary>
        /// This will poll this routing point for all gain and mute.
        /// This is done automatically only on connection.
        /// It requires a lot of commands to be sent so you have to manually do this if you need it.
        /// There are methods for this on each output, or on the main device as well.
        /// </summary>
        public void PollRoute()
        {
            var request = new Request(
                _deviceId,
                0x0A,
                new byte[] { (byte)Output.Number, (byte)Input.Number, 0x81 },
                (req, res) =>
                {
                    if (res.Data.Length == 3)
                    {
                        GainDb = res.Data.ToShort(0) / 10.0;
                        IsMuted = (res.Data[2] & 1) == 0;
                    }
                }
            );

            _messageHandler.SendRequest(request);
        }

        private void TrigEvent(FohhnNetDeviceRoutePointEventArgs args)
        {
            var ev = Events;
            if (ev != null)
                ev(this, args);
        }
    }
}