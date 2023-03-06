using System;
using System.Linq;
using System.Text;

namespace Fohhn.FohhnNet.Transports
{
    internal class FohhnNetSerialTransport : IFohhnNetTransport, IDisposable
    {
        public Action<byte[]> IncomingDataCallback { get; set; }

        private readonly Action<string> _sendCommand;
        private bool _started;
        
        public FohhnNetSerialTransport(Action<string> sendCommand)
        {
            _sendCommand = sendCommand;
        }

        public void Start()
        {
            _started = true;
        }

        public void Send(byte[] bytes)
        {
            if (_started)
                _sendCommand(bytes.GetString());
        }
        public void Dispose()
        {
            _started = false;
        }
    }
}