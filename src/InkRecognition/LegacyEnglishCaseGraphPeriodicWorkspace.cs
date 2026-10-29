using System.Buffers.Binary;
namespace InkRecognition;
internal sealed partial class LegacyEnglishCaseNode0Completion
{
    private GraphPeriodicWorkspace? graphPeriodicWorkspace;
    internal (int Node,int Span) NextActualGraphSpan(LegacyEnglishFreshCaseMainSource.Node1QueryLease lease,bool last)
    {
        RequireGraphLoopSpan(lease.Node,lease.Span);if(!ReferenceEquals(graphLoopLease,lease))throw new InvalidOperationException("Same actual graph span required.");
        return graphPeriodicActive?graphPeriodicWorkspace!.NextSpan(lease,last):LegacyEnglishCaseGraphSchedule.AfterSpan(lease.Node,lease.Span,last);
    }
    internal GraphPeriodicWorkspace RequirePeriodicWorkspace(GraphLanguageScope scope)
    {scope.RequirePhase1();if(!graphPeriodicActive||graphPeriodicWorkspace is null||!ReferenceEquals(scope.Owner,this))throw new InvalidOperationException("Same periodic workspace required.");return graphPeriodicWorkspace;}
    internal sealed class GraphPeriodicWorkspace
    {
        private readonly LegacyEnglishCaseNode0Completion owner;
        private readonly LegacyEnglishCaseInitialQueryPass pass;
        private readonly int issuedThread;
        private int node=-1,state,maximumSpan=-1; //0issued,1queryheap,2primary materialization,3language descriptor,4node reset completed,5closed
        private readonly byte[] slots,primary,rows,alternatives,pivot,temporary;
        private readonly int[] partitions,phase1Partitions,ordinals;
        private readonly LegacyNativeWordRecordComparer comparer;
        internal readonly LegacyNativeWordRecordHeap Heap;
        internal readonly LegacyNativeWordHeapDescriptor Descriptor;
        internal LegacyNativeWordHeapQuotaLimits Limits{get;}
        private GraphPeriodicWorkspace(LegacyEnglishCaseNode0Completion owner,LegacyNativeWordHeapQuotaLimits limits,CancellationToken token)
        {
            this.owner=owner;pass=owner.node1Pass!;issuedThread=Environment.CurrentManagedThreadId;Limits=limits;
            slots=owner.ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.Phase1Working,250*128);
            primary=owner.ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.Phase1Working,250*80);
            rows=owner.ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.Phase1Working,125*88);
            alternatives=owner.ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.Phase1Working,125*80);
            pivot=owner.ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.Phase1Working,128);
            temporary=owner.ledger.Bytes(LegacyEnglishCaseNode0Ledger.Site.Phase1Working,128);
            partitions=owner.ledger.Ints(LegacyEnglishCaseNode0Ledger.Site.Phase1Working,500);
            phase1Partitions=owner.ledger.Ints(LegacyEnglishCaseNode0Ledger.Site.Phase1Working,248);
            ordinals=owner.ledger.Ints(LegacyEnglishCaseNode0Ledger.Site.OrdinalVectors,156);
            owner.ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,160);
            owner.ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,160);
            comparer=new(LegacyNativeWordRecordComparer.Select(false,false,1,false),[0,0xc0e]);
            Heap=new(this);Descriptor=new(this);Require(token);
        }
        internal static GraphPeriodicWorkspace Issue(LegacyEnglishCaseNode0Completion owner,LegacyNativeWordHeapQuotaLimits limits,CancellationToken token)
        {
            token.ThrowIfCancellationRequested();if(!owner.graphPeriodicActive||!owner.graphLoopActive||owner.graphPeriodicWorkspace is not null||owner.node1Pass is null||
                limits.PrimaryInitialCapacity20!=250||limits.SecondaryInitialCapacity2c!=124)
                throw new InvalidOperationException("One same-owner native250/124 periodic workspace required.");
            owner.ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,256);
            owner.ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,256);
            var value=new GraphPeriodicWorkspace(owner,limits,token);owner.graphPeriodicWorkspace=value;return value;
        }
        private void Require(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();if(!owner.graphPeriodicActive||!owner.graphLoopActive||owner.thread!=issuedThread||issuedThread!=Environment.CurrentManagedThreadId||
                Volatile.Read(ref owner.phase)!=1||!ReferenceEquals(owner.node1Pass,pass)||state==5)
                throw new InvalidOperationException("Same periodic workspace issuer/pass/thread/state required.");
        }
        internal byte[] AttachHeap(LegacyNativeWordRecordHeap actual)
        {Require(default);if(state!=0||Heap is not null)throw new InvalidOperationException("Heap attaches once.");return slots;}
        internal byte[] AttachDescriptor(LegacyNativeWordHeapDescriptor actual)
        {Require(default);if(state!=0||Descriptor is not null)throw new InvalidOperationException("Descriptor attaches once.");return slots;}
        internal void AuthenticateHeap(LegacyNativeWordRecordHeap actual,byte[] bytes,CancellationToken token)
        {Require(token);if(!ReferenceEquals(owner.graphPeriodicWorkspace,this)||!ReferenceEquals(Heap,actual)||!ReferenceEquals(bytes,slots)||state!=1)throw new InvalidOperationException("Same actual query heap backing required.");}
        internal void AuthenticateDescriptor(LegacyNativeWordHeapDescriptor actual,byte[] bytes,CancellationToken token)
        {Require(token);if(!ReferenceEquals(owner.graphPeriodicWorkspace,this)||!ReferenceEquals(Descriptor,actual)||!ReferenceEquals(bytes,slots)||state is not(1 or 2 or 3))throw new InvalidOperationException("Same actual periodic descriptor backing required.");}
        internal void BeginNode(int current,CancellationToken token)
        {
            Require(token);if(!ReferenceEquals(owner.graphPeriodicWorkspace,this)||state is not(0 or 4)||current!=(node==-1?1:node+1)||current!=owner.graphLoopNode||owner.ring!.CurrentNode!=current-1)
                throw new InvalidOperationException("Only next actual node after sourceV80 may reset the private workspace.");
            node=current;state=1;maximumSpan=-1;
            //212D80 phase0 writes count0, then1F73D0 restores descriptor+20 capacity. Retained250 backing is NOT cleared.
            owner.ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Descriptor,12);Heap.ResetPeriodic(this,token);Descriptor.ResetPeriodicQueries(this,token);
            // Separate portable materializer scratch returns to the same explicit zero-owned residue used by the old helper.
            // This does not clear native slots or claim a native whole-allocation reset.
            owner.ledger.Fill(LegacyEnglishCaseNode0Ledger.Site.Phase1Working,primary,0);
            owner.ledger.Fill(LegacyEnglishCaseNode0Ledger.Site.Phase1Working,rows,0);
            owner.ledger.Fill(LegacyEnglishCaseNode0Ledger.Site.Phase1Working,alternatives,0);
            owner.ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OrdinalVectors,156*4);Array.Fill(ordinals,-1);
        }
        internal int MaximumSpan(CancellationToken token)
        {
            RequireQueryHeap(Heap,token);var graph=pass.Initializer.PlanForAudit.BindingForAudit.Graph;
            var arcs=graph.GetNode(node);if(node is<1 or>119||arcs.Count is<1 or>1024)throw new NotSupportedException("Actual bounded periodic graph node/arcs required.");
            int maximum=0;foreach(var arc in arcs){Visit(32);if(arc.Span>=8||arc.Span>node)throw new NotSupportedException("Actual periodic span0..7 required.");maximum=Math.Max(maximum,arc.Span);}
            maximumSpan=maximum;return maximum;
        }
        internal int PreviousNode(byte span,CancellationToken token)
        {RequireQueryHeap(Heap,token);if(maximumSpan<0||span>maximumSpan||span!=owner.graphLoopSpan)throw new InvalidOperationException("Actual current ordered periodic span required.");return node-span-1;}
        internal (int Node,int Span) NextSpan(LegacyEnglishFreshCaseMainSource.Node1QueryLease lease,bool last)
        {
            Require(default);if(!ReferenceEquals(owner.graphLoopLease,lease)||lease.Node!=node||lease.Span!=owner.graphLoopSpan||last!=(lease.Span==maximumSpan))
                throw new InvalidOperationException("Same actual periodic source span/end receipt required.");
            return last?(node+1,0):(node,lease.Span+1);
        }
        internal void EndNode(CancellationToken token)
        {Require(token);if(state!=3||node!=owner.graphLoopNode||owner.ring!.CurrentNode!=node||owner.graphLoopLease is not null)throw new InvalidOperationException("Same completed node and closed source span required.");state=4;}
        internal void Close(){state=5;}
        internal void RequireQueryHeap(LegacyNativeWordRecordHeap actual,CancellationToken token)
        {Require(token);if(state!=1||!ReferenceEquals(Heap,actual)||!ReferenceEquals(owner.graphPeriodicWorkspace,this))throw new InvalidOperationException("Same active actual periodic query heap required.");Visit(32);}
        internal void ChargeHeapSnapshot(LegacyNativeWordRecordHeap actual,int bytes)
        {RequireQueryHeap(actual,default);owner.ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.Export,bytes);owner.ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Export,bytes);}
        internal void Visit(long n)=>owner.ledger.Visit(LegacyEnglishCaseNode0Ledger.Site.Phase1Working,n);
        internal void Copy(long n)=>owner.ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.Phase1Working,n);
        private ReadOnlySpan<byte> Slot(int index){Visit(128);return slots.AsSpan(index*128,128);}
        private int Compare(int left,ReadOnlySpan<byte> right){Visit(256);return comparer.Compare(Slot(left),right);}
        private void Swap(int left,int right)
        {if(left==right)return;Copy(384);slots.AsSpan(left*128,128).CopyTo(temporary);slots.AsSpan(right*128,128).CopyTo(slots.AsSpan(left*128));temporary.CopyTo(slots.AsSpan(right*128));}
        internal LegacyEnglishCaseNode1Materialization.Images Materialize(LegacyNativeWordHeapPreparation preparation,LegacySymbolContextFactory contexts,
            GraphPeriodicRows parents,out int minimum,CancellationToken token)
        {
            Require(token);parents.RequireWorkspace(this,token);if(state!=1||preparation.Configuration.PrimaryArcLimit230a!=125||preparation.Configuration.PrimaryAlternativeCount2402!=2||preparation.Configuration.SecondaryAlternativeCount2404!=1)
                throw new InvalidOperationException("Actual periodic primary125/2/1 materialization required.");
            int initial=Heap.Count;state=2;minimum=0x4000000;
            for(int index=0;index<initial;index++)
            {
                token.ThrowIfCancellationRequested();Visit(128);Copy(4);var slot=slots.AsSpan(index*128,128);BinaryPrimitives.WriteInt32LittleEndian(slot[88..],index);
                int previous=unchecked(node-(sbyte)slot[32]-1);ushort parent=BinaryPrimitives.ReadUInt16LittleEndian(slot[30..]);
                if(previous==-1)Root(slot,contexts,token);else
                {
                    if(previous<0||parent>=125)throw new NotSupportedException("Periodic primary materializer supports only actual root/primary-parent routes.");
                    Parent(slot,parents.Record(previous,parent,token),parents.Primary(previous,parent,token),contexts,token);
                }
            }
            Sort(initial,token);int unique=initial==0?0:1;
            for(int index=1;index<initial;index++)
            {
                token.ThrowIfCancellationRequested();int leader=unique-1;
                if(Compare(leader,Slot(index))==0)
                {
                    if(BinaryPrimitives.ReadInt32LittleEndian(Slot(index)[16..])<BinaryPrimitives.ReadInt32LittleEndian(Slot(leader)[16..]))Swap(leader,index);
                    MergePrimary(Slot(leader),Slot(index));
                }
                else {if(unique!=index){Copy(128);slots.AsSpan(index*128,128).CopyTo(slots.AsSpan(unique*128));}unique++;}
            }
            Descriptor.BeginPeriodicMaterialization(this,0,token);
            for(int index=0;index<unique;index++)
            {token.ThrowIfCancellationRequested();Copy(128);slots.AsSpan(index*128,128).CopyTo(temporary);if(!Descriptor.AllocatePeriodicSnapshot(this,temporary,token))throw new InvalidDataException("Actual primary unique rebuild append refused.");}
            Descriptor.ApplyPeriodicPrimaryQuota(this,token);
            for(int i=0;i<125;i++)
            {
                Copy(16);BinaryPrimitives.WriteInt32LittleEndian(rows.AsSpan(i*88+16),0x4000000);BinaryPrimitives.WriteInt32LittleEndian(rows.AsSpan(i*88+20),0x4000000);
                BinaryPrimitives.WriteInt32LittleEndian(alternatives.AsSpan(i*80),0x4000000);BinaryPrimitives.WriteInt32LittleEndian(alternatives.AsSpan(i*80+4),0x4000000);
            }
            for(int i=0;i<Descriptor.Count;i++)
            {
                token.ThrowIfCancellationRequested();var slot=Slot(i);int ordinal=BinaryPrimitives.ReadInt32LittleEndian(slot[88..]);
                if((uint)ordinal>=250)throw new InvalidDataException("Actual primary pool ordinal required.");
                Copy(174);slot[..88].CopyTo(rows.AsSpan(i*88));if(BinaryPrimitives.ReadUInt16LittleEndian(rows.AsSpan(i*88+60))==ushort.MaxValue)
                    BinaryPrimitives.WriteUInt16LittleEndian(rows.AsSpan(i*88+60),(ushort)i);
                primary.AsSpan(ordinal*80,80).CopyTo(alternatives.AsSpan(i*80));ordinals[i]=ordinal;
                minimum=Math.Min(minimum,BinaryPrimitives.ReadInt32LittleEndian(slot[16..]));
            }
            state=3;owner.ledger.Allocate(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,64);owner.ledger.Copy(LegacyEnglishCaseNode0Ledger.Site.OwnerMetadata,64);
            return new(slots,rows,alternatives,Descriptor.Count,initial,Descriptor,ordinals);
        }
        private void Sort(int count,CancellationToken token)
        {
            if(count<2)return;int top=0;Push(0,count-1);
            while(top!=0)
            {
                token.ThrowIfCancellationRequested();int upper=partitions[--top],lower=partitions[--top],left=lower,right=upper;
                Copy(128);slots.AsSpan((lower+(upper-lower)/2)*128,128).CopyTo(pivot);
                do
                {
                    while(Compare(left,pivot)<0)if(++left>upper)throw new InvalidDataException("Actual primary left pivot escaped.");
                    while(Compare(right,pivot)>0)if(--right<lower)throw new InvalidDataException("Actual primary right pivot escaped.");
                    if(right<left)break;Swap(left,right);left++;right--;
                }while(left<=right);
                if(left<upper)Push(left,upper);if(lower<right)Push(lower,right);
            }
            void Push(int lo,int hi){if(top>partitions.Length-2)throw new InvalidDataException("Fixed250 partition scratch exhausted.");Copy(8);partitions[top++]=lo;partitions[top++]=hi;}
        }
        internal byte[] Phase1Pivot(GraphLanguageScope scope){RequireScope(scope);return pivot;}
        internal byte[] Phase1Temporary(GraphLanguageScope scope){RequireScope(scope);return temporary;}
        internal int[] Phase1Partitions(GraphLanguageScope scope){RequireScope(scope);return phase1Partitions;}
        private void RequireScope(GraphLanguageScope scope)
        {Require(default);scope.RequirePhase1();if(state!=3||!ReferenceEquals(scope.Owner,owner)||!ReferenceEquals(scope.Descriptor,Descriptor)||scope.CurrentNode!=node)throw new InvalidOperationException("Same language descriptor workspace required.");}
        private readonly record struct Selection(int Row,int Index,ulong Identity);
        private Span<byte> Alternative(int row,int index)=>primary.AsSpan(row*80+index*40,40);
        private void MergePrimary(ReadOnlySpan<byte> leaderSlot,ReadOnlySpan<byte> duplicateSlot)
        {
            Visit(256);int leader=BinaryPrimitives.ReadInt32LittleEndian(leaderSlot[88..]),duplicate=BinaryPrimitives.ReadInt32LittleEndian(duplicateSlot[88..]);
            var first=Alternative(leader,0);var second=Alternative(duplicate,0);bool heapCosts=(first[31]&second[31]&32)!=0;
            int firstCost=BinaryPrimitives.ReadInt32LittleEndian(heapCosts?leaderSlot[16..]:first),secondCost=BinaryPrimitives.ReadInt32LittleEndian(heapCosts?duplicateSlot[16..]:second);
            bool leaderFirst=firstCost<secondCost;int left=leaderFirst?1:0,right=leaderFirst?0:1;Span<Selection> selected=stackalloc Selection[2];
            int seed=leaderFirst?leader:duplicate;selected[0]=new(seed,0,BinaryPrimitives.ReadUInt64LittleEndian(Alternative(seed,0)[8..]));int count=1;
            while(count<2)
            {
                Visit(128);bool la=left<2&&BinaryPrimitives.ReadInt32LittleEndian(Alternative(leader,left))<0x4000000;
                bool ra=right<2&&BinaryPrimitives.ReadInt32LittleEndian(Alternative(duplicate,right))<0x4000000;if(!la&&!ra)break;
                bool takeLeft=la&&(!ra||BinaryPrimitives.ReadInt32LittleEndian(Alternative(leader,left))<BinaryPrimitives.ReadInt32LittleEndian(Alternative(duplicate,right)));
                int row=takeLeft?leader:duplicate,index=takeLeft?left++:right++;ulong id=BinaryPrimitives.ReadUInt64LittleEndian(Alternative(row,index)[8..]);
                bool exists=false;for(int i=0;i<count;i++)if(selected[i].Identity==id){exists=true;break;}if(!exists)selected[count++]=new(row,index,id);
            }
            for(int i=count-1;i>=0;i--)
            {
                var s=selected[i];if(s.Row==leader&&s.Index==i)continue;Copy(30);var input=Alternative(s.Row,s.Index);var output=Alternative(leader,i);
                input[..8].CopyTo(output);output[30]=input[30];input.Slice(24,4).CopyTo(output[24..]);input.Slice(8,16).CopyTo(output[8..]);output[31]=(byte)((output[31]&~0x3f)|(input[31]&0x3f));
            }
        }
        private ulong Mapped(LegacySymbolContextFactory contexts,ulong prefix,ulong suffix,CancellationToken token)
        {owner.graphPeriodicContextBudget!.BeforeComposition(suffix);return LegacyNativeWordContextComposition.CombineMapped(contexts,prefix,suffix,static s=>s,token);}
        private ulong Raw(LegacySymbolContextFactory contexts,ulong prefix,ulong suffix,CancellationToken token)
        {owner.graphPeriodicContextBudget!.BeforeComposition(suffix);return LegacyNativeWordContextComposition.CombineRaw(contexts,prefix,suffix,token);}
        private void Root(ReadOnlySpan<byte> slot,LegacySymbolContextFactory contexts,CancellationToken token)
        {
            int ordinal=BinaryPrimitives.ReadInt32LittleEndian(slot[88..]);var row=primary.AsSpan(ordinal*80,80);var first=row[..40];
            //20E7B1..20E7D0 header stores precede root composition at20E8C1/20E8D6.
            Copy(4);slot.Slice(30,2).CopyTo(first[24..]);first[26]=slot[32];first[31]=(byte)((first[31]&0xc0)|(slot[33]&0x1f));
            ulong suffix=BinaryPrimitives.ReadUInt64LittleEndian(slot[8..]);ulong mapped=Mapped(contexts,0,suffix,token),raw=Raw(contexts,0,suffix,token);
            //20E906..20E92A: mapped,raw,cost,cost14,bit20,metadata1E.
            Copy(26);BinaryPrimitives.WriteUInt64LittleEndian(first[8..],mapped);BinaryPrimitives.WriteUInt64LittleEndian(first[16..],raw);
            slot.Slice(16,4).CopyTo(first);slot.Slice(20,4).CopyTo(first[4..]);first[31]|=0x20;first[30]=slot[38];
            Copy(9);BinaryPrimitives.WriteInt32LittleEndian(row[40..],0x4000000);BinaryPrimitives.WriteInt32LittleEndian(row[44..],0x4000000);row[70]=0;
        }
        private void Parent(ReadOnlySpan<byte> slot,ReadOnlySpan<byte> parent,ReadOnlySpan<byte> parentPrimary,LegacySymbolContextFactory contexts,CancellationToken token)
        {
            Visit(296);int ordinal=BinaryPrimitives.ReadInt32LittleEndian(slot[88..]);var row=primary.AsSpan(ordinal*80,80);
            // Shared native header precedes branch-specific parent dereferences/composition.
            Copy(4);slot.Slice(30,2).CopyTo(row[24..]);row[26]=slot[32];row[31]=(byte)((row[31]&0xc0)|(slot[33]&0x1f));
            ulong source=BinaryPrimitives.ReadUInt64LittleEndian(parent[8..]);if(source==0)throw new InvalidDataException("Actual parent source context required.");
            int delta=unchecked(BinaryPrimitives.ReadInt32LittleEndian(slot[16..])-BinaryPrimitives.ReadInt32LittleEndian(parent[16..]));
            int increment=unchecked(BinaryPrimitives.ReadInt32LittleEndian(slot[20..])-BinaryPrimitives.ReadInt32LittleEndian(parent[20..]));Copy(1);row[27]&=0x80;
            bool reset=LegacyNativeInitialParentWordAlternativeProducer.ResetsPrimaryContext(contexts.ReadNode(source).Symbol);
            bool append=BinaryPrimitives.ReadUInt16LittleEndian(slot[24..])==node||(sbyte)slot[5]>=0;ulong suffix=BinaryPrimitives.ReadUInt64LittleEndian(slot[8..]);
            if(reset)
            {
                //20EF67/20EF7C finish both composition calls before20EF87..20EFA2 stores.
                ulong mapped=Mapped(contexts,0,suffix,token),raw=Raw(contexts,0,suffix,token);
                Copy(26);BinaryPrimitives.WriteUInt64LittleEndian(row[16..],raw);BinaryPrimitives.WriteInt32LittleEndian(row,delta);
                BinaryPrimitives.WriteUInt64LittleEndian(row[8..],mapped);BinaryPrimitives.WriteInt32LittleEndian(row[4..],increment);row[31]|=0x20;row[30]=slot[38];
                Copy(9);BinaryPrimitives.WriteInt32LittleEndian(row[40..],0x4000000);BinaryPrimitives.WriteInt32LittleEndian(row[44..],0x4000000);row[70]=0;
            }
            else for(int k=0;k<2;k++)
            {
                var input=parentPrimary.Slice(k*40,40);var output=row.Slice(k*40,40);Visit(40);
                if(BinaryPrimitives.ReadInt32LittleEndian(input)>=0x4000000){Copy(9);BinaryPrimitives.WriteInt32LittleEndian(output,0x4000000);BinaryPrimitives.WriteInt32LittleEndian(output[4..],0x4000000);output[30]=0;continue;}
                Copy(13);BinaryPrimitives.WriteInt32LittleEndian(output,unchecked(BinaryPrimitives.ReadInt32LittleEndian(input)+delta));BinaryPrimitives.WriteInt32LittleEndian(output[4..],unchecked(BinaryPrimitives.ReadInt32LittleEndian(input[4..])+increment));
                slot.Slice(30,2).CopyTo(output[24..]);output[26]=slot[32];output[27]=(byte)((output[27]&0x80)|k);output[30]=slot[38];
                ulong mapped=BinaryPrimitives.ReadUInt64LittleEndian(input[8..]),raw=BinaryPrimitives.ReadUInt64LittleEndian(input[16..]);if(append){mapped=Mapped(contexts,mapped,suffix,token);raw=Raw(contexts,raw,suffix,token);}
                Copy(17);BinaryPrimitives.WriteUInt64LittleEndian(output[8..],mapped);BinaryPrimitives.WriteUInt64LittleEndian(output[16..],raw);output[31]=(byte)((output[31]&0xc0)|(slot[33]&0x1f));
            }
        }
    }
}
public sealed partial class LegacyNativeWordRecordHeap
{
    internal LegacyNativeWordRecordHeap(LegacyEnglishCaseNode0Completion.GraphPeriodicWorkspace workspace)
    {Capacity=250;slots=workspace.AttachHeap(this);periodicWorkspace=workspace;}
    internal void ResetPeriodic(LegacyEnglishCaseNode0Completion.GraphPeriodicWorkspace workspace,CancellationToken token)
    {workspace.AuthenticateHeap(this,slots,token);if(mutating)throw new InvalidOperationException("Active heap reset refused.");Count=0;}
}
public sealed partial class LegacyNativeWordHeapDescriptor
{
    internal LegacyNativeWordHeapDescriptor(LegacyEnglishCaseNode0Completion.GraphPeriodicWorkspace workspace)
    {backing=workspace.AttachDescriptor(this);Capacity=250;Count=0;StorageTag10=0;Limits=workspace.Limits;}
    internal void ResetPeriodicQueries(LegacyEnglishCaseNode0Completion.GraphPeriodicWorkspace workspace,CancellationToken token)
    {workspace.AuthenticateDescriptor(this,backing,token);if(resizing||mutating||AllocatedCapacity!=250)throw new InvalidOperationException("Idle retained250 descriptor required.");Count=0;Capacity=250;}
    internal void BeginPeriodicMaterialization(LegacyEnglishCaseNode0Completion.GraphPeriodicWorkspace workspace,int count,CancellationToken token)
    {workspace.AuthenticateDescriptor(this,backing,token);if(resizing||mutating||(uint)count>250)throw new InvalidOperationException("Actual idle retained250 descriptor required.");Capacity=250;Count=count;}
    internal bool AllocatePeriodicSnapshot(LegacyEnglishCaseNode0Completion.GraphPeriodicWorkspace workspace,byte[] snapshot,CancellationToken token)
    {
        workspace.AuthenticateDescriptor(this,backing,token);workspace.Visit(32);int cost=Cost(snapshot);
        if(snapshot.Length!=128||Capacity is not(125 or 250)||Count>Capacity||cost>=0x4000000||mutating||resizing)throw new InvalidOperationException("Exact primary snapshot/reset capacity required.");
        int Read(int i){workspace.Visit(4);return ReadCost(i);}void Move(int from,int to){workspace.Visit(128);workspace.Copy(128);CopySlot(from,to);}
        if(Count==Capacity&&Read(0)<=cost)return false;mutating=true;
        try
        {
            int target;if(Count==Capacity)
            {target=0;while(target*2<Count-1){int child=target*2+1;if(child<Count-1&&Read(child)<=Read(child+1))child++;if(Read(child)<=cost)break;Move(child,target);target=child;}}
            else
            {workspace.Copy(4);target=Count++;while(target!=0){int parent=(target-1)/2;if(cost<=Read(parent))break;Move(parent,target);target=parent;}}
            workspace.Copy(252);var destination=backing.AsSpan(target*128,128);destination[..120].Clear();BinaryPrimitives.WriteInt32LittleEndian(destination[16..],cost);snapshot.CopyTo(destination);return true;
        }
        finally{mutating=false;}
    }
    internal void ApplyPeriodicPrimaryQuota(LegacyEnglishCaseNode0Completion.GraphPeriodicWorkspace workspace,CancellationToken token)
    {
        workspace.AuthenticateDescriptor(this,backing,token);int old=Count;
        //1F73D0 always writes descriptor+4 before its oldCount<=quota fast path.
        workspace.Copy(4);Capacity=125;if(old<=125)return;workspace.Copy(4);Count=125;
        // Tail inputs cannot be overwritten by prefix-only heap movement. Reuse one fixed snapshot.
        Span<byte> snapshot=stackalloc byte[128];
        for(int index=125;index<old;index++)
        {
            token.ThrowIfCancellationRequested();workspace.Visit(128);workspace.Copy(128);backing.AsSpan(index*128,128).CopyTo(snapshot);
            // The workspace's ordinary temporary cannot be exposed from here. Snapshot allocation is charged before this detached numerical copy.
            workspace.Copy(128);AllocatePeriodicStackSnapshot(workspace,snapshot,token);
        }
    }
    private void AllocatePeriodicStackSnapshot(LegacyEnglishCaseNode0Completion.GraphPeriodicWorkspace workspace,ReadOnlySpan<byte> source,CancellationToken token)
    {
        // Fixed stack128 is copied into the same accepted destination with literal allocator chronology; no byte[] clone.
        workspace.AuthenticateDescriptor(this,backing,token);int cost=Cost(source);workspace.Visit(32);if(Count==Capacity&&ReadCost(0)<=cost)return;
        mutating=true;
        try
        {int target=0;while(target*2<Count-1){workspace.Visit(16);int child=target*2+1;if(child<Count-1&&ReadCost(child)<=ReadCost(child+1))child++;if(ReadCost(child)<=cost)break;workspace.Copy(128);CopySlot(child,target);target=child;}workspace.Copy(252);var destination=backing.AsSpan(target*128,128);destination[..120].Clear();BinaryPrimitives.WriteInt32LittleEndian(destination[16..],cost);source.CopyTo(destination);}
        finally{mutating=false;}
    }
}
