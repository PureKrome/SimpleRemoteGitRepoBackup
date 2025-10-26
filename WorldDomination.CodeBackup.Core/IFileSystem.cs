namespace WorldDomination.SimpleRemoteGitRepoBackup.Core;

public interface IFileSystem
{
    /// <summary>
    /// Creates all directories and subdirectories in the specified path unless they already exist.
    /// </summary>
    void CreateDirectory(string path);

    /// <summary>
    /// Asynchronously creates a new file, writes the specified byte array to the file, and then closes the file.
    /// </summary>
    Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default);

    /// <summary>
    /// Determines whether the given path refers to an existing file on disk.
    /// </summary>
    bool FileExists(string path);

    /// <summary>
    /// Gets the size of the file in bytes.
    /// </summary>
    long GetFileSize(string path);
}
