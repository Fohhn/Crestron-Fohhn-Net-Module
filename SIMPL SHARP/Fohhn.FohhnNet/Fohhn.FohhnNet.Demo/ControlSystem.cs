using System;
using System.Text;
using Crestron.SimplSharp;                          	// For Basic SIMPL# Classes
using Crestron.SimplSharpPro;                       	// For Basic SIMPL#Pro classes
using Crestron.SimplSharpPro.CrestronThread; // For Threading
using Fohhn.FohhnNet.Demo.Xpanels;
using Fohhn.FohhnNet.Logging;
using Fohhn.FohhnNet.Outputs;
using Fohhn.FohhnNet.Routing;

namespace Fohhn.FohhnNet.Demo
{
    public class ControlSystem : CrestronControlSystem
    {
        public static ControlSystem Instance { get; private set; }
        private FohhnNetDevice _device;
        private Xpanel _xpanel;

        public ControlSystem()
        {
            try
            {
                Instance = this;
                Thread.MaxNumberOfUserThreads = 20;
                CrestronEnvironment.ProgramStatusEventHandler += ControlSystem_ControllerProgramEventHandler;
            }
            catch (Exception ex)
            {
                ErrorLog.Exception("Error in the constructor", ex);
            }
        }

        public override void InitializeSystem()
        {
            try
            {
                // Create a config
                var config = new FohhnNetDeviceConfig(1, 4, 4);

                // Instantiate the device
                _device = new FohhnNetDevice(config, null);

                // Create an Xpanel and pass the device
                _xpanel = new Xpanel(_device);

                // Register the Xpanel
                _xpanel.Register();

                // Create a console command for setting the LogLevel of the Fohhn library
                AddCommand("fohhnloglevel", "", cmd =>
                {
                    if (cmd == "")
                        CrestronConsole.ConsoleCommandResponse("Log level: " + _device.LogLevel);
                    else
                        _device.LogLevel = (LogLevel) Enum.Parse(typeof (LogLevel), cmd, true);
                });
            }
            catch (Exception ex)
            {
                ErrorLog.Exception("Error in InitializeSystem", ex);
            }
        }

        private void AddCommand(string command, string comment, SimplSharpProConsoleCmdFunction del)
        {
            CrestronConsole.AddNewConsoleCommand(del, command, comment, ConsoleAccessLevelEnum.AccessAdministrator);
        }

        private void ControlSystem_ControllerProgramEventHandler(eProgramStatusEventType programStatusEventType)
        {
            switch (programStatusEventType)
            {
                case (eProgramStatusEventType.Stopping):
                    // Dispose the device
                    _device.Dispose();
                    break;
            }
        }
    }
}