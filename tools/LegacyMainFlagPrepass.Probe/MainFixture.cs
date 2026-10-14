using System.Text.Json;
using InkRecognition;

internal sealed record MainFixture(LegacyRussianModelBundle Model, LegacyFreshSingleGlyphMainSampled Sampled,
    LegacyOwnedRussianDictionaryPreparation Dictionary, LegacyPreparedFreshMain240AcousticScorer Acoustic,
    LegacyCursiveSeparationTable Table)
{
    public static MainFixture Create(byte[] ak, byte[] lk, byte[] engine, byte[] tableBytes)
    {
        using var parsed = JsonDocument.Parse(engine);
        var activation = parsed.RootElement.GetProperty("activations");
        var activations = new LegacyInkActivations(
            activation.GetProperty("hidden").EnumerateArray().Select(v => v.GetInt32()).ToArray(),
            activation.GetProperty("output").EnumerateArray().Select(v => v.GetInt32()).ToArray());
        var table = LegacyCursiveSeparationTable.Load(tableBytes);
        var raw = new InkStroke(new InkPoint[]
        { new(0, 90), new(0, 10), new(25, 60), new(50, 10), new(50, 90) });
        var sampler = LegacyFreshSingleGlyphMainSampler.Prepare(ak, 0, raw, 0);
        var factory = sampler.Postprocessing.Input.FirstFactory;
        var row = factory.Frame.Row;
        var cuts = row.GenerateRussianCuts();
        var profile = LegacyFirstTransitionGeometryProfile.ReadSource(ak, 6120);
        var categories = new LegacyCursiveCategoryMapper(profile.Categories);
        var policy = LegacyFreshFirstControllerPolicy.ReadFirstSource(ak, 6120);
        var scope = new LegacyFirstRowPrimitiveScope(row, cuts,
            row.OriginalSource.Strokes.Select(s => s.Points.Count).ToArray());
        var scorer = LegacyFirstOrdinaryCacheScorer.Prepare(ak, 6120, row, activations);
        byte[] mask = Enumerable.Repeat((byte)1, scorer.Alphabet.LabelCount).ToArray();
        var seeds = LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts, row.Frame.Line.Height,
            LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(ak, 12727), [0], [-1]);
        var firstRanges = LegacyFreshFirstCandidateGraphPreparer.Prepare(ak, factory, cuts, seeds, table).SourceRanges;
        var graph = firstRanges.Graph;
        var links = new LegacyOrdinaryCandidateConnections(graph);
        var geometry = new LegacyFirstOrdinaryTransitionGeometry(profile, factory.Frame, cuts, graph, table);
        var separation = LegacyCursiveSeparationProfile.ReadV106Node(ak, profile.SamplerOffset);
        var scores = new LegacyFirstOrdinaryCacheScores[graph.Groups.Count];
        var ranked = new LegacyOrdinaryRankedAcousticGroup[graph.Groups.Count];
        for (int group = 0; group < graph.Groups.Count; group++)
        {
            var ranges = graph.Groups[group].Ranges.ToArray();
            int[] order = cuts.Chronology.OrderRangeIndicesForNormalization(ranges);
            var ordered = order.Select(i => ranges[i]).ToArray();
            var signs = LegacyCursiveGlyphBoundaryPolicy.Resolve(ordered, cuts.Cuts.PreparedInk.StrokePointCounts.Span, 0, false, false);
            scores[group] = scorer.Score(cuts, ranges, signs.Start, signs.End, 15,
                Math.Min(256, policy.MaximumNormalizedStrokes), mask,
                glyph => policy.CreateContext(scope.Evaluate(ranges, glyph.MergedGeometry.Strokes.Count), []), []);
            int cost = LegacyCursiveGroupSeparationCost.Evaluate(cuts.Cuts.PreparedInk, ranges, row.Frame.Line.Height,
                separation.GapOffset, separation.Slope, table.Values.Span, separation.MaximumCost);
            ranked[group] = new(scores[group].Cache, scorer.Alphabet, ak.AsSpan(scorer.AlphabetOffset, 16),
                Enumerable.Repeat(cost, categories.CompositeCategoryCount).ToArray(), 1);
        }
        //First regular/limited diagnostic proposals are kept explicit here.
        //The main source/frame and its four-family caches derive from the
        //actual materialized first result; this is not the full first winner.
        var requests = Enumerable.Range(0, graph.NodeCount).Select(_ => new List<LegacyFirstOrdinaryGlyphRequest>()).ToArray();
        for (int node = 0; node < graph.NodeCount; node++)
            for (byte span = (byte)Math.Min(4, node);; span--)
            {
                var arcs = graph.GetNode(node);
                for (int ordinal = 0; ordinal < arcs.Count; ordinal++)
                {
                    if (arcs[ordinal].Span != span) continue;
                    var group = ranked[arcs[ordinal].GroupId];
                    foreach (var entry in group.Entries.Span[..Math.Min(20, group.Entries.Length)])
                    {
                        if (!scorer.Alphabet.TryGetText(entry.Symbol, out string text) || text.Length != 1 ||
                            !(char.IsAsciiLetterOrDigit(text[0]) || text[0] is >= '\u0400' and <= '\u04ff')) continue;
                        foreach (bool end in new[] { false, true })
                        {
                            int category = categories.GetSymbolCompositeCategory(entry.Symbol, end);
                            if (category >= 0 && categories.GetFirstId(category) <= 5)
                                requests[node].Add(new((ushort)ordinal, span, entry.Symbol, (byte)category,
                                    group.Query(entry.Symbol, ushort.MaxValue, category, false, false),
                                    group.GetFirstControllerLikelihoodByte(entry.Symbol)));
                        }
                    }
                }
                if (span == 0) break;
            }
        var primary = LegacyFirstOrdinaryAcousticRecurrence.Run(links, requests, 5, geometry.Correct118, geometry.Correct140);
        int GlyphCost(int node, ushort arc, ushort symbol, byte category, CancellationToken token)
            => ranked[graph.GetNode(node)[arc].GroupId].Query(symbol, ushort.MaxValue, category, true, false, token);
        var merged = LegacyFirstOrdinaryContextPasses.RunForFreshSelectedChild(primary, links,
            factory.InvocationPolicy.Binding, GlyphCost, geometry.Correct118, geometry.Correct140, new());
        var materialized = new LegacyFirstPlainWordMaterializer(ak, 6120, factory, table)
            .Materialize(cuts, graph, primary, merged, scores);
        var sampled = sampler.ApplyFirstResult(materialized, firstRanges, table);
        var model = LegacyRussianModelBundle.Load(ak, lk, engine);
        var dictionary = LegacyOwnedRussianDictionaryPreparation.Prepare(ak, lk, model, sampled);
        return new(model, sampled, dictionary, dictionary.CreateAcousticScorer(ak), table);
    }
}
