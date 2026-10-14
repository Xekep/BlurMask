using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>One owned fresh selected-source invocation and its actual result274/279 factory policies.
/// Resource B9 supplies word geometry policy; driver220 supplies capture. Neither is an OCR/sample default.</summary>
public sealed class LegacyFreshFirstResultOwnershipFactory
{
    public LegacyFirstAcousticModel AcousticModel {get;}
    public LegacyFreshOrdinaryPacketFrame Frame {get;}
    public LegacyFreshDirectFirstInvocationPolicy InvocationPolicy {get;}
    public string SourceFingerprint=>AcousticModel.SourceFingerprint;
    public bool CaptureGlyphGeometry=>false;
    public bool FinalizeWordGeometry {get;}
    public bool SortWordsByGeometry=>FinalizeWordGeometry;
    public bool Configuration6C=>true;
    /// <summary>155DA0/155EF0 initialize the fresh source-row confidence
    /// exponent1C8 to binary32 one. This raw no-property/no-serialized-driver
    /// factory cannot receive158BC0's separate driver-payload override.</summary>
    public float SourceScoreExponent=>1f;
    public int ResourceB9Offset {get;}
    /// <summary>Actual1EF070 F8 allocation/primary record capacity read from
    /// selected247.virtual70=1D62B0's serializedBA. This is independent of the
    ///21b candidate span cap and later A0/A2/A4 result-column counts.</summary>
    public ushort PrimaryBeamWidthF8 {get;}
    private LegacyFreshFirstResultOwnershipFactory(LegacyFirstAcousticModel model,
        LegacyFreshOrdinaryPacketFrame frame,LegacyFreshDirectFirstInvocationPolicy invocation,bool geometry,int b9Offset,ushort primaryWidth)
    {AcousticModel=model;Frame=frame;InvocationPolicy=invocation;FinalizeWordGeometry=geometry;ResourceB9Offset=b9Offset;PrimaryBeamWidthF8=primaryWidth;}

    /// <summary>Create the raw source, fitted first frame and first policy together. This no-property-event API
    /// never imports a reused driver's220/configuration9B or an external274 capture setting.</summary>
    public static LegacyFreshFirstResultOwnershipFactory Prepare(ReadOnlySpan<byte> actualResource,int sourceOffset,
        IReadOnlyList<InkStroke> rawPhysicalStrokes,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(rawPhysicalStrokes);cancellationToken.ThrowIfCancellationRequested();
        if(actualResource.Length>64*1024*1024)throw new ArgumentOutOfRangeException(nameof(actualResource));
        byte[] resource=actualResource.ToArray();
        var model=LegacyFirstAcousticModel.ReadSource(resource,sourceOffset,cancellationToken);
        var frame=LegacyFreshOrdinaryPacketFrame.PrepareFirstSource(resource,sourceOffset,rawPhysicalStrokes,cancellationToken);
        var invocation=LegacyFreshDirectFirstInvocationPolicy.Create(resource,sourceOffset,frame,cancellationToken);
        if(frame.Row.Profile!=model.RowProfile)throw new ArgumentException("Factory first model and actual raw frame differ.");
        //1D4950 forwards sequential B8/B9/BA/BC into247; ParametersOffset follows their1+1+2+2 bytes.
        int b9=model.RowProfile.Source.ParametersOffset-5;
        if(b9<sourceOffset || b9>=resource.Length || resource[b9]>1)throw new InvalidDataException("Actual serialized247 B9 required.");
        ushort primaryWidth=BinaryPrimitives.ReadUInt16LittleEndian(resource.AsSpan(b9+1,2));
        cancellationToken.ThrowIfCancellationRequested();return new(model,frame,invocation,resource[b9]!=0,b9,primaryWidth);
    }

    public void ValidateInvocation(LegacyFirstAcousticModel actualModel,LegacyFreshOrdinaryPacketFrame actualFrame,
        LegacyFreshDirectFirstInvocationPolicy actualPolicy,CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(!ReferenceEquals(actualModel,AcousticModel)||!ReferenceEquals(actualFrame,Frame)||!ReferenceEquals(actualPolicy,InvocationPolicy))
            throw new ArgumentException("The result factory belongs to one actual fresh first invocation, not an equal-looking foreign frame.");
    }
    public void ValidateScores(LegacyFirstAcousticScores actualScores,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(actualScores);cancellationToken.ThrowIfCancellationRequested();
        if(actualScores.SourceFingerprint!=SourceFingerprint||actualScores.TrajectoryProbability32768.Length!=AcousticModel.TrajectoryNetwork.OutputWidth||
            actualScores.RasterProbability32768.Length!=AcousticModel.RasterNetwork.OutputWidth)
            throw new ArgumentException("Confidence channels must retain this factory's exact source fingerprint.");
    }

    /// <summary>New TOP273 glyph-pool and279/275 publisher sharing one owner, with proven fresh capture and resource-derived flags.
    /// A source requesting geometric finalization is refused until that actual branch is implemented.</summary>
    public LegacyFreshFirstPlainResult CreatePlainResult(CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(FinalizeWordGeometry)throw new NotSupportedException("Actual selected-source B9 enables geometric word finalization/order; the plain branch cannot replace it.");
        var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(CaptureGlyphGeometry,cancellationToken);
        var publisher=new LegacyNativePlainWordPublisher(pool,InvocationPolicy.Binding.OwnerFlagC4,FinalizeWordGeometry,SortWordsByGeometry);
        cancellationToken.ThrowIfCancellationRequested();return new(this,pool,publisher);
    }
}

/// <summary>Fresh273 result ownership with factory-derived capture and279 publication policies.</summary>
public sealed class LegacyFreshFirstPlainResult
{
    public LegacyFreshFirstResultOwnershipFactory Factory {get;}
    public LegacyNativeGlyphRecordPool GlyphPool {get;}
    public LegacyNativePlainWordPublisher WordPublisher {get;}
    internal LegacyFreshFirstPlainResult(LegacyFreshFirstResultOwnershipFactory factory,
        LegacyNativeGlyphRecordPool pool,LegacyNativePlainWordPublisher publisher)
    {Factory=factory;GlyphPool=pool;WordPublisher=publisher;}
}
