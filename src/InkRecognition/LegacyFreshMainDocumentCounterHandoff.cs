using System.Buffers.Binary;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LegacyMainDocumentCounterHandoff.Probe")]

namespace InkRecognition;

/// <summary>Native07AAF0 pending operation7 retained in the same C4D owner.
/// It records counters; it has not executed C20.VF8 or applied selections.</summary>
public sealed class LegacyOwnedFreshTextCounterCommand
{
    public LegacyOwnedFreshTextSourceContext Owner {get;}
    public int CommandIdentity {get;}
    public int PreviousCommandIdentity=>CommandIdentity-1;
    public byte OperationCode=>7;
    public long OldCounter20 {get;}
    public long NewCounter18 {get;}
    internal LegacyOwnedFreshTextCounterCommand(LegacyOwnedFreshTextSourceContext owner,int identity,long oldCounter,long newCounter)
    {Owner=owner;CommandIdentity=identity;OldCounter20=oldCounter;NewCounter18=newCounter;}
    /// <summary>All assigned bytes18..4F; pointer and padding lanes omitted.
    ///077230 clears28..48;07AAF0 writes new18 and old20, even if negative.</summary>
    public byte[] CopyAssignedBody56()
    {
        byte[] bytes=new byte[56];BinaryPrimitives.WriteInt64LittleEndian(bytes,NewCounter18);
        BinaryPrimitives.WriteInt64LittleEndian(bytes.AsSpan(8),OldCounter20);return bytes;
    }
}

/// <summary>Immutable view of one retained pending command. Only operation4
/// and the bounded trailing operation7 occur here. Links are owner-local
/// transaction identities, not serialized native pointers.</summary>
public sealed class LegacyOwnedFreshTextPendingCommand
{
    public LegacyOwnedFreshTextSourceContext Owner {get;}
    public LegacyOwnedFreshTextSelection? Selection {get;}
    public LegacyOwnedFreshTextCounterCommand? Counter {get;}
    public int CommandIdentity=>Selection?.CommandIdentity??Counter!.CommandIdentity;
    public int PreviousCommandIdentity=>CommandIdentity-1;
    public int NextCommandIdentity {get;}
    public byte OperationCode=>Selection?.OperationCode??Counter!.OperationCode;
    internal LegacyOwnedFreshTextPendingCommand(LegacyOwnedFreshTextSelection selection,int next)
    {Owner=selection.Owner;Selection=selection;NextCommandIdentity=next;}
    internal LegacyOwnedFreshTextPendingCommand(LegacyOwnedFreshTextCounterCommand counter)
    {Owner=counter.Owner;Counter=counter;}
}

