using System;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronSockets;
using Fohhn.FohhnNet.Inputs;
using Fohhn.FohhnNet.Logging;
using Fohhn.FohhnNet.Outputs;
using Fohhn.FohhnNet.Transports;

namespace Fohhn.FohhnNet
{
    /// <summary>
    /// Class used to control any Fohhn device that supports FohhnNet over UDP or RS485.
    /// </summary>
    public class FohhnNetDevice : IDisposable
    {
        /// <summary>
        /// This will be trigged when a property changes
        /// </summary>
        public event EventHandler<FohhnNetDeviceEventArgs> Events;

        /// <summary>
        /// This is the Id of the device
        /// </summary>
        public byte DeviceId { get { return _config.Id; } }

        private readonly Logger _logger;
        /// <summary>
        /// Get or set the current log level. 
        /// Change this to output the desired level of logging.
        /// </summary>
        public LogLevel LogLevel
        {
            get { return _logger.Level; }
            set { _logger.Level = value; }
        }

        /// <summary>
        /// Contains all outputs on the device
        /// This is where you control things such as volume and mute
        /// </summary>
        public FohhnNetDeviceOutput[] Outputs { get; private set; }
        private FohhnNetDeviceInput[] _inputs;

        /// <summary>
        /// Returns true if the device is responding properly to commands. 
        /// </summary>
        public bool Responding { get { return _messageHandler.Responding; } }

        /// <summary>
        /// Get or set the Ip/FQDN/Hostname of the device to connect to over UDP
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// Get or set the remote Port to use for UDP
        /// </summary>
        public int Port { get; set; }

        private bool _powerIsOn;
        /// <summary>
        /// Returns false if the device is in standby
        /// </summary>
        public bool PowerIsOn
        {
            get
            {
                return _powerIsOn;
            }
            private set
            {
                if (_powerIsOn == value)
                    return;
                _powerIsOn = value;
                TrigEvent(new FohhnNetDeviceEventArgs(FohhnNetDeviceEventArgs.FohhnNetDeviceEventTypes.Power, value));
            }
        }

        private string _deviceAlias = String.Empty;
        /// <summary>
        /// Returns the alias of the device. If no alias is set, this will be an empty string
        /// </summary>
        public string DeviceAlias
        {
            get
            {
                return _deviceAlias;
            }
            private set
            {
                if (_deviceAlias == value)
                    return;
                _deviceAlias = value;
                TrigEvent(new FohhnNetDeviceEventArgs(FohhnNetDeviceEventArgs.FohhnNetDeviceEventTypes.DeviceAlias, value));
            }
        }

        private string _firmwareVersion;
        /// <summary>
        /// Returns the firmware version of the device. Example "3.2.2"
        /// </summary>
        public string FirmwareVersion
        {
            get
            {
                return _firmwareVersion;
            }
            private set
            {
                if (_firmwareVersion == value)
                    return;
                _firmwareVersion = value;
                TrigEvent(new FohhnNetDeviceEventArgs(FohhnNetDeviceEventArgs.FohhnNetDeviceEventTypes.FirmwareVersion, value));
            }
        }

        private double _temperature;
        /// <summary>
        /// Returns the temperature in Celsius of the device
        /// </summary>
        public double Temperature
        {
            get
            {
                return _temperature;
            }
            private set
            {
                if (Math.Abs(_temperature - value) < .09)
                    return;
                _temperature = value;
                TrigEvent(new FohhnNetDeviceEventArgs(FohhnNetDeviceEventArgs.FohhnNetDeviceEventTypes.Temperature, value));
            }
        }

