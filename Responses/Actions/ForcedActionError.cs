using uwap.WebFramework.Responses.DefaultUI;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.Actions;

public class ForcedActionError(params string[] messages) : AbstractForcedActionHandler
{
    public readonly string[] Messages = messages;
    
    public override Task HandleAsync(AbstractWatchablePage page)
    {
        if (page is Page defaultPage)
        {
            DialogBuilder.Error(defaultPage, Messages);
            return Task.CompletedTask;
        }
        else
            throw new Exception("Unable to handle page of type: " + page.GetType().Name);
    }
}