namespace uwap.WebFramework;

/// <summary>
/// Delegate for a layer of the middleware, returns whether the request has been finished (the middleware will not continue if true was returned).
/// </summary>
public delegate Task<bool> LayerDelegate(LayerRequestData data);