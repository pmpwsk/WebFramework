using uwap.Database;
using uwap.WebFramework.Accounts;
using uwap.WebFramework.Plugins;

namespace uwap.WebFramework;

public static partial class Server
{
    /// <summary>
    /// The timer that triggers the worker.
    /// </summary>
    private static Timer Worker = new Timer(Work, null, Timeout.Infinite, Timeout.Infinite);

    /// <summary>
    /// Whether the worker is working right now.
    /// </summary>
    public static bool WorkerWorking { get; private set; } = false;

    /// <summary>
    /// Whether the worker should work again immediately after the current run.
    /// </summary>
    private static bool WorkAgain = false;

    /// <summary>
    /// When the next worker run is scheduled (UTC date and time).
    /// </summary>
    public static DateTime WorkerNextTick { get; private set; } = DateTime.MaxValue;

    /// <summary>
    /// The method for the worker.<br/>
    /// Checks auto certificates and existing certificates, updates the cache, checks the database, deletes expired auth tokens and expired bans, checks accelerator dictionaries for user tables, then calls the worker-finished event.
    /// </summary>
    private async static void Work(object? state)
    {
        WorkerWorking = true;

        //renew certificates + delete unused ones
        if (Config.AutoCertificate.Email != null)
            try
            {
                await CheckAutoCertificates();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error renewing certificates: " + ex.Message);
            }

        //update certificates in store
        try
        {
            UpdateCertificates();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error updating the certificates: " + ex.Message);
        }

        //update file cache
        try
        {
            UpdateCache();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error updating the file cache: " + ex.Message);
        }

        //check database for errors
        try
        {
            Tables.CheckAll();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error checking the database for errors: " + ex.Message);
        }

        //account stuff
        if (Config.Accounts.Enabled)
        {
            try //delete expired tokens
            {
                foreach (var table in Config.Accounts.UserTables.Values.Distinct())
                    foreach (var kv in table)
                        kv.Value.Auth.DeleteExpired();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting expired auth tokens: " + ex.Message);
            }

            try
            {
                AccountManager.DeleteExpiredBans();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting expired bans: " + ex.Message);
            }

            try
            {
                foreach (var userTable in Config.Accounts.UserTables.Values.Distinct())
                    userTable.FixAccessories();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error checking and fixing accelerating dictionaries for registered user tables: " + ex.Message);
            }
        }

        //backup
        try
        {
            await Backup();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error running a backup: " + ex.Message);
        }

        //call plugins
        await PluginManager.Work();

        //fire event that worker finished
        try
        {
            if (WorkerWorked != null) await WorkerWorked();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error firing the event after the worker ran: " + ex.Message);
        }

        //set the timer again
        if (WorkAgain)
        {
            WorkAgain = false;
            Worker.Change(0, Timeout.Infinite);
        }
        else
        {
            if (Config.WorkerInterval > 0)
            {
                WorkerNextTick = DateTime.UtcNow + TimeSpan.FromMinutes(Config.WorkerInterval);
                Worker.Change(Config.WorkerInterval*60000, Timeout.Infinite);
            }
            else WorkerNextTick = DateTime.MaxValue;
            WorkerWorking = false;
        }
    }

    /// <summary>
    /// Requests the worker to work.<br/>
    /// If the worker is not running, it will run right away. If it is already running, it will run again once it's done.
    /// </summary>
    public static void Work()
    {
        if (WorkerWorking) WorkAgain = true;
        else Worker.Change(0, Timeout.Infinite);
    }
}