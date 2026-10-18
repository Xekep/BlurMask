using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Fresh no-external selected249/119 policy, bound to one actual
/// English cache/source. Not a reused or caller-configured20E container.</summary>
public sealed class LegacyOwnedEnglish119FirstPolicy
{
    public LegacyOwnedEnglish119ControllerCosts Owner {get;}
    public int PrimaryBeamWidth {get;}
    public bool ExcludeConsecutiveBlank {get;}
    public bool NativeModeE0 {get;}
    public bool NativeProtocolExclusion194=>false;
    public ushort Blank {get;}
    public ushort Closed {get;}
    public ushort Replacement {get;}
    public ushort Shadda {get;}
    public ushort Newline {get;}
    public ushort Space {get;}
    public ushort NonbreakingSpace {get;}
    internal LegacyOrdinaryGraphSourceRanges SourceRanges {get;}
    internal LegacyCursiveCategoryProfile Categories {get;}
    private LegacyOwnedEnglish119FirstPolicy(LegacyOwnedEnglish119ControllerCosts owner,CancellationToken token)
    {
        Owner=owner;var candidates=owner.Owner.Owner.Owner;var model=owner.Model;var source=model.Source;
        model.ParentSource.ValidateChild(model.SelectedProcessor,token);
        if(!ReferenceEquals(candidates.Model,model)||!ReferenceEquals(candidates.Preparation.Source,source)||
            model.SelectedProcessor.Kind!=0x249||model.SelectedProcessor.Version!=0x100||source.SourceKind!=0x135||
            source.Offset!=model.SelectedProcessor.Offset+16||source.End!=model.SelectedProcessor.End-4)
            throw new ArgumentException("Same actual fresh selected249,119 model and raw source owner required.");
        SourceRanges=candidates.SourceRanges;Categories=candidates.Sampler.ReadCategories(token);
        if(!ReferenceEquals(SourceRanges.OriginalSource,candidates.Preparation.OriginalSource)||
            Categories.BaseEntryCount!=source.Alphabet.LabelCount||Categories.CompositeCategories.Length>64||
            SourceRanges.Graph.NodeCount>4096||SourceRanges.Graph.CandidateCount>131072||candidates.MaximumAtoms>8)
            throw new ArgumentException("Owned bounded fresh ordinary first graph/category domain required.");
        byte[] B(string name)=>source.Fields.Single(f=>f.Owner=="247"&&f.Destination==name).CopyBytes();
        byte b8=B("B8")[0];PrimaryBeamWidth=BinaryPrimitives.ReadUInt16LittleEndian(B("BA"));
        if(b8>1||PrimaryBeamWidth is <1 or >256)throw new NotSupportedException("Bounded actual native B8/BA required.");
        ExcludeConsecutiveBlank=b8!=0;var alphabet=source.Alphabet;Blank=checked((ushort)alphabet.LabelCount);
        Closed=alphabet.TryGetLabel("\U000f0001",out ushort closed)?closed:ushort.MaxValue;
        Replacement=alphabet.TryGetLabel("\ufffd",out ushort replacement)?replacement:ushort.MaxValue;
        Shadda=alphabet.TryGetLabel("\u0651",out ushort shadda)?shadda:ushort.MaxValue;
        byte[] header=alphabet.CopySerializedHeader();Newline=BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(8));
        Space=BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(10));NonbreakingSpace=BinaryPrimitives.ReadUInt16LittleEndian(header.AsSpan(12));
        //The cache's private factory already binds the exact no-Arabic stored
        //AL.1EDDE0 therefore selects the pinned five-zero1EDDB0 resolver.
        //NEW249/20E has C4=0; E0 also tests source137 or exactF0009.
        NativeModeE0=source.SourceKind==0x137||alphabet.TryGetLabel("\U000f0009",out _);
        if(NativeModeE0||Shadda!=ushort.MaxValue||Closed==ushort.MaxValue)
            throw new NotSupportedException("Audited zero-trait ordinary first policy required.");
    }
    internal static LegacyOwnedEnglish119FirstPolicy Prepare(LegacyOwnedEnglish119ControllerCosts owner,CancellationToken token)
    {ArgumentNullException.ThrowIfNull(owner);token.ThrowIfCancellationRequested();return new(owner,token);}
}
