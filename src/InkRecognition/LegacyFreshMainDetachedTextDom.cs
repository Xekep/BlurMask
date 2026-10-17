using System.Runtime.CompilerServices;
using System.Text;

namespace InkRecognition;

/// <summary>One detached031880 subtree, never an applied C20 document.</summary>
public sealed class LegacyOwnedMainDetachedTextDom
{
    public LegacyFreshMainSourceRangeUtf8 Sources {get;}
    /// <summary>Fresh0287B7 qwordB8=0: this BC field is distinct from the
    /// alternative selected by source-coverage postprocessing.</summary>
    public int NativeTextSelectedBC=>0;
    public uint NativeTextFreezeB8=>0;
    public LegacyLibraryTextDomArena Arena {get;}
    public bool Succeeded {get;}
    public LegacyLibraryTextDomTarget? Target {get;}
    internal long TargetEpoch {get;}
    internal LegacyDomNode? LiveRoot {get;}
    internal LegacyDomNode? PreviousTargetNode {get;}
    internal LegacyDomNode? QueuedPreviousPayload {get;}
    internal LegacyLibraryTextDomNode? BeforeTargetRoot {get;}
    private readonly LegacyLibraryTextDomNode? root;
    public LegacyLibraryTextDomNode Root=>root??throw new InvalidOperationException("The detached subtree did not complete.");
    internal LegacyOwnedMainDetachedTextDom(LegacyFreshMainSourceRangeUtf8 sources,LegacyLibraryTextDomArena arena,LegacyDomNode? node,LegacyLibraryTextDomTarget? target=null,long epoch=0,
        LegacyLibraryTextDomNode? before=null,LegacyDomNode? previous=null,LegacyDomNode? queuedPrevious=null)
    {Sources=sources;Arena=arena;Target=target;TargetEpoch=epoch;LiveRoot=node;BeforeTargetRoot=before;PreviousTargetNode=previous;QueuedPreviousPayload=queuedPrevious;Succeeded=node is not null;root=node is null?null:arena.Snapshot(node);}
}

