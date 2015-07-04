using System;
using System.Linq;
using Microsoft.SharePoint.Client;
using System.Security.Cryptography;
using IonFar.SharePoint.Migration.Providers;
using System.Collections.Generic;
using IonFar.SharePoint.Migration.Sync;

namespace IonFar.SharePoint.Migration.Services
{
    /// <summary>
    /// Synchronizes changed files in a folder to SharePoint, based on hash values
    /// </summary>
    public class Synchronizer
    {
        private readonly string _apiUrl = string.Empty;
        private readonly SynchronizerConfiguration _configuration;

        /// <summary>
        /// Creates a new Synchronizer with the specified configuration
        /// </summary>
        /// <param name="configuration">the configuration</param>
        public Synchronizer(SynchronizerConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Ensures the specified folder exists (in the context Web, usually the root), creating all folders in the path if necessary.
        /// </summary>
        /// <param name="folderPrefixedUrl">Server relative URL of the folder; may use '~sitecollection/' or '~site/' prefix.</param>
        /// <returns>The existing or newly created folder</returns>
        public Folder EnsureFolder(string folderPrefixedUrl)
        {
            using (_configuration.ContextManager.ContextScope(_configuration.Log))
            {
                return EnsureFolderInternal(_configuration.ContextManager.CurrentContext.Web, folderPrefixedUrl);
            }
        }

        ///// <summary>
        ///// Ensures the specified folder exists, creating all folders in the path if necessary.
        ///// </summary>
        ///// <param name="web">The web to create the folder within</param>
        ///// <param name="folderPrefixedUrl">Server relative URL of the folder; may use '~sitecollection/' or '~site/' prefix.</param>
        ///// <returns>The existing or newly created folder</returns>
        //public Folder EnsureFolder(Web web, string folderPrefixedUrl)
        //{
        //    return EnsureFolderInternal(web, folderPrefixedUrl);
        //}

        /// <summary>
        /// Creates or updates a site collection ScriptLink reference to a script file, doing nothing if the ScriptLink already exists with the specified values
        /// </summary>
        /// <param name="name">Key to identify the ScriptLink</param>
        /// <param name="scriptPrefixedUrl">URL of the script; may use '~sitecollection/' or '~site/' prefix.</param>
        /// <param name="sequence">Determines the order the ScriptLink is rendered in</param>
        /// <returns>The UserCustomAction representing the ScriptLink</returns>
        public UserCustomAction EnsureSiteScriptLink(string name, string scriptPrefixedUrl, int sequence)
        {
            using (_configuration.ContextManager.ContextScope(_configuration.Log))
            {
                var clientContext = _configuration.ContextManager.CurrentContext;
                var site = clientContext.Site;
                if (!site.IsObjectPropertyInstantiated("UserCustomActions"))
                {
                    clientContext.Load(site.UserCustomActions, collection => collection.Include(ca => ca.Name));
                    clientContext.ExecuteQuery();
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
                    _configuration.Log.Information("Adding ScriptLink '{0}' = '{1}'", name, scriptPrefixedUrl);
                    clientContext.ExecuteQuery();
                }
                else
                {
                    clientContext.Load(action);
                    clientContext.ExecuteQuery();
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
                        _configuration.Log.Information("Updating ScriptLink '{0}' = '{1}'", name, scriptPrefixedUrl);
                        action.Update();
                        clientContext.ExecuteQuery();
                    }
                    else
                    {
                        _configuration.Log.Information("No change for ScriptLink '{0}'", name);
                    }
                }
                return action;
            }
        }

        /// <summary>
        /// Uploads all files in the specified local folder, after checking a hash value for changes, to the destination folder in the context Web (usually root).
        /// </summary>
        /// <param name="sourcePath">Local folder to upload</param>
        /// <param name="destinationPrefixedUrl">Server relative URL of the destination folder; may use '~sitecollection/' or '~site/' prefix; the folder must already exist</param>
        public SynchronizationResult SynchronizeFolder(string sourcePath, string destinationPrefixedUrl)
        {
            return SynchronizeFolder(sourcePath, destinationPrefixedUrl, FileLevel.Published);
        }

