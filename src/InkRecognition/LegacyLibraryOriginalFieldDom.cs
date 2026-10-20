using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Text;

[assembly:InternalsVisibleTo("LegacyOriginalFieldDom.Probe")]
namespace InkRecognition;

/// <summary>Pre-NN original FIELD DOM owner with a named active overlay.
/// This library graph is not a CBB receiver, registered revision or native BE2 history.</summary>
public sealed class LegacyLibraryOriginalFieldDom:IDisposable
{
    private readonly object gate=new();
    private readonly LegacyOriginalFieldDomKernel kernel;
    private LegacyLibraryOriginalFieldDomPublication? publication;
    private bool disposed;
    public LegacyLibraryOriginalSourceHistory History {get;}
    public LegacyLibraryOriginalFieldSelection Input {get;}
    public LegacyLibraryOriginalDomNameOwner NameOwner=>History.Original.NameOwner;
    public string SourceName {get;}
    public LegacyLibraryOriginalFieldDomNode HistoricalPage {get;}
    public LegacyLibraryOriginalFieldDomNode ActivePage {get{lock(gate)return kernel.Snapshot(kernel.Root);}}
    public LegacyLibraryOriginalFieldDomPublication? Publication {get{lock(gate)return publication;}}
    internal LegacyLibraryOriginalFieldDom(LegacyLibraryOriginalSourceHistory history,LegacyLibraryOriginalFieldSelection input,CancellationToken token)
    {
        History=history;Input=input;kernel=new(history.HistoricalOriginal,input,token);
        SourceName="./page/content/textField[@id=\""+input.FieldId+"\"]";
        HistoricalPage=kernel.Snapshot(kernel.Root);
    }
    public LegacyLibraryOriginalFieldDomPublication PreparePublication(LegacyLibraryTextPublicationDocument actualDocument,
        LegacyLibraryOriginalSourceApplication actualSourceApplication,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualDocument);ArgumentNullException.ThrowIfNull(actualSourceApplication);
        return actualDocument.ClaimOriginalFieldDom(this,actualSourceApplication,token);
    }
    internal LegacyLibraryOriginalFieldDomPublication ClaimPublication(LegacyLibraryTextPublicationDocument document,
        LegacyLibraryOriginalSourceApplication app,CancellationToken token)
    {
        lock(gate)
        {
            Validate(token);
            if(publication is not null||!ReferenceEquals(app.Owner,History)||!ReferenceEquals(app.Compatibility.OriginalBootstrapInput,Input)||
                !ReferenceEquals(History.FieldDom,this)||!ReferenceEquals(app.DocumentOwner,document))
                throw new ArgumentException("One exact pre-NN chosen original field/history/source/document required.");
            var value=new LegacyLibraryOriginalFieldDomPublication(this,document,app,kernel,token);publication=value;return value;
        }
    }
    internal T WithPublication<T>(LegacyLibraryOriginalFieldDomPublication actual,Func<T> action,CancellationToken token)
    {lock(gate){Validate(token);if(!ReferenceEquals(publication,actual))throw new ArgumentException("Exact retained FIELD publication required.");return action();}}
    internal void ValidateBeforeSourceRun(LegacyLibraryOriginalSourceApplication app,CancellationToken token)
    {
        lock(gate)
        {
            Validate(token);
            if(publication is null||!ReferenceEquals(publication.SourceApplication,app)||!publication.State.IsReady)
                throw new InvalidOperationException("Claimed original whole-FIELD preflight must succeed before original source execution.");
        }
    }
    internal void ValidateSourceRollback(CancellationToken token)
    {lock(gate){Validate(token);if(publication?.State.ApplyEntered==true)throw new InvalidOperationException("Source-only inverse cannot remove tags after original FIELD DOM application entered.");}}
    private void Validate(CancellationToken token){ObjectDisposedException.ThrowIf(disposed,this);token.ThrowIfCancellationRequested();}
    public void Dispose(){lock(gate)disposed=true;}
}

