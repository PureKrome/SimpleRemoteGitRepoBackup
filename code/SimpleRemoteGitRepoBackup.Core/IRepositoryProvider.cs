namespace WorldDomination.SimpleRemoteGitRepoBackup.Core;

/// <summary>
/// Interface for repository backup providers (GitHub, GitLab, etc.)
/// </summary>
public interface IRepositoryProvider
{
    /// <summary>
    /// Name of the provider (e.g., "GitHub", "GitLab")
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets all repositories for the authenticated user or specified account
    /// </summary>
    /// <param name="username">Username to get repositories for (optional - uses authenticated user if null)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of repositories</returns>
    Task<IReadOnlyList<RepositoryInfo>> GetRepositoriesAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a repository archive to the specified directory
    /// </summary>
    /// <param name="owner">Owner of the repository</param>
    /// <param name="repository">Repository to download</param>
    /// <param name="branch">Branch to download</param>
    /// <param name="targetDirectory">Directory to save the archive</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>true if download was successful, otherwise false.</returns>
    Task<bool> DownloadRepositoryAsync(
        string owner,
        string repository,
        string branch,
        string targetDirectory,
        CancellationToken cancellationToken = default);
}
