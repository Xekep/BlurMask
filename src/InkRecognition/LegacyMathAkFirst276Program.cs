[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("LegacyMathAkFirst276Program.Probe")]
namespace InkRecognition;

public sealed partial class LegacyMathAkFeatureProfile
{
    //8B per-profile reference + private owner50fields +109program +1104calib.
    //No weights, prepared212 slot, cache, geometry, source or request authority.
    private readonly First276Program first276;
    internal const int First276AdditionalPayloadBytes=1271;
    internal const int First276AdditionalCopyEnvelope=1365;
    internal const int First276AdditionalSourceUnits=8*109+16*276+512;
    internal First276ProgramDescription First276ProgramInfo=>first276.Description;
    internal byte ReadFirst276ProgramByte(int index)=>first276.ReadProgramByte(index);
    internal short ReadFirst276Minimum(int index)=>first276.ReadMinimum(index);
    internal short ReadFirst276Maximum(int index)=>first276.ReadMaximum(index);

    //26 logical scalar bytes. This is raw wire metadata, not initialized
    //matcher fields10C/D, FIRST V18 state, an evaluator or native cache entry.
    internal readonly record struct First276ProgramDescription(byte SlotOrdinal,byte WhichModel,
        byte CoordinatePrefix,byte BoundaryPair,byte Mode,ushort CalibrationWidth,ushort ModelInputWidth,
        int ModelStart,int ModelEnd,int Parent13A5Start,int Parent13A5End,byte Source58);

    private sealed partial class First276Program
    {
        private readonly byte[] program;
        private readonly short[] minimum,maximum;
        internal readonly First276ProgramDescription Description;
        private First276Program(byte[] program,short[] minimum,short[] maximum,First276ProgramDescription description)
        {this.program=program;this.minimum=minimum;this.maximum=maximum;Description=description;}
        //Only Profile.PrepareCore calls this, AFTER its full private SHA gate
        //and BEFORE publication/ZeroMemory. No external factory/array ingress.
        internal static First276Program ReadAuthenticated(byte[] capture,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if(capture.Length!=LegacyMathMixed107RawResource.ResourceBytes||
                U32(capture,7465)!=0x0101fe90||U32(capture,7469)!=170290||
                U32(capture,7473)!=0x13a5||U16(capture,7479)!=0x104||
                capture[7590]!=1||U16(capture,7591)!=276||capture[8697]!=0||capture[8698]!=1||
                capture[8699]!=2||capture[8700]!=0||capture[8701]!=1||capture[177650]!=0||
                U16(capture,8702)!=0xfea0||U16(capture,8704)!=0x107||
                8702L+8+U32(capture,8706)!=177650||U16(capture,8711)!=0x3f||
                U32(capture,8713)!=276)
                throw new InvalidDataException("Actual FIRST276 program/slot2 parent differs.");
            //Fixed same-source scalar and array bounds precede ALL allocation.
            var p=new byte[109];var lo=new short[276];var hi=new short[276];
            for(int i=0;i<109;i++)
            {if((i&31)==0)token.ThrowIfCancellationRequested();p[i]=capture[7481+i];}
            for(int i=0;i<276;i++)
            {
                if((i&31)==0)token.ThrowIfCancellationRequested();
                lo[i]=unchecked((short)U16(capture,7593+2*i));
                hi[i]=unchecked((short)U16(capture,8145+2*i));
            }
            //All OTHER slot encodings are absent in this exact pinned owner.
            //No slot array/hidden runtime cache is synthesized from them.
            var d=new First276ProgramDescription(2,1,capture[8697],capture[8698],capture[8699],
                U16(capture,7591),checked((ushort)U32(capture,8713)),8702,177650,7465,177763,capture[177695]);
            token.ThrowIfCancellationRequested();return new(p,lo,hi,d);
        }
        private static ushort U16(byte[] b,int p)=>(ushort)(b[p]|b[p+1]<<8);
        private static uint U32(byte[] b,int p)=>(uint)(b[p]|b[p+1]<<8|b[p+2]<<16|b[p+3]<<24);
        internal byte ReadProgramByte(int index)
        {if((uint)index>=109)throw new ArgumentOutOfRangeException(nameof(index));return program[index];}
        internal short ReadMinimum(int index)
        {if((uint)index>=276)throw new ArgumentOutOfRangeException(nameof(index));return minimum[index];}
        internal short ReadMaximum(int index)
        {if((uint)index>=276)throw new ArgumentOutOfRangeException(nameof(index));return maximum[index];}
    }
}
