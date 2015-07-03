using System;
using System.Linq;
using Microsoft.SharePoint.Client;
using System.Security.Cryptography;
using IonFar.SharePoint.Migration.Providers;
using System.Collections.Generic;

namespace IonFar.SharePoint.Migration.Services
{
    /// <summary>
    /// File and folder related provisioning functions.
    /// </summary>
    public class FileUploadService : IFileUploadService
    {
        private readonly ClientContext _clientContext;
        private IHashProvider hashProvider;
        private readonly IUpgradeLog _logger;
        private List<ITextFilePreprocessor> preprocessors;

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
            hashProvider = new NullHashProvider();
            preprocessors = new List<ITextFilePreprocessor>();
        }

        /// <summary>
        /// Gets or sets the Provider top use to check file hashes before uploading; use NullHashProvider to always uploading files</param>
        /// </summary>
        public IHashProvider HashProvider {
            get { return hashProvider; }
            set {
                hashProvider = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IList<ITextFilePreprocessor> Preprocessors {
            get { return preprocessors; }
        }

        /// <summary>
        /// Ensures the specified folder exists (in the context Web, usually the root), creating all folders in the path if necessary.
        /// </summary>
        /// <param name="folderPrefixedUrl">Server relative URL of the folder; may use '~sitecollection/' or '~site/' prefix.</param>
        /// <returns>The existing or newly created folder</returns>
        public Folder EnsureFolder(string folderPrefixedUrl)
        {
            return EnsureFolderInternal(_clientContext.Web, folderPrefixedUrl);
        }

        /// <summary>
        /// Ensures the specified folder exists, creating all folders in the path if necessary.
        /// </summary>
        /// <param name="web">The web to create the folder within</param>
        /// <param name="folderPrefixedUrl">Server relative URL of the folder; may use '~sitecollection/' or '~site/' prefix.</param>
        /// <returns>The existing or newly created folder</returns>
        public Folder EnsureFolder(Web web, string folderPrefixedUrl)
        {
            return EnsureFolderInternal(web, folderPrefixedUrl);
        }

        /// <summary>
        /// Creates or updates a site collection ScriptLink reference to a script file, doing nothing if the ScriptLink already exists with the specified values
        /// </summary>
        /// <param name="name">Key to identify the ScriptLink</param>
        /// <param name="scriptPrefixedUrl">URL of the script; may use '~sitecollection/' or '~site/' prefix.</param>
        /// <param name="sequence">Determines the order the ScriptLink is rendered in</param>
        /// <returns>The UserCustomAction representing the ScriptLink</returns>
        public UserCustomAction EnsureSiteScriptLink(string name, string scriptPrefixedUrl, int sequence)
        {
            var site = _clientContext.Site;
            if (!site.IsObjectPropertyInstantiated("UserCustomActions"))
            {
                _clientContext.Load(site.UserCustomActions, collection => collection.Include(ca => ca.Name));
                _clientContext.ExecuteQuery();
            }
            var action = site.UserCustomActions.FirstOrDefault(ca => string.Equals(ca.Name, name, StringComparison.InvariantCultureIgnoreCase));
            if (action == null)
            {
                action = site.UserCustomActions.Add();
                action.Location = "ScriptLink";
                action.Name = name;
                action.ScriptSrc = scriptPrefixedUrl;
                action.Sequence = sequence;
                action.Update();
                _logger.Information("Adding ScriptLink '{0}'='{1}'", name, scriptPrefixedUrl);
                _clientContext.ExecuteQuery();
            }
            else
            {
                _clientContext.Load(action);
                _clientContext.ExecuteQuery();
                bool changed = false;
                if (action.Location != "ScriptLink")
                {
                    action.Location = "ScriptLink";
                    changed = true;
                }
                if (action.Name != name)
                {
                    action.Name = name;
                    changed = true;
                }
                if (action.ScriptSrc != scriptPrefixedUrl)
                {
                    action.ScriptSrc = scriptPrefixedUrl;
                    changed = true;
                }
                if (action.Sequence != sequence)
                {
                    action.Sequence = sequence;
                    changed = true;
                }
                if (changed)
                {
                    _logger.Information("Updating ScriptLink '{0}'='{1}'", name, scriptPrefixedUrl);
                    action.Update();
                    _clientContext.ExecuteQuery();
                }
                else
                {
                    _logger.Information("No change for ScriptLink '{0}'", name);
                }
            }
            return action;
        }

        /// <summary>
        /// Uploads a file, replacing any existing file with a new version, publishing if necessary.
        /// </summary>
        /// <param name="sourcePath">Local path of the file to upload</param>
        /// <param name="web">Web the destination folder exists in</param>
        /// <param name="destinationPrefixedUrl">Server relative URL of the destination file; may use '~sitecollection/' or '~site/' prefix; the folder must already exist</param>
        /// <returns>The updated or newly created file</returns>
        public File UploadFile(string sourcePath, Web web, string destinationPrefixedUrl)
        {
            return UploadFile(sourcePath, web, destinationPrefixedUrl, FileLevel.Published);
        }

        /// <summary>
        /// Uploads a file, replacing any existing file with a new version.
        /// </summary>
        /// <param name="sourcePath">Local path of the file to upload</param>
        /// <param name="web">Web the destination folder exists in</param>
        /// <param name="destinationPrefixedUrl">Server relative URL of the destination file; may use '~sitecollection/' or '~site/' prefix; the folder must already exist</param>
        /// <param name="publishingLevel">Target final state of the file, e.g. Published or Draft</param>
        /// <returns>The updated or newly created file</returns>
        public File UploadFile(string sourcePath, Web web, string destinationPrefixedUrl, FileLevel publishingLevel)
        {
            using (var sourceStream = System.IO.File.OpenRead(sourcePath))
            {
                return UploadFile(sourceStream, web, destinationPrefixedUrl, publishingLevel);
            }
        }

        /// <summary>
        /// Uploads a file, replacing any existing file with a new version.
        /// </summary>
        /// <param name="stream">Contents of the file to upload</param>
        /// <param name="web">Web the destination folder exists in</param>
        /// <param name="destinationPrefixedUrl">Server relative URL of the destination file; may use '~sitecollection/' or '~site/' prefix; the folder must already exist</param>
        /// <param name="publishingLevel">Target final state of the file, e.g. Published or Draft</param>
        /// <returns>The updated or newly created file</returns>
        public File UploadFile(System.IO.Stream stream, Web web, string destinationPrefixedUrl, FileLevel publishingLevel)
        {
            var destinationFileServerRelativeUrl = SPUrlUtility.ResolveServerRelativeUrl(_clientContext.Site, web, destinationPrefixedUrl);
            var destinationFolderServerRelativeUrl = destinationFileServerRelativeUrl.Substring(0, destinationFileServerRelativeUrl.LastIndexOf('/'));
            //            _logger.Information("DEBUG: file '{0}', folder '{1}'", fileName, folderServerRelativeUrl);
            var destinationFolder = web.GetFolderByServerRelativeUrl(destinationFolderServerRelativeUrl);

            return UploadFileInternal(stream, web, destinationFolder, destinationFileServerRelativeUrl, publishingLevel);
        }

        /// <summary>
        /// Uploads all files in the specified local folder, after checking a hash value for changes, to the destination folder in the context Web (usually root).
        /// </summary>
        /// <param name="sourcePath">Local folder to upload</param>
        /// <param name="destinationPrefixedUrl">Server relative URL of the destination folder; may use '~sitecollection/' or '~site/' prefix; the folder must already exist</param>
        public void UploadFolder(string sourcePath, string destinationPrefixedUrl)
        {
            UploadFolderInternal(sourcePath, _clientContext.Web, destinationPrefixedUrl, FileLevel.Published);
        }

        /// <summary>
        /// Uploads all files in the specified local folder, after checking a hash value for changes, to the destination folder.
        /// </summary>
        /// <param name="sourcePath">Local folder to upload</param>
        /// <param name="web">Web the destination folder exists in</param>
        /// <param name="destinationPrefixedUrl">Server relative URL of the destination folder; may use '~sitecollection/' or '~site/' prefix; the folder must already exist</param>
        /// <param name="publishingLevel">Target final state of the file, e.g. Published or Draft</param>
        public void UploadFolder(string sourcePath, Web web, string destinationPrefixedUrl, FileLevel publishingLevel)
        {
            UploadFolderInternal(sourcePath, web, destinationPrefixedUrl, publishingLevel);
        }

        private Folder EnsureFolderInternal(Web web, string folderPrefixedUrl)
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

        private File UploadFileInternal(System.IO.Stream stream, Web web, Folder destinationFolder, string destinationFileServerRelativeUrl, FileLevel publishingLevel)
        {
            //if (hashProvider == null)
            //{
            //    hashProvider = new NullHashProvider();
            //}
            //if (preprocessors == null)
            //{
            //    preprocessors = new ITextFilePreprocessor[0];
            //}

            File file = null;

            var splitIndex = destinationFileServerRelativeUrl.LastIndexOf('/');
            var fileName = destinationFileServerRelativeUrl.Substring(splitIndex + 1);

            byte[] serverHash = hashProvider.GetFileHash(destinationFileServerRelativeUrl);
            bool contentsMatch = false;

            //            _logger.Information("DEBUG: Check hash: {0}", BitConverter.ToString(serverHash));

            using (var streamPreprocessor = new StreamPreprocessor(stream, preprocessors))
            {
                // Compare hash
                HashAlgorithm ha = HashAlgorithm.Create();
                var localHash = ha.ComputeHash(streamPreprocessor.Stream);
                streamPreprocessor.Stream.Position = 0;
                //                _logger.Information("DEBUG: Local hash: {0}", BitConverter.ToString(localHash));
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
                    file = web.GetFileByServerRelativeUrl(destinationFileServerRelativeUrl);
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
                        _logger.Information("Checking out file '{0}'", destinationFileServerRelativeUrl);
                        file.CheckOut();
                        _clientContext.ExecuteQuery();
                    }

                    // Upload file
                    var newFileInfo = new FileCreationInformation()
                    {
                        ContentStream = streamPreprocessor.Stream,
                        Url = fileName,
                        Overwrite = true
                    };
                    _logger.Information("{0} : updating ({1}..)", destinationFileServerRelativeUrl, BitConverter.ToString(localHash).Substring(0, 12));
                    file = destinationFolder.Files.Add(newFileInfo);
                    _clientContext.Load(file);
                    _clientContext.ExecuteQuery();

                    // Check in and publish
                    var publishingRequired = false;
                    var approvalRequired = false;
                    if (publishingLevel == FileLevel.Draft || publishingLevel == FileLevel.Published)
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
                        if (publishingLevel == FileLevel.Published)
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

                    hashProvider.StoreFileHash(destinationFileServerRelativeUrl, localHash);
                }
                else
                {
                    _logger.Information("{0} : no change ({1}..).", destinationFileServerRelativeUrl, BitConverter.ToString(serverHash).Substring(0, 12));
                }
            }
            return file;
        }

        private void UploadFolderInternal(string sourcePath, Web web, string destinationPrefixedUrl, FileLevel publishingLevel)
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
                    var file = UploadFileInternal(localStream, web, destinationFolder, fileUrl, publishingLevel);
                }
            }
        }

    }

}
