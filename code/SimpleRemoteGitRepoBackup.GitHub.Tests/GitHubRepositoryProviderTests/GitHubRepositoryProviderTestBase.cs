namespace WorldDomination.SimpleRemoteGitRepoBackup.GitHub.Tests.GitHubRepositoryProviderTests;

public abstract class GitHubRepositoryProviderTestBase
{
    protected Mock<IGitHubClient> MockGitHubClient { get; init; }
    protected Mock<IRepositoriesClient> MockRepositoriesClient { get; init; }
    protected Mock<IRepositoryContentsClient> MockRepositoryContentsClient { get; init; }
    protected Mock<IFileSystem> MockFileSystem { get; init; }
    protected Mock<ILogger<GitHubRepositoryProvider>> MockLogger { get; init; }

    protected GitHubRepositoryProvider RepositoryProvider { get; init; }

    protected const string FakeUserName = "testuser";

    protected GitHubRepositoryProviderTestBase()
    {
        MockGitHubClient = new Mock<IGitHubClient>();
        MockRepositoriesClient = new Mock<IRepositoriesClient>();
        MockRepositoryContentsClient = new Mock<IRepositoryContentsClient>();
        MockFileSystem = new Mock<IFileSystem>();
        MockLogger = new Mock<ILogger<GitHubRepositoryProvider>>();

        MockGitHubClient
            .Setup(x => x.Repository)
            .Returns(MockRepositoriesClient.Object);

        RepositoryProvider = new GitHubRepositoryProvider(
            MockGitHubClient.Object,
            MockFileSystem.Object,
            MockLogger.Object);
    }
}
