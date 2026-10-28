using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

[assembly: InternalsVisibleTo("LegacyMathFreshPublisher.Probe")]

namespace InkRecognition.Math;

//Trusted view used only while the actual field gate remains held. These are
//actual selected payloads, never a public caller source/counter constructor.
internal sealed record Math2Mode0PublicationInput(LegacyOwnedRawInkDocument Document,
    LegacyLibraryOriginalDomNameOwner Names, IReadOnlyList<Math2Mode0MathFieldNode> FieldNodes,
    int FieldIdentity, long InputId, long Next, LegacyNativeInkPositionRange[] InputRanges,
    object SelectedInk, Func<Math2Mode0RetainedInk> CloneSelectedInk, Action<CancellationToken> Check);
internal enum Math2Mode0PublishStage { Claimed, OuterCopied, ThirdRegistryCloned, FieldCloned,
    CandidateRoot, Sequence, PendingLinked, Symbol, SegmentList, Segment, Candidate,
    Character, Terminal, PayloadCloned, ActionQueued, PatchApplied, HeaderFreed,
    Registered, TagWritten, NewTags, StaleCleaned, Complete }
internal sealed record Math2Mode0PublishNode(int Id, byte Kind, string Name, int Parent,
    int Origin, byte ValueTag, string? Text, long? Number, string? NamespaceUri, string? NamespacePrefix);
internal sealed record Math2Mode0PublishRegistry(long Id, string Name, int Category,
    LegacyNativeInkPositionRange[] Ranges);
internal sealed record Math2Mode0PublishPending(long TemporaryId, string Name, long ResolvedId,
    bool HasSelection, LegacyNativeInkPositionRange[] Ranges);
internal sealed record Math2Mode0PublishPatch(byte Operation, int Ordinal, int CandidateRoot, int PayloadRoot, int Parent, bool Applied);
internal sealed record Math2Mode0PublishSnapshot(bool Complete, bool Failed, bool Released,
    long Reservation, string Phase, long TemporaryNext, long RegistryNext, bool HeaderLive,
    int CleanupCount, bool SharedNames, bool DistinctInks, Math2Mode0PublishNode[] Candidate,
    Math2Mode0PublishNode[] Temporary, Math2Mode0PublishRegistry[] Registry,
    Math2Mode0PublishPending[] Pending, Math2Mode0PublishPatch[] Patch, long[] OldTags, long[] NewTags);

