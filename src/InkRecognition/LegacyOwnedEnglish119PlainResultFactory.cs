using System.Buffers.Binary;
namespace InkRecognition;

/// <summary>Same completed119 terminal's native first plain-result policy.
/// This is a semantic pool/WORD owner, not a serialized TOP18/V50 transport.</summary>
internal sealed class LegacyOwnedEnglish119PlainResultFactory
{
    internal LegacyEnglish119TerminalResult Terminal {get;}
    internal LegacyEnglish119TerminalAlternative Alternative {get;}
    internal LegacyOwnedEnglish119FirstPolicy Policy=>Terminal.Policy;
    internal LegacyOwnedEnglish119Candidates Candidates {get;}
    internal LegacyOwnedEnglishLiteFullPreprocessing Preparation=>Candidates.Preparation;
    internal LegacyOrdinaryCursiveCandidateGraph Graph=>Policy.SourceRanges.Graph;
    internal LegacyCursiveCategoryProfile Categories=>Policy.Categories;
    internal LegacyOrdinaryRawInputDescriptor AssignedDescriptor {get;}
    internal bool CaptureGeometry=>false;
    internal bool OwnerC4=>false;
    internal bool Configuration6C=>true;
    internal bool FinalizeGeometry {get;}
    private readonly ushort headerE;
    private readonly LegacyFirstPlainGlyphCache?[] caches;

