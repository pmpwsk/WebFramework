using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.Actions;

public class ForcedActionReload : AbstractForcedActionHandler
{
    public override void Handle(AbstractWatchablePage page)
        => page.Reload();
}