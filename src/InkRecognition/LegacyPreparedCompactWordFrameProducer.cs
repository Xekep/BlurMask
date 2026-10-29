using System.Buffers.Binary;

namespace InkRecognition;

public readonly record struct LegacyPreparedCompactWordContextPolicy(
    bool AssociatedSourcePresent2300, bool AssociatedQuotes2308, bool ContextSentinels24b8,
    ushort Apostrophe24ba, ushort Quote24bc, ushort Special251c, ushort Special251e,
    ushort KnowledgeSymbol0e, ushort Special24de)
{
    /// <summary>20C1C0's exact alphabet-derived symbols. Associated-source and
    /// sentinel flags remain explicit prepared policy; this does not select a
    /// driver or infer its mutable properties.</summary>
    public static LegacyPreparedCompactWordContextPolicy FromAlphabet(LegacyInkAlphabet alphabet,
        bool associatedSourcePresent2300, bool associatedQuotes2308, bool contextSentinels24b8)
    {
        ArgumentNullException.ThrowIfNull(alphabet);
        byte[] header = alphabet.CopySerializedHeader();
        //053F20 writes serialized header0..6 to runtime8..14; count7 goes1A.
        ushort knowledge = BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(6));
        return new(associatedSourcePresent2300, associatedQuotes2308, contextSentinels24b8,
            Resolve("'"), Resolve("\""), unchecked((ushort)(alphabet.LabelCount + 1)),
            unchecked((ushort)(alphabet.LabelCount + 2)), knowledge, Resolve("\u200c"));

        ushort Resolve(string text) => alphabet.TryGetLabel(text, out ushort label) ? label : ushort.MaxValue;
    }
}

/// <summary>Prepared ordinary2075E0 frame and its rewritten current context.
/// Context identities belong to the exact shared factory; frame getters copy.</summary>
public sealed class LegacyPreparedCompactWordFrame
{
    private readonly byte[] frame;
    public LegacySymbolContextFactory ContextOwner { get; }
    public ulong RewrittenCurrentContext { get; }
    internal LegacyPreparedCompactWordFrame(LegacySymbolContextFactory contexts, ulong current, byte[] frame)
    { ContextOwner = contexts; RewrittenCurrentContext = current; this.frame = frame; }
    public byte[] CopyFrame() => (byte[])frame.Clone();
}

/// <summary>207420 context surgery followed by ordinary21B's complete2075E0
/// prepared-frame route. This does not schedule207CB0 compact predecessors.</summary>
public static class LegacyPreparedCompactWordFrameProducer
{
    public static ulong RewriteCurrentContext(LegacySymbolContextFactory contexts, ulong currentContext,
        ReadOnlySpan<byte> sourceRow88, uint sourceNode, LegacyPreparedCompactWordContextPolicy policy,
        Func<ushort, byte> sourceSymbolCategory, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(contexts);
        ArgumentNullException.ThrowIfNull(sourceSymbolCategory);
        cancellationToken.ThrowIfCancellationRequested();
        if (sourceRow88.Length != 88) throw new ArgumentException("Complete explicit88-byte source row required.");
        byte[] row = sourceRow88.ToArray();
        //207420 eagerly reads the current parent's link before metadata guards.
        ulong parent = currentContext == 0 ? 0 : contexts.ReadNode(currentContext).ParentLink;
        ushort replacement;
        if (BinaryPrimitives.ReadUInt16LittleEndian(row.AsSpan(0x18)) == sourceNode)
        {
            if (!policy.AssociatedSourcePresent2300 || !policy.AssociatedQuotes2308) return currentContext;
            ushort symbol = BinaryPrimitives.ReadUInt16LittleEndian(row.AsSpan(0x24));
            bool rewrite = false;
            replacement = 0;
            if (symbol == policy.Apostrophe24ba)
            {
                byte category = sourceSymbolCategory(policy.Special251c);
                cancellationToken.ThrowIfCancellationRequested();
                if ((row[0x1a] & 0x3f) == category)
                { replacement = policy.Special251c; rewrite = true; }
            }
            if (!rewrite && symbol == policy.Quote24bc)
            {
                byte category = sourceSymbolCategory(policy.Special251e);
                cancellationToken.ThrowIfCancellationRequested();
                if ((row[0x1a] & 0x3f) == category)
                { replacement = policy.Special251e; rewrite = true; }
            }
            if (!rewrite) return currentContext;
        }
        else
        {
            if ((sbyte)row[5] >= 0) return currentContext;
            parent = 0;
            replacement = policy.KnowledgeSymbol0e;
        }
        var context = parent == 0 ? contexts.ResolveInitial(replacement, cancellationToken) :
            contexts.ResolveChild(parent, replacement, cancellationToken);
        //A committed insertion must retain its parent even if cancellation
        //arrives immediately afterward. Null allocation propagates as0.
        if (context.Identity != 0 && parent != 0 && context.Created)
            contexts.LinkCreatedContext(context.Identity, parent);
        cancellationToken.ThrowIfCancellationRequested();
        return context.Identity;
    }

