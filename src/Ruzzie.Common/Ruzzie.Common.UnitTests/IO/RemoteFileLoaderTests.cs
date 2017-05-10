using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Ruzzie.Common.IO;

namespace Ruzzie.Common.UnitTests.IO
{
#if! PORTABLE
    [TestFixture]
    public class RemoteRemoteFileLoaderTests
    {
        [Test]
        public void DontDownloadIfOlder()
        {
            //Arrange
            string filename = Guid.NewGuid() + ".json";
            File.Create(filename).Dispose(); //touch new file

            Mock<IFileDownloader> mockDownloader = new Mock<IFileDownloader>(MockBehavior.Strict);
            mockDownloader.Setup(downloader => downloader.MetaData)
                .Returns(new RemoteFileMetaData {LastModifiedTimeUtc = DateTime.UtcNow.Subtract(new TimeSpan(265, 0, 0))});

            IFileDownloader fileDownloader = mockDownloader.Object;

            //Act
            IRemoteFileLoader remoteFileLoader = new RemoteFileLoader(fileDownloader, filename, ".\\");

            remoteFileLoader.GetLocalOrDownloadIfNewer();

            //Assert
            mockDownloader.Verify();
        }

        [Test]
        public void DownloadIfNewer()
        {
            //Arrange
            string filename = Guid.NewGuid() + ".json";
            File.Create(filename).Dispose(); //touch new file

            Mock<IFileDownloader> mockDownloader = new Mock<IFileDownloader>(MockBehavior.Strict);
            mockDownloader.Setup(downloader => downloader.MetaData).Returns(new RemoteFileMetaData {LastModifiedTimeUtc = DateTime.UtcNow.AddYears(1)});
            mockDownloader.Setup(downloader => downloader.DownloadFile(".\\" + filename));

            IFileDownloader fileDownloader = mockDownloader.Object;

            //Act
            IRemoteFileLoader remoteFileLoader = new RemoteFileLoader(fileDownloader, filename, ".\\");

            remoteFileLoader.GetLocalOrDownloadIfNewer();

            //Assert
            mockDownloader.Verify();
        }

        [Test]
        public void MultipleThreadAccessSmokeTest()
        {
            //Arrange
            string filename = Guid.NewGuid() + ".json";
            File.Create(filename).Dispose(); //touch new file       

            IFileDownloader fileDownloader = new BlockingFileDownloaderForTesting(new RemoteFileMetaData { LastModifiedTimeUtc = DateTime.UtcNow.AddYears(1) },10);

            IRemoteFileLoader remoteFileLoader = new RemoteFileLoader(fileDownloader, filename, ".\\");
            //Act
            Parallel.For(0, 100, (i) =>
            {
                remoteFileLoader.GetLocalOrDownloadIfNewer();
            });           
        }

        class BlockingFileDownloaderForTesting : IFileDownloader
        {
            private readonly int _downloadBlockingTimeInMillis;

            public BlockingFileDownloaderForTesting(IRemoteFileMetaData metaDataToReturn, int downloadBlockingTimeInMillis)
            {
                _downloadBlockingTimeInMillis = downloadBlockingTimeInMillis;
                MetaData = metaDataToReturn;
            }

            public void DownloadFile(string localPathToStoreFile)
            {
                // ReSharper disable once UnusedVariable
                using (var fs = File.OpenWrite(localPathToStoreFile))
                {
                    Thread.Sleep(_downloadBlockingTimeInMillis);
                }
            }

            public IRemoteFileMetaData MetaData { get; }
        }

        [Test]
        public void DownloadIfNotExist()
        {
            //Arrange
            string filename = Guid.NewGuid() + ".json";
            //File.Create(filename).Close(); //touch new file

            Mock<IFileDownloader> mockDownloader = new Mock<IFileDownloader>(MockBehavior.Strict);
            mockDownloader.Setup(downloader => downloader.MetaData).Returns(new RemoteFileMetaData {LastModifiedTimeUtc = DateTime.UtcNow.AddYears(1)});
            mockDownloader.Setup(downloader => downloader.DownloadFile(".\\" + filename));

            IFileDownloader fileDownloader = mockDownloader.Object;

            //Act
            IRemoteFileLoader remoteFileLoader = new RemoteFileLoader(fileDownloader, filename, ".\\");

            remoteFileLoader.GetLocalOrDownloadIfNewer();

            //Assert
            mockDownloader.Verify();
        }
    }
#endif
}