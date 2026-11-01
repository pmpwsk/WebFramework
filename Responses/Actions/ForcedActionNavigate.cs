using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.Actions;

public class ForcedActionNavigate(string location) : AbstractForcedActionHandler
{
    public readonly string Location = location;
    
    public override Task HandleAsync(AbstractWatchablePage page)
        => page.NavigateAsync(Location);
}