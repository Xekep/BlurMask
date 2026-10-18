using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyDomPatchCompatibility.Probe")]
namespace InkRecognition;

/// <summary>Owned BE2.service8 scratch-tree preflight of the actual transported
/// library edit/target. This does not supply original page DOM, CB7 history,
/// CBB revisions or a native C20 acceptance receipt. Once claimed, the exact
/// ready result gates this document's selected-registry replay and DOM apply.</summary>
public sealed class LegacyLibraryTextDomPatchCompatibility
{
    private readonly object gate=new();
    private bool entered;
    private LegacyLibraryTextDomPatchCompatibilityState state;
    public LegacyLibraryTextPublicationDocument DocumentOwner {get;}
    public LegacyLibraryTextPublicationScope Scope {get;}
    public LegacyLibraryTextDomTransportRow TransportRow {get;}
    public LegacyLibraryTextDomEditResult Edit {get;}
    public LegacyLibraryTextDomTarget Target=>Edit.Target;
    public LegacyFreshMainTextDocumentSeed Seed=>Target.Seed;
    public LegacyOwnedRawInkDocument Document=>Target.Document;
    public LegacyOwnedFreshTextSourceContext TransactionOwner=>TransportRow.TransactionOwner;
    public LegacyLibraryTextDomPatchCompatibilityState State {get{lock(gate)return state;}}
    internal LegacyLibraryTextDomPatchCompatibility(LegacyLibraryTextPublicationDocument document,
        LegacyLibraryTextPublicationScope scope,LegacyLibraryTextDomTransportRow row,CancellationToken token)
    {
        DocumentOwner=document;Scope=scope;TransportRow=row;Edit=row.DomEdit;
        if(!ReferenceEquals(Edit.Target.Seed,scope.Seed)||!ReferenceEquals(Edit.Target.Document,document.Document)||
            !ReferenceEquals(Edit.Source.Sources.Seed,scope.Seed)||
            !ReferenceEquals(Edit.Source.Arena.NamePool,Target.Arena.NamePool)||
            !Target.Arena.IsParameterizedNoHistory||!ReferenceEquals(row.TransactionOwner,scope.Seed.PendingSelections))
            throw new ArgumentException("Exact no-history library target/edit/Seed/transaction required.");
        Target.WithPatchPreflight(Edit,(arena,root)=>{LegacyPreparedDomPatchPreflight.Validate(arena,root,Rows(),token);return 0;},token);
        state=new(this,null);
    }
    public static LegacyLibraryTextDomPatchCompatibility Prepare(LegacyLibraryTextPublicationDocument actualDocument,
        CancellationToken token=default)
    {ArgumentNullException.ThrowIfNull(actualDocument);return actualDocument.ClaimDomCompatibility(token);}
    private LegacyPreparedDomPatchRow[] Rows()=>Edit.OwnedRows.Select(row=>new LegacyPreparedDomPatchRow(
        row.Operation,checked((int)row.ChildOrdinal),row.Path.ToArray(),Edit.Source.Arena,row.LivePayload)).ToArray();
    public LegacyLibraryTextDomPatchCompatibilityState Run(CancellationToken token=default)=>RunCore(LegacyPreparedDomPatchFault.None,token);
    internal LegacyLibraryTextDomPatchCompatibilityState RunPrepared(LegacyPreparedDomPatchFault fault,CancellationToken token=default)
        =>RunCore(fault,token);
    private LegacyLibraryTextDomPatchCompatibilityState RunCore(LegacyPreparedDomPatchFault fault,CancellationToken token)
        =>DocumentOwner.WithDomCompatibility(this,()=>
        {
            lock(gate)
            {
                fault.Validate();token.ThrowIfCancellationRequested();
                if(entered)throw new InvalidOperationException("DOM preflight retains its entered prefix and is single-use.");
                return Target.WithPatchPreflight(Edit,(arena,root)=>
                {
                    var rows=Rows();LegacyPreparedDomPatchPreflight.Validate(arena,root,rows,token);
                    token.ThrowIfCancellationRequested();entered=true;
                    var result=LegacyPreparedDomPatchPreflight.Run(arena,root,rows,fault,token);
                    state=new(this,result);
                    if(result.RefusedUncheckedClone)throw new NotSupportedException("Native payload clone-null has no defined compatibility result; entered preflight is poisoned.");
                    if(result.Canceled)throw new OperationCanceledException("Entered scratch preflight cancellation prefix.");
                    if(!result.Written)throw new NotSupportedException("Unsupported scratch simulation boundary; entered preflight is poisoned.");
                    return state;
                },token);
            }
        },token);
}

