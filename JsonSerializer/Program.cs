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

class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    public Address Address { get; set; }
}

class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string ZipCode { get; set; }
}