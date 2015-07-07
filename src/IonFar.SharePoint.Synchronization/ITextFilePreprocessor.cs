using IonFar.SharePoint.Migration;

namespace IonFar.SharePoint.Synchronization
{
    /// <summary>
    /// Preprocessor that modifies text files before they are uploaded.
    /// </summary>
    public interface ITextFilePreprocessor
    {

        /// <summary>
        /// Performs some preprocessing step on a script
        /// </summary>
        /// <param name="contextManager">Provides the current SharePoint context</param>
        /// <param name="logger">To log messages to the migrator</param>
        /// <param name="contents">String contents to process</param>
        string Process(IContextManager contextManager, IUpgradeLog logger, string contents);
    }
}
