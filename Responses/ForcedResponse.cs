namespace uwap.WebFramework.Responses;

/// <summary>
/// An exception that aborts the current request handling to forcefully return the given response.
/// </summary>
public class ForcedResponse(IResponse response) : Exception
{
    public readonly IResponse Response = response;
}