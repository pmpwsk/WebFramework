using uwap.Database;

namespace uwap.WebFramework;

public static partial class Server
{
    public static partial class Layers
    {
        private const string DatabaseLayerPrefix = "/wf/db";
        
        public static async Task<bool> DatabaseLayer(LayerRequestData data)
            => data.Path.StartsWith(DatabaseLayerPrefix + '/')
               && await Handle(data, DatabaseLayerHandler);
        
        private static AbstractTable ValidateTableQuery(Request req, ClusterNode node, bool readingRequest)
        {
            if (!(req.Query.TryGetValue("table", out var tableName) && req.Query.TryGetValue(readingRequest ? "version" : "min-version", out var versionString) && Version.TryParse(versionString, out var version)))
                throw new BadRequestSignal();
            if (node.TableNames != null && !node.TableNames.Contains(tableName))
                throw new ForbiddenSignal();
            if (!Tables.Dictionary.TryGetValue(tableName, out var table))
                throw new NotFoundSignal();
            if (readingRequest ? version < table.GetMinVersion() : table.GetTypeVersion() < version)
                throw new TeapotSignal();
            return table;
        }

        private static async Task<bool> DatabaseLayerHandler(Request req)
        {
            var path = req.Path[DatabaseLayerPrefix.Length..];

            if (path == "/node-id")
            {
                await req.Write(Tables.NodeId);
                return true;
            }

            var cert = await req.GetClientCertificate();
            if (cert == null)
                throw new NotAuthenticatedSignal();

            var node = req.Query.TryGetValue("host", out var host) ? Config.Database.Cluster.FirstOrDefault(n => n.Host == host) : null;
            if (node == null || !node.CertificateValidators.Any(v => v.Validate(cert, node.Host.Before(':'))))
                throw new ForbiddenSignal();

            switch (path)
            {
                case "/state":
                {
                    req.ForceGET();
                    
                    var table = ValidateTableQuery(req, node, true);
                    
                    var result = table.GetState();
                    await req.WriteBytes(Serialization.Serialize(result));
                } break;
                
                case "/entry":
                {
                    req.ForceGET();
                    
                    var table = ValidateTableQuery(req, node, true);
                    if (!req.Query.TryGetValue("id", out var id))
                        throw new BadRequestSignal();
                    if (!table.TryGetAbstractEntry(id, out var entry))
                        throw new NotFoundSignal();
                    
                    var result = entry.GetBytes();
                    await req.WriteBytes(result);
                } break;
                
                case "/file":
                {
                    req.ForceGET();
                    
                    var table = ValidateTableQuery(req, node, true);
                    if (!(req.Query.TryGetValue("id", out var id) && req.Query.TryGetValue("file", out var fileId)))
                        throw new BadRequestSignal();
                    if (!(table.TryGetAbstractEntry(id, out var entry) && entry.EntryInfo.Files.ContainsKey(fileId)))
                        throw new NotFoundSignal();
                    
                    var result = await entry.GetFileBytes(fileId);
                    await req.WriteBytes(result);
                } break;
                
                case "/change":
                {
                    req.ForcePOST();
                    
                    var table = ValidateTableQuery(req, node, false);
                    if (!(req.Query.TryGetValue("id", out var id) && req.Query.TryGetValue("timestamp", out long timestamp) && req.Query.TryGetValue("randomness", out var randomness)))
                        throw new BadRequestSignal();
                    
                    req.BodySizeLimit = long.MaxValue;
                    var serialized = await req.GetBodyBytes();
                    
                    _ = Task.Run(() => //don't make the sender wait
                    {
                        try
                        {
                            table.UpdateEntry(node, id, serialized);
                        }
                        catch { }
                        
                        if (table.TryGetAbstractEntry(id, out var entry))
                            LockRequest.Delete(entry, timestamp, randomness);
                    });
                } break;
                
                case "/lock":
                {
                    req.ForceGET();
                    
                    var table = ValidateTableQuery(req, node, false);
                    if (!(req.Query.TryGetValue("id", out var id) && req.Query.TryGetValue("timestamp", out long timestamp) && req.Query.TryGetValue("randomness", out var randomness)))
                        throw new BadRequestSignal();
                    if (!table.TryGetAbstractEntry(id, out var entry))
                        throw new NotFoundSignal();
                    
                    LockRequest.CreateRemote(entry, timestamp, randomness);
                    await req.Write(string.Join('&', entry.LockRequests.Select(lockReq => $"{lockReq.Timestamp};{lockReq.Randomness}")));
                } break;
                
                case "/cancel":
                {
                    req.ForceGET();
                    
                    var table = ValidateTableQuery(req, node, false);
                    if (!(req.Query.TryGetValue("id", out var id) && req.Query.TryGetValue("timestamp", out long timestamp) && req.Query.TryGetValue("randomness", out var randomness)))
                        throw new BadRequestSignal();
                    if (!table.TryGetAbstractEntry(id, out var entry))
                        throw new NotFoundSignal();
                    
                    LockRequest.Delete(entry, timestamp, randomness);
                    await req.Write("ok");
                } break;
                
                case "/keep-alive":
                {
                    req.ForceGET();
                    await req.KeepEventAlive();
                } break;
            }

            return true;
        }
    }
}