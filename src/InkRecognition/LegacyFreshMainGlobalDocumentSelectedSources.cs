using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyGlobalDocumentSelectedSources.Probe")]
namespace InkRecognition;

/// <summary>Immutable01FD40/020000 overlay on the actual complete global CHAR
/// AVL. Native prior/C25 branches refuse only when their lazy guards reach them.</summary>
public sealed class LegacyGlobalDocumentCharacterState
{
    public LegacyFreshMainGlobalTextSourcePayload OriginalSource{get;}
    public uint MetadataFlags{get;}
    public uint Flags{get;}
    public int SelectedChoiceIndex44{get;}
    internal LegacyGlobalDocumentCharacterState(LegacyFreshMainGlobalTextSourcePayload source,uint metadata,uint flags,int index)
    {OriginalSource=source;MetadataFlags=metadata;Flags=flags;SelectedChoiceIndex44=index;}
}

public sealed class LegacyFreshMainGlobalDocumentSelectedSources
{
    private sealed class Snapshot
    {
        internal readonly LegacyFreshMainGlobalTextSourcePayload Source;
        internal readonly LegacyGlobalAssignedPayloadScalars Scalars;
        internal readonly byte[] Key;
        internal readonly byte[][] Rows;
        internal readonly LegacyNativeInternedUtf8?[] Texts;
        internal Snapshot(LegacyFreshMainGlobalTextSourcePayload source,CancellationToken token)
        {
            Source=source;Scalars=source.CopyAssignedScalars();Key=source.Node.CopyRanges16();
            var rows=source.CopyRecords(token);Rows=rows.Select(r=>r.CopyStorage40()).ToArray();Texts=rows.Select(r=>r.Text).ToArray();
        }
        internal void Validate(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if(!Source.IsComplete||Source.CopyAssignedScalars()!=Scalars||!Source.Node.CopyRanges16().AsSpan().SequenceEqual(Key))
                throw new InvalidOperationException("Actual CHAR metadata/source key changed after selection capture.");
            var rows=Source.CopyRecords(token);if(rows.Count!=Rows.Length)throw new InvalidOperationException("Actual CHAR record count changed.");
            for(int i=0;i<Rows.Length;i++)
            {
                token.ThrowIfCancellationRequested();if(!ReferenceEquals(rows[i].Text,Texts[i])||!rows[i].CopyStorage40().AsSpan().SequenceEqual(Rows[i]))
                    throw new InvalidOperationException("Actual CHAR scalar/string owner changed.");
                if(Texts[i] is{} text)Source.Owner.Choices.StringPool.Validate(text);
            }
        }
    }
    private readonly Snapshot[] snapshots;
    private readonly LegacyMainTextSourceTreeSnapshot[] tree;
    private readonly LegacyFreshMainGlobalWordCharacterLink[] links;
    private readonly LegacyGlobalDocumentCharacterState[] characters;
    private readonly LegacyMainDocumentCharacterSelectionWrite[] writes;
    private readonly byte[] word48;
    private readonly uint wordBefore;
    private readonly long wordSelection,next;
    public LegacyFreshMainGlobalDocumentCoverage Coverage{get;}
    public LegacyFreshMainGlobalWordTextPublication Publication=>Coverage.Publication;
    public int CharacterTreeRootIdentity{get;}
    public IReadOnlyList<LegacyMainTextSourceTreeSnapshot> CharacterTree=>Array.AsReadOnly(tree);
    public IReadOnlyList<LegacyGlobalDocumentCharacterState> Characters=>Array.AsReadOnly(characters);
    public IReadOnlyList<LegacyMainDocumentCharacterSelectionWrite> SelectionWrites=>Array.AsReadOnly(writes);
    public uint WordMetadataFlags{get;}
    public uint WordFlags=>WordMetadataFlags|0x08000000u;
    public int WordSelectedChoiceIndex44=>Publication.SelectedWordIndex;
    private LegacyFreshMainGlobalDocumentSelectedSources(LegacyFreshMainGlobalDocumentCoverage coverage,
        Snapshot[] snapshots,LegacyMainTextSourceTreeSnapshot[] tree,int root,
        LegacyFreshMainGlobalWordCharacterLink[] links,LegacyGlobalDocumentCharacterState[] states,
        LegacyMainDocumentCharacterSelectionWrite[] writes,uint wordMetadata,uint originalWordFlags,long originalWordSelection,byte[] originalWord48,long originalNext)
    {
        Coverage=coverage;this.snapshots=snapshots;this.tree=tree;CharacterTreeRootIdentity=root;this.links=links;
        characters=states;this.writes=writes;WordMetadataFlags=wordMetadata;
        wordBefore=originalWordFlags;wordSelection=originalWordSelection;word48=originalWord48;next=originalNext;
    }
    public static LegacyFreshMainGlobalDocumentSelectedSources Prepare(LegacyFreshMainGlobalDocumentCoverage actual,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actual);actual.Validate(token);var parent=actual.Publication;
        if(parent.SelectedWordIndex!=0||parent.AlternativeCount!=1||parent.WordSource.Field40!=-1||parent.Scope.SharedState.Selections.Failed||
            !ReferenceEquals(parent.Scope.Seed,parent.Scope.Context.Result.PublicationSeed)||
            !ReferenceEquals(parent.Scope.Seed.PendingSelections,parent.Scope.SharedState.Selections))
            throw new NotSupportedException("Actual current global selected WORD and shared pending Seed required.");
        uint originalWordFlags=parent.WordSource.Flags;long originalWordSelection=parent.WordSource.SelectionId;
        byte[] originalWord48=parent.WordSource.CopyChoiceScalar48();long originalNext=parent.Scope.SharedState.Selections.NextSelectionId;
        var links=parent.WordSource.CopyCharacterLinks().ToArray();var payloads=new Dictionary<int,LegacyFreshMainGlobalTextSourcePayload>();
        foreach(var link in links)
        {
            token.ThrowIfCancellationRequested();var source=link.Source;var node=source.Node;
            if(!source.IsComplete||source.Flags is null||source.SelectionId is null||
                !ReferenceEquals(source.Owner,parent.Wrapper)||!ReferenceEquals(node.Owner,parent.Wrapper.CharacterInterner)||
                !ReferenceEquals(node.FirstGlobalSource,source.Source))throw new ArgumentException("Complete typed same-wrapper global CHAR payload required.");
            if(payloads.TryGetValue(node.Identity,out var previous)&&!ReferenceEquals(previous,source))
                throw new ArgumentException("Actual CHAR node has conflicting typed payload identities.");
            payloads[node.Identity]=source;
        }
        var interner=parent.Wrapper.CharacterInterner;var nodes=interner.CopyInsertionOrder();var tree=interner.CopyTree();int root=interner.RootIdentity;
        if(nodes.Count is<1 or>4096||nodes.Count!=payloads.Count||tree.Length!=nodes.Count||!ReferenceEquals(interner.Document,parent.Scope.Document)||
            nodes.Any(n=>!payloads.TryGetValue(n.Identity,out var p)||!ReferenceEquals(p.Node,n)))
            throw new NotSupportedException("Every complete actual CHAR AVL node must retain its own typed global payload.");
        long bytes=0;foreach(var source in payloads.Values)
        {
            token.ThrowIfCancellationRequested();bytes+=(long)source.Node.RangeCount*16+(long)(source.ReuseKernel?.RecordCount??0)*40;
            if(bytes>4_194_304)throw new NotSupportedException("Whole selected-source scalar/key snapshot budget exceeded.");
        }
        var order=Traverse(tree,root,token);var captured=order.Select(id=>new Snapshot(payloads[id],token)).ToArray();
        var metadata=new uint[order.Length];var flags=new uint[order.Length];var indices=new int[order.Length];
        LegacyGlobalRawObjectKinds? rawKinds=null;
        for(int i=0;i<order.Length;i++)
        {
            token.ThrowIfCancellationRequested();var s=captured[i];
            metadata[i]=flags[i]=LegacyGlobalRawObjectKinds.ComputeMetadata(actual,s.Source,s.Scalars,s.Key,ref rawKinds,token);
            indices[i]=unchecked((int)(s.Scalars.Field40!.Value>>32));
        }
        var ordinal=order.Select((id,i)=>(id,i)).ToDictionary(x=>x.id,x=>x.i);
        uint wordMetadata=ComputeWordMetadata(originalWordFlags,links.Select(l=>metadata[ordinal[l.Source.Node.Identity]]).ToArray());
        var writes=links.Select(l=>new LegacyMainDocumentCharacterSelectionWrite(ordinal[l.Source.Node.Identity],l.Source.Node.Identity,l.Marker)).ToArray();
        token.ThrowIfCancellationRequested();
        //The only reused numerical method: raw native link-order writes,
        //separate immutable output arrays; no configured owner is constructed.
        LegacyFreshMainDocumentSelectedSources.ApplySelectedCharacters(flags,indices,writes.Select(w=>w.TreeOrdinal).ToArray(),writes.Select(w=>w.SelectedMarker).ToArray());
        var states=order.Select((id,i)=>new LegacyGlobalDocumentCharacterState(payloads[id],metadata[i],flags[i],indices[i])).ToArray();
        rawKinds?.Validate(token);
        actual.Validate(token);if(interner.RootIdentity!=root||!interner.CopyTree().AsSpan().SequenceEqual(tree))throw new InvalidOperationException("Actual CHAR AVL changed during overlay preparation.");
        var result=new LegacyFreshMainGlobalDocumentSelectedSources(actual,captured,tree,root,links,states,writes,wordMetadata,
            originalWordFlags,originalWordSelection,originalWord48,originalNext);result.Validate(token);return result;
    }
    //Internal scalar diagnostics exercise lazy branches without manufacturing
    //runtime source owners or accepting caller flags in the production API.
    internal static void ValidateMetadataBranch(uint flags,long selection,int count,ReadOnlySpan<byte> key)
    {
        if((flags&0x10600000)!=0)return;
        if((flags&0x20000000)!=0)
        {if(selection!=-1)throw new NotSupportedException("Actually used prior source V128 owner is unavailable.");return;}
        if(count!=1)return;
        if(key.Length!=16)throw new ArgumentException("One full actual packed source key required.");
        bool lookup=BinaryPrimitives.ReadUInt32LittleEndian(key[4..])==BinaryPrimitives.ReadUInt32LittleEndian(key[12..])&&
            BinaryPrimitives.ReadUInt16LittleEndian(key[2..])==0&&key[1]==0&&BinaryPrimitives.ReadUInt16LittleEndian(key[10..])==3&&key[9]==255;
        if(lookup)throw new NotSupportedException("Native01FD40 actually reaches C25 raw-object lookup; concrete kind is not inferred.");
    }
    internal static uint ComputeWordMetadata(uint original,ReadOnlySpan<uint> selectedLinkMetadata)
    {
        bool all=true;foreach(uint flags in selectedLinkMetadata)if((flags&0x11004000)==0){all=false;break;}
        return(original&0xfeffffffu)|(all?0x01000000u:0);
    }
    private static int[] Traverse(LegacyMainTextSourceTreeSnapshot[] tree,int root,CancellationToken token)
    {
        var shape=tree.ToDictionary(n=>n.Identity);var stack=new Stack<int>();var result=new List<int>();var entered=new HashSet<int>();int cursor=root;
        while(cursor!=0||stack.Count!=0)
        {
            while(cursor!=0)
            {
                token.ThrowIfCancellationRequested();if(stack.Count==64||!shape.TryGetValue(cursor,out var node)||!entered.Add(cursor))
                    throw new NotSupportedException("Native64-entry complete acyclic source traversal required.");
                stack.Push(cursor);cursor=node.Left;
            }
            cursor=stack.Pop();result.Add(cursor);cursor=shape[cursor].Right;
        }
        if(result.Count!=tree.Length)throw new InvalidDataException("Actual CHAR AVL contains unreachable nodes.");return result.ToArray();
    }
    public void Validate(CancellationToken token=default)
    {
        Coverage.Validate(token);var interner=Publication.Wrapper.CharacterInterner;var word=Publication.WordSource;
        if(Publication.Scope.SharedState.Selections.Failed||interner.RootIdentity!=CharacterTreeRootIdentity||!interner.CopyTree().AsSpan().SequenceEqual(tree)||
            !word.CopyCharacterLinks().SequenceEqual(links)||word.Flags!=wordBefore||word.SelectionId!=wordSelection||
            !word.CopyChoiceScalar48().AsSpan().SequenceEqual(word48)||Publication.Scope.SharedState.Selections.NextSelectionId!=next)
            throw new InvalidOperationException("Actual global source/tree/pending prefix changed.");
        foreach(var snapshot in snapshots)snapshot.Validate(token);
    }
}
