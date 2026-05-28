using System;
using System.IO;
using System.Text.Json;
using RymoraLandOfHeroes.Core.Data;

namespace RymoraLandOfHeroes.GodotAdapter.Data;

public sealed class JsonSaveStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly string _path;

    public JsonSaveStore(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _path = path;
    }

    public SaveData? TryLoad()
    {
        if (!File.Exists(_path))
        {
            return null;
        }

        var json = File.ReadAllText(_path);
        var save = JsonSerializer.Deserialize<SaveData>(json, Options)
            ?? throw new InvalidOperationException($"Save file is empty or invalid: {_path}.");
        SaveValidation.Validate(save);
        return save;
    }

    public void Save(SaveData save)
    {
        SaveValidation.Validate(save);
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var tempPath = $"{_path}.tmp";
        File.WriteAllText(tempPath, JsonSerializer.Serialize(save, Options));
        if (File.Exists(_path))
        {
            File.Replace(tempPath, _path, destinationBackupFileName: null);
            return;
        }

        File.Move(tempPath, _path);
    }
}
