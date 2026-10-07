using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.Actions;

public class ForcedActionNavigate(string location) : AbstractForcedActionHandler
{
    public readonly string Location = location;
    
    public override void Handle(AbstractWatchablePage page)
        => page.Navigate(Location);
}