using InkRecognition;
using System.Text.Json;
if(args.Length is not (2 or 5))throw new ArgumentException("case-data.json controls.json [publication.json AK.res LK.res]");
using var stream=File.OpenRead(args[0]);var data=LegacyNativeCaseData.Load(stream);
using var json=JsonDocument.Parse(File.ReadAllBytes(args[1]));
var expected=json.RootElement.GetProperty("counts").EnumerateArray().ToDictionary(r=>r[0].GetInt32(),r=>r[1].GetInt32());
int tested=0,nonzero=0;
for(int cp=0;cp<=0x10ffff;cp++)
{
 if(cp is >=0xd800 and <=0xdfff)continue;
 int count=data.GetAlternativeCount(cp);
 if(count!=expected.GetValueOrDefault(cp))throw new Exception($"Case count differs U+{cp:X}: {count}.");
 if(count!=0)nonzero++;tested++;
}
int variants=0;
foreach(var row in json.RootElement.GetProperty("variants").EnumerateArray())
{
 int cp=row[0].GetInt32();var expectedRows=row[1].EnumerateArray().ToArray();
 for(int capacity=0;capacity<=5;capacity++)
 {
  var actual=data.ReadAlternatives(cp,capacity);
  if(actual.TotalCount!=expectedRows.Length || actual.Written.Count!=Math.Min(capacity,expectedRows.Length))throw new Exception("Capacity/count differs.");
  for(int i=0;i<actual.Written.Count;i++)
  {
   var v=actual.Written[i];var e=expectedRows[i];
   if(!v.Lower.Span.SequenceEqual(e[0].EnumerateArray().Select(x=>x.GetInt32()).ToArray()) ||
      !v.Upper.Span.SequenceEqual(e[1].EnumerateArray().Select(x=>x.GetInt32()).ToArray()) ||
      !v.Title.Span.SequenceEqual(e[2].EnumerateArray().Select(x=>x.GetInt32()).ToArray()) ||
      v.LanguageFlags!=e[3].GetInt32() || v.ContextFlags!=e[4].GetInt32())throw new Exception($"Variant differs U+{cp:X}.");
   variants++;
  }
 }
}
foreach(int bad in new[]{-1,0xd800,0xdfff,0x110000})
 try{data.GetAlternativeCount(bad);throw new Exception("Invalid scalar admitted.");}catch(ArgumentOutOfRangeException){}
byte[] bytes=File.ReadAllBytes(args[0]);
using var truncated=new MemoryStream(bytes[..100]);try{LegacyNativeCaseData.Load(truncated);throw new Exception("Truncated JSON admitted.");}catch(JsonException){}
using var changed=new MemoryStream(System.Text.Encoding.UTF8.GetBytes(System.Text.Encoding.UTF8.GetString(bytes).Replace("      256,","      257,")));
try{LegacyNativeCaseData.Load(changed);throw new Exception("Changed numeric table admitted.");}catch(InvalidDataException){}
using var oversized=new MemoryStream(new byte[131073]);try{LegacyNativeCaseData.Load(oversized);throw new Exception("Oversized JSON admitted.");}catch(InvalidDataException){}
if(args.Length==5)
{
 using var ps=File.OpenRead(args[2]);var publication=LegacyNativeTextPublicationData.Load(ps);
 var alphabets=new[]{LegacyInkAlphabet.Load(File.ReadAllBytes(args[3]).AsSpan(99),0x103),LegacyInkAlphabet.Load(File.ReadAllBytes(args[4]).AsSpan(607074),0x103)};
 if(!alphabets[0].HasStoredRelationMaps || alphabets[1].HasStoredRelationMaps)throw new Exception("Actual acoustic/lexical stored-map branches differ.");
 foreach(var alphabet in alphabets)
 {
  if(!alphabet.HasStoredRelationMaps)for(ushort label=0;label<alphabet.LabelCount;label++)for(int mode=0;mode<3;mode++)
   if(alphabet.TryGetRelatedForms(label,mode,out _))throw new Exception("Fallback proof requires absent stored maps.");
  if(!data.HasCaseAlternatives(alphabet,publication))throw new Exception("Actual RU has case alternatives despite absent stored maps.");
  try{data.HasCaseAlternatives(alphabet,publication,new(true));throw new Exception("Canceled scan accepted.");}catch(OperationCanceledException){}
 }
 Console.WriteLine("PASS actual RU acoustic stored-map and lexical Unicode-fallback branches both activate052BD0 case predicate.");
}
Console.WriteLine($"PASS {tested} valid scalars/{nonzero} nonzero null-output case counts/{variants} exact written variants; micro-sign exception, invalid scalars, truncated and oversized data. Case292/contextual selection remains separate.");
