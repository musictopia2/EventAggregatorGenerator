namespace EventAggregatorGenerator;
internal class ParserClass
{
    private readonly Compilation _compilation;
    public ParserClass(Compilation compilation)
    {
        _compilation = compilation;
    }
    public BasicList<CustomInformation> GetResults(IEnumerable<ClassDeclarationSyntax> list)
    {
        BasicList<CustomInformation> output = new();
        foreach (var item in list)
        {
            SemanticModel compilationSemanticModel = item.GetSemanticModel(_compilation);
            INamedTypeSymbol symbol = (INamedTypeSymbol)compilationSemanticModel.GetDeclaredSymbol(item)!;
            CustomInformation model = new();
            model.HasPartialClass = item.IsPartial();
            var toUse = symbol.GetSymbolOfType("IEventAggregator");
            if (toUse is null)
            {
                model.VariableName = "";
            }
            else
            {
                model.VariableName = toUse.Name;
            }
            toUse = symbol.GetSpecificMethod("Subscribe");
            model.HasPartialSubscribe = toUse is not null;
            toUse = symbol.GetSpecificMethod("Unsubscribe"); //no caps
            model.HasPartialUnsubscribe = toUse is not null;
            bool parent = symbol.Implements("IBlazorParent");
            var nexts = symbol.GetGenericSymbolsImplemented("IHandle");
            foreach (var n in nexts)
            {
                HandleInfo h = new();
                h.Symbol = n;
                h.GenericUsed = n.GetGenericString();
                model.HandlesRegularImplemented.Add(h);
            }
            //model.HandlesRegularImplemented = symbol.GetGenericSymbolsImplemented("IHandle");
            nexts = symbol.GetGenericSymbolsImplemented("IHandleAsync");
            foreach (var n in nexts)
            {
                HandleInfo h = new();
                h.Symbol = n;
                h.GenericUsed = n.GetGenericString();
                model.HandlesAsyncImplemented.Add(h);
            }
            bool rets = symbol.TryGetAttribute(aa.CustomTag.CustomTagAttribute, out var attributes);
            if (rets)
            {
                model.TagName = attributes.AttributePropertyValue<string>(aa.CustomTag.GetTagInfo)!;
                if (string.IsNullOrWhiteSpace(model.TagName))
                {
                    //this means needs another condition.
                    model.NeedsTagVariables = true;
                    toUse = symbol.GetSpecificMethod("Subscribe", 1);
                    model.HasPartialSubscribe = toUse is not null;
                    if (model.HasPartialSubscribe)
                    {
                        var temps = toUse as IMethodSymbol;
                        var ss = temps!.Parameters.Single();
                        model.SubscribeTag = ss.Name;
                    }
                    toUse = symbol.GetSpecificMethod("Unsubscribe", 1); //no caps
                    model.HasPartialUnsubscribe = toUse is not null;
                    if (model.HasPartialUnsubscribe)
                    {
                        var temps = toUse as IMethodSymbol;
                        var ss = temps!.Parameters.Single();
                        model.UnsubscribeTag = ss.Name;
                    }
                }
            }
            model.SymbolUsed = symbol;
            if (symbol.Name == "ScreenViewModel" && symbol.ContainingNamespace.ToDisplayString() == "MVVMFramework.ViewModels")
            {
                model.Category = EnumCategory.Main;
            }
            else if (symbol.InheritsFrom("ScreenViewModel"))
            {
                model.Category = EnumCategory.Screen;
            }
            else if (parent)
            {
                model.Category = EnumCategory.Parent;
            }
            else
            {
                model.Category = EnumCategory.Regular;
            }
            model.GenericInfo = symbol.GetGenericString();
            output.Add(model);
        }
        return output;
    }
}