using InkRecognition;
using System.Security.Cryptography;
using System.Buffers.Binary;
using System.Runtime.InteropServices;

//Actual B7 guard work reuses the SAME already-computed B6..A owners. No NN
//evaluation, caller-promoted A8, reflection, new runtime seam or old edit.
internal static class LifecycleControls
{
    internal static int BasicRuns,MultiRuns;
    internal static readonly List<string> Passed=[];
    internal static readonly List<(string Label,string Type,string Message)> Proofs=[];
    internal static readonly List<string> Reached=[];
    private static Exception Refuse(string label,Action action)
    {
        try{action();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException or OperationCanceledException or InvalidOperationException)
        {Passed.Add(label);Proofs.Add((label,e.GetType().FullName!,label=="injected-fault"?e.Message:string.Empty));return e;}throw new Exception("Guard did not refuse:"+label);
    }
    private static void CancelRefuse(string label,Action action)
    {var caught=Refuse(label,action);if(caught is not OperationCanceledException)throw new Exception("Expected cancellation type:"+label,caught);}
    private static string Fingerprint(LegacyFullEnglishFirstPlainText owner)
    {
        using var sha=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);var scratch=new byte[10];
        foreach(var child in owner.CopyCompletedChildren())
        {
            foreach(var node in child.CopyGlyphOwners()){BinaryPrimitives.WriteUInt64LittleEndian(scratch,node.Key);BinaryPrimitives.WriteUInt16LittleEndian(scratch.AsSpan(8),node.PackedCount);sha.AppendData(scratch);sha.AppendData(node.Records.Span);}
            foreach(var word in child.NativeWords){sha.AppendData(word.Published.Word.CopyStorage());foreach(ulong reference in word.Published.Word.FlatReferences){BinaryPrimitives.WriteUInt64LittleEndian(scratch,reference);sha.AppendData(scratch.AsSpan(0,8));}}
        }return Convert.ToHexString(sha.GetHashAndReset());
    }
    internal static void RunBasic(LegacyFullEnglishFirstPlainText owner)
    {
        if(BasicRuns!=0)return;var children=owner.CopyCompletedChildren();if(children.Count==0||children[0].Words.Count==0)return;
        int nn=owner.Owner.Owner.Owner.Owner.Owner.Parent.NetworkEvaluationCount;string before=Fingerprint(owner);BasicRuns++;
        Refuse("null-owner",()=>LegacyFullEnglishFirstGlyphEmission.Prepare(null!));CancelRefuse("prepare-cancel",()=>LegacyFullEnglishFirstGlyphEmission.Prepare(owner,new CancellationToken(true)));
        var good=LegacyFullEnglishFirstGlyphEmission.Prepare(owner);CancelRefuse("preentry-cancel",()=>good.Run(new CancellationToken(true)));
        if(good.IsComplete||good.IsFaulted)throw new Exception("Preentry cancel consumed owner");good.Run();Refuse("double-run",()=>good.Run());
        var exact=LegacyFullEnglishFirstGlyphEmission.PrepareBounded(owner,good.AdditionalLogicalPeakBytes,good.LogicalCopyWorkBytes,good.LogicalVisitWork);exact.Run();
        Refuse("one-short-peak",()=>LegacyFullEnglishFirstGlyphEmission.PrepareBounded(owner,good.AdditionalLogicalPeakBytes-1,good.LogicalCopyWorkBytes,good.LogicalVisitWork));
        Refuse("one-short-work",()=>LegacyFullEnglishFirstGlyphEmission.PrepareBounded(owner,good.AdditionalLogicalPeakBytes,good.LogicalCopyWorkBytes-1,good.LogicalVisitWork));
        Refuse("one-short-visits",()=>LegacyFullEnglishFirstGlyphEmission.PrepareBounded(owner,good.AdditionalLogicalPeakBytes,good.LogicalCopyWorkBytes,good.LogicalVisitWork-1));
        Refuse("oversized-budget",()=>LegacyFullEnglishFirstGlyphEmission.PrepareBounded(owner,long.MaxValue,good.LogicalCopyWorkBytes,good.LogicalVisitWork));
        var invalid=LegacyFullEnglishFirstGlyphEmission.Prepare(owner);Refuse("allocator39",()=>invalid.RunPrepared(null,null,null,new byte[39]));
        if(invalid.IsFaulted||invalid.IsComplete)throw new Exception("Bad allocator consumed owner");invalid.Run();
        var incomplete=LegacyFullEnglishFirstPlainText.Prepare(owner.Owner);Refuse("incomplete-B6",()=>LegacyFullEnglishFirstGlyphEmission.Prepare(incomplete));
        var faulted=LegacyFullEnglishFirstPlainText.Prepare(owner.Owner);using(var cts=new CancellationTokenSource())
        {bool reached=false;CancelRefuse("B6-entered-cancel",()=>faulted.RunPrepared(s=>{if(s=="entry"){reached=true;cts.Cancel();}},null,new byte[120],new byte[40],cts.Token));if(!reached)throw new Exception("B6 cancel stage not reached");Reached.Add("B6-entered-cancel:entry");}
        Refuse("faulted-B6",()=>LegacyFullEnglishFirstGlyphEmission.Prepare(faulted));
        var policy=new LegacyFullEnglishFirstGlyphEmissionPolicy(owner,children[0],default);
        foreach(ushort header in new ushort[]{182,183,184})Refuse("PREPARED-header-"+header,()=>policy.ResolvePrototype(header));
        Refuse("PREPARED-symbol-FFFF",()=>policy.ResolvePrototype(65535));
        //Same model/source, different B6 object is still foreign ownership.
        var other=LegacyFullEnglishFirstPlainText.Prepare(owner.Owner);other.Run();
        Refuse("foreign-B6-child",()=>policy.Validate(other.CopyCompletedChildren()[0],default));
        foreach(string stage in new[]{"entry","child:entry","child:cloned","range:counted","range:filled","glyph:interned","glyph:score","glyph:complete","child:materialized","child:published"})
        {
            var pass=LegacyFullEnglishFirstGlyphEmission.Prepare(owner);using var cts=new CancellationTokenSource();bool reached=false;
            CancelRefuse("entered-cancel:"+stage,()=>pass.RunPrepared(s=>{if(s==stage){reached=true;cts.Cancel();}},null,null,new byte[40],cts.Token));
            if(!reached||!pass.IsFaulted||pass.IsComplete)throw new Exception("Missing cancel stage/poison:"+stage);Reached.Add("entered-cancel:"+stage);
            _=pass.CopyCompletedChildren();Refuse("retry-after-cancel:"+stage,()=>pass.Run());
        }
        var injected=LegacyFullEnglishFirstGlyphEmission.Prepare(owner);bool injectedReached=false;var injectedError=Refuse("injected-fault",()=>injected.RunPrepared(s=>{if(s=="child:materialized"){injectedReached=true;throw new InvalidDataException("Injected");}},null,null,new byte[40]));
        if(!injectedReached||injectedError.GetType()!=typeof(InvalidDataException)||injectedError.Message!="Injected")throw new Exception("Wrong injected fault/stage",injectedError);Reached.Add("injected-fault:child:materialized");
        if(!injected.IsFaulted)throw new Exception("Fault not poisoned");Refuse("retry-after-fault",()=>injected.Run());
        var concurrent=LegacyFullEnglishFirstGlyphEmission.Prepare(owner);using(var entered=new ManualResetEventSlim())using(var release=new ManualResetEventSlim())
        {
            var task=Task.Run(()=>concurrent.RunPrepared(s=>{if(s=="entry"){entered.Set();if(!release.Wait(TimeSpan.FromSeconds(30)))throw new Exception("Release timeout");}},null,null,new byte[40]));
            if(!entered.Wait(TimeSpan.FromSeconds(30))){release.Set();throw new Exception("Entry timeout");}
            try{Refuse("barrier-second-run",()=>concurrent.Run());_=concurrent.CopyCompletedChildren();}finally{release.Set();}task.GetAwaiter().GetResult();
            if(!concurrent.IsComplete||concurrent.IsFaulted)throw new Exception("Winning concurrent Run failed");
        }
        foreach(var child in good.CopyCompletedChildren())
        {
            foreach(var node in child.CopyGlyphOwners()){var data=MemoryMarshal.AsMemory(node.Records).Span;byte saved=data[0];data[0]^=255;try{if(child.CopyGlyphOwners().Single(x=>x.Key==node.Key).Records.Span[0]!=saved)throw new Exception("Mutable owner snapshot");}finally{data[0]=saved;}Passed.Add("owner-snapshot-detached");}
            foreach(ulong key in child.CopySourceRangeKeys()){var rs=child.CopySourceRanges(key);var saved=rs[0];rs[0]=new(new(123,456,789),new(321,654,987));if(child.CopySourceRanges(key)[0]!=saved)throw new Exception("Mutable range snapshot");Passed.Add("range-snapshot-detached");}
            foreach(var word in child.Words){byte[] data=word.CopyStorage();data[0]^=255;if(word.CopyStorage()[0]==data[0])throw new Exception("Mutable WORD snapshot");var refs=word.CopyReadingOrderReferences();if(refs.Length>0){ulong old=refs[0];refs[0]^=1;if(word.CopyReadingOrderReferences()[0]!=old)throw new Exception("Mutable WORD refs");}Passed.Add("word-snapshot-detached");}
        }
        if(before!=Fingerprint(owner)||owner.Owner.Owner.Owner.Owner.Owner.Parent.NetworkEvaluationCount!=nn)throw new Exception("Guards changed B6 or replayed NN");Passed.Add("B6-byte-identity-and-NN-count-unchanged");
    }
    internal static void RunMulti(LegacyFullEnglishFirstPlainText owner)
    {
        if(MultiRuns!=0||owner.CopyCompletedChildren().Count<2)return;MultiRuns++;int nn=owner.Owner.Owner.Owner.Owner.Owner.Parent.NetworkEvaluationCount;
        var pass=LegacyFullEnglishFirstGlyphEmission.Prepare(owner);int entry=0;using var cts=new CancellationTokenSource();
        CancelRefuse("multichild-prefix-cancel",()=>pass.RunPrepared(s=>{if(s=="child:entry"&&++entry==2)cts.Cancel();},null,null,new byte[40],cts.Token));
        if(entry!=2||!pass.IsFaulted||pass.IsComplete||pass.CopyCompletedChildren().Count!=1)throw new Exception("Expected one historical completed child prefix");Reached.Add("multichild-prefix-cancel:child:entry:2");
        Refuse("retry-prefix-fault",()=>pass.Run());if(owner.Owner.Owner.Owner.Owner.Owner.Parent.NetworkEvaluationCount!=nn)throw new Exception("Prefix guard replayed NN");
    }
}
