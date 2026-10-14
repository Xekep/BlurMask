using InkRecognition;
using System.Buffers.Binary;
using System.Text;
using System.Text.Json;

if(args.Length is not (1 or 2))throw new ArgumentException("controls.json [actual-ru-ak.res]");
using var doc=JsonDocument.Parse(File.ReadAllBytes(args[0]));string[] texts=doc.RootElement.GetProperty("texts").EnumerateArray().Select(v=>v.GetString()!).ToArray();var alphabet=LegacyInkAlphabet.Load(BuildAlphabet(texts),0x103);int controls=0;
void Require(bool v,string message){if(!v)throw new Exception(message);}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException or OperationCanceledException){return;}throw new Exception("Invalid remap input admitted.");}
foreach(var c in doc.RootElement.GetProperty("cases").EnumerateArray())
{
 byte[] header=new byte[16];for(int i=0;i<7;i++)BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(i*2),ushort.MaxValue);
 BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(2),c.GetProperty("dash").GetUInt16());BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(4),c.GetProperty("closed").GetUInt16());BinaryPrimitives.WriteUInt16LittleEndian(header.AsSpan(14),(ushort)texts.Length);
 var groups=c.GetProperty("groups").EnumerateArray().Select(g=>new LegacySymbolMaskKnowledgeGroup(g.GetProperty("labels").EnumerateArray().Select(v=>v.GetUInt16()).ToArray(),g.GetProperty("weight").GetSingle())).ToArray();
 var map=new LegacyAcousticConfidenceLabelMap(alphabet,header,c.GetProperty("width").GetInt32(),groups);
 Require(map.ExplicitMappings.OrderBy(v=>v.Key).Select(v=>new[]{(int)v.Key,(int)v.Value}).Zip(c.GetProperty("map").EnumerateArray()).All(p=>p.First.SequenceEqual(p.Second.EnumerateArray().Select(v=>v.GetInt32())))&&map.ExplicitMappings.Count==c.GetProperty("map").GetArrayLength(),"Native3A0 positive-group/insert-first/explicit prototype order differs.");
 for(ushort label=0;label<texts.Length+3;label++)Require(map.TryResolveOutputLabel(label,out ushort target)==c.GetProperty("outputs")[label][0].GetBoolean()&&target==c.GetProperty("outputs")[label][1].GetUInt16(),"One-step map/closed service/output bounds differ.");
 Reject(()=>map.TryResolveOutputLabel((ushort)(texts.Length+3),out _));
 Reject(()=>new LegacyAcousticConfidenceLabelMap(alphabet,header,c.GetProperty("width").GetInt32(),groups,new CancellationToken(true)));controls++;
}
Console.WriteLine($"PASS {controls} independent confidence-index maps; firstpositiveNNmember/group order, no recursive remap, prototype fallbacks, closed-service refusal, all label/output bounds and cancellation.");
{
 var missingAlphabet=LegacyInkAlphabet.Load(BuildAlphabet(new[]{"x","-"}),0x103);byte[] missingHeader=new byte[16];
 for(int i=0;i<7;i++)BinaryPrimitives.WriteUInt16LittleEndian(missingHeader.AsSpan(i*2),ushort.MaxValue);
 BinaryPrimitives.WriteUInt16LittleEndian(missingHeader.AsSpan(14),2);
 var missing=new LegacyAcousticConfidenceLabelMap(missingAlphabet,missingHeader,2,[]);
 Require(missing.ExplicitMappings[3]==ushort.MaxValue&&missing.ExplicitMappings[4]==ushort.MaxValue&&!missing.TryResolveOutputLabel(3,out _)&&!missing.TryResolveOutputLabel(4,out _),"Missing quote target must remain nativeFFFF rather than disappear or acquire probability.");
}
if(args.Length==2)
{
 byte[] resource=File.ReadAllBytes(args[1]);var map=LegacyAcousticConfidenceLabelMap.ReadFreshFirstSource(resource,6120);
 var ownedAlphabet=LegacyFreshFirstTextContextPolicy.ReadForFreshSelectedChild(resource,6120).Alphabet;Require(map.OutputWidth==180&&map.SymbolCount==192,"Real first source widths changed.");
 foreach(var entry in map.ExplicitMappings)Console.WriteLine($"actual confidence-only remap {entry.Key}->{entry.Value}");
 Require(ownedAlphabet.TryGetLabel("\u00ad",out ushort soft)&&soft==183&&map.TryResolveOutputLabel(soft,out ushort softBase)&&softBase==140,"Pinned first soft hyphen183 prototype remap omitted.");
 foreach(string text in new[]{"-","_","\U000f0002","\U000f0003","\u4e00"})if(ownedAlphabet.TryGetLabel(text,out ushort label))
  Require(map.TryResolveOutputLabel(label,out ushort baseLabel)&&baseLabel==140,"Real prototype confidence remap omitted.");
 Require(ownedAlphabet.TryGetLabel("\U000f0001",out ushort closed)&&!map.TryResolveOutputLabel(closed,out _),"Closed primitive acquired a guessed neural index.");
 Require(map.ExplicitMappings.Count>0&&!map.ExplicitMappings.Any(pair=>ownedAlphabet.TryGetText(pair.Key,out string text)&&text=="\u043c"&&ownedAlphabet.TryGetText(pair.Value,out string value)&&value=="M"),"Cyrillic/Latin was replaced ad-hoc.");
 Console.WriteLine("PASS real first192/output180 confidence map; actual7aliases includingAD183, no widepair guess, closed probability service remains unsupported. Output symbol text is never rewritten.");
}
static byte[] BuildAlphabet(string[] values)
{
 using var stream=new MemoryStream();using var writer=new BinaryWriter(stream);int count=values.Length;
 for(int i=0;i<7;i++)writer.Write(ushort.MaxValue);writer.Write((ushort)count);writer.Write((ushort)1);writer.Write((ushort)0);
 foreach(int n in new[]{32,64})for(int i=0;i<n;i++)writer.Write(i==n-1?(ushort)0:ushort.MaxValue);
 writer.Write((ushort)0);writer.Write((ushort)count);int offset=0;
 for(int i=0;i<=count;i++){writer.Write((uint)offset);writer.Write((uint)(i<count?i:0));if(i<count)offset+=Encoding.UTF8.GetByteCount(values[i]);}
 foreach(string value in values)writer.Write(Encoding.UTF8.GetBytes(value));for(int i=0;i<count;i++)writer.Write((ushort)i);writer.Write((byte)0);writer.Write((ushort)0);return stream.ToArray();
}
