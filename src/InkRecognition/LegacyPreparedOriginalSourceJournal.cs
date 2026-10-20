namespace InkRecognition;

// Library representation of the bounded original C51/source-journal state.
// No native pointers, allocator buckets or Base70 objects are synthesized.
internal sealed class LegacyPreparedOriginalSourceJournal
{
    internal readonly List<LegacyLibraryOriginalSourceRegistryEntry> Entries=[];
    internal readonly List<LegacyLibraryOriginalSourceJournalRecord> Journal=[];
    internal readonly List<string> Trace=[];
    private readonly LegacyLibraryOriginalSourceHistory owner;
    internal long Next, Maximum;
    internal bool MaximumDirty, Recording=true, JournalComplete=true;
    internal int Completed, Reversed;
    internal LegacyPreparedOriginalSourceJournal(LegacyLibraryOriginalSourceHistory owner)
    {
        this.owner=owner;var initial=owner.Original.State;Next=initial.NextRegistryId;Maximum=initial.MaximumRegistryId;
        foreach(var e in initial.Inputs)Entries.Add(new(owner,e.RegistryId,e.Name,(int)e.Category,e.RetainedSelection,null));
    }
    internal void Add(LegacyOwnedFreshTextSelection command,bool observerRefusal,CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        //078090's temporary C22 clone is distinct from the observer's clone.
        var temporary=new LegacyLibraryOriginalInkSelection(owner.Original,command.CopyRanges());
        try
        {
        LegacyLibraryOriginalInkSelection? historyClone=null;
        if(observerRefusal)
        {
            //0797F0 returns void after clearing the ENTIRE current110/118 frame.
            //0653D0/06BE30 still proceed to original registration.
            Journal.Clear();JournalComplete=false;Trace.Add("observer90:refused:clear-frame");
        }
        else historyClone=new(owner.Original,temporary.CopyRanges());
        var data=temporary.CopyRanges();
        var interned=Entries.Select(e=>e.RangesOwner).FirstOrDefault(r=>r.CopyRanges().AsSpan().SequenceEqual(data))??temporary;
        if(Entries.Any(e=>e.RegistryId==command.SelectionId))throw new InvalidOperationException("Fresh original ID already exists.");
        Entries.Add(new(owner,command.SelectionId,command.Name,command.Category,interned,command));
        Next=Math.Max(Next,checked(command.SelectionId+1));Maximum=Math.Max(Maximum,command.SelectionId);
        Trace.Add("register4:"+command.SelectionId);
        //13CB50 state0 events are removed before any original DOM rewrite.
        Trace.Add("CB7:event0:discard:"+command.SelectionId);
        if(historyClone is not null)Journal.Add(new(owner,command,historyClone));
        }
        catch
        {
            //An unexpected managed exception can occur after registration but
            //before journal append. Never authorize an incomplete inverse.
            JournalComplete=false;throw;
        }
    }
    internal void Counter(LegacyOwnedFreshTextCounterCommand command)
    {
        if(Next>=command.NewCounter18){Trace.Add("counter7:skip");return;}
        //The actual contiguous producer must reach its final counter via adds.
        //A non-skipped setter needs its real generated observer7, not a dummy.
        throw new NotSupportedException("This source-only owner requires the actual trailing counter setter to be skipped.");
    }
    internal void Delete(LegacyLibraryOriginalSourceJournalRecord record)
    {
        int index=Entries.FindIndex(e=>e.RegistryId==record.RegistryId);
        if(index<0){Trace.Add("delete4:missing:"+record.RegistryId);return;}
        var entry=Entries[index];
        if(entry.SourceCommand is null||!ReferenceEquals(entry.SourceCommand,record.SourceCommand))
            throw new NotSupportedException("Rollback cannot delete an initial or foreign original INPUT.");
        Trace.Add("C4D:B0:skip-recording0");
        Trace.Add("CB7:event2:"+record.RegistryId);
        Entries.RemoveAt(index);if(record.RegistryId==Maximum)MaximumDirty=true;
        Trace.Add("delete4:"+record.RegistryId);
        //All original tags are checked < initial Next; every new event >=Next.
        //After callbacks run in REVERSE attachment order: CB7 precedes C4D.
        Trace.Add("CB7:no-original-tag-match:"+record.RegistryId);
        Trace.Add("CB7:clear-events");
        Trace.Add("C4D:B8:skip-recording0");
    }
    internal static bool ReverseCounter(long next,long maximum,long oldValue,long newValue,out long result)
    {
        result=next;if(oldValue==-1||newValue==-1)return false;
        if(maximum<oldValue)result=oldValue;return true;
    }
}
