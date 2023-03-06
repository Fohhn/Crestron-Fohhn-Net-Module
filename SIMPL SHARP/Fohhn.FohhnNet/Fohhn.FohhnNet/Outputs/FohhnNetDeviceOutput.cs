using System;
using Fohhn.FohhnNet.Inputs;
using Fohhn.FohhnNet.Routing;

namespace Fohhn.FohhnNet.Outputs
{
    /// <summary>
    /// This class represents an output on the device
    /// </summary>
    public class FohhnNetDeviceOutput
    {
        /// <summary>
        /// This will be trigged when a property changes
        /// </summary>
        public event EventHandler<FohhnNetDeviceOutputEventArgs> Events;

        /// <summary>
        /// The 1-based number of this output
        /// </summary>
        public int Number { get; private set; }

        /// <summary>
        /// Contains every input that you can route to this output. This is where you perform routing operations
        /// </summary>
        public FohhnNetDeviceRoutePoint[] InputRoutes { get; private set; }

        private readonly byte _deviceId;
        private readonly MessageHandler _messageHandler;

        private bool _isMuted;
        /// <summary>
        /// Returns true if the output is muted
        /// </summary>
        public bool IsMuted
        {
            get { return _isMuted; }
            private set
            {
                if (_isMuted == value)
                    return;
                _isMuted = value;
                TrigEvent(new FohhnNetDeviceOutputEventArgs(FohhnNetDeviceOutputEventArgs.FohhnNetDeviceOutputEventTypes.Muted, value));
            }
        }
        private double _volumeDb;
        /// <summary>
        /// Returns the current volume of the output in dB (-80.0 to 12.0)
        /// </summary>
        public double VolumeDb
        {
            get { return _volumeDb; }
            private set
            {
                if (Math.Abs(_volumeDb - value) < .09)
                    return;
                _volumeDb = value;
                TrigEvent(new FohhnNetDeviceOutputEventArgs(FohhnNetDeviceOutputEventArgs.FohhnNetDeviceOutputEventTypes.Volume, value));
            }
        }

        internal FohhnNetDeviceOutput(int number, byte deviceId, FohhnNetDeviceInput[] inputs, MessageHandler messageHandler)
        {
            Number = number;
            _deviceId = deviceId;
            _messageHandler = messageHandler;

            InputRoutes = new FohhnNetDeviceRoutePoint[inputs.Length];
            for (int i = 0; i < inputs.Length; i++)
                InputRoutes[i] = new FohhnNetDeviceRoutePoint(deviceId, this, inputs[i], messageHandler);
        }

        /// <summary>
        /// Sets volume and mute for this output
        /// </summary>
        /// <param name="volumeDb">In decibels, -80.0 to 12.0</param>
        /// <param name="mute">Set true to mute this output</param>
        public void SetVolumeAndMute(double volumeDb, bool mute)
        {
            var volumeBytes = ((short) (volumeDb*10)).GetBytes();

            var request = new Request(
                _deviceId,
                0x87,
                new byte[] { (byte)Number, 1, volumeBytes[0], volumeBytes[1], (byte)(mute ? 0 : 1) },
                (req, res) =>
                {
                    if (res.IsAck)
                    {
                        IsMuted = mute;
                        VolumeDb = volumeDb;
                    }
                });

            _messageHandler.SendRequest(request);
        }
        /// <summary>
        /// Sets the mute state for this output
        /// </summary>
        /// <param name="mute">Set true to mute this output</param>
        public void SetMute(bool mute)
        {
            var request = new Request(
                _deviceId,
                0x96,
                new byte[] { (byte)Number, 1, 0, 0, (byte)(mute ? 0 : 5) },
                (req, res) =>
                {
                    if (res.IsAck)
                        IsMuted = mute;
                });

            _messageHandler.SendRequest(request);
        }
        private void PollVolumeAndMute()
        {
            var request = new Request(
                _deviceId,
                0x0A,
                new byte[] { (byte)Number, 1, 0x87 },
                (req, res) =>
                {
                    if (res.Data.Length == 3)
                    {
                        VolumeDb = res.Data.ToShort(0) / 10.0;
                        IsMuted = (res.Data[2] & 1) == 0;
                    }
                });

            _messageHandler.SendRequest(request);
        }
        
        internal void Poll()
        {
            PollVolumeAndMute();
        }

        /// <summary>
        /// This will poll this output for all routing information.
        /// This is done automatically only on connection.
        /// It requires a lot of commands to be sent so you have to manually do this if you need it.
        /// There are methods for this on each routing point, or on the main device as well.
        /// </summary>
        public void PollRouting()
        {
            foreach (var route in InputRoutes)
                route.PollRoute();
        }

        private void TrigEvent(FohhnNetDeviceOutputEventArgs args)
        {
            var ev = Events;
            if (ev != null)
                ev(this, args);
        }
    }
}