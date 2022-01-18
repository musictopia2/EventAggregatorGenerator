namespace EventAggregatorGenerator;
internal static class SourceContextExtensions
{
    public static void RaiseNoVariableException(this SourceProductionContext context, string className)
    {
        string information = $"Needs to have variable for IEventAggregator.  The class name was {className}";
        context.ReportDiagnostic(Diagnostic.Create(RaiseException(information, "NoVariable"), Location.None));
    }
    public static void RaiseNoPartialClassException(this SourceProductionContext context, string className)
    {
        string information = $"Needs to have partial class for class name was {className}";
        context.ReportDiagnostic(Diagnostic.Create(RaiseException(information, "NoClass"), Location.None));
    }
    public static void RaiseNoSubscribeException(this SourceProductionContext context, string className)
    {
        string information = $"Needs to have partial subscribe.  Try to use private partial void Subscribe();  The class name was {className}";
        context.ReportDiagnostic(Diagnostic.Create(RaiseException(information, "SubscribeMethod"), Location.None));
    }
    public static void RaiseNoUnsubscribeException(this SourceProductionContext context, string className)
    {
        string information = $"Needs to have partial unsubscribe.  Try to use private partial void Unsubscribe(); The class name was {className}";
        context.ReportDiagnostic(Diagnostic.Create(RaiseException(information, "UnsubscribeMethod"), Location.None));
    }
    public static void NoTagsAllowed(this SourceProductionContext context)
    {
        string information = $"Cannot put in tags because the ScreenViewModel has to do 2 subscriptions.  One is for name and the other is no tags.";
        context.ReportDiagnostic(Diagnostic.Create(RaiseException(information, "NoTags"), Location.None));
    }
    private static DiagnosticDescriptor RaiseException(string information, string id) => new(id,
        "Could not create helpers",
        information,
        "CustomID",
        DiagnosticSeverity.Error,
        true);

}