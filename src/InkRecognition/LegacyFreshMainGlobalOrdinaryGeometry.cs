using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyGlobalTimedGeometry.Probe")]
namespace InkRecognition;

public readonly record struct LegacyOrdinaryGeometryMutation(int Ordinal,int CharacterIndex,int SourceNode,
    uint Before,uint After,string Stage);
public readonly record struct LegacyOrdinaryGeometryBoundary(float? Coordinate,int? CharacterIndex);
public readonly record struct LegacyOrdinaryGeometryScratchSnapshot(int CharacterIndex,uint Written,int? Previous,int? Next,
    double? WeightedCount,double? WeightedSumX,double? WeightedSumX2,double? MeanX,double? StandardDeviationX,
    float? MinimumX,float? MaximumX,long? MinimumViewStart,long? MaximumViewStart,bool? Skipped,bool? GapClass,bool? Special);
public readonly record struct LegacyOrdinaryGeometryVerticalMoments(double Count,double SumY,double SumY2,
    float MinimumX,float MaximumX,float MinimumY,float MaximumY);
public readonly record struct LegacyOrdinaryGeometryCharacter(int SourceNode,int Marker,long SelectionId,
    bool StructureWritten,int? LastUtf8ByteIndex,int WordIndex,int SourceLine,uint InitialMetadata,uint EffectiveMetadata,uint SelectedFlags,
    float? MinimumX,float? MaximumX,float? MeanX,float? StandardDeviationX,long MinimumViewStart,long MaximumViewStart);
public readonly record struct LegacyOrdinaryGeometryWord(int SourceNode,int ChosenIndex,long SelectionId,
    int CharacterStart,int CharacterEnd,int LineStart,int LineEnd,long? MinimumViewStart,long? MaximumViewStart);
public readonly record struct LegacyOrdinaryGeometryLine(int SourceLine,int CharacterStart,int CharacterEnd,
    float? MinimumX,float? MaximumX,float? MeanY,float? StandardDeviationY,float? BaselineAtMinimumX,
    float? BaselineAtMaximumX,bool? Reversed);
public sealed record LegacyOrdinaryGeometrySnapshot(string Stage,bool Ready,bool Faulted,
    LegacyOrdinaryGeometryWord? Word,IReadOnlyList<LegacyOrdinaryGeometryCharacter> Characters,
    LegacyOrdinaryGeometryLine? Line,ReadOnlyMemory<byte> SelectedUtf8,
    double? MeanAngle,double? Cosine,double? Sine,IReadOnlyList<float> VerticalBoundaries,
    IReadOnlyList<LegacyOrdinaryGeometryBoundary> HorizontalBoundaries,float? GlobalMinimumX,float? GlobalMaximumX,
    float? MeanWidth,float? MeanSpacing,IReadOnlyList<LegacyOrdinaryGeometryMutation> Mutations,
    IReadOnlyList<string> Trace,LegacyOrdinaryGeometryVerticalMoments? VerticalMoments,
    IReadOnlyList<LegacyOrdinaryGeometryScratchSnapshot> Scratch);

