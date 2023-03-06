namespace Fohhn.FohhnNet
{
    /// <summary>
    /// This class contains information about a Fohhn device that is necessary to set upp the FohhnNetDevice class
    /// </summary>
    public class FohhnNetDeviceConfig
    {
        /// <summary>
        /// The device id of the device to control
        /// </summary>
        public byte Id { get; set; }
        /// <summary>
        /// The number of inputs on the device
        /// </summary>
        public int Inputs { get; set; }
        /// <summary>
        /// The number of outputs on the device
        /// </summary>
        public int Outputs { get; set; }

        /// <summary>
        /// This class contains information about a Fohhn device that is necessary to set upp the FohhnNetDevice class
        /// </summary>
        /// <param name="deviceId">The device id of the device to control</param>
        /// <param name="inputs">The number of inputs on the device</param>
        /// <param name="outputs">The number of outputs on the device</param>
        public FohhnNetDeviceConfig(byte deviceId, int inputs, int outputs)
        {
            Id = (byte) deviceId;
            Inputs = inputs;
            Outputs = outputs;
        }
    }
}