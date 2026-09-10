using System.Collections;
using System.Text;

namespace JsonSerializer;

public class MyJsonSerializer : IJasonSerializer
{
    public string Serialize(object? value)
    {
        if (value is null)
            return "null";

        var type = value.GetType();

        if (IsStringType(type))
            return "\"" + JsonString(value.ToString()!) + "\"";

        if (IsBooleanType(type))
            return value.ToString()!.ToLower();

        if (IsNumericType(type))
            return value.ToString()!;

        if (type.IsEnum)
            return "\"" + value + "\"";

        if (value is DateTime dt)
            return "\"" + dt.ToString("o") + "\"";

        if (value is Guid g)
            return "\"" + g + "\"";

        if (IsDictionaryType(type))
            return SerializeDictionary(value);

        if (IsEnumerableType(type))
            return SerializeArray(value);

        return SerializeObject(value);
    }

    private static bool IsNumericType(Type type)
    {
        return type == typeof(int) || type == typeof(long)
            || type == typeof(float) || type == typeof(double)
            || type == typeof(decimal);
    }

    private static bool IsStringType(Type type)
    {
        return type == typeof(string) || type == typeof(char);
    }

    private static bool IsBooleanType(Type type)
    {
        return type == typeof(bool);
    }

    private static bool IsEnumerableType(Type type)
    {
        return typeof(IEnumerable).IsAssignableFrom(type);
    }

    private static bool IsDictionaryType(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
    }

    private string JsonString(string value)
    {
        var sb = new StringBuilder();
        foreach (var c in value)
        {
            switch (c)
            {
                case '"':
                    sb.Append("\\\"");
                    break;
                case '\\':
                    sb.Append("\\\\");
                    break;
                case '\b':
                    sb.Append("\\b");
                    break;
                case '\f':
                    sb.Append("\\f");
                    break;
                case '\n':
                    sb.Append("\\n");
                    break;
                case '\r':
                    sb.Append("\\r");
                    break;
                case '\t':
                    sb.Append("\\t");
                    break;
                default:
                    if (char.IsControl(c))
                    {
                        sb.Append("\\u");
                        sb.Append(((int)c).ToString("x4"));
                    }
                    else
                    {
                        sb.Append(c);
                    }
                    break;
            }
        }
        return sb.ToString();
    }

    private string SerializeArray(object value)
    {
        var sb = new StringBuilder();
        sb.Append('[');
        var items = (IEnumerable)value;
        var first = true;
        foreach (var item in items)
        {
            if (!first) sb.Append(", ");
            sb.Append(Serialize(item));
            first = false;
        }
        sb.Append(']');
        return sb.ToString();
    }

    private string SerializeDictionary(object value)
    {
        var sb = new StringBuilder();
        sb.Append('{');
        var dict = (IDictionary)value;
        var first = true;
        foreach (DictionaryEntry entry in dict)
        {
            if (!first) sb.Append(", ");
            sb.Append("\"" + JsonString(entry.Key.ToString()!) + "\": ");
            sb.Append(Serialize(entry.Value));
            first = false;
        }
        sb.Append('}');
        return sb.ToString();
    }

    private string SerializeObject(object value)
    {
        var sb = new StringBuilder();
        sb.Append('{');
        var properties = value.GetType().GetProperties();
        var first = true;
        foreach (var prop in properties)
        {
            if (!prop.CanRead) continue;
            var propValue = prop.GetValue(value);
            if (!first) sb.Append(", ");
            sb.Append("\"" + JsonString(prop.Name) + "\": ");
            sb.Append(Serialize(propValue));
            first = false;
        }
        sb.Append('}');
        return sb.ToString();
    }
}
