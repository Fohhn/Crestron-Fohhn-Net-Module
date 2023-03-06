namespace Fohhn.FohhnNet.Logging
{
    /// <summary>
    /// Available logging levels that this library uses
    /// </summary>
    public enum LogLevel
    {
        /// <summary>This level of logging is very chatty and includes logging raw data, etc</summary>
        Debug = 0,
        /// <summary>This level contains informative messages</summary>
        Info = 1,
        /// <summary>This level contains real errors</summary>
        Error = 2
    }
}