/// <summary>Bounded ordinary fresh2DA.V30 builder over real retained source
/// payloads. Uses a new library-owned parameterized BCC no-history arena,
/// without a fabricated parent node, SDK page or BE2 transaction.</summary>
public sealed class LegacyFreshMainDetachedTextDom
{
    private sealed class Lease {internal bool Claimed;}
    private static readonly ConditionalWeakTable<LegacyFreshMainSourceRangeUtf8,Lease> lifetime=new();
    private readonly object gate=new();
    private bool entered,faulted;
    public LegacyFreshMainSourceRangeUtf8 Sources {get;}
    public LegacyLibraryTextDomArena? Arena {get;private set;}
    public bool IsFaulted=>Volatile.Read(ref faulted);
    internal int ScratchCapacity {get;private set;}
    internal byte[] ScratchBytes {get;private set;}=[];
    private readonly Dictionary<LegacyOwnedMainTextSourcePayload,LegacyOwnedMainSourceRangeUtf8> characters;
    private readonly LegacyOwnedMainSourceRangeUtf8 word,text;
    private readonly LegacyLibraryTextDomTarget? target;
    private readonly long targetEpoch;
    private LegacyFreshMainDetachedTextDom(LegacyFreshMainSourceRangeUtf8 source,LegacyLibraryTextDomTarget? target=null,long epoch=0)
    {
        this.target=target;targetEpoch=epoch;
        Sources=source;characters=source.Sources.Where(s=>s.CharacterSource is not null).ToDictionary(s=>s.CharacterSource!);
        word=source.Sources.Single(s=>s.WordSource is not null);text=source.Sources.Single(s=>s.TextSource is not null);
    }
    public static LegacyFreshMainDetachedTextDom Prepare(LegacyFreshMainSourceRangeUtf8 actualSources,CancellationToken token=default)
        =>PrepareCore(actualSources,null,token);
    public static LegacyFreshMainDetachedTextDom PrepareForTarget(LegacyFreshMainSourceRangeUtf8 actualSources,LegacyLibraryTextDomTarget actualTarget,CancellationToken token=default)
    {ArgumentNullException.ThrowIfNull(actualTarget);return PrepareCore(actualSources,actualTarget,token);}
    private static LegacyFreshMainDetachedTextDom PrepareCore(LegacyFreshMainSourceRangeUtf8 actualSources,LegacyLibraryTextDomTarget? actualTarget,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(actualSources);Validate(actualSources,token);
        long epoch=actualTarget?.CaptureEpoch(actualSources.Seed,token)??0;
        var owner=new LegacyFreshMainDetachedTextDom(actualSources,actualTarget,epoch);var lease=lifetime.GetValue(actualSources,_=>new());
        lock(lease){token.ThrowIfCancellationRequested();if(lease.Claimed)throw new InvalidOperationException("This exact source-formatting invocation already owns a detached DOM arena.");lease.Claimed=true;}
        return owner;
    }
    private static void Validate(LegacyFreshMainSourceRangeUtf8 source,CancellationToken token)
    {
        LegacyFreshMainDocumentCounterHandoff.Validate(source.CounterHandoff.Transforms,token);
        var state=source.CounterHandoff.Transforms.Cleanup.State;var words=state.Text.WordChoices;
        if(!ReferenceEquals(words.Owner.DocumentSeed,source.Seed)||!ReferenceEquals(words.Owner.Document,source.Document)||
            words.Alternatives.Count!=2||state.CharacterCoverage.SourceCoverage.NativeSelectedAlternativeIndex!=0||
            state.CharacterCoverage.SourceCoverage.LineConsistency.LineCount!=1||
            source.CounterHandoff.Transforms.TransformCount110!=0||!ReferenceEquals(source.Sources.Single(s=>s.WordSource is not null).WordSource,words.Source))
            throw new NotSupportedException("Actual fresh shared-WORD/source-line/empty-transform owner required.");
        foreach(var s in source.Sources)
        {
            token.ThrowIfCancellationRequested();if(!ReferenceEquals(s.Owner,source))throw new ArgumentException("Foreign formatted source.");
            if(s.CharacterSource is{} character)
            {
                if(character.PriorRangeCount!=0||character.Field1C!=0||unchecked((int)character.Field40)!=-1)
                    throw new NotSupportedException("Fresh ordinary CHAR metadata only.");
                foreach(var row in character.CopyRecords())
                {ValidateOrdinaryFlags(row.Flags);
                 if(row.Text is null)throw new NotSupportedException("Actual ordinary CHAR text owner required.");_=row.Text.ReadBytes();}
            }
        }
        var characterOwners=source.Sources.Where(s=>s.CharacterSource is not null).Select(s=>s.CharacterSource!).ToHashSet();
        foreach(var row in words.Source.Choices)foreach(var link in row.CharacterLinks)
            if(!characterOwners.Contains(link.Source)||!ReferenceEquals(link.Source.SourceNode.Owner.Document,source.Document))
                throw new ArgumentException("Every WORD link must retain its exact CHAR payload in this source/document owner.");
    }
    internal static void ValidateOrdinaryFlags(uint flags)
    {if((flags&0xc00)!=0)throw new NotSupportedException("External400/math800 CHAR DOM branches are not supported.");}
    public LegacyOwnedMainDetachedTextDom Build(CancellationToken token=default)=>BuildCore(-1,-1,token);
    //Prepared fault outcomes exercise fallible native services, not OS-page
    //allocator addresses or a public success/acceptance flag.
    internal LegacyOwnedMainDetachedTextDom BuildPrepared(int failAt=-1,int cancelAt=-1,CancellationToken token=default)
        =>BuildCore(failAt,cancelAt,token);
    private LegacyOwnedMainDetachedTextDom BuildCore(int failAt,int cancelAt,CancellationToken token)
    {
        lock(gate)
        {
            token.ThrowIfCancellationRequested();if(entered)throw new InvalidOperationException("Detached DOM construction is single-use, including failed prefixes.");
            Validate(Sources,token);target?.ValidateEpoch(targetEpoch,token);entered=true;
            var arena=target is null?new LegacyLibraryTextDomArena(Sources.Seed,failAt,cancelAt):new LegacyLibraryTextDomArena(Sources.Seed,target.Arena.NamePool,failAt,cancelAt);Arena=arena;
            LegacyDomNode? root=null;
            LegacyLibraryTextDomNode? before=null;LegacyDomNode? previous=null,queuedPrevious=null;
            try
            {
                if(target is not null)
                {
                    var captured=target.CaptureInsertion(targetEpoch,token);before=captured.Before;previous=captured.Previous;
                    //061510 queues removal BEFORE source.V30. Fresh old-reader
                    //is null, so V70 clones its old payload into the new writer.
                    if(previous is not null)
                    {
                        if(!arena.Allow("queue4")){faulted=true;return new(Sources,arena,null,target,targetEpoch,before,previous);}
                        queuedPrevious=arena.CloneFrom(target.Arena,previous,token);
                        if(queuedPrevious is null){faulted=true;return new(Sources,arena,null,target,targetEpoch,before,previous);}
                    }
                }
                root=arena.Element("textResult");if(root is null)return Failed();
                if(!arena.StringAttribute(root,"gen",Sources.Seed.LibraryIdentifier38.CopyUtf8Storage36()))return Failed();
                var chars=arena.Element("charSegmentList");if(chars is null)return Failed();arena.Append(root,chars);
                foreach(var source in Sources.Sources.Where(s=>s.CharacterSource is not null))
                {token.ThrowIfCancellationRequested();var node=Character(source);if(node is null)return Failed();arena.Append(chars,node);}
                var words=arena.Element("wordSegmentList");if(words is null)return Failed();arena.Append(root,words);
                var segment=Word();if(segment is null)return Failed();arena.Append(words,segment);
                var textNode=arena.Element("text");if(textNode is null)return Failed();
                var state=Sources.CounterHandoff.Transforms.Cleanup.State;
                if(!arena.I64(textNode,"tag",state.Text.Selection.SelectionId)||!RangeAttribute(textNode,"range",text)||
                    !arena.I32(textNode,"selected",0))return Failed();
                //Fresh0287B7 clears the full qwordB8: freezeB8=0/selectedBC=0.
                //NoE8/F0 text override and no prior cache exist in this owner.
                foreach(var alternative in state.Text.WordChoices.Alternatives)
                {
                    var candidate=arena.Element("textCandidate");
                    if(candidate is null)throw new NotSupportedException("Native031360 would invoke V90 on a null textCandidate; that prepared failure boundary is unsupported.");
                    var reference=arena.Element("word");if(reference is null){arena.Release(candidate);return Failed();}
                    if(!ReferenceEquals(alternative.Source,word.WordSource))throw new ArgumentException("Foreign WORD reference.");
                    if(!RangeAttribute(reference,"ref",word)||!arena.I32(reference,"selected",alternative.SelectedWordIndex))
                    {arena.Release(reference);arena.Release(candidate);return Failed();}
                    arena.Append(candidate,reference);arena.Append(textNode,candidate);
                }
                arena.Append(root,textNode);
                var lines=arena.Element("lines");if(lines is null)return Failed();
                foreach(var line in state.CharacterCoverage.SourceCoverage.LineConsistency.Lines)
                {
                    var node=arena.Element("line");if(node is null)return Failed();
                    bool ok=true;
                    if(Math.Abs(line.OriginX)>=1.1920928955078125e-7&&Math.Abs(line.Angle)>=1.1920928955078125e-7)ok=arena.F64(node,"x",line.OriginX);
                    ok=ok&&arena.F64(node,"y",line.OriginY)&&arena.F64(node,"a",line.Angle)&&arena.F64(node,"h",line.XHeight);
                    if(line.OwnerTag>=0)ok=ok&&arena.I64(node,"tagId",line.OwnerTag);
                    if(!ok){arena.Release(node);return Failed();}arena.Append(lines,node);
                }
                arena.Append(root,lines);
                //Queue failure occurs AFTER031880 returned its subtree. The
                //native caller does not run031880's normal-false root cleanup.
                if(target is not null&&!arena.Allow("queue2")){faulted=true;return new(Sources,arena,null,target,targetEpoch,before,previous,queuedPrevious);}
                return new(Sources,arena,root,target,targetEpoch,before,previous,queuedPrevious);
            }
            catch{faulted=true;throw;}
            LegacyOwnedMainDetachedTextDom Failed()
            {
                faulted=true;if(root is not null){arena.ClearChildren(root);arena.Release(root);}return new(Sources,arena,null,target,targetEpoch);
            }
            bool RangeAttribute(LegacyDomNode node,string name,LegacyOwnedMainSourceRangeUtf8 source)
            {
                token.ThrowIfCancellationRequested();if(!ReferenceEquals(source.Owner,Sources))throw new ArgumentException("Foreign range owner.");
                var formatted=LegacyNativeSourceRangeUtf8.FormatIntoScratch(source.CopyOriginalRanges(),LegacyOwnedRawInkRangeOperations.Prepare(Sources.Document),
                    ScratchCapacity,arena.Allow,token);ScratchCapacity=formatted.Capacity;ScratchBytes=formatted.Bytes;
                arena.Record("range:"+source.Kind+":"+source.SourceNodeIdentity+":"+ScratchCapacity+":"+Convert.ToBase64String(ScratchBytes));
                if(!formatted.Succeeded)return false;
                if(name!="range"||source.Kind!="TEXT")ScratchBytes=LegacyNativeSourceRangeUtf8.CallerFirstBrace(ScratchBytes,source.SourceFlags);
                return arena.StringAttribute(node,name,ScratchBytes);
            }
            LegacyDomNode? Character(LegacyOwnedMainSourceRangeUtf8 source)
            {
                var payload=source.CharacterSource!;var node=arena.Element("charSegment");if(node is null)return null;
                bool ok=RangeAttribute(node,"range",source);
                if(payload.SelectionId!=-1)ok=ok&&arena.I64(node,"tag",payload.SelectionId);
                if(payload.SourceLineId!=-1)ok=ok&&arena.I32(node,"line",payload.SourceLineId);
                foreach(var row in payload.CopyRecords())
                {
                    if(!ok)break;var candidate=arena.Element("charCandidate");
                    if(candidate is null){ok=false;break;}
                    var child=arena.Text(row.Text!.ReadBytes());
                    if(child is null){arena.Release(candidate);ok=false;break;}
                    arena.Append(candidate,child);arena.Append(node,candidate);
                }
                if(ok)return node;arena.Release(node);return null;
            }
            LegacyDomNode? Word()
            {
                var node=arena.Element("wordSegment");if(node is null)return null;
                bool ok=RangeAttribute(node,"range",word);
                if(word.WordSource!.SelectionId!=-1)ok=ok&&arena.I64(node,"tag",word.WordSource.SelectionId);
                foreach(var row in word.WordSource.Choices)
                {
                    if(!ok)break;var candidate=arena.Element("wordCandidate");if(candidate is null){ok=false;break;}
                    foreach(var link in row.CharacterLinks)
                    {
                        var reference=arena.Element("char");
                        if(!characters.TryGetValue(link.Source,out var source))throw new ArgumentException("Foreign WORD-to-CHAR payload link.");
                        if(reference is null||!RangeAttribute(reference,"ref",source)||!arena.I32(reference,"selected",link.SelectedMarker))
                        {ok=false;break;}
                        arena.Append(candidate,reference);
                    }
                    //030CC0 does not explicitly release an unattached failed
                    //CHAR-ref; its arena lifetime is retained independently.
                    if(!ok){arena.Release(candidate);break;}arena.Append(node,candidate);
                }
                if(ok)return node;arena.Release(node);return null;
            }
        }
    }
}
