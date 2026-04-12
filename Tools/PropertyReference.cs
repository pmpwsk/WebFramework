namespace uwap.WebFramework.Tools;

/// <summary>
/// Returns a reference to a property based on a given object.
/// </summary>
public delegate ref T PropertyReference<in C, T>(C obj);