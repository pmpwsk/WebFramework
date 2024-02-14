namespace uwap.WebFramework;

public static partial class Server
{
    public static bool BackupRunning { get; private set; } = false;
    private static async Task Backup()
    {
        if (!Config.Backup.Enabled)
            return;
        if (BackupRunning)
            throw new Exception("A backup is already running!");
        BackupRunning = true;
        BackupRunning = false;
    }
}