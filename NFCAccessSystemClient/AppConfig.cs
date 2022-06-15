using System.Text.Json;
using System.Text.Json.Serialization;

namespace NFCAccessSystemClient;

public class AppConfig
{
    private ConfigFileModel _fileModel;

    public string KeyboardId { get; }
    public Uri ServerAddress { get; }
    public Uri LocalServerAddress { get; }
    public string DbAccessKey { get; }

    private class ConfigFileModel
    {
        [JsonPropertyName("keyboard_id")] public string KeyboardId { get; set; }
        [JsonPropertyName("server_address")] public string ServerAddress { get; set; }

        [JsonPropertyName("local_server_address")]
        public string LocalServerAddress { get; set; }

        [JsonPropertyName("db_access_key")] public string DbAccessKey { get; set; }
    }

    public AppConfig(string jsonConfigPath)
    {
        _fileModel = JsonSerializer.Deserialize<ConfigFileModel>(File.ReadAllText(jsonConfigPath))!;
        KeyboardId = _fileModel.KeyboardId;
        ServerAddress = new Uri(_fileModel.ServerAddress);
        LocalServerAddress = new Uri(_fileModel.LocalServerAddress);
        DbAccessKey = _fileModel.DbAccessKey;
    }
}