using System.Buffers.Binary;
using System.Numerics;

namespace InkRecognition;

/// <summary>Private fresh append-only selected1388 V18/V68 projection and13B6
/// operation context. Not the complete native interface or a processed graph,
/// matcher request, existing-document selection or formula candidate.</summary>
internal sealed partial class LegacyMathAkOriginalInputContext
{
    private readonly LegacyMathAkOriginalInputFrame input;
    private readonly object documentIdentity;
    private LegacyMathAkProcessedStrokePrefix? prefix;
    private Vector4[] bounds = [];
    private uint[] processed = [], descriptorMasks = [];
    private int[] descriptorOrdinals = [];
    private RangeOwner?[] results = [];
    private LegacyMathAkProcessedStrokePrefix.OperatorSourceRow?[] sourceRows = [];
    private int capturedPoints;
    private int wordCount, descriptorCount, resultCount, claim, phase, owningThread;
    private const float FreshGuide = 0.5f;
    private LegacyMathAkOriginalInputContext(LegacyMathAkOriginalInputFrame input)
    { this.input = input; documentIdentity = input.DocumentIdentity; }

    internal static LegacyMathAkOriginalInputContext PrepareFresh(
        LegacyMathAkSourceGeometry original, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(original); token.ThrowIfCancellationRequested();
        //Same original input exists before006A40/139D/139F. No new IDs or
        //document are reconstructed from normalized or already processed XY.
        var frame = LegacyMathAkOriginalInputFrame.CreateFresh(original, token);
        var context = new LegacyMathAkOriginalInputContext(frame);
        try
        {
            context.prefix = LegacyMathAkProcessedStrokePrefix.PrepareOriginalInput(frame, token);
            context.Preflight(token);
            context.RunFresh13B6(token);
            token.ThrowIfCancellationRequested(); context.phase = 3;
            return context;
        }
        catch { context.phase = -1; throw; } //Never restore a consumed claim/B8 prefix.
    }

    private void Preflight(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if (prefix is null || !ReferenceEquals(prefix.OriginalInput, input) ||
            !ReferenceEquals(prefix.OriginalOwner, input.OriginalOwner) ||
            !ReferenceEquals(documentIdentity, input.DocumentIdentity) ||
            input.StrokeCount is < 1 or > 64 || input.ItemCount != input.StrokeCount ||
            prefix.PointCount is < 1 or > LegacyMathAkProcessedStrokePrefix.MaximumExpandedPoints)
            throw new InvalidOperationException("Fresh original-input lineage mismatch.");
        int count = input.StrokeCount;
        wordCount = input.ProcessedWordCount;
        if (wordCount != (input.ItemCount >> 5) + 1 || wordCount is < 1 or > 3)
            throw new InvalidOperationException("Fresh processed-word invariant failed.");
        for (int s = 0; s < count; s++)
        {
            token.ThrowIfCancellationRequested();
            if (input.ReadStrokeId(s) != s || input.ReadStrokeOrdinal(s) != s ||
                input.ReadItemKind(s) != -1 || !input.IsActive(s))
                throw new InvalidOperationException("Fresh append transition mismatch.");
        }
        //Native022F30 builds bboxes before eligibility. Fixed-capacity managed
        //storage is a portable allocation adaptation, not native heap layout.
        bounds = prefix.CopyOperatorBounds(token);
        if (bounds.Length != count) throw new InvalidOperationException("Owned bounds count mismatch.");
        foreach (var box in bounds)
            if (!float.IsFinite(box.X) || !float.IsFinite(box.Y) ||
                !float.IsFinite(box.Z) || !float.IsFinite(box.W) ||
                !float.IsFinite(box.Z - box.X) || !float.IsFinite(box.W - box.Y))
                throw new NotSupportedException("Operator13B6 requires finite bbox arithmetic.");
        processed = new uint[wordCount]; descriptorMasks = new uint[checked(count * wordCount)];
        descriptorOrdinals = new int[count]; results = new RangeOwner?[count];
        sourceRows = new LegacyMathAkProcessedStrokePrefix.OperatorSourceRow?[count];
        token.ThrowIfCancellationRequested(); phase = 1;
    }