public sealed partial class LegacyOwnedFreshTextSourceContext
{
    private bool counterHandoffEntered;
    private LegacyOwnedFreshTextCounterCommand? counterCommand;
    /// <summary>Complete supported transaction view. CopyPendingSelections
    /// remains the unchanged operation4-only selection snapshot API.</summary>
    public IReadOnlyList<LegacyOwnedFreshTextPendingCommand> CopyPendingCommands()
    {
        lock(gate)
        {
            var rows=new LegacyOwnedFreshTextPendingCommand[commands.Count+(counterCommand is null?0:1)];
            for(int index=0;index<commands.Count;index++)rows[index]=new(commands[index],index+1<rows.Length?index+2:0);
            if(counterCommand is not null)rows[^1]=new(counterCommand);
            return Array.AsReadOnly(rows);
        }
    }
    internal LegacyPendingCounterAppend AppendOwnedCounter(LegacyFreshMainDocumentInputTransforms actualTransforms,CancellationToken token)
    {
        LegacyFreshMainDocumentCounterHandoff.Validate(actualTransforms,token);
        lock(gate)
        {
            if(!ReferenceEquals(actualTransforms.Seed.PendingSelections,this))throw new ArgumentException("Same retained pending C4D owner required.");
            var text=actualTransforms.Cleanup.State.Text;var prefix=text.WordChoices.PendingSelections;
            if(commands.Count!=prefix.Count+1||!ReferenceEquals(commands[^1],text.Selection)||
                commands.Take(prefix.Count).Where((command,index)=>!ReferenceEquals(command,prefix[index])).Any())
                throw new InvalidOperationException("The exact retained CHAR/WORD/TEXT prefix changed before counter append.");
            return AppendCounterCore(actualTransforms.Seed.InputBinding.Next,next,false,token);
        }
    }
    internal LegacyPendingCounterAppend AppendOwnedEnglishFreshCounter(
        LegacyEnglishCaseNode0Completion.GraphWordPublicationContext actual,CancellationToken token)
    {
        var stamp=actual.RequireFreshCounterOwner(this,token);
        return AppendCounterCore(stamp.Old,stamp.Next,false,token,stamp.Guard);
    }
    //Explicit scalar/fault controls, not a public choice of caller counters.
    internal LegacyPendingCounterAppend AppendPreparedCounter(long oldCounter,long newCounter,bool allocationFailure=false,
        CancellationToken token=default)=>AppendCounterCore(oldCounter,newCounter,allocationFailure,token);
    private LegacyPendingCounterAppend AppendCounterCore(long oldCounter,long newCounter,bool allocationFailure,CancellationToken token,ILegacyFreshSourceIndexGuard? guard=null)
    {
        lock(gate)
        {
            token.ThrowIfCancellationRequested();guard?.Require(token);
            if(failed||counterHandoffEntered)throw new InvalidOperationException("This pending counter handoff retains an entered prefix and is single-use.");
            counterHandoffEntered=true;
            if(oldCounter==newCounter)return new(true,null);
            //0298AA equality skips the allocation entirely. Otherwise a
            //failed07AAF0 allocation leaves all previous commands intact.
            if(allocationFailure){failed=true;return new(false,null);}
            try
            {
                var created=new LegacyOwnedFreshTextCounterCommand(this,checked(commands.Count+1),oldCounter,newCounter);
#if ENGLISH_FRESH_CONTINUATION_CONTROLS
                LegacyFreshContinuationTestHooks.Checkpoint?.Invoke("after-counter-allocation");
#endif
                token.ThrowIfCancellationRequested();guard?.Require(token);counterCommand=created;
                return new(true,counterCommand);
            }
            catch{failed=true;throw;}
        }
    }
}
internal sealed record LegacyPendingCounterAppend(bool Succeeded,LegacyOwnedFreshTextCounterCommand? Command);

/// <summary>Actual post026EF0 numeric-counter handoff to the same pending
/// C4D transaction. It precedes2E7.V0; document application/acceptance remains
/// outside this stage.</summary>
public sealed class LegacyFreshMainDocumentCounterHandoff
{
    public LegacyFreshMainDocumentInputTransforms Transforms {get;}
    public LegacyOwnedFreshTextSourceContext TransactionOwner=>Transforms.Seed.PendingSelections;
    public LegacyOwnedFreshTextCounterCommand? CounterCommand {get;}
    public IReadOnlyList<LegacyOwnedFreshTextPendingCommand> Commands {get;}
    public long InitialPendingCounter=>Transforms.Seed.InputBinding.Next;
    public long FinalPendingCounter {get;}
    private LegacyFreshMainDocumentCounterHandoff(LegacyFreshMainDocumentInputTransforms transforms,LegacyPendingCounterAppend append)
    {Transforms=transforms;CounterCommand=append.Command;FinalPendingCounter=TransactionOwner.NextSelectionId;Commands=TransactionOwner.CopyPendingCommands();}
    public static LegacyFreshMainDocumentCounterHandoff Apply(LegacyFreshMainDocumentInputTransforms actualTransforms,
        CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(actualTransforms);var transaction=actualTransforms.Seed.PendingSelections;
        var result=transaction.AppendOwnedCounter(actualTransforms,token);
        if(!result.Succeeded)throw new InvalidOperationException("Native07AAF0 retained an allocation-failure prefix.");
        return new(actualTransforms,result);
    }
    internal static void Validate(LegacyFreshMainDocumentInputTransforms actual,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(actual);token.ThrowIfCancellationRequested();
        if(!actual.Succeeded||!actual.FollowingStage026EF0Executed||actual.TransformCount110!=0||actual.DafInputCount!=0||
            !ReferenceEquals(actual.VectorOwner,actual.Seed.CurrentTransforms))
            throw new ArgumentException("Actual successful same-seed037E80 and empty DAF026EF0 result required.");
        actual.Cleanup.ValidateCurrent(token);
        actual.Seed.Validate(actual.Cleanup.State.Text.WordChoices.Owner,token);
    }
}
