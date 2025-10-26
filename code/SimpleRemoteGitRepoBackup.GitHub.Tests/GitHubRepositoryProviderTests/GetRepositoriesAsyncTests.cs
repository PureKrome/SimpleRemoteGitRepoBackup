namespace WorldDomination.SimpleRemoteGitRepoBackup.GitHub.Tests.GitHubRepositoryProviderTests;

public class GetRepositoriesAsyncTests : GitHubRepositoryProviderTestBase
{
    public GetRepositoriesAsyncTests() : base()
    {
    }

    

    [Fact]
    public async Task GetRepositoriesAsync_WithValidUsername_ShouldReturnRepositories()
    {
        // Arrange.
        const string username = "testuser";

        var mockRepositories = new List<Repository>
        {
            CreateFakeRepository("repo1", "testuser", false, false, 100, "main"),
            CreateFakeRepository("repo2", "testuser", true, false, 2,  "master"),
            CreateFakeRepository("repo3", "testuser", false, true, 0,  "main")
        };

        MockRepositoriesClient
            .Setup(x => x.GetAllForUser(FakeUserName, ApiOptions.None))
            .ReturnsAsync(mockRepositories);

        // Act.
        var result = await RepositoryProvider.GetRepositoriesAsync(username);

        // Assert.
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);

        result[0].Name.ShouldBe("repo1");
        result[0].Owner.ShouldBe("testuser");
        result[0].IsPrivate.ShouldBeFalse();
        result[0].IsArchived.ShouldBeFalse();
        result[0].IsEmpty.ShouldBeFalse();
        result[0].DefaultBranch.ShouldBe("main");

        result[1].Name.ShouldBe("repo2");
        result[1].IsPrivate.ShouldBeTrue();
        result[1].DefaultBranch.ShouldBe("master");

        result[2].Name.ShouldBe("repo3");
        result[2].IsArchived.ShouldBeTrue();
        result[2].IsEmpty.ShouldBeTrue();

        MockRepositoriesClient.VerifyAll();
        MockRepositoriesClient.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetRepositoriesAsync_WithEmptyRepositoryList_ShouldReturnEmptyList()
    {
        // Arrange
        var mockRepositories = new List<Repository>();

        MockRepositoriesClient
            .Setup(x => x.GetAllForUser(FakeUserName, ApiOptions.None))
            .ReturnsAsync(mockRepositories);

        // Act
        var result = await RepositoryProvider.GetRepositoriesAsync(FakeUserName);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(0);

        MockRepositoriesClient.VerifyAll();
        MockRepositoriesClient.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetRepositoriesAsync_WhenExceptionOccurs_ShouldThrowInvalidOperationException()
    {
        // Arrange.
        var expectedException = new Exception("GitHub API error");

        MockRepositoriesClient
            .Setup(x => x.GetAllForUser(FakeUserName, ApiOptions.None))
            .ThrowsAsync(expectedException);

        // Act & Assert.
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            async () => await RepositoryProvider.GetRepositoriesAsync(FakeUserName));

        exception.Message.ShouldContain("Failed to get repositories");
        exception.InnerException.ShouldBe(expectedException);

        MockRepositoriesClient.VerifyAll();
        MockRepositoriesClient.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task GetRepositoriesAsync_WithCancellationToken_ShouldPassTokenThrough()
    {
        // Arrange.
        var cancellationToken = new CancellationToken();
        var mockRepositories = new List<Repository>
        {
            CreateFakeRepository("repo1", "testuser", false, false, 22, "main")
        };

        MockRepositoriesClient
            .Setup(x => x.GetAllForUser(FakeUserName, ApiOptions.None))
            .ReturnsAsync(mockRepositories);

        // Act.
        var result = await RepositoryProvider.GetRepositoriesAsync(FakeUserName, cancellationToken);

        // Assert.
        result.ShouldNotBeNull();
        result.Count.ShouldBe(1);

        MockRepositoriesClient.VerifyAll();
        MockRepositoriesClient.VerifyNoOtherCalls();
    }

    private static Repository CreateFakeRepository(
        string name,
        string ownerName,
        bool isPrivate,
        bool isArchived,
        int size,
        string defaultBranch)
    {
        var owner = new User(
            null,
            null,
            null,
            0,
            null,
            DateTime.UtcNow,
            DateTime.UtcNow,
            0,
            null,
            0,
            0,
            null,
            null,
            0,
            0,
            null,
            null,
            ownerName,
            null,
            0,
            null,
            0,
            0,
            0,
            null,
            null,
            false,
            null,
            null);

        var repository = new Repository(
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            0,
            null,
            owner,
            name,
            null,
            false,
            null,
            null,
            null,
            isPrivate,
            false,
            0,
            0,
            defaultBranch,
            0,
            DateTime.UtcNow,
            DateTime.UtcNow,
            DateTime.UtcNow,
            null,
            null,
            null,
            null,
            false,
            false,
            false,
            false,
            false,
            0,
            size,
            null,
            null,
            null,
            isArchived,
            0,
            null,
            RepositoryVisibility.Public,
            [],
            false,
            false,
            false,
            null);

        return repository;
    }
}
