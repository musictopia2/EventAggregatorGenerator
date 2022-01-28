﻿namespace EventAggregatorGenerator;
internal class CustomInformation
{
    public EnumCategory Category { get; set; }
    public BasicList<HandleInfo> HandlesRegularImplemented { get; set; } = new();
    public BasicList<HandleInfo> HandlesAsyncImplemented { get; set; } = new();
    public string VariableName { get; set; } = "";
    public string TagName { get; set; } = "";
    public bool HasPartialSubscribe { get; set; }
    public bool HasPartialUnsubscribe { get; set; }
    public bool HasPartialClass { get; set; }
    public string GenericInfo { get; set; } = "";
    public INamedTypeSymbol? SymbolUsed { get; set; }
    public bool HasErrors()
    {
        if (VariableName == "")
        {
            return true;
        }
        if (Category == EnumCategory.Screen)
        {
            return HasPartialClass == false;
        }
        if (HasPartialClass == false || HasPartialSubscribe == false || HasPartialUnsubscribe == false)
        {
            return true;
        }
        return false;
    }
}