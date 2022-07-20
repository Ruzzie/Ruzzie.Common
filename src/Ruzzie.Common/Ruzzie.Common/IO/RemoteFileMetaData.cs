using System;

namespace Ruzzie.Common.IO;

/// <summary>
/// Metadata for a remote file.
/// </summary>
/// <seealso cref="IRemoteFileMetaData" />
public class RemoteFileMetaData : IRemoteFileMetaData
{
    /// <summary>
    /// Gets or sets the last modified time UTC.
    /// </summary>
    /// <value>
    /// The last modified time UTC.
    /// </value>
    public DateTime? LastModifiedTimeUtc { get; set; }
}