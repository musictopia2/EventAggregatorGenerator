global using EventAggregatorGenerator;
namespace EventAggregatorGenerator;
[AttributeUsage(AttributeTargets.Class)]
internal class CustomTagAttribute : Attribute
{
    [Required] //still make required but can be blank though.
    public string Tag { get; set; } = "";
}