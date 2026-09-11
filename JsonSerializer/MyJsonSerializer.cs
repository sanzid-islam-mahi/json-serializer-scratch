using System.Collections;
using System.Reflection;
using System.Text;

namespace JsonSerializer;

public class MyJsonSerializer : IJasonSerializer
{
    private HashSet<object> seenObjects = new HashSet<object>();

    public string Serialize(object? value)
    {
        seenObjects.Clear();
        return SerializeInternal(value);
    }

    private string SerializeInternal(object? value)
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

    public T? Deserialize<T>(string json)
    {
        object? result = Deserialize(json, typeof(T));
        if (result == null)
        {
            return default;
        }
        return (T)result;
    }

    public object? Deserialize(string json, Type targetType)
    {
        if (json == null)
        {
            throw new ArgumentNullException(nameof(json));
        }

        string token = json.Trim();

        if (token == "null")
        {
            return null;
        }

        if (targetType == typeof(string))
        {
            return ParseJsonString(token);
        }

        if (targetType == typeof(int))
        {
            return int.Parse(token);
        }

        if (targetType == typeof(long))
        {
            return long.Parse(token);
        }

        if (targetType == typeof(double))
        {
            return double.Parse(token);
        }

        if (targetType == typeof(bool))
        {
            return bool.Parse(token);
        }

        if (targetType == typeof(Guid))
        {
            return Guid.Parse(token.Trim('"'));
        }

        if (targetType == typeof(DateTime))
        {
            return DateTime.Parse(token.Trim('"'));
        }

        if (targetType.IsEnum)
        {
            return Enum.Parse(targetType, token.Trim('"'));
        }

        if (token.StartsWith("{") && token.EndsWith("}"))
        {
            return DeserializeObject(token, targetType);
        }

        if (token.StartsWith("[") && token.EndsWith("]"))
        {
            return DeserializeArray(token, targetType);
        }

        throw new NotImplementedException("Deserialization for " + targetType.Name + " is not implemented yet.");
    }

    private object DeserializeArray(string token, Type targetType)
    {
        string inner = token.Substring(1, token.Length - 2).Trim();
        List<string> items = inner.Length == 0 ? new List<string>() : SplitJsonElements(inner);

        if (targetType.IsArray)
        {
            Type elementType = targetType.GetElementType()!;
            Array array = Array.CreateInstance(elementType, items.Count);
            for (int i = 0; i < items.Count; i++)
            {
                object? itemValue = Deserialize(items[i].Trim(), elementType);
                array.SetValue(itemValue, i);
            }
            return array;
        }

        Type? genericTypeDef = targetType.IsGenericType ? targetType.GetGenericTypeDefinition() : null;
        if (genericTypeDef == typeof(List<>) || genericTypeDef == typeof(IEnumerable<>) || genericTypeDef == typeof(ICollection<>) || genericTypeDef == typeof(IList<>))
        {
            Type elementType = targetType.GetGenericArguments()[0];
            Type listType = typeof(List<>).MakeGenericType(elementType);
            IList list = (IList)Activator.CreateInstance(listType)!;

            for (int i = 0; i < items.Count; i++)
            {
                object? itemValue = Deserialize(items[i].Trim(), elementType);
                list.Add(itemValue);
            }

            return list;
        }

        throw new NotImplementedException("Collection type " + targetType.Name + " is not supported for deserialization.");
    }

    private object DeserializeObject(string token, Type targetType)
    {
        object instance = Activator.CreateInstance(targetType)!;
        string inner = token.Substring(1, token.Length - 2).Trim();
        if (inner.Length == 0)
        {
            return instance;
        }

        List<string> pairs = SplitJsonElements(inner);
        for (int i = 0; i < pairs.Count; i++)
        {
            string pair = pairs[i].Trim();
            if (pair.Length == 0)
            {
                continue;
            }

            int colonIndex = FindColonIndex(pair);
            if (colonIndex == -1)
            {
                throw new FormatException("Invalid key-value pair in JSON object: " + pair);
            }

            string keyPart = pair.Substring(0, colonIndex).Trim();
            string valuePart = pair.Substring(colonIndex + 1).Trim();
            string propName = keyPart.Trim('"');

            PropertyInfo? prop = targetType.GetProperty(propName, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite)
            {
                object? propValue = Deserialize(valuePart, prop.PropertyType);
                prop.SetValue(instance, propValue);
            }
        }

        return instance;
    }

    private static List<string> SplitJsonElements(string content)
    {
        List<string> elements = new List<string>();
        StringBuilder current = new StringBuilder();
        bool inQuotes = false;
        int braceDepth = 0;
        int bracketDepth = 0;

        for (int i = 0; i < content.Length; i++)
        {
            char c = content[i];

            if (c == '"' && (i == 0 || content[i - 1] != '\\'))
            {
                inQuotes = !inQuotes;
            }

            if (!inQuotes)
            {
                if (c == '{') braceDepth++;
                else if (c == '}') braceDepth--;
                else if (c == '[') bracketDepth++;
                else if (c == ']') bracketDepth--;
                else if (c == ',' && braceDepth == 0 && bracketDepth == 0)
                {
                    elements.Add(current.ToString());
                    current.Clear();
                    continue;
                }
            }

            current.Append(c);
        }

        if (current.Length > 0)
        {
            elements.Add(current.ToString());
        }

        return elements;
    }

    private static int FindColonIndex(string pair)
    {
        bool inQuotes = false;
        for (int i = 0; i < pair.Length; i++)
        {
            char c = pair[i];
            if (c == '"' && (i == 0 || pair[i - 1] != '\\'))
            {
                inQuotes = !inQuotes;
            }

            if (c == ':' && !inQuotes)
            {
                return i;
            }
        }
        return -1;
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

    private static string ParseJsonString(string token)
    {
        if (token.Length < 2 || token[0] != '"' || token[token.Length - 1] != '"')
        {
            throw new FormatException("A JSON string must start and end with a quote.");
        }

        string content = token.Substring(1, token.Length - 2);
        StringBuilder result = new StringBuilder();

        for (int i = 0; i < content.Length; i++)
        {
            char c = content[i];

            if (c == '\\')
            {
                i++;
                if (i >= content.Length)
                {
                    throw new FormatException("Invalid escape sequence at the end of a JSON string.");
                }

                char escaped = content[i];
                switch (escaped)
                {
                    case '"':
                        result.Append('"');
                        break;
                    case '\\':
                        result.Append('\\');
                        break;
                    case '/':
                        result.Append('/');
                        break;
                    case 'b':
                        result.Append('\b');
                        break;
                    case 'f':
                        result.Append('\f');
                        break;
                    case 'n':
                        result.Append('\n');
                        break;
                    case 'r':
                        result.Append('\r');
                        break;
                    case 't':
                        result.Append('\t');
                        break;
                    default:
                        throw new FormatException("Unsupported JSON escape sequence: \\" + escaped);
                }
            }
            else
            {
                result.Append(c);
            }
        }

        return result.ToString();
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
            sb.Append(SerializeInternal(item));
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
            sb.Append(SerializeInternal(entry.Value));
            first = false;
        }
        sb.Append('}');
        return sb.ToString();
    }

    private string SerializeObject(object value)
    {
        if (seenObjects.Contains(value))
        {
            throw new InvalidOperationException("Circular reference detected!");
        }

        seenObjects.Add(value);

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
            sb.Append(SerializeInternal(propValue));
            first = false;
        }
        sb.Append('}');

        seenObjects.Remove(value);
        return sb.ToString();
    }
}
