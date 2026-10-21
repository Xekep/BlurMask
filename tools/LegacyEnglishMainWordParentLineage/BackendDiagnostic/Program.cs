using InkRecognition;
using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
// Test-only projection of genuine pooled D0 rows through EXISTING pure kernels.
// Selected acoustic symbols/categories here are PREPARED numeric requests,
// not owning dictionary rows or proof of word-search encounter/reachability.
var model=File.ReadAllBytes(args[0]);var source=LegacySource247WireLayout.Read(model);
var categories=LegacyCursiveCategoryProfile.Load(model.AsSpan(182477),182477,out int consumed);
if(consumed!=183584-182477||categories.BaseEntryCount!=217||categories.CompositeCategories.Length!=42)throw new InvalidDataException();
var maps=new LegacyCursiveCategoryMapper(categories);
ushort[] bypass=new[]{"\U000f0001","\U000f0002","\U000f0003","\U000f0004","\U000f0008"}.Select(t=>source.Alphabet.TryGetLabel(t,out ushort x)?x:ushort.MaxValue).ToArray();
// Thresholds have actual source21D F0/F4/F8 values, though kind6 bypasses them.
var scorer=new LegacyNativeWordAcousticRequestScorer(new(20480,24576,32768),bypass);
using var d0=JsonDocument.Parse(File.ReadAllBytes(args[1]));using var geom=JsonDocument.Parse(File.ReadAllBytes(args[2]));
if(d0.RootElement.GetProperty("TargetsRead").GetBoolean())throw new InvalidDataException();
var geometry=geom.RootElement.GetProperty("rows").EnumerateArray().ToDictionary(r=>r.GetProperty("Name").GetString()!,r=>r.GetProperty("Groups").EnumerateArray().ToDictionary(g=>g[0].GetInt32(),g=>g[1].GetInt32()));
using var hash=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);long cases=0,accepted=0,rejected=0,selected=0,finite=0,missing=0;int owners=0;
foreach(var row in d0.RootElement.GetProperty("Rows").EnumerateArray()){
 owners++;var boxes=geometry[row.GetProperty("Name").GetString()!];
 foreach(var pass in row.GetProperty("Passes").EnumerateArray()){
 int node=pass.GetProperty("Anchor").GetInt32();byte span=pass.GetProperty("Span").GetByte();
 foreach(var selectedRow in pass.GetProperty("Selected").EnumerateArray()){
 selected++;int arc=selectedRow.GetProperty("Ordinal").GetInt32(),group=selectedRow.GetProperty("Group").GetInt32();
 var costs=selectedRow.GetProperty("Costs").EnumerateArray().Select(v=>v.GetInt32()).ToArray();if(costs.Length!=220)throw new InvalidDataException();
 var owned=new LegacyNativeWordAcousticGroupCosts(costs,Enumerable.Repeat(boxes[group],24).ToArray());bool haveImpossible=false;
 for(ushort symbol=0;symbol<217;symbol++){
 if(costs[symbol]==0x4000000){if(haveImpossible)continue;haveImpossible=true;}else finite++;
 for(int variant=0;variant<2;variant++){
 int category=maps.GetSymbolCompositeCategory(symbol,variant!=0);if(category<0){missing++;continue;}
 for(int pattern=0;pattern<2;pattern++){
 var residue=Enumerable.Range(0,80).Select(i=>(byte)((pattern==0?0xa5:0x3c)^i)).ToArray();
 var cell=new LegacyNativeWordTransitionCostCell(checked((ulong)cases+1),0);
 var request=LegacyNativeWordStartupRequestFactory.Create((uint)node,span,(ushort)arc,symbol,category,0x100000000UL,0,0,0xc0e,0,categories,cell,0,residue);
 int calls=0;bool ok=scorer.Score(request,(actualNode,actualArc)=>{if(actualNode!=node||actualArc!=arc)throw new InvalidDataException();calls++;return owned;},false);
 if(calls!=1)throw new InvalidDataException();if(ok)accepted++;else rejected++;
 byte[] packet=new byte[101];request.GetSnapshot().Span.CopyTo(packet);BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(80),cell.Value);packet[84]=ok?(byte)1:(byte)0;
 BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(85),node);BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(89),arc);BinaryPrimitives.WriteInt32LittleEndian(packet.AsSpan(93),group);BinaryPrimitives.WriteUInt16LittleEndian(packet.AsSpan(97),symbol);BinaryPrimitives.WriteUInt16LittleEndian(packet.AsSpan(99),(ushort)category);hash.AppendData(packet);cases++;
 }}}}}}
using var output=File.Create(args[3]);using var w=new Utf8JsonWriter(output,new(){Indented=true});w.WriteStartObject();
w.WriteString("Scope","TEST ONLY actual pooled D0 costs -> existing pure startup initializer/backend58/frontend128; PREPARED numeric requests, not dictionary-query authority or decoder");w.WriteNumber("Owners",owners);w.WriteNumber("SelectedRows",selected);w.WriteNumber("FiniteSymbols",finite);w.WriteNumber("MissingCategoryDiagnosticCases",missing);w.WriteNumber("Cases",cases);w.WriteNumber("Accepted",accepted);w.WriteNumber("Rejected",rejected);w.WriteString("Digest",Convert.ToHexStringLower(hash.GetHashAndReset()));w.WriteBoolean("TargetsRead",false);w.WriteBoolean("NativeExecuted",false);w.WriteBoolean("GenuineQueryRows",false);w.WriteEndObject();