        private readonly bool[] _statusBits = new bool[4];
        /// <summary>
        /// This contains some status about things. 
        /// This is different for every Fohhn device type. It contains 4 bits.
        /// 
        /// DLI-130/230/330/430     | 0: Fault,     | 1: Audio (AES), | 2: Pilotton     |                |
        /// FV-100/200              | 0: Fault,     | 1: Audio (AES)  |                 |                |
        /// LFI-120/220/350/450     | 0: Fault,     | 1: Piltoton     |                 |                |
        /// FMI-100/110/400         | 0: Fault,     | 1: Piltoton     |                 |                |
        /// DI-2.2000/4000          | 0: Protect 1, | 1: Protect 2    |                 |                |
        /// DI-4.1000/2000          | 0: Protect 1, | 1: Protect 2,   | 2: Protect 3,   | 3: Protect 4   | 
        /// DFM-100/110/400         | 0: Fault,     | 1: Audio (AES), | 2: Pilotton     |                | 
        /// MA-4.100/600            | 0: Protect 1, | 1: Protect 2,   | 2: Protect 3,   | 3: Protect 4   | 
        /// </summary>
        public bool[] StatusBits
        {
            get
            {
                return _statusBits;
            }
            private set
            {
                var changed = false;
                for (int i = 0; i < 4; i++)
                {
                    if (_statusBits[i] != value[i])
                    {
                        changed = true;
                        _statusBits[i] = value[i];
                    }
                }

                if (changed)
                    TrigEvent(new FohhnNetDeviceEventArgs(FohhnNetDeviceEventArgs.FohhnNetDeviceEventTypes.StatusBits, _statusBits));
            }
        }

        /// <summary>
        /// Get or set at which rate the device should be polled (in milliseconds). It's recommended to not go lower than 5000.
        /// Default is 30000
        /// </summary>
        public int PollRateMs
        {
            get { return _messageHandler.PollRateMs; }
            set { _messageHandler.PollRateMs = value; }
        }

        private MessageHandler _messageHandler;
        private readonly FohhnNetDeviceConfig _config;
        private bool _communicationStarted;
        private readonly bool _usingUdp;
        private readonly Action<string> _serialSendCallback;
        private readonly Encoding _encoding = Encoding.GetEncoding("latin1");
        private TransportHandler _transportHandler;

        /// <summary>
        /// Class used to control any Fohhn device that supports FohhnNet over UDP or RS485.
        /// This constructor uses UDP
        /// </summary>
        /// <param name="config">An object containing information about the device</param>
        /// <param name="logCallback">
        /// You may provide a your own method for logging. Set to null to use default.
        /// The default logger writes to errorlog if this is a server-based processor. Otherwise prints to console, and errorlog if loglevel is an error
        /// </param>
        public FohhnNetDevice(FohhnNetDeviceConfig config, FohhnNetLoggingDelegate logCallback)
            : this(config, String.Empty, 2101, logCallback) { }

        /// <summary>
        /// Class used to control any Fohhn device that supports FohhnNet over UDP or RS485.
        /// This constructor uses UDP
        /// </summary>
        /// <param name="config">An object containing information about the device</param>
        /// <param name="host">The Ip/FQDN/Hostname of the device to connect to over UDP</param>
        /// <param name="logCallback">
        /// You may provide a your own method for logging. Set to null to use default.
        /// The default logger writes to errorlog if this is a server-based processor. Otherwise prints to console, and errorlog if loglevel is an error
        /// </param>
        public FohhnNetDevice(FohhnNetDeviceConfig config, string host, FohhnNetLoggingDelegate logCallback)
            : this(config, host, 2101, logCallback) { }

        /// <summary>
        /// Class used to control any Fohhn device that supports FohhnNet over UDP or RS485.
        /// This constructor uses UDP
        /// </summary>
        /// <param name="config">An object containing information about the device</param>
        /// <param name="host">The Ip/FQDN/Hostname of the device to connect to over UDP</param>
        /// <param name="port">The remote port number to use for UDP</param>
        /// <param name="logCallback">
        /// You may provide a your own method for logging. Set to null to use default.
        /// The default logger writes to errorlog if this is a server-based processor. Otherwise prints to console, and errorlog if loglevel is an error
        /// </param>
        public FohhnNetDevice(FohhnNetDeviceConfig config, string host, int port, FohhnNetLoggingDelegate logCallback)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            _usingUdp = true;
            Host = host;
            Port = port;
            DeviceAlias = String.Empty;
            _config = config;
            _logger = new Logger(logCallback);

