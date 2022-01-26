namespace EventAggregatorGenerator;
internal static class WriterExtensions
{
    public static IWriter PopulateHandle(this IWriter w, string name)
    {
        w.GlobalWrite()
            .Write("MessengingHelpers.")
            .Write(name);
        return w;
    }
}