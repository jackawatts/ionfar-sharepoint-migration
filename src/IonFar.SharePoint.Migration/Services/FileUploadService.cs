using System;
using System.Linq;
using Microsoft.SharePoint.Client;
using System.Security.Cryptography;
using IonFar.SharePoint.Migration.Providers;

namespace IonFar.SharePoint.Migration.Services
{
    /// <summary>
    /// File and folder related provisioning functions.
    /// </summary>
    public class FileUploadService : IFileUploadService
    {
        private readonly ClientContext _clientContext;
        private readonly IUpgradeLog _logger;

        private readonly string _apiUrl = string.Empty;

        /// <summary>
        /// Creates a new file upload service
        /// </summary>
        /// <param name="clientContext">Context to use</param>
        /// <param name="logger">(Optional) logger to use; if not specified defaults to TraceSource</param>
        public FileUploadService(ClientContext clientContext, IUpgradeLog logger = null)
        {
            _clientContext = clientContext;
            _logger = logger ?? new TraceUpgradeLog();
        }

        /// <summary>
        /// Ensures the specified folder exists (in the context Web, usually the root), creating all folders in the path if necessary.
        /// </summary>
        /// <param name="folderPrefixedUrl">Server relative URL of the folder; may use '~sitecollection/' or '~site/' prefix.</param>
        /// <returns>The existing or newly created folder</returns>
        public Folder EnsureFolder(string folderPrefixedUrl)
        {
            return EnsureFolder(_clientContext.Web, folderPrefixedUrl);
        }

        /// <summary>
        /// Ensures the specified folder exists, creating all folders in the path if necessary.
        /// </summary>
        /// <param name="web">The web to create the folder within</param>
        /// <param name="folderPrefixedUrl">Server relative URL of the folder; may use '~sitecollection/' or '~site/' prefix.</param>
        /// <returns>The existing or newly created folder</returns>
        public Folder EnsureFolder(Web web, string folderPrefixedUrl)
        {
            if (web == null) { throw new ArgumentNullException("web"); }
            if (folderPrefixedUrl == null) { throw new ArgumentNullException("folderPrefixedUrl"); }

            var folderServerRelativeUrl = SPUrlUtility.ResolveServerRelativeUrl(_clientContext.Site, web, folderPrefixedUrl);

            if (!_clientContext.Web.IsPropertyAvailable("ServerRelativeUrl"))
            {
                _clientContext.Load(_clientContext.Web);
                _clientContext.ExecuteQuery();
            }

            if (!folderServerRelativeUrl.StartsWith(_clientContext.Web.ServerRelativeUrl))
            {
                var msg = string.Format("You should not create a folder above the current Web root (web root: {0}, folder: {1})",
                    _clientContext.Web.ServerRelativeUrl,
                    folderServerRelativeUrl);
                throw new Exception(msg);
            }

            var folder = _clientContext.Web.GetFolderByServerRelativeUrl(folderServerRelativeUrl);
            _clientContext.Load(folder);
            try
            {
                _clientContext.ExecuteQuery();
            }
            catch (ServerException)
            {
                var segments = folderServerRelativeUrl.Split(new[] { '/' }).ToList();
                var lastSegment = segments.Last();
                var parentFolderPath = string.Join("/", segments.Take(segments.Count() - 1));

                // Recurse
                var parentFolder = EnsureFolder(parentFolderPath);

                folder = parentFolder.Folders.Add(lastSegment);
                _clientContext.Load(folder);
                _clientContext.ExecuteQuery();
            }
            return folder;
        }

        /// <summary>
        /// Uploads a file to the context Web (usually root), replacing any existing file with a new version.
        /// </summary>
        /// <param name="sourcePath">Local path of the file to upload</param>
        /// <param name="folderPrefixedUrl">Server relative URL of the destination folder; may use '~sitecollection/' or '~site/' prefix; the folder must already exist</param>
        /// <param name="destinationName">Name to use for the uploaded file</param>
        /// <param name="checkout">true to check the file out before uploaded</param>
        /// <returns>The updated or newly created file</returns>
        public File UploadFile(string sourcePath, string folderPrefixedUrl, string destinationName, bool checkout)
        {
            return UploadFile(sourcePath, _clientContext.Web, folderPrefixedUrl, destinationName, checkout);
        }

