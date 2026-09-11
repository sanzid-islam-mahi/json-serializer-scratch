namespace JsonSerializer;

public interface IJasonSerializer
{
    string Serialize(object? obj);
    T? Deserialize<T>(string json);
}
