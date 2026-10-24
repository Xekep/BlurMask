using System.Buffers.Binary;
namespace InkRecognition;

//48 logical scalar bytes once per authenticated profile, not per subset.
//This numeric value is not source, sampler, graph or request authority.
internal readonly record struct LegacyMathAkSource13A2Policy(float LooseSlope,float StrongSlope,
    float MinimumArc,float LongArc,bool KeepOneAnchor,bool HorizontalPass,bool ReversePass,
    float GapRadius,float LoopDrop,float ReversalSpacing,float PruneRadius,float HighAnchorMargin,
    float LoopSpacing,byte MinimumMarkerGate,float WidthScale)
{
    internal static LegacyMathAkSource13A2Policy ReadAuthenticated(ReadOnlySpan<byte> snapshot)
    {
        //Caller is the existing profile loader AFTER full private-snapshot SHA.
        //Exact67B actual Math-local297/v108, including all three absent branches.
        ReadOnlySpan<byte> expected=[0x90,0xfe,0x01,0x01,0x3b,0x00,0x00,0x00,0x97,0x02,0x00,0x00,0x00,0x00,0x08,0x01,0x00,0x00,0x00,0x3f,0x33,0x33,0xb3,0x3e,0xcd,0xcc,0x4c,0x3e,0x66,0x66,0xe6,0x3f,0x01,0x01,0x01,0x33,0x33,0xb3,0x3e,0xcd,0xcc,0x4c,0x3e,0x00,0x00,0x80,0x3e,0x00,0x00,0x00,0x3f,0x00,0x00,0x00,0x3f,0xcd,0xcc,0xcc,0x3d,0x06,0x00,0x00,0x80,0x40,0x00,0x00,0x00];
        if(snapshot.Length!=LegacyMathMixed107RawResource.ResourceBytes||
            !snapshot.Slice(7365,67).SequenceEqual(expected))
            throw new InvalidDataException("Actual Math-local297 sampler policy differs.");
        return new(F(snapshot,7381),F(snapshot,7385),F(snapshot,7389),F(snapshot,7393),
            snapshot[7397]!=0,snapshot[7398]!=0,snapshot[7399]!=0,F(snapshot,7400),F(snapshot,7404),
            F(snapshot,7408),F(snapshot,7412),F(snapshot,7416),F(snapshot,7420),snapshot[7424],F(snapshot,7425));
    }
    private static float F(ReadOnlySpan<byte> b,int at)=>BinaryPrimitives.ReadSingleLittleEndian(b.Slice(at,4));
}
