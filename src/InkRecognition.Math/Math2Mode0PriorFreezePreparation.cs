using System.Runtime.CompilerServices;
using System.Text;

namespace InkRecognition.Math;

internal sealed record Math2Mode0FreezeRow(byte[] Text,LegacyNativeInkPositionRange[] Ranges,byte Flag);
internal readonly record struct Math2Mode0FreezeText(bool HasNode,byte Tag,string? Value,bool HasPointer=true);
// Prepared algorithm interface only. A service object does not issue an owning
// preparation; only the private live selected-scope borrow can do that.
internal interface IMath2Mode0FreezeServices
{
    int ChildCount { get; }
    bool HasTag(int child);
    bool HasFreeze(int child);
    long TagNumber(int child);
    object? FindSelection(long tag);
    Math2Mode0FreezeText FirstChild(int child);
    Math2Mode0FreezeRow? CloneRow(object selection,byte[] text);
}

internal static class Math2Mode0FreezeExtraction
{
    internal static bool Append(IMath2Mode0FreezeServices input,List<Math2Mode0FreezeRow> rows,
        Action<int>? appended=null,CancellationToken token=default)
    {
        for(int i=0;i<input.ChildCount;i++)
        {
            token.ThrowIfCancellationRequested();
            // Native makes BOTH attribute lookups, and tests presence only.
            bool tag=input.HasTag(i),freeze=input.HasFreeze(i);
            if(!tag||!freeze)continue;
            long id=input.TagNumber(i);
            object? selection=id==-1?null:input.FindSelection(id);
            var text=input.FirstChild(i); // Even after invalid ID/missing C22.
            if(selection is null||!text.HasNode||text.Tag is not(1 or 2 or 3)||!text.HasPointer)return false;
            if(text.Tag!=3&&text.Value is null)return false;
            byte[] bytes=text.Tag==3?[]:new UTF8Encoding(false,true).GetBytes(text.Value!);
            if(text.Tag==2&&(bytes.Length is<1 or>15))throw new NotSupportedException("Bounded valid inline string required.");
            var row=input.CloneRow(selection,bytes);
            if(row is null)return false;
            rows.Add(row);appended?.Invoke(rows.Count-1);
        }
        return true;
    }
}

internal enum Math2Mode0FreezeStage { Claimed, Appended, Complete }
internal sealed record Math2Mode0FreezeSnapshot(bool Complete,bool Failed,bool Released,long Reservation,
    int SequenceChildren,Math2Mode0FreezeRow[] Rows);

// Document02A440's owning extraction only. Nonzero rows still need0290C0 and
//02A830; no C23/F0/NN binding or nonfresh diff is issued here.
internal sealed partial class Math2Mode0PriorFreezePreparation:IDisposable
{
    internal const long DefaultBudget=16L*1024*1024;
    private sealed class Claim {internal int Used;}
    private static readonly ConditionalWeakTable<Math2Mode0PriorSelectedScope,Claim> Claims=new();
    private readonly object gate=new();
    private readonly Math2Mode0PriorSelectedScope scope;
    private readonly List<Math2Mode0FreezeRow> rows=[];
    private readonly long reservation;
    private readonly int children;
    private bool complete,failed,released;
    private Math2Mode0PriorFreezePreparation(Math2Mode0PriorSelectedScope scope,long reservation,int children)
    {this.scope=scope;this.reservation=reservation;this.children=children;}
    internal static Math2Mode0PriorFreezePreparation Create(Math2Mode0PriorSelectedScope scope,CancellationToken token=default)
        =>CreateDiagnostic(scope,DefaultBudget,null,token);
    internal static Math2Mode0PriorFreezePreparation CreateDiagnostic(Math2Mode0PriorSelectedScope scope,long budget,
        Action<Math2Mode0FreezeStage,int,Math2Mode0PriorFreezePreparation>? observe,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(scope);token.ThrowIfCancellationRequested();
        if(budget<0||budget>DefaultBudget)throw new ArgumentOutOfRangeException(nameof(budget));
        Math2Mode0PriorFreezePreparation? target=null;
        try
        {
            return scope.WithFreezeInput(input=>
            {
                long planned=input.Plan(budget,token);
                var claim=Claims.GetValue(scope,_=>new());
                if(Interlocked.CompareExchange(ref claim.Used,1,0)!=0)throw new InvalidOperationException("Prior freeze scope already consumed.");
                target=new(scope,planned,input.ChildCount);
                lock(target.gate)
                {
                    void Stage(Math2Mode0FreezeStage stage,int index=-1)
                    {observe?.Invoke(stage,index,target);ObjectDisposedException.ThrowIf(target.released,target);input.Check();token.ThrowIfCancellationRequested();}
                    Stage(Math2Mode0FreezeStage.Claimed);
                    if(!Math2Mode0FreezeExtraction.Append(input,target.rows,i=>Stage(Math2Mode0FreezeStage.Appended,i),token))
                        throw new InvalidOperationException("Prior freeze extraction failed; copied prefix retained.");
                    Stage(Math2Mode0FreezeStage.Complete);target.complete=true;return target;
                }
            },token);
        }
        catch
        {
            if(target is not null){lock(target.gate){target.failed=true;if(observe is null)target.Dispose();}}
            throw;
        }
    }
    internal Math2Mode0FreezeSnapshot CopySnapshot()
    {lock(gate)return new(complete,failed,released,reservation,children,rows.Select(r=>new Math2Mode0FreezeRow(r.Text.ToArray(),r.Ranges.ToArray(),r.Flag)).ToArray());}
    internal Math2Mode0FreezeSnapshot CopyResult(CancellationToken token=default)
        =>scope.WithCurrent(()=>{lock(gate){ObjectDisposedException.ThrowIf(released,this);if(!complete||failed)throw new InvalidOperationException("Successful live prior freeze preparation required.");return CopySnapshot();}},token);
    public void Dispose(){lock(gate)released=true;}
}

