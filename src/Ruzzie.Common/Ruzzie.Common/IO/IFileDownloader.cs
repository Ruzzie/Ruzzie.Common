namespace Ruzzie.Common.IO
{
    /// <summary>
    /// Interface for a file downloader.
    /// </summary>
    public interface IFileDownloader
    {
        /// <summary>
        /// Downloads the file.
        /// </summary>
        /// <param name="localPathToStoreFile">The local path to store file.</param>
        void DownloadFile(string localPathToStoreFile);
     
        /// <summary>
        /// Gets the meta data of the remote file.
        /// </summary>
        /// <value>
        /// The meta data.
        /// </value>
        IRemoteFileMetaData MetaData { get; }
    }
}