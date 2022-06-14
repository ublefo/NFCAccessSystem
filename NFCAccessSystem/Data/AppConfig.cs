using System.Text.Json;
using System.Text.Json.Serialization;

namespace NFCAccessSystem.Data;

public class AppConfig
{
    private ConfigFileModel _fileModel;

    public bool IsClient { get; }
    public string DbPath { get; }
    public bool DbReadOnly { get; }
    public string DbAccessKey { get; }

    private class ConfigFileModel
    {
        [JsonPropertyName("is_client")] public bool IsClient { get; set; }
        [JsonPropertyName("db_path")] public string DbPath { get; set; }
        [JsonPropertyName("db_access_key")] public string DbAccessKey { get; set; }
    }

    public AppConfig(string jsonConfigPath)
    {
        _fileModel = JsonSerializer.Deserialize<ConfigFileModel>(File.ReadAllText(jsonConfigPath))!;
        IsClient = _fileModel.IsClient;
        // client mode will not write to the db
        DbReadOnly = IsClient;
        DbPath = _fileModel.DbPath;
        DbAccessKey = _fileModel.DbAccessKey;
    }
}