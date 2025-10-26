# Testing the Application

## Quick Test Commands

### Test 1: Display Help
```bash
dotnet run --project WorldDomination.CodeBackup.Console -- --help
```

Expected output: Shows application usage and available commands

### Test 2: Display Backup Command Help
```bash
dotnet run --project WorldDomination.CodeBackup.Console backup --help
```

Expected output: Shows all available options for the backup command

### Test 3: Backup Public Repos (No Auth)
```bash
dotnet run --project WorldDomination.CodeBackup.Console backup octocat
```

Expected behavior:
- Shows authentication status (public access)
- Lists found repositories
- Asks for confirmation
- Downloads repositories to ~/CodeBackup

### Test 4: Test with Token (Safe - Use Your Own)
```bash
dotnet run --project WorldDomination.CodeBackup.Console backup YOUR_USERNAME --token YOUR_TOKEN
```

Expected behavior:
- Successfully authenticates
- Shows all repositories (public + private)
- Downloads to default location

### Test 5: Test Filters
```bash
# Test private only (requires token)
dotnet run --project WorldDomination.CodeBackup.Console backup YOUR_USERNAME \
  --token YOUR_TOKEN \
  --private-only

# Test including archived
dotnet run --project WorldDomination.CodeBackup.Console backup YOUR_USERNAME \
  --include-archived

# Test custom directory
dotnet run --project WorldDomination.CodeBackup.Console backup YOUR_USERNAME \
  --directory ./test-backup
```

### Test 6: Test No Confirmation
```bash
dotnet run --project WorldDomination.CodeBackup.Console backup octocat \
  --no-confirm \
  --directory ./test-output
```

Expected behavior: Immediately starts downloading without prompting

## Verify Output

After running a backup, verify:

1. **Directory Created**: Check that the target directory exists
2. **Files Downloaded**: Verify .zip files are present
3. **File Names**: Should be in format `owner_repo.zip`
4. **File Contents**: Unzip a file to verify it contains the repository code
5. **Logs**: Check console output for any errors or warnings

## Manual Verification

```bash
# Navigate to backup directory
cd ~/CodeBackup

# List downloaded files
ls -lh

# Check file contents (unzip one)
unzip -l owner_repository.zip | head -20

# Verify it's a git repository
unzip owner_repository.zip
cd owner-repository-main  # or master
ls -la .git
```

## Testing Error Scenarios

### Invalid Username
```bash
dotnet run --project WorldDomination.CodeBackup.Console backup thisuserdoesnotexist12345
```

Expected: Should show error message

### Invalid Token
```bash
dotnet run --project WorldDomination.CodeBackup.Console backup YOUR_USERNAME --token invalid_token
```

Expected: Authentication should fail with clear error message

### Invalid Directory (No Write Permission)
```bash
# On Linux/macOS
dotnet run --project WorldDomination.CodeBackup.Console backup octocat --directory /root/backup

# On Windows
dotnet run --project WorldDomination.CodeBackup.Console backup octocat --directory C:\Windows\backup
```

Expected: Should show permission error

## Performance Testing

### Small Account (< 10 repos)
```bash
time dotnet run --project WorldDomination.CodeBackup.Console backup octocat --no-confirm
```

### Medium Account (10-50 repos)
Test with your own account or a user with moderate repos

### Large Account (> 100 repos)
**Warning**: This will take significant time and bandwidth!

## Integration Testing

### Test with CI/CD

Create a test script:

```bash
#!/bin/bash
set -e

echo "Testing code-backup..."

# Test 1: Help works
dotnet run --project WorldDomination.CodeBackup.Console -- --help > /dev/null
echo "✓ Help command works"

# Test 2: Backup command help works
dotnet run --project WorldDomination.CodeBackup.Console backup --help > /dev/null
echo "✓ Backup help works"

# Test 3: Actual backup (with test account)
TEST_DIR=$(mktemp -d)
dotnet run --project WorldDomination.CodeBackup.Console backup octocat \
  --directory "$TEST_DIR" \
  --no-confirm

# Verify files were created
FILE_COUNT=$(ls -1 "$TEST_DIR" | wc -l)
if [ "$FILE_COUNT" -gt 0 ]; then
  echo "✓ Backup created $FILE_COUNT files"
else
  echo "✗ No files created!"
  exit 1
fi

# Cleanup
rm -rf "$TEST_DIR"

echo "All tests passed!"
```

## Logging Tests

### Test Different Log Levels

The app currently logs at Information level and above. To test:

1. **Information**: Normal operations (green)
2. **Warning**: Expected in some scenarios
3. **Error**: Download failures, auth failures (red)
4. **Critical**: Unhandled exceptions (red, bold)

### Capture Logs

```bash
dotnet run --project WorldDomination.CodeBackup.Console backup octocat 2>&1 | tee backup.log
```

Then inspect `backup.log` for proper formatting and colors.

## Regression Testing Checklist

Before each release, verify:

- [ ] Help text displays correctly
- [ ] All command-line options work
- [ ] Authentication works (with valid token)
- [ ] Authentication fails gracefully (with invalid token)
- [ ] Public repo download works (no auth)
- [ ] Private repo download works (with auth)
- [ ] Filters work (private-only, include-archived)
- [ ] Custom directory works
- [ ] No-confirm flag works
- [ ] Files are properly named
- [ ] ZIP files contain repository content
- [ ] Logs display with proper colors
- [ ] Errors are handled gracefully
- [ ] Exit codes are correct (0 for success, 1 for failure)

## Known Limitations to Test

1. **Rate Limiting**: GitHub API has limits
   - Test: Try downloading > 60 repos without auth
   - Expected: May hit rate limit

2. **Large Repositories**: Very large repos may take time
   - Test: Try downloading a repo > 500MB
   - Expected: Should work but may be slow

3. **Network Issues**: Intermittent connectivity
   - Test: Disconnect network mid-download
   - Expected: Should fail gracefully with error message

4. **Disk Space**: Insufficient space
   - Test: Download to nearly-full partition
   - Expected: Should fail with clear error

## Debug Mode

To enable more verbose logging, modify `Program.cs`:

```csharp
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()  // Already set
    .WriteTo.SpectreConsole(
        "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        minLevel: Serilog.Events.LogEventLevel.Debug)  // Change to Debug
    .CreateLogger();
```

This will show all debug messages including API calls, file operations, etc.
