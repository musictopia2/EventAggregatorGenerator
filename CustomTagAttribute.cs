global using EventAggregatorGenerator;
namespace EventAggregatorGenerator;
[AttributeUsage(AttributeTargets.Class)]
internal class CustomTagAttribute : Attribute
{
    [Required]
    public string Tag { get; set; } = "";
    public bool AlsoNoTags { get; set; }
}