using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>One owned fresh main247 invocation with exactly one physical stroke.
/// The single-stroke15F050 reorder is identity;1629C0 creates no guide without
/// page lines or an auxiliary rectangle. First249's selected child is fitted
/// separately, then its actual275 glyph references produce720 rows for24B.
/// This does not restore multi-stroke grouping, main decoding or24D/29B postpasses.</summary>
public sealed class LegacyFreshSingleStrokeMainLine
{
    private readonly byte[] initialDescriptor;
    public LegacyMainFirstResultLineController Controller {get;}
    public LegacyFreshFirstResultOwnershipFactory FirstFactory {get;}
    public ReadOnlyMemory<byte> CopyInitialDescriptor()=>initialDescriptor.ToArray();
    private LegacyFreshSingleStrokeMainLine(LegacyMainFirstResultLineController controller,
        LegacyFreshFirstResultOwnershipFactory first,byte[] descriptor)
    {Controller=controller;FirstFactory=first;initialDescriptor=descriptor;}

    /// <summary>Fresh direct167210 input, no events, prior source, page lines or
    /// auxiliary rectangles. Resolution is the actual164520 float (zero is the
    /// fresh constructor value), never an assumed desktop DPI. More than one
    /// physical stroke requires the unrecovered15F050 group/reorder producer.</summary>
    public static LegacyFreshSingleStrokeMainLine Prepare(ReadOnlySpan<byte> resource,int sourceOffset,
        InkStroke rawPhysicalStroke,float actualPhysicalResolution,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(rawPhysicalStroke);cancellationToken.ThrowIfCancellationRequested();
        byte[] snapshot=resource.ToArray();
        var controller=LegacyMainFirstResultLineController.Prepare(snapshot,sourceOffset,[rawPhysicalStroke],
            actualPhysicalResolution,cancellationToken);
        //The child receives the retained parent buffer, not the user's dropped
        //duplicate points.157C60 submission membership is the exact singleton0.
        var first=LegacyFreshFirstResultOwnershipFactory.Prepare(snapshot,controller.FirstSourceOffset,
            controller.Source.Strokes,cancellationToken);
        var descriptor=new byte[88];
        BinaryPrimitives.WriteUInt16LittleEndian(descriptor,0x10);
        WI(descriptor,4,int.MinValue);
        foreach(int at in new[]{8,12,16,20,24,32})WI(descriptor,at,-1);
        var point=controller.Source.Strokes[0].Points[0];
        BinaryPrimitives.WriteSingleLittleEndian(descriptor.AsSpan(44),point.X);
        BinaryPrimitives.WriteSingleLittleEndian(descriptor.AsSpan(48),point.Y);
        descriptor[72]=1;
        descriptor.AsSpan(76,8).Fill(255);
        //24/28 physical endpoints and34/38 source720 interval are initially0.
        //No active source130 means4C/50 endpoint tags remainFFFF; no aliasing
        //between this descriptor ordinal and an arbitrary input stroke index.
        cancellationToken.ThrowIfCancellationRequested();return new(controller,first,descriptor);
    }

    /// <summary>Actual selected rich275 result, owned by this invocation. No text,
    /// glyph labels, boxes, category masks, guide or descriptor interval is supplied
    /// by the caller. Loaded249's serialized exponent comes from the category owner.
    /// The caller still must obtain a genuine selected first result; this API is
    /// not a completed winner/recognition endpoint.</summary>
    public LegacyFreshSingleStrokeMainLineResult ApplyFirstResult(LegacyFirstMaterializedPlainWord actualWord,
        LegacyOrdinaryGraphSourceRanges actualSourceRanges,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(actualWord);ArgumentNullException.ThrowIfNull(actualSourceRanges);
        cancellationToken.ThrowIfCancellationRequested();
        if(!ReferenceEquals(actualWord.Result.Factory,FirstFactory))
            throw new ArgumentException("Selected first word must belong to this main-to-child submission.");
        var rows=LegacyFirstResultSourceMaterializer.Materialize(actualWord,actualSourceRanges,
            Controller.FirstCategoryOwner,[0],Controller.Source.Strokes,cancellationToken);
        var descriptor=initialDescriptor.ToArray();WI(descriptor,52,0);WI(descriptor,56,rows.Length);
        var owners=rows.Select(row=>new LegacyMainOwnedFirstResultLineRow(Controller,row)).ToArray();
        //Native170400's temporary guide has unspecified inactive stack bytes.
        //Portable storage zeroes those ignored bytes; it does not claim their
        //vendor bit-pattern. Defined geometry is supplied exclusively by24B.
        var output=Controller.ProcessDescriptor(descriptor,owners,[],new byte[40],cancellationToken);
        return new(output,rows);
    }
    private static void WI(Span<byte>b,int at,int value)=>BinaryPrimitives.WriteInt32LittleEndian(b[at..],value);
}

/// <summary>Main guide after24B and before24D shear/29B gap/265 resampling.
/// A nonempty first result assigns line0, so next24A16E850 skips geometric
/// inference for this descriptor. An empty result still requires24A inference.</summary>
public sealed class LegacyFreshSingleStrokeMainLineResult
{
    private readonly LegacyFirstResultSourceRecord[] rows;
    public LegacyMainFirstResultLineOutput Output {get;}
    /// <summary>Owned720 records immediately before24B. Its transformed point
    /// buffers and remeasured records are Output.Deskew, when fitting succeeds.</summary>
    public IReadOnlyList<LegacyFirstResultSourceRecord> SourceRows {get;}
    public bool Next24ARequiresLineInference=>BinaryPrimitives.ReadInt32LittleEndian(Output.CopyDescriptor().Span[16..])<0;
    internal LegacyFreshSingleStrokeMainLineResult(LegacyMainFirstResultLineOutput output,LegacyFirstResultSourceRecord[] rows)
    {Output=output;this.rows=rows;SourceRows=Array.AsReadOnly(this.rows);}
}
