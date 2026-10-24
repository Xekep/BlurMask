using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args is ["--compact-data",var akPath,var priorPath,var compactOutput]){PreparedCompact.Run(akPath,priorPath,compactOutput);return;}
if(args is not[var output]||File.Exists(output))throw new ArgumentException("fresh-output");
var cases=new List<object>();int queries=0,guards=0;
GraphWordSourceNumerics.RequireSupportedReplacement(57,65533);GraphWordSourceNumerics.RequireSupportedReplacement(53,65533);
Reject(()=>GraphWordSourceNumerics.RequireSupportedReplacement(65533,65533));
for(int variant=0;variant<12;variant++)
{
    bool duplicate=(variant&1)!=0;byte shift=(byte)((variant/2)%2);bool reorder=variant>=8;
    var raw=Enumerable.Range(0,2+variant%3).Select(s=>new InkStroke(Enumerable.Range(0,5).SelectMany(p=>
    {
        var point=new InkPoint(s*30+p*4,(p%2)*20+s*3);
        return duplicate?new[]{point,point,point}:new[]{point};
    }).ToArray())).ToArray();
    var original=LegacyOrdinarySourceInk.Prepare(raw,new(0x135,false,0,0));
    var ink=LegacyCursivePreparedInk.Create(original.Strokes);var chronology=original.Chronology.WithInsertedPreparedInk(ink);
    int[] counts=ink.StrokePointCounts.ToArray();var markers=new List<LegacyCursiveCutMarker>();
    for(int s=0;s<counts.Length;s++){markers.Add(new(0,s,0));markers.Add(new(0,s,counts[s]/2));}
    var cuts=new LegacyCursiveCutResult(ink,markers.ToArray(),LegacyCursiveInkAtoms.Build(counts,markers.ToArray()));
    var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts,80,new(reorder,shift,.3f,.3f),new byte[counts.Length],Enumerable.Repeat(-1,counts.Length).ToArray());
    var source=LegacyOrdinaryGraphSourceRanges.Prepare(seeds,chronology,original,4);var graph=source.Graph;
    var seedGraph=shift==0?LegacyOrdinaryCursiveSeedGraph.CreateShiftZero(seeds):LegacyOrdinaryCursiveSeedGraph.CreateOrdinaryDeferred(seeds);
    var keys=LegacyOrdinaryGraphSourceRanges.CopyOwnedKeyRecords(original);
    var metadata=Enumerable.Range(0,counts.Length).Select(s=>Enumerable.Range(0,counts[s]).Select(p=>
    {var start=chronology.Resolve(s,p);var end=original.ResolveEndingMetadata(start);return new{Start=start.Key,End=end.Key,start.KeyRecordIndex,start.RemainderByte};}).ToArray()).ToArray();
    var results=new List<object>();
    for(int n=0;n<graph.NodeCount;n++)foreach(var arc in graph.GetNode(n))
    {
        var bounds=source.PreflightGraphWordForward(arc.StartContextId,arc.EndContextId,default);
        var count=source.ResolveForwardWithResiduals(arc.StartContextId,arc.EndContextId);
        var fill=source.ResolveForwardWithResiduals(arc.StartContextId,arc.EndContextId);
        Check(count.AsSpan().SequenceEqual(fill)&&fill.Length<=bounds.Depth);
        byte[] storage=GraphWordSourceNumerics.Encode26(fill,default);
        results.Add(new{First=arc.StartContextId,Last=arc.EndContextId,Bounds=bounds,Count=count,Fill=fill,Storage=storage});queries++;
        if(fill.Length>0){var old=fill[0];fill[0]=new(new(9,9,9),new(9,9,9),0,0);Check(source.ResolveForwardWithResiduals(arc.StartContextId,arc.EndContextId)[0]==old);fill[0]=old;}
    }
    var nodes=Enumerable.Range(0,seedGraph.NodeCount).Select(n=>seedGraph.GetNode(n).Select(a=>new{Group=a.SeedGroupIndex,Start=a.StartContextId,End=a.EndContextId,Next=a.SuccessorArcIndices.ToArray()}).ToArray()).ToArray();
    cases.Add(new{Variant=variant,Raw=raw.Select(s=>s.Points.Select(p=>new[]{p.X,p.Y}).ToArray()).ToArray(),Keys=keys,Metadata=metadata,Counts=counts,Factor=chronology.SourceFactor,Map=chronology.PreparedPointMap.ToArray(),
        Seeds=seeds.Seeds.Select(s=>s.Atom.Range).ToArray(),Nodes=nodes,Queries=results});
    using var cancelled=new CancellationTokenSource();cancelled.Cancel();bool reached=false;
    try{source.PreflightGraphWordForward(0,0,cancelled.Token);}catch(OperationCanceledException e){Check(e.CancellationToken==cancelled.Token);reached=true;}Check(reached);
    Reject(()=>source.PreflightGraphWordForward(uint.MaxValue,0,default));
    Reject(()=>source.PreflightGraphWordForward(1,0,default));
    Reject(()=>source.PreflightGraphWordForward(0,uint.MaxValue,default));
}
var duplicates=new List<object>();
foreach(int index in new[]{0,1,2})foreach(int initial in new[]{1,2})foreach(bool cls in new[]{false,true})foreach(float a in new[]{-2f,0f,2f})foreach(float b in new[]{-2f,0f,2f})
    duplicates.Add(new{Index=index,Initial=initial,Class=cls,NewCost=a,OldCost=b,Update=GraphWordSourceNumerics.ShouldUpdateDuplicate(index,initial,cls,a,b)});
var ranks=new List<object>();
for(int count=1;count<=7;count++)for(int seed=0;seed<8;seed++)
{
    var words=Enumerable.Range(0,count).Select(i=>{var b=new byte[120];BinaryPrimitives.WriteInt32LittleEndian(b,(i+seed)%3);BinaryPrimitives.WriteSingleLittleEndian(b.AsSpan(16),(i%3-1)*.25f);BinaryPrimitives.WriteSingleLittleEndian(b.AsSpan(28),(i%4)*.125f);return b;}).ToArray();
    bool Equal(ReadOnlySpan<byte> a,ReadOnlySpan<byte> b)=>BinaryPrimitives.ReadInt32LittleEndian(a)==BinaryPrimitives.ReadInt32LittleEndian(b);
    int[] selected=[seed%count];var ranked=LegacyPreparedMainWordRanking.Rank(words,selected,2,false,Equal);
    var scored=LegacyPreparedMainSpanScorePublication.Normalize(ranked.Words);
    ranks.Add(new{Input=words,Selected=selected,Ranked=ranked,Scored=scored});
}
File.WriteAllText(output,JsonSerializer.Serialize(new{Scope="PREPARED_SOURCE_METADATA_COUNT_FILL_AND_SHARED_RANK_ONLY_NO_EN_LIVE_AUTHORITY",NN=0,Cases=cases,Duplicates=duplicates,Ranks=ranks,Queries=queries,Guards=guards}));
Console.WriteLine($"PASS prepared {cases.Count} raw source fixtures/{queries} count-fill pairs/{duplicates.Count} duplicate predicates/{ranks.Count} rank+mass cases/{guards} checks; NN0");
void Check(bool condition){if(!condition)throw new InvalidDataException("Prepared check");guards++;}
void Reject(Action a){try{a();}catch(NotSupportedException){guards++;return;}throw new InvalidDataException("Expected source refusal");}
