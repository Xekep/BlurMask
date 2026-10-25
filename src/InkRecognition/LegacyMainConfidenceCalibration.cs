using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>The actual Russian main21D mode1 calibration table, including
/// original alphabet labels beyond the180 neural outputs. This is only the
/// EXP/13E8E0 scalar step, not candidate ranking or an owned group score cache.</summary>
public sealed class LegacyMainConfidenceCalibration
{
    private readonly float[] centers, slopes;
    public int SymbolCount { get; }
    public int ActiveFamilyCount { get; }

    private LegacyMainConfidenceCalibration(int symbols, int families, float[] centers, float[] slopes)
    { SymbolCount=symbols; ActiveFamilyCount=families; this.centers=centers; this.slopes=slopes; }

    public static LegacyMainConfidenceCalibration ReadRussianSource(ReadOnlySpan<byte> resource,
        CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(resource.Length!=572763 || !SHA256.HashData(resource).AsSpan().SequenceEqual(
            Convert.FromHexString("7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978")))
            throw new InvalidDataException("The checked Russian main resource is required.");
        if(BinaryPrimitives.ReadInt32LittleEndian(resource[566408..])!=1 ||
            BinaryPrimitives.ReadUInt16LittleEndian(resource[566414..])!=0x100)
            throw new InvalidDataException("Actual main mode1/float calibration required.");
        int symbols=BinaryPrimitives.ReadUInt16LittleEndian(resource[566412..]);
        var weights=LegacyDirectionalAcousticWeightConfiguration.LoadV110Prefix(resource[189734..],out _,cancellationToken);
        int families=0;
        for(int i=0;i<7;i++)
        {
            if(weights.Slots.Span[i].NetworkMask!=(i<4?2:0))
                throw new InvalidDataException("Actual four second-only main families required.");
            if(weights.Slots.Span[i].NetworkMask!=0)families++;
        }
        if(symbols!=192 || 566416+symbols*families*8!=572560)
            throw new InvalidDataException("Main calibration table extent differs.");
        var centers=new float[symbols*families];var slopes=new float[centers.Length];
        for(int i=0;i<centers.Length;i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            centers[i]=BinaryPrimitives.ReadSingleLittleEndian(resource[(566416+i*8)..]);
            slopes[i]=BinaryPrimitives.ReadSingleLittleEndian(resource[(566420+i*8)..]);
            if(!float.IsFinite(centers[i]) || !float.IsFinite(slopes[i]) || slopes[i]<=0)
                throw new InvalidDataException("Finite main calibration and positive slope required.");
        }
        return new(symbols,families,centers,slopes);
    }

    /// <summary>Calibration is indexed by the ORIGINAL candidate symbol,
    /// independently of the alias used to obtain its neural probability.
    /// Out-of-table label/family uses native DOUBLE0.575, not0.575f.</summary>
    public float Calibrate(ushort originalSymbol, uint activeFamilyOrdinal, int probability32768,
        CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(probability32768 is <0 or >32768)throw new ArgumentOutOfRangeException(nameof(probability32768));
        float probability=probability32768*(1f/32768);
        double center=0, slope=.575;
        if(originalSymbol<SymbolCount && activeFamilyOrdinal<(uint)ActiveFamilyCount)
        {
            int index=(int)activeFamilyOrdinal*SymbolCount+originalSymbol;
            center=centers[index];slope=slopes[index];
        }
        double exponent=Math.Exp(((double)probability-center)*-slope);
        return exponent>=float.MaxValue?0f:(float)(1/(exponent+1));
    }
}
