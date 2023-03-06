using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Crestron.SimplSharp;
using Fohhn.FohhnNet.Logging;

namespace Fohhn.FohhnNet.Transports
{
    internal static class TransportBroker
    {
        private static readonly Dictionary<string, TransportHandler> _udpTransports = new Dictionary<string, TransportHandler>();
        private static readonly Dictionary<string, List<object>> _transportUsedBy = new Dictionary<string, List<object>>();
        private static readonly CCriticalSection _transportLock = new CCriticalSection();

        public static TransportHandler GetSerialTransport(Action<string> sendCallback, object usedBy, Logger logger)
        {
            var transport = new FohhnNetSerialTransport(sendCallback);
            var handler = new TransportHandler(transport, logger);
            transport.Start();
            return handler;
        }
        public static TransportHandler GetUdpTransport(string ip, int port, object usedBy, Logger logger)
        {
            try
            {
                _transportLock.Enter();

                var identifier = GetUdpIdentifier(ip, port);
                TransportHandler transportHandler;
                if (!_udpTransports.TryGetValue(identifier, out transportHandler))
                {
                    var transport = new FohhnNetUdpTransport(ip, port, logger);
                    transportHandler = new TransportHandler(transport, logger);
                    _udpTransports.Add(identifier, transportHandler);
                    var userList = new List<object> { usedBy };
                    _transportUsedBy.Add(identifier, userList);
                    transport.Start();
                }
                else
                {
                    var userList = _transportUsedBy[identifier];
                    if (!userList.Contains(usedBy))
                        userList.Add(usedBy);
                }
                return transportHandler;
            }
            finally
            {
                _transportLock.Leave();
            }
        }
        public static void ReturnTransport(object usedBy)
        {
            try
            {
                _transportLock.Enter();
                var transportUsedByCopy = new Dictionary<string, List<object>>(_transportUsedBy);
                foreach (var kvp in transportUsedByCopy)
                {
                    if (kvp.Value.Contains(usedBy))
                    {
                        kvp.Value.Remove(usedBy);
                        if (kvp.Value.Count > 0)
                            return;

                        _udpTransports[kvp.Key].Dispose();
                        _udpTransports.Remove(kvp.Key);
                        _transportUsedBy.Remove(kvp.Key);
                    }
                }
            }
            finally
            {
                _transportLock.Leave();
            }
        }

        private static string GetUdpIdentifier(string ip, int port)
        {
            return String.Format("{0}:{1}", ip, port);
        }



    }
}