namespace WorldDomination.SimpleRemoteGitRepoBackup.GitHub.Tests.GitHubRepositoryProviderTests;

public class GitHubRepositoryProviderTests : GitHubRepositoryProviderTestBase
{
    [Fact]
    public void Constructor_WithNullClient_ShouldThrowArgumentNullException() =>
        Should.Throw<ArgumentNullException>(() =>
            new GitHubRepositoryProvider(null!, MockFileSystem.Object, MockLogger.Object));

    [Fact]
    public void Constructor_WithNullFileSystem_ShouldThrowArgumentNullException() =>
        Should.Throw<ArgumentNullException>(() =>
            new GitHubRepositoryProvider(MockGitHubClient.Object, null!, MockLogger.Object));

    [Fact]
    public void Constructor_WithNullLogger_ShouldThrowArgumentNullException() =>
        Should.Throw<ArgumentNullException>(() =>
            new GitHubRepositoryProvider(MockGitHubClient.Object, MockFileSystem.Object, null!));

    [Fact]
    public void ProviderName_ShouldReturnGitHub()
    {
        // Arrange & Act.
        var providerName = RepositoryProvider.ProviderName;

        // Assert.
        providerName.ShouldBe("GitHub");
    }
}
