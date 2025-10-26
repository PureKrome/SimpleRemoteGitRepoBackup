namespace WorldDomination.SimpleRemoteGitRepoBackup.Console;

public class CommandOptions
{
    [Option('u', "username", Required = true, HelpText = "Repository username to backup repositories from")]
    public string Username { get; init; } = string.Empty;

    [Option('s', "site", Default = "github", HelpText = "Repository website. Accepted values: GitHub")]
    public string Site { get; init; } = "github";

    [Option('t', "token", HelpText = "Repository Personal Access Token for authentication (required for private repos)")]
    public string? Token { get; init; }

    [Option('d', "directory", Required = false, HelpText = "Target directory for downloads .(Will default to the current directory if ommited)")]
    public string? TargetDirectory { get; init; } = string.Empty;

    [Option('p', "private-only", Default = false, HelpText = "Download only private repositories")]
    public bool PrivateOnly { get; init; }

    [Option('a', "include-archived", Default = true, HelpText = "Include archived repositories")]
    public bool IncludeArchived { get; init; }

    [Option('c', "concurrent", Default = 10, HelpText = "Maximum number of concurrent downloads (1-10, default: 3)")]
    public int MaxConcurrentDownloads { get; init; }

    [Option('b', "all-branches", Default = false, HelpText = "Maximum number of concurrent downloads (1-10, default: 3)")]
    public bool AllBranches { get; init; }
}
