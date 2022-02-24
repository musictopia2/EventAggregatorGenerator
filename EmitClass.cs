using System.Reflection;

namespace EventAggregatorGenerator;
internal class EmitClass
{
    private readonly SourceProductionContext _context;
    private readonly IEnumerable<CustomInformation> _list;
    private readonly Compilation _compilation;
    public EmitClass(SourceProductionContext context, IEnumerable<CustomInformation> list, Compilation compilation)
    {
        _context = context;
        _list = list;
        _compilation = compilation;
    }
    public void Emit()
    {
        //bool hadErrors = false;
        foreach (var item in _list)
        {
            if (item.HasPartialSubscribe == false && item.Category != EnumCategory.Screen)
            {
                _context.RaiseNoSubscribeException(item.SymbolUsed!.Name, item.NeedsTagVariables);
                //hadErrors = true;
            }
            if (item.HasPartialUnsubscribe == false && item.Category != EnumCategory.Screen)
            {
                _context.RaiseNoUnsubscribeException(item.SymbolUsed!.Name, item.NeedsTagVariables);
                //hadErrors = true;
            }
            if (item.HasPartialClass == false)
            {
                _context.RaiseNoPartialClassException(item.SymbolUsed!.Name);
                //hadErrors = true;
            }
            if (item.VariableName == "")
            {
                _context.RaiseNoVariableException(item.SymbolUsed!.Name);
                //hadErrors = true;
            }
            if (item.Category == EnumCategory.Main && item.TagName != "")
            {
                _context.NoTagsAllowed();
                //hadErrors = true;
            }
        }
        //if (hadErrors)
        //{
        //    return;
        //}

        foreach (var item in _list)
        {
            if (item.HasErrors() == false)
            {
                SourceCodeStringBuilder builder = new();
                builder.WriteLine("#nullable enable")
                    .WriteLine(w =>
                    {
                        w.Write("namespace ")
                        .Write(item.SymbolUsed!.ContainingNamespace)
                        .Write(";");
                    })
                .WriteLine(w =>
                {
                    w.Write("public partial class ")
                    .Write(item.SymbolUsed!.Name)
                    .Write(item.GenericInfo);
                })
                .WriteCodeBlock(w =>
                {
                    if (item.Category != EnumCategory.Screen)
                    {
                        if (item.SubscribeTag != "")
                        {
                            w.WriteLine(w =>
                            {
                                w.Write("private partial void Subscribe(string ")
                                .Write(item.SubscribeTag)
                                .Write(")");
                            });
                        }
                        else
                        {
                            w.WriteLine("private partial void Subscribe()");
                        }
                        w.WriteCodeBlock(w =>
                        {
                            WriteSubscribe(w, item);
                        });
                        if (item.UnsubscribeTag != "")
                        {
                            w.WriteLine(w =>
                            {
                                w.Write("private partial void Unsubscribe(string ")
                               .Write(item.SubscribeTag)
                               .Write(")");
                            });
                        }
                        else
                        {
                            w.WriteLine("private partial void Unsubscribe()");
                        }
                        w.WriteCodeBlock(w =>
                        {

                            WriteUnsubscribe(w, item);
                        });
                    }
                    else
                    {
                        w.WriteLine("protected override void OpenAggregator()")
                        .WriteCodeBlock(w =>
                        {
                            //the bad news is closing out did not help after all.  somehow was still hosed.  this means that something else has to close it manually
                            //w.WriteLine("CloseAggregator();"); //needs to close it just in case somehow did not do what it was supposed to do.
                            w.WriteLine("base.OpenAggregator();");
                            WriteSubscribe(w, item);
                        });
                        w.WriteLine("protected override void CloseAggregator()")
                        .WriteCodeBlock(w =>
                        {
                            w.WriteLine("base.CloseAggregator();");
                            WriteUnsubscribe(w, item);
                        });
                    }
                });
                _context.AddSource($"{item.SymbolUsed!.Name}.EventAggravatorMethods.g", builder.ToString());
            }
        }
        AddGlobal();
    }
    private void AddGlobal()
    {
        if (_list.Count() == 0)
        {
            return;
        }
        SourceCodeStringBuilder builder = new();
        string ns = _compilation.AssemblyName!;
        builder.WriteLine("#nullable enable")
        .WriteLine(w =>
        {
            w.Write("namespace ")
            .Write(ns)
            .Write(".EventAggravatorProcesses;");
        })
        .WriteLine("public static class GlobalEventAggravatorClass")
        .WriteCodeBlock(w =>
        {
            w.WriteLine(w =>
            {
                w.Write("public static void ClearSubscriptions(")
                .GlobalWrite()
                .Write("MessengingHelpers.IEventAggregator aggravator)");
            })
            .WriteCodeBlock(w =>
            {
                foreach (var item in _list)
                {
                    var fins = GetUniqueSymbols(item);
                    foreach (var ff in fins)
                    {
                        if (ff.GenericUsed == "")
                        {
                            w.WriteLine(w =>
                            {

                                w.Write("aggravator.Clear");
                                PrintGenerics(w, ff);
                                w.Write("();");
                            });
                        }
                    }
                }
            });
        });
        _context.AddSource("generatedglobal.g", builder.ToString());
    }
    private void WriteParent(ICodeBlock w, CustomInformation info)
    {
        if (info.Category == EnumCategory.Parent)
        {
            w.WriteLine(w =>
            {
                w.Write("Type type = typeof(")
                .Write(info.SymbolUsed!.TypeArguments.Single().Name)
                .Write(");");
            })
            .WriteLine("string name = type.Name;");
        }
    }
    private void WriteSubscribeAsyncMethod(ICodeBlock w, CustomInformation info, int index, HandleInfo fins, bool doTags)
    {
        w.WriteLine(w =>
        {
            w.Write(info.VariableName)
                    .Write("!.Subscribe");
            PrintGenerics(w, fins);
            w.Write("(this, model")
            .Write(index)
            .Write(".HandleAsync, ");
            if (doTags)
            {
                w.Write(info.SubscribeTag);
            }
            else
            {
                w.AppendDoubleQuote(info.TagName);
            }
            w.Write(");");
        });
    }
    private void WriteSubscribeRegularMethod(ICodeBlock w, CustomInformation info, int index, HandleInfo fins, bool doTags)
    {
        w.WriteLine(w =>
         {
             w.Write(info.VariableName)
             .Write("!.Subscribe");
             PrintGenerics(w, fins);
             w.Write("(this, model")
             .Write(index)
             .Write(".Handle, ");
             if (info.Category == EnumCategory.Main)
             {
                 w.AppendDoubleQuote();
             }
             else if (info.Category == EnumCategory.Parent)
             {
                 w.Write(" name");
             }
             else if (doTags == true)
             {
                 w.Write(info.SubscribeTag);
             }
             else
             {
                 w.AppendDoubleQuote(info.TagName);
             }
             w.Write(");");
         });
    }
    private void WriteSubscribe(ICodeBlock w, CustomInformation info)
    {
        int index = 0;
        WriteParent(w, info);
        if (info.Category != EnumCategory.Main)
        {
            foreach (var fins in info.HandlesAsyncImplemented)
            {
                w.WriteLine(w =>
                {
                    w.PopulateHandle("IHandleAsync");
                    PrintGenerics(w, fins);
                    w.Write("model")
                    .Write(index)
                    .Write(" = this;");
                });
                if (info.SubscribeTag != "")
                {
                    WriteSubscribeAsyncMethod(w, info, index, fins, true);
                }
                else
                {
                    WriteSubscribeAsyncMethod(w, info, index, fins, false);
                }
                if (info.AlsoNoTags)
                {
                    WriteSubscribeAsyncMethod(w, info, index, fins, false);
                }
                index++;
            }
        }

        foreach (var fins in info.HandlesRegularImplemented)
        {
            w.WriteLine(w =>
            {
                w.PopulateHandle("IHandle");
                PrintGenerics(w, fins);
                w.Write("model")
                .Write(index)
                .Write(" = this;");
            });
            if (info.SubscribeTag != "")
            {
                WriteSubscribeRegularMethod(w, info, index, fins, true);
            }
            else
            {
                WriteSubscribeRegularMethod(w, info, index, fins, false);
            }
            if (info.AlsoNoTags)
            {
                WriteSubscribeRegularMethod(w, info, index, fins, false);
            }
            index++;
            if (info.Category == EnumCategory.Main)
            {
                w.WriteLine(w =>
                {
                    w.PopulateHandle("IHandle");
                    PrintGenerics(w, fins);
                    w.Write("model")
                    .Write(index)
                    .Write(" = this;");
                })
                .WriteLine("string name = GetType().Name;")
               .WriteLine(w =>
               {
                   w.Write(info.VariableName)
                   .Write("!.Subscribe");
                   PrintGenerics(w, fins);
                   w.Write("(this, model")
                   .Write(index)
                   .Write(".Handle, name);");
               });
               index++;
            }
        }
    }
    private BasicList<HandleInfo> GetUniqueSymbols(CustomInformation info)
    {
        BasicList<HandleInfo> output = new();
        foreach (var item in info.HandlesRegularImplemented)
        {
            output.Add(item);
        }
        foreach (var item in info.HandlesAsyncImplemented)
        {
            if (output.Exists(x => x.Symbol!.Name == item.Symbol!.Name && x.Symbol.ContainingNamespace.ToDisplayString == item.Symbol.ContainingNamespace.ToDisplayString) == false)
            {
                output.Add(item);
            }
        }
        return output;
    }
    private void WriteUnsubscribe(ICodeBlock w, CustomInformation info)
    {
        BasicList<HandleInfo> list = GetUniqueSymbols(info);
        WriteParent(w, info);
        foreach (var item in list)
        {
            WriteUnsubscribe(w, info, item);
        }
    }
    private void WriteUnsubscribe(ICodeBlock w, CustomInformation info, HandleInfo subs, bool doTags)
    {
        w.WriteLine(w =>
        {
            w.Write(info.VariableName)
            .Write("!.UnsubscribeSingle");
            PrintGenerics(w, subs);
            w.Write("(this, ");
            if (info.Category == EnumCategory.Main)
            {
                w.AppendDoubleQuote();
            }
            else if (info.Category == EnumCategory.Parent)
            {
                w.Write(" name");
            }
            else if (doTags)
            {
                w.Write(info.UnsubscribeTag);
            }
            else
            {
                w.AppendDoubleQuote(info.TagName);
            }
            w.Write(");");
        });
    }
    private void WriteUnsubscribe(ICodeBlock w, CustomInformation info, HandleInfo subs)
    {
        if (info.UnsubscribeTag != "")
        {
            WriteUnsubscribe(w, info, subs, true);
        }
        else
        {
            WriteUnsubscribe(w, info, subs, false);
        }
        if (info.AlsoNoTags)
        {
            WriteUnsubscribe(w, info, subs, false);
        }
        if (info.Category == EnumCategory.Main)
        {
            w.WriteLine("string name = GetType().Name;");
            w.WriteLine(w =>
            {
                w.Write(info.VariableName)
                .Write("!.UnsubscribeSingle");
                PrintGenerics(w, subs);
                w.Write("(this, name);");
            });
        }
    }
    private void PrintGenerics(IWriter w, HandleInfo handle)
    {
        w.SingleGenericWrite(w =>
        {
            w.GlobalWrite()
            .Write(handle.Symbol!.ContainingNamespace)
            .Write(".")
            .Write(handle.Symbol!.Name);
            if (handle.GenericUsed != "")
            {
                w.Write(handle.GenericUsed);
            }
        });
    }
}