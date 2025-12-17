using System.Text.Json;
using System.Web;
using uwap.WebFramework.Responses;
using uwap.WebFramework.Responses.Actions;
using uwap.WebFramework.Responses.DefaultUI;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework;

public static partial class Server
{
    public static partial class Layers
    {
        private const string DynamicPageLayerPrefix = "/wf/dyn";
        
        public static async Task<IResponse?> DynamicPageLayer(Request req)
        {
            if (!req.Path.StartsWith(DynamicPageLayerPrefix + '/'))
                return null;
            
            var path = req.Path[DynamicPageLayerPrefix.Length..];
            
            switch (path)
            {
                case "/watcher":
                {
                    req.ForceGET();
                    var url = req.Query.GetOrThrow("url");
                    
                    var otherResponse = await GetOtherResponseAsync(req, url);
                    if (otherResponse is not Page page)
                        return StatusResponse.NotFound;
                    var watcher = WatcherManager.CreateWatcher(page);
                    
                    var response = new EventResponse();
                    await response.KeepEventAliveCancelled.RegisterAsync((_, _) =>
                    {
                        watcher.EventResponse = null;
                        WatcherManager.DeleteWatcher(watcher);
                        return Task.CompletedTask;
                    });
                    watcher.EventResponse = response;
                    response.OnStart = () =>
                    {
                        watcher.Welcome();
                        watcher.WritePage(page);
                        return Task.CompletedTask;
                    };
                    return response;
                }

                case "/submit":
                {
                    req.ForcePOST();
                    if (req.Form == null)
                        return StatusResponse.BadRequest;
                    var id = req.Query.GetOrThrow("id");
                    if (!WatcherManager.TryGetPage(id, out var page))
                        return StatusResponse.NotFound;
                    var elemPathEncoded = req.Query.GetOrThrow("path");
                    var elemPath = JsonSerializer.Deserialize<string[]>(HttpUtility.UrlDecode(elemPathEncoded));
                    if (elemPath == null)
                        return StatusResponse.BadRequest;
                    var element = page.FindByPath(elemPath);
                    ActionHandler action;
                    switch (element)
                    {
                        case null:
                            return StatusResponse.NotFound;
                        case IActionHaver actionHaver:
                            action = actionHaver.Action;
                            break;
                        default:
                            return StatusResponse.BadRequest;
                    }
                    
                    foreach (var (encodedKey, stringValues) in req.Form.Data)
                    {
                        var key = Uri.UnescapeDataString(encodedKey);
                        var value = (string?)stringValues;
                        
                        var formElemPath = JsonSerializer.Deserialize<string[]>(HttpUtility.UrlDecode(key));
                        if (formElemPath == null)
                            return StatusResponse.BadRequest;
                        
                        var formElement = page.FindByPath(formElemPath);
                        if (formElement is not AbstractInput formInput)
                            return StatusResponse.BadRequest;
                        
                        formInput.SetValueFromForm(value ?? "");
                    }
                    
                    return new TextResponse(JsonSerializer.Serialize((await action(req)).Generate(req)));
                }
                
                default:
                    return StatusResponse.NotFound;
            }
        }
    }
}