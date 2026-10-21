using InkRecognition;
using System.Text.Json;
using System.Security.Cryptography;
if(args.Length is not(2 or 3))throw new ArgumentException("MODELS CAPTURE [BLIND]");
byte[] R(string n)=>File.ReadAllBytes(Path.Combine(args[0],n));
var model=ExperimentalGlobalTextRecognizer.Load(R("ru-RU/ru_RU-ak-cur.lite.res"),R("ru-RU/ru_RU-lk-text.lite.res"),R("ru-RU/legacy-engine-profile.json"),R("line-geometry/line-2e3-30.res"),R("line-geometry/line-2e3-50.res"),R("ru-RU/legacy-separation-profile.json"),R("text-unicode/legacy-case-data.json"),R("text-unicode/legacy-text-publication-data.json"));
InkStroke Draw(params (float X,float Y)[] p)=>new(p.Select(x=>new InkPoint(x.X,x.Y)).ToArray());
var samples=new List<(string Name,InkStroke[] Ink)> {
 ("M",[Draw((0,90),(0,10),(25,60),(50,10),(50,90))]),
 ("MIR-ui",[Draw((0,90),(0,10),(25,60),(50,10),(50,90)),Draw((65,90),(65,10)),Draw((65,90),(115,10)),Draw((115,90),(115,10)),Draw((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))])};
int guards=0;void Empty(InkStroke[] ink){var r=model.Recognize(ink);if(r.Text.Length!=0||r.Confidence!=0||!r.ExperimentalCandidateScore)throw new Exception("Unsupported ink admitted.");guards++;}
Empty([]);Empty([Draw((float.NaN,0))]);Empty([Draw((float.PositiveInfinity,0))]);Empty([Draw((100001,0))]);Empty([new(Array.Empty<InkPoint>())]);Empty(Enumerable.Repeat(samples[0].Ink[0],33).ToArray());Empty([new(Enumerable.Range(0,1025).Select(i=>new InkPoint(i,i)).ToArray())]);
using(var c=new CancellationTokenSource()){c.Cancel();try{model.Recognize(samples[0].Ink,c.Token);throw new Exception("Caller cancellation ignored.");}catch(OperationCanceledException){guards++;}}
var publicResult=model.Recognize(samples[1].Ink);if(publicResult.Text!="МИР"||publicResult.Confidence<.5f||!publicResult.ExperimentalCandidateScore)throw new Exception("Public global adapter missed actual diagnostic word.");guards++;
var changing=samples[1].Ink.Select(s=>new ChangingPoints(s.Points)).ToArray();
var stable=model.Recognize(changing.Select(s=>new InkStroke(s)).ToArray());
if(stable.Text!="МИР"||changing.Any(s=>s.Enumerations!=0||s.Reads!=s.Count))throw new Exception("Caller input was read again after bounded snapshot.");guards++;
Empty([new InkStroke(new ShrinkingPoints())]);
var recorded=RecordedInputs.Load("tools/test-data/russian-recorded-words/manifest.json");for(int i=0;i<recorded.Names.Length;i++)samples.Add((recorded.Names[i],recorded.Samples[i]));
if(args.Length==3){byte[] input=File.ReadAllBytes(args[2]);if(Convert.ToHexStringLower(SHA256.HashData(input))!="c8fe150f3ca960cf7216da88c1dd90e1045aed2ef4652a8fa66c9e7a85353dce")throw new Exception("Frozen blind coordinates required.");samples.Clear();using var d=JsonDocument.Parse(input);foreach(var s in d.RootElement.GetProperty("samples").EnumerateArray())samples.Add((s.GetProperty("id").GetString()!,s.GetProperty("strokes").EnumerateArray().Select(st=>new InkStroke(st.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray()));}
using var file=File.Create(args[1]);using var j=new Utf8JsonWriter(file);j.WriteStartObject();j.WriteString("scope","Actual global word admission, native candidate score, not calibrated probability");j.WriteNumber("guards",guards);j.WriteStartArray("samples");
foreach(var(name,ink)in samples){var r=model.RecognizeDiagnostic(ink);j.WriteStartObject();j.WriteString("id",name);j.WriteString("text",r.Text);j.WriteNumber("scoreBits",BitConverter.SingleToInt32Bits(r.Confidence));j.WriteNumber("frames",r.Frames);j.WriteBoolean("experimental",r.ExperimentalCandidateScore);j.WriteEndObject();j.Flush();if(args.Length==2)Console.WriteLine(name+": "+r.Text+" score="+r.Confidence);}
j.WriteEndArray();j.WriteEndObject();Console.WriteLine("PASS actual fullglobal adapter with unsupported/cancel guards; no expected labels read");
sealed class ChangingPoints(IReadOnlyList<InkPoint> initial):IReadOnlyList<InkPoint>
{
 public int Enumerations{get;private set;}public int Reads{get;private set;}public int Count=>initial.Count;
 public InkPoint this[int i]{get{Reads++;return initial[i];}}
 public IEnumerator<InkPoint> GetEnumerator(){int invocation=Enumerations++;foreach(var p in initial)yield return invocation==0?p:new(p.X+200000,p.Y+200000);}
 System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()=>GetEnumerator();
}
sealed class ShrinkingPoints:IReadOnlyList<InkPoint>
{
 public int Count=>1;
 public InkPoint this[int i]=>throw new ArgumentOutOfRangeException(nameof(i));
 public IEnumerator<InkPoint> GetEnumerator()=>throw new InvalidOperationException("Snapshot should use bounded indexed reads.");
 System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()=>GetEnumerator();
}
