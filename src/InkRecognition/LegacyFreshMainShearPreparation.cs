using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Fresh direct main0 ownership through24B -> actual24D. Its physical
/// records originate at167210 with association−1, survive16AA00's whole-record
/// permutations, and are not reassigned by the scoped249/157C60/24B720 path.
/// This factory accepts no prior/reused-source object or metadata overrides.</summary>
public sealed class LegacyFreshMainShearPreparation
{
    public LegacyFreshMainFirstResultLineSet Input {get;}
    public LegacyOrdinaryShearProfile Profile {get;}
    public LegacyNativeCaseAlphabetRanges CasedRanges {get;}
    public LegacyOrdinaryShearResult Output {get;}
    public int Processor24DOffset {get;}
    private LegacyFreshMainShearPreparation(LegacyFreshMainFirstResultLineSet input,LegacyOrdinaryShearProfile profile,
        LegacyNativeCaseAlphabetRanges ranges,LegacyOrdinaryShearResult output,int offset)
    {Input=input;Profile=profile;CasedRanges=ranges;Output=output;Processor24DOffset=offset;}

    public static LegacyFreshMainShearPreparation Prepare(ReadOnlySpan<byte> resource,LegacyFreshMainFirstResultLineSet actualInput,
        LegacyNativeCaseData caseData,LegacyNativeTextPublicationData publication,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(actualInput);ArgumentNullException.ThrowIfNull(caseData);ArgumentNullException.ThrowIfNull(publication);
        cancellationToken.ThrowIfCancellationRequested();
        var controller=actualInput.Owner.Controller;
        if(!Convert.ToHexString(SHA256.HashData(resource)).Equals(actualInput.Owner.Input.Input.Input.SourceFingerprint,StringComparison.OrdinalIgnoreCase)||
            controller.SourceOffset!=0)throw new ArgumentException("Exact fresh direct main0 source owner required.");
        var config=LegacyOrdinarySourceInk.ReadV10dResourceProfile(resource,0);int at=config.EnclosureFlagOffset+12;
        if(I(resource,at)!=6)throw new InvalidDataException("Actual six-processor main source required.");at+=4;
        int d=-1;int[] kinds=[0x249,0x24b,0x24a,0x24d,0x29b,0x265];
        foreach(int kind in kinds)
        {
            if(at<0||at>resource.Length-16||I(resource,at)!=0x101fe90||I(resource,at+8)!=kind)
                throw new InvalidDataException("Ordered owned main processor sequence required.");
            int length=I(resource,at+4);if(length<8||length>resource.Length-at-8)throw new InvalidDataException("Truncated main processor.");
            if(kind==0x24d)d=at;at=checked(at+length+8);
        }
        var profile=LegacyOrdinaryShearProfile.ReadV100Node(resource,d);
        var lookup=controller.Alphabet;var alphabet=LegacyInkAlphabet.Load(resource[lookup.AlphabetPayloadOffset..],
            BinaryPrimitives.ReadUInt16LittleEndian(resource[(lookup.AlphabetPayloadOffset-2)..]));
        var ranges=LegacyNativeCaseAlphabetRanges.Prepare(alphabet,caseData,publication,cancellationToken);
        var points=actualInput.CopyPhysicalPoints();
        //The typed lineage excludes the9C direct-source-off branch, prior
        //association rows, recognized-input kind6 and external callbacks.
        //The producer proof is recorded by audit_fresh_main_shear.py.
        var associations=Enumerable.Repeat(-1,points.Length).ToArray();
        var output=LegacyNativeOrdinaryShearPreparation.Apply(profile,
            actualInput.CopyDescriptors().Select(b=>new ReadOnlyMemory<byte>(b)).ToArray(),
            actualInput.CopyGuides().Select(b=>new ReadOnlyMemory<byte>(b)).ToArray(),
            points.Select(s=>new InkStroke(s)).ToArray(),associations,actualInput.CurrentRows,ranges,cancellationToken);
        return new(actualInput,profile,ranges,output,d);
    }
    private static int I(ReadOnlySpan<byte>b,int at)=>BinaryPrimitives.ReadInt32LittleEndian(b[at..]);
}