/// <summary>Immutable diagnostic prefix. Compatibility is distinct from the
/// Worked return. ScratchTree is an owner-free diagnostic copy, never a target
/// or an accepted document. Defined setup failure writes compatibility1;
/// unsupported unchecked payload-clone failure/cancellation writes no result.</summary>
public sealed class LegacyLibraryTextDomPatchCompatibilityState
{
    public LegacyLibraryTextDomPatchCompatibility Owner {get;}
    public bool WasRun {get;}
    public bool Worked {get;}
    public byte CompatibilityByte {get;}
    public bool CompatibilityWasWritten {get;}
    public bool IsFaulted {get;}
    public bool IsReady=>WasRun&&Worked&&CompatibilityWasWritten&&CompatibilityByte==1&&!IsFaulted;
    public int InitialChildrenCopied {get;}
    public int MappedRowCount {get;}
    public int SimulatedRowCount {get;}
    public bool RefusedUncheckedClone {get;}
    public LegacyLibraryTextDomTreeValue? DiagnosticScratchTree {get;}
    public IReadOnlyList<string> Trace {get;}
    internal LegacyLibraryTextDomPatchCompatibilityState(LegacyLibraryTextDomPatchCompatibility owner,LegacyPreparedDomPatchResult? result)
    {
        Owner=owner;WasRun=result is not null;Worked=result?.Worked??false;
        CompatibilityByte=result?.Compatible??0;CompatibilityWasWritten=result?.Written??false;
        IsFaulted=result is not null&&(!result.Worked||!result.Written||result.Compatible!=1);
        InitialChildrenCopied=result?.InitialChildrenCopied??0;MappedRowCount=result?.MappedRows??0;
        SimulatedRowCount=result?.SimulatedRows??0;RefusedUncheckedClone=result?.RefusedUncheckedClone??false;
        DiagnosticScratchTree=result?.Tree;Trace=Array.AsReadOnly(result?.Trace??[]);
    }
    public byte[] CopyOutputBytes()=>[Worked?(byte)1:(byte)0,CompatibilityByte];
}

internal sealed record LegacyPreparedDomPatchRow(uint Operation,int ChildOrdinal,uint[] Path,
    LegacyLibraryTextDomArena PayloadArena,LegacyDomNode Payload);
//Prepared-only native allocation/undefined-boundary controls, not caller flags.
internal readonly record struct LegacyPreparedDomPatchFault(int Stage=0,int InitialChild=-1,int PayloadRow=-1,int CancelBeforeRow=-1)
{
    internal static LegacyPreparedDomPatchFault None=>new(0,-1,-1,-1);
    internal void Validate(){if(Stage is <0 or >2||InitialChild< -1||PayloadRow< -1||CancelBeforeRow< -1)throw new ArgumentOutOfRangeException(nameof(Stage));}
}
internal sealed record LegacyPreparedDomPatchResult(bool Worked,byte Compatible,bool Written,
    int InitialChildrenCopied,int MappedRows,int SimulatedRows,bool RefusedUncheckedClone,bool Canceled,
    LegacyLibraryTextDomTreeValue? Tree,string[] Trace);
