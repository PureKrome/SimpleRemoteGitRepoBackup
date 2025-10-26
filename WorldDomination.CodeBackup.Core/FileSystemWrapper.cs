namespace WorldDomination.SimpleRemoteGitRepoBackup.Core;

public class FileSystemWrapper : IFileSystem
{
    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }

    public async Task WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default)
    {
        await File.WriteAllBytesAsync(path, bytes, cancellationToken);
    }

    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public long GetFileSize(string path)
    {
        var fileInfo = new FileInfo(path);
        return fileInfo.Length;
    }
}
