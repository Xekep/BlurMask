using System.Buffers.Binary;
using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyFullEnglishFirstTerminal.Probe")]
namespace InkRecognition;

/// <summary>Full-source FIRST numeric terminal and exact B8 trace only. No
/// rich273, WORD/source720, final guide, dictionary winner or app-default change.</summary>
public sealed class LegacyFullEnglishFirstTerminal
{
    public LegacyFullEnglishFirstContextPasses Owner{get;}
    public LegacyFirstTerminalSelectionPolicy Invocation{get;}
    private readonly object gate=new();
    private readonly LegacyFullEnglishFirstContextChild[] children;
    private readonly LegacyFullEnglishFirstTerminalBudget budget;
    private readonly List<LegacyFullEnglishFirstTerminalChild> prefix=[];
    private bool entered,running,complete,faulted;
    public bool IsComplete{get{lock(gate)return complete;}}
    public bool IsFaulted{get{lock(gate)return faulted;}}
    public long AdditionalLogicalPeakBytes=>budget.Peak;
    public long LogicalCopyWorkBytes=>budget.Work;
    public long LogicalVisitWork=>budget.Visits;
    private LegacyFullEnglishFirstTerminal(LegacyFullEnglishFirstContextPasses owner,long peak,long work,long visits,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(owner);token.ThrowIfCancellationRequested();Owner=owner;RequireComplete();
        children=owner.CopyCompletedChildren().ToArray();
        foreach(var c in children){token.ThrowIfCancellationRequested();if(!ReferenceEquals(c.Owner,owner)||!ReferenceEquals(c.Primary.Owner,owner.Owner)||!ReferenceEquals(c.Primary.Initializer.Owner,owner.Owner.Owner))throw new ArgumentException("Same canonical full B4/B3/B2 children required.");}
        budget=new(children,peak,work,visits,token);
        var model=owner.Owner.Owner.Owner.Parent.Model;var source=model.Source;
        if(model.SelectedProcessor.Kind!=0x249||model.SelectedProcessor.Version!=0x100||model.SelectedProcessor.Offset!=8827||model.SelectedProcessor.End!=373961||source.Offset!=8843||source.End!=373957||source.SourceKind!=0x135||source.Version!=0x110)
            throw new NotSupportedException("Actual full selected249/source135 required.");
        byte[] Field(string name)=>source.Fields.Single(f=>f.Owner=="247"&&f.Destination==name).CopyBytes();
        if(Field("B8")[0]!=0||Field("B9")[0]!=0||BinaryPrimitives.ReadUInt16LittleEndian(Field("BA"))!=5||BinaryPrimitives.ReadUInt16LittleEndian(Field("BC"))!=65535||source.Alphabet.LabelCount!=217||source.Alphabet.TryGetLabel("\u0651",out _))
            throw new NotSupportedException("Actual full width5, zero-trait fresh source required.");
        foreach(var c in children)
        {
            token.ThrowIfCancellationRequested();var cache=c.Primary.Initializer.ActualCache;
            if(!ReferenceEquals(cache.Model,model)||!ReferenceEquals(cache.Owner.Preparation.Model,model)||!ReferenceEquals(cache.Owner.Child.Owner,owner.Owner.Owner.Owner.Parent))throw new ArgumentException("Same actual full parent/cache/preparation required.");
        }
        //158970 copies actual raw source150 into child configA8; fresh16A130
        //source150=0 survives admitted24A/XY-only24C/265 with no properties/reuse.
        //Fresh2B3 ctor clears99;1EDB60 writes F4=A000; fresh/reset273 clears78.
        //This typed route is distinct from arbitrary native reused invocations.
        Invocation=new(65535,0xa000,false,false,source.SourceKind,0);
    }
    private void RequireComplete()
    {
        if(!Owner.IsComplete||Owner.IsFaulted||!Owner.Owner.IsComplete||Owner.Owner.IsFaulted||!Owner.Owner.Owner.IsComplete||Owner.Owner.Owner.IsFaulted||!Owner.Owner.Owner.Owner.IsComplete||Owner.Owner.Owner.Owner.IsFaulted||!Owner.Owner.Owner.Owner.Parent.IsComplete||Owner.Owner.Owner.Owner.Parent.IsFaulted)
            throw new ArgumentException("Complete nonfaulted full B4/B3/B2/B1/A authority required.");
    }
    public static LegacyFullEnglishFirstTerminal Prepare(LegacyFullEnglishFirstContextPasses completedB4,CancellationToken token=default)=>new(completedB4,LegacyFullEnglishFirstTerminalBudget.PeakLimit,LegacyFullEnglishFirstTerminalBudget.WorkLimit,LegacyFullEnglishFirstTerminalBudget.VisitLimit,token);
    internal static LegacyFullEnglishFirstTerminal PrepareBounded(LegacyFullEnglishFirstContextPasses owner,long peak,long work,long visits,CancellationToken token=default)=>new(owner,peak,work,visits,token);
    public void Run(CancellationToken token=default)=>RunCore(null,token);
    internal void RunPrepared(Action<string> checkpoint,CancellationToken token=default)=>RunCore(checkpoint,token);
    private void RunCore(Action<string>? checkpoint,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();RequireComplete();lock(gate){token.ThrowIfCancellationRequested();if(entered||running)throw new InvalidOperationException("Full B5 terminal owner is one-use.");entered=running=true;}
        try
        {
            checkpoint?.Invoke("entry");token.ThrowIfCancellationRequested();int retainedCells=0;
            foreach(var child in children)
            {
                checkpoint?.Invoke("child:entry");token.ThrowIfCancellationRequested();var value=Build(child,checkpoint,token);
                retainedCells=checked(retainedCells+value.Cells.Count);if(retainedCells>budget.Nodes)throw new InvalidDataException("Trace exceeded whole descending-node bound.");
                lock(gate){token.ThrowIfCancellationRequested();prefix.Add(value);}
                checkpoint?.Invoke("child:published");token.ThrowIfCancellationRequested();
            }
            lock(gate){token.ThrowIfCancellationRequested();complete=true;}
        }
        catch{lock(gate)faulted=true;throw;}
        finally{lock(gate)running=false;}
    }
    private LegacyFullEnglishFirstTerminalChild Build(LegacyFullEnglishFirstContextChild child,Action<string>? checkpoint,CancellationToken token)
    {
        var graph=child.Primary.Initializer.ActualCache.Owner.SourceRanges.Graph;
        if(graph.NodeCount!=child.NodeCount||child.Primary.Primary.NodeCount!=child.NodeCount)throw new InvalidDataException("Same graph/primary/B8 dimensions required.");
        var storage=new ReadOnlyMemory<byte>[child.NodeCount];var primary=new LegacyFirstAcousticRecordTable[child.NodeCount];
        for(int n=0;n<storage.Length;n++)
        {
            token.ThrowIfCancellationRequested();var rows=new byte[200];
            for(int r=0;r<5;r++){token.ThrowIfCancellationRequested();var row=child.GetB8RowCopy(n,r);var bytes=row.CopyStorage();if(bytes.Length!=40||row.Count is<0 or>1)throw new InvalidDataException("Actual A4=1/F8=5 B8 required.");bytes.CopyTo(rows,r*40);}
            storage[n]=rows;primary[n]=child.Primary.Primary.GetRowCopy(n);
        }
        var grid=new LegacyFirstMergedRecordGrid(1,5,storage,token);checkpoint?.Invoke("grid");token.ThrowIfCancellationRequested();
        var selected=LegacyFirstTerminalSelection.Select(grid,Invocation,token);if(selected.Choices.Count>1)throw new InvalidDataException("Actual full A4=1 terminal exceeded one choice.");
        checkpoint?.Invoke("selected");token.ThrowIfCancellationRequested();
        if(selected.Choices.Count==0)return new(this,child,selected.ReferenceCost78,null,Array.Empty<LegacyFirstSelectedGlyphCell>());
        var choice=selected.Choices[0];var trace=LegacyFirstSelectedGlyphTrace.Read(grid,choice,token);
        foreach(var cell in trace)
        {
            token.ThrowIfCancellationRequested();var arcs=graph.GetNode(cell.Node);var row=primary[cell.Node];
            if(cell.Arc>=arcs.Count||cell.Span!=arcs[cell.Arc].Span||cell.Row>=row.Count||row.Get(cell.Row).Symbol!=cell.Symbol||row.Get(cell.Row).ArcOrdinal!=cell.Arc)
                throw new InvalidDataException("Selected full B8 trace differs from same graph/primary row.");
        }
        checkpoint?.Invoke("trace");token.ThrowIfCancellationRequested();return new(this,child,selected.ReferenceCost78,choice,trace);
    }
    /// <summary>Complete immutable child prefixes; after failure these are
    /// historical diagnostics, never authority for future WORD publication.</summary>
    public IReadOnlyList<LegacyFullEnglishFirstTerminalChild> CopyCompletedChildren(){lock(gate)return Array.AsReadOnly(prefix.ToArray());}
}

/// <summary>One canonical child slot even when no finite terminal exists.
/// Empty means Choice=null and Cells.Count=0, never a guessed text/WORD.</summary>
public sealed class LegacyFullEnglishFirstTerminalChild
{
    public LegacyFullEnglishFirstTerminal Owner{get;}
    public LegacyFullEnglishFirstContextChild Context{get;}
    public long ReferenceCost78{get;}
    public LegacyFirstTerminalChoice? Choice{get;}
    public bool HasFiniteTerminal=>Choice.HasValue;
    public IReadOnlyList<LegacyFirstSelectedGlyphCell> Cells{get;}
    internal LegacyFullEnglishFirstTerminalChild(LegacyFullEnglishFirstTerminal owner,LegacyFullEnglishFirstContextChild context,long reference,LegacyFirstTerminalChoice? choice,IReadOnlyList<LegacyFirstSelectedGlyphCell> cells)
    {Owner=owner;Context=context;ReferenceCost78=reference;Choice=choice;Cells=Array.AsReadOnly(cells.ToArray());}
}
