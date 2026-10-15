using InkRecognition;
using System.Text.Json;
internal static class MainFixture
{
    public static LegacyFreshSingleGlyphMainSampled Create(byte[] ak,byte[] engineBytes,LegacyCursiveSeparationTable table)
    {
        using var engine=JsonDocument.Parse(engineBytes);var a=engine.RootElement.GetProperty("activations");
        var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
        var raw=new InkStroke(new InkPoint[]{new(0,90),new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)});
        var sampler=LegacyFreshSingleGlyphMainSampler.Prepare(ak,0,raw,0);var factory=sampler.Postprocessing.Input.FirstFactory;
        var row=factory.Frame.Row;var cuts=row.GenerateRussianCuts();var mask=LegacyFreshFirstAcousticMaskPolicy.Create(ak,factory);
        var firstPolicy=LegacyFreshFirstControllerPolicy.ReadFirstSource(ak,6120);var firstMask=mask.CopyMasks().Primary.ToArray();var pairs=LegacyWideSymbolPairs.CreateUnrestricted(factory.InvocationPolicy.Binding.Alphabet);
        var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,row.Frame.Line.Height,LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(ak,12727),[0],[-1]);
        var prepared=LegacyFreshFirstCandidateGraphPreparer.Prepare(ak,factory,cuts,seeds,table);var graph=prepared.SourceRanges.Graph;
        var scorer=LegacyFirstOrdinaryCacheScorer.Prepare(ak,6120,row,activations);var primitive=new LegacyFirstRowPrimitiveScope(row,cuts,[row.OriginalSource.Strokes[0].Points.Count]);
        var scores=new LegacyFirstOrdinaryCacheScores[graph.Groups.Count];
        for(int g=0;g<scores.Length;g++)
        {
            var ranges=graph.Groups[g].Ranges.ToArray();var order=cuts.Chronology.OrderRangeIndicesForNormalization(ranges);
            var clip=LegacyCursiveGlyphBoundaryPolicy.Resolve(order.Select(i=>ranges[i]).ToArray(),cuts.Cuts.PreparedInk.StrokePointCounts.Span,0,false,false);
            scores[g]=scorer.Score(cuts,ranges,clip.Start,clip.End,15,firstPolicy.MaximumNormalizedStrokes,firstMask,
                glyph=>firstPolicy.CreateContext(primitive.Evaluate(ranges,glyph.MergedGeometry.Strokes.Count),[]),pairs);
        }
        var producer=LegacyFreshFirstProtocolOwnership.Create(factory).PreparePrimary(ak,cuts,prepared,scores,table);
        var primary=producer.Run();var context=LegacyFirstBlankContextPasses.Run(ak,factory,cuts,prepared,producer,table);
        var word=new LegacyFirstPlainWordMaterializer(ak,6120,factory,table).MaterializeLiteralSymbols(ak,6120,cuts,graph,primary,context,scores);
        return sampler.ApplyFirstResult(word,prepared.SourceRanges,table);
    }
}
