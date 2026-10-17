using System.Buffers.Binary;

namespace InkRecognition;

/// <summary>Complete known297/106 or108 scalar/category wire profile, bound to
/// one delegated source130 child. Positive134/1D0 payloads remain unsupported,
/// rather than being skipped or replaced by version106 defaults.</summary>
public sealed class LegacySampler297WireProfile
{
    public LegacySource247WireLayout Source {get;}
    public LegacySource247WireChild Child {get;}
    public int CategoryOffset {get;}
    private readonly byte[] category;
    public IReadOnlyList<LegacyResourceWireField> Fields {get;}
    private LegacySampler297WireProfile(LegacySource247WireLayout source,LegacySource247WireChild child,
        int categoryOffset,byte[] category,List<LegacyResourceWireField> fields)
    {Source=source;Child=child;CategoryOffset=categoryOffset;this.category=category;Fields=fields.AsReadOnly();}

    /// <summary>Fresh category object from the retained same-resource bytes;
    /// public recoverable ReadOnlyMemory cannot mutate this owner's snapshot.</summary>
    public LegacyCursiveCategoryProfile ReadCategories(CancellationToken token=default)
        =>LegacyCursiveCategoryProfile.Load(category,CategoryOffset,out _,token);

    internal static LegacySampler297WireProfile Read(LegacySource247WireLayout owner,LegacySource247WireChild child,
        ReadOnlySpan<byte> serialized,CancellationToken token)
    {
        owner.ValidateChild(child,token);
        if(child.Destination!="247.130"||child.Kind!=0x297||child.Version is not(0x106 or 0x108))
            throw new NotSupportedException("Actual source130 sampler297/106 or108 required.");
        byte[] bytes=serialized.ToArray();var r=new LegacyResourceWireReader(bytes,child.Offset,token);
        r.Take("297","envelope",1,16);
        foreach(string f in new[]{"08","0C","10","14"})r.U32("297",f);
        foreach(string f in new[]{"18","19","1A"})r.Flag("297",f);
        foreach(string f in new[]{"1C","20","24","28","2C","30"})r.U32("297",f);
        r.U8("297","12C");r.U32("297","130");
        if(!r.Flag("297","40.categoryPresent"))throw new NotSupportedException("The known category-bearing sampler branch is required.");
        int categoryOffset=r.Absolute;
        //The serialized category version is separate from runtime category38.
        if(bytes.Length-r.Position<2)throw new InvalidDataException("Truncated category version.");
        if(BinaryPrimitives.ReadUInt16LittleEndian(bytes.AsSpan(r.Position))!=0x201)
            throw new NotSupportedException("Only category201 is supported.");
        var categories=LegacyCursiveCategoryProfile.Load(bytes.AsSpan(r.Position),categoryOffset,out int categoryBytes,token);
        if(categories.BaseEntryCount!=owner.Alphabet.LabelCount)throw new InvalidDataException("Sampler category domain differs from its actual source AL.");
        byte[] category=r.Take("297","40.category201",1,categoryBytes);
        r.U32("297","48");r.U32("297","4C");r.Flag("297","50");
        foreach(string f in new[]{"54","58","5C","60","68","6C","70","74"})r.U32("297",f);
        for(int row=0;row<6;row++)r.Take("297",$"{0x78+row*24:X}.matrixRow",4,6);
        r.U32("297","108");r.Flag("297","10C");
        foreach(string f in new[]{"110","114","118","11C","120","124"})r.U32("297",f);
        if(child.Version>=0x107)
        {
            if(r.Flag("297","134.present"))
                throw new NotSupportedException("Positive sampler134 requires the actual214 payload reader; no version106 defaults are substituted.");
            if(child.Version>=0x108&&r.Flag("297","1D0.present"))
                throw new NotSupportedException("Positive sampler1D0 requires the actual delegated resource and1A0 scalar reader.");
        }
        r.RequireEnd();return new(owner,child,categoryOffset,category,r.Fields);
    }
}
