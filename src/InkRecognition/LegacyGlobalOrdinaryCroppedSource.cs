using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyGlobalCroppedSource.Probe")]
namespace InkRecognition;

public enum LegacyGlobalRawCurveViewKind { OriginalC49, IntegerCropC4A, FractionalCropC4B }

/// <summary>Owned numeric view of actual original raw source points. Runtime
/// class names describe proven semantics, not a fabricated native pointer/ABI.</summary>
public sealed class LegacyGlobalOrdinaryCurveView
{
    public LegacyGlobalOrdinaryCroppedSource Owner{get;}
    public bool IsWord{get;}
    public int SourceNodeIdentity{get;}
    public int RangeOrdinal{get;}
    public int RawObjectIndex{get;}
    public LegacyGlobalRawCurveViewKind Kind{get;}
    public int FirstOriginalPoint{get;}
    public int LastOriginalPoint{get;}
    public float FirstFraction{get;}
    public float LastFraction{get;}
    public bool IncludesFirst{get;}
    public bool IncludesLast{get;}
    public int PointCount=>points.Length;
    private readonly InkPoint[] points;
    internal LegacyGlobalOrdinaryCurveView(LegacyGlobalOrdinaryCroppedSource owner,LegacyGlobalRawCropPlan p,
        InkPoint[] points)
    {Owner=owner;IsWord=p.IsWord;SourceNodeIdentity=p.Node;RangeOrdinal=p.Range;RawObjectIndex=p.Object;
     Kind=p.Kind;FirstOriginalPoint=p.First;LastOriginalPoint=p.Last;FirstFraction=p.Fraction0;LastFraction=p.Fraction1;
     IncludesFirst=p.Include0;IncludesLast=p.Include1;this.points=points;}
    public InkPoint[] CopyPoints(CancellationToken token=default){Owner.Validate(token);return points.ToArray();}
    public InkPoint[] ReadChunk(int offset,int count,CancellationToken token=default)
    {Owner.Validate(token);if(offset<0||count<0||count>20||offset>points.Length-count)throw new ArgumentOutOfRangeException(nameof(offset));return points.AsSpan(offset,count).ToArray();}
}

internal sealed record LegacyGlobalRawCropPlan(bool IsWord,int Node,int Range,int Object,
    LegacyGlobalRawCurveViewKind Kind,int First,int Last,float Fraction0,float Fraction1,bool Include0,bool Include1,int Count);

/// <summary>Actual selected global source ranges -> original C49 and cropped
/// C4A/C4B numeric point views. No2DB boundaries, guide writes or238B0 mutation.</summary>
public sealed class LegacyGlobalOrdinaryCroppedSource
{
    public LegacyFreshMainGlobalWordTextPublication Publication{get;}
    public IReadOnlyList<LegacyGlobalOrdinaryCurveView> Views{get;private set;}=[];
    private readonly LegacyFreshMainGlobalWordCharacterLink[] links;
    private readonly LegacyOwnedFreshTextSelection[] pending;
    private readonly long next;
    private LegacyGlobalOrdinaryCroppedSource(LegacyFreshMainGlobalWordTextPublication publication,
        LegacyFreshMainGlobalWordCharacterLink[] links,LegacyOwnedFreshTextSelection[] pending)
    {Publication=publication;this.links=links;this.pending=pending;next=publication.Scope.SharedState.Selections.NextSelectionId;}
    public static LegacyGlobalOrdinaryCroppedSource Prepare(LegacyFreshMainGlobalWordTextPublication actual,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actual);token.ThrowIfCancellationRequested();actual.Scope.Validate();
        if(actual.Scope.SharedState.Selections.Failed||!actual.IsComplete||actual.IsFaulted||actual.Wrapper.IsFaulted||actual.Wrapper.Choices.IsFaulted||
            actual.WordSource.ChoiceCount!=1||(actual.WordSource.Flags&0x200000)!=0)
            throw new ArgumentException("Same complete ordinary global WORD/TEXT producer required.");
        var links=actual.WordSource.CopyCharacterLinks().ToArray();var pending=actual.CopyPendingSelections().ToArray();
        if(links.Length is<1 or>4096||pending.Length<3||!ReferenceEquals(pending[^1],actual.Text.Selection)||
            !ReferenceEquals(pending[^2],actual.WordSource.Selection))throw new ArgumentException("Same complete CHAR/WORD/TEXT identities required.");
        var document=actual.Scope.Document;
        var plans=new List<LegacyGlobalRawCropPlan>();long visits=0;
        foreach(var link in links)
        {
            token.ThrowIfCancellationRequested();var s=link.Source;
            if(!s.IsComplete||!ReferenceEquals(s.Owner,actual.Wrapper)||!s.Source.IsOrdinarySource||
                !ReferenceEquals(s.Node.FirstGlobalSource,s.Source)||!ReferenceEquals(s.Node.Owner,actual.Wrapper.CharacterInterner))
                throw new ArgumentException("Same actual ordinary global CHAR key owner required.");
            Add(false,s.Node.Identity,s.Source.CopyRanges(token));
        }
        Add(true,actual.WordSource.Node.Identity,actual.OriginalWordSource.CopyOriginalKey());
        var result=new LegacyGlobalOrdinaryCroppedSource(actual,links,pending);var views=new List<LegacyGlobalOrdinaryCurveView>(plans.Count);
        //All view/count/point work budgets were checked before point copies.
        foreach(var plan in plans)
        {
            token.ThrowIfCancellationRequested();var source=document.Strokes[plan.Object].Points;var copy=new InkPoint[plan.Count];
            int cursor=0;
            for(int p=plan.First;p<=plan.Last;p++)
            {
                token.ThrowIfCancellationRequested();copy[cursor++]=p==plan.First&&plan.Fraction0!=0?
                    Interpolate(source[p],source[p+1],plan.Fraction0):source[p];
            }
            if(plan.Fraction1!=0)copy[cursor++]=Interpolate(source[plan.Last],source[plan.Last+1],plan.Fraction1);
            if(cursor!=copy.Length||copy.Any(p=>!float.IsFinite(p.X)||!float.IsFinite(p.Y)))throw new NotSupportedException("Finite complete raw crop required.");
            views.Add(new(result,plan,copy));
        }
        result.Views=Array.AsReadOnly(views.ToArray());result.Validate(token);return result;

