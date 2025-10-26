namespace WorldDomination.SimpleRemoteGitRepoBackup.Core;

/// <summary>
/// Represents a repository to be backed up
/// </summary>
public class RepositoryInfo
{
    /// <summary>
    /// Owner of the repository.
    /// </summary>
    public required string Owner{ get; set; }

    /// <summary>
    /// The name of the repository
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Whether the repository is private
    /// </summary>
    public bool IsPrivate { get; init; }

    /// <summary>
    /// Whether the repository is archived
    /// </summary>
    public bool IsArchived { get; init; }

    /// <summary>
    /// Whether the repository is empty (no commits/code)
    /// </summary>
    public bool IsEmpty { get; init; }

    /// <summary>
    /// The default branch name (e.g., "main", "master")
    /// </summary>
    public string DefaultBranch { get; init; } = "main";

    public List<string>? Branches { get; set; }
}
