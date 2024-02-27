namespace uwap.WebFramework;

/// <summary>
/// Intended for backend requests, either to path /api/... or routed over an app request.
/// </summary>
public class ApiRequest(LayerRequestData data) : SimpleResponseRequest(data)
{
}