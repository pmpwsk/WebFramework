using System.Collections.ObjectModel;
using uwap.Database;
using uwap.WebFramework.Plugins;

namespace uwap.WebFramework;

public static partial class Server
{
    public static bool BackupRunning { get; private set; } = false;

    public static async Task BackupNow(bool forceFresh = false)
        => await Backup(true, forceFresh);

    private static async Task Backup(bool allowOutOfSchedule, bool forceFresh)
    {
        if (!Config.Backup.Enabled)
            return;
        Directory.CreateDirectory(Config.Backup.Directory);
        if (!BackupNecessary(out string id, out var basedOnIds, allowOutOfSchedule, forceFresh))
            return;
        if (BackupRunning)
            throw new Exception("A backup is already running!");
        if (RestoreRunning)
            throw new Exception("A restore is already running!");
        BackupRunning = true;

        //tables
        Tables.BackupAll(id, basedOnIds);

        //plugins
        await PluginManager.Backup(id, basedOnIds);

        //event
        try
        {
            if (BackupAlmostDone != null) await BackupAlmostDone(id, basedOnIds);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error firing the backup event: " + ex.Message);
        }

        //finish
        File.WriteAllText($"{Config.Backup.Directory}{id}/BasedOn.txt", basedOnIds.LastOrDefault() ?? "-");
        BackupRunning = false;
    }

    private static bool BackupNecessary(out string id, out ReadOnlyCollection<string> basedOnIds, bool allowOutOfSchedule, bool forceFresh)
    {
        DateTime dt = DateTime.UtcNow;
        id = dt.Ticks.ToString();

        //find the last actual backup, delete backups that haven't been finished for some reason
        long lastTicks = -1;
        foreach (var d in new DirectoryInfo(Config.Backup.Directory).GetDirectories("*", SearchOption.TopDirectoryOnly))
            if (long.TryParse(d.Name, out long dL) && dL > lastTicks)
            {
                if (!File.Exists($"{Config.Backup.Directory}{d.Name}/BasedOn.txt"))
                {
                    Directory.Delete($"{Config.Backup.Directory}{d.Name}", true);
                    continue;
                }
                lastTicks = dL;
            }

        //is no backup present?
        if (lastTicks == -1)
        {
            basedOnIds = new([]);
            return true; //fresh
        }

        //is the last backup more than a week old?
        DateTime last = new(lastTicks);
        if (dt - last > TimeSpan.FromDays(7))
        {
            basedOnIds = new([]);
            return true; //fresh
        }

        //calculate the last scheduled backup
        DateTime scheduled = dt.Date + Config.Backup.Time;
        if (scheduled > dt)
            scheduled = scheduled.AddDays(-1);

        //has the scheduled backup already been created?
        if (last >= scheduled && !allowOutOfSchedule)
        {
            basedOnIds = new([]);
            return false; //no
        }

        //force a fresh backup?
        if (forceFresh)
        {
            basedOnIds = new([]);
            return true;
        }

        //calculate the last scheduled fresh backup
        int scheduledFreshOffset = 0 - ((int)dt.DayOfWeek + 7 - (int)Config.Backup.FreshDay) % 7;
        DateTime scheduledFresh = dt.AddDays(scheduledFreshOffset).Date + Config.Backup.Time;
        if (scheduledFresh > dt)
            scheduledFresh = scheduledFresh.AddDays(-7);

        //is a fresh backup in order?
        if (last < scheduledFresh)
        {
            basedOnIds = new([]);
            return true; //fresh
        }

        string lastInChain = lastTicks.ToString();
        List<string> basedOnIdsWritable = [];
        while (File.Exists($"{Config.Backup.Directory}{lastInChain}/BasedOn.txt"))
        {
            basedOnIdsWritable.Insert(0, lastInChain);
            lastInChain = File.ReadAllText($"{Config.Backup.Directory}{lastInChain}/BasedOn.txt");
            if (lastInChain == "-")
            {
                //start of the chain has been reached
                basedOnIds = basedOnIdsWritable.AsReadOnly();
                return true; //based
            }
        }

        //error in chain
        basedOnIds = new([]);
        return true; //fresh
    }
}