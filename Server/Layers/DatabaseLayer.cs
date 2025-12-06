using uwap.WebFramework.Database;
using uwap.WebFramework.Responses;

namespace uwap.WebFramework;

public static partial class Server
{
    public static partial class Layers
    {
        private const string DatabaseLayerPrefix = "/wf/db";
        
        private static AbstractTable ValidateTableQuery(Request req, ClusterNode node, bool readingRequest)
        {
            if (!(req.Query.TryGetValue("table", out var tableName) && req.Query.TryGetValue(readingRequest ? "version" : "min-version", out var versionString) && Version.TryParse(versionString, out var version)))
                throw new ForcedResponse(StatusResponse.BadRequest);
            if (node.TableNames != null && !node.TableNames.Contains(tableName))
                throw new ForcedResponse(StatusResponse.Forbidden);
            if (!Tables.Dictionary.TryGetValue(tableName, out var table))
                throw new ForcedResponse(StatusResponse.NotFound);
            if (readingRequest ? version < table.GetMinVersion() : table.GetTypeVersion() < version)
                throw new ForcedResponse(StatusResponse.Teapot);
            return table;
        }
        
        public static async Task<IResponse?> DatabaseLayer(Request req)
        {
            if (!req.Path.StartsWith(DatabaseLayerPrefix + '/'))
                return null;
            
            var path = req.Path[DatabaseLayerPrefix.Length..];

            if (path == "/node-id")
                return new TextResponse(Tables.NodeId);

            var cert = await req.GetClientCertificate();
            if (cert == null)
                return StatusResponse.NotAuthenticated;

            var node = req.Query.TryGetValue("host", out var host) ? Config.Database.Cluster.FirstOrDefault(n => n.Host == host) : null;
            if (node == null || !node.CertificateValidators.Any(v => v.Validate(cert, node.Host.Before(':'))))
                return StatusResponse.Forbidden;

            switch (path)
            {
                case "/state":
                {
                    req.ForceGET();
                    
                    var table = ValidateTableQuery(req, node, true);
                    
                    var result = table.GetState();
                    return new ByteArrayResponse(Serialization.Serialize(result), null, false, null);
                }
                
                case "/entry":
                {
                    req.ForceGET();
                    
                    var table = ValidateTableQuery(req, node, true);
                    if (!req.Query.TryGetValue("id", out var id))
                        return StatusResponse.BadRequest;
                    if (!table.TryGetAbstractEntry(id, out var entry))
                        return StatusResponse.NotFound;
                    
                    var result = entry.GetBytes();
                    return new ByteArrayResponse(result, null, false, null);
                }
                
                case "/file":
                {
                    req.ForceGET();
                    
                    var table = ValidateTableQuery(req, node, true);
                    if (!(req.Query.TryGetValue("id", out var id) && req.Query.TryGetValue("file", out var fileId)))
                        return StatusResponse.BadRequest;
                    if (!(table.TryGetAbstractEntry(id, out var entry) && entry.EntryInfo.Files.ContainsKey(fileId)))
                        return StatusResponse.NotFound;
                    
                    var result = await entry.GetFileBytes(fileId);
                    return new ByteArrayResponse(result, null, false, null);
                }
                
                case "/change":
                {
                    req.ForcePOST();
                    
                    var table = ValidateTableQuery(req, node, false);
                    if (!(req.Query.TryGetValue("id", out var id) && req.Query.TryGetValue("timestamp", out long timestamp) && req.Query.TryGetValue("randomness", out var randomness)))
                        return StatusResponse.BadRequest;
                    
                    req.BodySizeLimit = long.MaxValue;
                    var serialized = await req.GetBodyBytes();
                    
                    _ = Task.Run(async () => //don't make the sender wait
                    {
                        try
                        {
                            await table.UpdateEntryAsync(node, id, serialized);
                        }
                        catch { }
                        
                        if (table.TryGetAbstractEntry(id, out var entry))
                            await LockRequest.DeleteAsync(entry, timestamp, randomness);
                    });
                    
                    return StatusResponse.Success;
                }
                
                case "/lock":
                {
                    req.ForceGET();
                    
                    var table = ValidateTableQuery(req, node, false);
                    if (!(req.Query.TryGetValue("id", out var id) && req.Query.TryGetValue("timestamp", out long timestamp) && req.Query.TryGetValue("randomness", out var randomness)))
                        return StatusResponse.BadRequest;
                    if (!table.TryGetAbstractEntry(id, out var entry))
                        return StatusResponse.NotFound;
                    
                    await LockRequest.CreateRemoteAsync(entry, timestamp, randomness);
                    return new TextResponse(string.Join('&', entry.LockRequests.Select(lockReq => $"{lockReq.Timestamp};{lockReq.Randomness}")));
                }
                
                case "/cancel":
                {
                    req.ForceGET();
                    
                    var table = ValidateTableQuery(req, node, false);
                    if (!(req.Query.TryGetValue("id", out var id) && req.Query.TryGetValue("timestamp", out long timestamp) && req.Query.TryGetValue("randomness", out var randomness)))
                        return StatusResponse.BadRequest;
                    if (!table.TryGetAbstractEntry(id, out var entry))
                        return StatusResponse.NotFound;
                    
                    await LockRequest.DeleteAsync(entry, timestamp, randomness);
                    return new TextResponse("ok");
                }
                
                case "/keep-alive":
                {
                    req.ForceGET();
                    return new EventResponse();
                }
                
                default:
                {
                    return StatusResponse.NotFound;
                }
            }
        }
    }
}