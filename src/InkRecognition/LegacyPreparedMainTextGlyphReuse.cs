using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMainTextGlyphReuse.Probe")]

namespace InkRecognition;

/// <summary>One40-byte text-source record with its pointer16 represented by
/// an owner-local interned string sidecar. Scalar storage keeps that slot zero;
/// no managed identity is serialized as a native pointer.</summary>
public sealed class LegacyPreparedTextGlyphReuseRecord
{
    private readonly byte[] storage;
    public LegacyNativeInternedUtf8? Text { get; }
    public uint Flags => BinaryPrimitives.ReadUInt32LittleEndian(storage.AsSpan(8));
    public uint ByteCount => BinaryPrimitives.ReadUInt32LittleEndian(storage.AsSpan(32));
    public LegacyPreparedTextGlyphReuseRecord(ReadOnlySpan<byte> scalarStorage40, LegacyNativeInternedUtf8? text)
    {
        if (scalarStorage40.Length != 40 || BinaryPrimitives.ReadUInt64LittleEndian(scalarStorage40[16..]) != 0)
            throw new ArgumentException("Complete portable40-byte scalar storage with a separate pointer16 sidecar required.");
        storage = scalarStorage40.ToArray(); Text = text;
    }
    public byte[] CopyStorage40() => storage.ToArray();
}

public readonly record struct LegacyPreparedTextGlyphReuseResult(bool Succeeded, int AdjustedMarkerIndex,
    int OriginalRecordCount, int VisitedInputCount, int AppendedRecordCount);

/// <summary>Complete bounded021760 reuse kernel. Existing source-node records
/// remain explicitly prepared inputs until024F30/01B4E0 are
/// joined. Every production emission still resolves an actual same-TOP glyph.</summary>
public sealed class LegacyPreparedMainTextGlyphReuse
{
    private sealed class Row
    {
        internal readonly byte[] Storage;
        internal LegacyNativeInternedUtf8? Text;
        internal Row(byte[] storage, LegacyNativeInternedUtf8? text) { Storage = storage; Text = text; }
    }
    private readonly List<Row> rows = [];
    private readonly LegacyNativeUtf8StringPool strings;
    private readonly LegacyNativeTextPublicationData properties;
    private readonly LegacyNativeUnicodeNormalizationData normalization;
    private readonly object gate = new();
    private bool failed;
    public LegacyFreshMainGlyphUtf8Emission Emission { get; }
    public uint AggregateFlags { get; private set; }
    public int RecordCount { get { lock(gate) return rows.Count; } }

    public LegacyPreparedMainTextGlyphReuse(LegacyFreshMainGlyphUtf8Emission actualEmission,
        LegacyNativeUtf8StringPool actualStringPool, LegacyNativeTextPublicationData properties,
        LegacyNativeUnicodeNormalizationData normalization,
        IReadOnlyList<LegacyPreparedTextGlyphReuseRecord> existingRecords, uint aggregateFlags = 0)
    {
        ArgumentNullException.ThrowIfNull(actualEmission); ArgumentNullException.ThrowIfNull(actualStringPool);
        ArgumentNullException.ThrowIfNull(properties); ArgumentNullException.ThrowIfNull(normalization);
        ArgumentNullException.ThrowIfNull(existingRecords);
        if(existingRecords.Count>65536)throw new ArgumentOutOfRangeException(nameof(existingRecords));
        Emission=actualEmission;strings=actualStringPool;this.properties=properties;this.normalization=normalization;
        AggregateFlags=aggregateFlags;
        foreach(var record in existingRecords)
        {
            ArgumentNullException.ThrowIfNull(record);
            if(record.Text is not null)actualStringPool.Validate(record.Text);
            rows.Add(new(record.CopyStorage40(),record.Text));
        }
    }

    public IReadOnlyList<LegacyPreparedTextGlyphReuseRecord> CopyRecords()
    {
        lock(gate)return Array.AsReadOnly(rows.Select(r=>new LegacyPreparedTextGlyphReuseRecord(r.Storage,r.Text)).ToArray());
    }

    public LegacyPreparedTextGlyphReuseResult Apply(LegacyNativeGlyphRecordOwner actualSnapshot,
        ReadOnlySpan<byte> selectionRows4, ulong referenceBase, int selectedInputIndex,
        CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(actualSnapshot);token.ThrowIfCancellationRequested();
        if(selectionRows4.Length%4!=0||selectionRows4.Length>65536*4||
            referenceBase!=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(referenceBase))
            throw new ArgumentException("Bounded complete selection rows and canonical source key required.");
        byte[] selections=selectionRows4.ToArray();
        return Process(selections.Length/4,selectedInputIndex,i=>
        {
            ushort ordinal=BinaryPrimitives.ReadUInt16LittleEndian(selections.AsSpan(i*4));
            var glyph=actualSnapshot.Resolve(referenceBase|((ulong)ordinal<<40));
            return new(glyph.PublicationFlags,Emission.Emit(glyph,token));
        },token);
    }