    private LegacyOwnedEnglish119PlainResultFactory(LegacyEnglish119TerminalResult terminal,CancellationToken token)
    {
        Terminal=terminal;Candidates=Policy.Owner.Owner.Owner.Owner;var source=Policy.Owner.Model.Source;
        if(!ReferenceEquals(terminal.Context.Owner.Primary.Owner.Policy,Policy)||
            !ReferenceEquals(Preparation.Model,Policy.Owner.Model)||!ReferenceEquals(Preparation.Source,source)||
            !ReferenceEquals(Policy.SourceRanges,Candidates.SourceRanges)||
            !ReferenceEquals(Policy.SourceRanges.OriginalSource,Preparation.OriginalSource)||
            terminal.Invocation.Configuration99||terminal.Invocation.ConfigurationA8||Policy.NativeModeE0||
            source.SourceKind!=0x135||terminal.Context.Owner.MergedColumnsA4!=1||terminal.Alternatives.Count!=1)
            throw new NotSupportedException("One actual fresh ordinary selected119 terminal and its own source/cache required.");
        Alternative=terminal.Alternatives[0];
        if(Alternative.Cells.Count is <1 or >2047)
            throw new NotSupportedException("Native plain first materialization has a portable2047-cell budget.");
        var raw=Preparation.RawDescriptor;var counts=Candidates.CopyCutCounts();
        if(raw.Flags!=0x10||raw.CurrentLineId!=-1||raw.ScopeId!=-1||raw.FirstPhysicalStroke!=0||
            raw.LastPhysicalStroke!=Preparation.OriginalSource.Strokes.Count-1||counts.Length!=Preparation.OriginalSource.Strokes.Count)
            throw new ArgumentException("Matching unscoped ordinary ingress descriptor and retained physical domain required.");
        //16A17C guide count80=0;16E972 appends/increments first guide;
        //16EAC1 writes descriptor10=count-1=0, before169750/16E570.
        //24C/265 preserve this assignment. RawDescriptor stays preassignment.
        AssignedDescriptor=raw with{CurrentLineId=0};
        var bytes=source.CopyChildEnvelopeBytes(Candidates.GraphChild,token);
        var span=LegacyCursiveLatticeSpanProfile.ReadV10aResourceNode(bytes,0);
        if(span.NativeSecondByte!=1||bytes[350]!=0)
            throw new NotSupportedException("Assigned ordinary graph09=1/resource159=0 delimiter scope required.");
        byte b9=source.Fields.Single(f=>f.Owner=="247"&&f.Destination=="B9").CopyBytes()[0];
        if(b9>1)throw new InvalidDataException("Native boolean247B9 required.");
        FinalizeGeometry=b9!=0;
        if(FinalizeGeometry)throw new NotSupportedException("First word geometry finalization/order remains unported.");
        headerE=BinaryPrimitives.ReadUInt16LittleEndian(source.Alphabet.CopySerializedHeader().AsSpan(6));
        if(headerE>=source.Alphabet.LabelCount)throw new NotSupportedException("Actual literal blank header label required.");
        caches=new LegacyFirstPlainGlyphCache?[Graph.Groups.Count];
        foreach(var cell in Alternative.Cells)
        {
            token.ThrowIfCancellationRequested();var record=cell.CopyRecord().Span;
            if(record[32]!=0||cell.Symbol==Policy.Shadda||cell.Symbol>Policy.Blank+2)
                throw new NotSupportedException("Fresh zero-trait literal first symbol scope required.");
        }
    }
    internal static LegacyOwnedEnglish119PlainResultFactory Prepare(LegacyEnglish119TerminalResult terminal,CancellationToken token)
    {ArgumentNullException.ThrowIfNull(terminal);token.ThrowIfCancellationRequested();return new(terminal,token);}
    internal LegacyFirstPlainTextStorage CreateStorage(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(CaptureGeometry,token);
        return new(pool,new LegacyNativePlainWordPublisher(pool,OwnerC4,FinalizeGeometry,FinalizeGeometry));
    }
    internal bool ShouldAllocate(ushort symbol,CancellationToken token)
    {token.ThrowIfCancellationRequested();if(symbol>Policy.Blank+2)throw new ArgumentOutOfRangeException(nameof(symbol));return symbol!=Policy.Blank&&symbol!=Policy.Closed;}
    internal LegacyFirstLiteralGlyphSymbolPlan CreatePlan(ushort selected,byte composite,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(selected>Policy.Blank+2||composite>63)throw new ArgumentOutOfRangeException(nameof(selected));
        ushort published=selected==Policy.Blank?headerE:selected;
        if(published>=Categories.StartCompositeMap.Length)throw new ArgumentException("Selected symbol escapes its own category storage.");
        int category=Categories.StartCompositeMap.Span[published];
        byte geometry=category<0?(byte)0:checked((byte)Categories.CompositeCategories.Span[category].GeometryIndex);
        ushort mismatch=published==headerE||category==composite?(ushort)0:(ushort)8;
        return new(selected,published,category,geometry,mismatch,published==Policy.Replacement?(ushort)0x48:(ushort)0x40);
    }
    internal void ApplySymbol(LegacyNativeGlyphRecordPool pool,ulong reference,LegacyFirstLiteralGlyphSymbolPlan plan,byte composite,CancellationToken token)
    {
        if(plan!=CreatePlan(plan.SelectedLabel,composite,token))throw new ArgumentException("Literal plan differs from its actual source.");
        LegacyNativeFirstLiteralGlyphKernel.Apply(pool,reference,plan,token);
    }
    internal LegacyFirstPlainGlyphCache Cache(int group,CancellationToken token)
    {
        if((uint)group>=(uint)caches.Length)throw new ArgumentOutOfRangeException(nameof(group));
        if(caches[group] is{} found)return found;
        token.ThrowIfCancellationRequested();var actual=Policy.Owner.ScoreGroup(group,token);var confidence=actual.Confidence;
        if(!ReferenceEquals(actual.Owner,Policy.Owner)||!ReferenceEquals(confidence.Owner,Policy.Owner.Owner)||
            !ReferenceEquals(actual.ActualCache,confidence.ActualCache)||!ReferenceEquals(actual.ActualCache.Raw.Owner,Candidates))
            throw new ArgumentException("Same actual ranked, confidence and classified group required.");
        var values=confidence.CopyValues();float first=values.Length==0?0:values[0].Value;
        float Score(ushort symbol,int channel,CancellationToken token)
        {if(channel!=0)throw new NotSupportedException("Fresh native owner88 channel0 required.");return confidence.ReadSymbolScore(symbol,token);}
        int Cost(ReadOnlySpan<byte> request,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();if(request.Length!=16)throw new ArgumentException("Complete private resolved-group cost request required.");
            return actual.ReadClassCost(BinaryPrimitives.ReadUInt16LittleEndian(request[8..]),
                BinaryPrimitives.ReadUInt16LittleEndian(request[6..]),(sbyte)request[10],request[11]!=0,request[12]!=0,token);
        }
        return caches[group]=new(Score,Cost,token=>{token.ThrowIfCancellationRequested();return first;});
    }
}