        /// <summary>
        /// Uploads a file, replacing any existing file with a new version.
        /// </summary>
        /// <param name="sourcePath">Local path of the file to upload</param>
        /// <param name="web">Web the destination folder exists in</param>
        /// <param name="folderPrefixedUrl">Server relative URL of the destination folder; may use '~sitecollection/' or '~site/' prefix; the folder must already exist</param>
        /// <param name="destinationName">Name to use for the uploaded file</param>
        /// <param name="checkout">true to check the file out before uploaded</param>
        /// <returns>The updated or newly created file</returns>
        public File UploadFile(string sourcePath, Web web, string folderPrefixedUrl, string destinationName, bool checkout)
        {
            // TODO: Integrate with UploadFolder, i.e. don't duplicate code (and have same behaviour around publish, etc; for example checkout should be automatic if needed)
            // TODO: Add target level as a parameter

            _logger.Information("Uploading file: " + destinationName);

            var folderServerRelativeUrl = SPUrlUtility.ResolveServerRelativeUrl(_clientContext.Site, web, folderPrefixedUrl);

            var fileUrl = SPUrlUtility.Combine(folderServerRelativeUrl, destinationName);
            File existingFile = web.GetFileByServerRelativeUrl(fileUrl);
            _clientContext.Load(existingFile);
            try
            {
                _clientContext.ExecuteQuery();
            }
            catch (ServerException ex)
            {
                existingFile = null;
                if (ex.ServerErrorTypeName != "System.IO.FileNotFoundException")
                {
                    throw;
                }
            }

            if (existingFile != null)
            {
                if (checkout)
                {
                    existingFile.CheckOut();
                    _clientContext.ExecuteQuery();
                }
            }
            else
            {
                _logger.Information("- No existing file");
            }

            var folder = web.GetFolderByServerRelativeUrl(folderServerRelativeUrl);

            var fileCreate = new FileCreationInformation();
            fileCreate.Overwrite = true;
            fileCreate.Url = destinationName;

            File created;
            using (var sourceStream = System.IO.File.OpenRead(sourcePath))
            {
                fileCreate.ContentStream = sourceStream;
                created = folder.Files.Add(fileCreate);
                _clientContext.Load(created);
                _clientContext.ExecuteQuery();
            }

            if (created.CheckOutType != CheckOutType.None)
            {
                created.CheckIn("Published by migration", CheckinType.MajorCheckIn);
                _clientContext.ExecuteQuery();
            }
            else
            {
                created.Publish("Published by migration");
                _clientContext.ExecuteQuery();
            }

            return created;
        }

        /// <summary>
        /// Uploads all files in the specified local folder, after optionally checking a hash value for changes, to the destination folder in the context Web (usually root).
        /// </summary>
        /// <param name="sourcePath">Local folder to upload</param>
        /// <param name="destinationPrefixedUrl">Server relative URL of the destination folder; may use '~sitecollection/' or '~site/' prefix; the folder must already exist</param>
        /// <param name="hashProvider">Provider to use to check file hashes before uploading; if null then NullHashProvider is used, always uploading files</param>
        public void UploadFolder(string sourcePath, string destinationPrefixedUrl, IHashProvider hashProvider)
        {
            UploadFolder(sourcePath, _clientContext.Web, destinationPrefixedUrl, hashProvider);
        }

