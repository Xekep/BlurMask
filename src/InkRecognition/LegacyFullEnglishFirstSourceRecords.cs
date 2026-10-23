using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyFullEnglishFirstSourceRecords.Probe")]
namespace InkRecognition;

/// <summary>One immutable full-source720 with exact child/WORD/reference origin.
/// The source pointer placeholder is zero; native pointers/accepted text are not exposed.</summary>
public sealed class LegacyFullEnglishFirstSourceRow
{
    public LegacyFullEnglishFirstSourceRecords Owner{get;}
    public LegacyFullEnglishFirstGlyphEmissionChild Child{get;}
    public int WordOrdinal{get;} public int ReferenceOrdinal{get;} public ulong Reference{get;}
    public bool SoleFlatOverride{get;}
    public LegacyFirstResultSourceRecord Record{get;}
    private readonly LegacyFirstResultSourceRange[] childRanges;
    internal LegacyFullEnglishFirstSourceRow(LegacyFullEnglishFirstSourceRecords owner,LegacyFullEnglishFirstSourceRecordChild child,
        LegacyFullEnglishFirstSourceRecordEntry entry,LegacyFirstResultSourceRecord record)
    {Owner=owner;Child=child.Child;WordOrdinal=entry.Word;ReferenceOrdinal=entry.Ordinal;Reference=entry.Reference;SoleFlatOverride=entry.Flat;Record=record;childRanges=child.Ranges[entry.Range];}
    public LegacyFirstResultSourceRange[] CopyChildRanges()=>childRanges.ToArray();
}