    public static LegacyPreparedCompactWordFrame CreateOrdinary(LegacySymbolContextFactory contexts,
        ulong currentContext, ulong previousContext, ReadOnlySpan<byte> sourceRow88, uint sourceNode,
        byte glyphDifference, LegacyPreparedCompactWordContextPolicy policy,
        Func<ushort, byte> sourceSymbolCategory, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (sourceRow88.Length != 88) throw new ArgumentException("Complete explicit88-byte source row required.");
        byte[] row = sourceRow88.ToArray();
        //2075E0 calls207420, then zeroes the entire3B0 frame before2067C0.
        ulong rewritten = RewriteCurrentContext(contexts, currentContext, row, sourceNode,
            policy, sourceSymbolCategory, cancellationToken);
        byte[] frame = LegacyPreparedCompactWordSequence.AppendContextDifference(
            new byte[LegacyPreparedCompactWordSequence.FrameBytes], contexts, rewritten, previousContext,
            row, sourceNode, policy.KnowledgeSymbol0e, policy.ContextSentinels24b8,
            policy.Special24de, cancellationToken);
        ushort sourceArc = BinaryPrimitives.ReadUInt16LittleEndian(row.AsSpan(0x1c));
        int count = LegacyNativeMainSourceCoordinates.WriteSingle(sourceNode, sourceArc, frame.AsSpan(0x260, 20 * 16));
        BinaryPrimitives.WriteInt32LittleEndian(frame.AsSpan(0x25c), count);
        LegacyNativeMainSourceCoordinates.WriteCallbackCounts(glyphDifference, frame);
        frame = LegacyPreparedCompactWordSequence.Normalize(frame, policy.ContextSentinels24b8, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        return new(contexts, rewritten, frame);
    }

    //Only the authenticated periodic LAST store consumes this one fresh array.
    //Keep BOTH row clones and public context-rewrite/callback semantics.
    internal static byte[] CreateOrdinaryOwned(LegacySymbolContextFactory contexts,
        ulong currentContext,ulong previousContext,ReadOnlySpan<byte> sourceRow88,uint sourceNode,
        byte glyphDifference,LegacyPreparedCompactWordContextPolicy policy,
        Func<ushort,byte> sourceSymbolCategory,ILegacyOwnedCompactFrameBudget budget,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();ArgumentNullException.ThrowIfNull(budget);
        if(sourceRow88.Length!=88)throw new ArgumentException("Complete explicit88-byte source row required.");
        byte[] row=sourceRow88.ToArray();
        ulong rewritten=RewriteCurrentContext(contexts,currentContext,row,sourceNode,policy,sourceSymbolCategory,cancellationToken);
        byte[] frame=LegacyPreparedCompactWordSequence.AppendContextDifferenceOwned(
            new byte[LegacyPreparedCompactWordSequence.FrameBytes],contexts,rewritten,previousContext,
            row,sourceNode,policy.KnowledgeSymbol0e,policy.ContextSentinels24b8,policy.Special24de,cancellationToken);
        ushort sourceArc=BinaryPrimitives.ReadUInt16LittleEndian(row.AsSpan(0x1c));
        int count=LegacyNativeMainSourceCoordinates.WriteSingle(sourceNode,sourceArc,frame.AsSpan(0x260,20*16));
        BinaryPrimitives.WriteInt32LittleEndian(frame.AsSpan(0x25c),count);
        LegacyNativeMainSourceCoordinates.WriteCallbackCounts(glyphDifference,frame);
        budget.BeforeNormalize(frame,cancellationToken);
        frame=LegacyPreparedCompactWordSequence.NormalizeOwned(frame,policy.ContextSentinels24b8,cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();return frame;
    }
}

//Closed internal accounting continuation. The real LAST store authenticates
//its actual source/lease before any normalization metadata is read.
internal interface ILegacyOwnedCompactFrameBudget
{
    void BeforeNormalize(ReadOnlySpan<byte> frame,CancellationToken token);
}
