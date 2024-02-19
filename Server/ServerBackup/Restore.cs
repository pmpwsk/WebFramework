namespace uwap.WebFramework;

public static partial class Server
{
    public static bool RestoreRunning { get; private set; } = false;
    public static async Task Restore(string id)
    {
        if (BackupRunning)
            throw new Exception("A backup is already running!");
        if (RestoreRunning)
            throw new Exception("A restore is already running!");
    }
}