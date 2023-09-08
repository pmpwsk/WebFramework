namespace uwap.WebFramework;

public abstract class IPage
{
    public string Title = "Untitled";
    public List<IStyle> Styles = new List<IStyle>();
    public List<IScript> Scripts = new List<IScript>();

    public abstract string Export(AppRequest request);
}