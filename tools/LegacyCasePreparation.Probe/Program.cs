using InkRecognition;
using System.Text.Json;
if(args.Length!=3)throw new ArgumentException("case-data publication-data controls");
using var cs=File.OpenRead(args[0]);var data=LegacyNativeCaseData.Load(cs);
using var ps=File.OpenRead(args[1]);var publication=LegacyNativeTextPublicationData.Load(ps);
using var json=JsonDocument.Parse(File.ReadAllBytes(args[2]));int rows=0,alphabets=0;
foreach(var item in json.RootElement.EnumerateArray())
{
 var a=item.GetProperty("alphabet");var alphabet=LegacyInkAlphabet.Load(Convert.FromBase64String(a.GetProperty("payload").GetString()!),a.GetProperty("version").GetUInt16());
 var ranges=LegacyNativeCaseAlphabetRanges.Prepare(alphabet,data,publication);
 var producer=LegacyNativeCasePreparation.Prepare(ranges,publication);alphabets++;
 foreach(var row in item.GetProperty("rows").EnumerateArray())
 {
  ushort label=row[0].GetUInt16();
  for(int mode=0;mode<2;mode++)
  {
   if(producer.ReadClassification(label,mode==1)!=row[mode+1].GetByte())throw new Exception("Classification mismatch.");
   bool present=producer.TryReadForms(label,mode==1,out var forms);var expected=row[mode+3];
   if(present!=(expected.ValueKind!=JsonValueKind.Null))throw new Exception("Form presence mismatch.");
   if(present && !new[]{forms.First,forms.Second,forms.Third}.AsSpan().SequenceEqual(expected.EnumerateArray().Select(v=>v.GetUInt16()).ToArray()))throw new Exception("Stored form mismatch.");
  }
  rows++;
 }
 foreach(ushort invalid in new ushort[]{(ushort)alphabet.LabelCount,65506})
  try{producer.ReadClassification(invalid);throw new Exception("Outside domain accepted.");}catch(ArgumentOutOfRangeException){}
 try{LegacyNativeCasePreparation.Prepare(ranges,publication,new(true));throw new Exception("Canceled accepted.");}catch(OperationCanceledException){}
}
Console.WriteLine($"PASS {alphabets} stored alphabets/{rows} extended symbols, two classification tables and owned form maps. Stateful292 queries are not implemented by this producer.");
