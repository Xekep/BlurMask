using System.Buffers.Binary;

namespace InkRecognition;

public readonly record struct LegacyFirstLiteralGlyphSymbolPlan(ushort SelectedLabel,ushort PublishedLabel,
    int CompositeCategory,byte GeometryIndex,ushort CategoryMismatchFlag4,ushort PublicationFlags6);

/// <summary>Fresh6C branch of1F42D0: literal symbol/count1, with the preceding
/// E2 blank-label replacement. Does not select a glyph, fold script/case,
/// construct word boundaries or infer an alternate alphabet.</summary>
public sealed class LegacyFreshFirstGlyphSymbolPlan
{
    private readonly LegacyFreshFirstResultOwnershipFactory factory;
    private readonly LegacyFirstTransitionGeometryProfile profile;
    private readonly ushort headerE,replacement;
    private LegacyFreshFirstGlyphSymbolPlan(LegacyFreshFirstResultOwnershipFactory factory,
        LegacyFirstTransitionGeometryProfile profile,ushort headerE,ushort replacement)
    {this.factory=factory;this.profile=profile;this.headerE=headerE;this.replacement=replacement;}

    public static LegacyFreshFirstGlyphSymbolPlan ReadSource(ReadOnlySpan<byte> resource,int sourceOffset,
        LegacyFreshFirstResultOwnershipFactory factory,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(factory);cancellationToken.ThrowIfCancellationRequested();
        var model=LegacyFirstAcousticModel.ReadSource(resource,sourceOffset,cancellationToken);
        if(model.SourceFingerprint!=factory.SourceFingerprint||sourceOffset!=factory.InvocationPolicy.Binding.SourceOffset||!factory.Configuration6C)
            throw new ArgumentException("Actual selected fresh6C factory and matching source required.");
        var profile=LegacyFirstTransitionGeometryProfile.ReadSource(resource,sourceOffset,cancellationToken);
        int p=sourceOffset+20+4+56;int marker=BinaryPrimitives.ReadInt32LittleEndian(resource[p..]);p+=4;if(marker==-1)p+=4;p+=11;
        ushort headerE=BinaryPrimitives.ReadUInt16LittleEndian(resource[(p+6)..]);
        if(headerE==ushort.MaxValue||headerE>=factory.InvocationPolicy.Binding.Alphabet.LabelCount)
            throw new NotSupportedException("Fresh literal blank conversion requires its actual headerE label.");
        ushort replacement=factory.InvocationPolicy.Binding.Alphabet.TryGetLabel("\ufffd",out ushort label)?label:ushort.MaxValue;
        return new(factory,profile,headerE,replacement);
    }

    public LegacyFirstLiteralGlyphSymbolPlan Create(ushort actualSelectedLabel,byte actualPrimaryCompositeCategory,
        CancellationToken cancellationToken=default)
    {
        cancellationToken.ThrowIfCancellationRequested();int count=factory.InvocationPolicy.Binding.Alphabet.LabelCount;
        if(actualSelectedLabel>count+2||actualPrimaryCompositeCategory>63)
            throw new ArgumentOutOfRangeException(nameof(actualSelectedLabel),"Actual bounded cache label and six-bit primary category required.");
        //1EDDE0 E2=alphabet.virtual10 count.1F42 replaces it with alphabet+E
        //before testing6C. All other labels remain literal in this branch.
        ushort published=actualSelectedLabel==count?headerE:actualSelectedLabel;
        if(published>=profile.Categories.StartCompositeMap.Length)
            throw new ArgumentException("Actual symbol lies outside its owned category-map storage.");
        int category=profile.Categories.StartCompositeMap.Span[published];
        byte geometry=category<0?(byte)0:checked((byte)profile.Categories.CompositeCategories.Span[category].GeometryIndex);
        ushort mismatch=published==headerE||category==actualPrimaryCompositeCategory?(ushort)0:(ushort)8;
        ushort flags=published==replacement?(ushort)0x48:(ushort)0x40;
        return new(actualSelectedLabel,published,category,geometry,mismatch,flags);
    }

    /// <summary>Apply13AF30 count1/kind0, then1F42 category/result flag updates;
    /// existing descriptor IDs, scalar fields and unrelated flag bits remain owned.</summary>
    public void Apply(LegacyNativeGlyphRecordPool pool,ulong actualReference,LegacyFirstLiteralGlyphSymbolPlan plan,
        byte actualPrimaryCompositeCategory,CancellationToken cancellationToken=default)
    {
        ArgumentNullException.ThrowIfNull(pool);cancellationToken.ThrowIfCancellationRequested();
        if(plan!=Create(plan.SelectedLabel,actualPrimaryCompositeCategory,cancellationToken))
            throw new ArgumentException("Literal glyph plan belongs to a different selected-source policy.");
        _=LegacyNativeGlyphSymbolUpdate.Apply(pool,actualReference,plan.PublishedLabel,1,0,false,cancellationToken);
        byte[] glyph=pool.CopyRecord(actualReference,cancellationToken);
        ushort kind=BinaryPrimitives.ReadUInt16LittleEndian(glyph.AsSpan(4));
        BinaryPrimitives.WriteUInt16LittleEndian(glyph.AsSpan(4),(ushort)((kind&0xfff7)|plan.CategoryMismatchFlag4));
        ushort flags=BinaryPrimitives.ReadUInt16LittleEndian(glyph.AsSpan(6));
        BinaryPrimitives.WriteUInt16LittleEndian(glyph.AsSpan(6),(ushort)(flags|plan.PublicationFlags6));
        pool.ReplaceRecord(actualReference,glyph,cancellationToken);
    }
}
