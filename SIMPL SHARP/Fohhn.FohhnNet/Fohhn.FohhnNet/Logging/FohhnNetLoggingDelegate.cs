namespace Fohhn.FohhnNet.Logging
{
    /// <summary>
    /// A delegate that contains a method reference to your own logging writer
    /// </summary>
    /// <param name="level">At which level the message is written</param>
    /// <param name="message">The message</param>
    public delegate void FohhnNetLoggingDelegate(LogLevel level, string message);
}