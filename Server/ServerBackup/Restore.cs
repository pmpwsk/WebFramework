﻿using System.Collections.ObjectModel;
using uwap.Database;
using uwap.WebFramework.Plugins;

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
        RestoreRunning = true;

        try
        {
            //find ids
            ReadOnlyCollection<string>? ids = null;
            string lastInChain = id;
            List<string> idsWritable = [];
            while (File.Exists($"{Config.Backup.Directory}{lastInChain}/BasedOn.txt"))
            {
                idsWritable.Insert(0, lastInChain);
                lastInChain = File.ReadAllText($"{Config.Backup.Directory}{lastInChain}/BasedOn.txt");
                if (lastInChain == "-")
                {
                    //start of the chain has been reached
                    ids = idsWritable.AsReadOnly();
                    break;
                }
            }
            if (ids == null)
                throw new Exception("The chain of backups is corrupt!");

            //tables
            Tables.RestoreAll(ids);

            //plugin
            await PluginManager.Restore(ids);

            //event
            try
            {
                if (RestoreAlmostDone != null) await RestoreAlmostDone(ids);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error firing the restore event: " + ex.Message);
            }
        }
        finally
        {
            RestoreRunning = false;
        }
    }
}