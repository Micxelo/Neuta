using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Neuta.Config;

public sealed class ConfigStore<T>(
    string path,
    JsonTypeInfo<T> typeInfo,
    Func<T> createDefault,
    Action<T> validate) where T : class
{
    private readonly string _path = Path.GetFullPath(path);
    private readonly JsonTypeInfo<T> _typeInfo = typeInfo;
    private readonly Func<T> _createDefault = createDefault;
    private readonly Action<T> _validate = validate;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private T? _current;

    public string FilePath => _path;

    public async Task<T> ReadAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            T current = _current ??= await LoadCoreAsync(cancellationToken);
            return Clone(current);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<T> ReloadAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken);
        try
        {
            T loaded = await LoadCoreAsync(cancellationToken);
            _current = loaded;
            return Clone(loaded);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<T> UpdateAsync(Action<T> edit, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(edit);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            T current = _current ??= await LoadCoreAsync(cancellationToken);
            T next = Clone(current);
            edit(next);
            _validate(next);
            await SaveCoreAsync(next, cancellationToken);
            _current = next;
            return Clone(next);
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task<T> ReplaceAsync(T next, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(next);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            T detached = Clone(next);
            _validate(detached);
            await SaveCoreAsync(detached, cancellationToken);
            _current = detached;
            return Clone(detached);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<T> LoadCoreAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_path))
        {
            T defaults = _createDefault();
            _validate(defaults);
            await SaveCoreAsync(defaults, cancellationToken);
            return defaults;
        }

        try
        {
            string json = await File.ReadAllTextAsync(_path, cancellationToken);
            T result = JsonSerializer.Deserialize(json, _typeInfo)
                ?? throw new JsonException("The root JSON value must not be null.");
            _validate(result);
            return result;
        }
        catch (JsonException ex)
        {
            throw new ConfigFileException($"Cannot parse configuration: {_path}", ex);
        }
    }

    private async Task SaveCoreAsync(T value, CancellationToken cancellationToken)
    {
        string directory = Path.GetDirectoryName(_path)!;
        Directory.CreateDirectory(directory);
        string tempPath = Path.Combine(directory, $".{Path.GetFileName(_path)}.{Guid.NewGuid():N}.tmp");

        try
        {
            string json = JsonSerializer.Serialize(value, _typeInfo);
            await File.WriteAllTextAsync(tempPath, json + "\n", cancellationToken);
            File.Move(tempPath, _path, overwrite: true);
        }
        finally
        {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
        }
    }

    private T Clone(T value)
    {
        string json = JsonSerializer.Serialize(value, _typeInfo);
        return JsonSerializer.Deserialize(json, _typeInfo)
            ?? throw new InvalidOperationException("Failed to clone a configuration.");
    }
}