        /// <summary>
        /// Uploads all files in the specified local folder, after optionally checking a hash value for changes, to the destination folder.
        /// </summary>
        /// <param name="sourcePath">Local folder to upload</param>
        /// <param name="web">Web the destination folder exists in</param>
        /// <param name="destinationPrefixedUrl">Server relative URL of the destination folder; may use '~sitecollection/' or '~site/' prefix; the folder must already exist</param>
        /// <param name="hashProvider">Provider to use to check file hashes before uploading; if null then NullHashProvider is used, always uploading files</param>
        public void UploadFolder(string sourcePath, Web web, string destinationPrefixedUrl, IHashProvider hashProvider)
        {
            // TODO: Integrate with UploadFolder, i.e. don't duplicate code (and have same behaviour around publish, etc; for example checkout should be automatic if needed)
            // TODO: Add target level as a parameter
            // TODO: Consider adding recursive, but think about subfolder creation.

            if (sourcePath == null) { throw new ArgumentNullException("sourcePath"); }
            if (web == null) { throw new ArgumentNullException("web"); }
            if (destinationPrefixedUrl == null) { throw new ArgumentNullException("destinationPrefixedUrl"); }

            //if (recursive)
            //{
            //    throw new NotSupportedException("Recursive not supported yet");
            //}

            if (hashProvider == null)
            {
                hashProvider = new NullHashProvider();
            }

            _logger.Information("Uploading folder '{0}'", destinationPrefixedUrl);

            var destinationServerRelativeUrl = SPUrlUtility.ResolveServerRelativeUrl(_clientContext.Site, web, destinationPrefixedUrl);
            var destinationFolder = web.GetFolderByServerRelativeUrl(destinationServerRelativeUrl);

            var filePaths = System.IO.Directory.EnumerateFiles(sourcePath);
            foreach (var filePath in filePaths)
            {
				var fileName = System.IO.Path.GetFileName(filePath);
				var fileUrl = SPUrlUtility.Combine(destinationServerRelativeUrl, fileName);

                using (var localStream = System.IO.File.OpenRead(filePath))
                {
					byte[] serverHash = hashProvider.GetFileHash(fileUrl);
					bool contentsMatch = false;
					
                    //_logger.Information("Check hash: {0}", BitConverter.ToString(serverHash));

                    // Compare hash
                    HashAlgorithm ha = HashAlgorithm.Create();
                    var localHash = ha.ComputeHash(localStream);
                    localStream.Position = 0;
                    //_logger.Information("Local hash: {0}", BitConverter.ToString(localHash));
                    if (localHash.Length == serverHash.Length)
                    {
                        contentsMatch = true;
                        for (var index = 0; index < serverHash.Length; index++)
                        {
                            if (serverHash[index] != localHash[index])
                            {
                                //Console.WriteLine("Hash does not match");
                                contentsMatch = false;
                                break;
                            }
                        }
                    }

                    if (!contentsMatch)
                    {
                        // Checkout if required
                        var checkOutRequired = false;
                        var file = web.GetFileByServerRelativeUrl(fileUrl);
                        _clientContext.Load(file, f => f.CheckOutType);
                        try
                        {
                            _clientContext.ExecuteQuery();

                            var parentList = file.ListItemAllFields.ParentList;
                            _clientContext.Load(parentList, l => l.ForceCheckout);
                            try
                            {
                                _clientContext.ExecuteQuery();
                                if (parentList.ForceCheckout && file.CheckOutType == CheckOutType.None)
                                {
                                    checkOutRequired = true;
                                }
                            }
                            catch (ServerException ex)
                            {
                                if (ex.Message != "The object specified does not belong to a list.")
                                {
                                    throw;
                                }
                            }
                        }
                        catch (ServerException ex)
                        {
                            if (ex.Message != "File Not Found.")
                            {
                                throw;
                            }
                        }
                        if (checkOutRequired)
                        {
                            _logger.Information("Checking out file '{0}'", fileUrl);
                            file.CheckOut();
                            _clientContext.ExecuteQuery();
                        }

                        // Upload file
                        var newFileInfo = new FileCreationInformation()
                        {
                            ContentStream = localStream,
                            Url = fileName,
                            Overwrite = true
                        };
                        _logger.Information("{0} : updating ({1}..)", fileUrl, BitConverter.ToString(localHash).Substring(0, 12));
                        file = destinationFolder.Files.Add(newFileInfo);
                        _clientContext.Load(file);
                        _clientContext.ExecuteQuery();

                        // Check in and publish
                        var level = FileLevel.Published;
                        var publishingRequired = false;
                        var approvalRequired = false;
                        if (level == FileLevel.Draft || level == FileLevel.Published)
                        {
                            var context = file.Context;
                            var parentList = file.ListItemAllFields.ParentList;
                            context.Load(parentList,
                                        l => l.EnableMinorVersions,
                                        l => l.EnableModeration,
                                        l => l.ForceCheckout);
                            try
                            {
                                context.ExecuteQuery();
                                checkOutRequired = parentList.ForceCheckout;
                                publishingRequired = parentList.EnableMinorVersions; // minor versions implies that the file must be published
                                approvalRequired = parentList.EnableModeration;
                            }
                            catch (ServerException ex)
                            {
                                if (ex.Message != "The object specified does not belong to a list.")
                                {
                                    throw;
                                }
                            }
                            if (file.CheckOutType != CheckOutType.None || checkOutRequired)
                            {
                                _logger.Information("Checking in file '{0}'", file.Name);
                                file.CheckIn("Checked in by provisioning", publishingRequired ? CheckinType.MinorCheckIn : CheckinType.MajorCheckIn);
                                context.ExecuteQuery();
                            }
                            if (level == FileLevel.Published)
                            {
                                if (publishingRequired)
                                {
                                    _logger.Information("Publishing file '{0}'", file.Name);
                                    file.Publish("Published by provisioning");
                                    context.ExecuteQuery();
                                }

                                if (approvalRequired)
                                {
                                    _logger.Information("Approving file '{0}'", file.Name);
                                    file.Approve("Approved by provisioning");
                                    context.ExecuteQuery();
                                }
                            }
                        }

                        hashProvider.StoreFileHash(fileUrl, localHash);
                    }
                    else
                    {
                        _logger.Information("{0} : no change ({1}..).", fileUrl, BitConverter.ToString(serverHash).Substring(0, 12));
                    }
                }

            }
        }

    }
}
