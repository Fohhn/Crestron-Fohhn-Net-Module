using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace Fohhn.FohhnNet.Demo.Xpanels
{
    public class PanelBase
    {
        private readonly Dictionary<uint, Action<Sig>> _digitals = new Dictionary<uint, Action<Sig>>();
        private readonly Dictionary<uint, Action<Sig>> _analogs = new Dictionary<uint, Action<Sig>>();
        private readonly Dictionary<uint, Action<Sig>> _serials = new Dictionary<uint, Action<Sig>>();

        public BasicTriList Panel { get; private set; }

        public PanelBase(BasicTriList panel)
        {
            Panel = panel;
            Panel.SigChange += PanelOnSigChange;
        }

        public void Register()
        {
            var result = Panel.Register();
            if (result != eDeviceRegistrationUnRegistrationResponse.Success)
                ErrorLog.Warn("Could not register Panel with ipId {0:X2}: {1}", Panel.ID, Panel.RegistrationFailureReason);
        }

        public void OnDigital(uint join, Action<Sig> callback)
        {
            _digitals[join] = callback;
        }
        public void OnDigitalPress(uint join, Action<Sig> callback)
        {
            _digitals[join] = s =>
            {
                if (s.BoolValue) 
                    callback(s);
            };
        }
        public void OnAnalog(uint join, Action<Sig> callback)
        {
            _analogs[join] = callback;
        }
        public void OnSerial(uint join, Action<Sig> callback)
        {
            _serials[join] = callback;
        }

        public void SetDigital(uint join, bool value)
        {
            Panel.BooleanInput[join].BoolValue = value;
        }
        public void SetAnalog(uint join, ushort value)
        {
            Panel.UShortInput[join].UShortValue = value;
        }
        public void SetAnalog(uint join, short value)
        {
            Panel.UShortInput[join].ShortValue = value;
        }
        public void SetSerial(uint join, string value)
        {
            Panel.StringInput[join].StringValue = value;
        }

        // Event handlers
        private void PanelOnSigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            switch (args.Event)
            {
                case eSigEvent.BoolChange:
                    CallDigital(args);
                    break;
                case eSigEvent.UShortChange:
                    CallAnalog(args);
                    break;
                case eSigEvent.StringChange:
                    CallSerial(args);
                    break;
            }
        }
        private void CallDigital(SigEventArgs args)
        {
            Action<Sig> callback;
            if (_digitals.TryGetValue(args.Sig.Number, out callback))
                callback(args.Sig);
        }
        private void CallAnalog(SigEventArgs args)
        {
            Action<Sig> callback;
            if (_analogs.TryGetValue(args.Sig.Number, out callback))
                callback(args.Sig);
        }
        private void CallSerial(SigEventArgs args)
        {
            Action<Sig> callback;
            if (_serials.TryGetValue(args.Sig.Number, out callback))
                callback(args.Sig);
        }
    }
}