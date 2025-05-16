namespace Latihan_dotnet.Config;


public class AppSettings
{
    public static AppSettings Instance {get;set;} = new();

    public AppSettings()
    {
        Instance = this;
    }

    public ConnectionStringsConfig ConnectionStrings {get;set;}
}

public class ConnectionStringsConfig
{
    public string Default {get;set;} = string.Empty;
}