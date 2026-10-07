using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.Actions;

public abstract class AbstractForcedActionHandler : Exception
{
    /// <summary>
    /// Performs the reaction on the provided page.
    /// </summary>
    public abstract void Handle(AbstractWatchablePage page);
}