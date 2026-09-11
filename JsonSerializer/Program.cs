using JsonSerializer;


var serializer = new MyJsonSerializer();

string name = "John Doe";

string jsonName = serializer.Serialize(name);

Console.WriteLine(jsonName); // Output: "John Doe"

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

var jsonPerson = serializer.Serialize(person);
Console.WriteLine(jsonPerson);

IEnumerable<int> numbers = new HashSet<int> { 1, 2, 3 };
Console.WriteLine(serializer.Serialize(numbers));

string? parsedName = serializer.Deserialize<string>("\"Ada Lovelace\"");
int parsedAge = serializer.Deserialize<int>("36");
bool parsedActive = serializer.Deserialize<bool>("true");
Console.WriteLine("Primitives: " + parsedName + ", " + parsedAge + ", " + parsedActive);

Person? restoredPerson = serializer.Deserialize<Person>(jsonPerson);
Console.WriteLine("Restored Object: " + restoredPerson?.Name + " living in " + restoredPerson?.Address?.City);

int[]? restoredNumbers = serializer.Deserialize<int[]>("[10, 20, 30]");
Console.WriteLine("Restored Array: Length = " + restoredNumbers?.Length + ", items = [" + string.Join(", ", restoredNumbers ?? Array.Empty<int>()) + "]");

try
{
    Node circularNode = new Node { Name = "Loop" };
    circularNode.Next = circularNode;
    serializer.Serialize(circularNode);
}
catch (InvalidOperationException ex)
{
    Console.WriteLine("Circular Reference Handled: " + ex.Message);
}

try
{
    serializer.Deserialize<int>("\"not_a_number\"");
}
catch (FormatException ex)
{
    Console.WriteLine("Error Handled Gracefully: " + ex.Message);
}

class Person
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public Address Address { get; set; } = new Address();
}

class Address
{
    public string Street { get; set; } = "";
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string ZipCode { get; set; } = "";
}

class Node
{
    public string Name { get; set; } = "";
    public Node? Next { get; set; }
}