//Native CHAR80.Source points to a shared source+18 flag word. The phase owns
//this mutable cell; payload identity, not equal keys or local NodeID, aliases it.
internal sealed class LegacyOrdinaryGeometryMetadata(LegacyFreshMainGlobalTextSourcePayload source,uint flags)
{
    internal LegacyFreshMainGlobalTextSourcePayload Source{get;}=source;
    internal uint Initial{get;}=flags;
    internal uint Flags=flags;
}
internal sealed class LegacyOrdinaryGeometryMetadataMap
{
    private readonly Dictionary<LegacyFreshMainGlobalTextSourcePayload,LegacyOrdinaryGeometryMetadata> cells=new(ReferenceEqualityComparer.Instance);
    internal LegacyOrdinaryGeometryMetadata Get(LegacyFreshMainGlobalTextSourcePayload source)
    {
        if(!source.IsComplete||source.Flags is not{} flags)throw new InvalidOperationException("Complete metadata source required.");
        if(!cells.TryGetValue(source,out var cell))cells.Add(source,cell=new(source,flags));
        if(cell.Initial!=flags)throw new InvalidOperationException("Original source metadata changed during preclaim capture.");
        return cell;
    }
}
internal sealed class LegacyOrdinaryGeometryCharacterState
{
    internal required LegacyFreshMainGlobalWordCharacterLink Link;
    internal required LegacyGlobalAssignedPayloadScalars Scalars;
    internal required byte[] Key;
    internal required byte[] Row;
    internal required LegacyNativeInternedUtf8 Text;
    internal required byte[] Utf8;
    internal uint Initial,Selected;
    internal LegacyOrdinaryGeometryMetadata? Metadata;
    private uint preparedFlags;
    internal uint Flags
    {
        get=>Metadata?.Flags??preparedFlags;
        set{if(Metadata is{} shared)shared.Flags=value;else preparedFlags=value;}
    }
    internal int? LastByte;
    internal int Marker;
    internal bool StructureWritten;
    internal LegacyOrdinaryGeometryView[] Views=[];
    internal float? Min,Max,Mean,Std;
    internal long TimeMin=long.MaxValue,TimeMax=long.MinValue;
    internal LegacyOrdinaryGeometryCharacter Snapshot()=>new(Link.Source.Node.Identity,Marker,Scalars.SelectionId!.Value,
        StructureWritten,LastByte,0,0,Initial,Flags,Selected,Min,Max,Mean,Std,TimeMin,TimeMax);
}
internal readonly record struct LegacyOrdinaryGeometryView(LegacyGlobalOrdinaryCurveView Actual,InkPoint[] Points,long Start);
internal readonly record struct LegacyOrdinaryGeometryGuide(double X,double Y,double Angle,double Height,double Baseline);

/// <summary>Native050660/04B4C0/04E480 ordinary numerical phase on a SAME
/// genuine WORD/TEXT parent and NEW timed raw ingress. No238B0/DOM acceptance.</summary>
public sealed class LegacyFreshMainGlobalOrdinaryGeometry
{
    private static readonly ConditionalWeakTable<LegacyFreshMainGlobalWordTextPublication,LegacyFreshMainGlobalOrdinaryGeometry> owners=new();
    private readonly object gate=new();
    private readonly LegacyFreshMainGlobalWordCharacterLink[] links;
    private readonly LegacyOwnedFreshTextSelection[] pending;
    private readonly LegacyOrdinaryGeometryCharacterState[] characters;
    private readonly LegacyOrdinaryGeometryView[] views;
    private readonly byte[] sourceLines,word48,wordKey;
    private readonly uint wordFlags;
    private readonly long wordSelection,next;
    private readonly List<string> trace=[];
    private readonly List<LegacyOrdinaryGeometryMutation> mutations=[];
    private readonly List<byte> selected;
    private LegacyOrdinaryGeometryWord? word;
    private LegacyOrdinaryGeometryLine? line;
    private float[] vertical=[];
    private LegacyOrdinaryGeometryBoundary[] horizontal=[];
    private double? meanAngle,cosine,sine;
    private float? minimum,maximum,width,spacing;
    private LegacyOrdinaryGeometryVerticalMoments? moments;
    private LegacyOrdinaryGeometryScratch[] scratch=[];
    private bool entered,ready,faulted;
    private string stage="prepared";
    public LegacyFreshMainGlobalWordTextPublication Publication{get;}
    public LegacyGlobalOrdinaryCroppedSource CroppedSource{get;}
    public LegacyPhysicalRawInkTiming TimingReceipt{get;}

