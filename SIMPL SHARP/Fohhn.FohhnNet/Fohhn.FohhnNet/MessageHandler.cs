using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp;
using Fohhn.FohhnNet.Logging;
using Fohhn.FohhnNet.Transports;
using Timeout = Crestron.SimplSharp.Timeout;

namespace Fohhn.FohhnNet
{
    class MessageHandler : IDisposable
    {
        public event EventHandler<FohhnNetDeviceEventArgs> RespondingChanged;
        private bool _responding;

        public bool Responding
        {
            get { return _responding; }
            internal set
            {
                if (_responding == value)
                    return;

                _responding = value;
                var ev = RespondingChanged;
                if (ev != null)
                    ev(this, new FohhnNetDeviceEventArgs(FohhnNetDeviceEventArgs.FohhnNetDeviceEventTypes.Responding, value));
            }
        }

        public int PollRateMs { get; set; }
        public List<Action> PollMethods { get; set; }

        private readonly byte _deviceId;
        private readonly Logger _logger;
        private TransportHandler _transportHandler;

        private readonly CTimer _pollTimer;

        private bool _pollActive;

        public MessageHandler(byte deviceId, Logger logger)
        {
            _deviceId = deviceId;
            _logger = logger;

            _pollTimer = new CTimer(PollTimerExpired, Timeout.Infinite);
            PollRateMs = 30000;
            PollMethods = new List<Action>();
        }

        public void SetTransportHandler(TransportHandler transportHandler)
        {
            if (_transportHandler == transportHandler)
                return;

            ClearTransportHandler();

            _transportHandler = transportHandler;
            transportHandler.SubscribeToResponses(_deviceId, this);
        }
        public void SendRequest(Request request)
        {
            if (_transportHandler != null)
                _transportHandler.SendRequest(request);
        }

        public void ClearTransportHandler()
        {
            StopPolling();
            if (_transportHandler != null)
            {
                _transportHandler.UnsubscribeFromResponses(_deviceId);
                _transportHandler = null;
            }
            ClearResponding();
        }

        public void ClearResponding()
        {
            Responding = false;
        }
        public void StartPolling()
        {
            _pollActive = true;
            _pollTimer.Reset(100);
        }
        public void StopPolling()
        {
            _pollActive = false;
            _pollTimer.Stop();
        }
        private void PollTimerExpired(object obj)
        {
            try
            {
                if (Responding)
                {
                    foreach (var action in PollMethods)
                        action();
                }
                else
                {
                    var action = PollMethods.FirstOrDefault();
                    if (action != null)
                        action();
                }

            }
            finally
            {
                if (_pollActive)
                    _pollTimer.Reset(PollRateMs);
            }
        }
        
        public void Dispose()
        {
            _pollActive = false;
            ClearTransportHandler();

            if (_pollTimer != null && !_pollTimer.Disposed)
                _pollTimer.Dispose();
        }
    }
}