using InkRecognition;
using System.Buffers.Binary;
using System.Text;
using System.Text.Json;
if(args.Length is not (2 or 3))throw new ArgumentException("actualAK.res controls.json [engine-profile.json]");
byte[] resource=File.ReadAllBytes(args[0]);InkStroke[] raw=[new(new InkPoint[]{new(0,0),new(60,0),new(60,80),new(0,80),new(0,0)}),new(new InkPoint[]{new(20,30),new(30,50)})];
var factory=LegacyFreshFirstResultOwnershipFactory.Prepare(resource,6120,raw);var planOwner=LegacyFreshFirstGlyphSymbolPlan.ReadSource(resource,6120,factory);var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
void Require(bool value,string reason){if(!value)throw new Exception(reason);}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException){return;}throw new Exception("Invalid literal symbol owner admitted.");}
using var doc=JsonDocument.Parse(File.ReadAllBytes(args[1]));int controls=0;
foreach(var c in doc.RootElement.EnumerateArray())
{
 ushort label=c.GetProperty("label").GetUInt16();byte category=c.GetProperty("primary").GetByte();var plan=planOwner.Create(label,category);
 Require(plan.PublishedLabel==c.GetProperty("published").GetUInt16()&&plan.CompositeCategory==c.GetProperty("category").GetInt32()&&plan.GeometryIndex==c.GetProperty("geometry").GetByte()&&plan.CategoryMismatchFlag4==c.GetProperty("mismatch").GetUInt16()&&plan.PublicationFlags6==c.GetProperty("flags").GetUInt16(),$"Actual fresh literal plan differs: {plan}; independent {c.GetRawText()}.");
 byte[] before=Convert.FromBase64String(c.GetProperty("before").GetString()!),expected=Convert.FromBase64String(c.GetProperty("after").GetString()!);
 int descriptor=BinaryPrimitives.ReadInt32LittleEndian(before);var added=pool.AppendDescriptor((ulong)controls,descriptor,false,before);pool.ReplaceRecord(added.Reference,before);planOwner.Apply(pool,added.Reference,plan,category);
 Require(pool.CopyRecord(added.Reference).SequenceEqual(expected),"13AF30/1F42 update changed scalar/descriptor or unrelated flags.");controls++;
}
Require(factory.InvocationPolicy.Binding.Alphabet.TryGetLabel("\U000f0001",out ushort closed),"Actual closed primitive missing.");
var literal=planOwner.Create(closed,0);var p=factory.CreatePlainResult();var word=LegacyNativeWordBuilder.CreateEmpty(p.GlyphPool,new byte[120]);var glyph=word.AppendGlyph(0,0,false,new byte[40]);planOwner.Apply(p.GlyphPool,glyph.Reference,literal,0);word.SetLastOrdinarySymbol(literal.PublishedLabel);
byte[] row=p.GlyphPool.CopyRecord(glyph.Reference);byte[] utf8=LegacyNativeOrdinaryGlyphUtf8.Emit(new(BinaryPrimitives.ReadUInt16LittleEndian(row.AsSpan(4)),BinaryPrimitives.ReadUInt16LittleEndian(row.AsSpan(8))),factory.InvocationPolicy.Binding.Alphabet);
Require(Encoding.UTF8.GetString(utf8)=="\U000f0001"&&word.GetSymbolSpan()==1,"Fresh closed primitive was silently replaced with a guessed character or removed.");
var published=p.WordPublisher.Publish(word,(_,_)=>0,(_,_)=>1);Require(published.Word.FlatReferences.SequenceEqual(new[]{glyph.Reference}),"Literal symbol publication lost glyph ownership.");
Reject(()=>planOwner.Create(195,0));Reject(()=>planOwner.Create(0,64));Reject(()=>planOwner.Create(closed,0,new CancellationToken(true)));
Reject(()=>planOwner.Apply(pool,0,literal with{PublishedLabel=0},0));
Console.WriteLine($"PASS {controls} independent actual fresh6C plans/40-byte glyph updates; F0001 literal private UTF8 and single-reference WORD275 publication preserved. These are prepared selected-label controls, not whole-word winner/handwriting accuracy.");
if(args.Length==3)
{
 using var engine=JsonDocument.Parse(File.ReadAllBytes(args[2]));var a=engine.RootElement.GetProperty("activations");var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
 var rowSource=factory.Frame.Row;var cuts=rowSource.GenerateRussianCuts();var policy=LegacyFreshFirstControllerPolicy.ReadFirstSource(resource,6120);var primitiveScope=new LegacyFirstRowPrimitiveScope(rowSource,cuts,raw.Select(s=>s.Points.Count).ToArray());
 var scorer=LegacyFirstOrdinaryCacheScorer.Prepare(resource,6120,rowSource,activations);var mask=Enumerable.Repeat((byte)1,scorer.Alphabet.LabelCount).ToArray();
 var seeds=LegacyOrdinaryCursiveSeedProducer.GenerateLeftToRight(cuts.Cuts,rowSource.Frame.Line.Height,LegacyOrdinaryCursiveSeedProfile.ReadV10aResourceNode(resource,12727),new byte[raw.Length],Enumerable.Repeat(-1,raw.Length).ToArray());
 var graph=LegacyOrdinaryCursiveCandidateGraph.Generate(seeds,LegacyOrdinaryCursiveSeedGraph.CreateShiftZero(seeds),5);var scores=new LegacyFirstOrdinaryCacheScores[graph.Groups.Count];
 for(int g=0;g<scores.Length;g++)
 {
  var ranges=graph.Groups[g].Ranges.ToArray();var order=cuts.Chronology.OrderRangeIndicesForNormalization(ranges);var clip=LegacyCursiveGlyphBoundaryPolicy.Resolve(order.Select(i=>ranges[i]).ToArray(),cuts.Cuts.PreparedInk.StrokePointCounts.Span,0,false,false);
  scores[g]=scorer.Score(cuts,ranges,clip.Start,clip.End,15,Math.Min(256,policy.MaximumNormalizedStrokes),mask,state=>policy.CreateContext(primitiveScope.Evaluate(ranges,state.MergedGeometry.Strokes.Count),[]),[]);
 }
 var confidence=new LegacyFirstOwnedGlyphConfidence(LegacyFirstOrdinaryGlyphScoreProducer.ReadFreshSelectedSource(resource,6120),factory,cuts,graph,scores);
 int retained=0;
 for(int g=0;g<scores.Length;g++)
 {
  var candidates=scores[g].Cache.Candidates.ToArray();int rank=Array.FindIndex(candidates,c=>c.Symbol==closed);if(rank<0||rank>=20)continue;
  int physical=graph.Groups[g].Ranges.Span[0].StrokeIndex;float actualProbability=confidence.ReadSymbolScore(g,closed);
  Require(actualProbability==rowSource.OriginalSource.SourceStrokeProbabilities.Span[physical]&&actualProbability>0,"Actual retained primitive must carry its original source service value.");
  var sourcePlan=planOwner.Create(candidates[rank].Symbol,0);var result=factory.CreatePlainResult();var literalWord=LegacyNativeWordBuilder.CreateEmpty(result.GlyphPool,new byte[120]);var actualGlyph=literalWord.AppendGlyph((ulong)g,rank,false,new byte[40]);
  planOwner.Apply(result.GlyphPool,actualGlyph.Reference,sourcePlan,0);literalWord.SetLastOrdinarySymbol(sourcePlan.PublishedLabel);
  byte[] actualRow=result.GlyphPool.CopyRecord(actualGlyph.Reference);BinaryPrimitives.WriteSingleLittleEndian(actualRow.AsSpan(16),actualProbability);result.GlyphPool.ReplaceRecord(actualGlyph.Reference,actualRow);
  byte[] actualUtf8=LegacyNativeOrdinaryGlyphUtf8.Emit(new(BinaryPrimitives.ReadUInt16LittleEndian(actualRow.AsSpan(4)),BinaryPrimitives.ReadUInt16LittleEndian(actualRow.AsSpan(8))),factory.InvocationPolicy.Binding.Alphabet);
  Require(actualUtf8.SequenceEqual("\U000f0001"u8.ToArray()),"Actual retained native primitive was replaced by a guessed character.");
  var actualPublished=result.WordPublisher.Publish(literalWord,(_,_)=>0,(_,_)=>1);Require(actualPublished.Word.FlatReferences.SequenceEqual(new[]{actualGlyph.Reference}),"Actual retained primitive ownership lost at publication.");retained++;
 }
 Require(retained>0,"Actual NN/final-cache fixture did not retain a closed primitive.");
 Console.WriteLine($"PASS actual rawsource/frame/cuts/NN/{scores.Length} groups ->{retained} retained F0001 candidates -> literal kind0/UTF8/WORD275. Retained candidate chosen explicitly; native full273 winner remains separate.");
}
