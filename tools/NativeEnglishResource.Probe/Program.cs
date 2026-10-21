using InkRecognition;
using System.Text.Json;
using System.Security.Cryptography;
if(args.Length is not(3 or 4))throw new ArgumentException("MODELS REFERENCE-PACK CAPTURE [FIXED-INPUTS]");
byte[] resource=File.ReadAllBytes(Path.Combine(args[0],"en-US/en_US-ak-cur.res"));
byte[] packed=NativeEnglishTextResource.CreatePortableSnapshot(resource);byte[] expected=File.ReadAllBytes(args[1]);
if(!packed.AsSpan().SequenceEqual(expected)||Convert.ToHexStringLower(SHA256.HashData(packed))!="39418d35708502db6520fa22781614c30c7256efdb9981bf6bbf9b1b0c4f8d5b")throw new Exception("Native resource differs from independent Python full payload/metadata reference.");
var model=NativeEnglishTextResource.Load(resource);using var referenceStream=new MemoryStream(expected);var reference=StrokeTextRecognizer.Load(referenceStream);int guards=1;
void Reject(Action f){try{f();}catch(Exception e)when(e is InvalidDataException or OperationCanceledException){guards++;return;}throw new Exception("Malformed/canceled native resource admitted.");}
Reject(()=>NativeEnglishTextResource.Load([]));Reject(()=>NativeEnglishTextResource.Load(resource.AsSpan(0,resource.Length-1)));
foreach(int at in new[]{0,14,99,resource.Length/2,resource.Length-1}){byte[] changed=resource.ToArray();changed[at]^=1;Reject(()=>NativeEnglishTextResource.Load(changed));}
using(var c=new CancellationTokenSource()){c.Cancel();Reject(()=>NativeEnglishTextResource.Load(resource,c.Token));}
Array.Fill(resource,(byte)0);Reject(()=>NativeEnglishTextResource.Load(resource));
InkStroke Stroke(params(float X,float Y)[] points)=>new(points.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
var samples=new List<(string Id,InkStroke[] Ink)>{("M",[Stroke((0,90),(0,10),(25,60),(50,10),(50,90))]),("curve",[Stroke((0,50),(10,10),(30,0),(50,10),(60,50),(50,90),(30,100),(10,90),(0,50))])};
var baseline=model.Recognize(samples[0].Ink);Parallel.For(0,4,_=>{if(model.Recognize(samples[0].Ink)!=baseline)throw new Exception("Shared immutable model changed concurrent inference.");});guards++;
using(var c=new CancellationTokenSource()){c.Cancel();Reject(()=>model.Recognize(samples[0].Ink,c.Token));}
if(args.Length==4){byte[] inputs=File.ReadAllBytes(args[3]);if(Convert.ToHexStringLower(SHA256.HashData(inputs))!="bcd10f1a92a3abf6d78c5eb18787abf33ad291ab07660daae7468cf465dab285")throw new Exception("Fixed PluColl inputs required.");samples.Clear();using var d=JsonDocument.Parse(inputs);foreach(var s in d.RootElement.GetProperty("samples").EnumerateArray())samples.Add((s.GetProperty("id").GetString()!,s.GetProperty("strokes").EnumerateArray().Select(st=>new InkStroke(st.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray()));}
using var output=File.Create(args[2]);using var json=new Utf8JsonWriter(output);json.WriteStartObject();json.WriteString("scope","Pinned English resource loaded in C#; existing CTC pipeline unchanged, no MAIN/bilingual claim");json.WriteNumber("guards",guards);json.WriteString("portableSha256",Convert.ToHexStringLower(SHA256.HashData(packed)));json.WriteStartArray("samples");
foreach(var(id,ink)in samples){var result=model.Recognize(ink);var original=reference.Recognize(ink);if(result!=original)throw new Exception("Native resource changed inference: "+id);json.WriteStartObject();json.WriteString("id",id);json.WriteString("text",result.Text);json.WriteNumber("scoreBits",BitConverter.SingleToInt32Bits(result.Confidence));json.WriteNumber("frames",result.Frames);json.WriteEndObject();json.Flush();}
json.WriteEndArray();json.WriteEndObject();Console.WriteLine($"PASS raw English resource, whole67-tensor reference parity, {guards} guards, {samples.Count} identical inference rows; no labels read");
