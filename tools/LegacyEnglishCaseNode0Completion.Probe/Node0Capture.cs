using System.Text.Json;
using System.Runtime.CompilerServices;
using InkRecognition;

// Dedicated SOURCE-ONLY actual-owner draft. Not compiled or executed.
// The production readiness guard still refuses before any capture is returned.
internal static class Node0Capture
{
    internal static void Run(string models,string rawInput,string output,string dataset)
    {
        if(dataset is not ("134" or "100"))throw new ArgumentException("Exact dataset134 or100 required.");
        var acoustic=File.ReadAllBytes(Path.Combine(models,"en-US/en_US-ak-cur.lite.res"));var source=LegacySource247WireLayout.Read(acoustic);
        var model=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(acoustic,source,source.Children.First(c=>c.Kind==0x249));
        var lexical=File.ReadAllBytes(Path.Combine(models,"en-US/en_US-lk-text.lite.res"));var data=LegacyEnglishLkPackedData.Read(source,lexical);
        var language=LegacyEnglishLkSparseLanguageData.Read(data,lexical);var numeric=LegacyEnglishLkFreshNumericBinding.Create(language);
        var m30=File.ReadAllBytes(Path.Combine(models,"line-geometry/line-2e3-30.res"));var m50=File.ReadAllBytes(Path.Combine(models,"line-geometry/line-2e3-50.res"));
        var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(models,"ru-RU/legacy-separation-profile.json")));
        using var cf=File.OpenRead(Path.Combine(models,"text-unicode/legacy-case-data.json"));using var pf=File.OpenRead(Path.Combine(models,"text-unicode/legacy-text-publication-data.json"));
        var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
        using var input=JsonDocument.Parse(File.ReadAllBytes(rawInput));var captured=new List<object>();object? guards=null;
        bool rawArray=input.RootElement.ValueKind==JsonValueKind.Array;
        var items=rawArray?input.RootElement.EnumerateArray():input.RootElement.GetProperty("samples").EnumerateArray();
        foreach(var item in items)
        {
            //Only raw strokes/resolution and inert test name are read. No targets.
            var strokes=item.GetProperty(rawArray?"Raw":"strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
            var parent=LegacyEnglish119ParentSubmission.Prepare(model,strokes,rawArray?item.GetProperty("Resolution").GetSingle():0,m30,m50);
            parent.Publish(parent.Children.Select(c=>Recognize(c.Submission)).ToArray());
            var post=LegacyEnglish119ParentPostprocessing.Prepare(parent,cases,publication).Run();if(post.CopySnapshots()[0].CopyDescriptors().Length!=1)continue;
            var finalized=LegacyEnglish119FinalizedSingleSource.Prepare(post).Run();var local=LegacyEnglish119MainLocalCandidates.Prepare(finalized,table).Run();var knowledge=LegacyEnglish119MainKnowledge.Create(local);
            var owner=LegacyEnglishCaseNode0Completion.Prepare(knowledge,numeric,cases,publication);var consumer=Consumer(Primary(owner));var pass=Pass(consumer);
            var events=new List<string>();byte[][]? lastInput=null;LegacySymbolContextNode[]? beforeContexts=null;ulong beforeRoot=0;
            var receipt=owner.Run(stage=>{events.Add(stage);if(stage=="query:page"){lastInput=consumer.CopyHistoricalHeap().CopySlots();var contexts=pass.Initializer.PlanForAudit.BindingForAudit.Contexts;beforeContexts=contexts.CopyNodes().ToArray();beforeRoot=contexts.RootTreeIdentity;}});
            var primaryReceipt=PrimaryReceipt(receipt);var historical=receipt.ExportPrimaryDiagnostic();var after=pass.Initializer.PlanForAudit.BindingForAudit.Contexts.CopyNodes().ToArray();
            //Stable portable IDs are emitted for the oracle to canonicalize by
            //allocation order; this probe never supplies context IDs to an owner.
            var primaryCapture=JsonOutput.D(("Name",item.GetProperty(rawArray?"Name":"id").GetString()),("Input",lastInput!.Select(Convert.ToHexStringLower).ToArray()),("BeforeContexts",beforeContexts!.Select(Node).ToArray()),("BeforeRoot",beforeRoot),("AfterContexts",after.Select(Node).ToArray()),("AfterRoot",pass.Initializer.PlanForAudit.BindingForAudit.Contexts.RootTreeIdentity),
                ("Rows",Convert.ToHexStringLower(historical.Rows88)),("Alternatives",Convert.ToHexStringLower(historical.PrimaryAlternatives80)),("RowMasks",Convert.ToHexStringLower(historical.RowDefinedBitMasks)),("AlternativeMasks",Convert.ToHexStringLower(historical.AlternativeDefinedBitMasks)),("Ordinals",historical.SourceOrdinals),
                ("Count",primaryReceipt.Count),("InitialCount",primaryReceipt.InitialCount),("ContextNodes",primaryReceipt.ContextNodes),("ReservedContextResolutions",primaryReceipt.ReservedContextResolutions),("LogicalPayloadBytes",primaryReceipt.LogicalPayloadBytes),("ReservedCopyWorkBytes",primaryReceipt.ReservedCopyWorkBytes));
            var graph=new object[receipt.Graph.NodeCount];
            for(int node=0;node<graph.Length;node++)
            {
                var arcs=new object[receipt.Graph.CountAt(node)];
                for(int j=0;j<arcs.Length;j++){var a=receipt.Graph.ReadArc(node,j);arcs[j]=new long[]{a.GroupId,a.Span,a.Flags,a.StartContextId,a.EndContextId};}
                graph[node]=arcs;
            }
            var ledger=Ledger(owner);var sites=new object[64];
            for(int i=0;i<64;i++){var v=ledger.ReadSite((LegacyEnglishCaseNode0Ledger.Site)i);sites[i]=JsonOutput.D(("Name",Enum.GetName((LegacyEnglishCaseNode0Ledger.Site)i)??("Reserved"+i)),("Allocation",v.Allocation),("Copy",v.Copy),("Visit",v.Visit));}
            object? aux=receipt.Controller is null?null:JsonOutput.D(("Controller4096",Hex(receipt.Controller)),("ControllerMask",Hex(receipt.ControllerMask!)),("First440",Hex(receipt.First!)),("FirstMask",Hex(receipt.FirstMask!)),("Second440",Hex(receipt.Second!)),("SecondMask",Hex(receipt.SecondMask!)));
            captured.Add(JsonOutput.D(("Name",item.GetProperty(rawArray?"Name":"id").GetString()),("Primary",primaryCapture),("Graph",graph),
                ("T",receipt.T),("N",receipt.N),("Count",receipt.Count),("LogicalCapacity",receipt.Capacity),("SourceEnd",receipt.SourceEnd),("RetainedEnd",receipt.RetainedEnd),
                ("Heap250",Hex(receipt.Values)),("HeapMasks250",Hex(receipt.Masks)),("Rows88",Hex(receipt.Rows)),("RowMasks",Hex(receipt.RowMask)),("Alternatives80",Hex(receipt.Primary)),("AlternativeMasks",Hex(receipt.PrimaryMask)),("Ordinals156",receipt.Ordinals),("Aux",aux),
                ("Budgets",JsonOutput.D(("NewContinuationPayload",ledger.Payload),("NewCopies",ledger.Copies),("NewVisits",ledger.Visits),("Sites",sites))),("Lifecycle",events.ToArray())));
            guards??=Node0Guards.Run(knowledge,numeric,cases,publication,owner,receipt);
        }
        int expected=dataset=="134"?134:dataset=="100"?100:throw new ArgumentException("Exact dataset134 or100 required.");
        if(captured.Count!=expected)throw new InvalidDataException("Exact fullowner capture denominator required.");
        // No PASS label: strict independent capture/guard oracle is required.
        JsonOutput.Save(output,JsonOutput.D(("Scope","SOURCE_DRAFT_MINIMAL_FINAL_IMAGES_WITH_44_GUARDS"),("Dataset",dataset),("ContextIdentityConvention","RealFactoryIds_CheckerCanonicalizesKnownFields"),("Rows",captured),("Guards",guards)));
        static string Hex(byte[] b)=>Convert.ToHexStringLower(b);
        static long[] Node(LegacySymbolContextNode n)=>[(long)n.Identity,(long)n.ParentLink,(long)n.ChildTreeRoot,(long)n.Left,(long)n.Right,n.Symbol,n.Balance,n.PageIndex,n.PageSlot];
        LegacyEnglish119PlainTextResult Recognize(LegacyEnglish119SourceSubmission submission)
        {
            var candidates=LegacyOwnedEnglish119Candidates.Prepare(submission.Preparation,table);var cache=LegacyOwnedEnglish119AcousticCache.Prepare(candidates);
            var confidence=LegacyOwnedEnglish119GlyphConfidence.Prepare(cache);var costs=LegacyOwnedEnglish119ControllerCosts.Prepare(confidence,table);
            var primary=LegacyOwnedEnglish119FirstPrimary.Prepare(costs,table).Run();var contexts=LegacyOwnedEnglish119ContextPasses.Prepare(primary).Run();
            return LegacyOwnedEnglish119PlainText.Materialize(LegacyOwnedEnglish119Terminal.Select(contexts),new(new byte[120],new byte[40]));
        }
    }
    [UnsafeAccessor(UnsafeAccessorKind.Field,Name="primary")]private static extern ref LegacyEnglishCasePrimaryCompletion Primary(LegacyEnglishCaseNode0Completion owner);
    [UnsafeAccessor(UnsafeAccessorKind.Field,Name="ledger")]private static extern ref LegacyEnglishCaseNode0Ledger Ledger(LegacyEnglishCaseNode0Completion owner);
    [UnsafeAccessor(UnsafeAccessorKind.Field,Name="primaryDiagnostic")]private static extern ref LegacyEnglishCasePrimaryCompletionReceipt PrimaryReceipt(LegacyEnglishCaseNode0Completion.DataExport result);
    [UnsafeAccessor(UnsafeAccessorKind.Field,Name="consumer")]private static extern ref LegacyEnglishCaseInitialConsumer Consumer(LegacyEnglishCasePrimaryCompletion owner);
    [UnsafeAccessor(UnsafeAccessorKind.Field,Name="pass")]private static extern ref LegacyEnglishCaseInitialQueryPass Pass(LegacyEnglishCaseInitialConsumer consumer);
}