/// <summary>Fresh library candidate -> produced/cloned three-op2 patch -> SAME
/// temporary field's late CB8 registration. No native patch ABI or acceptance.</summary>
internal sealed class Math2Mode0FreshMathPublisher : IDisposable
{
    internal const long DefaultPayloadBudget = 16L * 1024 * 1024;
    private sealed class Claim { internal int Entered; }
    private static readonly ConditionalWeakTable<Math2Mode0SelectedMathFieldCopy, Claim> Claims = new();
    private sealed class Node(int id, byte kind, LegacyLibraryOriginalDomName name, int origin = 0)
    {
        internal readonly int Id = id; internal readonly byte Kind = kind;
        internal readonly LegacyLibraryOriginalDomName Name = name; internal readonly int Origin = origin;
        internal Node? Parent; internal byte Tag; internal string? Text; internal long? Number;
        internal string? Uri, Prefix; internal readonly List<Node> Attributes = [], Children = [];
    }
    private sealed class Arena(LegacyLibraryOriginalDomNameOwner names)
    {
        internal readonly LegacyLibraryOriginalDomNameOwner Names = names;
        internal readonly List<Node> Nodes = [];
        internal Node New(byte kind, string name, int origin = 0)
        { var n = new Node(Nodes.Count + 1, kind, Names.Intern(name), origin); Nodes.Add(n); return n; }
        internal static void Attach(Node parent, Node child)
        { if (child.Parent is not null) throw new InvalidOperationException("Node already attached."); child.Parent = parent; if (child.Kind == 0) parent.Attributes.Add(child); else parent.Children.Add(child); }
        internal Node Clone(Node source)
        {
            var n = New(source.Kind, source.Name.Text, source.Id); n.Tag = source.Tag;
            n.Text = source.Text; n.Number = source.Number; n.Uri = source.Uri; n.Prefix = source.Prefix;
            foreach (var a in source.Attributes) Attach(n, Clone(a));
            foreach (var c in source.Children) Attach(n, Clone(c)); return n;
        }
        internal Math2Mode0PublishNode[] Snapshot() => Nodes.Select(n => new Math2Mode0PublishNode(n.Id,n.Kind,
            n.Name.Text,n.Parent?.Id??0,n.Origin,n.Tag,n.Text,n.Number,n.Uri,n.Prefix)).ToArray();
    }
    private sealed class Pending(long id, LegacyLibraryOriginalDomName name, object? selection)
    { internal long Id=id, Resolved=-1; internal readonly LegacyLibraryOriginalDomName Name=name; internal readonly object? Selection=selection; }
    // Native V28 prepends pending entries. Borrow that order without cloning
    // operands or exposing a caller-populated snapshot as owner authority.
    private sealed class PendingView(List<Pending> entries) : IReadOnlyList<Math2DomCb8Pending>
    {
        public int Count => entries.Count;
        public Math2DomCb8Pending this[int index]
        { get { var p=entries[entries.Count-1-index];return new(p.Id,p.Name,p.Selection); } }
        public IEnumerator<Math2DomCb8Pending> GetEnumerator()
        { for(int i=0;i<Count;i++)yield return this[i]; }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()=>GetEnumerator();
    }
    private sealed class Patch(int ordinal, Node candidate, Node payload, Node parent)
    { internal readonly int Ordinal=ordinal; internal readonly Node Candidate=candidate, Payload=payload, Parent=parent; internal bool Applied; }
    private readonly object gate = new();
    private Math2Mode0RetainedInk? retainedInk;
    private readonly Math2Mode0SelectedMathFieldCopy receipt;
    private readonly Arena candidate, temporary;
    private readonly long reservation;
    private readonly List<Pending> pending = [];
    private readonly List<Patch> patch = [];
    private readonly List<long> oldTags = [], newTags = [];
    private readonly object selectedInk;
    private Math2Mode0OuterSymbolVector? outer;
    private long temporaryNext=2;
    private bool complete, failed, released, headerLive;
    private int cleanupCount;
    private string phase="claimed";
    private Math2Mode0FreshMathPublisher(Math2Mode0SelectedMathFieldCopy receipt, Math2Mode0PublicationInput input, long reservation)
    { this.receipt=receipt;this.reservation=reservation;candidate=new(input.Names);temporary=new(input.Names);selectedInk=input.SelectedInk; }

