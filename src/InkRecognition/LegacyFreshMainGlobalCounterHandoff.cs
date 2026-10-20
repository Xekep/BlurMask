using System.Runtime.CompilerServices;
[assembly:InternalsVisibleTo("LegacyGlobalCounterHandoff.Probe")]
namespace InkRecognition;

public sealed partial class LegacyOwnedFreshTextSourceContext
{
    internal LegacyPendingCounterAppend AppendActualGlobalCounter(LegacyFreshMainGlobalInputTransforms transforms,bool preparedAllocationFailure,CancellationToken token)
    {
        transforms.ValidateCurrent(token);var parent=transforms.Cleanup.State.Publication;
        if(!ReferenceEquals(transforms.Seed.PendingSelections,this)||!ReferenceEquals(InputBinding,transforms.Seed.InputBinding))
            throw new ArgumentException("The genuine global result must retain this identical selected INPUT/C4D owner.");
        var prefix=parent.CopyPendingSelections();long expectedNext=NextSelectionId,initial=transforms.Seed.InputBinding.Next;
        if(prefix.Count is<1 or>65536||!ReferenceEquals(prefix[^1],parent.Text.Selection)||initial<0||expectedNext<initial)
            throw new InvalidOperationException("Complete actual global CHAR/WORD/TEXT prefix and monotonic selected registry counters required.");
        lock(gate)
        {
            token.ThrowIfCancellationRequested();
            if(failed||next!=expectedNext||commands.Count!=prefix.Count||commands.Where((c,i)=>!ReferenceEquals(c,prefix[i])).Any())
                throw new InvalidOperationException("Actual global selection prefix changed before counter handoff.");
            //Unchanged07AAF0 numeric append owns its private entered flag and
            //retains earlier commands on allocation failure. It does not
            //apply selections or run selected C20.VF8.
            return AppendCounterCore(initial,next,preparedAllocationFailure,token);
        }
    }
}

/// <summary>Actual global post026EF0 operation7 append in the existing pending
/// C4D. Original registry replay, DOM transport and acceptance are separate.</summary>
public sealed class LegacyFreshMainGlobalCounterHandoff
{
    public LegacyFreshMainGlobalInputTransforms Transforms{get;}
    public LegacyOwnedFreshTextSourceContext TransactionOwner=>Transforms.Seed.PendingSelections;
    public LegacyOwnedFreshTextCounterCommand? CounterCommand{get;}
    public long InitialPendingCounter{get;}
    public long FinalPendingCounter{get;}
    public IReadOnlyList<LegacyOwnedFreshTextPendingCommand> Commands{get;}
    private LegacyFreshMainGlobalCounterHandoff(LegacyFreshMainGlobalInputTransforms transforms,LegacyPendingCounterAppend append)
    {
        Transforms=transforms;CounterCommand=append.Command;InitialPendingCounter=transforms.Seed.InputBinding.Next;
        FinalPendingCounter=append.Command?.NewCounter18??InitialPendingCounter;Commands=TransactionOwner.CopyPendingCommands();
    }
    public static LegacyFreshMainGlobalCounterHandoff Apply(LegacyFreshMainGlobalInputTransforms transforms,CancellationToken token=default)
        =>ApplyCore(transforms,false,token);
    internal static LegacyFreshMainGlobalCounterHandoff ApplyPreparedAllocationFailure(LegacyFreshMainGlobalInputTransforms transforms,CancellationToken token=default)
        =>ApplyCore(transforms,true,token);
    private static LegacyFreshMainGlobalCounterHandoff ApplyCore(LegacyFreshMainGlobalInputTransforms transforms,bool allocationFailure,CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(transforms);transforms.ValidateCurrent(token);
        var append=transforms.Seed.PendingSelections.AppendActualGlobalCounter(transforms,allocationFailure,token);
        if(!append.Succeeded)throw new InvalidOperationException("Global07AAF0 allocation failure retains its earlier pending prefix.");
        var result=new LegacyFreshMainGlobalCounterHandoff(transforms,append);result.ValidateCurrent(token);return result;
    }
    public void ValidateCurrent(CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();Transforms.ValidateCurrent(token);
        if(TransactionOwner.Failed||TransactionOwner.NextSelectionId!=FinalPendingCounter||Transforms.Seed.InputBinding.Next!=InitialPendingCounter)
            throw new InvalidOperationException("Actual global pending counter state changed.");
        var current=TransactionOwner.CopyPendingCommands();
        if(current.Count!=Commands.Count||current.Where((c,i)=>!ReferenceEquals(c.Owner,Commands[i].Owner)||
            !ReferenceEquals(c.Selection,Commands[i].Selection)||!ReferenceEquals(c.Counter,Commands[i].Counter)||
            c.CommandIdentity!=Commands[i].CommandIdentity||c.PreviousCommandIdentity!=Commands[i].PreviousCommandIdentity||c.NextCommandIdentity!=Commands[i].NextCommandIdentity).Any())
            throw new InvalidOperationException("Actual operation4/7 command identities or links changed.");
    }
}
