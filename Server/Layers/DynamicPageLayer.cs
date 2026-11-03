using uwap.WebFramework.Database;
using uwap.WebFramework.Localization;
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
                case "/ping":
                    return new TextResponse("pong");
                
                case "/watcher":
                {
                    var url = req.Query.GetOrThrow("url");
                    
                    var response = new SocketResponse();
                    var watcher = new ChangeWatcher(req, url, response);
                    
                    await response.ConnectionClosed.RegisterAsync(watcher.Dispose);
                    await response.ConnectionOpened.RegisterAsync(watcher.LoadAsync);
                    await response.MessageReceived.RegisterAsync((data, isText) =>
                    {
                        if (isText)
                        {
                            var action = Serializers.Json.DeserializeNullable<ClientActions.AbstractAction>(data.Span);
                            _ = Task.Run(async () =>
                            {
                                switch (action)
                                {
                                    case ClientActions.Submit submit:
                                        await HandleDynamicPageSubmit(watcher, submit);
                                        break;
                                    case ClientActions.CookieApplied cookieApplied:
                                        await DynamicCookieActionManager.CloseAsync(cookieApplied.Id);
                                        break;
                                }
                            });
                        }
                        return Task.CompletedTask;
                    });
                    return response;
                }
                
                case "/cookies":
                {
                    var id = req.Query.GetOrThrow("id");
                    if (await DynamicCookieActionManager.ApplyAsync(id, req))
                        return new TextResponse("ok");
                    else
                        return StatusResponse.NotFound;
                }
                
                default:
                    return StatusResponse.NotFound;
            }
        }
        
        private static async Task HandleDynamicPageSubmit(ChangeWatcher watcher, ClientActions.Submit submit)
        {
            var page = watcher.Page ?? throw new Exception("No page is present.");
            var req = watcher.Request;
            var element = page.FindByPath(submit.Path);
            if (element is not IActionHaver actionHaver)
            {
                await page.BrowserAlertAsync("Error: Action target is invalid");
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
                    await page.BrowserAlertAsync("Error: Form input is invalid");
                    return;
                }
                
                formInput.SetValueFromForm(entry.Value ?? "");
            }
            
            if (page is Page defaultPage
                && !actionHaver.IgnoresRequired
                && element.TryGetParent<WatchedElement>(e => e.RenderedTag == "form", out var form)
                && form.TryGetFirstChild<TextBox>(t => t.Required && t.IsEmpty, out _))
            {
                DialogBuilder.Error(defaultPage, CommonTexts.EmptyFields);
                await watcher.FinishActionAsync();
                return;
            }
            
            if (req.BlockBanned && req.IsBanned)
                await watcher.ReloadAsync();
            else
            {
                try
                {
                    await action(req);
                }
                catch (AbstractForcedActionHandler forcedHandler)
                {
                    await forcedHandler.HandleAsync(page);
                }
                catch (Exception ex)
                {
                    await page.BrowserAlertAsync($"Error: Action failed ({ex.Message})");
                }
                if (req.BlockBanned && req.IsBanned)
                    await watcher.ReloadAsync();
                await watcher.FinishActionAsync();
            }
        }
    }
}