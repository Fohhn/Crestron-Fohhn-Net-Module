using System;

namespace Fohhn.FohhnNet.Outputs
{
    /// <summary>
    /// Contains information on what property has changed on FohhnNetDeviceOutput
    /// </summary>
    public class FohhnNetDeviceOutputEventArgs : EventArgs
    {
        /// <summary>
        /// Contains possible properties that can change
        /// </summary>
        public enum FohhnNetDeviceOutputEventTypes
        {
            /// <summary>See DoubleValue</summary>
            Volume,
            /// <summary>See BoolValue</summary>
            Muted
        } 
        /// <summary>
        /// Contains which property has changed
        /// </summary>
        public FohhnNetDeviceOutputEventTypes EventType { get; private set; }
        /// <summary>
        /// Contains the new value for EventTypes: Muted
        /// </summary>
        public bool BoolValue { get; private set; }
        /// <summary>
        /// Contains the new value for EventTypes: Volume
        /// </summary>
        public double DoubleValue { get; private set; }

        /// <summary>
        /// Contains information on what property has changed on th FohhnNetDeviceOutput
        /// </summary>
        public FohhnNetDeviceOutputEventArgs(FohhnNetDeviceOutputEventTypes eventType, bool boolValue)
        {
            EventType = eventType;
            BoolValue = boolValue;
        }
        /// <summary>
        /// Contains information on what property has changed on th FohhnNetDeviceOutput
        /// </summary>
        public FohhnNetDeviceOutputEventArgs(FohhnNetDeviceOutputEventTypes eventType, double doubleValue)
        {
            EventType = eventType;
            DoubleValue = doubleValue;
        }
        /// <summary>
        /// Returns "ClassType - EventType: eventType, Value: value"
        /// </summary>
        public override string ToString()
        {
            switch (EventType)
            {
                case FohhnNetDeviceOutputEventTypes.Muted:
                    return String.Format("{0} - EventType: {1}, BoolValue: {2}", GetType(), EventType, BoolValue);
                case FohhnNetDeviceOutputEventTypes.Volume:
                    return String.Format("{0} - EventType: {1}, DoubleValue: {2}", GetType(), EventType, DoubleValue);
                default:
                    return String.Format("{0} - EventType: {1}", GetType(), EventType);
            }
        }
    }
}