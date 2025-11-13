using System.Text;
using System.Text.Json;
using System.Web;
using uwap.WebFramework;
using uwap.WebFramework.Accounts.Certificates;

namespace uwap.Database;

/// <summary>
/// Describes a node in a WebFramework database cluster.
/// </summary>
public class ClusterNode(string host, List<string>? tableNames, List<ICertificateValidator> certificateValidators, bool isReachable)
{
    /// <summary>
    /// The node's hostname. This can be either an IP address or a domain name, and may include a port like <c>uwap.org:443</c>.
    /// </summary>
    public string Host = host;

    /// <summary>
    /// The tables that should be shared with this node or null to include all tables.
    /// </summary>
    public List<string>? TableNames = tableNames;
    
    /// <summary>
    /// The validator to use in order to validate this node's certificate (server and client certificate).
    /// </summary>
    public List<ICertificateValidator> CertificateValidators = certificateValidators;

    /// <summary>
    /// Whether this node can be used to pull missing and outdated data.
    /// </summary>
    public bool IsReachable = isReachable;
    
    /// <summary>
    /// Whether the node is currently connected.
    /// </summary>
    public bool IsConnected { get; private set; } = isReachable;
    
    /// <summary>
    /// The cancellation token source to cancel the connection monitor.
    /// </summary>
    private CancellationTokenSource ShutdownTokenSource = new();

    /// <summary>
    /// Identify this node as "self" if connections to it reach this program instance.
    /// </summary>
    internal async Task MarkSelf()
    {
        var id = await GetString("/node-id", TimeSpan.FromSeconds(3));
        if (id == Tables.NodeId)
        {
            IsReachable = false;
            Tables.Self = this;
            Console.WriteLine($"Identified self: {Host}");
        }
    }
    
