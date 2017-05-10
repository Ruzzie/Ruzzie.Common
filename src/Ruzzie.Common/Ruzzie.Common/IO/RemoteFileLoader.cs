using System;
using System.IO;

namespace Ruzzie.Common.IO
{
#if !PORTABLE
    /// <summary>
    /// Loads a remote file and caches it locally.
    /// </summary>
    /// <seealso cref="IRemoteFileLoader" />
    public class RemoteFileLoader : IRemoteFileLoader
    {
        private readonly IFileDownloader _fileDownloader;
        private readonly string _filename;
        private readonly string _localPathToStoreFile;
        private readonly object _lockObject = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteFileLoader"/> class.
        /// </summary>
        /// <param name="fileDownloader">The file downloader.</param>
        /// <param name="filename">The filename.</param>
        /// <param name="localPathToStoreFile">The path (directory where to store the file) id the directory does not exists it will be created.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException">
        /// Value cannot be null or whitespace.
        /// or
        /// Value cannot be null or whitespace.
        /// </exception>
        public RemoteFileLoader(IFileDownloader fileDownloader, string filename, string localPathToStoreFile = ".")
        {
            if (fileDownloader == null)
            {
                throw new ArgumentNullException(nameof(fileDownloader));
            }

            if (string.IsNullOrWhiteSpace(filename))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(filename));
            }
            if (string.IsNullOrWhiteSpace(localPathToStoreFile))
            {
                throw new ArgumentException("Value cannot be null or whitespace.", nameof(localPathToStoreFile));
            }
            
            if (!Directory.Exists(localPathToStoreFile))
            {
                Directory.CreateDirectory(localPathToStoreFile);
            }

            _fileDownloader = fileDownloader;
            _filename = filename;
            _localPathToStoreFile = localPathToStoreFile;
        }

        /// <summary>
        /// Returns the local (cached) file, and downloads the remote file first if it is newer.
        /// </summary>
        /// <returns>
        /// The FileInfo to the file.
        /// </returns>
        public FileInfo GetLocalOrDownloadIfNewer()
        {
            //Check if there is a local file
            string localFileName = Path.Combine(_localPathToStoreFile, _filename);
            FileInfo fileInfo = new FileInfo(localFileName);
            if (!fileInfo.Exists)
            {
                lock (_lockObject)
                {
                    if (!fileInfo.Exists) //double check
                    {
                        //File does not exists so download the file
                        _fileDownloader.DownloadFile(localFileName);
                    }
                }
            }
            else
            {
                //Dont check for a new version if it was less than a day ago (perf opt)
                //Check if there is a newer version of the file
                var localFileLastWriteTime = fileInfo.LastWriteTimeUtc;
                DateTime? remoteFileLastModifiedDateTime = _fileDownloader.MetaData.LastModifiedTimeUtc;

                if (remoteFileLastModifiedDateTime > localFileLastWriteTime)
                {
                    lock (_lockObject) //assume remote metadata access is lock free
                    {
                        if (remoteFileLastModifiedDateTime > new FileInfo(localFileName).LastWriteTimeUtc)//double check
                        {
                            //remote file is newer so download the file
                            _fileDownloader.DownloadFile(localFileName);
                        }
                    }
                }
            }

            return new FileInfo(localFileName);
        }
    }
#endif
}
