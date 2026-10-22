using System.Buffers.Binary;
namespace InkRecognition;

//The exact full source policy. Numeric equality to lite never promotes ownership.
internal sealed class LegacyFullEnglishFirstPrimaryPolicy
{
    internal readonly LegacyFullEnglishFirstPolicy Profile;
    internal LegacyCursiveCategoryProfile Categories=>Profile.Categories;
    internal readonly ushort Blank,Closed,Replacement,Shadda,Newline,Space,NonbreakingSpace;
    internal readonly bool HasBlankHeader,ExcludeConsecutiveBlank;
    internal readonly int Width;
    internal LegacyFullEnglishFirstPrimaryPolicy(LegacyFullEnglishFirstControllerCosts owner,LegacyFullEnglishFirstControllerChild child,CancellationToken token)
    {
        Profile=new(owner.Owner,child.ActualCache,token);var source=Profile.Model.Source;var alphabet=source.Alphabet;
        source.ValidateChild(Profile.Model.AcousticChild,token);
        byte[] B(string n)=>source.Fields.Single(f=>f.Owner=="247"&&f.Destination==n).CopyBytes();
        Width=BinaryPrimitives.ReadUInt16LittleEndian(B("BA"));
        if(B("B8")[0]!=0||B("B9")[0]!=0||Width!=5||BinaryPrimitives.ReadUInt16LittleEndian(B("BC"))!=ushort.MaxValue)
            throw new NotSupportedException("Actual full FIRST policy bytes required.");
        ExcludeConsecutiveBlank=false;Blank=checked((ushort)alphabet.LabelCount);
        Closed=alphabet.TryGetLabel("\U000f0001",out var c)?c:ushort.MaxValue;
        Replacement=alphabet.TryGetLabel("\ufffd",out var r)?r:ushort.MaxValue;
        Shadda=alphabet.TryGetLabel("\u0651",out var s)?s:ushort.MaxValue;
        if(Blank!=217||Closed!=211||Replacement!=209||Shadda!=ushort.MaxValue||alphabet.TryGetLabel("\U000f0009",out _))
            throw new NotSupportedException("Actual full zero-trait alphabet required.");
        var h=Profile.Header;HasBlankHeader=BinaryPrimitives.ReadUInt16LittleEndian(h.AsSpan(6))!=ushort.MaxValue;
        Newline=BinaryPrimitives.ReadUInt16LittleEndian(h.AsSpan(8));Space=BinaryPrimitives.ReadUInt16LittleEndian(h.AsSpan(10));NonbreakingSpace=BinaryPrimitives.ReadUInt16LittleEndian(h.AsSpan(12));
        if(Newline!=184||Space!=182||NonbreakingSpace!=183||!HasBlankHeader)throw new NotSupportedException("Actual full FIRST header required.");
    }
}