internal sealed partial class Math2Mode0PriorSelectedScope
{
    private FreezeBorrow? activeFreezeBorrow;
    internal sealed partial class FreezeBorrow:IMath2Mode0FreezeServices
    {
        private readonly Math2Mode0PriorSelectedScope owner;
        private readonly Action check;
        private readonly IReadOnlyList<Math2Mode0OriginalArena.Node> children;
        private FreezeBorrow(Math2Mode0PriorSelectedScope owner,Action check)
        {
            this.owner=owner;this.check=check;
            var field=owner.selectedArena.Nodes.Single(n=>n.Parent==0&&n.Kind==3&&n.Name.Text=="mathField");
            var result=owner.selectedArena.Children(field.Id).Single(n=>n.Kind==3&&n.Name.Text=="mathResult");
            var sequence=owner.selectedArena.Children(result.Id).FirstOrDefault(n=>n.Kind==3&&n.Name.Text=="sequence");
            children=sequence is null?[]:owner.selectedArena.Children(sequence.Id).Where(n=>n.Kind!=0).ToArray();
        }
        internal static FreezeBorrow Issue(Math2Mode0PriorSelectedScope owner,Action check)
        {if(!Monitor.IsEntered(owner.gate)||owner.activeFreezeBorrow is not null)throw new InvalidOperationException("Exclusive live scope borrow required.");check();return new(owner,check);}
        internal void Check()
        {if(!Monitor.IsEntered(owner.gate)||!ReferenceEquals(owner.activeFreezeBorrow,this))throw new InvalidOperationException("Current private freeze borrow required.");check();}
        public int ChildCount {get{Check();return children.Count;}}
        private Math2Mode0OriginalArena.Node? Attribute(int child,string name)
        {Check();return owner.selectedArena.Children(children[child].Id).FirstOrDefault(n=>n.Kind==0&&n.Name.Text==name);}
        public bool HasTag(int child)=>Attribute(child,"tag") is not null;
        public bool HasFreeze(int child)=>Attribute(child,"freeze") is not null;
        public long TagNumber(int child)
        {
            var tag=Attribute(child,"tag")??throw new InvalidOperationException();
            // Preserve existing actual-result tag4 policy. Generic typed reader
            //branches remain separately tested; no string-tag producer admitted.
            if(tag.Tag!=4||tag.Number is not long value)throw new NotSupportedException("Actual typed INT64 result tag required.");
            return Math2DomValueConversion.Signed64(value);
        }
        public object? FindSelection(long tag)
        {Check();try{return owner.selectedInk.SelectionIdentity(tag);}catch(KeyNotFoundException){return null;}}
        public Math2Mode0FreezeText FirstChild(int child)
        {
            Check();var text=owner.selectedArena.Children(children[child].Id).FirstOrDefault(n=>n.Kind!=0);
            return text is null?new(false,0,null):new(true,text.Tag,text.Tag==3?"":text.Text,text.Tag==3||text.Text is not null);
        }
        public Math2Mode0FreezeRow CloneRow(object selection,byte[] text)
        {Check();return new(text.ToArray(),owner.selectedInk.BorrowSelectionRanges(selection).ToArray(),1);}
        internal long Plan(long budget,CancellationToken token)
        {
            Check();long bytes=0;
            if(children.Count>65536)throw new NotSupportedException("Whole sequence bound.");
            foreach(var child in children)
            {token.ThrowIfCancellationRequested();var text=owner.selectedArena.Children(child.Id).FirstOrDefault(n=>n.Kind!=0);bytes=checked(bytes+Encoding.UTF8.GetByteCount(text?.Text??""));}
            // Conservative clone-payload bound, not all preflight metadata/RSS.
            long planned=checked(8192+children.Count*512L+bytes*4+children.Count*(long)owner.selectedInk.MaximumRangeCount*16);
            if(planned>budget)throw new NotSupportedException("Whole freeze payload exceeds budget before copying.");return planned;
        }
    }
    internal T WithFreezeInput<T>(Func<FreezeBorrow,T> consume,CancellationToken token=default)
        =>parent.WithCompletedOriginalInput(prior=>
        {
            lock(gate)
            {
                void Check()
                {
                    ObjectDisposedException.ThrowIf(disposed,this);token.ThrowIfCancellationRequested();prior.Check();
                    if(!ReferenceEquals(prior.ArenaIdentity,originalArena)||!ReferenceEquals(prior.InkIdentity,originalInk)||!ReferenceEquals(prior.Names,names)||prior.Epoch!=epoch)
                        throw new InvalidOperationException("Same live selected prior epoch required.");
                }
                Check();var input=FreezeBorrow.Issue(this,Check);activeFreezeBorrow=input;
                try{var result=consume(input);input.Check();return result;}
                finally{activeFreezeBorrow=null;}
            }
        },token);
}