            InitMessageHandler(config.Id, _logger);
            InitConfig(config);
        }
        /// <summary>
        /// Class used to control any Fohhn device that supports FohhnNet over UDP or RS485.
        /// This constructor uses RS485. When you get a serial response from the device you have to call HandleSerialResponse(data) with the data.
        /// </summary>
        /// <param name="config">An object containing information about the device</param>
        /// <param name="sendCommand">Provide a method to use for sending commands to the serial port</param>
        /// <param name="logCallback">
        /// You may provide a your own method for logging. Set to null to use default.
        /// The default logger writes to errorlog if this is a server-based processor. Otherwise prints to console, and errorlog if loglevel is an error
        /// </param>
        public FohhnNetDevice(FohhnNetDeviceConfig config, Action<string> sendCommand, FohhnNetLoggingDelegate logCallback)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            DeviceAlias = String.Empty;
            _config = config;
            _logger = new Logger(logCallback);
            _serialSendCallback = sendCommand;

            InitMessageHandler(config.Id, _logger);
            InitConfig(config);
        }
        private void InitMessageHandler(byte deviceId, Logger logger)
        {
            _messageHandler = new MessageHandler(deviceId, logger);
            _messageHandler.RespondingChanged += (sender, args) =>
            {
                TrigEvent(args);
                if (args.EventType == FohhnNetDeviceEventArgs.FohhnNetDeviceEventTypes.Responding && args.BoolValue)
                    PollInitialValues();
            };
            _messageHandler.PollMethods.AddRange(new Action[]
            {
                PollPower,
                PollProtectAndTemperature,
                () => {
                    foreach (var o in Outputs)
                        o.Poll();
                },
            });
        }
        private void InitConfig(FohhnNetDeviceConfig config)
        {
            _inputs = new FohhnNetDeviceInput[config.Inputs];
            for (int i = 0; i < config.Inputs; i++)
                _inputs[i] = new FohhnNetDeviceInput(i + 1, DeviceId);

            Outputs = new FohhnNetDeviceOutput[config.Outputs];
            for (int i = 0; i < config.Outputs; i++)
                Outputs[i] = new FohhnNetDeviceOutput(i + 1, DeviceId, _inputs, _messageHandler);
        }

        private void PollInitialValues()
        {
            PollFirmwareVersion();
            PollAlias();
            PollRouting();
        }

        /// <summary>
        /// Starts communicating with the device.
        /// Use this if RS485 is used, or if you have supplied Host address in constructor or directly on the Host property.
        /// </summary>
        public void StartCommunication()
        {
            if (_communicationStarted)
                return;

            _communicationStarted = true;
            if (_usingUdp)
                _transportHandler = TransportBroker.GetUdpTransport(Host, Port, this, _logger);
            else
                _transportHandler = TransportBroker.GetSerialTransport(_serialSendCallback, this, _logger);
            
            _messageHandler.SetTransportHandler(_transportHandler);
            _messageHandler.StartPolling();
        }
        /// <summary>
        /// Starts communicating with the device using UDP on remote port 2101
        /// </summary>
        /// <param name="host">The Ip/FQDN/Hostname of the device to connect to over UDP</param>
        public void StartCommunication(string host)
        {
            StartCommunication(host, 2101);
        }
        /// <summary>
        /// Starts communicating with the device using UDP
        /// </summary>
        /// <param name="host">The Ip/FQDN/Hostname of the device to connect to over UDP</param>
        /// <param name="port">The remote port number to use for UDP</param>
        public void StartCommunication(string host, int port)
        {
            Host = host;
            Port = port;

            StartCommunication();
        }

        /// <summary>
        /// Stops communicating with the device
        /// </summary>
        public void StopCommunication()
        {
            _communicationStarted = false;
            TransportBroker.ReturnTransport(this);
            _transportHandler = null;
            _messageHandler.ClearTransportHandler();
        }

        /// <summary>
        /// Recalls a preset in the device
        /// </summary>
        /// <param name="number">Range 1-100</param>
        public void RecallPreset(int number)
        {
            var request = new Request(
                DeviceId,
                0x05,
                new byte[] { 1, (byte)number, 0 },
                null);

            _messageHandler.SendRequest(request);
        }

        /// <summary>
        /// Sets the standby state of the device
        /// </summary>
        /// <param name="powerOn">False sets the device in standby</param>
        public void SetPower(bool powerOn)
        {
            var request = new Request(
                DeviceId,
                0x0C,
                new byte[] {0, 0, (byte)(powerOn ? 0 : 1)},
                (req, res) =>
                {
                    if (res.IsAck)
                        PowerIsOn = powerOn;
                });

            _messageHandler.SendRequest(request);
        }
        private void PollPower()
        {
            var request = new Request(
                DeviceId,
                0x0A,
                new byte[] { 0, 0, 0x0C },
                (req, res) =>
                {
                    if (res.Data.Length == 1)
                        PowerIsOn = res.Data[0] == 0;
                });

            _messageHandler.SendRequest(request);
        }

        private void PollFirmwareVersion()
        {
            var request = new Request(
               DeviceId,
               0x20,
               new byte[] { 0, 0, 1 },
               (req, res) =>
               {
                   if (res.Data.Length == 5)
                       FirmwareVersion = String.Format("{0}.{1}.{2}", res.Data[2], res.Data[3], res.Data[4]);
               });

            _messageHandler.SendRequest(request);
        }
        private void PollAlias()
        {
            var request = new Request(
               DeviceId,
               0x90,
               new byte[] { 1, 0, 0 },
               (req, res) =>
               {
                   if (res.Data.Length == 20)
                       DeviceAlias = res.Data.GetString(2, 16).Trim('\0');
               });

            _messageHandler.SendRequest(request);
        }
        private void PollProtectAndTemperature()
        {
            var request = new Request(
               DeviceId,
               0x07,
               new byte[] { 0, 0, 0 },
               (req, res) =>
               {
                   if (res.Data.Length == 4)
                   {
                       StatusBits = res.Data[0].ToBits();
                       Temperature = res.Data.ToShort(1) / 10.0;
                   }
               });

            _messageHandler.SendRequest(request);
        }

        /// <summary>
        /// This will poll all outputs for all routing information.
        /// This is done automatically only on connection.
        /// It requires a lot of commands to be sent so you have to manually do this if you need it.
        /// There are methods for this on each output and routing point.
        /// </summary>
        public void PollRouting()
        {
            foreach (var output in Outputs)
                output.PollRouting();
        }

        /// <summary>
        /// Call this method when you get serial data from the device
        /// </summary>
        /// <param name="data">The data received from the device</param>
        public void HandleSerialResponse(string data)
        {
            if (_transportHandler != null)
                _transportHandler.HandleIncomingData(_encoding.GetBytes(data));    
        }

        /// <summary>
        /// Use this on program stop, or whenever you want to dispose the device
        /// </summary>
        public void Dispose()
        {
            _messageHandler.Dispose();
            _transportHandler = null;
            TransportBroker.ReturnTransport(this);
        }

        /// <summary>
        /// Method to send custom commands.
        /// Used if you want additional functionality that this library does not provide
        /// </summary>
        /// <param name="bytes">The bytes to send</param>
        /// <param name="responseCallback">Supply a method that will be called when/if we get a response</param>
        public void SendCustomCommand(byte[] bytes, Action<byte[]> responseCallback)
        {
            var request = new Request(bytes, (req, res) => responseCallback(res.Raw));
            _messageHandler.SendRequest(request);
        }

        private void TrigEvent(FohhnNetDeviceEventArgs args)
        {
            var ev = Events;
            if (ev != null)
                ev(this, args);
        }
    }
}