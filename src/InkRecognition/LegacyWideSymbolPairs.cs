using System.Buffers;
using System.Text;

namespace InkRecognition;

/// <summary>
/// The unrestricted Engine58da0 branch: single scalar, native decomposition
/// kind0x800 (Unicode wide), existing base symbol -> (base,wide) cache pair.
/// This does not fold case, fonts, compatibility in general or scripts.
/// </summary>
public static class LegacyWideSymbolPairs
{
    /// <summary>Construct the exact observed104 wide decompositions without storing an engine Unicode table.</summary>
    public static bool TryGetWideBase(int scalar,out int baseScalar)
    {
        if(scalar is >=0xff01 and <=0xff5e){baseScalar=scalar-0xfee0;return true;}
        baseScalar=scalar switch
        {
            0x3000=>0x20,0xff5f=>0x2985,0xff60=>0x2986,
            0xffe0=>0xa2,0xffe1=>0xa3,0xffe2=>0xac,0xffe3=>0xaf,
            0xffe4=>0xa6,0xffe5=>0xa5,0xffe6=>0x20a9,_=>-1
        };
        return baseScalar>=0;
    }

    /// <summary>
    /// Native param3=false: scan acoustic labels in ascending order and require
    /// a single UTF32 scalar, a wide substitution, and an exact alphabet lookup.
    /// Restricted native punctuation/letter eligibility is outside this method.
    /// </summary>
    public static LegacyAcousticCachePair[] CreateUnrestricted(LegacyInkAlphabet alphabet,
        CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(alphabet);cancellationToken.ThrowIfCancellationRequested();
        var pairs=new List<LegacyAcousticCachePair>(104);
        for(int label=0;label<alphabet.LabelCount;label++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if(!alphabet.TryGetText((ushort)label,out string text))continue;
            if(Rune.DecodeFromUtf16(text,out var rune,out int consumed)!=OperationStatus.Done||consumed!=text.Length||
                !TryGetWideBase(rune.Value,out int baseScalar)||!alphabet.TryGetLabel(new Rune(baseScalar).ToString(),out ushort baseLabel))continue;
            pairs.Add(new(baseLabel,(ushort)label));
        }
        return pairs.ToArray();
    }
}
