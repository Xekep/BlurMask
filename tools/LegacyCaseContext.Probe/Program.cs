using InkRecognition;
using System.Text.Json;
if(args.Length!=4)throw new ArgumentException("case-data normalization-data scalar-controls context-controls");
using var cs=File.OpenRead(args[0]);var cases=LegacyNativeCaseData.Load(cs);
using var ns=File.OpenRead(args[1]);var norm=LegacyNativeUnicodeNormalizationData.Load(ns);
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[3]));
using var vc=JsonDocument.Parse(File.ReadAllBytes(args[2]));
var variants=vc.RootElement.GetProperty("variants").EnumerateArray().ToDictionary(r=>r[0].GetInt32(),r=>r[1].EnumerateArray().ToArray());
int records=0,selected=0;
foreach(var item in controls.RootElement.EnumerateArray())
{
 int[] cp=item.GetProperty("cps").EnumerateArray().Select(x=>x.GetInt32()).ToArray();
 var context=LegacyNativeCaseContext.Prepare(cp,norm);if(!ReferenceEquals(context.CombiningData,norm))throw new Exception("Foreign CCC owner.");
 for(int i=0;i<cp.Length;i++)
 {
  byte[] actual=context.CopyRecord(i);if(!actual.AsSpan().SequenceEqual(Convert.FromHexString(item.GetProperty("records")[i].GetString()!)))throw new Exception("Context eight-byte record differs.");
  actual[4]^=1;if(context.CopyRecord(i)[4]==actual[4])throw new Exception("Context leaked mutable owner.");
  records++;
  for(uint request=0;request<4;request++)for(int language=0;language<5;language++)
  {
   int ix=item.GetProperty("selected")[i][checked((int)request*5+language)].GetInt32();
   var value=context.Select(cases,i,request,language);
   if(ix<0){if(value!=null)throw new Exception("Expected original-scalar fallback.");}
   else
   {
    if(value==null)throw new Exception("Missing selected variant.");var e=variants[cp[i]][ix];var v=value.Value;
    if(!v.Lower.Span.SequenceEqual(e[0].EnumerateArray().Select(x=>x.GetInt32()).ToArray()) ||
       !v.Upper.Span.SequenceEqual(e[1].EnumerateArray().Select(x=>x.GetInt32()).ToArray()) ||
       !v.Title.Span.SequenceEqual(e[2].EnumerateArray().Select(x=>x.GetInt32()).ToArray()) || v.LanguageFlags!=e[3].GetInt32() || v.ContextFlags!=e[4].GetInt32())throw new Exception("Ordered contextual selection differs.");
   }
   selected++;
  }
 }
}
foreach(int[] bad in new[]{Array.Empty<int>(),new[]{0xd800},new[]{-1},new[]{0x110000},Enumerable.Repeat(65,4097).ToArray()})
 try{LegacyNativeCaseContext.Prepare(bad,norm);throw new Exception("Invalid bounded context admitted.");}catch(ArgumentException){}
try{LegacyNativeCaseContext.Prepare(new[]{65},norm,new(true));throw new Exception("Canceled context admitted.");}catch(OperationCanceledException){}
Console.WriteLine($"PASS {records} independent complete8-byte case-context records/{selected} ordered variant selections; owner copy, exact CCC gaps, backward flags, opaque language fallback. Full04F170 normalized string lookup/case292 query still separate.");
