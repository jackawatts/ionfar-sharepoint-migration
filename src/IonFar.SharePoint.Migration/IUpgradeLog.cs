namespace IonFar.SharePoint.Migration
{
    /// <summary>
    /// Interface for logging
    /// </summary>
    /// <remarks>
    /// <para>
    /// If passed in as a method parameter it can be used directly, however
    /// if passed to the constructor of a provider, it should be passed as
    /// Func&lt;IUpgradeLog&gt; to allow lazy instantiation.
    /// </para>
    /// </remarks>
    public interface IUpgradeLog
    {
        /// <summary>
        /// Writes a critical error message to the log.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        void Critical(string format, params object[] args);

        /// <summary>
        /// Writes an error message to the log.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        void Error(string format, params object[] args);

        /// <summary>
        /// Writes an informational message to the log.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        void Information(string format, params object[] args);

        /// <summary>
        /// Writes a verbose message to the log.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        void Verbose(string format, params object[] args);

        /// <summary>
        /// Writes a warning message to the log.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="args">The args.</param>
        void Warning(string format, params object[] args);

        ///// <summary>
        ///// Writes directly to the log (no line break).
        ///// </summary>
        ///// <param name="format">The format.</param>
        ///// <param name="args">The args.</param>
        //void Write(string format, params object[] args);
    }
}