        void Add(bool word,int node,LegacyNativeInkPositionRange[] ranges)
        {
            for(int index=0;index<ranges.Length;index++)
            {
                token.ThrowIfCancellationRequested();var a=ranges[index].First;var b=ranges[index].Last;
                if(a.SourceIndex>b.SourceIndex||b.SourceIndex>=(uint)document.ObjectCount||
                    a.Mode is not(1 or 3)||b.Mode is not(1 or 5)||
                    a.Fraction is>=200 and<255||b.Fraction is>=200 and<255)
                    throw new NotSupportedException("Bounded original raw ranges only; known-text/DOM/transformed sources excluded.");
                for(int obj=(int)a.SourceIndex;obj<=(int)b.SourceIndex;obj++)
                {
                    token.ThrowIfCancellationRequested();int n=document.Strokes[obj].Points.Count;
                    bool hasFirst=obj==a.SourceIndex&&!(a.PointIndex==0&&a.Fraction==0&&a.Mode==3);
                    bool hasLast=obj==b.SourceIndex&&b.Fraction!=255;
                    int first=hasFirst?a.PointIndex:0,last=hasLast?b.PointIndex:n-1;
                    float f0=hasFirst&&a.Fraction!=255?a.Fraction*.005f:0,f1=hasLast?b.Fraction*.005f:0;
                    bool include0=!hasFirst||a.Mode==3,include1=!hasLast||b.Mode==5;
                    if(first<0||last<first||last>=n||f0!=0&&first+1>=n||f1!=0&&last+1>=n||first==last&&f0>f1)
                        throw new NotSupportedException("Ordered finite raw curve crop endpoints required.");
                    var kind=!hasFirst&&!hasLast?LegacyGlobalRawCurveViewKind.OriginalC49:
                        f0==0&&f1==0?LegacyGlobalRawCurveViewKind.IntegerCropC4A:LegacyGlobalRawCurveViewKind.FractionalCropC4B;
                    int count=checked(last-first+1+(f1!=0?1:0));visits+=count;
                    if(plans.Count>=65536||visits>1_048_576)throw new NotSupportedException("Whole cropped-source view/point budget exceeded before coordinate copies.");
                    plans.Add(new(word,node,index,obj,kind,first,last,f0,f1,include0,include1,count));
                }
            }
        }
    }
    internal static InkPoint Interpolate(InkPoint a,InkPoint b,float f)
    {float dx=b.X-a.X,dy=b.Y-a.Y;float x=dx*f,y=dy*f;return new(x+a.X,y+a.Y);}
    public void Validate(CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();Publication.Scope.Validate();
        if(Publication.Scope.SharedState.Selections.Failed||Publication.Scope.SharedState.Selections.NextSelectionId!=next||
            !Publication.IsComplete||Publication.IsFaulted||Publication.Wrapper.IsFaulted||
            !Publication.CopyPendingSelections().SequenceEqual(pending)||!Publication.WordSource.CopyCharacterLinks().SequenceEqual(links))
            throw new InvalidOperationException("Actual global original-source owner changed.");
    }
}
