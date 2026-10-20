using System.Buffers.Binary;
namespace InkRecognition;

[Flags]
public enum LegacyGlobalPayloadWrites
{None=0,SelectionId=1,BasicScalars=2,EmptyPriorVector=4,PriorRead=8,DefaultLine=16,AssignedLine=32,ComputedFlags=64}

public readonly record struct LegacyGlobalAssignedPayloadScalars(long? SelectionId,int? Field08,
    long? Field40,int? Field00,int? Field1C,byte? Field64,int? Field48,int? SourceLineId,uint? Flags);

/// <summary>Typed024D40 payload with explicit written-field state. Unwritten
/// native slots have nullable sidecars instead of fabricated zero bytes.</summary>
public sealed class LegacyFreshMainGlobalTextSourcePayload
{
    private readonly LegacyPreparedTextGlyphReuseRecord[] initial;
    public LegacyFreshMainGlobalTextSourceWrapper Owner=>Source.Owner;
    public LegacyFreshMainGlobalProcessedGlyphSource Source{get;}
    public LegacyOwnedMainTextSourceNode Node{get;}
    public LegacyOwnedFreshTextSelection? Selection{get;private set;}
    public long? SelectionId{get;private set;}
    public int? SourceLineId{get;private set;}
    public uint? Flags{get;private set;}
    public byte PriorByte{get;private set;}
    public int PriorVisits{get;private set;}
    public int PriorRangeCount=>0;
    public LegacyGlobalPayloadWrites Written{get;private set;}
    public bool IsComplete{get;private set;}
    public LegacyGlobalAssignedPayloadScalars CopyAssignedScalars()
    {
        Owner.Scope.Validate();bool basic=(Written&LegacyGlobalPayloadWrites.BasicScalars)!=0;
        return new(SelectionId,basic?-1:null,basic?-1L:null,basic?0:null,basic?0:null,
            basic?(byte)0:null,basic?-1:null,SourceLineId,Flags);
    }
    internal LegacyPreparedMainTextGlyphReuse? ReuseKernel{get;private set;}
    internal LegacyFreshMainGlobalTextSourcePayload(LegacyFreshMainGlobalProcessedGlyphSource source,
        LegacyOwnedMainTextSourceNode node,LegacyNativeTextGlyphCallerResult published)
    {
        Source=source;Node=node;initial=published.Records.Select(record=>
        {
            Owner.Choices.StringPool.Validate(record.Text);byte[] scalar=new byte[40];
            BinaryPrimitives.WriteUInt32LittleEndian(scalar.AsSpan(8),record.Flags);
            BinaryPrimitives.WriteUInt32LittleEndian(scalar.AsSpan(32),record.ByteCount);
            return new LegacyPreparedTextGlyphReuseRecord(scalar,record.Text);
        }).ToArray();
    }
    internal bool Initialize(int preparedCharFailure,CancellationToken token)
    {
        Owner.RequireInitializingPayload(this);bool succeeded=true;
        if(Source.IsOrdinarySource)
        {
            Selection=Owner.Scope.SharedState.Selections.CreateGlobalCharacterSelection(Source,preparedCharFailure,token);
            succeeded=Selection is not null;
            if(succeeded){SelectionId=Selection!.SelectionId;Written|=LegacyGlobalPayloadWrites.SelectionId;}
        }
        else{SelectionId=-1;Written|=LegacyGlobalPayloadWrites.SelectionId;}
        //Native false registration still writes these defaults, initializes
        //priorC21 and writes line=-1 before skipping computed flags.
        Written|=LegacyGlobalPayloadWrites.BasicScalars|LegacyGlobalPayloadWrites.EmptyPriorVector;
        if(succeeded&&Source.IsOrdinarySource)
        {
            var prior=Owner.ReadPrior(Source,token);PriorByte=prior.Prior;PriorVisits=prior.Visits;
            Written|=LegacyGlobalPayloadWrites.PriorRead;
        }
        SourceLineId=-1;Written|=LegacyGlobalPayloadWrites.DefaultLine;
        if(!succeeded)return false;
        SourceLineId=Owner.ReadSourceLine(Source,token);Written|=LegacyGlobalPayloadWrites.AssignedLine;
        uint aggregate=0;foreach(var row in initial)aggregate|=row.Flags;
        var key=Source.CopyRanges(token);uint degenerate=Source.IsOrdinarySource&&key.Length==1&&SameCoordinate(key[0])?0x800000u:0;
        Flags=(((uint)(PriorByte&1)<<24)^((Source.IsOrdinarySource?0u:0x200000u)|(aggregate&0xce1fffffu)|degenerate))&0xfdffffffu;
        Written|=LegacyGlobalPayloadWrites.ComputedFlags;
        ReuseKernel=LegacyPreparedMainTextGlyphReuse.CreateGlobal(this,initial,Flags.Value);IsComplete=true;return true;
    }
    internal LegacyPreparedTextGlyphReuseResult Reuse(LegacyGlobalSourceSelectedGlyph selected,CancellationToken token)
    {
        if(!IsComplete||ReuseKernel is null)throw new InvalidOperationException("Successful typed global source payload required.");
        var result=ReuseKernel.ApplyGlobal(this,selected,token);Flags=ReuseKernel.AggregateFlags;return result;
    }
    public IReadOnlyList<LegacyPreparedTextGlyphReuseRecord> CopyRecords(CancellationToken token=default)
    {Owner.Scope.Validate();token.ThrowIfCancellationRequested();return ReuseKernel?.CopyRecords()??Array.AsReadOnly(initial.ToArray());}
    private static bool SameCoordinate(LegacyNativeInkPositionRange r)
        =>r.First.SourceIndex==r.Last.SourceIndex&&r.First.PointIndex==r.Last.PointIndex&&
          (r.First.Fraction==r.Last.Fraction||r.First.Fraction==255||r.Last.Fraction==255);
}
