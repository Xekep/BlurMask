using System.Buffers.Binary;
namespace InkRecognition;
public sealed partial class LegacyNativeWordBuilder
{
    //219580's working276 flags32 bit80 precedes218A30/218560. The input is
    //source compact flag10 derived by the private live caller, not a policy knob.
    internal void ApplyAugmentationFlag32(bool actualCandidateBit10,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(HeadReference!=ulong.MaxValue)throw new InvalidOperationException("Freshly reset actual augmentation WORD required.");
        if(actualCandidateBit10)W16(data,50,(ushort)(U16(data,50)|0x80));
    }
}