    internal static Math2Mode0FreshMathPublisher Create(Math2Mode0SelectedMathFieldCopy receipt, CancellationToken token=default)
        => CreateDiagnostic(receipt,DefaultPayloadBudget,null,-1,token);
    internal static Math2Mode0FreshMathPublisher CreateDiagnostic(Math2Mode0SelectedMathFieldCopy receipt,long budget,
        Action<Math2Mode0PublishStage,int,Math2Mode0FreshMathPublisher>? observer,int failSelection=-1,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(receipt);token.ThrowIfCancellationRequested();
        if(budget<0||budget>DefaultPayloadBudget)throw new ArgumentOutOfRangeException(nameof(budget));
        if(failSelection < -1)throw new ArgumentOutOfRangeException(nameof(failSelection));
        //Receipt gate MUST be acquired before the field accessor, never inside it.
        receipt.Validate(receipt.Copy,receipt.Field,token);
        return receipt.Field.WithFreshPublicationInput(receipt,input => receipt.Copy.WithSymbolRowsForOuterCopy((rows,ranges,sourceReservation) =>
        {
            if(rows.Length is <1 or >65535||ranges.Length!=rows.Length)
                throw new NotSupportedException("Whole nonempty actual first-copy vector required; no candidate fallback.");
            long bytes=0,rangeCount=0;
            for(int i=0;i<rows.Length;i++)
            {
                token.ThrowIfCancellationRequested();if(rows[i] is null||rows[i].ByteCount<1||rows[i].ByteCount!=rows[i].Text.ByteCount)
                    throw new NotSupportedException("Whole actual nonempty first-alternative vector required.");
                bytes=checked(bytes+rows[i].ByteCount);rangeCount=checked(rangeCount+ranges[i].LongLength);
                foreach(var r in ranges[i]){LegacyNativeSourceRangeUtf8.ValidatePosition(r.First);LegacyNativeSourceRangeUtf8.ValidatePosition(r.Last);}
            }
            //Conservative complete chain envelope, including owned outer,
            //three DOM copies, names, range strings, pending/registry C22s,
            //cloned payloads and snapshots' retained semantic storage. Not RSS.
            long planned=checked(sourceReservation+8192+rows.LongLength*1536+bytes*32+rangeCount*512+input.InputRanges.LongLength*1024);
            if(planned>budget)throw new NotSupportedException("Complete Math publication envelope exceeds budget before payload copies.");
            input.Check(token);var claim=Claims.GetValue(receipt,_=>new());
            if(Interlocked.CompareExchange(ref claim.Entered,1,0)!=0)throw new InvalidOperationException("Math publication receipt already consumed.");
            var target=new Math2Mode0FreshMathPublisher(receipt,input,planned);
            lock(target.gate)
            {
                try{target.Build(input,observer,failSelection,token);return target;}
                catch{target.failed=true;if(observer is null)target.Dispose();throw;}
            }
        },token),token);
    }
    private void Build(Math2Mode0PublicationInput input, Action<Math2Mode0PublishStage,int,Math2Mode0FreshMathPublisher>? observer,
        int failSelection,CancellationToken token)
    {
        void Observe(Math2Mode0PublishStage stage,int index=-1)
        {phase=stage.ToString();observer?.Invoke(stage,index,this);ObjectDisposedException.ThrowIf(released,this);input.Check(token);}
        Node Element(Arena a,Node parent,string name)
        {var n=a.New(3,name);Arena.Attach(parent,n);return n;}
        void Attribute(Arena a,Node parent,string name,byte type,string? text=null,long? number=null)
        {var n=a.New(0,name);n.Tag=type;n.Text=text;n.Number=number;Arena.Attach(parent,n);}
        static byte StringTag(string text)=>text.Length==0?(byte)3:Encoding.UTF8.GetByteCount(text)<16?(byte)2:(byte)1;
        void StringAttribute(Node n,string name,string value)=>Attribute(candidate,n,name,StringTag(value),value);
        var operations=LegacyOwnedRawInkRangeOperations.Prepare(input.Document);
        string Format(LegacyNativeInkPositionRange[] ranges)
        {var value=LegacyNativeSourceRangeUtf8.Format(ranges,operations,token:token);if(!value.Succeeded)throw new InvalidOperationException("Range serialization failed.");return Encoding.UTF8.GetString(value.Bytes);}
        long RegisterPending(string name,LegacyNativeInkPositionRange[] ranges)
        {
            long id=temporaryNext++;
            var p=new Pending(id,candidate.Names.Intern(name),pending.Count==failSelection?null:retainedInk!.CreateSelection(ranges));pending.Add(p);
            Observe(Math2Mode0PublishStage.PendingLinked,pending.Count-1);
            if(ranges.Length!=0&&p.Selection is null)throw new InvalidOperationException("Prepared C22 clone refusal leaves linked pending prefix.");
            return id;
        }
        Observe(Math2Mode0PublishStage.Claimed);
        outer=Math2Mode0OuterSymbolVector.CreateSingle(receipt.Copy,token);outer.ValidateDocument(input.Document,token);
        var symbols=outer.CopySymbols();Observe(Math2Mode0PublishStage.OuterCopied);
        retainedInk=input.CloneSelectedInk();
        if(ReferenceEquals(retainedInk.Identity,input.SelectedInk)||!ReferenceEquals(retainedInk.Names,input.Names)||retainedInk.SnapshotNext!=input.Next)
            throw new InvalidOperationException("Exact distinct selected Ink clone with preserved names/Next required.");
        Observe(Math2Mode0PublishStage.ThirdRegistryCloned);
        //028070.V88 clones the SELECTED FIELD subtree, not the PAGE scaffold.
        //Retain source node identities and clone attributes before children.
        Node CloneSelected(int identity)
        {
            var source=input.FieldNodes.Single(n=>n.Id==identity);
            var n=temporary.New(source.Kind,source.Name,source.Id);
            n.Tag=source.ValueTag;n.Text=source.StringValue;n.Number=source.NumericValue;n.Uri=source.NamespaceUri;n.Prefix=source.NamespacePrefix;
            foreach(var a in input.FieldNodes.Where(n=>n.Parent==identity&&n.Kind==0))Arena.Attach(n,CloneSelected(a.Id));
            foreach(var c in input.FieldNodes.Where(n=>n.Parent==identity&&n.Kind!=0))Arena.Attach(n,CloneSelected(c.Id));
            return n;
        }
        var tRoot=temporary.New(4,"");var tField=CloneSelected(input.FieldIdentity);Arena.Attach(tRoot,tField);
        var oldRoot=Element(temporary,tField,"mathResult");Observe(Math2Mode0PublishStage.FieldCloned);
        var cRoot=candidate.New(4,"");var result=Element(candidate,cRoot,"mathResult");Observe(Math2Mode0PublishStage.CandidateRoot);
        var sequence=Element(candidate,result,"sequence");Observe(Math2Mode0PublishStage.Sequence);
        for(int i=0;i<symbols.Length;i++)
        {
            var row=symbols[i];var symbol=candidate.New(3,"symbol");
            if(row.Ranges.Length!=0)
            {
                StringAttribute(symbol,"range",Format(row.Ranges));
                long id=RegisterPending("MATH_SYMBOL",row.Ranges);Attribute(candidate,symbol,"tag",4,number:-id);
            }
            var text=candidate.New(1,"");
            //Actual native027E20 uses a first-alternative C string pointer.
            byte[] raw=row.Alternatives.Length==0?[]:row.Alternatives[0].Text.ReadBytes();int nul=Array.IndexOf(raw,(byte)0);
            text.Text=new UTF8Encoding(false,true).GetString(raw,0,nul<0?raw.Length:nul);text.Tag=StringTag(text.Text);
            Arena.Attach(symbol,text);Arena.Attach(sequence,symbol);Observe(Math2Mode0PublishStage.Symbol,i);
        }
        //The admitted raw C49/full-input owner supplies identity-only DAF;
        //the filtered list is genuinely empty, and prior freeze rows are absent.
        string fullRange=Format(input.InputRanges);
        var list=Element(candidate,result,"symbolSegmentList");Observe(Math2Mode0PublishStage.SegmentList);
        var segment=Element(candidate,list,"symbolSegment");StringAttribute(segment,"range",fullRange);Observe(Math2Mode0PublishStage.Segment);
        var choice=Element(candidate,segment,"symbolCandidate");Attribute(candidate,choice,"nrs",10,number:0);Observe(Math2Mode0PublishStage.Candidate);
        var character=Element(candidate,choice,"char");var question=candidate.New(1,"");question.Text="?";question.Tag=2;Arena.Attach(character,question);Observe(Math2Mode0PublishStage.Character);
        var terminal=Element(candidate,result,"mathTerminalNode");StringAttribute(terminal,"name","");StringAttribute(terminal,"ref",fullRange);
        Attribute(candidate,terminal,"selected",7,number:0);long nodeId=RegisterPending("MATH_NODE",input.InputRanges);
        Attribute(candidate,terminal,"tag",4,number:-nodeId);Observe(Math2Mode0PublishStage.Terminal);
        if(result.Children.Count!=3||oldRoot.Children.Count!=0||oldRoot.Attributes.Count!=0)
            throw new NotSupportedException("Fresh default three-insertion root required.");
        headerLive=true;
        //Default BE0 empty-old reduction. Every payload clone belongs to the
        //OLD temporary writer before any action is linked or applied.
        for(int i=0;i<result.Children.Count;i++)
        {
            var child=result.Children[i];var payload=temporary.Clone(child);Observe(Math2Mode0PublishStage.PayloadCloned,i);
            patch.Add(new(i,child,payload,oldRoot));Observe(Math2Mode0PublishStage.ActionQueued,i);
        }
        //Filter real produced actions through the retained owner's callback.
        //This fresh producer has only opcode2; the callback's opcode7 path is
        //not admitted by inventing old/new variants or an old result.
        var pendingView=new PendingView(pending);var tagName=temporary.Names.Intern("tag");
        foreach(var action in patch)
        {
            if(Math2DomCb8EditMatcher.Matches(2,action.Parent.Kind,null,action.Parent.Name,tagName,
                default,default,pendingView,retainedInk!,token:token))
                throw new InvalidOperationException("A produced opcode2 must not be filtered as an opcode7 tag edit.");
            Arena.Attach(action.Parent,action.Payload);action.Applied=true;Observe(Math2Mode0PublishStage.PatchApplied,action.Ordinal);
        }
        headerLive=false;Observe(Math2Mode0PublishStage.HeaderFreed);
        IEnumerable<Node> Walk(Node parent)
        {foreach(var child in parent.Children){yield return child;foreach(var descendant in Walk(child))yield return descendant;}}
        foreach(var node in Walk(oldRoot))foreach(var attribute in node.Attributes)
        {
            if(attribute.Name.Text!="tag")continue;
            if(attribute.Tag!=4||attribute.Number is not long actualTag)throw new NotSupportedException("Actual typed INT64 tag required.");
            long tag=Math2DomValueConversion.RegistrationTag(actualTag);
            if(tag>=0)continue;
            var p=pending.SingleOrDefault(p=>p.Id==-tag)??throw new InvalidOperationException("Missing actual CB8 pending entry.");
            if(p.Resolved<0)
            {
                if(p.Selection is null)throw new InvalidOperationException("Pending C22 construction failed.");
                long id=retainedInk!.Register(p.Name,p.Selection);p.Resolved=id;
                Observe(Math2Mode0PublishStage.Registered,checked((int)p.Id-2));
            }
            attribute.Number=p.Resolved;Observe(Math2Mode0PublishStage.TagWritten,checked((int)p.Id-2));
        }
        foreach(var node in Walk(oldRoot))foreach(var attribute in node.Attributes)
            if(attribute.Name.Text=="tag")newTags.Add(Math2DomValueConversion.CollectedTag(attribute.Number!.Value));
        Observe(Math2Mode0PublishStage.NewTags);
        //Actual fresh old result has zero old tags; native V50 has no IDs to
        //delete. Nonempty-old/imported stale deletion is not admitted here.
        if(oldTags.Count!=0)throw new NotSupportedException("No prior result tags in fresh scope.");
        if(!Math2DomCb8TagCleanup.RemoveStale(CollectionsMarshal.AsSpan(oldTags),CollectionsMarshal.AsSpan(newTags),retainedInk!,token))
            throw new InvalidOperationException("Retained Ink stale-tag cleanup failed.");
        Observe(Math2Mode0PublishStage.StaleCleaned);Cleanup();Observe(Math2Mode0PublishStage.Complete);complete=true;
    }
    internal Math2Mode0PublishSnapshot CopySnapshot()
    {
        lock(gate)return new(complete,failed,released,reservation,phase,temporaryNext,retainedInk?.SnapshotNext??0,headerLive,cleanupCount,
            ReferenceEquals(candidate.Names,temporary.Names),!ReferenceEquals(retainedInk?.Identity,selectedInk),candidate.Snapshot(),temporary.Snapshot(),
            retainedInk?.CopyRegistry()??[],pending.Select(p=>new Math2Mode0PublishPending(p.Id,p.Name.Text,p.Resolved,p.Selection is not null,p.Selection is null?[]:retainedInk!.CopySelectionRanges(p.Selection))).ToArray(),
            patch.Select(p=>new Math2Mode0PublishPatch(2,p.Ordinal,p.Candidate.Id,p.Payload.Id,p.Parent.Id,p.Applied)).ToArray(),oldTags.ToArray(),newTags.ToArray());
    }
    internal Math2Mode0PublishSnapshot CopyResult(CancellationToken token=default)
    {
        receipt.Validate(receipt.Copy,receipt.Field,token);
        lock(gate){ObjectDisposedException.ThrowIf(released,this);if(!complete||failed)throw new InvalidOperationException("Successful live Math temporary publication required.");return CopySnapshot();}
    }
    //Review borrow under the same receipt -> field -> publisher order. This is
    //not an opcode7/edit-production entrypoint and grants no original mutation.
    internal T WithRetainedInkForReview<T>(Func<Math2Mode0RetainedInk,T> review,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(review);
        return WithCompletedTransferInput(_=>review(retainedInk!),token);
    }
    //Trusted outgoing view. Receipt validation is OUTSIDE field/publisher gates;
    //the actual pre-NN binding and copy are rechecked inside the field accessor.
    internal T WithCompletedTransferInput<T>(Func<Math2Mode0OutgoingInput,T> consume,CancellationToken token=default)
    {
        receipt.Validate(receipt.Copy,receipt.Field,token);
        return receipt.Field.WithFreshPublicationInput(receipt,input=>
        {
            lock(gate)
            {
                void Check(CancellationToken cancellation)
                {input.Check(cancellation);ObjectDisposedException.ThrowIf(released,this);if(!complete||failed||headerLive)throw new InvalidOperationException("Completed live temporary publication required.");}
                Check(token);
                IEnumerable<Math2Mode0PublishNode> Nodes()
                {foreach(var n in temporary.Nodes)yield return new(n.Id,n.Kind,n.Name.Text,n.Parent?.Id??0,n.Origin,n.Tag,n.Text,n.Number,n.Uri,n.Prefix);}
                return consume(new(input,receipt.Field.OriginalInkIdentity,retainedInk!.Identity,temporary.Names,
                    temporary.Nodes.Count,Nodes,retainedInk.BorrowRegistry(),retainedInk.SnapshotNext,reservation,Check));
            }
        },token);
    }
    private void Cleanup(){if(cleanupCount!=0)return;cleanupCount=1;headerLive=false;outer?.Dispose();}
    public void Dispose(){lock(gate){if(released)return;released=true;Cleanup();retainedInk?.Dispose();}}
}
