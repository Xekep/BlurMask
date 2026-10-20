using System.Runtime.CompilerServices;
using System.Text;

namespace InkRecognition.Math;

//Borrowed views are valid only under the actual field and publisher gates.
internal sealed record Math2Mode0OutgoingInput(Math2Mode0PublicationInput Field,object OriginalInk,
    object ThirdInk,LegacyLibraryOriginalDomNameOwner Names,int NodeCount,
    Func<IEnumerable<Math2Mode0PublishNode>> Nodes,IReadOnlyList<Math2Mode0PublishRegistry> Registry,
    long FinalNext,long PublicationReservation,Action<CancellationToken> Check);
internal enum Math2Mode0TransferStage { Claimed, WriterCreated, PayloadNode, DomExported,
    SourceSelection, Counter, SourceExported, Handoff, Complete }
internal sealed record Math2Mode0TransferCommand(byte Operation,long Id,string? Name,int Category,
    LegacyNativeInkPositionRange[] Ranges,long OldCounter20,long NewCounter18,bool DetachedC22);
internal sealed record Math2Mode0TransferSnapshot(bool Complete,bool Failed,bool Released,long Reservation,
    string Phase,int CleanupCount,bool DistinctRegistryOwners,bool FourthNameOwner,
    int SelectedField,int Ordinal,int[] Path,Math2Mode0PublishNode[] Writer,
    Math2Mode0TransferCommand[] Commands,bool DomExported,bool SourceExported,bool HandedOff,
    long Before,long After,bool ReverseDomNull,byte DomOperation,int PayloadRoot,int PendingRows,bool SameHandoffPayloads);

