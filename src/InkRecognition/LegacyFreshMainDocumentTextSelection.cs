namespace InkRecognition;

/// <summary>Fresh0283B0 NULL-reference source projection followed by pending
/// kind2 TEXT registration. Actual raw selected-input rows supply the key;
/// this does not apply document postprocessors or commit accepted text.</summary>
public sealed class LegacyFreshMainDocumentTextSelection
{
    private readonly LegacyNativeSelectedInkRange[] inputs;
    private readonly LegacyNativeInkPositionRange[] ranges;
    private readonly object gate=new();
    private bool entered;
    private LegacyOwnedFreshTextSelection? selection;
    public LegacyFreshMainSourceLinePublication SourceLines {get;}
    public LegacyOwnedMainWordSourceChoicesResult WordChoices=>SourceLines.WordChoices;
    public LegacyOwnedRawInkDocument Document=>WordChoices.Owner.Document;
    public int InputCount=>inputs.Length;
    public bool IsFaulted {get;private set;}
    public bool IsRegistered=>selection is not null;
    public LegacyOwnedFreshTextSelection Selection=>selection??throw new InvalidOperationException("TEXT has not been registered.");
    internal LegacyOwnedFreshTextSourceContext SelectionContext=>WordChoices.Owner.SelectionContext;
    private LegacyFreshMainDocumentTextSelection(LegacyFreshMainSourceLinePublication sourceLines,
        LegacyNativeSelectedInkRange[] inputs,LegacyNativeInkPositionRange[] ranges)
    {SourceLines=sourceLines;this.inputs=inputs;this.ranges=ranges;}

    public static LegacyFreshMainDocumentTextSelection Prepare(LegacyFreshMainSourceLinePublication actualSourceLines,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualSourceLines);token.ThrowIfCancellationRequested();
        var choices=actualSourceLines.WordChoices;
        if(choices.Owner.IsFaulted||actualSourceLines.Alternatives.Count!=2||actualSourceLines.LineCount!=2)
            throw new ArgumentException("Actual fresh completed shared-WORD/source-line owner required.");
        foreach(var word in choices.Owner.WordViews)word.Validate(token);
        var document=choices.Owner.Document;var selected=document.CopySelectedInputs();
        if(selected.Length!=document.ObjectCount)throw new InvalidDataException("Actual complete raw root selection required.");
        var operations=LegacyOwnedRawInkRangeOperations.Prepare(document);LegacyNativeInkPositionRange[] ranges=[];
        //05AFA0 param6=NULL is a different branch from nonnull/count0. It
        //visits every selected40 row;2C1.V18/053F90 receives no overrides.
        //C21.VA8/0802E0 skips mode4, otherwise invokes07FAF0 normalization.
        for(int i=0;i<selected.Length;i++)
        {
            token.ThrowIfCancellationRequested();var row=selected[i];
            if(row.IsKnownText||row.SourceIndex!=(uint)i||row.PointCount!=document.Strokes[i].Points.Count)
                throw new ArgumentException("Same original raw object IDs/point counts required.");
            var projected=LegacyNativeTextSourceRangeProjection.Project(row);
            if(projected.First.Mode==4)continue;
            ranges=operations.Union(ranges,[projected],token);
        }
        return new(actualSourceLines,selected,ranges);
    }

    public LegacyOwnedFreshTextSelection Register(CancellationToken token=default)
    {
        lock(gate)
        {
            token.ThrowIfCancellationRequested();
            if(entered)throw new InvalidOperationException("TEXT registration is single-use after entry.");
            foreach(var word in WordChoices.Owner.WordViews)word.Validate(token);
            entered=true;
            try{selection=SelectionContext.CreateTextSelection(this,token);return selection;}
            catch{IsFaulted=true;throw;}
        }
    }
    public LegacyNativeSelectedInkRange[] CopySelectedInputs()=>inputs.ToArray();
    public LegacyNativeInkPositionRange[] CopyRanges()=>ranges.ToArray();
    public IReadOnlyList<LegacyOwnedFreshTextSelection> CopyPendingSelections()=>SelectionContext.CopyPendingSelections();
}