    //Prepared numeric controls exercise native prefix failures independently
    //of actual source ownership. The public entry point has no such callback.
    internal LegacyPreparedTextGlyphReuseResult ApplyPrepared(
        IReadOnlyList<LegacyPreparedGlyphReuseInput> inputs,int selectedInputIndex,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        return Process(inputs.Count,selectedInputIndex,i=>
        {
            var input=inputs[i];
            if(input.FailureStage==1)throw new NativeEmissionFailure();
            return input;
        },token);
    }

    private LegacyPreparedTextGlyphReuseResult Process(int count,int selectedInputIndex,
        Func<int,LegacyPreparedGlyphReuseInput> read,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();if(count is <0 or >65536)throw new ArgumentOutOfRangeException(nameof(count));
        lock(gate)
        {
            if(failed)throw new InvalidOperationException("Failed021760 prefix is retained; portable retry is unsupported.");
            int original=rows.Count,marker=-1,visited=0,appended=0;
            try
            {
                for(int inputIndex=0;inputIndex<count;inputIndex++)
                {
                    token.ThrowIfCancellationRequested();var input=read(inputIndex);
                    if(input.Utf8.Length>1_048_576)throw new NotSupportedException("Bounded emitted-byte extent exceeded.");
                    //Native21760 interns first, even for an empty interval.
                    int beforeIntern=strings.Count;
                    var identity=strings.Intern(input.Utf8.Span,token);
                    //The apparent fifth21760 parameter is reused as the OUT
                    //new-entry byte from builtin3E.virtual10, NOT caller policy.
                    bool newlyInterned=strings.Count!=beforeIntern;
                    if(input.FailureStage==2)throw new NativeEmissionFailure();
                    int match=-1;
                    if(!newlyInterned)
                        for(int old=0;old<original;old++)if(ReferenceEquals(rows[old].Text,identity)){match=old;break;}
                    if(match>=0)
                    {
                        if(inputIndex==selectedInputIndex)marker=match;
                    }
                    else
                    {
                        if(inputIndex==selectedInputIndex)marker=rows.Count;
                        if(input.FailureStage==3)throw new NativeEmissionFailure();
                        if(rows.Count==65536)throw new NotSupportedException("Bounded source-record capacity exceeded.");
                        //Native zeroes/appends the full row and increments its
                        //count BEFORE calling021000; do not roll this back.
                        var row=new Row(new byte[40],null);rows.Add(row);appended++;
                        BinaryPrimitives.WriteUInt32LittleEndian(row.Storage.AsSpan(32),(uint)input.Utf8.Length);
                        BinaryPrimitives.WriteUInt32LittleEndian(row.Storage.AsSpan(8),(uint)(input.GlyphFlags&0x3ff));
                        if(input.FailureStage==4)throw new NativeEmissionFailure();
                        row.Text=strings.Intern(input.Utf8.Span,token);
                        if(input.FailureStage==5)throw new NativeEmissionFailure();
                        if(input.FailureStage==6)throw new OperationCanceledException(token);
                        var published=LegacyNativeTextGlyphPublication.Publish(strings,input.GlyphFlags,input.Utf8.Span,
                            properties,normalization,cancellationToken:token);
                        uint flags=published.Flags;
                        var bytes=input.Utf8.Span;
                        if(bytes.Length==4&&bytes[0]==0xf3&&bytes[1]==0xb0&&bytes[2]==0x80&&bytes[3]<0x82)flags|=0x100000;
                        BinaryPrimitives.WriteUInt32LittleEndian(row.Storage.AsSpan(8),flags);
                        AggregateFlags|=flags;
                    }
                    visited++;
                }
                return new(true,marker,original,visited,appended);
            }
            catch(OperationCanceledException){failed=true;throw;}
            catch(Exception error)when(error is NativeEmissionFailure or InvalidDataException or NotSupportedException)
            {failed=true;return new(false,marker,original,visited,appended);}
            catch{failed=true;throw;}
        }
    }
    private sealed class NativeEmissionFailure:Exception;
}

internal readonly record struct LegacyPreparedGlyphReuseInput(ushort GlyphFlags,ReadOnlyMemory<byte> Utf8,int FailureStage=0);
