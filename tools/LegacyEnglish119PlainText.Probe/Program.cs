using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;
if(args.Length!=3)throw new ArgumentException("models-directory audited-controller-costs.json output.json");
using var input=JsonDocument.Parse(File.ReadAllBytes(args[1]));
int guards=0,words=0,delimiters=0,hidden=0,literal=0,multiple=0;var canceled=new CancellationToken(true);
void Guard(Action action){try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or NotSupportedException){guards++;return;}throw new Exception("Expected refusal.");}
InkStroke[] Ink(JsonElement e)=>e.EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
string path="en-US/en_US-ak-cur.lite.res";byte[] bytes=File.ReadAllBytes(Path.Combine(args[0],path));var parent=LegacySource247WireLayout.Read(bytes);
var model=LegacyFirstAcoustic119WireModel.ReadSelectedFirst(bytes,parent,parent.Children.First(c=>c.Kind==0x249));
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(Path.Combine(args[0],"ru-RU/legacy-separation-profile.json")));
using var file=File.Create(args[2]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();w.WriteString("Path",path);w.WriteString("Fingerprint",model.Source.Fingerprint);w.WriteStartArray("Actual");
int index=0;LegacyEnglish119TerminalResult? firstTerminal=null;
foreach(var reference in input.RootElement.GetProperty("Rows").EnumerateArray())
{
 var raw=Ink(reference.GetProperty("Raw"));var prepared=LegacyOwnedEnglishLiteFullPreprocessing.Prepare(model,raw);
 var candidates=LegacyOwnedEnglish119Candidates.Prepare(prepared,table);var cache=LegacyOwnedEnglish119AcousticCache.Prepare(candidates);
 var confidence=LegacyOwnedEnglish119GlyphConfidence.Prepare(cache);var costs=LegacyOwnedEnglish119ControllerCosts.Prepare(confidence,table);
 var primary=LegacyOwnedEnglish119FirstPrimary.Prepare(costs,table).Run();var contexts=LegacyOwnedEnglish119ContextPasses.Prepare(primary).Run();
 var terminal=LegacyOwnedEnglish119Terminal.Select(contexts);firstTerminal??=terminal;var alternative=terminal.Alternatives.Single();
 var groupIds=candidates.CopyLiveGroupIds();var caches=groupIds.Select(g=>cache.ScoreGroup(g)).ToArray();int scored=cache.ScoredGroupCount;
 w.WriteStartObject();w.WriteNumber("Index",index++);w.WriteNumber("ScoredBefore",scored);w.WriteNumber("Height",prepared.Line.Height);
 w.WriteStartArray("Trace");foreach(var cell in alternative.Cells){w.WriteStartObject();w.WriteStartArray("Coordinates");foreach(int v in new[]{cell.Node,cell.Row,cell.Column,(int)cell.Arc,cell.Symbol,cell.Span,cell.WordBoundaryBit?1:0,cell.PreviousNode,cell.PreviousArc})w.WriteNumberValue(v);w.WriteEndArray();w.WriteBase64String("Bytes",cell.CopyRecord().Span);w.WriteEndObject();}w.WriteEndArray();
 w.WriteStartArray("Variants");
 for(int residue=0;residue<2;residue++)
 {
  byte[] word=new byte[120],glyph=new byte[40];if(residue!=0){for(int i=0;i<120;i++)word[i]=(byte)(i*37+11);for(int i=0;i<40;i++)glyph[i]=(byte)(i*59+23);}
  foreach(int at in new[]{32,88,104})word.AsSpan(at,8).Clear();foreach(int at in new[]{96,112})word.AsSpan(at,4).Clear();
  var allocation=new LegacyEnglish119PlainAllocationStorage(word,glyph);byte[] originalWord=word.ToArray(),originalGlyph=glyph.ToArray();word[0]^=255;glyph[0]^=255;
  var text=LegacyOwnedEnglish119PlainText.Materialize(terminal,allocation);var again=LegacyOwnedEnglish119PlainText.Materialize(terminal,allocation);
  if(!ReferenceEquals(text.Terminal,terminal)||text.RawDescriptor.CurrentLineId!=-1||text.AssignedDescriptor.CurrentLineId!=0||text.RawDescriptor!=(text.AssignedDescriptor with{CurrentLineId=-1})||
     !text.AllocatedGlyphReferences.SequenceEqual(again.AllocatedGlyphReferences)||text.Words.Count!=again.Words.Count)throw new Exception("Actual same terminal/descriptor/allocation ownership changed.");
  var owners=text.CopyGlyphOwners();var repeated=again.CopyGlyphOwners();
  if(owners.Count!=repeated.Count||owners.Where((n,i)=>n.Key!=repeated[i].Key||n.PackedCount!=repeated[i].PackedCount||!n.Records.Span.SequenceEqual(repeated[i].Records.Span)).Any())throw new Exception("Repeated materialization changed native records.");
  for(int i=0;i<text.Words.Count;i++)if(!text.Words[i].CopyStorage().AsSpan().SequenceEqual(again.Words[i].CopyStorage())||!text.Words[i].CopyReadingOrderReferences().SequenceEqual(again.Words[i].CopyReadingOrderReferences()))throw new Exception("Repeated materialization changed WORD bytes.");
  if(text.Words.Count>0){var b=text.Words[0].CopyStorage();byte original=b[0];b[0]^=255;if(text.Words[0].CopyStorage()[0]!=original)throw new Exception("Mutable WORD escaped.");}
  foreach(var n in owners){byte[] b=n.Records.ToArray();if(b.Length>0){byte original=b[0];b[0]^=255;if(text.CopyGlyphOwners().Single(x=>x.Key==n.Key).Records.Span[0]!=original)throw new Exception("Mutable pool escaped.");}}
  Guard(()=>LegacyOwnedEnglish119PlainText.Materialize(terminal,allocation,canceled));
  Guard(()=>((IList<LegacyEnglish119PlainWordSnapshot>)text.Words).Add(null!));
  Guard(()=>text.CopyGlyphRecord(0xffffffffffffffff));
  w.WriteStartObject();w.WriteNumber("Residue",residue);w.WriteBase64String("WordAllocation",originalWord);w.WriteBase64String("GlyphAllocation",originalGlyph);w.WriteString("AllocationFingerprint",text.AllocationStorageFingerprint);
  w.WriteNumber("MinimumScore",text.MinimumOwnerFirstScore80);w.WriteStartArray("OwnerScores");foreach(var entry in text.OwnerFirstScores1C){w.WriteStartArray();w.WriteNumberValue(entry.Key);w.WriteNumberValue(BitConverter.SingleToInt32Bits(entry.Value));w.WriteEndArray();}w.WriteEndArray();
  w.WriteStartArray("Allocated");foreach(ulong r in text.AllocatedGlyphReferences)w.WriteNumberValue(r);w.WriteEndArray();w.WriteStartArray("Detached");foreach(ulong r in text.DetachedDelimiterReferences)w.WriteNumberValue(r);w.WriteEndArray();
  w.WriteStartArray("Owners");foreach(var owner in owners){w.WriteStartObject();w.WriteNumber("Key",owner.Key);w.WriteNumber("PackedCount",owner.PackedCount);w.WriteBase64String("Records",owner.Records.Span);w.WriteEndObject();}w.WriteEndArray();
  w.WriteStartArray("Words");foreach(var value in text.Words){w.WriteStartObject();w.WriteBoolean("Delimiter",value.IsDelimiter);w.WriteNumber("Total",value.TotalCostDelta);w.WriteNumber("Acoustic",value.AcousticCostDelta);w.WriteNumber("NodeSpan",value.SourceNodeSpan);w.WriteNumber("First",value.Span.First);w.WriteNumber("Last",value.Span.Last);w.WriteNumber("Ordinal",value.WordIndex);w.WriteBase64String("Storage",value.CopyStorage());w.WriteStartArray("References");foreach(ulong r in value.CopyReadingOrderReferences())w.WriteNumberValue(r);w.WriteEndArray();w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();
  if(residue==0){words+=text.Words.Count(x=>!x.IsDelimiter);delimiters+=text.Words.Count(x=>x.IsDelimiter);multiple+=text.Words.Count(x=>!x.IsDelimiter)>1?1:0;hidden+=alternative.Cells.Count(c=>c.Symbol is 211 or 217);literal+=alternative.Cells.Count(c=>c.Symbol==190);}
 }
 w.WriteEndArray();
 if(cache.ScoredGroupCount!=scored||groupIds.Where((g,i)=>!ReferenceEquals(caches[i],cache.ScoreGroup(g))).Any())throw new Exception("Rich materialization reclassified the same source.");
 w.WriteNumber("ScoredAfter",cache.ScoredGroupCount);w.WriteEndObject();
}
Guard(()=>new LegacyEnglish119PlainAllocationStorage(new byte[119],new byte[40]));Guard(()=>new LegacyEnglish119PlainAllocationStorage(new byte[120],new byte[39]));
foreach(int at in new[]{32,88,96,104,112}){var bad=new byte[120];bad[at]=1;Guard(()=>new LegacyEnglish119PlainAllocationStorage(bad,new byte[40]));}
Guard(()=>LegacyOwnedEnglish119PlainText.Materialize(null!,new(new byte[120],new byte[40])));
w.WriteEndArray();
//Explicit injected cancellation controls over the same completed actual first
//terminal. They test prefix/write order, not native cancellation reachability.
var firstFactory=LegacyOwnedEnglish119PlainResultFactory.Prepare(firstTerminal!,default);
var firstCell=firstFactory.Alternative.Cells.Single();
var actualCache=firstFactory.Cache(firstFactory.Graph.GetNode(firstCell.Node)[firstCell.Arc].GroupId,default);
Guard(()=>actualCache.ReadSymbolScore(firstCell.Symbol,1,default));Guard(()=>actualCache.ReadRequestCost(new byte[15],default));
Guard(()=>firstFactory.Cache(int.MaxValue,default));Guard(()=>firstFactory.CreatePlan(220,0,default));
var actualInterior=new LegacyOwnedEnglish119InteriorDelimiter(firstFactory,default);
Guard(()=>actualInterior.Read(-1,0,firstCell.Symbol,firstCell.Node,firstCell.Arc,firstCell.Symbol,default));
w.WriteStartArray("PreparedFailurePrefixes");
for(int phase=0;phase<3;phase++)
{
 var storage=firstFactory.CreateStorage(default);var operations=new LegacyFirstPlainTextOperations(firstFactory.ShouldAllocate,firstFactory.CreatePlan,
  (pool,r,plan,category,ct)=>{firstFactory.ApplySymbol(pool,r,plan,category,ct);if(phase==0)throw new OperationCanceledException();},
  _=>new((symbol,channel,ct)=>phase==1?throw new OperationCanceledException():actualCache.ReadSymbolScore(symbol,channel,ct),
   (ReadOnlySpan<byte> request,CancellationToken ct)=>phase==2?throw new OperationCanceledException():actualCache.ReadRequestCost(request,ct),actualCache.ReadFirstCandidateScore),actualInterior.Read);
 Guard(()=>LegacyNativeFirstPlainTextKernel.Run(storage,firstFactory.Graph,firstTerminal!.Context.Owner.Primary.Primary,firstFactory.Alternative.Choice,
  firstFactory.Alternative.Cells,1,0x135,operations,new byte[120],new byte[40],default));
 if(storage.WordPublisher.Head is not null)throw new Exception("A failed one-cell prefix was published.");
 w.WriteStartObject();w.WriteNumber("Phase",phase);w.WriteStartArray("Owners");foreach(var owner in storage.GlyphPool.CopyOwnerNodes()){w.WriteStartObject();w.WriteNumber("Key",owner.Key);w.WriteNumber("PackedCount",owner.PackedCount);w.WriteBase64String("Records",owner.Records.Span);w.WriteEndObject();}w.WriteEndArray();w.WriteEndObject();
}
w.WriteEndArray();w.WriteNumber("Words",words);w.WriteNumber("Delimiters",delimiters);w.WriteNumber("Hidden",hidden);w.WriteNumber("LiteralF0003",literal);w.WriteNumber("Multiple",multiple);w.WriteNumber("Guards",guards);w.WriteEndObject();w.Flush();
if(index!=124||hidden==0||literal==0)throw new Exception("Missing actual first source scope coverage.");
Console.WriteLine($"PASS actual119:{index} sources/two allocator patterns/{words}WORDs/{delimiters}delimiters/{hidden}hidden/{literal}literalF0003/{guards}guards; same cachedNN, no TOP18/UTF8/accepted text.");
