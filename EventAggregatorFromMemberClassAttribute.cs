namespace EventAggregatorGenerator;
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
internal sealed class EventAggregatorFromMemberClassAttribute(string memberName) : Attribute
{
    public string MemberName { get; } = memberName;
}