    private LegacyFreshMainGlobalOrdinaryGeometry(LegacyFreshMainGlobalWordTextPublication parent,
        LegacyGlobalOrdinaryCroppedSource cropped,LegacyPhysicalRawInkTiming timing,
        LegacyFreshMainGlobalWordCharacterLink[] links,LegacyOrdinaryGeometryCharacterState[] characters,
        LegacyOrdinaryGeometryView[] views,byte[] sourceLines)
    {
        Publication=parent;CroppedSource=cropped;TimingReceipt=timing;this.links=links;this.characters=characters;
        selected=new(characters.Sum(c=>c.Utf8.Length));
        this.views=views;this.sourceLines=sourceLines;pending=parent.CopyPendingSelections().ToArray();
        next=parent.Scope.SharedState.Selections.NextSelectionId;word48=parent.WordSource.CopyChoiceScalar48();
        wordFlags=parent.WordSource.Flags;wordSelection=parent.WordSource.SelectionId;wordKey=parent.WordSource.Node.CopyRanges16();
    }
    public static LegacyFreshMainGlobalOrdinaryGeometry Prepare(LegacyFreshMainGlobalWordTextPublication parent,
        LegacyGlobalOrdinaryCroppedSource cropped,LegacyPhysicalRawInkTiming timing,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(parent);ArgumentNullException.ThrowIfNull(cropped);ArgumentNullException.ThrowIfNull(timing);
        token.ThrowIfCancellationRequested();parent.Scope.Validate();cropped.Validate(token);timing.Validate(token);
        if(!ReferenceEquals(parent,cropped.Publication)||!ReferenceEquals(parent.Scope.Document,timing.Document)||
            !parent.IsComplete||parent.IsFaulted||parent.Wrapper.IsFaulted||parent.Wrapper.Choices.IsFaulted||
            parent.Scope.SharedState.Selections.Failed||parent.LineCount!=1||parent.AlternativeCount!=1||
            parent.WordSource.ChoiceCount!=1||parent.SelectedWordIndex!=0||
            (parent.WordSource.Flags&0x10200000)!=0)
            throw new NotSupportedException("Same complete timed ordinary oneWORD/oneALT/one-line global producer required.");
        var links=parent.WordSource.CopyCharacterLinks().ToArray();var sourceLines=parent.CopySourceLines56();
        if(links.Length is<1 or>4096||sourceLines.Length!=56||cropped.Views.Count is<1 or>65536)
            throw new NotSupportedException("Whole native geometry source/view/line budget exceeded.");
        var guide=Guide(sourceLines);
        if(!double.IsFinite(guide.X)||!double.IsFinite(guide.Y)||!double.IsFinite(guide.Angle)||!double.IsFinite(guide.Height)||
            Math.Abs(guide.X)>1_000_000||Math.Abs(guide.Y)>1_000_000||Math.Abs(guide.Angle)>Math.PI||guide.Height is<=0 or>1_000_000||
            (float)guide.Height<=0)
            throw new NotSupportedException("Finite bounded positive actual source guide required.");
        long rows=0,bytes=0,visits=0,keyBytes=parent.WordSource.Node.RangeCount*16L;
        foreach(var link in links)
        {
            token.ThrowIfCancellationRequested();var s=link.Source;var records=s.CopyRecords(token);rows+=records.Count;
            keyBytes+=s.Node.RangeCount*16L;
            if(keyBytes>4*1024*1024)throw new NotSupportedException("Whole geometry source-key snapshot budget exceeded.");
            if(rows>65536||!s.IsComplete||!s.Source.IsOrdinarySource||!ReferenceEquals(s.Owner,parent.Wrapper)||
                !ReferenceEquals(s.Node.FirstGlobalSource,s.Source)||s.SourceLineId!=0||s.SelectionId is null||
                s.Flags is null||(s.Flags.Value&0x10600000)!=0||(uint)link.Marker>=(uint)records.Count)
                throw new NotSupportedException("Actual same-parent assigned ordinary source metadata/marker required.");
            var row=records[link.Marker];bytes+=row.ByteCount;
            if((row.Flags&0xc00)!=0||row.ByteCount is<1 or>4||row.Text is null||bytes>4*1024*1024)
                throw new NotSupportedException("Positive selected UTF8 records without special/suppressed400/C00 branches required.");
            parent.Wrapper.Choices.StringPool.Validate(row.Text);
        }
        foreach(var view in cropped.Views)
        {
            token.ThrowIfCancellationRequested();visits+=view.PointCount;
            if(visits>1_048_576)throw new NotSupportedException("Whole geometry point visit budget exceeded.");
            _=timing.ReadViewStartValidated(view); //includes every fractional conversion before lifetime claim
        }
        //Root cropped views are emitted in CHAR LINK order, then WORD order.
        //Repeated node keys retain separate occurrence blocks; never aggregate
        //all equal-node copies into every repeated CHAR's numerical input.
        var counts=new int[links.Length];int cursor=0;
        for(int i=0;i<links.Length;i++)
        {
            int start=cursor;
            for(int range=0;range<links[i].Source.Node.Key.Length;range++)
            {
                var r=links[i].Source.Node.Key[range];
                for(uint raw=r.First.SourceIndex;raw<=r.Last.SourceIndex;raw++)
                {
                    token.ThrowIfCancellationRequested();
                    if(cursor>=cropped.Views.Count)throw new InvalidOperationException("CHAR view occurrence missing.");
                    var v=cropped.Views[cursor++];
                    if(v.IsWord||v.SourceNodeIdentity!=links[i].Source.Node.Identity||v.RangeOrdinal!=range||v.RawObjectIndex!=raw)
                        throw new InvalidOperationException("Actual CHAR link/range/raw occurrence order changed.");
                }
            }
            counts[i]=cursor-start;
            if(counts[i]==0)throw new NotSupportedException("Positive ordinary CHAR source ranges required.");
        }
        int wordStart=cursor;
        var originalWordRanges=parent.OriginalWordSource.CopyOriginalKey();
        for(int range=0;range<originalWordRanges.Length;range++)
        {
            var r=originalWordRanges[range];
            for(uint raw=r.First.SourceIndex;raw<=r.Last.SourceIndex;raw++)
            {
                token.ThrowIfCancellationRequested();
                if(cursor>=cropped.Views.Count)throw new InvalidOperationException("WORD view occurrence missing.");
                var v=cropped.Views[cursor++];
                if(!v.IsWord||v.SourceNodeIdentity!=parent.WordSource.Node.Identity||v.RangeOrdinal!=range||v.RawObjectIndex!=raw)
                    throw new InvalidOperationException("Actual WORD source occurrence changed.");
            }
        }
        if(cursor!=cropped.Views.Count||cursor==wordStart)throw new NotSupportedException("Complete positive WORD point views required.");
        //Includes both view-reference arrays, selected buffer/export copies,
        //typed source/scratch/snapshot records and row/key/point payloads.
        //CLR object/allocator overhead is not called native physical memory.
        long payload=checked(rows*40+bytes*4+keyBytes+visits*8+cropped.Views.Count*64L+links.Length*512L+sourceLines.Length);
        if(payload>32*1024*1024)throw new NotSupportedException("Whole typed geometry payload budget exceeded.");
        var states=new LegacyOrdinaryGeometryCharacterState[links.Length];
        var metadata=new LegacyOrdinaryGeometryMetadataMap();
        for(int i=0;i<states.Length;i++)
        {
            token.ThrowIfCancellationRequested();var link=links[i];var s=link.Source;var row=s.CopyRecords(token)[link.Marker];
            byte[] text=row.Text!.ReadBytes();
            if(text.Length!=row.ByteCount)throw new InvalidOperationException("Actual selected string byte count changed.");
            var shared=metadata.Get(s);
            states[i]=new(){Link=link,Metadata=shared,Scalars=s.CopyAssignedScalars(),Key=s.Node.CopyRanges16(),Row=row.CopyStorage40(),
                Text=row.Text,Utf8=text,Initial=shared.Initial,Selected=row.Flags,Marker=link.Marker};
        }
        var preparedViews=cropped.Views.Select(v=>new LegacyOrdinaryGeometryView(v,v.CopyPoints(token),timing.ReadViewStartValidated(v))).ToArray();
        cursor=0;
        for(int i=0;i<states.Length;i++){states[i].Views=preparedViews.AsSpan(cursor,counts[i]).ToArray();cursor+=counts[i];}
        var candidate=new LegacyFreshMainGlobalOrdinaryGeometry(parent,cropped,timing,links,states,preparedViews,sourceLines);
        candidate.Validate(token);
        var actual=owners.GetValue(parent,_=>candidate);
        if(!ReferenceEquals(actual.CroppedSource,cropped)||!ReferenceEquals(actual.TimingReceipt,timing))
            throw new InvalidOperationException("Actual parent is already reserved by another typed geometry claim.");
        actual.Validate(token);return actual;
    }
    internal static LegacyOrdinaryGeometryGuide Guide(byte[] row)=>new(
        BinaryPrimitives.ReadDoubleLittleEndian(row.AsSpan(8)),BinaryPrimitives.ReadDoubleLittleEndian(row.AsSpan(16)),
        BinaryPrimitives.ReadDoubleLittleEndian(row.AsSpan(24)),BinaryPrimitives.ReadDoubleLittleEndian(row.AsSpan(32)),
        BinaryPrimitives.ReadDoubleLittleEndian(row.AsSpan(48)));
    public void Validate(CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();Publication.Scope.Validate();CroppedSource.Validate(token);TimingReceipt.Validate(token);
        if(!Publication.IsComplete||Publication.IsFaulted||Publication.Scope.SharedState.Selections.Failed||
            Publication.Scope.SharedState.Selections.NextSelectionId!=next||
            !Publication.CopyPendingSelections().SequenceEqual(pending)||!Publication.WordSource.CopyCharacterLinks().SequenceEqual(links)||
            Publication.WordSource.Flags!=wordFlags||Publication.WordSource.SelectionId!=wordSelection||
            !Publication.WordSource.CopyChoiceScalar48().AsSpan().SequenceEqual(word48)||
            !Publication.WordSource.Node.CopyRanges16().AsSpan().SequenceEqual(wordKey)||
            !Publication.CopySourceLines56().AsSpan().SequenceEqual(sourceLines))
            throw new InvalidOperationException("Actual global producer/selection/guide changed after geometry reservation.");
        foreach(var c in characters)
        {
            token.ThrowIfCancellationRequested();var s=c.Link.Source;var records=s.CopyRecords(token);
            if(!s.IsComplete||c.Metadata is not{} cell||!ReferenceEquals(cell.Source,s)||cell.Initial!=c.Initial||c.Scalars.Flags!=c.Initial||s.CopyAssignedScalars()!=c.Scalars||!s.Node.CopyRanges16().AsSpan().SequenceEqual(c.Key)||
                (uint)c.Marker>=(uint)records.Count||!ReferenceEquals(records[c.Marker].Text,c.Text)||
                !records[c.Marker].CopyStorage40().AsSpan().SequenceEqual(c.Row)||!c.Text.ReadBytes().AsSpan().SequenceEqual(c.Utf8))
                throw new InvalidOperationException("Actual chosen CHAR scalar/string/source key changed.");
        }
    }
    public LegacyOrdinaryGeometrySnapshot Run(CancellationToken token=default)=>RunCore(0,false,token);
    //Internal diagnostic boundary only: no public callback/native flags are
    //accepted and no input/source node is substituted for this phase's owner.
    internal LegacyOrdinaryGeometrySnapshot RunPrepared(int boundary,bool cancel=false,CancellationToken token=default)
    {
        if(boundary is<1 or>6)throw new ArgumentOutOfRangeException(nameof(boundary));
        return RunCore(boundary,cancel,token);
    }
    private LegacyOrdinaryGeometrySnapshot RunCore(int boundary,bool cancel,CancellationToken token)
    {
        lock(gate)
        {
            Validate(token);if(entered)throw new InvalidOperationException("Geometry claim is single-use, including failed prefixes.");
            entered=true;
            try
            {
                word=new(Publication.WordSource.Node.Identity,Publication.SelectedWordIndex,wordSelection,0,characters.Length,0,1,null,null);
                minimum=float.MaxValue;maximum=BitConverter.Int32BitsToSingle(0x00800000);width=spacing=0;
                foreach(var c in characters){token.ThrowIfCancellationRequested();selected.AddRange(c.Utf8);c.LastByte=selected.Count-1;c.StructureWritten=true;}
                //Native line48 was memset before these pointer/interval writes.
                line=new(0,0,characters.Length,0,0,0,0,0,0,false);
                Enter("structure");
                var guide=Guide(sourceLines);meanAngle=guide.Angle;cosine=Math.Cos(guide.Angle);sine=Math.Sin(guide.Angle);
                Enter("rotation");
                var data=LegacyGlobalOrdinaryGeometryKernels.Vertical(views.Where(v=>v.Actual.IsWord).ToArray(),guide,
                    meanAngle.Value,cosine.Value,sine.Value,token);
                line=line.Value with{MinimumX=data.MinimumX,MaximumX=data.MaximumX,MeanY=data.MeanY,
                    StandardDeviationY=data.StandardDeviationY,BaselineAtMinimumX=data.Baseline0,BaselineAtMaximumX=data.Baseline1};
                vertical=[data.Top,data.Bottom];
                moments=new(data.Count,data.SumY,data.SumY2,data.MinimumX,data.MaximumX,data.MinimumY,data.MaximumY);
                Enter("vertical");
                scratch=Enumerable.Range(0,characters.Length).Select(_=>new LegacyOrdinaryGeometryScratch()).ToArray();
                horizontal=new LegacyOrdinaryGeometryBoundary[characters.Length+1];
                var horizontalResult=LegacyGlobalOrdinaryGeometryKernels.Horizontal(characters,views,guide,
                    data,cosine.Value,sine.Value,scratch,horizontal,WriteMetadata,
                    reversed=>line=line!.Value with{Reversed=reversed},
                    (meanWidth,meanSpacing)=>{if(meanWidth is{} w)width=w;if(meanSpacing is{} s)spacing=s;},Enter,token);
                horizontal=horizontalResult.Boundaries;line=line.Value with{Reversed=horizontalResult.Reversed};
                width=horizontalResult.MeanWidth;spacing=horizontalResult.MeanSpacing;
                stage="horizontal";trace.Add(stage);
                minimum=horizontal[0].Coordinate!.Value;maximum=horizontal[^1].Coordinate!.Value;
                word=word.Value with{MinimumViewStart=characters.Min(c=>c.TimeMin),MaximumViewStart=characters.Max(c=>c.TimeMax)};
                ready=true;stage="ready";trace.Add(stage);Validate(token);return SnapshotCore();
            }
            catch{ready=false;faulted=true;trace.Add("failed:"+stage);throw;}
        }
        void Enter(string value)
        {
            stage=value;trace.Add(value);token.ThrowIfCancellationRequested();
            bool hit=boundary switch{1=>value=="structure",2=>value=="rotation",3=>value=="vertical",
                4=>value=="04E0E0",5=>value=="04DA20",6=>value=="04DD50",_=>false};
            if(hit)
            {
                if(cancel)throw new OperationCanceledException("Prepared deterministic geometry boundary.");
                throw new InvalidOperationException("Prepared deterministic geometry prefix failure.");
            }
        }
    }
    private void WriteMetadata(int character,uint flags,string writer)
    {
        var c=characters[character];uint before=c.Flags;c.Flags=flags;
        mutations.Add(new(mutations.Count,character,c.Link.Source.Node.Identity,before,flags,writer));
    }
    public LegacyOrdinaryGeometrySnapshot CopySnapshot(CancellationToken token=default)
    {lock(gate){Validate(token);return SnapshotCore();}}
    private LegacyOrdinaryGeometrySnapshot SnapshotCore()=>new(stage,ready,faulted,word,
        Array.AsReadOnly(characters.Select(c=>c.Snapshot()).ToArray()),line,selected.ToArray(),meanAngle,cosine,sine,
        Array.AsReadOnly(vertical.ToArray()),Array.AsReadOnly(horizontal.ToArray()),minimum,maximum,width,spacing,
        Array.AsReadOnly(mutations.ToArray()),Array.AsReadOnly(trace.ToArray()),moments,
        Array.AsReadOnly(scratch.Select((s,i)=>s.Snapshot(i)).ToArray()));
}
