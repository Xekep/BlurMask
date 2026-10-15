using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

internal sealed record TrackedCaseInput(LegacyFreshMainSingleLineSampled Source,LegacyFreshMainSingleLineContext Context)
{
    internal static TrackedCaseInput Create(byte[] ak,byte[] engine,LegacyCursiveSeparationTable table,
        byte[] line30,byte[] line50,InkStroke[] raw,LegacyNativeCaseData cases,LegacyNativeTextPublicationData publication)
    {
        using var parsed=JsonDocument.Parse(engine);var a=parsed.RootElement.GetProperty("activations");
        var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),
            a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
        var grouping=LegacyFreshMainPhysicalGrouping.Prepare(ak,line30,line50,raw,0);
        var reordered=LegacyFreshMainPhysicalReordering.Prepare(LegacyFreshMainDescriptorSplit.Prepare(grouping),line30,line50);
        var first=LegacyFreshMainFirstResultLines.Prepare(ak,reordered);
        var materialized=first.ApplyFirstResults(first.FirstSubmissions.Select(BuildFirst).ToArray());
        var shear=LegacyFreshMainShearPreparation.Prepare(ak,materialized,cases,publication);
        var gap=LegacyFreshMainActiveGapPreparation.Prepare(ak,shear).Smooth(ak);
        var sampler=LegacyFreshMainSingleLineSampler.Prepare(ak,gap);var source=sampler.Sample(table);
        return new(source,LegacyFreshMainSingleLineContext.Prepare(ak,source,activations));

        LegacyMainSelectedFirstWord BuildFirst(LegacyMainFirstSubmission submission)
        {
            var factory=submission.Factory;var row=factory.Frame.Row;var cuts=row.GenerateRussianCuts();
            var profile=LegacyFirstTransitionGeometryProfile.ReadSource(ak,6120);
            var policy=LegacyFreshFirstControllerPolicy.ReadFirstSource(ak,6120);
            var scope=new LegacyFirstRowPrimitiveScope(row,cuts,row.OriginalSource.Strokes.Select(s=>s.Points.Count).ToArray());
            var scorer=LegacyFirstOrdinaryCacheScorer.Prepare(ak,6120,row,activations);
            byte[] mask=LegacyFreshFirstAcousticMaskPolicy.Create(ak,factory).CopyMasks().Primary.ToArray();
            int graphOffset=profile.SamplerOffset+8+BinaryPrimitives.ReadInt32LittleEndian(ak.AsSpan(profile.SamplerOffset+4));
            var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,row.Frame.Line.Height,
                LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(ak,graphOffset),new byte[row.OriginalSource.Strokes.Count],
                Enumerable.Repeat(-1,row.OriginalSource.Strokes.Count).ToArray());
            var prepared=LegacyFreshFirstCandidateGraphPreparer.Prepare(ak,factory,cuts,seeds,table);
            var graph=prepared.SourceRanges.Graph;var scores=new LegacyFirstOrdinaryCacheScores[graph.Groups.Count];
            for(int g=0;g<scores.Length;g++)
            {
                var ranges=graph.Groups[g].Ranges.ToArray();var order=cuts.Chronology.OrderRangeIndicesForNormalization(ranges);
                var signs=LegacyCursiveGlyphBoundaryPolicy.Resolve(order.Select(i=>ranges[i]).ToArray(),cuts.Cuts.PreparedInk.StrokePointCounts.Span,0,false,false);
                scores[g]=scorer.Score(cuts,ranges,signs.Start,signs.End,15,policy.MaximumNormalizedStrokes,mask,
                    glyph=>policy.CreateContext(scope.Evaluate(ranges,glyph.MergedGeometry.Strokes.Count),[]),[]);
            }
            var plan=LegacyFreshFirstProtocolOwnership.Create(factory).PreparePrimary(ak,cuts,prepared,scores,table);
            var primary=plan.Run();var contexts=LegacyFirstBlankContextPasses.Run(ak,factory,cuts,prepared,plan,table);
            var word=new LegacyFirstPlainWordMaterializer(ak,6120,factory,table).Materialize(cuts,graph,primary,contexts,scores);
            return new(submission.DescriptorOrdinal,word,prepared.SourceRanges);
        }
    }
}
