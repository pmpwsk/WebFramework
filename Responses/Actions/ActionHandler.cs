namespace uwap.WebFramework.Responses.Actions;

/// <summary>
/// A delegate to handle UI action requests synchronously.
/// </summary>
public delegate void ActionHandler(Request req);

/// <summary>
/// A delegate to handle UI action requests asynchronously.
/// </summary>
public delegate Task ActionHandlerAsync(Request req);

public static class ActionHandlerConverter
{
    /// <summary>
    /// Converts the provided synchronous action handler to an asynchronous action handler.
    /// </summary>
    public static ActionHandlerAsync ToAsync(this ActionHandler handler)
        => req =>
        {
            handler(req);
            return Task.CompletedTask;
        };
}