/// <summary>Owning library outgoing transfer from one genuine completed Math
/// temporary field. No mutation of the original PAGE/Ink, CBB or native edit ABI.</summary>
internal sealed class Math2Mode0OutgoingTransfer:IDisposable
{
    internal const long DefaultPayloadBudget=64L*1024*1024;
    private sealed class Claim {internal int Entered;}
    private static readonly ConditionalWeakTable<Math2Mode0FreshMathPublisher,Claim> Claims=new();
    private readonly object gate=new();
    private readonly Math2Mode0FreshMathPublisher publisher;
    private readonly LegacyLibraryOriginalDomNameOwner names=new();
    private readonly List<Math2Mode0PublishNode> writer=[];
    private readonly List<Math2Mode0TransferCommand> commands=[];
    private sealed record OwnedDomEdit(byte Operation,int[] Path,int Ordinal,int PayloadRoot,
        LegacyLibraryOriginalDomNameOwner Names,List<Math2Mode0PublishNode> Writer);
    private sealed record OwnedSourceEdit(List<Math2Mode0TransferCommand> Commands);
    private sealed record PendingRow(long Before,long After,OwnedDomEdit ForwardDom,OwnedDomEdit? ReverseDom,OwnedSourceEdit Ink);
    private OwnedDomEdit? dom;
    private OwnedSourceEdit? source;
    private PendingRow? pending;
    private readonly long reservation;
    private readonly bool distinctOwners,fourthNames;
    private readonly object originalInk,selectedInk,thirdInk;
    private readonly int selectedField,ordinal;
    private bool complete,failed,released,domExported,sourceExported,handedOff;
    private int cleanupCount;
    private string phase="claimed";
    private Math2Mode0OutgoingTransfer(Math2Mode0FreshMathPublisher publisher,Math2Mode0OutgoingInput input,long planned,int ordinal)
    {this.publisher=publisher;reservation=planned;this.ordinal=ordinal;selectedField=input.Field.FieldIdentity;
     originalInk=input.OriginalInk;selectedInk=input.Field.SelectedInk;thirdInk=input.ThirdInk;
     distinctOwners=!ReferenceEquals(input.OriginalInk,input.Field.SelectedInk)&&!ReferenceEquals(input.ThirdInk,input.Field.SelectedInk)&&!ReferenceEquals(input.ThirdInk,input.OriginalInk);
     fourthNames=!ReferenceEquals(names,input.Names);}
    internal static Math2Mode0OutgoingTransfer Create(Math2Mode0FreshMathPublisher publisher,CancellationToken token=default)
        =>CreateDiagnostic(publisher,DefaultPayloadBudget,null,token);
    internal static Math2Mode0OutgoingTransfer CreateDiagnostic(Math2Mode0FreshMathPublisher publisher,long budget,
        Action<Math2Mode0TransferStage,int,Math2Mode0OutgoingTransfer>? observer,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(publisher);token.ThrowIfCancellationRequested();
        if(budget<0||budget>DefaultPayloadBudget)throw new ArgumentOutOfRangeException(nameof(budget));
        return publisher.WithCompletedTransferInput(input=>
        {
            var plan=Math2Mode0OutgoingKernel.Plan(input,budget,token);
            input.Check(token);var claim=Claims.GetValue(publisher,_=>new());
            if(Interlocked.CompareExchange(ref claim.Entered,1,0)!=0)throw new InvalidOperationException("Actual completed publisher already transferred.");
            var target=new Math2Mode0OutgoingTransfer(publisher,input,plan.Reservation,plan.Ordinal);
            lock(target.gate)
            {
                try{target.Build(input,plan,observer,token);return target;}
                catch{target.failed=true;if(observer is null)target.Dispose();throw;}
            }
        },token);
    }
    private void Build(Math2Mode0OutgoingInput input,Math2Mode0OutgoingKernel.Prepared plan,
        Action<Math2Mode0TransferStage,int,Math2Mode0OutgoingTransfer>? observer,CancellationToken token)
    {
        void Observe(Math2Mode0TransferStage stage,int index=-1)
        {phase=stage.ToString();observer?.Invoke(stage,index,this);ObjectDisposedException.ThrowIf(released,this);input.Check(token);}
        Observe(Math2Mode0TransferStage.Claimed);
        writer.Add(new(1,4,names.Intern("").Text,0,0,0,null,null,null,null));Observe(Math2Mode0TransferStage.WriterCreated);
        void Clone(Math2Mode0PublishNode source,int parent)
        {
            input.Check(token);int id=writer.Count+1;
            writer.Add(source with{Id=id,Parent=parent,Origin=source.Id,Name=names.Intern(source.Name).Text});
            Observe(Math2Mode0TransferStage.PayloadNode,id);
            if(plan.Children.TryGetValue(source.Id,out var children))
            {foreach(var child in children.Where(n=>n.Kind==0))Clone(child,id);foreach(var child in children.Where(n=>n.Kind!=0))Clone(child,id);}
        }
        int payloadRoot=writer.Count+1;Clone(plan.Result,0);dom=new(2,[],ordinal,payloadRoot,names,writer);domExported=true;Observe(Math2Mode0TransferStage.DomExported);
        //07AE90 uses the concrete signed-ID ordered set, not hash/pending order.
        foreach(var entry in input.Registry.Where(r=>r.Id!=input.Field.InputId).OrderBy(r=>r.Id))
        {
            input.Check(token);commands.Add(new(4,entry.Id,names.Intern(entry.Name).Text,entry.Category,
                entry.Ranges.ToArray(),-1,-1,true));Observe(Math2Mode0TransferStage.SourceSelection,checked((int)entry.Id));
        }
        if(input.Field.Next<input.FinalNext)
        {input.Check(token);commands.Add(new(7,-1,null,0,[],input.Field.Next,input.FinalNext,false));Observe(Math2Mode0TransferStage.Counter);}
        source=new(commands);sourceExported=true;Observe(Math2Mode0TransferStage.SourceExported);
        input.Check(token);pending=new(-1,-1,dom,null,source);handedOff=true;Observe(Math2Mode0TransferStage.Handoff);
        Observe(Math2Mode0TransferStage.Complete);complete=true;
    }
    internal Math2Mode0TransferSnapshot CopySnapshot()
    {
        lock(gate)return new(complete,failed,released,reservation,phase,cleanupCount,distinctOwners,fourthNames,
            selectedField,ordinal,dom?.Path.ToArray()??[],writer.ToArray(),commands.Select(c=>c with{Ranges=c.Ranges.ToArray()}).ToArray(),
            domExported,sourceExported,handedOff,pending?.Before??-1,pending?.After??-1,pending?.ReverseDom is null,
            dom?.Operation??0,dom?.PayloadRoot??0,pending is null?0:1,
            pending is not null&&ReferenceEquals(pending.ForwardDom,dom)&&ReferenceEquals(pending.Ink,source)&&ReferenceEquals(dom?.Writer,writer)&&ReferenceEquals(source?.Commands,commands));
    }
    internal Math2Mode0TransferSnapshot CopyResult(CancellationToken token=default)
        =>publisher.WithCompletedTransferInput(input=>
        {lock(gate){ObjectDisposedException.ThrowIf(released,this);if(!complete||failed||!handedOff||pending is null||!ReferenceEquals(pending.ForwardDom,dom)||!ReferenceEquals(pending.Ink,source)||!ReferenceEquals(originalInk,input.OriginalInk)||!ReferenceEquals(selectedInk,input.Field.SelectedInk)||!ReferenceEquals(thirdInk,input.ThirdInk)||selectedField!=input.Field.FieldIdentity)throw new InvalidOperationException("Completed live owning outgoing transfer required.");return CopySnapshot();}},token);
    public void Dispose(){lock(gate){if(released)return;released=true;cleanupCount++;}}
}

