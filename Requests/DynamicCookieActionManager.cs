using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using uwap.WebFramework.Responses.Dynamic;
using uwap.WebFramework.Tools;

namespace uwap.WebFramework;

/// <summary>
/// Manages cookie actions for dynamic pages. 
/// </summary>
public static class DynamicCookieActionManager
{
    /// <summary>
    /// The time duration after which an entry expires.
    /// </summary>
    private static readonly TimeSpan Expiration = TimeSpan.FromMinutes(1);
    
    /// <summary>
    /// The unfinished entries.
    /// </summary>
    private static readonly ConcurrentDictionary<string, Entry> Entries = [];
    
    /// <summary>
    /// Creates a new cookie request, sends it to the watcher and waits for its confirmation or expiration.
    /// </summary>
    public static async Task OpenAndWaitAsync(ChangeWatcher watcher, Action<HttpContext> action)
    {
        using var entry = new Entry(action);
        var id = Parsers.RandomString(64, Entries.Keys);
        Entries[id] = entry;
        _ = Task.Run(async () =>
        {
            await Task.Delay(Expiration);
            await CloseAsync(id);
        });
        await watcher.CookieActionAsync(id);
        await entry.Waiter.WaitAsync();
    }
    
    /// <summary>
    /// Applies the cookie action with the provided ID to the request and returns ture if present, otherwise returns false.<br/>
    /// This doesn't close the cookie request because the client needs to confirm that separately, after the response was received.
    /// </summary>
    public static async Task<bool> ApplyAsync(string id, Request req)
    {
        if (Entries.TryGetValue(id, out var entry))
        {
            await req.Cookies.ApplyAsync(entry.Action);
            return true;
        }
        else
        {
            return false;            
        }
    }
    
    /// <summary>
    /// Closes and removes the cookie action with the provided ID if it exists.
    /// </summary>
    public static async Task CloseAsync(string id)
    {
        if (Entries.TryRemove(id, out var entry))
            await entry.Waiter.ReadyAsync();
    }
    
    /// <summary>
    /// Stores the action and waiting functionality for a cookie request.
    /// </summary>
    private class Entry(Action<HttpContext> action) : IDisposable
    {
        /// <summary>
        /// The action to perform on the requesting context.
        /// </summary>
        public readonly Action<HttpContext> Action = action;
        
        /// <summary>
        /// The waiter to pause the execution of the cookie action requester.
        /// </summary>
        public readonly ReadyWaiter Waiter = new();
        
        public void Dispose()
        {
            Waiter.Dispose();
        }
    }
}