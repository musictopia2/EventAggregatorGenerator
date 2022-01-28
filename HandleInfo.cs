using System;
using System.Collections.Generic;
using System.Text;

namespace EventAggregatorGenerator;

public class HandleInfo
{
    public INamedTypeSymbol? Symbol { get; set; }
    public string GenericUsed { get; set; } = "";
}