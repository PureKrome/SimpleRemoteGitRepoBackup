namespace WorldDomination.SimpleRemoteGitRepoBackup.GitHub;

/// <summary>
/// GitHub implementation of the repository provider
/// </summary>
public class GitHubRepositoryProvider(IGitHubClient client, IFileSystem fileSystem, ILogger<GitHubRepositoryProvider> logger) : IRepositoryProvider
{
    private readonly IGitHubClient _client = client ?? throw new ArgumentNullException(nameof(client));
    private readonly IFileSystem _fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
    private readonly ILogger<GitHubRepositoryProvider> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public string ProviderName => "GitHub";

    public async Task<IReadOnlyList<RepositoryInfo>> GetRepositoriesAsync(string username, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Fetching all repositories for user: {Username} (with automatic pagination)", username);

            // ApiOptions.None will fetch ALL repositories across all pages automatically.
            var repositories = await _client.Repository.GetAllForUser(username, ApiOptions.None);

            _logger.LogDebug("Found {Count} repositories", repositories.Count);

            var foundRepositories = repositories.Select(repo => new RepositoryInfo
            {
                Owner = repo.Owner.Name,
                Name = repo.Name,
                IsPrivate = repo.Private,
                IsArchived = repo.Archived,
                IsEmpty = repo.Size <= 0,
                DefaultBranch = repo.DefaultBranch ?? "main",
            }).ToList();

            return foundRepositories;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to get repositories");
            throw new InvalidOperationException($"Failed to get repositories: {exception.Message}", exception);
        }
    }

    public async Task<bool> DownloadRepositoryAsync(
        string owner,
        string repository,
        string branch,
        string targetDirectory,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Starting download of repository: {RepositoryName}", repository);

            // Ensure the target directory exists
            _fileSystem.CreateDirectory(targetDirectory);

            // Create a safe filename
            var safeFileName = string.Join("_", repository.Split(Path.GetInvalidFileNameChars()));
            var filePath = Path.Combine(targetDirectory, $"{safeFileName}.zip");

            var archiveBytes = await _client.Repository.Content.GetArchive(
                owner,
                repository,
                ArchiveFormat.Zipball,
                branch);

            await _fileSystem.WriteAllBytesAsync(filePath, archiveBytes, cancellationToken);

            var fileSize = _fileSystem.GetFileSize(filePath);

            _logger.LogInformation("Successfully downloaded {RepositoryName} ({SizeInMB:F2} MB)",
                repository, fileSize / (1024.0 * 1024.0));

            return true;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to download repository: {RepositoryName}. Error Message: {ErrorMessage}",
                repository,
                exception.Message);

            return false;
        }
    }
}
