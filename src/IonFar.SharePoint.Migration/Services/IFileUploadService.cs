using Microsoft.SharePoint.Client;
using System.Collections.Generic;

namespace IonFar.SharePoint.Migration.Services
{
    /// <summary>
    /// File and folder related provisioning functions.
    /// </summary>
    public interface IFileUploadService
    {
        /// <summary>
        /// Gets or sets the Provider top use to check file hashes before uploading; use NullHashProvider to always uploading files</param>
        /// </summary>
        IHashProvider HashProvider { get; set; }

        /// <summary>
        /// 
        /// </summary>
        IList<ITextFilePreprocessor> Preprocessors { get; }

        /// <summary>
        /// Ensures the specified folder exists (in the context Web, usually the root), creating all folders in the path if necessary.
        /// </summary>
        /// <param name="folderPrefixedUrl">Server relative URL of the folder; may use '~sitecollection/' or '~site/' prefix.</param>
        /// <returns>The existing or newly created folder</returns>
        Folder EnsureFolder(string folderPrefixedUrl);

        /// <summary>
        /// Ensures the specified folder exists, creating all folders in the path if necessary.
        /// </summary>
        /// <param name="web">The web to create the folder within</param>
        /// <param name="folderPrefixedUrl">Server relative URL of the folder; may use '~sitecollection/' or '~site/' prefix.</param>
        /// <returns>The existing or newly created folder</returns>
        Folder EnsureFolder(Web web, string folderPrefixedUrl);

        /// <summary>
        /// Creates or updates a site collection ScriptLink reference to a script file, doing nothing if the ScriptLink already exists with the specified values
        /// </summary>
        /// <param name="name">Key to identify the ScriptLink</param>
        /// <param name="scriptPrefixedUrl">URL of the script; may use '~sitecollection/' or '~site/' prefix.</param>
        /// <param name="sequence">Determines the order the ScriptLink is rendered in</param>
        /// <returns>The UserCustomAction representing the ScriptLink</returns>
        UserCustomAction EnsureSiteScriptLink(string name, string scriptPrefixedUrl, int sequence);

        /// <summary>
        /// Uploads a file, replacing any existing file with a new version, publishing if necessary.
        /// </summary>
        /// <param name="sourcePath">Local path of the file to upload</param>
        /// <param name="web">Web the destination folder exists in</param>
        /// <param name="destinationPrefixedUrl">Server relative URL of the destination file; may use '~sitecollection/' or '~site/' prefix; the folder must already exist</param>
        /// <returns>The updated or newly created file</returns>
        File UploadFile(string sourcePath, Web web, string destinationPrefixedUrl);

        /// <summary>
        /// Uploads a file, replacing any existing file with a new version.
        /// </summary>
        /// <param name="sourcePath">Local path of the file to upload</param>
        /// <param name="web">Web the destination folder exists in</param>
        /// <param name="destinationPrefixedUrl">Server relative URL of the destination file; may use '~sitecollection/' or '~site/' prefix; the folder must already exist</param>
        /// <param name="publishingLevel">Target final state of the file, e.g. Published or Draft</param>
        /// <returns>The updated or newly created file</returns>
        File UploadFile(string sourcePath, Web web, string destinationPrefixedUrl, FileLevel publishingLevel);

        /// <summary>
        /// Uploads a file, replacing any existing file with a new version.
        /// </summary>
        /// <param name="stream">Contents of the file to upload</param>
        /// <param name="web">Web the destination folder exists in</param>
        /// <param name="destinationPrefixedUrl">Server relative URL of the destination file; may use '~sitecollection/' or '~site/' prefix; the folder must already exist</param>
        /// <param name="publishingLevel">Target final state of the file, e.g. Published or Draft</param>
        /// <returns>The updated or newly created file</returns>
        File UploadFile(System.IO.Stream stream, Web web, string destinationPrefixedUrl, FileLevel publishingLevel);

        /// <summary>
        /// Uploads all files in the specified local folder, after checking a hash value for changes, to the destination folder in the context Web (usually root).
        /// </summary>
        /// <param name="sourcePath">Local folder to upload</param>
        /// <param name="destinationPrefixedUrl">Server relative URL of the destination folder; may use '~sitecollection/' or '~site/' prefix; the folder must already exist</param>
        void UploadFolder(string sourcePath, string destinationPrefixedUrl);

        /// <summary>
        /// Uploads all files in the specified local folder, after checking a hash value for changes, to the destination folder.
        /// </summary>
        /// <param name="sourcePath">Local folder to upload</param>
        /// <param name="web">Web the destination folder exists in</param>
        /// <param name="destinationPrefixedUrl">Server relative URL of the destination folder; may use '~sitecollection/' or '~site/' prefix; the folder must already exist</param>
        /// <param name="publishingLevel">Target final state of the file, e.g. Published or Draft</param>
        void UploadFolder(string sourcePath, Web web, string destinationPrefixedUrl, FileLevel publishingLevel);
    }
}