    private void Claim()
    {
        if (phase != 1 || prefix is null || !ReferenceEquals(prefix.OriginalInput, input) ||
            !ReferenceEquals(input.DocumentIdentity, documentIdentity))
            throw new InvalidOperationException("Fresh operator context is not claimable.");
        if (Interlocked.CompareExchange(ref claim, 1, 0) != 0)
            throw new InvalidOperationException("Fresh operator context already consumed.");
        owningThread = Environment.CurrentManagedThreadId; phase = 2;
    }
    private void Live()
    {
        if (phase != 2 || claim != 1 || owningThread != Environment.CurrentManagedThreadId ||
            prefix is null || !ReferenceEquals(prefix.OriginalInput, input) ||
            !ReferenceEquals(input.DocumentIdentity, documentIdentity))
            throw new InvalidOperationException("Operator13B6 live owner mismatch.");
    }
    private LegacyMathAkProcessedStrokePrefix.OperatorSourceRow Row(int ordinal,CancellationToken token)
    {
        Live();token.ThrowIfCancellationRequested();
        if((uint)ordinal>=(uint)sourceRows.Length)throw new InvalidOperationException("Owned row ordinal out of bounds.");
        if(sourceRows[ordinal] is { } retained){retained.Validate(prefix!,ordinal);return retained;}
        int count=prefix!.ReadOperatorStrokeCount(ordinal);
        if(count<1||count>prefix.PointCount-capturedPoints)throw new InvalidOperationException("Row capture point budget exceeded.");
        var row=prefix.CaptureOperatorRow(ordinal,token);row.Validate(prefix,ordinal);
        if(row.Length!=count)throw new InvalidOperationException("Row capture changed.");
        capturedPoints+=count;sourceRows[ordinal]=row;return row;
    }
    private bool Marked(int id) => (processed[id >> 5] & (1u << (id & 31))) != 0;
    private static bool Bit(ReadOnlySpan<uint> words, int id) => (words[id >> 5] & (1u << (id & 31))) != 0;
    private void Finished()
    { if (phase != 3) throw new InvalidOperationException("Only completed numeric state can be inspected."); }
    internal LegacyMathAkOriginalInputFrame OriginalInput => input;
    internal LegacyMathAkProcessedStrokePrefix Prefix => prefix ?? throw new InvalidOperationException();
    internal int RangeResultCount { get { Finished(); return resultCount; } }
    internal int DescriptorCount { get { Finished(); return descriptorCount; } }
    internal bool HasProcessedGraphAuthority => false;
    internal bool HasMatcherRequestAuthority => false;
    internal uint[] CopyProcessedWords(CancellationToken token)
    { Finished(); token.ThrowIfCancellationRequested(); var copy=(uint[])processed.Clone(); token.ThrowIfCancellationRequested(); return copy; }
    internal byte[][] CopyRangeRecords(int resultIndex, CancellationToken token)
    {
        Finished(); token.ThrowIfCancellationRequested();
        if ((uint)resultIndex >= (uint)resultCount) throw new ArgumentOutOfRangeException(nameof(resultIndex));
        return results[resultIndex]!.Copy(this, token);
    }

    private readonly record struct RangeValue(uint OriginalId, uint StartBits, uint EndBits)
    {
        internal void Write(Span<byte> output)
        { BinaryPrimitives.WriteUInt32LittleEndian(output,OriginalId); BinaryPrimitives.WriteUInt32LittleEndian(output[4..],StartBits); BinaryPrimitives.WriteUInt32LittleEndian(output[8..],EndBits); }
    }
    private sealed class RangeOwner
    {
        private readonly LegacyMathAkOriginalInputContext context;
        private readonly object document;
        private readonly RangeValue[] current, prior;
        private int state;
        //Arrays originate only in the private actual writer in this same
        //partial class. Their ownership moves; no clone/caller ingress.
        internal RangeOwner(LegacyMathAkOriginalInputContext context, RangeValue[] current, RangeValue[] prior)
        { this.context=context;document=context.documentIdentity;this.current=current;this.prior=prior; }
        internal void Retain(LegacyMathAkOriginalInputContext expected)
        {
            if (!ReferenceEquals(context,expected) || !ReferenceEquals(document,expected.input.DocumentIdentity) ||
                Interlocked.CompareExchange(ref state,1,0)!=0)
                throw new InvalidOperationException("Range ownership cannot be transferred.");
        }
        internal void ReleaseUnretained() => Interlocked.Exchange(ref state,2);
        internal byte[][] Copy(LegacyMathAkOriginalInputContext expected,CancellationToken token)
        {
            if(state!=1||!ReferenceEquals(context,expected)||!ReferenceEquals(document,expected.documentIdentity))
                throw new InvalidOperationException("Range owner is foreign or released.");
            var output=new byte[2][];
            RangeValue[][] values=[current,prior];
            for(int r=0;r<2;r++)
            {
                token.ThrowIfCancellationRequested();output[r]=new byte[checked(values[r].Length*12)];
                for(int j=0;j<values[r].Length;j++){token.ThrowIfCancellationRequested();values[r][j].Write(output[r].AsSpan(j*12,12));}
            }
            token.ThrowIfCancellationRequested();return output;
        }
    }
}
