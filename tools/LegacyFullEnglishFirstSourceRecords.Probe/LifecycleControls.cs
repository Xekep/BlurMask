using InkRecognition;
using System.Runtime.InteropServices;
namespace FullFirstSourceRecordsProbe;

//Unexecuted future friend controls. Every refusal checks exact type+reached;
//OCE additionally requires the exact expected, cancelable, requested token.
//only Injected message is canonical (framework messages may be localized).
internal static class LifecycleControls
{
    internal sealed record HistoricalRow(int Descriptor,int Word,int Ordinal,ulong Reference,bool Flat,byte[] Storage,int[][] ChildRanges,int[][] Projected,int LabelCount);
    internal sealed record HistoricalPrefix(string Stage,bool Complete,bool Faulted,int ReservedRows,HistoricalRow[] Rows,byte[][] Descriptors);
    internal static readonly List<HistoricalPrefix> Prefixes=[];
    private static void Capture(string stage,LegacyFullEnglishFirstSourceRecords pass)
    {
        var values=pass.CopyRows();if(values.Count>64||Prefixes.Count>=5)throw new InvalidDataException("Guard prefix export cap");
        var rows=values.Select(r=>new HistoricalRow(r.Child.Original.Factory.Candidates.Child.DescriptorOrdinal,r.WordOrdinal,r.ReferenceOrdinal,r.Reference,r.SoleFlatOverride,
            r.Record.CopyStorage().ToArray(),r.CopyChildRanges().Select(SourceExports.Range).ToArray(),r.Record.Ranges.Select(SourceExports.Range).ToArray(),r.Record.LabelCount)).ToArray();
        Prefixes.Add(new(stage,pass.IsComplete,pass.IsFaulted,pass.OrdinaryReferenceCount,rows,pass.CopyDescriptors()));
    }
    private static string CancellationWitness(OperationCanceledException error,CancellationToken expected)
    {
        var actual=error.CancellationToken;
        if(actual!=expected||!actual.CanBeCanceled||!expected.CanBeCanceled||!actual.IsCancellationRequested||!expected.IsCancellationRequested)
            throw new InvalidDataException("Wrong cancellation token/state");
        return "|tokenMatch=1|actualCancelable=1|actualRequested=1|expectedCancelable=1|expectedRequested=1";
    }
    internal static IReadOnlyList<string> Run(LegacyFullEnglishFirstGlyphEmission cached)
    {
        Prefixes.Clear();GuardAdmission.Require(cached);var proofs=new List<string>();var parent=cached.Owner.Owner.Owner.Owner.Owner.Owner.Parent;int nn=parent.NetworkEvaluationCount;
        void Check(bool condition,string label){if(!condition)throw new InvalidDataException(label);proofs.Add(label+"|PASS");}
        void Refuse(string label,Action action,Type type,Func<bool> reached,bool expectedReached,bool injected=false,CancellationToken? expectedToken=null)
        {
            Exception? error=null;try{action();}catch(Exception e){error=e;}
            if(error is null||error.GetType()!=type||reached()!=expectedReached||injected&&error.Message!="Injected")
                throw new InvalidDataException("Wrong refusal type/reached: "+label,error);
            string cancellation="";
            if(error is OperationCanceledException canceled)
            {
                if(expectedToken is not CancellationToken token)throw new InvalidDataException("Missing expected cancellation token");
                cancellation=CancellationWitness(canceled,token);
            }
            else if(expectedToken.HasValue)throw new InvalidDataException("Unexpected cancellation-token contract");
            proofs.Add(label+"|"+error.GetType().Name+"|"+(reached()?1:0)+"|"+(injected?error.Message:"")+cancellation);
        }
        //Prepared validator controls, not actual source-owner cancellation evidence.
        using(var expectedSource=new CancellationTokenSource())
        using(var foreignSource=new CancellationTokenSource())
        using(var unrequestedSource=new CancellationTokenSource())
        {
            expectedSource.Cancel();foreignSource.Cancel();
            void RejectToken(string label,OperationCanceledException error,CancellationToken expected)
            {
                bool rejected=false;try{CancellationWitness(error,expected);}catch(InvalidDataException){rejected=true;}
                Check(rejected,"prepared-token-validator-"+label);
            }
            RejectToken("default",new OperationCanceledException(),expectedSource.Token);
            RejectToken("foreign",new OperationCanceledException(foreignSource.Token),expectedSource.Token);
            RejectToken("unrequested",new OperationCanceledException(unrequestedSource.Token),unrequestedSource.Token);
        }
        var measured=LegacyFullEnglishFirstSourceRecords.Prepare(cached);
        if(measured.OrdinaryReferenceCount>64||measured.ProjectedRangeFragments>512)throw new NotSupportedException("Small actual lifecycle fixtures only");
        Refuse("peak-one-short",()=>LegacyFullEnglishFirstSourceRecords.PrepareBounded(cached,measured.AdditionalLogicalPeakBytes-1,measured.LogicalCopyWorkBytes,measured.LogicalVisitWork),typeof(ArgumentException),()=>false,false);
        Refuse("copy-one-short",()=>LegacyFullEnglishFirstSourceRecords.PrepareBounded(cached,measured.AdditionalLogicalPeakBytes,measured.LogicalCopyWorkBytes-1,measured.LogicalVisitWork),typeof(ArgumentException),()=>false,false);
        Refuse("visits-one-short",()=>LegacyFullEnglishFirstSourceRecords.PrepareBounded(cached,measured.AdditionalLogicalPeakBytes,measured.LogicalCopyWorkBytes,measured.LogicalVisitWork-1),typeof(ArgumentException),()=>false,false);
        using(var cancel=new CancellationTokenSource())
        {
            cancel.Cancel();Refuse("prepare-canceled",()=>LegacyFullEnglishFirstSourceRecords.Prepare(cached,cancel.Token),typeof(OperationCanceledException),()=>false,false,expectedToken:cancel.Token);
            Refuse("run-preentry-canceled",()=>measured.Run(cancel.Token),typeof(OperationCanceledException),()=>false,false,expectedToken:cancel.Token);
        }
        Check(!measured.IsFaulted&&!measured.IsComplete&&measured.CopyRows().Count==0,"preentry-retryable-empty");
        measured.Run();Check(measured.IsComplete&&!measured.IsFaulted,"retry-after-preentry-success");
        Refuse("second-run",()=>measured.Run(),typeof(InvalidOperationException),()=>false,false);
        foreach(string stage in new[]{"entry","row:created","row:published"})
        {
            var pass=LegacyFullEnglishFirstSourceRecords.Prepare(cached);bool reached=false;using var cts=new CancellationTokenSource();
            Refuse("entered-cancel-"+stage,()=>pass.RunPrepared(s=>{if(s==stage){reached=true;cts.Cancel();}},cts.Token),typeof(OperationCanceledException),()=>reached,true,expectedToken:cts.Token);
            Check(pass.IsFaulted&&!pass.IsComplete,"poison-"+stage);
            Check(pass.CopyRows().Count==(stage=="row:published"?1:0),"historical-prefix-"+stage);Capture(stage,pass);
            Refuse("postfault-run-"+stage,()=>pass.Run(),typeof(InvalidOperationException),()=>false,false);
        }
        {
            var pass=LegacyFullEnglishFirstSourceRecords.Prepare(cached);bool reached=false;
            Refuse("injected-row-failure",()=>pass.RunPrepared(s=>{if(s=="row:published"){reached=true;throw new InvalidDataException("Injected");}}),typeof(InvalidDataException),()=>reached,true,true);
            Check(pass.IsFaulted&&!pass.IsComplete&&pass.CopyRows().Count==1,"injected-historical-row");Capture("injected-row",pass);
        }
        if(cached.CopyCompletedChildren().Count>1)
        {
            var pass=LegacyFullEnglishFirstSourceRecords.Prepare(cached);bool reached=false;using var cts=new CancellationTokenSource();
            Refuse("multi-child-close-cancel",()=>pass.RunPrepared(s=>{if(s=="child:closed"){reached=true;cts.Cancel();}},cts.Token),typeof(OperationCanceledException),()=>reached,true,expectedToken:cts.Token);
            Check(pass.IsFaulted&&!pass.IsComplete&&pass.CopyRows().Count>0&&pass.CopyRows().Count<pass.OrdinaryReferenceCount,"multi-child-historical-prefix");Capture("multi-child",pass);
        }
        {
            var incomplete=LegacyFullEnglishFirstGlyphEmission.Prepare(cached.Owner);
            Refuse("incomplete-upstream",()=>LegacyFullEnglishFirstSourceRecords.Prepare(incomplete),typeof(ArgumentException),()=>false,false);
            bool reached=false;Refuse("upstream-injected-entry",()=>incomplete.RunPrepared(s=>{if(s=="entry"){reached=true;throw new InvalidDataException("Injected");}},null,null,new byte[40]),typeof(InvalidDataException),()=>reached,true,true);
            Refuse("faulted-upstream",()=>LegacyFullEnglishFirstSourceRecords.Prepare(incomplete),typeof(ArgumentException),()=>false,false);
        }
        {
            var pass=LegacyFullEnglishFirstSourceRecords.Prepare(cached);using var entry=new ManualResetEventSlim();using var release=new ManualResetEventSlim();
            var worker=Task.Run(()=>pass.RunPrepared(s=>{if(s=="entry"){entry.Set();if(!release.Wait(TimeSpan.FromSeconds(10)))throw new TimeoutException("guard release");}}));
            try
            {
                if(!entry.Wait(TimeSpan.FromSeconds(10)))throw new TimeoutException("guard entry");
                Check(pass.CopyRows().Count==0,"offlock-prefix-readable");
                Refuse("concurrent-run-loser",()=>pass.Run(),typeof(InvalidOperationException),()=>true,true);
            }
            finally{release.Set();worker.GetAwaiter().GetResult();}
            Check(pass.IsComplete&&!pass.IsFaulted&&pass.CopyRows().Count==pass.OrdinaryReferenceCount,"concurrent-one-winner");
        }
        var row=measured.CopyRows()[0];var storage=row.Record.CopyStorage();byte old=storage.Span[0];
        try{MemoryMarshal.AsMemory(storage).Span[0]^=1;Check(row.Record.CopyStorage().Span[0]==old,"defensive-actual-returned-ROM");}finally{MemoryMarshal.AsMemory(storage).Span[0]=old;}
        var ranges=row.CopyChildRanges();var first=ranges[0];ranges[0]=new(new(-1,-1,-1),new(-1,-1,-1));Check(row.CopyChildRanges()[0]==first,"defensive-origin-ranges");
        var descriptors=measured.CopyDescriptors();byte db=descriptors[0][0];descriptors[0][0]^=1;Check(measured.CopyDescriptors()[0][0]==db,"defensive-descriptors");
        var points=measured.CopyCurrentSource();var firstPoint=points[0].Points[0];((InkPoint[])points[0].Points)[0]=new(firstPoint.X==999?998:999,firstPoint.Y);Check(measured.CopyCurrentSource()[0].Points[0]==firstPoint,"defensive-current-source");
        var ids=measured.CopyCurrentChronology();var firstId=ids[0];ids[0]=firstId with{Key=new(999,998,997)};Check(measured.CopyCurrentChronology()[0]==firstId,"defensive-chronology");
        Check(ReferenceEquals(row.Owner,measured)&&ReferenceEquals(row.Child.Owner,cached),"canonical-row-source-owner");
        //No public API accepts caller-owned child/maps/records/rows at all;
        //foreign row promotion is unrepresentable, not a fabricated negative test.
        Check(parent.NetworkEvaluationCount==nn,"cached-lifecycle-noNN");return proofs;
    }
}