        /// <summary>
        /// Uploads all files in the specified local folder, after checking a hash value for changes, to the destination folder.
        /// </summary>
        /// <param name="sourcePath">Local folder to upload</param>
        /// <param name="destinationPrefixedUrl">Server relative URL of the destination folder; may use '~sitecollection/' or '~site/' prefix; the folder must already exist</param>
        /// <param name="publishingLevel">Target final state of the file, e.g. Published or Draft</param>
        public SynchronizationResult SynchronizeFolder(string sourcePath, string destinationPrefixedUrl, FileLevel publishingLevel)
        {
            using (_configuration.ContextManager.ContextScope(_configuration.Log))
            {
                return SynchronizeFolderInternal(sourcePath, _configuration.ContextManager.CurrentContext.Web, destinationPrefixedUrl, publishingLevel);
            }
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
            var destinationFileServerRelativeUrl = SPUrlUtility.ResolveServerRelativeUrl(_configuration.ContextManager.CurrentContext.Site, web, destinationPrefixedUrl);
            var destinationFolderServerRelativeUrl = destinationFileServerRelativeUrl.Substring(0, destinationFileServerRelativeUrl.LastIndexOf('/'));
            //            _configuration.Log.Information("DEBUG: file '{0}', folder '{1}'", fileName, folderServerRelativeUrl);
            var destinationFolder = web.GetFolderByServerRelativeUrl(destinationFolderServerRelativeUrl);

            using (var sourceStream = System.IO.File.OpenRead(sourcePath))
            {
                var result = UploadFileInternal(sourceStream, web, destinationFolder, sourcePath, destinationFileServerRelativeUrl, publishingLevel);
                return result.Item1;
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
            var destinationFileServerRelativeUrl = SPUrlUtility.ResolveServerRelativeUrl(_configuration.ContextManager.CurrentContext.Site, web, destinationPrefixedUrl);
            var destinationFolderServerRelativeUrl = destinationFileServerRelativeUrl.Substring(0, destinationFileServerRelativeUrl.LastIndexOf('/'));
            //            _configuration.Log.Information("DEBUG: file '{0}', folder '{1}'", fileName, folderServerRelativeUrl);
            var destinationFolder = web.GetFolderByServerRelativeUrl(destinationFolderServerRelativeUrl);

            var result = UploadFileInternal(stream, web, destinationFolder, null, destinationFileServerRelativeUrl, publishingLevel);
            return result.Item1;
        }

        private Folder EnsureFolderInternal(Web web, string folderPrefixedUrl)
        {
            if (web == null) { throw new ArgumentNullException("web"); }
            if (folderPrefixedUrl == null) { throw new ArgumentNullException("folderPrefixedUrl"); }

            var folderServerRelativeUrl = SPUrlUtility.ResolveServerRelativeUrl(_configuration.ContextManager.CurrentContext.Site, web, folderPrefixedUrl);

            _configuration.Log.Information("Ensuring folder '{0}' exists, creating if necessary", folderServerRelativeUrl);

            if (!_configuration.ContextManager.CurrentContext.Web.IsPropertyAvailable("ServerRelativeUrl"))
            {
                _configuration.ContextManager.CurrentContext.Load(_configuration.ContextManager.CurrentContext.Web);
                _configuration.ContextManager.CurrentContext.ExecuteQuery();
            }

            if (!folderServerRelativeUrl.StartsWith(_configuration.ContextManager.CurrentContext.Web.ServerRelativeUrl))
            {
                var msg = string.Format("You should not create a folder above the current Web root (web root: {0}, folder: {1})",
                    _configuration.ContextManager.CurrentContext.Web.ServerRelativeUrl,
                    folderServerRelativeUrl);
                throw new Exception(msg);
            }

            var folder = _configuration.ContextManager.CurrentContext.Web.GetFolderByServerRelativeUrl(folderServerRelativeUrl);
            _configuration.ContextManager.CurrentContext.Load(folder);
            try
            {
                _configuration.ContextManager.CurrentContext.ExecuteQuery();
            }
            catch (ServerException)
            {
                var segments = folderServerRelativeUrl.Split(new[] { '/' }).ToList();
                var lastSegment = segments.Last();
                var parentFolderPath = string.Join("/", segments.Take(segments.Count() - 1));

                // Recurse
                var parentFolder = EnsureFolder(parentFolderPath);

                folder = parentFolder.Folders.Add(lastSegment);
                _configuration.ContextManager.CurrentContext.Load(folder);
                _configuration.Log.Information("Creating folder '{0}' under parent '{1}'", lastSegment, parentFolder);
                _configuration.ContextManager.CurrentContext.ExecuteQuery();
            }
            return folder;
        }

        private SynchronizationResult SynchronizeFolderInternal(string sourcePath, Web web, string destinationPrefixedUrl, FileLevel publishingLevel)
        {
            // TODO: Consider adding recursive, but think about subfolder creation.

            if (sourcePath == null) { throw new ArgumentNullException("sourcePath"); }
            if (web == null) { throw new ArgumentNullException("web"); }
            if (destinationPrefixedUrl == null) { throw new ArgumentNullException("destinationPrefixedUrl"); }

            var processedFiles = new List<UploadInfo>();
            try
            {
                //if (recursive)
                //{
                //    throw new NotSupportedException("Recursive not supported yet");
                //}

                _configuration.Log.Information("Uploading folder '{0}'", destinationPrefixedUrl);

                var destinationServerRelativeUrl = SPUrlUtility.ResolveServerRelativeUrl(_configuration.ContextManager.CurrentContext.Site, web, destinationPrefixedUrl);
                var destinationFolder = web.GetFolderByServerRelativeUrl(destinationServerRelativeUrl);

                var filePaths = System.IO.Directory.EnumerateFiles(sourcePath);
                foreach (var filePath in filePaths)
                {
                    var fileName = System.IO.Path.GetFileName(filePath);
                    var fileUrl = SPUrlUtility.Combine(destinationServerRelativeUrl, fileName);

                    using (var localStream = System.IO.File.OpenRead(filePath))
                    {
                        var uploadResult = UploadFileInternal(localStream, web, destinationFolder, filePath, fileUrl, publishingLevel);
                        processedFiles.Add(uploadResult.Item2);
                    }
                }

                return new SynchronizationResult(processedFiles, successful: true, error: null);
            }
            catch (Exception ex)
            {
                _configuration.Log.Error(
                    "Synchronization failed and the environment has been left in a partially complete state, manual intervention may be required.\nException: {0}", ex
                );
                return new SynchronizationResult(processedFiles, successful: false, error: ex);
            }
        }

        private Tuple<File, UploadInfo> UploadFileInternal(System.IO.Stream stream, Web web, Folder destinationFolder, string sourceFilePath, string destinationFileServerRelativeUrl, FileLevel publishingLevel)
        {
            File file = null;
            byte[] localHash = null;
            bool changed = false;

            var splitIndex = destinationFileServerRelativeUrl.LastIndexOf('/');
            var fileName = destinationFileServerRelativeUrl.Substring(splitIndex + 1);

            byte[] serverHash = _configuration.HashProvider.GetFileHash(_configuration.ContextManager, _configuration.Log, destinationFileServerRelativeUrl);
            bool contentsMatch = false;

            //            _configuration.Log.Information("DEBUG: Check hash: {0}", BitConverter.ToString(serverHash));

            using (var streamPreprocessor = new StreamPreprocessor(_configuration.ContextManager, _configuration.Log, stream, _configuration.Preprocessors))
            {
                // Compare hash
                HashAlgorithm ha = HashAlgorithm.Create();
                localHash = ha.ComputeHash(streamPreprocessor.Stream);
                streamPreprocessor.Stream.Position = 0;
                //                _configuration.Log.Information("DEBUG: Local hash: {0}", BitConverter.ToString(localHash));
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
                    _configuration.ContextManager.CurrentContext.Load(file, f => f.CheckOutType);
                    try
                    {
                        _configuration.ContextManager.CurrentContext.ExecuteQuery();

                        var parentList = file.ListItemAllFields.ParentList;
                        _configuration.ContextManager.CurrentContext.Load(parentList, l => l.ForceCheckout);
                        try
                        {
                            _configuration.ContextManager.CurrentContext.ExecuteQuery();
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
                        _configuration.Log.Information("Checking out file '{0}'", destinationFileServerRelativeUrl);
                        file.CheckOut();
                        _configuration.ContextManager.CurrentContext.ExecuteQuery();
                    }

                    // Upload file
                    var newFileInfo = new FileCreationInformation()
                    {
                        ContentStream = streamPreprocessor.Stream,
                        Url = fileName,
                        Overwrite = true
                    };
                    _configuration.Log.Information("{0} : updating ({1}..)", destinationFileServerRelativeUrl, BitConverter.ToString(localHash).Substring(0, 12));
                    file = destinationFolder.Files.Add(newFileInfo);
                    _configuration.ContextManager.CurrentContext.Load(file);
                    _configuration.ContextManager.CurrentContext.ExecuteQuery();
                    changed = true;

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
                            _configuration.Log.Information("Checking in file '{0}'", file.Name);
                            file.CheckIn("Checked in by provisioning", publishingRequired ? CheckinType.MinorCheckIn : CheckinType.MajorCheckIn);
                            context.ExecuteQuery();
                        }
                        if (publishingLevel == FileLevel.Published)
                        {
                            if (publishingRequired)
                            {
                                _configuration.Log.Information("Publishing file '{0}'", file.Name);
                                file.Publish("Published by provisioning");
                                context.ExecuteQuery();
                            }

                            if (approvalRequired)
                            {
                                _configuration.Log.Information("Approving file '{0}'", file.Name);
                                file.Approve("Approved by provisioning");
                                context.ExecuteQuery();
                            }
                        }
                    }

                    _configuration.HashProvider.StoreFileHash(_configuration.ContextManager, _configuration.Log, destinationFileServerRelativeUrl, localHash);
                }
                else
                {
                    _configuration.Log.Information("{0} : no change ({1}..).", destinationFileServerRelativeUrl, BitConverter.ToString(serverHash).Substring(0, 12));
                }
            }

            var syncInfo = new UploadInfo(sourceFilePath, destinationFileServerRelativeUrl, changed, localHash);
            return Tuple.Create(file, syncInfo);
        }

    }

}