    /// <summary>
    /// Keeps monitoring the connection state to this node.
    /// </summary>
    internal async Task MonitorConnection()
    {
        ShutdownTokenSource = new();
        
        while (!ShutdownTokenSource.IsCancellationRequested)
        {
            try
            {
                using var client = CreateRequestClient();
                using var request = new HttpRequestMessage(HttpMethod.Get, BuildPath("/keep-alive"));
                request.Headers.Accept.ParseAdd("text/event-stream");
                using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ShutdownTokenSource.Token);
                response.EnsureSuccessStatusCode();
                await using var stream = await response.Content.ReadAsStreamAsync();
                using var reader = new StreamReader(stream);
                
                OnConnected();
                
                while (!ShutdownTokenSource.IsCancellationRequested && !reader.EndOfStream)
                {
                    if (await reader.ReadLineAsync() == null)
                        break;
                }
            }
            catch { }
            
            if (IsConnected)
                await OnDisconnected();
            await Task.Delay(1000);
        }
    }
    
    /// <summary>
    /// Stops the connection monitor.
    /// </summary>
    internal void StopMonitoringConnection()
        => ShutdownTokenSource.Cancel();
    
    /// <summary>
    /// Action to call when the node is newly connected after some down-time, pulls updates from the node and marks the node as connected.
    /// </summary>
    private void OnConnected()
    {
        IsConnected = true;
        Console.WriteLine($"Connected to database node: {Host}");
        if (IsReachable)
            foreach (var table in Tables.Dictionary.Values.Where(table => TableNames == null || TableNames.Contains(table.Name)))
            {
                var state = PullState(table);
                if (state != null)
                    table.SyncFrom(this, state);
            }
    }
    
    /// <summary>
    /// Action to call when the node is newly disconnected after some up-time, marks the node as disconnected.
    /// </summary>
    /// <returns></returns>
    private Task OnDisconnected()
    {
        IsConnected = false;
        Console.WriteLine($"Disconnected from database node: {Host}");
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Creates a URL query part to specific the table name and version information depending on whether it's a reading or writing request.
    /// </summary>
    private static string TableQuery(AbstractTable table, bool readingRequest)
        => $"table={HttpUtility.UrlEncode(table.Name)}&{(readingRequest ? "" : "min-")}version={(readingRequest ? table.GetTypeVersion() : table.GetMinVersion()).ToString()}";
    
    /// <summary>
    /// Sends a lock request while receiving the node's list of existing lock requests.
    /// </summary>
    internal Task<string?> SendLockAsync(AbstractTable table, string id, long timestamp, string randomness)
        => GetString($"/lock?{TableQuery(table, false)}&id={HttpUtility.UrlEncode(id)}&timestamp={timestamp}&randomness={randomness}", Server.Config.Database.RequestTimeout);
    
    /// <summary>
    /// Requests to cancel a lock request.
    /// </summary>
    internal Task<string?> SendCancelAsync(AbstractTable table, string id, long timestamp, string randomness)
        => GetString($"/cancel?{TableQuery(table, false)}&id={HttpUtility.UrlEncode(id)}&timestamp={timestamp}&randomness={randomness}", Server.Config.Database.RequestTimeout);
    
    /// <summary>
    /// Pushes a table entry update along with the transaction's lock request.
    /// </summary>
    internal Task PushChangeAsync(AbstractTable table, string id, long timestamp, string randomness, byte[] serialized)
        => PostBytes($"/change?{TableQuery(table, false)}&id={HttpUtility.UrlEncode(id)}&timestamp={timestamp}&randomness={randomness}", serialized, Server.Config.Database.RequestTimeout * 4);
    
    /// <summary>
    /// Pulls the serialized value from an entry.
    /// </summary>
    internal byte[]? PullEntry(AbstractTable table, string id)
        => GetBytes($"/entry?{TableQuery(table, true)}&id={HttpUtility.UrlEncode(id)}").GetAwaiter().GetResult();
    
    /// <summary>
    /// Pulls the file contents of an attached file to the given path and returns whether the operation was successful.
    /// </summary>
    internal bool PullFile(AbstractTable table, string id, string fileId, string targetFilePath)
        => Download($"/file?{TableQuery(table, true)}&id={HttpUtility.UrlEncode(id)}&file={HttpUtility.UrlEncode(fileId)}", targetFilePath).GetAwaiter().GetResult();
    
    /// <summary>
    /// Pulls the state of a table.
    /// </summary>
    internal Dictionary<string, MinimalTableValue>? PullState(AbstractTable table)
        => PullStateAsync(table).GetAwaiter().GetResult();
    
    /// <summary>
    /// Pulls the state of a table asynchronously.
    /// </summary>
    private async Task<Dictionary<string, MinimalTableValue>?> PullStateAsync(AbstractTable table)
    {
        var serialized = await GetBytes($"/state?{TableQuery(table, true)}");
        if (serialized == null)
            return null;

        try
        {
            using var document = JsonDocument.Parse(serialized);
            Dictionary<string, MinimalTableValue> state = [];
            foreach (var kv in document.RootElement.EnumerateArray())
            {
                var id = kv.GetProperty("Key").GetString() ?? throw new Exception("Key-value pair without key found.");
                var serializedInfo = Encoding.UTF8.GetBytes(kv.GetProperty("Value").GetRawText());
                state[id] = Serialization.Deserialize<MinimalTableValue>(table.Name, id, serializedInfo) ?? throw new Exception($"Failed to deserialize entry with ID \"{id}\"");
            }
            return state;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to parse state from database node {Host}: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Pulls a string asynchronously.
    /// </summary>
    private async Task<string?> GetString(string path, TimeSpan? timeout = null)
    {
        if (!IsConnected)
            return null;
        try
        {
            using var client = CreateRequestClient();
            if (timeout == null)
            {
                return await client.GetStringAsync(BuildPath(path));
            }
            else
            {
                using var cts = new CancellationTokenSource();
                cts.CancelAfter(timeout.Value);
                return await client.GetStringAsync(BuildPath(path), cts.Token);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to database node {Host} ({path}): {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Sends a post request asynchronously.
    /// </summary>
    private async Task PostBytes(string path, byte[] body, TimeSpan? timeout = null)
    {
        if (!IsConnected)
            return;
        try
        {
            using var client = CreateRequestClient();
            if (timeout == null)
            {
                await client.PostAsync(BuildPath(path), new ByteArrayContent(body));
            }
            else
            {
                using var cts = new CancellationTokenSource();
                cts.CancelAfter(timeout.Value);
                await client.PostAsync(BuildPath(path), new ByteArrayContent(body), cts.Token);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to database node {Host} ({path}): {ex.Message}");
        }
    }
    
    /// <summary>
    /// Pulls a byte array asynchronously.
    /// </summary>
    private async Task<byte[]?> GetBytes(string path, TimeSpan? timeout = null)
    {
        if (!IsConnected)
            return null;
        try
        {
            using var client = CreateRequestClient();
            if (timeout == null)
            {
                return await client.GetByteArrayAsync(BuildPath(path));
            }
            else
            {
                using var cts = new CancellationTokenSource();
                cts.CancelAfter(timeout.Value);
                return await client.GetByteArrayAsync(BuildPath(path), cts.Token);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to database node {Host} ({path}): {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Downloads a file to the given path asynchronously.
    /// </summary>
    private async Task<bool> Download(string path, string targetFilePath)
    {
        if (!IsConnected)
            return false;
        try
        {
            using var client = CreateRequestClient();
            await using var responseStream = await client.GetStreamAsync(BuildPath(path));
            await using var fileStream = new FileStream(targetFilePath, FileMode.Create);
            await responseStream.CopyToAsync(fileStream);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to connect to database node {Host} ({path}): {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Builds a request URL for the given path.
    /// </summary>
    private string BuildPath(string path)
    {
        if (path != "/node-id")
            path = $"{path}{(path.Contains('?') ? '&' : '?')}host={HttpUtility.UrlEncode(Tables.Self.Host)}";
        return $"https://{Host}/wf/db{path}";
    }
    
    /// <summary>
    /// Creates an HTTP client with a client certificate.
    /// </summary>
    private HttpClient CreateRequestClient()
    {
        var clientCertificate = Server.GetCertificate(Server.Config.Database.CertificateDomain);
        if (clientCertificate == null)
            throw new Exception("No database certificate was mapped!");
        
        return new(new HttpClientHandler
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ClientCertificates = { clientCertificate },
            ServerCertificateCustomValidationCallback = (_, certificate, _, _) => certificate != null && CertificateValidators.Any(v => v.Validate(certificate, Host.Before(':')))
        });
    }
}