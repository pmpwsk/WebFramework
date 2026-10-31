using System.Web;
using uwap.WebFramework.Database;
using uwap.WebFramework.Responses;
using uwap.WebFramework.Responses.Actions;
using uwap.WebFramework.Responses.DefaultUI;
using uwap.WebFramework.Responses.Dynamic;
using JsonSerializer = System.Text.Json.JsonSerializer;

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
                case "/ping":
                    return new TextResponse("pong");
                
                case "/watcher":
                {
                    var url = req.Query.GetOrThrow("url");
                    
                    var otherResponse = await GetOtherResponseAsync(req, url);
                    if (otherResponse is not Page page)
                    {
                        ServerActions.AbstractAction action = otherResponse is RedirectResponse redirectResponse
                            ? new ServerActions.Navigate(redirectResponse.Location)
                            : new ServerActions.BrowserReload();
                        
                        otherResponse.Dispose();
                        var otherSocket = new SocketResponse();
                        await otherSocket.ConnectionOpened.RegisterAsync(async () =>
                        {
                            await otherSocket.SendJsonAsync(
                                action
                            );
                        });
                        return otherSocket;
                    }
                    
                    var watcher = new ChangeWatcher();
                    page.ChangeWatcher = watcher;
                    
                    var response = new SocketResponse();
                    await response.ConnectionClosed.RegisterAsync(() =>
                    {
                        watcher.Socket = null;
                        page.Dispose();
                    });
                    watcher.Socket = response;
                    await response.ConnectionOpened.RegisterAsync(() =>
                    {
                        watcher.WritePage(page);
                    });
                    await response.MessageReceived.RegisterAsync(async (data, isText) =>
                    {
                        if (!isText)
                            return;
                        
                        switch (Serializers.Json.DeserializeNullable<ClientActions.AbstractAction>(data.Span))
                        {
                            case ClientActions.Submit submit:
                                await HandleDynamicPageSubmit(watcher, req, page, submit);
                                break;
                        }
                    });
                    return response;
                }
                
                default:
                    return StatusResponse.NotFound;
            }
        }
        
        private static async Task HandleDynamicPageSubmit(ChangeWatcher watcher, Request req, Page page, ClientActions.Submit submit)
        {
            var element = page.FindByPath(submit.Path);
            if (element is not IActionHaver actionHaver)
            {
                page.BrowserAlert("Error: Action target is invalid");
                return;
            }
            if (element.HasActionIgnoringParent)
                return;
            var action = actionHaver.Action;
            
            foreach (var entry in submit.Data)
            {
                var formElement = page.FindByPath(entry.Path);
                if (formElement is not AbstractInput formInput)
                {
                    page.BrowserAlert("Error: Form input is invalid");
                    return;
                }
                
                formInput.SetValueFromForm(entry.Value ?? "");
            }
            
            if (page.Request.BlockBanned && req.IsBanned)
                page.Reload();
            else
            {
                try
                {
                    await action(req);
                }
                catch (AbstractForcedActionHandler forcedHandler)
                {
                    forcedHandler.Handle(page);
                }
                catch (Exception ex)
                {
                    page.BrowserAlert($"Error: Action failed ({ex.Message})");
                }
                if (page.Request.BlockBanned && req.IsBanned)
                    page.Reload();
                watcher.FinishAction();
            }
        }
    }
}