internal static class LegacyPreparedDomPatchPreflight
{
    internal const int MaximumRows=65536,MaximumNodes=65536,MaximumDepth=128,
        MaximumValueBytes=4*1024*1024,MaximumPathEntries=65536;
    internal static void Validate(LegacyLibraryTextDomArena target,LegacyDomNode root,
        LegacyPreparedDomPatchRow[] rows,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        if(root.Kind!=4||root.Released||!target.IsParameterizedNoHistory||rows.Length>MaximumRows)
            throw new NotSupportedException("Bounded live parameterized-BCC kind4 edit required.");
        long nodes=0,bytes=0,paths=0;
        void Tree(LegacyDomNode source,int depth,bool rootAllowed)
        {
            token.ThrowIfCancellationRequested();
            if(source.Released||depth>MaximumDepth||source.Kind is not (0 or 1 or 3 or 4)||source.Kind==4&&!rootAllowed)
                throw new NotSupportedException("Only ordinary live DOM content within the native clone domain is supported.");
            nodes++;bytes+=source.Value.Length;
            if(nodes>MaximumNodes||bytes>MaximumValueBytes)throw new NotSupportedException("Scratch clone budget exhausted.");
            foreach(var attr in source.Attributes){if(attr.Kind!=0)throw new NotSupportedException("Actual attribute kind0 required.");Tree(attr,depth+1,false);}
            foreach(var child in source.Children){if(child.Kind is not (1 or 3)||!ReferenceEquals(child.Parent,source))throw new NotSupportedException("No terminator/external or detached child support.");Tree(child,depth+1,false);}
        }
        Tree(root,1,true);
        foreach(var row in rows)
        {
            if(row.Operation is not (2 or 4)||row.Payload.Kind is not (1 or 3)||
                !ReferenceEquals(row.PayloadArena.Document,target.Document)||
                !ReferenceEquals(row.PayloadArena.NamePool,target.NamePool))
                throw new NotSupportedException("Actual same-name-owner ordinary operation2/4 payloads required.");
            paths+=row.Path.Length;
            if(row.Path.Length>MaximumDepth||paths>MaximumPathEntries)throw new NotSupportedException("Mapped-path budget exhausted.");
            Tree(row.Payload,row.Path.Length+2,false);
        }
    }
    internal static LegacyPreparedDomPatchResult Run(LegacyLibraryTextDomArena target,LegacyDomNode root,
        LegacyPreparedDomPatchRow[] rows,LegacyPreparedDomPatchFault fault,CancellationToken token)
    {
        fault.Validate();Validate(target,root,rows,default);
        var trace=new List<string>();LegacyLibraryTextDomArena? scratch=null;LegacyDomNode? copiedRoot=null;
        var detached=new List<LegacyDomNode>();int initial=0,mapped=0,simulated=0;
        LegacyPreparedDomPatchResult Finish(bool worked,byte compatible,bool written=true,bool uncheckedClone=false,bool canceled=false)
        {
            LegacyLibraryTextDomTreeValue? tree=copiedRoot is null?null:LegacyLibraryTextDomTreeValue.CopyFrom(scratch!.Snapshot(copiedRoot));
            return new(worked,compatible,written,initial,mapped,simulated,uncheckedClone,canceled,tree,trace.Concat(scratch?.Trace??Array.Empty<string>()).ToArray());
        }
        if(rows.Length==0){trace.Add("empty-edit");return Finish(true,1);}
        try
        {
            token.ThrowIfCancellationRequested();
            trace.Add("scratch-bcc");
            scratch=new(target.Seed,target.NamePool,failAt:fault.Stage==1?0:-1);
            copiedRoot=scratch.TryConstructorRoot();if(copiedRoot is null)return Finish(false,1);
            foreach(var child in root.Children)
            {
                token.ThrowIfCancellationRequested();
                if(initial==fault.InitialChild)scratch.BeginFaultScope(0);
                var copy=scratch.CloneFrom(target,child,token);if(copy is null)return Finish(false,1);
                scratch.Append(copiedRoot,copy);initial++;
            }
            trace.Add("parent-map");if(fault.Stage==2)return Finish(false,1);
            var parents=new LegacyDomNode[rows.Length];
            for(int i=0;i<rows.Length;i++)
            {
                token.ThrowIfCancellationRequested();var current=copiedRoot;
                foreach(uint step in rows[i].Path)
                {if(step>int.MaxValue||step>=current.Children.Count){current=null;break;}current=current.Children[(int)step];}
                if(current is null){trace.Add("missing-path:"+i);return Finish(true,0);}
                parents[i]=current;mapped++;
            }
            for(int i=0;i<rows.Length;i++)
            {
                token.ThrowIfCancellationRequested();if(i==fault.CancelBeforeRow)throw new OperationCanceledException();
                var row=rows[i];var parent=parents[i];
                if(parent.Released)throw new NotSupportedException("Native pre-resolved parent was detached/freed by an earlier row.");
                LegacyDomNode? child=row.ChildOrdinal>=0&&row.ChildOrdinal<parent.Children.Count?parent.Children[row.ChildOrdinal]:null;
                trace.Add("simulate:"+i+":"+row.Operation);
                if(row.Operation==4)
                {
                    if(child is null)return Finish(true,0);
                    parent.Children.Remove(child);child.Parent=null;detached.Add(child);
                }
                else
                {
                    if(i==fault.PayloadRow)scratch.BeginFaultScope(0);
                    var copy=scratch.CloneFrom(row.PayloadArena,row.Payload,token);
                    if(copy is null)return Finish(false,0,false,true);
                    if(child is not null)
                    {
                        //BCC.VB8=0EADD0 inserts before the existing child;
                        //it neither unlinks nor frees that child.
                        parent.Children.Insert(row.ChildOrdinal,copy);copy.Parent=parent;
                    }
                    else if(parent.Children.Count==row.ChildOrdinal)scratch.Append(parent,copy);
                    else{detached.Add(copy);return Finish(true,0);}
                }
                simulated++;
            }
            return Finish(true,1);
        }
        catch(OperationCanceledException){return Finish(false,0,false,canceled:true);}
        catch(NotSupportedException e){trace.Add("unsupported:"+e.Message);return Finish(false,0,false);}
        finally
        {
            //Managed scratch storage has no external handles. Native destroys
            //the entire temporary BCC, including detached children, at return.
            if(copiedRoot is not null&&!copiedRoot.Released)scratch!.Release(copiedRoot);
            foreach(var child in detached)if(!child.Released)scratch!.Release(child);
        }
    }
}
