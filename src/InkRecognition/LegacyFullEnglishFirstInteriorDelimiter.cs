namespace InkRecognition;

/// <summary>Actual full FIRST assigned24A guide/cut-source interior1A0 owner.
/// Public callers cannot supply symbols, delimiter decisions or geometry.</summary>
internal sealed class LegacyFullEnglishFirstInteriorDelimiter
{
    private readonly LegacyFullEnglishFirstPlainResultFactory factory;
    private readonly LegacyCursivePreparedPoint[] points;
    private readonly int[] counts,starts;
    private readonly Dictionary<int,LegacyNativeResidualBounds> bounds=[];
    private readonly Dictionary<int,LegacyFirstSelectedGlyphCell> selected;
    private readonly ushort hyphen,slash,hash,at;
    private long pointVisits;
    internal const long MaximumPointVisits=1_048_576;
    internal LegacyFullEnglishFirstInteriorDelimiter(LegacyFullEnglishFirstPlainResultFactory factory,CancellationToken token)
    {
        this.factory=factory;token.ThrowIfCancellationRequested();
        if(factory.AssignedDescriptor.CurrentLineId!=0)throw new ArgumentException("Proved post24A assigned line0 required.");
        counts=factory.Candidates.CopyCutCounts();long total=counts.Sum(n=>(long)n);
        if(total>MaximumPointVisits)throw new NotSupportedException("Portable first interior point budget exceeded before copying source points.");
        points=factory.Candidates.CopyCutRecords();starts=new int[counts.Length];
        for(int i=1;i<starts.Length;i++)starts[i]=checked(starts[i-1]+counts[i-1]);
        if(total!=points.Length)throw new InvalidDataException("Actual cut prefix and point storage differ.");
        selected=factory.Cells.ToDictionary(c=>c.Node);
        var alphabet=factory.Initializer.ActualCache.Model.Source.Alphabet;
        ushort Label(string text)=>alphabet.TryGetLabel(text,out ushort value)?value:ushort.MaxValue;
        hyphen=Label("-");slash=Label("/");hash=Label("#");at=Label("@");
    }
    internal LegacyFirstInteriorDelimiterResult Read(int previousNode,ushort previousArc,ushort previousSymbol,
        int currentNode,ushort currentArc,ushort currentSymbol,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(!selected.TryGetValue(previousNode,out var previous)||!selected.TryGetValue(currentNode,out var current)||
            previous.Arc!=previousArc||previous.Symbol!=previousSymbol||current.Arc!=currentArc||current.Symbol!=currentSymbol||
            current.PreviousNode!=previousNode||current.PreviousArc!=previousArc)
            throw new ArgumentException("Same selected FIRST predecessor cells required.");
        var graph=factory.Graph;var left=graph.GetNode(previousNode)[previousArc];var right=graph.GetNode(currentNode)[currentArc];
        return LegacyNativeFirstInteriorDelimiterKernel.Read(currentSymbol,previousSymbol,
            FirstId(currentSymbol),FirstId(previousSymbol),left.StartContextId,right.Flags,factory.Preparation.Line.Height,
            hyphen,slash,hash,at,()=>ReadBounds(left.GroupId,token),()=>ReadBounds(right.GroupId,token),token);
    }
    private int FirstId(ushort symbol)
    {
        if(symbol==ushort.MaxValue)return 0;var categories=factory.Categories;
        if(symbol>=categories.FirstMap.Length||categories.FirstMap.Span[symbol]<0)
            throw new NotSupportedException("Actual first delimiter category is unavailable.");
        int value=categories.FirstIds.Span[categories.FirstMap.Span[symbol]];
        if((uint)value>=7)throw new InvalidDataException("Native first category VALUE0..6 required.");return value;
    }
    internal LegacyNativeResidualBounds ReadBounds(int group,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(bounds.TryGetValue(group,out var found))return found;
        if((uint)group>=(uint)factory.Graph.Groups.Count)throw new ArgumentOutOfRangeException(nameof(group));
        float minx=float.MaxValue,miny=float.MaxValue,maxx=-float.MaxValue,maxy=-float.MaxValue;
        foreach(var range in factory.Graph.Groups[group].Ranges.Span)
        {
            if((uint)range.StrokeIndex>=(uint)counts.Length||range.StartPoint<0||range.EndPointExclusive>counts[range.StrokeIndex]||range.StartPoint>=range.EndPointExclusive)
                throw new InvalidDataException("First bounds range escapes its own cut source.");
            for(int p=range.StartPoint;p<range.EndPointExclusive;p++)
            {
                token.ThrowIfCancellationRequested();if(pointVisits==MaximumPointVisits)throw new NotSupportedException("Portable first interior point-visit budget exceeded.");pointVisits++;
                var point=points[starts[range.StrokeIndex]+p];minx=Math.Min(minx,point.X);miny=Math.Min(miny,point.Y);maxx=Math.Max(maxx,point.X);maxy=Math.Max(maxy,point.Y);
            }
        }
        token.ThrowIfCancellationRequested();var result=new LegacyNativeResidualBounds(minx,miny,maxx,maxy);bounds.Add(group,result);return result;
    }
}
