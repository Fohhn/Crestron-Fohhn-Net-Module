using System;

namespace Fohhn.FohhnNet.Transports
{
    interface IFohhnNetTransport
    {
        Action<byte[]> IncomingDataCallback { get; set; }
        void Send(byte[] bytes);
        void Start();
    }
}