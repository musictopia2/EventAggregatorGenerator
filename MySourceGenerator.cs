namespace EventAggravatorGeneratorV2.GeneratorLibrary;
[Generator]
public class MySourceGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(c => c.CreateCustomSource().AddAttributesToSourceOnly());
        IncrementalValuesProvider<ClassDeclarationSyntax> declares = context.SyntaxProvider.CreateSyntaxProvider(
            (s, _) => IsSyntaxTarget(s),
            (t, _) => GetTarget(t))
            .Where(m => m != null)!;
        IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilation
            = context.CompilationProvider.Combine(declares.Collect());
        context.RegisterSourceOutput(compilation, (spc, source) =>
        {
            Execute(source.Item1, source.Item2, spc);
        });
    }
    private bool IsSyntaxTarget(SyntaxNode syntax)
    {
        if (syntax is ClassDeclarationSyntax cts == false)
        {
            return false;
        }
        if (cts.Implements("IHandle"))
        {
            return true;
        }
        if (cts.Implements("IHandleAsync"))
        {
            return true;
        }
        if (cts.Implements("IBlazorParent"))
        {
            return true;
        }
        return false;
    }
    private ClassDeclarationSyntax? GetTarget(GeneratorSyntaxContext context)
    {
        var ourClass = context.GetClassNode();
        return ourClass;
    }
    private void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> list, SourceProductionContext context)
    {
        var others = list.Distinct();
        ParserClass parses = new(compilation);
        var results = parses.GetResults(others);
        EmitClass emits = new(context, results, compilation);
        emits.Emit();
    }
}