using System.Text.Json;
using System.Runtime.CompilerServices;
using InkRecognition;

//Dedicated actual-owner entry is compiled, never invoked by --prepared.
internal static class OwnerCapture
{
    internal static void Run(string models,string rawInput,string output)
    {
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
            var owner=LegacyEnglishCasePrimaryCompletion.Prepare(knowledge,numeric,cases,publication);var consumer=Consumer(owner);var pass=Pass(consumer);
            byte[][]? lastInput=null;LegacySymbolContextNode[]? beforeContexts=null;ulong beforeRoot=0;
            var receipt=owner.Run(stage=>{if(stage=="query:page"){lastInput=consumer.CopyHistoricalHeap().CopySlots();var contexts=pass.Initializer.PlanForAudit.BindingForAudit.Contexts;beforeContexts=contexts.CopyNodes().ToArray();beforeRoot=contexts.RootTreeIdentity;}});
            var historical=receipt.Export();var after=pass.Initializer.PlanForAudit.BindingForAudit.Contexts.CopyNodes().ToArray();
            //Stable portable IDs are emitted for the oracle to canonicalize by
            //allocation order; this probe never supplies context IDs to an owner.
            captured.Add(JsonOutput.D(("Name",item.GetProperty(rawArray?"Name":"id").GetString()),("Input",lastInput!.Select(Convert.ToHexStringLower).ToArray()),("BeforeContexts",beforeContexts!.Select(Node).ToArray()),("BeforeRoot",beforeRoot),("AfterContexts",after.Select(Node).ToArray()),("AfterRoot",pass.Initializer.PlanForAudit.BindingForAudit.Contexts.RootTreeIdentity),
                ("Rows",Convert.ToHexStringLower(historical.Rows88)),("Alternatives",Convert.ToHexStringLower(historical.PrimaryAlternatives80)),("RowMasks",Convert.ToHexStringLower(historical.RowDefinedBitMasks)),("AlternativeMasks",Convert.ToHexStringLower(historical.AlternativeDefinedBitMasks)),("Ordinals",historical.SourceOrdinals),
                ("Count",receipt.Count),("InitialCount",receipt.InitialCount),("ContextNodes",receipt.ContextNodes),("ReservedContextResolutions",receipt.ReservedContextResolutions),("LogicalPayloadBytes",receipt.LogicalPayloadBytes),("ReservedCopyWorkBytes",receipt.ReservedCopyWorkBytes)));
            guards??=OwnerGuards.Run(knowledge,numeric,cases,publication,owner,receipt,historical);
        }
        JsonOutput.Save(output,JsonOutput.D(("Scope","ACTUAL OWNER: requires separate explicit root runtime approval"),("Rows",captured),("Guards",guards)));
        static long[] Node(LegacySymbolContextNode n)=>[(long)n.Identity,(long)n.ParentLink,(long)n.ChildTreeRoot,(long)n.Left,(long)n.Right,n.Symbol,n.Balance,n.PageIndex,n.PageSlot];
        LegacyEnglish119PlainTextResult Recognize(LegacyEnglish119SourceSubmission submission)
        {
            var candidates=LegacyOwnedEnglish119Candidates.Prepare(submission.Preparation,table);var cache=LegacyOwnedEnglish119AcousticCache.Prepare(candidates);
            var confidence=LegacyOwnedEnglish119GlyphConfidence.Prepare(cache);var costs=LegacyOwnedEnglish119ControllerCosts.Prepare(confidence,table);
            var primary=LegacyOwnedEnglish119FirstPrimary.Prepare(costs,table).Run();var contexts=LegacyOwnedEnglish119ContextPasses.Prepare(primary).Run();
            return LegacyOwnedEnglish119PlainText.Materialize(LegacyOwnedEnglish119Terminal.Select(contexts),new(new byte[120],new byte[40]));
        }
    }
    [UnsafeAccessor(UnsafeAccessorKind.Field,Name="consumer")]private static extern ref LegacyEnglishCaseInitialConsumer Consumer(LegacyEnglishCasePrimaryCompletion owner);
    [UnsafeAccessor(UnsafeAccessorKind.Field,Name="pass")]private static extern ref LegacyEnglishCaseInitialQueryPass Pass(LegacyEnglishCaseInitialConsumer consumer);
}
