using InkRecognition;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
if(args.Length!=4)throw new ArgumentException("case-data normalization-data independent-controls official-NormalizationTest9");
using var c=File.OpenRead(args[0]);var caseData=LegacyNativeCaseData.Load(c);using var n=File.OpenRead(args[1]);var normalization=LegacyNativeUnicodeNormalizationData.Load(n);
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[2]));int cases=0;
foreach(var sample in controls.RootElement.EnumerateArray())
{
 int[] input=Values(sample.GetProperty("input"));uint flags=sample.GetProperty("flags").GetUInt32();int language=sample.GetProperty("language").GetInt32();
 var got=LegacyNativeMutableCaseStreams.Prepare(input,caseData,normalization,flags,language);
 if(!got.CopyLower().SequenceEqual(Values(sample.GetProperty("lower")))||!got.CopyUpper().SequenceEqual(Values(sample.GetProperty("upper")))||!got.CopyStrippedUpper().SequenceEqual(Values(sample.GetProperty("stripped"))))throw new Exception($"Independent case stream mismatch at{cases}.");
 cases++;
}
byte[] corpus=File.ReadAllBytes(args[3]);if(Convert.ToHexString(SHA256.HashData(corpus)).ToLowerInvariant()!="2d48d848656b3cf889df59980ab13551988950c4ca8c5190a17c691a17f8b9bb")throw new Exception("Pinned Unicode9 corpus required.");
int projections=0,differences=0;
foreach(string raw in Encoding.UTF8.GetString(corpus).Split('\n'))
{
 string line=raw.Split('#')[0].Trim();if(line.Length==0||line[0]=='@')continue;
 int[][] fields=line.Split(';').Take(5).Select(field=>field.Trim().Split(' ',StringSplitOptions.RemoveEmptyEntries).Select(v=>Convert.ToInt32(v,16)).ToArray()).ToArray();
 for(int i=0;i<5;i++)
 {
  int[] got=LegacyNativeCanonicalDecomposition.PrepareDecodedCodePoints(fields[i],normalization);int[] wanted=fields[i<3?2:4];
  if(!got.SequenceEqual(wanted)){if(fields[i].Length!=1||fields[i][0] is not(0x37e or 0x387)||!got.SequenceEqual(fields[i]))throw new Exception("Official NFD mismatch.");differences++;}
  projections++;
 }
}
if(projections!=93610||differences!=2)throw new Exception("Official NFD cardinality changed.");
int[] source=[0x451];var own=LegacyNativeMutableCaseStreams.Prepare(source,caseData,normalization,0,0);source[0]=0;own.CopyUpper().AsSpan().Clear();if(!own.CopyUpper().SequenceEqual(new[]{0x401}))throw new Exception("Owned streams aliased.");
var accents=LegacyNativeMutableCaseStreams.Prepare([0x61,0x5b0,0x301],caseData,normalization,0,0);if(!accents.CopyStrippedUpper().SequenceEqual(new[]{0x41,0x5b0}))throw new Exception("Native filter must retain HebrewCCC10 and remove acute230.");
var title=LegacyNativeMutableCaseStreams.Prepare([0x1c6],caseData,normalization,0,0);if(!title.CopyUpper().SequenceEqual(new[]{0x1c4}))throw new Exception("Upper stream confused with titlecase.");
var gap=LegacyNativeCanonicalDecomposition.PrepareDecodedCodePoints([0x61,0x590,0x301],normalization);if(!gap.SequenceEqual(new[]{0x590,0x61,0x301}))throw new Exception("Native missingCCC -1 ordering changed.");
if(!LegacyNativeCanonicalDecomposition.PrepareDecodedCodePoints([0xd800,0xfffe,0xffff],normalization).SequenceEqual(new[]{0x1a,0x1a,0x1a}))throw new Exception("Defaultwriter substitution changed.");
Reject(()=>LegacyNativeMutableCaseStreams.Prepare([],caseData,normalization,0,0));Reject(()=>LegacyNativeMutableCaseStreams.Prepare([0xd800],caseData,normalization,0,0));Reject(()=>LegacyNativeCanonicalDecomposition.PrepareDecodedCodePoints([-1],normalization));Reject(()=>LegacyNativeCanonicalDecomposition.PrepareDecodedCodePoints(new int[65537],normalization));Reject(()=>LegacyNativeCanonicalDecomposition.PrepareDecodedCodePoints(Enumerable.Repeat(0x301,2000).ToArray(),normalization));
try{LegacyNativeMutableCaseStreams.Prepare([0x61],caseData,normalization,0,0,new(true));throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
Console.WriteLine($"PASS {cases} independent lower/upper/strippedUpper streams; {projections} officialUnicode9 NFD projections/exact{differences} native Greek deviations; CCC10retain/230remove/title-vs-upper/gap/substitution/ownership/budget/cancel. Prelookup scalar stage, not mutable AL factory.");
static int[] Values(JsonElement value)=>value.EnumerateArray().Select(v=>v.GetInt32()).ToArray();
static void Reject(Action action){try{action();}catch(ArgumentException){return;}catch(InvalidDataException){return;}throw new Exception("Invalid/unbounded input accepted.");}
