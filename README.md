# Custom JSON Serializer

A custom JSON serializer and deserializer built from scratch in C# (.NET 10) without external JSON libraries (such as `System.Text.Json` or `Newtonsoft.Json`). It uses reflection and standard library utilities to dynamically serialize and deserialize values, collections, and complex object graphs.

---

## Features

### 1. Serialization
- **Primitives & Core Types**:
  - `null` &rarr; `null`
  - Strings & characters &rarr; quoted strings with escape handling (`\"`, `\\`, `\n`, `\r`, `\t`, `\b`, `\f`, and `\uXXXX` control characters)
  - Booleans &rarr; `true` / `false`
  - Numbers &rarr; `int`, `long`, `float`, `double`, `decimal`
- **Special Types**:
  - `enum` &rarr; string representation
  - `DateTime` &rarr; ISO 8601 round-trip string format (`"o"`)
  - `Guid` &rarr; standard UUID string format
- **Collections & Enumerable Types**:
  - Any `IEnumerable` (including arrays, `List<T>`, `HashSet<T>`, etc.) &rarr; JSON arrays `[...]`
- **Dictionaries**:
  - Generic `Dictionary<TKey, TValue>` &rarr; JSON objects `{ "key": value, ... }`
- **Complex & Nested Objects**:
  - Dynamically discovers readable public properties via reflection (`Type.GetProperties()`).
  - Recursively serializes nested objects at arbitrary depths.
- **Circular Reference Detection**:
  - Tracks visited objects using a `HashSet<object>` during traversal.
  - Throws `InvalidOperationException` if a circular reference is encountered, preventing `StackOverflowException`.

### 2. Deserialization
- **Primitives & Special Types**:
  - Strings (with escape sequence parsing), `int`, `long`, `double`, `bool`
  - `DateTime`, `Guid`, and `enum` values
- **Objects & Nested Classes**:
  - Instantiates target classes using `Activator.CreateInstance`.
  - Discovers writable properties dynamically using reflection and maps JSON key-value pairs recursively.
- **Collections**:
  - Deserializes JSON arrays `[...]` into typed C# arrays (`T[]`) and `List<T>`.
- **Error Handling**:
  - Raises descriptive `FormatException`s on malformed JSON (unterminated strings, missing braces/brackets, type conversion mismatches).

---

## Project Structure

```
.
├── JsonSerializer/
│   ├── Interfaces/
│   │   └── IJasonSerializer.cs   # Serializer interface definition
│   ├── MyJsonSerializer.cs       # Core serialization and deserialization logic
│   ├── Program.cs                # Demonstration and sample runs
│   └── JsonSerializer.csproj     # Project file (.NET 10)
├── assignment.html               # Assignment specification
└── README.md
```

---

## Quick Start

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/)

### Running the Demo
From the repository root:

```bash
dotnet run --project JsonSerializer/JsonSerializer.csproj
```

---

## Usage Examples

```csharp
using JsonSerializer;

var serializer = new MyJsonSerializer();

// 1. Serialization
var person = new Person
{
    Name = "Jane Smith",
    Age = 30,
    Address = new Address
    {
        Street = "123 Main St",
        City = "Anytown",
        State = "CA",
        ZipCode = "12345"
    }
};

string json = serializer.Serialize(person);
// Output: {"Name": "Jane Smith", "Age": 30, "Address": {"Street": "123 Main St", "City": "Anytown", "State": "CA", "ZipCode": "12345"}}

// 2. Deserialization (Object)
Person? restored = serializer.Deserialize<Person>(json);
Console.WriteLine(restored?.Name + " from " + restored?.Address?.City);

// 3. Deserialization (Array)
int[]? numbers = serializer.Deserialize<int[]>("[10, 20, 30]");

// 4. Circular Reference Protection
Node node = new Node { Name = "Loop" };
node.Next = node;
// serializer.Serialize(node); // Throws InvalidOperationException: Circular reference detected!
```
