using System.Buffers.Binary;
namespace InkRecognition;

internal readonly record struct LegacyFullEnglishFirstSourceRecordEntry(int Child,int Word,int Ordinal,
    ulong Reference,int Owner,int Range,short Selected,bool Flat);
internal sealed class LegacyFullEnglishFirstSourceRecordChild
{
    internal readonly LegacyFullEnglishFirstGlyphEmissionChild Child;
    internal readonly LegacyFullEnglishFirstChildInput Input;
    internal readonly int[] Map;
    internal readonly IReadOnlyList<LegacyNativeGlyphRecordNode> Owners;
    internal readonly LegacyFirstResultSourceRange[][] Ranges;
    internal readonly LegacyFullEnglishFirstSourceRecordEntry[] Entries;
    internal LegacyFullEnglishFirstSourceRecordChild(LegacyFullEnglishFirstGlyphEmissionChild child,
        LegacyFullEnglishFirstChildInput input,int[] map,IReadOnlyList<LegacyNativeGlyphRecordNode> owners,
        LegacyFirstResultSourceRange[][] ranges,LegacyFullEnglishFirstSourceRecordEntry[] entries)
    {Child=child;Input=input;Map=map;Owners=owners;Ranges=ranges;Entries=entries;}
}

internal sealed class LegacyFullEnglishFirstSourceRecordPolicy
{
    internal readonly LegacyFullEnglishFirstGlyphEmission Owner;
    internal readonly LegacyFullEnglishFirstParentInput Parent;
    internal readonly LegacyCursiveCategoryProfile Categories;
    private readonly LegacyCursiveCategoryProfile canonicalCategories;
    internal readonly LegacyFullEnglishFirstSourceRecordChild[] Children;
    internal readonly InkStroke[] Points;
    internal readonly LegacySourcePointIdentity[] Chronology;
    internal readonly byte[][] Descriptors;
    internal readonly float Exponent;
    internal readonly ushort Underscore;
    internal readonly int RowCount,FragmentCount,PointVisits;
    internal LegacyFullEnglishFirstSourceRecordPolicy(LegacyFullEnglishFirstGlyphEmission owner,CancellationToken token)
    {
        Owner=owner;Parent=owner.Owner.Owner.Owner.Owner.Owner.Owner.Parent;RequireComplete();
        var children=owner.CopyCompletedChildren();
        if(children.Count is<1 or>256||children.Count!=Parent.Children.Count)throw new ArgumentException("Complete canonical full child set required.");
        var model=Parent.Model;var source=model.Source;
        if(model.SelectedProcessor.Kind!=0x249||model.SelectedProcessor.Version!=0x100||model.SelectedProcessor.Offset!=8827||
           model.SelectedProcessor.End!=373961||source.Offset!=8843||source.End!=373957||source.Version!=0x110||source.SourceKind!=0x135||
           source.Alphabet.LabelCount!=217||model.SourceScoreExponent!=.75f)
            throw new NotSupportedException("Same actual full249/source135/110/.75 required.");
        model.ParentSource.ValidateChild(model.SelectedProcessor,token);Exponent=model.SourceScoreExponent;
        //This is the ACTUAL fullsource readable2B table. No literal label188.
        Underscore=source.Alphabet.TryGetLabel("_",out ushort underscore)?underscore:ushort.MaxValue;
        //The exposed Factory profile is canonical IDENTITY ONLY: its public ROM
        //can be recovered/mutated. Geometry must come from retained authenticated
        //source bytes, not copied caller-tampered arrays or a shared profile.
        canonicalCategories=children[0].Original.Factory.Categories;
        var samplerChild=source.Children.Single(c=>c.Destination=="247.130");
        if(samplerChild.Kind!=0x297||samplerChild.Version!=0x108||samplerChild.Offset!=16952||samplerChild.End!=18340)
            throw new NotSupportedException("Actual bounded fullsource297/108 category owner required.");
        source.ValidateChild(samplerChild,token);
        var privateSampler=source.ReadSampler(samplerChild,token);
        if(!ReferenceEquals(privateSampler.Source,source)||!ReferenceEquals(privateSampler.Child,samplerChild)||
           privateSampler.CategoryOffset!=17017||privateSampler.Fields.Count!=45)
            throw new InvalidDataException("Actual raw fullsource sampler/category framing required.");
        Categories=privateSampler.ReadCategories(token);
        if(Categories.BaseEntryCount!=217||Categories.FirstIds.Length!=7||Categories.Prototypes.Length!=24||Categories.CompositeCategories.Length!=42||Categories.StartGeometryMap.Length!=220||Categories.EndGeometryMap.Length!=220)
            throw new NotSupportedException("Actual fullsource geometry profile required.");
        var current=Parent.Current;int count=current.Count;
        if(count is<1 or>256)throw new NotSupportedException("Actual retained current-parent physical cap.");
        var references=new IReadOnlyList<InkPoint>[count];var counts=new int[count];int total=0;
        for(int s=0;s<count;s++)
        {
            token.ThrowIfCancellationRequested();references[s]=current[s].Points;int n=references[s].Count;
            if(n is<1 or>4096||n>8192-total)throw new NotSupportedException("Whole B8 current-parent8192-point cap.");
            counts[s]=n;total+=n;
            for(int i=0;i<n;i++){token.ThrowIfCancellationRequested();var pt=references[s][i];if(!float.IsFinite(pt.X)||!float.IsFinite(pt.Y))throw new InvalidDataException("Finite actual parent points required.");}
        }
        Points=new InkStroke[count];Chronology=new LegacySourcePointIdentity[total];int point=0;
        for(int s=0;s<count;s++)
        {
            token.ThrowIfCancellationRequested();var p=references[s];if(p.Count!=counts[s])throw new InvalidDataException("Actual current source changed during snapshot.");
            var copied=new InkPoint[counts[s]];
            for(int i=0;i<copied.Length;i++)
            {
                token.ThrowIfCancellationRequested();var value=p[i];if(!float.IsFinite(value.X)||!float.IsFinite(value.Y))throw new InvalidDataException("Actual finite source snapshot required.");
                copied[i]=value;Chronology[point++]=Parent.ResolveCurrentPoint(s,i);
                //Resolve the actual ending provenance once, without retaining a
                //second24B-per-point array or inventing timestamps from keys.
                _=Parent.ResolveCurrentEndingMetadata(s,i,token);
            }
            if(p.Count!=counts[s])throw new InvalidDataException("Actual current source changed during snapshot.");Points[s]=new(copied);
        }
        Descriptors=Parent.CopyDescriptors();if(Descriptors.Length>511||Descriptors.Any(d=>d.Length!=88))throw new NotSupportedException("Bounded actual descriptor88 array required.");
        Children=new LegacyFullEnglishFirstSourceRecordChild[children.Count];int rows=0,allRefs=0,words=0,retained=0,fragments=0,visits=0,previousDescriptor=-1;
        for(int c=0;c<Children.Length;c++)
        {
            token.ThrowIfCancellationRequested();var child=children[c];var original=child.Original;var input=original.Factory.Candidates.Child;
            if(!ReferenceEquals(child.Owner,owner)||!ReferenceEquals(original.Owner,owner.Owner)||!ReferenceEquals(input,Parent.Children[c])||
               !ReferenceEquals(input.Owner,Parent)||!ReferenceEquals(original.Factory.Categories,canonicalCategories)||
               !ReferenceEquals(original.Factory.Initializer.ActualCache.Model,model)||!ReferenceEquals(original.Factory.Preparation.Source,source))
                throw new ArgumentException("Same actual B7/B6/fullsource/canonical child/category owner required.");
            int d=input.DescriptorOrdinal;if(d<=previousDescriptor||(uint)d>=(uint)Descriptors.Length)throw new InvalidDataException("Actual descriptor order required.");previousDescriptor=d;
            var descriptor=Descriptors[d];int kind=descriptor[0]&15;
            if(kind is not(0 or 6)||BinaryPrimitives.ReadInt32LittleEndian(descriptor.AsSpan(56))>BinaryPrimitives.ReadInt32LittleEndian(descriptor.AsSpan(52)))
                throw new NotSupportedException("Actual unresolved ordinary interval required.");
            int[] map=input.CopyCurrentParentMap();if(map.Length is<1 or>256||map.Length!=input.Submission.Raw.Count)throw new InvalidDataException("Actual pre-NN physical map required.");
            for(int i=0;i<map.Length;i++)
            {
                token.ThrowIfCancellationRequested();if((uint)map[i]>=(uint)Points.Length||i>0&&map[i]<=map[i-1]||
                   !Points[map[i]].Points.SequenceEqual(input.Submission.Raw[i].Points))throw new InvalidDataException("Child pre-NN source differs from retained parent map.");
            }
            var owners=child.CopyGlyphOwners(token);var index=new Dictionary<ulong,int>();
            for(int i=0;i<owners.Count;i++)
            {
                token.ThrowIfCancellationRequested();var node=owners[i];int n=(node.PackedCount>>2)&2047;
                if(n is<1 or>2040||node.Records.Length!=n*40||node.Key!=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(node.Key)||!index.TryAdd(node.Key,i))
                    throw new InvalidDataException("Faithful full B7 owner records required.");
                retained=checked(retained+n);if(retained>12800)throw new NotSupportedException("Whole B8 retained rich-row reservation exceeded.");
            }
            var ranges=new List<LegacyFirstResultSourceRange[]>();var entries=new List<LegacyFullEnglishFirstSourceRecordEntry>();var encountered=new HashSet<ulong>();
            for(int w=0;w<child.Words.Count;w++)
            {
                token.ThrowIfCancellationRequested();if(++words>8192)throw new NotSupportedException("Whole B8 WORD cap.");
                ulong[] refs=child.Words[w].CopyReadingOrderReferences();
                if(refs.Length>8192-allRefs)throw new NotSupportedException("Whole B8 all-reference cap.");allRefs+=refs.Length;
                for(int r=0;r<refs.Length;r++)
                {
                    token.ThrowIfCancellationRequested();ulong reference=refs[r];var key=LegacyFirstResultGlyphKey.Decode(reference);
                    ulong canonical=LegacyNativeGlyphRecordOwner.NormalizeOwnerKey(reference);
                    if(!index.TryGetValue(canonical,out int oi)||key.Alternative<0||key.Alternative>=owners[oi].Records.Length/40)
                        throw new InvalidDataException("Same full B7 signed WORD alternative required, including delimiters.");
                    if(key.Arc<0)continue;
                    //NoDedup affects append interning; it does NOT bypass the
                    //source-list point-in-time canonical-owner admission gate.
                    if(!encountered.Add(canonical))throw new NotSupportedException("Repeated ordinary canonical owner requires native interleaved720 history.");
                    if(++rows>4096)throw new NotSupportedException("Whole B8 ordinary-row cap, independent of enrichment attempts.");
                    var actual=child.CopySourceRanges(canonical,token);
                    LegacyFullEnglishFirstSourceRecordBudget.Ranges(actual,map,Points,ref fragments,ref visits,token);
                    int ri=ranges.Count;ranges.Add(actual);entries.Add(new(c,w,r,reference,oi,ri,key.Alternative,false));
                }
            }
            if(entries.Count==0)throw new NotSupportedException("Empty ordinary child requires excluded native previous-row/reuse fallback.");
            if(input.IsSoleFlat&&Underscore!=ushort.MaxValue)entries[^1]=entries[^1] with{Flat=true};
            Children[c]=new(child,input,map,owners,ranges.ToArray(),entries.ToArray());
        }
        RowCount=rows;FragmentCount=fragments;PointVisits=visits;RequireComplete();token.ThrowIfCancellationRequested();
    }
    internal void RequireComplete()
    {
        var plain=Owner.Owner;var terminal=plain.Owner;var context=terminal.Owner;var primary=context.Owner;var initializer=primary.Owner;var confidence=initializer.Owner;
        if(!Owner.IsComplete||Owner.IsFaulted||!plain.IsComplete||plain.IsFaulted||!terminal.IsComplete||terminal.IsFaulted||!context.IsComplete||context.IsFaulted||
           !primary.IsComplete||primary.IsFaulted||!initializer.IsComplete||initializer.IsFaulted||!confidence.IsComplete||confidence.IsFaulted||!Parent.IsComplete||Parent.IsFaulted||
           !ReferenceEquals(confidence.Parent,Parent))throw new ArgumentException("Complete nonfaulted canonical full B7..A required; historical prefixes are not authority.");
    }
}