/// <summary>Actual complete full FIRST B7 -> fresh source720 list. Deliberately
/// refuses repeated canonical ordinary keys/empty children before entry. No
/// previous-row reuse, accepted273 text, final guide or application default.</summary>
public sealed class LegacyFullEnglishFirstSourceRecords
{
    public LegacyFullEnglishFirstGlyphEmission Owner{get;}
    private readonly object gate=new();
    private readonly LegacyFullEnglishFirstSourceRecordBudget budget;
    private readonly LegacyFullEnglishFirstSourceRecordPolicy policy;
    private readonly List<LegacyFullEnglishFirstSourceRow> prefix;
    private readonly byte[][] published;
    private bool entered,running,complete,faulted;
    public bool IsComplete{get{lock(gate)return complete;}}public bool IsFaulted{get{lock(gate)return faulted;}}
    public long AdditionalLogicalPeakBytes=>budget.Peak;
    public long LogicalCopyWorkBytes=>budget.Copy;public long LogicalVisitWork=>budget.Visits;
    public int OrdinaryReferenceCount=>policy.RowCount;
    public int ProjectedRangeFragments=>policy.FragmentCount;public int GeometryPointVisits=>policy.PointVisits;
    private LegacyFullEnglishFirstSourceRecords(LegacyFullEnglishFirstGlyphEmission owner,long peak,long copy,long visits,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(owner);Owner=owner;token.ThrowIfCancellationRequested();
        //All capacity reserves precede any parent/child/pool/WORD copying.
        budget=new(peak,copy,visits,token);policy=new(owner,token);
        prefix=new(policy.RowCount);published=policy.Descriptors.Select(d=>d.ToArray()).ToArray();
    }
    public static LegacyFullEnglishFirstSourceRecords Prepare(LegacyFullEnglishFirstGlyphEmission owner,CancellationToken token=default)
        =>new(owner,LegacyFullEnglishFirstSourceRecordBudget.PeakLimit,LegacyFullEnglishFirstSourceRecordBudget.CopyLimit,LegacyFullEnglishFirstSourceRecordBudget.VisitLimit,token);
    internal static LegacyFullEnglishFirstSourceRecords PrepareBounded(LegacyFullEnglishFirstGlyphEmission owner,long peak,long copy,long visits,CancellationToken token=default)
        =>new(owner,peak,copy,visits,token);
    public void Run(CancellationToken token=default)=>RunCore(null,token);
    internal void RunPrepared(Action<string>? checkpoint,CancellationToken token=default)=>RunCore(checkpoint,token);
    private void RunCore(Action<string>? checkpoint,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();policy.RequireComplete();
        lock(gate){token.ThrowIfCancellationRequested();if(entered||running)throw new InvalidOperationException("Full B8 source owner is one-use.");entered=running=true;}
        try
        {
            checkpoint?.Invoke("entry");token.ThrowIfCancellationRequested();
            foreach(var child in policy.Children)
            {
                token.ThrowIfCancellationRequested();checkpoint?.Invoke("child:entry");token.ThrowIfCancellationRequested();
                int begin;byte[] descriptor=policy.Descriptors[child.Input.DescriptorOrdinal].ToArray();
                lock(gate){begin=prefix.Count;BinaryPrimitives.WriteInt32LittleEndian(descriptor.AsSpan(52),begin);BinaryPrimitives.WriteInt32LittleEndian(descriptor.AsSpan(56),begin);published[child.Input.DescriptorOrdinal]=descriptor;}
                foreach(var entry in child.Entries)
                {
                    token.ThrowIfCancellationRequested();ReadOnlyMemory<byte> alternatives=child.Owners[entry.Owner].Records;int selected=entry.Selected;
                    if(entry.Flat)
                    {
                        byte[] overwrite=new byte[40];BinaryPrimitives.WriteUInt16LittleEndian(overwrite.AsSpan(8),policy.Underscore);
                        BinaryPrimitives.WriteSingleLittleEndian(overwrite.AsSpan(12),1);BinaryPrimitives.WriteSingleLittleEndian(overwrite.AsSpan(16),1);
                        alternatives=overwrite;selected=0;
                    }
                    //The SAME owned current-parent snapshot and its actual
                    //pre-NN map enter geometry, never a remapped child bbox.
                    var record=LegacyFirstResultSourceRecord.Create(alternatives.Span,selected,child.Ranges[entry.Range],child.Map,
                        policy.Points,policy.Categories,policy.Exponent,token);
                    var row=new LegacyFullEnglishFirstSourceRow(this,child,entry,record);
                    checkpoint?.Invoke("row:created");token.ThrowIfCancellationRequested();
                    lock(gate){token.ThrowIfCancellationRequested();if(prefix.Count>=policy.RowCount)throw new InvalidDataException("B8 reserved row count exceeded.");prefix.Add(row);}
                    checkpoint?.Invoke("row:published");token.ThrowIfCancellationRequested();
                }
                int count;lock(gate)count=prefix.Count-begin;
                var closed=LegacyFirstResultSourceRecord.PublishDescriptorInterval(descriptor,begin,count,token);
                lock(gate){token.ThrowIfCancellationRequested();published[child.Input.DescriptorOrdinal]=closed;}
                checkpoint?.Invoke("child:closed");token.ThrowIfCancellationRequested();
            }
            lock(gate){token.ThrowIfCancellationRequested();if(prefix.Count!=policy.RowCount)throw new InvalidDataException("Complete B8 source list required.");complete=true;}
        }
        catch{lock(gate)faulted=true;throw;}finally{lock(gate)running=false;}
    }
    /// <summary>Immutable completed row prefix; after failure historical only.</summary>
    public IReadOnlyList<LegacyFullEnglishFirstSourceRow> CopyRows(){lock(gate)return Array.AsReadOnly(prefix.ToArray());}
    public byte[][] CopyDescriptors(){lock(gate)return published.Select(d=>d.ToArray()).ToArray();}
    public byte[][] CopyOriginalDescriptors()=>policy.Descriptors.Select(d=>d.ToArray()).ToArray();
    public InkStroke[] CopyCurrentSource()=>LegacyFullEnglishFirstParentInput.Copy(policy.Points);
    public LegacySourcePointIdentity[] CopyCurrentChronology()=>policy.Chronology.ToArray();
}
