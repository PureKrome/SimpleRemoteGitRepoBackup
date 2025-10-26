using System.Text;
using Serilog;
using WorldDomination.SimpleRemoteGitRepoBackup.Console;


// Configure console to support UTF-8 output
Console.OutputEncoding = Encoding.UTF8;

// Configure Serilog with Spectre.Console sink
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .CreateLogger();

const string banner = @"
           _____ _                 _                                          
          / ____(_)               | |                                         
         | (___  _ _ __ ___  _ __ | | ___                                     
          \___ \| | '_ ` _ \| '_ \| |/ _ \                                    
          ____) | | | | | | | |_) | |  __/                                    
         |_____/|_|_| |_| |_| .__/|_|\___|                                    
                            | |                                               
  _____                     |_|         _____ _ _     _____                   
 |  __ \                    | |        / ____(_) |   |  __ \                  
 | |__) |___ _ __ ___   ___ | |_ ___  | |  __ _| |_  | |__) |___ _ __   ___   
 |  _  // _ \ '_ ` _ \ / _ \| __/ _ \ | | |_ | | __| |  _  // _ \ '_ \ / _ \  
 | | \ \  __/ | | | | | (_) | ||  __/ | |__| | | |_  | | \ \  __/ |_) | (_) | 
 |_|  \_\___|_|_|_| |_|\___/ \__\___|  \_____|_|\__| |_|  \_\___| .__/ \___/  
           |  _ \           | |                                 | |           
           | |_) | __ _  ___| | ___   _ _ __                    |_|           
           |  _ < / _` |/ __| |/ / | | | '_ \                                 
           | |_) | (_| | (__|   <| |_| | |_) |                                
           |____/ \__,_|\___|_|\_\\__,_| .__/                                 
                                       | |                                    
                                       |_|                                    
";

Log.Logger.Information(banner);
Log.Logger.Information("Starting application: {CurrentDateTime}", DateTime.Now);

try
{
    // Create service collection for dependency injection
    var services = new ServiceCollection();

    // Add logging
    services.AddLogging(builder =>
    {
        builder.AddSerilog(Log.Logger, dispose: true);
    });

    // Create service provider
    var serviceProvider = services.BuildServiceProvider();

    await Parser.Default.ParseArguments<CommandOptions>(args)
        .WithParsedAsync(async options =>
        {
            var scope = serviceProvider.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

            if (options.PrivateOnly)
            {
                logger.LogInformation("Including private repositories in the backup.");
            }

            if (options.IncludeArchived)
            {
                logger.LogInformation("Including archived repositories in the backup.");
            }

            IRepositoryProvider provider;


            if (options.Site.Equals("github", StringComparison.OrdinalIgnoreCase))
            {
                var githubLogger = scope.ServiceProvider.GetRequiredService<ILogger<GitHubRepositoryProvider>>();

                // If we don't provide any credentials for GH then we are limited
                // in accessing only public repositories and have very low rate limits.
                var credentials = string.IsNullOrWhiteSpace(options.Token)
                    ? null
                    : new Credentials(options.Token);

                if (credentials == null)
                {
                    logger.LogWarning("⚠️⚠️⚠️ No GitHub Personal Access Token provided. Only public repositories can be accessed, and rate limits will be low.");
                }

                var client = new GitHubClient(new ProductHeaderValue("CodeBackupApp"))
                {
                    Credentials = credentials
                };

                var fileSystem = new FileSystemWrapper();

                provider = new GitHubRepositoryProvider(client, fileSystem, githubLogger);
            }
            else
            {
                Log.Error("Unsupported site: {Site}. Currently, only 'github' is supported.", options.Site);
                return;
            }

            // **** LETS GO LETS GO!!!!

            var repositories = await provider.GetRepositoriesAsync(options.Username);
            logger.LogInformation("Found {RepositoryCount} repositories for user {Username}.", repositories.Count, options.Username);

            // Filter repositories based on options
            var repositoriesToBackup = repositories.Where(repository =>
            {
                if (repository.IsEmpty)
                {
                    return false;
                }

                if (repository.IsPrivate && !options.PrivateOnly)
                {
                    return false;
                }

                if (repository.IsArchived && !options.IncludeArchived)
                {
                    return false;
                }

                return true;
            }).ToList();

            var emptyRepositories = repositories.Count(r => r.IsEmpty);
            var privateRepositories = repositories.Count(r => r.IsPrivate);
            var publicRepositories = repositories.Count(r => !r.IsPrivate && !options.PrivateOnly);
            var archivedRepositories = repositories.Count(r => r.IsArchived && !options.IncludeArchived);

            logger.LogInformation("Found {Count} repositories (skipped {EmptyCount} empty, {PublicCount} public, {PrivateCount} private, {ArchivedCount} archived).",
                repositoriesToBackup.Count,
                emptyRepositories,
                publicRepositories,
                privateRepositories,
                archivedRepositories);

            logger.LogInformation("Backing up {Count} repositories with max {MaxConcurrent} concurrent downloads...",
                repositoriesToBackup.Count,
                options.MaxConcurrentDownloads);

            var successCount = 0;
            var failureCount = 0;

            // Create semaphore to limit concurrent downloads.
            var maxConcurrency = Math.Clamp(options.MaxConcurrentDownloads, 1, 10);
            using var semaphore = new SemaphoreSlim(maxConcurrency, maxConcurrency);

            // Create all download tasks
            var downloadTasks = repositoriesToBackup.Select(async (repository, index) =>
            {
                // Lets wait for a slot to be free.
                await semaphore.WaitAsync();

                try
                {
                    var currentIndex = index + 1;

                    logger.LogInformation("    [{CurrentIndex}/{Total}] Backing up repository: {RepoName}",
                        currentIndex,
                        repositoriesToBackup.Count,
                        repository.Name);

                    var targetDirectory = string.IsNullOrWhiteSpace(options.TargetDirectory)
                        ? Path.Combine(Directory.GetCurrentDirectory(), "GitRepoBackups-", options.Username)
                        : options.TargetDirectory!;

                    var downloadResult = await provider.DownloadRepositoryAsync(
                        options.Username,
                        repository.Name,
                        repository.DefaultBranch,
                        targetDirectory);

                    if (downloadResult)
                    {
                        Interlocked.Increment(ref successCount);
                        logger.LogInformation("✓ Successfully backed up repository: {RepositoryName}", repository.Name);
                    }
                    else
                    {
                        Interlocked.Increment(ref failureCount);
                        logger.LogError("✗ Failed to back up repository: {RepositoryName}", repository.Name);
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });

            // Wait for all downloads to complete
            await Task.WhenAll(downloadTasks);

            logger.LogInformation("🚀 Backup completed: {SuccessCount} successful, {FailureCount} failed.",
                 successCount,
                 failureCount);
        });
}
catch (Exception exception)
{
    Log.Fatal(exception, "Application terminated unexpectedly");
    Environment.Exit(1);
}
finally
{
    await Log.CloseAndFlushAsync();
}
