using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMainDocumentSelectedSources.Probe")]

namespace InkRecognition;

/// <summary>An immutable overlay on an actual CHAR payload. Other fields and
/// strings retain OriginalSource ownership; flags18 and selected index44 are
/// the only modeled writes. No native pointer/padding bytes are invented.</summary>
public sealed class LegacyOwnedMainDocumentCharacterState
{
    public LegacyOwnedMainTextSourcePayload OriginalSource {get;}
    public uint MetadataFlags {get;}
    public uint Flags {get;}
    public int SelectedChoiceIndex44 {get;}
    internal LegacyOwnedMainDocumentCharacterState(LegacyOwnedMainTextSourcePayload source,
        uint metadataFlags,uint flags,int index)
    {OriginalSource=source;MetadataFlags=metadataFlags;Flags=flags;SelectedChoiceIndex44=index;}
}

public readonly record struct LegacyMainDocumentCharacterSelectionWrite(
    int TreeOrdinal,int SourceNodeIdentity,int SelectedMarker);

/// <summary>Actual01FD40/020000 fresh source overlay after01C740. Complete
/// CHAR tree and actual selected WORD/CHAR links are retained. Prior-owner
/// V128 and an actually reached raw-object C25 lookup are unsupported;
///020920 and final document acceptance remain separate.</summary>
public sealed class LegacyFreshMainDocumentSelectedSources
{
    public LegacyFreshMainDocumentCharacterCoverage CharacterCoverage {get;}
    public LegacyFreshMainDocumentTextSelection Text=>CharacterCoverage.Text;
    public LegacyOwnedMainWordSourcePayload OriginalWordSource=>Text.WordChoices.Source;
    public LegacyOwnedMainWordSourceChoiceRow SelectedWord {get;}
    public uint WordMetadataFlags {get;}
    public uint WordFlags {get;}
    public int WordSelectedChoiceIndex44 {get;}
    public int CharacterTreeRootIdentity {get;}
    public IReadOnlyList<LegacyMainTextSourceTreeSnapshot> CharacterTree {get;}
    /// <summary>Native in-order01FD40 traversal of the complete actual CHAR tree.</summary>
    public IReadOnlyList<LegacyOwnedMainDocumentCharacterState> Characters {get;}
    public IReadOnlyList<LegacyMainDocumentCharacterSelectionWrite> SelectionWrites {get;}

    private LegacyFreshMainDocumentSelectedSources(LegacyFreshMainDocumentCharacterCoverage source,
        LegacyOwnedMainWordSourceChoiceRow selected,uint wordMetadata,int selectedIndex,int root,
        LegacyMainTextSourceTreeSnapshot[] tree,LegacyOwnedMainDocumentCharacterState[] characters,
        LegacyMainDocumentCharacterSelectionWrite[] writes)
    {CharacterCoverage=source;SelectedWord=selected;WordMetadataFlags=wordMetadata;
     WordFlags=wordMetadata|0x08000000;WordSelectedChoiceIndex44=selectedIndex;CharacterTreeRootIdentity=root;
     CharacterTree=Array.AsReadOnly(tree);Characters=Array.AsReadOnly(characters);SelectionWrites=Array.AsReadOnly(writes);}

