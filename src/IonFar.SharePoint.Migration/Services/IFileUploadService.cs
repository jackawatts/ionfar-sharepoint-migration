using Microsoft.SharePoint.Client;

namespace IonFar.SharePoint.Migration.Services
{
    /// <summary>
    /// File and folder related provisioning functions.
    /// </summary>
    public interface IFileUploadService
    {
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
        /// Uploads a file to the context Web (usually root), replacing any existing file with a new version.
        /// </summary>
        /// <param name="sourcePath">Local path of the file to upload</param>
        /// <param name="folderPrefixedUrl">Server relative URL of the destination folder; may use '~sitecollection/' or '~site/' prefix; the folder must already exist</param>
        /// <param name="destinationName">Name to use for the uploaded file</param>
        /// <param name="checkout">true to check the file out before uploaded</param>
        /// <returns>The updated or newly created file</returns>
        File UploadFile(string sourcePath, string folderPrefixedUrl, string destinationName, bool checkout);

        /// <summary>
        /// Uploads a file, replacing any existing file with a new version.
        /// </summary>
        /// <param name="sourcePath">Local path of the file to upload</param>
        /// <param name="web">Web the destination folder exists in</param>
        /// <param name="folderPrefixedUrl">Server relative URL of the destination folder; may use '~sitecollection/' or '~site/' prefix; the folder must already exist</param>
        /// <param name="destinationName">Name to use for the uploaded file</param>
        /// <param name="checkout">true to check the file out before uploaded</param>
        /// <returns>The updated or newly created file</returns>
        File UploadFile(string sourcePath, Web web, string folderPrefixedUrl, string destinationName, bool checkout);

        /// <summary>
        /// Uploads all files in the specified local folder, after optionally checking a hash value for changes, to the destination folder in the context Web (usually root).
        /// </summary>
        /// <param name="sourcePath">Local folder to upload</param>
        /// <param name="destinationPrefixedUrl">Server relative URL of the destination folder; may use '~sitecollection/' or '~site/' prefix; the folder must already exist</param>
        /// <param name="hashProvider">Provider to use to check file hashes before uploading; if null then NullHashProvider is used, always uploading files</param>
        void UploadFolder(string sourcePath, string destinationPrefixedUrl, IHashProvider hashProvider);

        /// <summary>
        /// Uploads all files in the specified local folder, after optionally checking a hash value for changes, to the destination folder.
        /// </summary>
        /// <param name="sourcePath">Local folder to upload</param>
        /// <param name="web">Web the destination folder exists in</param>
        /// <param name="destinationPrefixedUrl">Server relative URL of the destination folder; may use '~sitecollection/' or '~site/' prefix; the folder must already exist</param>
        /// <param name="hashProvider">Provider to use to check file hashes before uploading; if null then NullHashProvider is used, always uploading files</param>
        void UploadFolder(string sourcePath, Web web, string destinationPrefixedUrl, IHashProvider hashProvider);
    }
}