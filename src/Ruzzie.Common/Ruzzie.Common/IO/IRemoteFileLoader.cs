using System.IO;

namespace Ruzzie.Common.IO
{
#if !PORTABLE
    /// <summary>
    /// Interface for a remote file loader.
    /// </summary>
    public interface IRemoteFileLoader
    {
        /// <summary>
        /// Returns the local (cached) file, and downloads the remote file first if it is newer.
        /// </summary>
        /// <returns>The FileInfo to the file.</returns>
        FileInfo GetLocalOrDownloadIfNewer();
    }
#endif
}
