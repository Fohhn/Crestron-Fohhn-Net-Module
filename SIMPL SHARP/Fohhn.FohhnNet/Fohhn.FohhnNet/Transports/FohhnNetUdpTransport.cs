using System;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using Fohhn.FohhnNet.Logging;

namespace Fohhn.FohhnNet.Transports
{
    class FohhnNetUdpTransport : IFohhnNetTransport, IDisposable
    {
        public string Ip { get; private set; }
        public int Port { get; private set; }
        public Action<byte[]> IncomingDataCallback { get; set; }

        private readonly Logger _logger;
        private readonly UDPServer _client;
        private bool _disposed;

        public FohhnNetUdpTransport(string ip, int port, Logger logger)
        {
            Ip = ip;
            Port = port;
            _logger = logger;
            _client = new UDPServer();
        }
        
        public void Start()
        {
            var result = _client.EnableUDPServer(Ip, 0, Port);
            if (result == SocketErrorCodes.SOCKET_OK)
            {
                _logger.Debug(String.Format("UDP successfully enabled for: {0}:{1}", Ip, Port));
                result = _client.ReceiveDataAsync(OnDataReceived);
                if (result != SocketErrorCodes.SOCKET_OK && result != SocketErrorCodes.SOCKET_OPERATION_PENDING)
                    _logger.Error("Could not start listening for data: " + result);
            }
            else
            {
                _logger.Error("Could not enable UDP: " + result);
            }


        }

        public void Send(byte[] bytes)
        {
            if (_client.ServerStatus == SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                _logger.Debug(String.Format("Sending: {0}", bytes.GetBytesAsString()));
                _client.SendData(bytes, bytes.Length, Ip, Port);
            }

        }

        public void Dispose()
        {
            _disposed = true;
            _client.Dispose();
        }

        private void OnDataReceived(UDPServer client, int length)
        {
            if (length == 0)
                return;

            try
            {
                var bytes = new byte[length];
                Buffer.BlockCopy(client.IncomingDataBuffer, 0, bytes, 0, length);
                _logger.Debug(String.Format("Received: {0}", bytes.GetBytesAsString()));
                IncomingDataCallback(bytes);
            }
            catch (Exception ex)
            {
                _logger.Error("Exception in UdpTransport.OnDataReceived:\r\n" + ex);
            }
            if (!_disposed)
            {
                var result = client.ReceiveDataAsync(OnDataReceived);
                if (result != SocketErrorCodes.SOCKET_OK && result != SocketErrorCodes.SOCKET_OPERATION_PENDING)
                    _logger.Error("Could not start listening for data: " + result);
            }
        }
    }
}