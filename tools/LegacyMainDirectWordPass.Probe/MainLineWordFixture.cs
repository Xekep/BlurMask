using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

internal sealed record MainLineWordFixture(LegacyFreshMainSingleLineSampled Source,
    LegacyFreshMainSingleLineContext Context, LegacyCursiveSeparationTable Table)
{
    internal static MainLineWordFixture Create(byte[] ak, byte[] engine, byte[] separation,
        byte[] line30, byte[] line50, LegacyNativeCaseData caseData, LegacyNativeTextPublicationData publication,
        int physicalCount = 14)
    {
        using var parsed = JsonDocument.Parse(engine);
        var activation = parsed.RootElement.GetProperty("activations");
        var activations = new LegacyInkActivations(
            activation.GetProperty("hidden").EnumerateArray().Select(value => value.GetInt32()).ToArray(),
            activation.GetProperty("output").EnumerateArray().Select(value => value.GetInt32()).ToArray());
        var table = LegacyCursiveSeparationTable.Load(separation);
        if (physicalCount is < 1 or > 32) throw new ArgumentOutOfRangeException(nameof(physicalCount));
        var raw = Enumerable.Range(0, physicalCount).Select(index => new InkStroke(new InkPoint[]
        {
            new(index * 70, 90), new(index * 70, 10), new(index * 70 + 25, 60),
            new(index * 70 + 50, 10), new(index * 70 + 50, 90)
        })).ToArray();
        var grouping = LegacyFreshMainPhysicalGrouping.Prepare(ak, line30, line50, raw, 0);
        var reordered = LegacyFreshMainPhysicalReordering.Prepare(LegacyFreshMainDescriptorSplit.Prepare(grouping), line30, line50);
        var first = LegacyFreshMainFirstResultLines.Prepare(ak, reordered);
        var materialized = first.ApplyFirstResults(first.FirstSubmissions.Select(BuildFirst).ToArray());
        var shear = LegacyFreshMainShearPreparation.Prepare(ak, materialized, caseData, publication);
        var gap = LegacyFreshMainActiveGapPreparation.Prepare(ak, shear).Smooth(ak);
        var sampler = LegacyFreshMainSingleLineSampler.Prepare(ak, gap);
        var sampled = sampler.Sample(table);
        var context = LegacyFreshMainSingleLineContext.Prepare(ak, sampled, activations);
        return new(sampled, context, table);

        LegacyMainSelectedFirstWord BuildFirst(LegacyMainFirstSubmission submission)
        {
            var factory = submission.Factory;
            var row = factory.Frame.Row;
            var cuts = row.GenerateRussianCuts();
            var profile = LegacyFirstTransitionGeometryProfile.ReadSource(ak, 6120);
            var policy = LegacyFreshFirstControllerPolicy.ReadFirstSource(ak, 6120);
            var scope = new LegacyFirstRowPrimitiveScope(row, cuts,
                row.OriginalSource.Strokes.Select(stroke => stroke.Points.Count).ToArray());
            var scorer = LegacyFirstOrdinaryCacheScorer.Prepare(ak, 6120, row, activations);
            byte[] mask = Enumerable.Repeat((byte)1, scorer.Alphabet.LabelCount).ToArray();
            int graphOffset = profile.SamplerOffset + 8 + BinaryPrimitives.ReadInt32LittleEndian(ak.AsSpan(profile.SamplerOffset + 4));
            var seeds = LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts, row.Frame.Line.Height,
                LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(ak, graphOffset),
                new byte[row.OriginalSource.Strokes.Count], Enumerable.Repeat(-1, row.OriginalSource.Strokes.Count).ToArray());
            var prepared = LegacyFreshFirstCandidateGraphPreparer.Prepare(ak, factory, cuts, seeds, table);
            var graph = prepared.SourceRanges.Graph;
            var scores = new LegacyFirstOrdinaryCacheScores[graph.Groups.Count];
            for (int group = 0; group < scores.Length; group++)
            {
                var ranges = graph.Groups[group].Ranges.ToArray();
                var order = cuts.Chronology.OrderRangeIndicesForNormalization(ranges);
                var boundaries = LegacyCursiveGlyphBoundaryPolicy.Resolve(order.Select(index => ranges[index]).ToArray(),
                    cuts.Cuts.PreparedInk.StrokePointCounts.Span, 0, false, false);
                scores[group] = scorer.Score(cuts, ranges, boundaries.Start, boundaries.End, 15,
                    Math.Min(256, policy.MaximumNormalizedStrokes), mask,
                    glyph => policy.CreateContext(scope.Evaluate(ranges, glyph.MergedGeometry.Strokes.Count), []), []);
            }
            var primaryPlan = LegacyFreshFirstProtocolOwnership.Create(factory).PreparePrimary(ak, cuts, prepared, scores, table);
            var primary = primaryPlan.Run();
            var contextPass = primaryPlan.RunSupportedNonSkippedContext();
            var word = new LegacyFirstPlainWordMaterializer(ak, 6120, factory, table)
                .Materialize(cuts, graph, primary, contextPass, scores);
            return new(submission.DescriptorOrdinal, word, prepared.SourceRanges);
        }
    }
}