internal static class Math2Mode0OutgoingKernel
{
    internal sealed record Prepared(long Reservation,int Ordinal,Math2Mode0PublishNode Result,
        IReadOnlyDictionary<int,Math2Mode0PublishNode[]> Children);
    internal static Prepared Plan(Math2Mode0OutgoingInput input,long budget,CancellationToken token)
    {
        input.Check(token);
        if(!ReferenceEquals(input.Names,input.Field.Names)||ReferenceEquals(input.OriginalInk,input.Field.SelectedInk)||ReferenceEquals(input.ThirdInk,input.Field.SelectedInk)||ReferenceEquals(input.ThirdInk,input.OriginalInk))throw new InvalidOperationException("Actual original/selected/third owner identities required.");
        if(input.NodeCount is<1 or>65536||input.Registry.Count is<2 or>65536)throw new NotSupportedException("Bounded whole fresh publisher required.");
        //Metadata records borrow immutable strings and actual ranges; payload/name/
        //C22 copies occur only after this complete preflight and actual claim.
        var nodes=input.Nodes().ToArray();if(nodes.Length!=input.NodeCount)throw new InvalidOperationException("Changing actual temporary graph.");
        var map=nodes.ToDictionary(n=>n.Id);var children=nodes.GroupBy(n=>n.Parent).ToDictionary(g=>g.Key,g=>g.ToArray());long valueBytes=0,rangeCount=0;
        foreach(var n in nodes)
        {
            token.ThrowIfCancellationRequested();int depth=0,id=n.Id;
            while(id!=0){if(++depth>256||!map.TryGetValue(id,out var parent))throw new NotSupportedException("Bounded actual temporary graph required.");id=parent.Parent;}
            valueBytes=checked(valueBytes+Encoding.UTF8.GetByteCount(n.Name)+Encoding.UTF8.GetByteCount(n.Text??"")+Encoding.UTF8.GetByteCount(n.NamespaceUri??"")+Encoding.UTF8.GetByteCount(n.NamespacePrefix??""));
        }
        if(valueBytes>32L*1024*1024)throw new NotSupportedException("Whole typed-value bound exceeded.");
        var original=input.Field.FieldNodes.Single(n=>n.Id==input.Field.FieldIdentity&&n.Kind==3&&n.Name=="mathField");
        var field=nodes.Single(n=>n.Kind==3&&n.Name=="mathField"&&n.Origin==original.Id);
        Math2Mode0MathFieldNode[] OldChildren(int id,bool attributes)=>input.Field.FieldNodes.Where(n=>n.Parent==id&&(attributes?n.Kind==0:n.Kind!=0)).ToArray();
        Math2Mode0PublishNode[] NewChildren(int id,bool attributes)=>children.TryGetValue(id,out var list)?list.Where(n=>attributes?n.Kind==0:n.Kind!=0).ToArray():[];
        void Same(Math2Mode0MathFieldNode old,Math2Mode0PublishNode next)
        {
            if(next.Origin!=old.Id||next.Kind!=old.Kind||next.Name!=old.Name||next.ValueTag!=old.ValueTag||next.Text!=old.StringValue||next.Number!=old.NumericValue||next.NamespaceUri!=old.NamespaceUri||next.NamespacePrefix!=old.NamespacePrefix)
                throw new NotSupportedException("Actual selected prefix changed.");
            foreach(bool attrs in new[]{true,false}){var left=OldChildren(old.Id,attrs);var right=NewChildren(next.Id,attrs);if(left.Length!=right.Length)throw new NotSupportedException("Selected prefix structure changed.");for(int i=0;i<left.Length;i++)Same(left[i],right[i]);}
        }
        var oa=OldChildren(original.Id,true);var na=NewChildren(field.Id,true);if(oa.Length!=1||na.Length!=1||oa[0].Name!="id")throw new NotSupportedException("Fresh id attribute required.");Same(oa[0],na[0]);
        var oc=OldChildren(original.Id,false);var nc=NewChildren(field.Id,false);
        if(oc.Length!=1||oc[0].Name!="input"||nc.Length!=2||nc[1].Name!="mathResult")throw new NotSupportedException("Fresh actual field INPUT prefix/result required.");Same(oc[0],nc[0]);
        var initial=input.Registry.Single(r=>r.Id==input.Field.InputId);
        if(initial.Name!="INPUT"||initial.Category!=102||!initial.Ranges.AsSpan().SequenceEqual(input.Field.InputRanges))throw new InvalidOperationException("Actual selected INPUT registry changed.");
        long expected=input.Field.Next;
        foreach(var r in input.Registry.Where(r=>r.Id!=input.Field.InputId).OrderBy(r=>r.Id))
        {
            token.ThrowIfCancellationRequested();if(r.Id!=expected++||r.Category!=102||r.Name is not("MATH_SYMBOL"or"MATH_NODE")||r.Ranges.Length==0)throw new NotSupportedException("Actual contiguous fresh third registry required.");
            foreach(var range in r.Ranges){LegacyNativeSourceRangeUtf8.ValidatePosition(range.First);LegacyNativeSourceRangeUtf8.ValidatePosition(range.Last);if(!LegacyOwnedRawInkRangeOperations.Contains(input.Field.InputRanges,range))throw new NotSupportedException("Third registry range is outside retained INPUT.");}
            rangeCount=checked(rangeCount+r.Ranges.LongLength);
        }
        if(expected!=input.FinalNext||input.Field.InputId>=input.Field.Next)throw new NotSupportedException("Actual initial/final counter provenance required.");
        var actualTags=new HashSet<long>();
        void ValidateTags(Math2Mode0PublishNode node)
        {
            token.ThrowIfCancellationRequested();
            if(node.Kind==0&&node.Name=="tag")
            {if(node.ValueTag!=4||node.Number is not long id||id<input.Field.Next||id>=input.FinalNext)throw new NotSupportedException("Actual rewritten INT64 result tags required.");actualTags.Add(id);}
            if(children.TryGetValue(node.Id,out var list))foreach(var child in list)ValidateTags(child);
        }
        ValidateTags(nc[1]);if(actualTags.Count!=input.Registry.Count-1)throw new NotSupportedException("Result tag and third registry sets differ.");
        long planned=checked(input.PublicationReservation+8192+nodes.LongLength*768+valueBytes*8+rangeCount*256);
        if(planned>budget)throw new NotSupportedException("Complete outgoing transfer envelope exceeds budget before payload clones.");
        return new(planned,oc.Length,nc[1],children);
    }
}
