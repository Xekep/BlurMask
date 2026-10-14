using InkRecognition;
using System.Text.Json;
if(args.Length!=3)throw new ArgumentException("case-data publication-data controls");
using var cs=File.OpenRead(args[0]);var data=LegacyNativeCaseData.Load(cs);
using var ps=File.OpenRead(args[1]);var publication=LegacyNativeTextPublicationData.Load(ps);
using var json=JsonDocument.Parse(File.ReadAllBytes(args[2]));int rows=0,callbacks=0,stops=0;
foreach(var item in json.RootElement.EnumerateArray())
{
 var a=item.GetProperty("alphabet");var alphabet=LegacyInkAlphabet.Load(Convert.FromBase64String(a.GetProperty("payload").GetString()!),a.GetProperty("version").GetUInt16());
 var ranges=LegacyNativeCaseAlphabetRanges.Prepare(alphabet,data,publication);var producer=new LegacyNativeCaseSymbolEnumeration(ranges);
 foreach(var row in item.GetProperty("rows").EnumerateArray())
 {
  ushort label=row[0].GetUInt16();ushort[] expected=row[1].EnumerateArray().Select(v=>v.GetUInt16()).ToArray();var actual=new List<ushort>();
  if(!producer.Enumerate(label,v=>{actual.Add(v);return true;}) || !actual.ToArray().AsSpan().SequenceEqual(expected))throw new Exception("Callback order/duplicates differ.");
  callbacks+=actual.Count;rows++;
  for(int stop=1;stop<=expected.Length;stop++)
  {
   actual.Clear();int at=stop;
   bool ok=producer.Enumerate(label,v=>{actual.Add(v);return actual.Count<at;});
   if(ok || !actual.ToArray().AsSpan().SequenceEqual(expected.AsSpan(0,stop)))throw new Exception("Native early callback stop differs.");stops++;
  }
  try{producer.Enumerate(label,_=>true,new(true));throw new Exception("Canceled callback accepted.");}catch(OperationCanceledException){}
 }
}
Console.WriteLine($"PASS {rows} raw mode-map symbol expansions/{callbacks} ordered callbacks/{stops} early-stop prefixes; literal controls, missing forms, repeated modes, native triplet repair. Full292 query/state/source factory remains separate.");