/// <summary>NEW result-derived field-relative edit, never the existing
/// ordinal0 kind4 export. Preflight, source execution and live apply are separate.</summary>
public sealed class LegacyLibraryOriginalFieldDomPublication
{
    private readonly LegacyOriginalFieldDomKernel kernel;
    private readonly LegacyLibraryTextDomNode payload;
    private LegacyOriginalFieldDomKernel.Node? scratch;
    private bool preflightEntered,applyEntered;
    private long epoch;
    private LegacyLibraryOriginalFieldDomPublicationState state;
    public LegacyLibraryOriginalFieldDom Owner {get;}
    public LegacyLibraryTextPublicationDocument DocumentOwner {get;}
    public LegacyLibraryOriginalSourceApplication SourceApplication {get;}
    public uint ChildOrdinal {get;}
    public uint Operation=>2;
    public IReadOnlyList<uint> Path {get;}=Array.AsReadOnly(Array.Empty<uint>());
    public LegacyLibraryOriginalFieldDomPublicationState State=>Volatile.Read(ref state);
    internal LegacyLibraryOriginalFieldDomPublication(LegacyLibraryOriginalFieldDom owner,LegacyLibraryTextPublicationDocument document,
        LegacyLibraryOriginalSourceApplication app,LegacyOriginalFieldDomKernel kernel,CancellationToken token)
    {
        Owner=owner;DocumentOwner=document;SourceApplication=app;this.kernel=kernel;
        payload=document.ActualDom.Root??throw new InvalidOperationException("Genuine completed result writer required.");
        var size=LegacyOriginalFieldDomKernel.MeasurePayload(payload,token);
        //Complete aggregate bounds BEFORE any scratch/payload copying. Both
        //scratch and live payload copies are retained by this bounded lifetime.
        long originalBytes=kernel.Nodes.Sum(n=>(long)n.Value.Length+Encoding.UTF8.GetByteCount(n.Name?.Text??""));
        int fieldCount=0;long fieldBytes=0;
        void Count(LegacyOriginalFieldDomKernel.Node n,int depth)
        {if(depth>LegacyOriginalFieldDomKernel.MaxDepth)throw new NotSupportedException("Original FIELD depth exceeded.");
         fieldCount++;fieldBytes+=n.Value.Length+Encoding.UTF8.GetByteCount(n.Name?.Text??"");foreach(var a in n.Attributes)Count(a,depth+1);foreach(var c in n.Children)Count(c,depth+1);}
        Count(kernel.Field,0);
        int fieldDepth=0;
        for(var parent=kernel.Field.Parent;parent is not null;parent=parent.Parent)
        {
            token.ThrowIfCancellationRequested();
            if(++fieldDepth>LegacyOriginalFieldDomKernel.MaxDepth)throw new NotSupportedException("Original FIELD parent depth exceeded.");
        }
        if(kernel.Nodes.Count+fieldCount+2L*size.Nodes>LegacyOriginalFieldDomKernel.MaxNodes||
            originalBytes+fieldBytes+2*size.Bytes>LegacyOriginalFieldDomKernel.MaxValueBytes||fieldDepth+1+size.Depth>LegacyOriginalFieldDomKernel.MaxDepth)
            throw new NotSupportedException("Whole FIELD/scratch/live aggregate allocation budget exceeded.");
        //061510: actual child scan, no prior result -> count, not literal0.
        if(kernel.Field.Children.Any(c=>c.Name?.Text=="textResult"))throw new NotSupportedException("Prior original result requires its actual removal/history branch.");
        ChildOrdinal=checked((uint)kernel.Field.Children.Count);
        if(ChildOrdinal!=1)throw new NotSupportedException("Fresh chosen field must retain its one actual INPUT.");
        state=Capture(false,true,false,false,[]);
    }
    public byte[] CopyEditScalarStorage40()
    {byte[] b=new byte[40];BinaryPrimitives.WriteUInt32LittleEndian(b,Operation);BinaryPrimitives.WriteUInt32LittleEndian(b.AsSpan(24),ChildOrdinal);return b;}
    public LegacyLibraryOriginalFieldDomPublicationState Preflight(CancellationToken token=default)=>PreflightCore(-1,-1,token);
    internal LegacyLibraryOriginalFieldDomPublicationState PreflightPrepared(int fail=-1,int cancel=-1,CancellationToken token=default)
        =>PreflightCore(fail,cancel,token);
    private LegacyLibraryOriginalFieldDomPublicationState PreflightCore(int fail,int cancel,CancellationToken token)
        =>DocumentOwner.WithOriginalFieldDom(this,()=>Owner.WithPublication(this,()=>
        {
            if(preflightEntered||applyEntered||SourceApplication.State.HasEntered)throw new InvalidOperationException("One whole-FIELD preflight before actual original source entry required.");
            token.ThrowIfCancellationRequested();kernel.Begin(fail,cancel);preflightEntered=true;
            try
            {
                scratch=kernel.CloneField(token);kernel.Step("map-all-parents",token);
                var copy=kernel.ClonePayload(payload,token);kernel.Append(scratch,copy,checked((int)ChildOrdinal),token);
                return Set(true,true,false,false,[]);
            }
            catch(LegacyOriginalFieldDomKernel.DomServiceRefusal){return Set(false,true,true,false,[]);}
            catch{Set(false,true,true,false,[]);throw;}
        },token),token);
    public LegacyLibraryOriginalFieldDomPublicationState Apply(CancellationToken token=default)=>ApplyCore(-1,-1,false,token);
    internal LegacyLibraryOriginalFieldDomPublicationState ApplyPrepared(int fail=-1,int cancel=-1,bool failPostcheck=false,CancellationToken token=default)
        =>ApplyCore(fail,cancel,failPostcheck,token);
    private LegacyLibraryOriginalFieldDomPublicationState ApplyCore(int fail,int cancel,bool failPostcheck,CancellationToken token)
        =>DocumentOwner.WithOriginalFieldDom(this,()=>Owner.WithPublication(this,()=>
        {
            if(!state.IsReady||applyEntered||epoch!=0||!SourceApplication.State.IsSafeForLibraryApply)
                throw new InvalidOperationException("Successful whole-field preflight and exact complete original source execution required.");
            var registry=SourceApplication.State.Registry.ToDictionary(e=>e.RegistryId);
            var pairs=ValidateIdentityMap(Owner,SourceApplication,registry,token);
            token.ThrowIfCancellationRequested();kernel.Begin(fail,cancel);applyEntered=true;
            try
            {
                kernel.Step("map-all-parents",token);var copy=kernel.ClonePayload(payload,token);
                kernel.Append(kernel.Field,copy,checked((int)ChildOrdinal),token);
                kernel.Step("postcheck",token);
                ValidateTags(kernel.Field,registry,Owner.Input.CopyRanges(),token);
                if(failPostcheck)throw new InvalidOperationException("Prepared post-application containment refusal.");
                epoch++;return Set(true,true,false,true,pairs.ToArray());
            }
            catch(LegacyOriginalFieldDomKernel.DomServiceRefusal){return Set(true,true,true,false,pairs.ToArray());}
            catch{Set(true,true,true,false,pairs.ToArray());throw;}
        },token),token);
    internal static LegacyLibraryOriginalFieldDomIdentityPair[] ValidateIdentityMap(LegacyLibraryOriginalFieldDom owner,
        LegacyLibraryOriginalSourceApplication app,IReadOnlyDictionary<long,LegacyLibraryOriginalSourceRegistryEntry> registry,
        CancellationToken token=default)
    {
            token.ThrowIfCancellationRequested();
            var pending=app.TransactionOwner.CopyPendingSelections();
            if(registry.Count!=owner.History.HistoricalOriginal.Inputs.Count+pending.Count)
                throw new InvalidOperationException("Complete source-produced identity map required.");
            foreach(var original in owner.History.HistoricalOriginal.Inputs)
            {
                token.ThrowIfCancellationRequested();
                if(!registry.TryGetValue(original.RegistryId,out var e)||e.RegistryId!=original.RegistryId||!ReferenceEquals(e.Owner,owner.History)||e.SourceCommand is not null||
                    !ReferenceEquals(e.RangesOwner,original.RetainedSelection)||e.Name!="INPUT"||e.Category!=100)
                    throw new InvalidOperationException("Original INPUT identity changed.");
            }
            var pairs=new List<LegacyLibraryOriginalFieldDomIdentityPair>();
            foreach(var source in pending)
            {
                token.ThrowIfCancellationRequested();
                if(!registry.TryGetValue(source.SelectionId,out var e)||e.RegistryId!=source.SelectionId||!ReferenceEquals(e.SourceCommand,source)||
                    !ReferenceEquals(e.Owner,owner.History)||e.Name!=source.Name||e.Category!=source.Category||
                    !e.CopyRanges().AsSpan().SequenceEqual(source.CopyRanges()))
                    throw new InvalidOperationException("Source requested/actual ID, command or original-domain ranges changed.");
                pairs.Add(new(source.SelectionId,e.RegistryId));
            }
            return pairs.ToArray();
    }
    internal static void ValidateTags(LegacyOriginalFieldDomKernel.Node n,
        IReadOnlyDictionary<long,LegacyLibraryOriginalSourceRegistryEntry> registry,LegacyNativeInkPositionRange[] domain,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(n.Kind==3)
        {
            long tag=LegacyOriginalFieldDomKernel.ReadTag(n);
            if(tag>=0)
            {
                if(!registry.TryGetValue(tag,out var e))throw new InvalidOperationException("Inserted tag lacks an actual original source entry.");
                foreach(var r in e.CopyRanges())if(!LegacyOwnedRawInkRangeOperations.Contains(domain,r))
                    throw new InvalidOperationException("Inserted tag escapes original INPUT C22.");
            }
        }
        foreach(var c in n.Children)if(c.Kind==3)ValidateTags(c,registry,domain,token);
    }
    private LegacyLibraryOriginalFieldDomPublicationState Set(bool worked,bool compatible,bool faulted,bool success,
        LegacyLibraryOriginalFieldDomIdentityPair[] pairs)
    {var value=Capture(worked,compatible,faulted,success,pairs);Volatile.Write(ref state,value);return value;}
    private LegacyLibraryOriginalFieldDomPublicationState Capture(bool worked,bool compatible,bool faulted,bool success,
        LegacyLibraryOriginalFieldDomIdentityPair[] pairs)=>new(preflightEntered,worked,compatible,faulted,applyEntered,success,epoch,
            kernel.Attempts,kernel.Nodes.Count,kernel.Snapshot(kernel.Root),scratch is null?null:kernel.Snapshot(scratch),kernel.Trace.ToArray(),pairs);
}