    public static LegacyFreshMainDocumentSelectedSources Prepare(
        LegacyFreshMainDocumentCharacterCoverage actualCoverage,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualCoverage);token.ThrowIfCancellationRequested();
        var text=actualCoverage.Text;var words=text.WordChoices;ValidatePrefix(text,token);
        var payloads=new Dictionary<int,LegacyOwnedMainTextSourcePayload>();
        foreach(var choice in words.Source.Choices)
            foreach(var link in choice.CharacterLinks)
            {
                token.ThrowIfCancellationRequested();var source=link.Source;int id=source.SourceNode.Identity;
                if(payloads.TryGetValue(id,out var previous)&&!ReferenceEquals(previous,source))
                    throw new ArgumentException("One actual CHAR node cannot have two different payload owners.");
                payloads[id]=source;
            }
        if(payloads.Count==0)throw new NotSupportedException("Actual complete nonempty CHAR source tree required.");
        var interner=payloads.Values.First().SourceNode.Owner;var nodes=interner.CopyInsertionOrder();
        var tree=interner.CopyTree();int root=interner.RootIdentity;
        if(nodes.Count!=payloads.Count||tree.Length!=nodes.Count||!ReferenceEquals(interner.Document,text.Document)||
            nodes.Any(n=>!payloads.TryGetValue(n.Identity,out var p)||!ReferenceEquals(p.SourceNode,n)||!ReferenceEquals(n.Owner,interner)))
            throw new NotSupportedException("Every actual CHAR tree node must retain its exact typed payload; missing/unreferenced payload reconstruction is not supported.");
        var shape=tree.ToDictionary(n=>n.Identity);var order=new List<int>();var stack=new Stack<int>();int cursor=root;
        while(cursor!=0||stack.Count!=0)
        {
            while(cursor!=0)
            {
                token.ThrowIfCancellationRequested();if(stack.Count==64||!shape.TryGetValue(cursor,out var node))
                    throw new NotSupportedException("Native64-entry source traversal budget or complete tree identity is unavailable.");
                stack.Push(cursor);cursor=node.Left;
            }
            cursor=stack.Pop();order.Add(cursor);cursor=shape[cursor].Right;
            if(order.Count>tree.Length)throw new InvalidDataException("Cyclic source tree.");
        }
        if(order.Count!=tree.Length||order.Distinct().Count()!=tree.Length)
            throw new InvalidDataException("Source tree traversal did not visit every actual node exactly once.");
        var metadata=new uint[order.Count];var finalFlags=new uint[order.Count];var indices=new int[order.Count];
        for(int i=0;i<order.Count;i++)
        {
            token.ThrowIfCancellationRequested();var source=payloads[order[i]];uint flags=source.Flags;
            //01FD40 guards are lazy: masked sources do not read prior owners
            //or original keys. A bit29 source with selectionID -1 also skips.
            if((flags&0x10600000)==0)
            {
                if((flags&0x20000000)!=0)
                {
                    if(source.SelectionId!=-1)
                        throw new NotSupportedException("Actually used prior source V128 owner is not modeled.");
                }
                else if(source.SourceNode.RangeCount==1)
                {
                    byte[] key=source.SourceNode.CopyRanges16();
                    bool lookup=BinaryPrimitives.ReadUInt32LittleEndian(key.AsSpan(4))==BinaryPrimitives.ReadUInt32LittleEndian(key.AsSpan(12))&&
                        BinaryPrimitives.ReadUInt16LittleEndian(key.AsSpan(2))==0&&key[1]==0&&
                        BinaryPrimitives.ReadUInt16LittleEndian(key.AsSpan(10))==3&&key[9]==255;
                    if(lookup)throw new NotSupportedException("Native01FD40 actually reaches C25 raw-object lookup; its owning object-kind producer is not yet available.");
                }
            }
            //All supported CHAR branches perform no metadata write; the
            //C25-dependent write branch above is explicitly not guessed.
            metadata[i]=finalFlags[i]=flags;indices[i]=unchecked((int)(source.Field40>>32));
        }
        var alternative=words.Alternatives[actualCoverage.SourceCoverage.NativeSelectedAlternativeIndex];
        int selectedIndex=alternative.SelectedWordIndex;var selected=words.Source.Choices[selectedIndex];
        var ordinal=order.Select((id,index)=>(id,index)).ToDictionary(x=>x.id,x=>x.index);
        bool all=true;
        foreach(var link in selected.CharacterLinks)
            if((metadata[ordinal[link.Source.SourceNode.Identity]]&0x11004000)==0){all=false;break;}
        uint wordMetadata=(words.Source.Flags&0xfeffffff)|(all?0x01000000u:0);
        var writes=selected.CharacterLinks.Select(link=>new LegacyMainDocumentCharacterSelectionWrite(
            ordinal[link.Source.SourceNode.Identity],link.Source.SourceNode.Identity,link.SelectedMarker)).ToArray();
        ApplySelectedCharacters(finalFlags,indices,writes.Select(w=>w.TreeOrdinal).ToArray(),writes.Select(w=>w.SelectedMarker).ToArray());
        ValidatePrefix(text,token);
        if(interner.RootIdentity!=root||!interner.CopyTree().AsSpan().SequenceEqual(tree))
            throw new InvalidOperationException("The actual CHAR source tree changed during metadata preparation.");
        var result=order.Select((id,i)=>new LegacyOwnedMainDocumentCharacterState(payloads[id],metadata[i],finalFlags[i],indices[i])).ToArray();
        return new(actualCoverage,selected,wordMetadata,selectedIndex,root,tree,result,writes);
    }

    //020000 writes each selected CHAR in link order. Repeated sources retain
    //the last raw signed marker; flags are ORed without clearing other bits.
    internal static void ApplySelectedCharacters(Span<uint> flags,Span<int> indices,
        ReadOnlySpan<int> sourceOrdinals,ReadOnlySpan<int> markers)
    {
        if(flags.Length!=indices.Length||sourceOrdinals.Length!=markers.Length)
            throw new ArgumentException("Matching selection storage required.");
        if(indices.Overlaps(sourceOrdinals)||indices.Overlaps(markers))
            throw new ArgumentException("Native CHAR output fields and selected link inputs must be separate storage.");
        foreach(int index in sourceOrdinals)if((uint)index>=(uint)flags.Length)throw new ArgumentOutOfRangeException(nameof(sourceOrdinals));
        for(int i=0;i<sourceOrdinals.Length;i++)
        {int index=sourceOrdinals[i];flags[index]|=0x08000000;indices[index]=markers[i];}
    }
    internal void Validate(CancellationToken token=default)
    {
        ValidatePrefix(Text,token);
        var actualTree=Characters[0].OriginalSource.SourceNode.Owner;
        if(actualTree.RootIdentity!=CharacterTreeRootIdentity||!actualTree.CopyTree().SequenceEqual(CharacterTree))
            throw new InvalidOperationException("The captured complete CHAR source tree is no longer current.");
    }
    private static void ValidatePrefix(LegacyFreshMainDocumentTextSelection text,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();foreach(var word in text.WordChoices.Owner.WordViews)word.Validate(token);
        var current=text.CopyPendingSelections();var prefix=text.WordChoices.PendingSelections;
        if(!text.IsRegistered||text.IsFaulted||current.Count!=prefix.Count+1||!ReferenceEquals(current[^1],text.Selection)||
            !current.Take(prefix.Count).SequenceEqual(prefix))throw new ArgumentException("Exact current CHAR/WORD/TEXT source prefix required.");
    }
}
