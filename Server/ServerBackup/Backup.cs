using System.Collections.ObjectModel;
using uwap.WebFramework.Database;
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

        try
        {
            //tables
            LegacyTables.BackupAll(id, basedOnIds);
            await Tables.BackupAllAsync(id, basedOnIds.LastOrDefault());

            //plugins
            await PluginManager.Backup(id, basedOnIds);

            //event
            await BackupAlmostDone.InvokeWithAsyncCaller
            (
                s => s(id, basedOnIds),
                ex => Console.WriteLine("Error firing a backup event: " + ex.Message),
                false
            );

            //finish
            await File.WriteAllTextAsync($"{Config.Backup.Directory}{id}/BasedOn.txt", basedOnIds.LastOrDefault() ?? "-");
        }
        finally
        {
            BackupRunning = false;
        }
    }

    private static bool BackupNecessary(out string id, out ReadOnlyCollection<string> basedOnIds, bool allowOutOfSchedule, bool forceFresh)
    {
        DateTime dt = DateTime.UtcNow;
        id = dt.Ticks.ToString();

        //find the last actual backup, delete backups that haven't been finished for some reason
        SortedSet<DateTime> ids = [];
        foreach (var d in new DirectoryInfo(Config.Backup.Directory).GetDirectories("*", SearchOption.TopDirectoryOnly))
            if (long.TryParse(d.Name, out long dL))
            {
                if (!File.Exists($"{Config.Backup.Directory}{d.Name}/BasedOn.txt"))
                {
                    Directory.Delete($"{Config.Backup.Directory}{d.Name}", true);
                    continue;
                }
                ids.Add(new DateTime(dL));
            }

        //is no backup present?
        if (ids.Count == 0)
        {
            basedOnIds = new([]);
            return true; //fresh
        }
        DateTime last = ids.Max;

        //delete old backups
        /**ignore ones that are:
         * up to two weeks old
         * fresh AND up to two months old
         * the first of a month AND up to two years old
         * the first of a year*/
        int currentYear = 0;
        int currentMonth = 0;
        foreach (DateTime d in ids) //keep in mind that they are sorted, that's why currentYear/Month works
        {
            TimeSpan age = dt - d;
            
            if (!((age <= TimeSpan.FromDays(14))
                || (File.ReadAllText($"{Config.Backup.Directory}{d.Ticks}/BasedOn.txt") == "-"
                    && (age <= TimeSpan.FromDays(60)
                    || (age <= TimeSpan.FromDays(730) && currentMonth != d.Month)
                    || currentYear != d.Year))))
                Directory.Delete($"{Config.Backup.Directory}{d.Ticks}", true);
            
            currentYear = d.Year;
            currentMonth = d.Month;
        }

        //is the last backup more than a week old?
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

        string lastInChain = last.Ticks.ToString();
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