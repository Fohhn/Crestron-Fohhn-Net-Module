using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Fohhn.FohhnNet.Logging;

namespace Fohhn.FohhnNet.Transports
{
    internal class TransportHandler : IDisposable
    {
        private const int _responseTimeout = 350;
        private const int _requestQueueSize = 100;

        private readonly Queue<Request> _requestQueue = new Queue<Request>();
        private readonly CCriticalSection _requestQueueLock = new CCriticalSection();
        private Request _lastRequest;

        private readonly CTimer _responseTimer;
        
        private readonly IFohhnNetTransport _transport;
        private readonly List<byte> _buffer = new List<byte>();
        private readonly CCriticalSection _sendLock = new CCriticalSection();
        private bool _sending;
        
        private readonly Logger _logger;

        private readonly Dictionary<byte, MessageHandler> _messageHandlers = new Dictionary<byte, MessageHandler>();
        private readonly CCriticalSection _messageHandlerLock = new CCriticalSection();
        private readonly Dictionary<byte, int> _missedResponses = new Dictionary<byte, int>();
        private readonly CCriticalSection _missedResponsesLock = new CCriticalSection();

        public TransportHandler(IFohhnNetTransport transport, Logger logger)
        {
            _logger = logger;
            _transport = transport;
            _transport.IncomingDataCallback = HandleIncomingData;

            _responseTimer = new CTimer(ResponseTimerExpired, Timeout.Infinite);
        }

        public void SubscribeToResponses(byte deviceId, MessageHandler messageHandler)
        {
            try
            {
                _messageHandlerLock.Enter();
                _messageHandlers[deviceId] = messageHandler;
            }
            finally
            {
                _messageHandlerLock.Leave();
            }
        }
        public void UnsubscribeFromResponses(byte deviceId)
        {
            try
            {
                _messageHandlerLock.Enter();
                _messageHandlers.Remove(deviceId);
            }
            finally
            {
                _messageHandlerLock.Leave();
            }
        }

        public void SendRequest(Request request)
        {
            try
            {
                _requestQueueLock.Enter();
                if (_requestQueue.Count > _requestQueueSize)
                    return;

                _requestQueue.Enqueue(request);
            }
            finally
            {
                _requestQueueLock.Leave();
                SendNext(false);
            }
        }

        private void SendNext(bool previousComplete)
        {
            try
            {
                _sendLock.Enter();
                if (!previousComplete && _sending)
                    return;

                Request request = null;
                try
                {
                    _requestQueueLock.Enter();
                    if (_requestQueue.Count > 0)
                        request = _requestQueue.Dequeue();
                }
                finally
                {
                    _requestQueueLock.Leave();
                }

                _lastRequest = request;
                if (request != null)
                {
                    _sending = true;
                    _responseTimer.Reset(_responseTimeout);
                    _transport.Send(_lastRequest.GetBytes());
                    return;
                }
                _sending = false;
            }
            finally
            {
                _sendLock.Leave();
            }
        }

        public void HandleIncomingData(byte[] data)
        {
            _buffer.AddRange(data);
            int delimiterIndex;
            while ((delimiterIndex = _buffer.IndexOf(0xF0)) >= 0)
            {
                var responseBytes = new byte[delimiterIndex + 1];
                _buffer.CopyTo(0, responseBytes, 0, delimiterIndex + 1);
                _buffer.RemoveRange(0, delimiterIndex + 1);
                var response = new Response(responseBytes);
                if (response.IsValid)
                    HandleResponse(response);
                else
                {
                    _logger.Info("Received invalid response: " + responseBytes.GetBytesAsString());
                }
            }

            if (_buffer.Count > 1024)
                _buffer.Clear();
        }
        private void HandleResponse(Response response)
        {
            _logger.Debug(String.Format("Got response for DeviceId: {0} with Data: {1}", response.DeviceId, response.Data.GetBytesAsString()));
            try
            {
                _responseTimer.Stop();
                var responding = false;
                MessageHandler messageHandler;
                try
                {
                    _messageHandlerLock.Enter();
                    _messageHandlers.TryGetValue(response.DeviceId, out messageHandler);
                }
                finally
                {
                    _messageHandlerLock.Leave();
                }
                if (messageHandler != null)
                {
                    try
                    {
                        _missedResponsesLock.Enter();
                        _missedResponses[response.DeviceId] = 0;
                    }
                    finally
                    {
                        _missedResponsesLock.Leave();
                    }
                    responding = true;
                }

                if (_lastRequest != null && 
                    _lastRequest.DeviceId == response.DeviceId && 
                    _lastRequest.ResponseCallback != null)
                    _lastRequest.ResponseCallback(_lastRequest, response);

                _lastRequest = null;
                if (responding && messageHandler != null)
                    messageHandler.Responding = true;

            }
            catch (Exception ex)
            {
                _logger.Error("Exception when handling response\r\n" + ex);
            }
            finally
            {
                _lastRequest = null;
                SendNext(true);
            }
        }

        private void ResponseTimerExpired(object obj)
        {
            if (_lastRequest == null)
            {
                _logger.Info(String.Format("Device did not respond within {0}ms", _responseTimeout));
            }
            else
            {
                int missedResponses;
                try
                {
                    _missedResponsesLock.Enter();
                    _missedResponses.TryGetValue(_lastRequest.DeviceId, out missedResponses);
                    _missedResponses[_lastRequest.DeviceId] = ++missedResponses;
                }
                finally
                {
                    _missedResponsesLock.Leave();
                }

                _logger.Info(String.Format("Device did not respond within {0}ms for command {1}. Number of missed responses: {2}", _responseTimeout, _lastRequest.GetBytes().GetBytesAsString(), missedResponses));
                
                if (missedResponses >= 3)
                {
                    MessageHandler messageHandler;
                    try
                    {
                        _messageHandlerLock.Enter();
                        _messageHandlers.TryGetValue(_lastRequest.DeviceId, out messageHandler);
                    }
                    finally
                    {
                        _messageHandlerLock.Leave();
                    }
                    if (messageHandler != null)
                        messageHandler.Responding = false;
                }
                _lastRequest = null;
            }

            SendNext(true);
        }

        public void Dispose()
        {
            _requestQueue.Clear();
            if (_responseTimer != null && !_responseTimer.Disposed)
                _responseTimer.Dispose();

            var d = _transport as IDisposable;
            if (d != null)
                d.Dispose();
        }
    }
}