namespace uwap.WebFramework;

public class RedirectSignal(string location, bool permanent = false) : Exception
{
    public readonly string Location = location;

    public readonly bool Permanent = permanent;
}

public class RedirectToLoginSignal() : RedirectSignal(Server.Config.Accounts.LoginPath);