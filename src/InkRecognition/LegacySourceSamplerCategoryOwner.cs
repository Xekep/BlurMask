using System.Buffers.Binary;
using System.Security.Cryptography;

namespace InkRecognition;

/// <summary>Immutable actual selected247/v10d→resource130 sampler297/v106→runtime40
/// category owner. Source offsets remain distinct: Russian main0 owns188222;
/// first child6120 owns11571. This is not selected by alphabet label equivalence.</summary>
public sealed class LegacySourceSamplerCategoryOwner
{
    public int SelectedSourceOffset {get;}
    public int SamplerNodeOffset {get;}
    public int CategoryProfileOffset {get;}
    public string SourceFingerprint {get;}
    public LegacyCursiveCategoryProfile Categories {get;}
    public int? OwningProcessor249Offset {get;}
    /// <summary>158BC0 resource reader's actual tail.155DA0 initially resets1C8 to1
    /// but its later tail copy restores the loaded source1C8 qword. Loaded249 clones
    /// therefore preserve this value; unbound raw constructor policy is separate.</summary>
    public float? Serialized249SourceScoreExponent {get;}
    private LegacySourceSamplerCategoryOwner(int source,int sampler,int profile,string fingerprint,LegacyCursiveCategoryProfile categories,
        int? processor=null,float? exponent=null)
    {SelectedSourceOffset=source;SamplerNodeOffset=sampler;CategoryProfileOffset=profile;SourceFingerprint=fingerprint;Categories=categories;
        OwningProcessor249Offset=processor;Serialized249SourceScoreExponent=exponent;}

    public static LegacySourceSamplerCategoryOwner ReadV10dSource(ReadOnlySpan<byte> resource,int selectedSourceOffset,
        CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var configuration=LegacyOrdinarySourceInk.ReadV10dResourceProfile(resource,selectedSourceOffset);
        int end=NodeEnd(resource,selectedSourceOffset,0x247,resource.Length);
        int at=configuration.EnclosureFlagOffset+12;if(at>end-4)throw new InvalidDataException("Missing owned processor array.");
        int count=BinaryPrimitives.ReadInt32LittleEndian(resource[at..]);at+=4;
        if(count is <0 or >256)throw new InvalidDataException("Invalid processor count.");
        for(int i=0;i<count;i++){cancellationToken.ThrowIfCancellationRequested();at=NodeEnd(resource,at,null,end);}
        //1D4950 v10d reads local57, then1D47C0 assigns resource130.
        if(at>=end||resource[at++]!=1)throw new NotSupportedException("Selected source does not own a sampler/graph/acoustic trio.");
        int sampler=at;int samplerEnd=NodeEnd(resource,sampler,0x297,end);
        if(BinaryPrimitives.ReadUInt16LittleEndian(resource[(sampler+14)..])!=0x106)
            throw new NotSupportedException("Only proved297 sampler version106 is supported.");
        int profile=checked(sampler+65);if(profile>samplerEnd-4)throw new InvalidDataException("Truncated sampler category payload.");
        if(resource[profile-1]!=1)throw new NotSupportedException("Sampler does not own its category structure.");
        var categories=LegacyCursiveCategoryProfile.Load(resource.Slice(profile,samplerEnd-profile),profile,out _,cancellationToken);
        string fingerprint=Convert.ToHexString(SHA256.HashData(resource.Slice(selectedSourceOffset,end-selectedSourceOffset))).ToLowerInvariant();
        return new(selectedSourceOffset,sampler,profile,fingerprint,categories);
    }

    /// <summary>First249/version100 reader158BC0 owns its direct nested247/v10d.
    /// This is the child driver1E0 used by156200/156630, not its parent main247.</summary>
    public static LegacySourceSamplerCategoryOwner ReadFirstChildOf249(ReadOnlySpan<byte> resource,int processor249Offset,
        CancellationToken cancellationToken=default)
    {
        int end=NodeEnd(resource,processor249Offset,0x249,resource.Length);
        if(BinaryPrimitives.ReadUInt16LittleEndian(resource[(processor249Offset+14)..])!=0x100)
            throw new NotSupportedException("Confirmed249/version100 direct child layout required.");
        int child=processor249Offset+16;int childEnd=NodeEnd(resource,child,0x247,end);
        if(childEnd!=end-4)throw new NotSupportedException("249 must own only the direct child and its4-byte source exponent.");
        float exponent=BinaryPrimitives.ReadSingleLittleEndian(resource[childEnd..]);
        if(!float.IsFinite(exponent))throw new InvalidDataException("Finite serialized249 exponent required.");
        var owner=ReadV10dSource(resource.Slice(0,end),child,cancellationToken);
        return new(owner.SelectedSourceOffset,owner.SamplerNodeOffset,owner.CategoryProfileOffset,owner.SourceFingerprint,
            owner.Categories,processor249Offset,exponent);
    }

    private static int NodeEnd(ReadOnlySpan<byte>b,int at,int? kind,int end)
    {
        if(at<0||end>b.Length||at>end-16||BinaryPrimitives.ReadUInt32LittleEndian(b[at..])!=0x101fe90||BinaryPrimitives.ReadUInt16LittleEndian(b[(at+12)..])!=0)
            throw new InvalidDataException("Complete unencoded resource required.");
        uint length=BinaryPrimitives.ReadUInt32LittleEndian(b[(at+4)..]);
        if(length<8||length>end-at-8||kind is{} type&&BinaryPrimitives.ReadInt32LittleEndian(b[(at+8)..])!=type)
            throw new InvalidDataException("Wrong/truncated resource envelope.");
        return checked(at+8+(int)length);
    }
}
