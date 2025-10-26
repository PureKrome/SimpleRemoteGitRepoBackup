# SimpleRemoteGitRepoBackup

A cross-platform console application for backing up Git repositories to local storage.

## Features

- 🌍 **Cross-Platform**: Works on Windows, Linux, and macOS
- 🔐 **Authentication Support**: Access both public and private repositories with Personal Access Token
- 🎯 **Flexible Filtering**: 
  - Download private repositories only
  - Exclude archived repositories
  - Filter by specific criteria
- 🖥️ **Command-Line Interface**: Built with [Spectre.Console.Cli](https://spectreconsole.net/) for robust argument parsing
- 🔌 **Extensible Architecture**: Core/Provider pattern allows easy addition of other Git hosting providers (GitLab, Bitbucket, etc.)


## Quick Start

```bash
# Download all public repositories for a user (this is for GitHub))
SimpleRemoteGitRepoBackup -username PureKrome

# Download with authentication (access private repos)
SimpleRemoteGitRepoBackup -username PureKrome -token ghp_xxxxxxxxxxxx

# Download only private repositories
SimpleRemoteGitRepoBackup -username PureKrome -token  ghp_xxxxxxxxxxxx --private-only

# Specify custom directory and include archived repos
SimpleRemoteGitRepoBackup -username PureKrome -token  ghp_xxxxxxxxxxxx --private-only -directory /path/to/backups --include-archived
```

## Command-Line Options

### Usage

```
SimpleRemoteGitRepoBackup [OPTIONS]
```

### Options

| Option | Short | Description | Default |
|--------|-------|-------------|---------|
| `--username <USERNAME>` | `-u` | Git Repo Owner/UserName | - |
| `--token <TOKEN>` | `-t` | Git Repo Personal Access Token for authentication | None (public access) |
| `--directory <PATH>` | `-d` | Target directory for downloads | `~/CodeBackup` |
| `--private-only` | | Download only private repositories | `false` |
| `--include-archived` | | Include archived repositories | `false` (excluded by default) |



## Creating a GitHub Personal Access Token

1. Go to GitHub Settings → Developer Settings → Personal Access Tokens → Tokens (classic)
2. Click "Generate new token"
3. Give it a descriptive name (e.g., "CodeBackup")
4. Select the following scopes:
   - `repo` (for private repositories)
   - `public_repo` (for public repositories - included in `repo`)
5. Click "Generate token"
6. **Important**: Copy the token immediately - you won't be able to see it again!

```bash
export GITHUB_TOKEN=ghp_xxxxxxxxxxxx
dotnet run --project WorldDomination.CodeBackup.Console backup PureKrome --token $GITHUB_TOKEN
```

## Installation

### Building from Source

```bash
# Clone the repository
git clone https://github.com/yourusername/WorldDomination.CodeBackup.git
cd WorldDomination.CodeBackup

# Build the solution
dotnet build

# Run the application
dotnet run --project WorldDomination.CodeBackup.Console -username <username> ...
```

### Publishing as Executable

Create a self-contained executable:

```bash
# For Windows
dotnet publish WorldDomination.CodeBackup.Console -c Release -r win-x64 -o $PWD/publish

# For Linux
dotnet publish WorldDomination.CodeBackup.Console -c Release -r linux-x64 -o $PWD/publish

# For macOS
dotnet publish WorldDomination.CodeBackup.Console -c Release -r osx-x64 -o $PWD/publish
```

After publishing, you can run the executable directly:

```bash
# Windows
.\publish\SimpleRemoteGitRepoBackup.exe --username PureKrome ...

# Linux/macOS
./publish/SimpleRemoteGitRepoBackup --username PureKrome ...
```

## Prerequisites

- .NET 9.0 SDK or later
- A GitHub account
- (Optional) GitHub Personal Access Token for private repositories

## Configuration

## Architecture

### Core Project
- **Purpose**: Contains platform-agnostic interfaces and models
- **Key Interfaces**:
  - `IRepositoryProvider`: Contract for repository providers

## Extending for Other Providers

To add support for other Git hosting providers (GitLab, Bitbucket, etc.):

1. Create a new project: `WorldDomination.CodeBackup.GitLab`
2. Implement `IRepositoryProvider` interface
3. Add provider-specific authentication and API calls
4. Register in the console application

Example:

```csharp
public class GitLabRepositoryProvider : IRepositoryProvider
{
    public string ProviderName => "GitLab";
    
    // Implement interface methods...
}
```

## Contributing

Contributions are welcome! Please:
1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## License

[Your License Here]

## Acknowledgments

- [Octokit](https://github.com/octokit/octokit.net) - GitHub API client for .NET
- [Serilog](https://serilog.net/) - Flexible logging library
