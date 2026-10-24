namespace InkRecognition;

public sealed partial class LegacyOrdinaryGraphSourceRanges
{
    internal sealed record GraphWordDiagnosticArc(int Group,uint Start,uint End,int[] Next);
    internal sealed record GraphWordDiagnosticSeed(int Stroke,int First,int End);
    internal sealed record GraphWordDiagnosticInputs(GraphWordDiagnosticArc[][] Nodes,GraphWordDiagnosticSeed[] Seeds,
        int[] PreparedCounts,uint[] PreparedMap,uint Factor,LegacySourcePointIdentity[][] PreparedIdentities,
        LegacySourcePointKey[] Keys,int[][] OriginalIndices,float[][] OriginalXY,
        LegacySourcePointIdentity[][] OriginalIdentities,LegacySourcePointKey[][] OriginalEndKeys);

    // Explicit bounded detached diagnostic DATA. Never accepted by a source,
    // lease, range issuer or glyph factory. Private actual inputs only.
    internal GraphWordDiagnosticInputs CopyGraphWordDiagnosticInputs(CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        var actualSeeds=seeds.Seeds;int total=0,arcs=0;
        if(seedGraph.NodeCount is<1 or>16||actualSeeds.Count is<1 or>128||keys.Length is<1 or>128||
            prepared.StrokePointCounts.Length is<1 or>16||original.Strokes.Count is<1 or>16)
            throw new NotSupportedException("Small source diagnostic bounds required before copies.");
        foreach(int count in prepared.StrokePointCounts.Span)total=checked(total+count);
        int originalTotal=0;foreach(var stroke in original.Strokes)originalTotal=checked(originalTotal+stroke.Points.Count);
        for(int n=0;n<seedGraph.NodeCount;n++)
        {
            var actual=seedGraph.GetNode(n);arcs=checked(arcs+actual.Count);
            for(int a=0;a<actual.Count;a++)if(actual[a].SuccessorArcIndices.Length>128)
                throw new NotSupportedException("Bounded diagnostic successors required before copies.");
        }
        if(total>128||originalTotal>128||arcs>128)
            throw new NotSupportedException("Diagnostic points/arcs exceed precharged128KiB payload.");
        var nodes=new GraphWordDiagnosticArc[seedGraph.NodeCount][];
        for(int n=0;n<nodes.Length;n++)
        {
            token.ThrowIfCancellationRequested();var actual=seedGraph.GetNode(n);nodes[n]=new GraphWordDiagnosticArc[actual.Count];
            for(int a=0;a<actual.Count;a++)
            {
                var arc=actual[a];if(arc.SuccessorArcIndices.Length>128)throw new NotSupportedException("Bounded diagnostic successors required.");
                nodes[n][a]=new(arc.SeedGroupIndex,arc.StartContextId,arc.EndContextId,arc.SuccessorArcIndices.ToArray());
            }
        }
        var seedRows=new GraphWordDiagnosticSeed[actualSeeds.Count];
        for(int i=0;i<seedRows.Length;i++)
        {token.ThrowIfCancellationRequested();var range=actualSeeds[i].Atom.Range;seedRows[i]=new(range.StrokeIndex,range.StartPoint,range.EndPointExclusive);}
        var counts=prepared.StrokePointCounts.ToArray();var identities=new LegacySourcePointIdentity[counts.Length][];
        for(int s=0;s<counts.Length;s++)
        {identities[s]=new LegacySourcePointIdentity[counts[s]];for(int at=0;at<counts[s];at++){token.ThrowIfCancellationRequested();identities[s][at]=prepared.Resolve(s,at);}}
        var originalIds=new LegacySourcePointIdentity[original.Strokes.Count][];
        var ending=new LegacySourcePointKey[original.Strokes.Count][];
        var indices=new int[original.Strokes.Count][];var xy=new float[original.Strokes.Count][];
        for(int s=0;s<original.Strokes.Count;s++)
        {
            token.ThrowIfCancellationRequested();var points=original.Strokes[s].Points;
            originalIds[s]=new LegacySourcePointIdentity[points.Count];ending[s]=new LegacySourcePointKey[points.Count];
            indices[s]=original.OriginalPointIndices[s].ToArray();xy[s]=new float[checked(points.Count*2)];
            for(int at=0;at<points.Count;at++)
            {
                token.ThrowIfCancellationRequested();var id=original.Chronology.Resolve(s,at);originalIds[s][at]=id;
                ending[s][at]=original.ResolveEndingMetadata(id,token).Key;xy[s][at*2]=points[at].X;xy[s][at*2+1]=points[at].Y;
            }
        }
        token.ThrowIfCancellationRequested();
        return new(nodes,seedRows,counts,prepared.PreparedPointMap.ToArray(),prepared.SourceFactor,identities,
            keys.ToArray(),indices,xy,originalIds,ending);
    }
}
