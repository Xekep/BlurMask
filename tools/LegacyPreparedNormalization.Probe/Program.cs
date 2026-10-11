using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;
var prepared=LegacyCursivePreparedInk.Create(new InkStroke[]{new(new InkPoint[]{new(10,10),new(30,50)})}).Insert(0,1,new(10.25f,49.75f),2);
LegacyInkRange[] ranges=[new(0,0,3)];
var normalized=LegacyPreparedGlyphNormalization.NormalizeOrderedRanges(prepared,ranges,10,50,40,100,10);
Require(normalized.Baseline==new LegacyInkBaseline(0,100)&&normalized.Strokes.Single().Points.SequenceEqual(new LegacyInkPoint[]{new(0,-100),new(0,0),new(50,0)}),"Scaled fractional coordinates must truncate toward zero, not round raw ink.");
Require(ReferenceEquals(normalized.PreparedInk,prepared)&&normalized.Ranges.Span.SequenceEqual(ranges)&&prepared.Points.Span[1].X==10.25f,"Prepared provenance or fractional point changed.");
var scaled=LegacyCursivePreparedInk.Create(new InkStroke[]{new(new InkPoint[]{new(-80,330),new(-79.5f,409.5f),new(-40,410)})});
var translated=LegacyPreparedGlyphNormalization.NormalizeOrderedRanges(scaled,ranges,-80,410,80,100,10);
Require(translated.Strokes[0].Points.SequenceEqual(normalized.Strokes[0].Points),"Consistent scale/translation changed normalized glyph.");
var cap=LegacyPreparedGlyphNormalization.NormalizeOrderedRanges(prepared,ranges,-1e5f,50,1,100,10);
Require(cap.Strokes[0].Points.ToArray().All(p=>p.X==short.MaxValue),"Native upper saturation differs.");
Reject(()=>LegacyPreparedGlyphNormalization.NormalizeOrderedRanges(prepared,ranges,10,50,0,100,10));
Reject(()=>LegacyPreparedGlyphNormalization.NormalizeOrderedRanges(prepared,ranges,10,50,40,0,10));
Reject(()=>LegacyPreparedGlyphNormalization.NormalizeOrderedRanges(prepared,ranges,10,1e5f,1,100,10));
Reject(()=>LegacyPreparedGlyphNormalization.NormalizeOrderedRanges(prepared,new LegacyInkRange[]{new(0,0,4)},10,50,40,100,10));
Reject(()=>LegacyPreparedGlyphNormalization.NormalizeOrderedRanges(prepared,new LegacyInkRange[]{new(0,0,1),new(0,1,3)},10,50,40,100,1));
try{LegacyPreparedGlyphNormalization.NormalizeOrderedRanges(prepared,ranges,10,50,40,100,10,new CancellationToken(true));throw new Exception("Cancel ignored.");}catch(OperationCanceledException){}
Console.WriteLine("PASS FF/FF prepared normalization: scale before truncation, negative fractional truncation, baseline0/height100, source provenance, scale/translation, upper cap, fail-closed lower range/merge bound and cancellation.");
if(args.Length==0)return;
if(args.Length==2&&args[0]=="--resource")
{
 var resource=File.ReadAllBytes(args[1]);
 Require(resource[189750]==255&&resource[189751]==255&&BinaryPrimitives.ReadUInt16LittleEndian(resource.AsSpan(189752))==100,"Actual Russian source branch FF/FF,target100 differs.");
 Console.WriteLine("PASS actual resource source branch flagsFF/FF,stored target100. Explicit source frame and range merge threshold remain caller supplied.");return;
}
if(args.Length!=2||args[0]!="--controls")throw new ArgumentException("Usage: LegacyPreparedNormalization.Probe [--controls own.json | --resource acoustic.res]");
using var document=JsonDocument.Parse(File.ReadAllBytes(args[1]));int count=0;
foreach(var c in document.RootElement.GetProperty("cases").EnumerateArray())
{
 var ink=c.GetProperty("strokes").EnumerateArray().Select(s=>new InkStroke(s.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray())).ToArray();
 var source=LegacyCursivePreparedInk.Create(ink);
 var selected=c.GetProperty("ranges").EnumerateArray().Select(r=>new LegacyInkRange(r[0].GetInt32(),r[1].GetInt32(),r[2].GetInt32())).ToArray();
 var frame=c.GetProperty("frame");var result=LegacyPreparedGlyphNormalization.NormalizeOrderedRanges(source,selected,frame[0].GetSingle(),frame[1].GetSingle(),frame[2].GetSingle(),frame[3].GetInt16(),10);
 var expected=c.GetProperty("expected").EnumerateArray().ToArray();Require(result.Strokes.Count==expected.Length,"Independent range count differs.");
 for(int i=0;i<expected.Length;i++){var points=expected[i].EnumerateArray().Select(p=>new LegacyInkPoint(p[0].GetInt16(),p[1].GetInt16())).ToArray();Require(result.Strokes[i].Points.SequenceEqual(points),$"Independent normalization case{count} differs.");}
 count++;
}
Require(count>0,"Empty references.");Console.WriteLine($"PASS {count} independent float32?signed-short prepared range controls.");
static void Require(bool ok,string message){if(!ok)throw new InvalidDataException(message);}
static void Reject(Action action){try{action();throw new Exception("Bad normalization accepted.");}catch(ArgumentException){}}
