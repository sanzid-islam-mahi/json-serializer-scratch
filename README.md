# Custom JSON Serializer

A custom JSON serializer built from scratch in C# (.NET 10) without using external JSON libraries (such as `System.Text.Json` or `Newtonsoft.Json`). It utilizes reflection and base class library utilities to dynamically serialize values and complex object graphs into JSON text.

---

## Current Status (Committed Features)

The current implementation focuses on the **Serialization** engine:

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

---

## Project Structure

```
.
├── JsonSerializer/
│   ├── Interfaces/
│   │   └── IJasonSerializer.cs   # Serializer interface definition
│   ├── MyJsonSerializer.cs       # Core serialization logic
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

## Usage Example

```csharp
using JsonSerializer;

var serializer = new MyJsonSerializer();

// 1. Primitive serialization
string nameJson = serializer.Serialize("John Doe");
// Output: "John Doe"

// 2. Complex & nested object serialization
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

string personJson = serializer.Serialize(person);
// Output: {"Name": "Jane Smith", "Age": 30, "Address": {"Street": "123 Main St", "City": "Anytown", "State": "CA", "ZipCode": "12345"}}

// 3. Collection serialization
IEnumerable<int> numbers = new HashSet<int> { 1, 2, 3 };
string numbersJson = serializer.Serialize(numbers);
// Output: [1, 2, 3]
```

---

## Next Steps (Planned / In Progress)

Per the assignment roadmap, upcoming capabilities include:
- JSON deserialization back into typed C# objects (`Deserialize<T>`)
- Circular reference detection and handling
- Robust parsing error handling with descriptive exceptions
- Performance benchmarking and reflection caching
