using System.Buffers.Binary;
[assembly:System.Runtime.CompilerServices.InternalsVisibleTo("LegacyEnglishTop.ActualEntry")]
namespace InkRecognition;

internal sealed partial class LegacyEnglishCaseNode0Completion
{
    // This export is DATA. Neither normalized singleton mass nor Top80 is
    // handwriting admission, and this stage has not published native TEXT.
    internal sealed record GraphTopData(byte[] Constructor400,byte[] Scored400,
        byte[] Normalized400,int TerminalOrdinal,int TotalCost,int RowCost,
        int NodeCount,int SelectedWordOrdinal,int WordChoices,float RawMass,float Top80);
    internal sealed record GraphTopResult(GraphWordSourceResult Source,GraphTopData Top);
    private bool graphTopRequested;
    private GraphTopOwner? graphTopOwner;

    internal GraphTopResult RunGraphFirstTop(CancellationToken token=default,Action<GraphMainConfidenceObservation>? observer=null,bool captureSourceInputs=false)
    {
        primary.RequireGraphLoop(this,token);
        if(graphTopRequested||graphTopOwner is not null)throw new InvalidOperationException("One same-owner TOP operation required.");
        graphTopRequested=true;
        var source=RunGraphFirstWordSourceCompletion(token,observer,captureSourceInputs);
        var top=graphTopOwner??throw new InvalidOperationException("No actual EN TOP owner.");
        if(Volatile.Read(ref phase)!=2)throw new InvalidOperationException("Completed graph required.");
        return new(source,top.CopyCompleted(token));
    }

    private void PrepareGraphTop(GraphLanguageScope stage,LegacyNativePlainWordPublisher publisher,
        LegacyNativeGlyphRecordPool pool,LegacyPreparedMainTerminalWinner winner,CancellationToken token)
    {
        RequireGraphPublication(stage,token);
        if(graphTopOwner is not null||!graphTopRequested||graphWordSourceSession is not null)
            throw new InvalidOperationException("Fresh pre-source TOP owner required.");
        // Fixed reservation covers three400B images, detached copies, holder,
        // score result and constructor temporaries. It does not claim CLR RSS.
        stage.Ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.FinalImages,8192);
        stage.Ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.FinalImages,16384);
        graphTopOwner=new(this,stage,publisher,pool,winner,token);
    }

    private sealed class GraphTopOwner
    {
        private readonly LegacyEnglishCaseNode0Completion owner;
        private readonly GraphLanguageScope stage;
        private readonly LegacyNativePlainWordPublisher publisher;
        private readonly LegacyNativeGlyphRecordPool pool;
        private readonly LegacyNativePublishedWordNode head;
        private readonly byte[] constructor,scored,normalized;
        private readonly LegacyPreparedMainTerminalWinner winner;
        private readonly int nodes;
        private readonly float mass,top80;
        private int complete;
        private int selected,choices;
        internal GraphTopOwner(LegacyEnglishCaseNode0Completion owner,GraphLanguageScope stage,
            LegacyNativePlainWordPublisher publisher,LegacyNativeGlyphRecordPool pool,
            LegacyPreparedMainTerminalWinner winner,CancellationToken token)
        {
            this.owner=owner;this.stage=stage;this.publisher=publisher;this.pool=pool;this.winner=winner;
            owner.RequireGraphPublication(stage,token);
            head=publisher.Head??throw new NotSupportedException("Actual positive279 head required.");
            _=publisher.ReadFreshSingletonTopScalars(head,pool,token);
            if(!winner.HasWinner||head.Next is not null||!ReferenceEquals(head.Word.Pool,pool))
                throw new NotSupportedException("One genuine selected result and same-pool head required.");
            var source=stage.Lease.Root.Source;
            nodes=stage.Lease.Pass.Initializer.PlanForAudit.BindingForAudit.Graph.NodeCount;
            if(source.SourceKind!=0x135||nodes<1)throw new NotSupportedException("Actual EN source135 required.");
            constructor=new byte[400];
            LegacyOwnedMainFirstAlternativeSnapshot.ConstructRow40(constructor.AsSpan(0,40),token);
            // The first genuine terminal establishes fresh reference78. Pointer
            // fields retain portable zero; this object carries both live owners.
            long reference=LegacyPreparedResultReferencePolicy.UpdateReferenceCost(0,winner.TotalCost,0,true,false,false,token);
            if(reference!=winner.TotalCost)throw new InvalidDataException("Fresh first reference differs.");
            scored=constructor.ToArray();
            LegacyMainResultScore.WriteCostChannels(scored.AsSpan(0,40),
                new(winner.TotalCost,checked((int)reference),nodes,winner.RowCost14,source.SourceKind),token).CopyTo(scored,0);
            var result=LegacyMainResultScorePublication.Normalize(scored,1,-1f,token);
            normalized=result.CopyStorage();mass=result.RawFirstMass08!.Value;top80=result.Top80;
            owner.RequireGraphPublication(stage,token);
        }
        internal void Complete(CancellationToken token)
        {
            owner.RequireGraphPublication(stage,token);
            if(complete!=0||!ReferenceEquals(publisher.Head,head)||!ReferenceEquals(head.Word.Pool,pool)||
                owner.graphWordSourceSession is null||
                !ReferenceEquals(owner.graphTopOwner,this))throw new InvalidOperationException("Same successful source callback and live TOP required.");
            owner.graphWordSourceSession.RequireCompletedForTop(publisher,pool,token);
            var span=publisher.ReadFreshSingletonTopScalars(head,pool,token);
            selected=span.Selected;choices=span.Count;
            owner.RequireGraphPublication(stage,token);Volatile.Write(ref complete,1);
        }
        internal GraphTopData CopyCompleted(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if(Volatile.Read(ref complete)!=1||!ReferenceEquals(owner.graphTopOwner,this)||
                !ReferenceEquals(publisher.Head,head))throw new InvalidOperationException("Completed private TOP required.");
            if(!ReferenceEquals(head.Word.Pool,pool))throw new InvalidOperationException("Completed pool changed.");
            var current=publisher.ReadFreshSingletonTopScalars(head,pool,token);
            if(current.Count!=choices||current.Selected!=selected)
                throw new InvalidOperationException("Completed selected275 identity changed.");
            return new(constructor.ToArray(),scored.ToArray(),normalized.ToArray(),winner.Ordinal,
                winner.TotalCost,winner.RowCost14,nodes,selected,choices,mass,top80);
        }
    }
}
