namespace uwap.WebFramework.Responses.Dynamic;

/// <summary>
/// A class for required attributes that are being watched for changes.
/// </summary>
public class RequiredWatchedAttribute(WatchedElement parent, string name, string value) : AbstractWatchedAttribute(parent, name)
{
    private string _Value = value;
    
    /// <summary>
    /// The actual value.
    /// </summary>
    public string Value
    {
        get => _Value;
        set
        {
            _Value = value;
            Parent.ChangeWatcher?.AttributeChanged(Parent, Name);
        }
    }
    
    /// <summary>
    /// Sets the stored value without applying it to the watcher.
    /// </summary>
    public void SetValueWithoutNotifying(string value)
        => _Value = value;

    public override (string Name, string? Value) Build()
        => (Name, _Value);
}