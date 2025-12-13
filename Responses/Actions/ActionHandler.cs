namespace uwap.WebFramework.Responses.Actions;

/// <summary>
/// A delegate to handle UI action requests.
/// </summary>
public delegate Task<IActionResponse> ActionHandler(Request req);