public sealed record LegacyLibraryOriginalFieldDomIdentityPair(long RequestedId,long ActualId);
public sealed class LegacyLibraryOriginalFieldDomPublicationState
{
    public bool PreflightEntered {get;}
    public bool Worked {get;}
    public bool Compatible {get;}
    public bool IsFaulted {get;}
    public bool ApplyEntered {get;}
    public bool Succeeded {get;}
    public bool IsReady=>PreflightEntered&&Worked&&Compatible&&!IsFaulted;
    public long LibraryDomEpoch {get;}
    public int Attempts {get;}
    public int AllocatedNodeCount {get;}
    public LegacyLibraryOriginalFieldDomNode Page {get;}
    public LegacyLibraryOriginalFieldDomNode? ScratchField {get;}
    public IReadOnlyList<string> Trace {get;}
    public IReadOnlyList<LegacyLibraryOriginalFieldDomIdentityPair> IdentityMap {get;}
    internal LegacyLibraryOriginalFieldDomPublicationState(bool entered,bool worked,bool compatible,bool faulted,bool applied,bool success,
        long epoch,int attempts,int nodes,LegacyLibraryOriginalFieldDomNode page,LegacyLibraryOriginalFieldDomNode? scratch,string[] trace,
        LegacyLibraryOriginalFieldDomIdentityPair[] pairs)
    {PreflightEntered=entered;Worked=worked;Compatible=compatible;IsFaulted=faulted;ApplyEntered=applied;Succeeded=success;LibraryDomEpoch=epoch;
     Attempts=attempts;AllocatedNodeCount=nodes;Page=page;ScratchField=scratch;Trace=Array.AsReadOnly(trace);IdentityMap=Array.AsReadOnly(pairs);}
}
