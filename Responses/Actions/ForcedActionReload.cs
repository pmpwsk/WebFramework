using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.Actions;

public class ForcedActionReload : AbstractForcedActionHandler
{
    public override Task HandleAsync(AbstractWatchablePage page)
        => page.ReloadAsync();
}