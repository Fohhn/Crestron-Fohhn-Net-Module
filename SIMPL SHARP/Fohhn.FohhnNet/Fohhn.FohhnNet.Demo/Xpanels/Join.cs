namespace Fohhn.FohhnNet.Demo.Xpanels
{
    public static class Join
    {
        public const uint BConnected = 10;
        public const uint BConnect = 11;
        public const uint BDisconnect = 12;

        public const uint SIp = 10;
        public const uint SDeviceAlias = 11;
        public const uint SFirmwareVersion = 12;

        public static class Device
        {
            public const uint BPage = 20;

            public const uint BPowerOn = 21;
            public const uint BPowerOff = 22;
            
            public const uint BPreset3 = 25;
            public const uint BPreset4 = 26;
            public const uint BPreset5 = 27;
            public const uint BPreset6 = 28;

            public const uint BStatusBit1 = 30;
            public const uint BStatusBit2 = 31;
            public const uint BStatusBit3 = 32;
            public const uint BStatusBit4 = 33;

            public const uint UTemperature = 20;
        }

        public static class Output
        {
            public const uint Offset = 30;
            public const uint BPage = 50;

            public const uint UVolume = 50;

            public const uint BVolumeUp = 51;
            public const uint BVolumeDown = 52;
            public const uint BVolumeMute = 53;

            public const uint BInput1Gain = 60;
            public const uint BInput1GainUp = 60;
            public const uint BInput1GainDown = 61;
            public const uint BInput1Mute = 62;

            public const uint BInput2Gain = 65;
            public const uint BInput2GainUp = 65;
            public const uint BInput2GainDown = 66;
            public const uint BInput2Mute = 67;

            public const uint BInput3Gain = 70;
            public const uint BInput3GainUp = 70;
            public const uint BInput3GainDown = 71;
            public const uint BInput3Mute = 72;

            public const uint BInput4Gain = 75;
            public const uint BInput4GainUp = 75;
            public const uint BInput4GainDown = 76;
            public const uint BInput4Mute = 77;
        }
    }
}