using System;
using System.Linq;

namespace Fohhn.FohhnNet
{
    /// <summary>
    /// Contains information on what property has changed on FohhnNetDevice
    /// </summary>
    public class FohhnNetDeviceEventArgs : EventArgs
    {
        /// <summary>
        /// Contains possible properties that can change
        /// </summary>
        public enum FohhnNetDeviceEventTypes
        {
            /// <summary>See BoolValue</summary>
            Responding,
            /// <summary>See BoolValue</summary>
            Power,
            /// <summary>See StringValue</summary>
            FirmwareVersion,
            /// <summary>See DoubleValue</summary>
            Temperature,
            /// <summary>See BoolArrayValue</summary>
            StatusBits,
            /// <summary>See StringValue</summary>
            DeviceAlias
        }
        /// <summary>
        /// Contains which property has changed
        /// </summary>
        public FohhnNetDeviceEventTypes EventType { get; private set; }
        /// <summary>
        /// Contains the new value for EventTypes: Responding, Power
        /// </summary>
        public bool BoolValue { get; private set; }
        /// <summary>
        /// Contains the new value for EventTypes: FirmwareVersion, DeviceAlias
        /// </summary>
        public string StringValue { get; private set; }
        /// <summary>
        /// Contains the new value for EventTypes: Temperature
        /// </summary>
        public double DoubleValue { get; private set; }
        /// <summary>
        /// Contains the new value for EventTypes: StatusBits
        /// </summary>
        public bool[] BoolArrayValue { get; private set; }

        /// <summary>
        /// Contains information on what property has changed on FohhnNetDevice
        /// </summary>
        public FohhnNetDeviceEventArgs(FohhnNetDeviceEventTypes eventType, bool boolValue)
        {
            EventType = eventType;
            BoolValue = boolValue;
        }
        /// <summary>
        /// Contains information on what property has changed on FohhnNetDevice
        /// </summary>
        public FohhnNetDeviceEventArgs(FohhnNetDeviceEventTypes eventType, string stringValue)
        {
            EventType = eventType;
            StringValue = stringValue;
        }
        /// <summary>
        /// Contains information on what property has changed on FohhnNetDevice
        /// </summary>
        public FohhnNetDeviceEventArgs(FohhnNetDeviceEventTypes eventType, double doubleValue)
        {
            EventType = eventType;
            DoubleValue = doubleValue;
        }
        /// <summary>
        /// Contains information on what property has changed on FohhnNetDevice
        /// </summary>
        public FohhnNetDeviceEventArgs(FohhnNetDeviceEventTypes eventType, bool[] boolArrayValue)
        {
            EventType = eventType;
            BoolArrayValue = boolArrayValue;
        }

        /// <summary>
        /// Returns "ClassType - EventType: eventType, Value: value"
        /// </summary>
        public override string ToString()
        {
            switch (EventType)
            {
                case FohhnNetDeviceEventTypes.Power:
                case FohhnNetDeviceEventTypes.Responding:
                    return String.Format("{0} - EventType: {1}, BoolValue: {2}", GetType(), EventType, BoolValue);
                case FohhnNetDeviceEventTypes.FirmwareVersion:
                case FohhnNetDeviceEventTypes.DeviceAlias:
                    return String.Format("{0} - EventType: {1}, StringValue: {2}", GetType(), EventType, StringValue);
                case FohhnNetDeviceEventTypes.Temperature:
                    return String.Format("{0} - EventType: {1}, DoubleValue: {2}", GetType(), EventType, DoubleValue);
                case FohhnNetDeviceEventTypes.StatusBits:
                    return String.Format("{0} - EventType: {1}, ByteArrayValue: {2}", GetType(), EventType, String.Join(",",BoolArrayValue.Select(x => x.ToString()).ToArray()));
                default:
                    return String.Format("{0} - EventType: {1}", GetType(), EventType);
            }
        }
    }
}