namespace InkRecognition;

/// <summary>INTERNAL PREPARED transport cursor; not a sealed N/D0 request.</summary>
internal sealed class LegacyEnglishPreparedQueryCursor
{
    internal LegacyEnglishFreshDictionaryQuerySession Owner{get;}
    internal LegacyPreparedDictionaryQuery Query{get;}
    internal bool Auxiliary{get;}
    internal LegacyEnglishPreparedQueryCursor(LegacyEnglishFreshDictionaryQuerySession owner,LegacyPreparedDictionaryQuery query,bool auxiliary){Owner=owner;Query=query;Auxiliary=auxiliary;}
}

public sealed partial class LegacyEnglishFreshDictionaryQuerySession
{
    //This entire partial is probe-only PREPARED transport. No production caller
    //can promote its explicit IDs/flags to an N request or D0 permission lease.
    private void ValidateMutable()
    {
        Validate(Scorer,LanguageQuery);
        if(faulted)throw new InvalidOperationException("Failed source-query session is poisoned.");
        if(active)throw new InvalidOperationException("Source-query transport is not reentrant.");
    }
    private T Prepared<T>(int maximumAttempts,int maximumOutputs,Action<string>? observer,CancellationToken token,Func<T> operation)
    {
        token.ThrowIfCancellationRequested();lock(gate)
        {
            token.ThrowIfCancellationRequested();ValidateMutable();
            //All work refusals precede operation entry and consume no counter.
            if(maximumAttempts<0||maximumOutputs<0||maximumAttempts>Bounds.MaximumAttemptedInsertions-attempts||maximumOutputs>Bounds.MaximumOutputRows-outputs||operations>=Bounds.MaximumOperations)throw new ArgumentException("Complete remaining transport work budget required before entry.");
            active=true;operations++;
            try{observer?.Invoke("prepared:enter");token.ThrowIfCancellationRequested();T result=operation();observer?.Invoke("prepared:complete");token.ThrowIfCancellationRequested();Validate(Scorer,LanguageQuery);return result;}
            catch{faulted=true;throw;}
            finally{active=false;}
        }
    }
    internal LegacyEnglishPreparedQueryCursor CreatePreparedCursor(int node,int capacity=128,ulong parent=0,bool rootPresent=true,bool auxiliary=false,ushort flags24=0,ushort flags26=0,byte category=0,int parameter1C=0,int parameter20=0,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();lock(gate)
        {
            ValidateMutable();if((uint)node>=(uint)LanguageQuery.DataOwner.Dictionary.NodeCount||capacity is<1 or>128)throw new ArgumentException("Actual node/bounded PREPARED page required.");
            if(parent!=0)_=contexts.ReadNode(parent);
            return Prepared(0,0,null,token,()=>new LegacyEnglishPreparedQueryCursor(this,new(){Node=node,Capacity=capacity,ParentContext=parent,ContextRoot=rootPresent?contexts.ContainerIdentity:0,Flags24=flags24,Flags26=flags26,Category18=category,Parameter1c=unchecked((uint)parameter1C),Parameter20=unchecked((uint)parameter20)},auxiliary));
        }
    }
    internal LegacyPreparedDictionaryQueryPage ReadPreparedPage(LegacyEnglishPreparedQueryCursor cursor,int skip=0,Action<string>? observer=null,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(cursor);if(!ReferenceEquals(cursor.Owner,this))throw new ArgumentException("PREPARED cursor belongs to another binding.");
        if(skip is<0 or>2_000_000)throw new ArgumentOutOfRangeException(nameof(skip));
        token.ThrowIfCancellationRequested();lock(gate)
        {
            ValidateMutable();if(cursor.Query.ParentContext!=0)_=contexts.ReadNode(cursor.Query.ParentContext);
            int maxAttempts=cursor.Query.ParentContext!=0&&cursor.Query.ContextRoot!=0?cursor.Query.Capacity:0;
            return Prepared(maxAttempts,cursor.Query.Capacity,observer,token,()=>
            {
            cursor.Query.Skip=skip;
            var page=querySource.Read(cursor.Query,cursor.Auxiliary,
                (parent,symbol)=>
                {
                    attempts++;var child=contexts.ResolveChild(parent,symbol,token);
                    observer?.Invoke("prepared:resolved");token.ThrowIfCancellationRequested();
                    if(child.Identity==0)throw new InvalidOperationException("Reserved whole insertion bound unexpectedly exhausted.");return child;
                },
                (child,parent)=>{contexts.LinkCreatedContext(child,parent);observer?.Invoke("prepared:linked");token.ThrowIfCancellationRequested();},true,token);
            outputs=checked(outputs+page.ReturnedCount);observer?.Invoke("prepared:page");token.ThrowIfCancellationRequested();return page;
            });
        }
    }
    internal ulong ResolvePreparedRoot(ushort symbol,Action<string>? observer=null,CancellationToken token=default)=>Prepared(1,0,observer,token,()=>
    {
        attempts++;var result=contexts.ResolveInitial(symbol,token);observer?.Invoke("prepared:resolved");token.ThrowIfCancellationRequested();if(result.Identity==0)throw new InvalidOperationException("Reserved initial lookup bound exhausted.");return result.Identity;
    });
    internal ulong ResolvePreparedChild(ulong parent,ushort symbol,Action<string>? observer=null,CancellationToken token=default)
    {
        //Foreign/stale IDs are refused before entering a mutating operation.
        token.ThrowIfCancellationRequested();lock(gate)
        {
            ValidateMutable();_=contexts.ReadNode(parent);
            return Prepared(1,0,observer,token,()=>
            {
                attempts++;var result=contexts.ResolveChild(parent,symbol,token);observer?.Invoke("prepared:resolved");token.ThrowIfCancellationRequested();if(result.Identity==0)throw new InvalidOperationException("Reserved child lookup bound exhausted.");
                if(result.Created){contexts.LinkCreatedContext(result.Identity,parent);observer?.Invoke("prepared:linked");token.ThrowIfCancellationRequested();}return result.Identity;
            });
        }
    }
    internal LegacyEnglishQuerySessionCheckpoint CapturePreparedCheckpoint(Action<string>? observer=null,CancellationToken token=default)
    {
        token.ThrowIfCancellationRequested();lock(gate)
        {
            ValidateMutable();if(checkpoints>=Bounds.MaximumCheckpoints)throw new ArgumentException("Whole retained checkpoint budget exhausted.");
            return Prepared(0,0,observer,token,()=>{var snapshot=contexts.Capture(token);checkpoints++;observer?.Invoke("prepared:captured");token.ThrowIfCancellationRequested();return new LegacyEnglishQuerySessionCheckpoint(this,snapshot);});
        }
    }
    internal void RestorePreparedCheckpoint(LegacyEnglishQuerySessionCheckpoint saved,Action<string>? observer=null,CancellationToken token=default)
    {
        ArgumentNullException.ThrowIfNull(saved);if(!ReferenceEquals(saved.Owner,this))throw new ArgumentException("Same binding checkpoint required.");
        _=Prepared(0,0,observer,token,()=>{contexts.Restore(saved.Pool,token);epoch=checked(epoch+1);observer?.Invoke("prepared:restored");token.ThrowIfCancellationRequested();return 0;});
        //Native pool restoration does NOT reset dictionary rings or working50.
        //Attempt/output/work counters are monotonic, never rewound as capacity.
    }
    internal void RestorePreparedBaseline(Action<string>? observer=null,CancellationToken token=default)=>RestorePreparedCheckpoint(baseline,observer,token);
    internal void ResetPreparedQueries(Action<string>? observer=null,CancellationToken token=default)
        =>_=Prepared(0,0,observer,token,()=>{querySource.ResetQueries();epoch=checked(epoch+1);observer?.Invoke("prepared:reset");token.ThrowIfCancellationRequested();return 0;});
    internal void RefreshPreparedPermissions(ReadOnlySpan<byte> permissions,Action<string>? observer=null,CancellationToken token=default)
    {
        if(permissions.Length!=217)throw new ArgumentException("Exact217 PREPARED permission bytes required.");token.ThrowIfCancellationRequested();byte[] snapshot=permissions.ToArray();
        _=Prepared(0,0,observer,token,()=>{var next=working.Refresh(snapshot,token);working=next;querySource.SetWorkingMap(next);epoch=checked(epoch+1);observer?.Invoke("prepared:refreshed");token.ThrowIfCancellationRequested();return 0;});
        //Only working50 changes. Source68 stays the fresh092990 context map;
        //query invalidation is separate. Actual D0 install awaits sealed receipt.
    }
}
