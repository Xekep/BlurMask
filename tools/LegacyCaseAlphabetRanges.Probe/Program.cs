using InkRecognition;
using System.Text.Json;
if(args.Length!=3)throw new ArgumentException("case-data publication-data controls");
using var cs=File.OpenRead(args[0]);var cases=LegacyNativeCaseData.Load(cs);
using var ps=File.OpenRead(args[1]);var publication=LegacyNativeTextPublicationData.Load(ps);
using var json=JsonDocument.Parse(File.ReadAllBytes(args[2]));int count=0,rejected=0,predicates=0;
foreach(var item in json.RootElement.EnumerateArray())
{
 var alphabet=LegacyInkAlphabet.Load(Convert.FromBase64String(item.GetProperty("payload").GetString()!),item.GetProperty("version").GetUInt16());
 if(alphabet.IsCaseRangeMetadataEnabled!=item.GetProperty("enabled").GetBoolean())throw new Exception("Native case-enabled metadata differs.");
 if(alphabet.HasStoredRelationMaps)
  for(ushort label=0;label<alphabet.LabelCount;label++)
  {if(cases.IsCasedSymbol(alphabet,label,publication)!=item.GetProperty("storedBits")[label].GetBoolean())throw new Exception("Stored map predicate used Unicode fallback.");predicates++;}
 if(item.GetProperty("expected").ValueKind==JsonValueKind.Null)
 {try{LegacyNativeCaseAlphabetRanges.Prepare(alphabet,cases,publication);throw new Exception("Third cased run accepted.");}catch(InvalidDataException){rejected++;}}
 else
 {
  var result=LegacyNativeCaseAlphabetRanges.Prepare(alphabet,cases,publication);
  ushort[] actual=[result.FirstStart,result.FirstEnd,result.SecondStart,result.SecondEnd];
  if(!actual.AsSpan().SequenceEqual(item.GetProperty("expected").EnumerateArray().Select(x=>x.GetUInt16()).ToArray()) || !ReferenceEquals(result.Alphabet,alphabet))throw new Exception("Cased ranges/owner differ.");
  if(item.GetProperty("name").GetString()!.StartsWith("actual"))Console.WriteLine($"Actual {item.GetProperty("name").GetString()}: [{string.Join(",",actual)}].");
 }
 try{LegacyNativeCaseAlphabetRanges.Prepare(alphabet,cases,publication,new(true));throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
 count++;
}
Console.WriteLine($"PASS {count} exact byte-alphabet ranges/{rejected} third-run refusals/{predicates} stored-map predicates;104 enabled overrides, native sentinel/one-past-end and copy-owner/cancellation. Wrapper292 query integration remains separate.");
