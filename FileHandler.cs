namespace uwap.WebFramework;

public static class SystemFiles
{
    public static byte[]? GetFile(string relPath, string pathPrefix, string domain)
        => relPath switch
        {
            "/default-ui-layout.css" => (byte[]?)PackedFiles_ResourceManager.GetObject("File0"),
            "/default-ui-theme.css" => (byte[]?)PackedFiles_ResourceManager.GetObject("File1"),
            "/default-ui.js" => (byte[]?)PackedFiles_ResourceManager.GetObject("File2"),
            "/fonts/roboto-mono.eot" => (byte[]?)PackedFiles_ResourceManager.GetObject("File3"),
            "/fonts/roboto-mono.otf" => (byte[]?)PackedFiles_ResourceManager.GetObject("File4"),
            "/fonts/roboto-mono.svg" => (byte[]?)PackedFiles_ResourceManager.GetObject("File5"),
            "/fonts/roboto-mono.ttf" => (byte[]?)PackedFiles_ResourceManager.GetObject("File6"),
            "/fonts/roboto-mono.woff" => (byte[]?)PackedFiles_ResourceManager.GetObject("File7"),
            "/fonts/roboto-mono.woff2" => (byte[]?)PackedFiles_ResourceManager.GetObject("File8"),
            "/fonts/roboto.eot" => (byte[]?)PackedFiles_ResourceManager.GetObject("File9"),
            "/fonts/roboto.otf" => (byte[]?)PackedFiles_ResourceManager.GetObject("File10"),
            "/fonts/roboto.svg" => (byte[]?)PackedFiles_ResourceManager.GetObject("File11"),
            "/fonts/roboto.ttf" => (byte[]?)PackedFiles_ResourceManager.GetObject("File12"),
            "/fonts/roboto.woff" => (byte[]?)PackedFiles_ResourceManager.GetObject("File13"),
            "/fonts/roboto.woff2" => (byte[]?)PackedFiles_ResourceManager.GetObject("File14"),
            _ => null
        };
    
    public static string? GetFileVersion(string relPath)
        => relPath switch
        {
            "/default-ui-layout.css" => "639001009289181620",
            "/default-ui-theme.css" => "639001009358571205",
            "/default-ui.js" => "639010782958438230",
            "/fonts/roboto-mono.eot" => "638993200608997332",
            "/fonts/roboto-mono.otf" => "638993200609177334",
            "/fonts/roboto-mono.svg" => "638993200584362885",
            "/fonts/roboto-mono.ttf" => "638993200609387335",
            "/fonts/roboto-mono.woff" => "638993200609587336",
            "/fonts/roboto-mono.woff2" => "638993200609817338",
            "/fonts/roboto.eot" => "638993200607766801",
            "/fonts/roboto.otf" => "638993200608022917",
            "/fonts/roboto.svg" => "638993200584167170",
            "/fonts/roboto.ttf" => "638993200608347328",
            "/fonts/roboto.woff" => "638993200608537329",
            "/fonts/roboto.woff2" => "638993200608737331",
            _ => null
        };
    
    private static readonly System.Resources.ResourceManager PackedFiles_ResourceManager = new("WebFramework.Properties.PackedFiles", typeof(SystemFiles).Assembly);
}