//Precharge8MiB small trusted snapshot envelope BEFORE these count copies;
//then limit actual guard inputs before any of its repeated B8 preparations.
internal static class GuardAdmission
{
    internal static void Require(LegacyFullEnglishFirstGlyphEmission cached)
    {
        var parent=cached.Owner.Owner.Owner.Owner.Owner.Owner.Parent;var children=cached.CopyCompletedChildren();
        if(children.Count>8||parent.Current.Sum(s=>s.Points.Count)>64)throw new NotSupportedException("Guard child/point bound");
        int rows=0,words=0,refs=0;
        foreach(var child in children)
        {
            foreach(var owner in child.CopyGlyphOwners()){rows=checked(rows+owner.Records.Length/40);if(rows>128)throw new NotSupportedException("Guard retained rich rows128");}
            foreach(var word in child.Words){if(++words>64)throw new NotSupportedException("Guard WORD64");refs=checked(refs+word.CopyReadingOrderReferences().Length);if(refs>128)throw new NotSupportedException("Guard allrefs128");}
        }
        //At most16 scoped prepares including category test and actual output,
        //conservative actual-smallcaps ledger+fixedbases, NOT productionbudget
        //relaxation and NOTRSS: Peak256MiB,Copy512MiB,Visits128Mi.
        long peak=16*(8L*1024*1024+2048L*64+128L*512+128L*128+32L*64+512L*8+512L*64)+8L*1024*1024;
        long work=16*(16L*1024*1024+4096L*64+256L*128+256L*512+64L*64+4096L*8+1024L*64)+8L*1024*1024;
        long visits=16*(4L*1024*1024+64L*64+8L*32768+256L*128+512L*64+32L*512+128L*128+256L*8);
        if(peak>256L*1024*1024||work>512L*1024*1024||visits>128L*1024*1024)throw new InvalidDataException("Guard aggregate envelope");
    }
}
