using System.Collections.ObjectModel;
﻿namespace uwap.WebFramework;

public static partial class Server
{
    public static bool BackupRunning { get; private set; } = false;
    private static async Task Backup()
    {
        if (!Config.Backup.Enabled)
            return;
        if (!BackupNecessary(out string id, out var basedOnIds))
            return;
        if (BackupRunning)
            throw new Exception("A backup is already running!");
        BackupRunning = true;
        BackupRunning = false;
    }

    private static bool BackupNecessary(out string id, out ReadOnlyCollection<string> basedOnIds)
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
        DateTime last = new(lastTicks);

        //is a backup present or is the last backup more than a week old?
        if (lastTicks != -1 || dt - last > TimeSpan.FromDays(7))
        {
            basedOnIds = new([]);
            return true; //fresh
        }

        //calculate the last scheduled backup
        DateTime scheduled = dt.Date + Config.Backup.Time;
        if (scheduled > dt)
            scheduled = scheduled.AddDays(-1);

        //has the scheduled backup already been created?
        if (last >= scheduled)
        {
            basedOnIds = new([]);
            return false; //no
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