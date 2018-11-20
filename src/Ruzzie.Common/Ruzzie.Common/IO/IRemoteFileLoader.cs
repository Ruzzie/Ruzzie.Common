using System.IO;

namespace Ruzzie.Common.IO
{
#if !PORTABLE && !NETSTANDARD1_1 && !NETSTANDARD1_2
    /// <summary>
    /// Interface for a remote file loader.B
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
