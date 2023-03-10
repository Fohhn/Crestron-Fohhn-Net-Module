using System;

namespace Fohhn.FohhnNet.Routing
{
    /// <summary>
    /// Contains information on what property has changed on FohhnNetDeviceRoutePoint
    /// </summary>
    public class FohhnNetDeviceRoutePointEventArgs : EventArgs
    {
        /// <summary>
        /// Contains possible properties that can change
        /// </summary>
        public enum FohhnNetDeviceRoutePointEventTypes 
        {
            /// <summary>See DoubleValue</summary>
            Gain,
            /// <summary>See BoolValue</summary>
            Muted
        }
        /// <summary>
        /// Contains which property has changed
        /// </summary>
        public FohhnNetDeviceRoutePointEventTypes EventType { get; private set; }
        /// <summary>
        /// Contains the new value for EventTypes: Muted
        /// </summary>
        public bool BoolValue { get; private set; }
        /// <summary>
        /// Contains the new value for EventTypes: Gain
        /// </summary>
        public double DoubleValue { get; private set; }

        /// <summary>
        /// Contains information on what property has changed on FohhnNetDeviceRoutePoint
        /// </summary>
        public FohhnNetDeviceRoutePointEventArgs(FohhnNetDeviceRoutePointEventTypes eventType, bool boolValue)
        {
            EventType = eventType;
            BoolValue = boolValue;
        }
        /// <summary>
        /// Contains information on what property has changed on FohhnNetDeviceRoutePoint
        /// </summary>
        public FohhnNetDeviceRoutePointEventArgs(FohhnNetDeviceRoutePointEventTypes eventType, double doubleValue)
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
                case FohhnNetDeviceRoutePointEventTypes.Muted:
                    return String.Format("{0} - EventType: {1}, BoolValue: {2}", GetType(), EventType, BoolValue);
                case FohhnNetDeviceRoutePointEventTypes.Gain:
                    return String.Format("{0} - EventType: {1}, DoubleValue: {2}", GetType(), EventType, DoubleValue);
                default:
                    return String.Format("{0} - EventType: {1}", GetType(), EventType);
            }
        }
    }
}