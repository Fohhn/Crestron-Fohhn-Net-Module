namespace Fohhn.FohhnNet.Inputs
{
    /// <summary>
    /// This class represents an input on the device
    /// </summary>
    public class FohhnNetDeviceInput
    {
        /// <summary>
        /// The 1-based number of the input
        /// </summary>
        public int Number { get; private set; }
        
        private readonly byte _deviceId;

        internal FohhnNetDeviceInput(int number, byte deviceId)
        {
            _deviceId = deviceId;
            Number = number;
        }
    }

}