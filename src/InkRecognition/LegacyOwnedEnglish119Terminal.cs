namespace InkRecognition;

/// <summary>Native FIRST1F42D0 terminal selection over the completed same-owner
/// English119 B8 tables. This is a numeric winner/trace, not a rich273/WORD,
/// dictionary winner, accepted text or recognition-confidence result.</summary>
public static class LegacyOwnedEnglish119Terminal
{
    public static LegacyEnglish119TerminalResult Select(LegacyEnglish119ContextResult context,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(context);token.ThrowIfCancellationRequested();
        var owner=context.Owner;var primary=owner.Primary;var policy=owner.Policy;
        var candidates=policy.Owner.Owner.Owner.Owner;var preparation=candidates.Preparation;
        var source=policy.Owner.Model.Source;var graph=policy.SourceRanges.Graph;
        if(!ReferenceEquals(primary.Owner.Policy,policy)||!ReferenceEquals(policy.Owner.Model,preparation.Model)||
            !ReferenceEquals(preparation.Source,source)||!ReferenceEquals(candidates.SourceRanges,policy.SourceRanges)||
            !ReferenceEquals(candidates.SourceRanges.OriginalSource,preparation.OriginalSource)||
            context.NodeCount!=primary.Primary.NodeCount||context.NodeCount!=graph.NodeCount||
            context.BeamWidth!=policy.PrimaryBeamWidth||owner.MergedColumnsA4!=1||policy.NativeModeE0)
            throw new ArgumentException("Same completed selected249/119 context, primary, cache and fresh raw source required.",nameof(context));
        //The private fresh raw preparation has no property/clone/reuse API.
        //16A130 source150=0 survives24A,24C XY-only stores and265.158970
        //copies that actual source150 into configA8. NEW2B3/156630 has99=0;
        //the external temporary99 wrapper2458B0 is outside this lifecycle.
        //1EDB60 sets F4=0xA000; TOP ctor/reset set78=0. See the independently
        //pinned audit_english119_terminal_plan.py; no RU factory is coerced.
        var invocation=new LegacyFirstTerminalSelectionPolicy(policy.Shadda,0xa000,false,false,source.SourceKind,0);
        var grid=new LegacyFirstMergedRecordGrid(owner.MergedColumnsA4,context.BeamWidth,context.Passes.CopyB8Storage(),token);
        var selected=LegacyFirstTerminalSelection.Select(grid,invocation,token);
        var alternatives=new List<LegacyEnglish119TerminalAlternative>(selected.Choices.Count);
        foreach(var choice in selected.Choices)
        {
            token.ThrowIfCancellationRequested();var trace=LegacyFirstSelectedGlyphTrace.Read(grid,choice,token);
            foreach(var cell in trace)
            {
                token.ThrowIfCancellationRequested();var arcs=graph.GetNode(cell.Node);
                if(cell.Arc>=arcs.Count||cell.Span!=arcs[cell.Arc].Span)
                    throw new InvalidDataException("Selected FIRST cell differs from its actual candidate graph.");
                var row=primary.Primary.GetRowCopy(cell.Node);
                if(cell.Row>=row.Count||row.Get(cell.Row).Symbol!=cell.Symbol||row.Get(cell.Row).ArcOrdinal!=cell.Arc)
                    throw new InvalidDataException("Selected FIRST cell differs from its actual primary row.");
            }
            alternatives.Add(new(choice,trace));
        }
        token.ThrowIfCancellationRequested();return new(context,invocation,selected.ReferenceCost78,alternatives);
    }
}

/// <summary>One actual B8 terminal and its exact source-order predecessor
/// cells. Graph node/arc identities are not physical input-point ordinals.</summary>
public sealed class LegacyEnglish119TerminalAlternative
{
    public LegacyFirstTerminalChoice Choice {get;}
    public IReadOnlyList<LegacyFirstSelectedGlyphCell> Cells {get;}
    internal LegacyEnglish119TerminalAlternative(LegacyFirstTerminalChoice choice,IReadOnlyList<LegacyFirstSelectedGlyphCell> cells)
    {Choice=choice;Cells=Array.AsReadOnly(cells.ToArray());}
}

/// <summary>Immutable result retaining the actual completed context and its
/// full source/model/cache ownership. Empty alternatives mean no finite B8
/// terminal; no synthetic fallback word or text is supplied.</summary>
public sealed class LegacyEnglish119TerminalResult
{
    public LegacyEnglish119ContextResult Context {get;}
    public LegacyOwnedEnglish119FirstPolicy Policy=>Context.Owner.Policy;
    public LegacyFirstTerminalSelectionPolicy Invocation {get;}
    public long ReferenceCost78 {get;}
    public IReadOnlyList<LegacyEnglish119TerminalAlternative> Alternatives {get;}
    internal LegacyEnglish119TerminalResult(LegacyEnglish119ContextResult context,LegacyFirstTerminalSelectionPolicy invocation,
        long reference,IReadOnlyList<LegacyEnglish119TerminalAlternative> alternatives)
    {Context=context;Invocation=invocation;ReferenceCost78=reference;Alternatives=Array.AsReadOnly(alternatives.ToArray());}
}
