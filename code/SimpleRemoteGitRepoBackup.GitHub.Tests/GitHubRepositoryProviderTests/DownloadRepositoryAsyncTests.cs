namespace WorldDomination.SimpleRemoteGitRepoBackup.GitHub.Tests.GitHubRepositoryProviderTests;

public class DownloadRepositoryAsyncTests : GitHubRepositoryProviderTestBase
{
    public DownloadRepositoryAsyncTests() : base()
    {
        MockRepositoriesClient
            .Setup(x => x.Content)
            .Returns(MockRepositoryContentsClient.Object);
    }

    [Fact]
    public async Task DownloadRepositoryAsync_WithValidParameters_ShouldReturnTrue()
    {
        // Arrange.
        const string repository = "testrepo";
        const string branch = "main";
        const string targetDirectory = "/fake/path";
        var archiveBytes = new byte[] { 0x50, 0x4B, 0x03, 0x04 }; // ZIP file signature
        var expectedFilePath = Path.Combine(targetDirectory, $"{repository}.zip");

        MockRepositoryContentsClient
            .Setup(x => x.GetArchive(FakeUserName, repository, ArchiveFormat.Zipball, branch))
            .ReturnsAsync(archiveBytes);

        MockFileSystem
            .Setup(x => x.GetFileSize(expectedFilePath))
            .Returns(archiveBytes.Length);

        // Act.
        var result = await RepositoryProvider.DownloadRepositoryAsync(
            FakeUserName,
            repository,
            branch,
            targetDirectory);

        // Assert.
        result.ShouldBeTrue();

        // Verify the file system interactions
        MockFileSystem.Verify(x => x.CreateDirectory(targetDirectory), Times.Once);
        MockFileSystem.Verify(x => x.WriteAllBytesAsync(
            expectedFilePath,
            archiveBytes,
            It.IsAny<CancellationToken>()), Times.Once);
        MockFileSystem.Verify(x => x.GetFileSize(expectedFilePath), Times.Once);
    }

    [Fact]
    public async Task DownloadRepositoryAsync_WithInvalidCharactersInRepositoryName_ShouldSanitizeFilename()
    {
        // Arrange.
        const string owner = "testuser";
        const string repository = "test<>repo:name";
        const string branch = "main";
        const string targetDirectory = "/fake/path";
        var archiveBytes = new byte[] { 0x50, 0x4B, 0x03, 0x04 };
        var sanitizedFileName = string.Join("_", repository.Split(Path.GetInvalidFileNameChars()));
        var expectedFilePath = Path.Combine(targetDirectory, $"{sanitizedFileName}.zip");

        MockRepositoryContentsClient
            .Setup(x => x.GetArchive(owner, repository, ArchiveFormat.Zipball, branch))
            .ReturnsAsync(archiveBytes);

        MockFileSystem
            .Setup(x => x.GetFileSize(expectedFilePath))
            .Returns(archiveBytes.Length);

        // Act.
        var result = await RepositoryProvider.DownloadRepositoryAsync(
            owner,
            repository,
            branch,
            targetDirectory);

        // Assert.
        result.ShouldBeTrue();

        // Verify the file was written with sanitized name
        MockFileSystem.Verify(x => x.WriteAllBytesAsync(
            expectedFilePath,
            archiveBytes,
            It.IsAny<CancellationToken>()), Times.Once);
        }

    [Fact]
    public async Task DownloadRepositoryAsync_WhenExceptionOccurs_ShouldReturnFalse()
    {
        // Arrange.
        const string owner = "testuser";
        const string repository = "testrepo";
        const string branch = "main";
        const string targetDirectory = "/fake/path";

        MockRepositoryContentsClient
            .Setup(x => x.GetArchive(owner, repository, ArchiveFormat.Zipball, branch))
            .ThrowsAsync(new Exception("Download failed"));

        // Act.
        var result = await RepositoryProvider.DownloadRepositoryAsync(owner, repository, branch, targetDirectory);

        // Assert.
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task DownloadRepositoryAsync_WithCancellationToken_ShouldHandleCancellation()
    {
        // Arrange.
        const string owner = "testuser";
        const string repository = "testrepo";
        const string branch = "main";
        const string targetDirectory = "/fake/path";
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        MockRepositoryContentsClient
            .Setup(x => x.GetArchive(owner, repository, ArchiveFormat.Zipball, branch))
            .ReturnsAsync([0x50, 0x4B, 0x03, 0x04]);

        MockFileSystem
            .Setup(x => x.WriteAllBytesAsync(It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        // Act.
        var result = await RepositoryProvider.DownloadRepositoryAsync(
            owner,
            repository,
            branch,
            targetDirectory,
            cancellationTokenSource.Token);

        // Assert.
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task DownloadRepositoryAsync_ShouldCreateTargetDirectoryIfNotExists()
    {
        // Arrange.
        const string owner = "testuser";
        const string repository = "testrepo";
        const string branch = "main";
        const string targetDirectory = "/fake/nested/path";
        var archiveBytes = new byte[] { 0x50, 0x4B, 0x03, 0x04 };
        var expectedFilePath = Path.Combine(targetDirectory, $"{repository}.zip");

        MockRepositoryContentsClient
            .Setup(x => x.GetArchive(owner, repository, ArchiveFormat.Zipball, branch))
            .ReturnsAsync(archiveBytes);

        MockFileSystem
            .Setup(x => x.GetFileSize(expectedFilePath))
            .Returns(archiveBytes.Length);

        // Act.
        var result = await RepositoryProvider.DownloadRepositoryAsync(
            owner,
            repository,
            branch,
            targetDirectory);

        // Assert.
        result.ShouldBeTrue();

        // Verify that CreateDirectory was called
        MockFileSystem.Verify(x => x.CreateDirectory(targetDirectory), Times.Once);
    }
}
