using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition.Math;
if(args.Length!=2)throw new ArgumentException("rawMath2.res capture.json");
byte[] resource=File.ReadAllBytes(args[0]);var gate=LegacyMath2ObservableTokens.BindBundledResource(resource);
var rows=new List<Row>();int guards=0;
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException){guards++;return;}throw new InvalidDataException("Required pruning refusal absent.");}
ushort[] symbols=[0,1,2,50,190,261,262,268,276,296,299,65535];
for(int trial=0;trial<4096;trial++)
{
 int count=1+trial%19,n=1+trial%43;
 uint[] units=Enumerable.Range(0,count).Select(i=>(uint)symbols[(i+trial)%symbols.Length]|
     ((uint)(((i+trial)%4) switch{0=>0,1=>1,2=>2,_=>255})<<16)|((uint)((i*7+trial)&255)<<24)).ToArray();
 int[] assignments=Enumerable.Range(0,n).Select(i=>(trial%5) switch{0=>0,1=>(i/3)%count,2=>i%count,_=>(i*7+trial)%count}).ToArray();
 long[] ends=Enumerable.Range(1,n).Where(i=>(trial%4) switch{0=>false,1=>true,2=>i%3==0,_=>i%7==0}).Select(i=>(long)i).ToArray();
 var result=LegacyPreparedMathUnusedUnitPruning.EvaluatePrepared(units,assignments,ends,gate);
 var row=new Row(units.ToArray(),assignments.ToArray(),ends.ToArray(),result.CopyUnits(),result.CopyAssignments(),
     result.CopyUseCounts(),result.CopyOldToNew(),result.CopySuppressedSegments(),result.CopyDeletionMask());
 rows.Add(row);Array.Fill(units,0xffffffff);Array.Fill(assignments,-1);Array.Fill(ends,-1);
 var output=result.CopyUnits();Array.Fill(output,0xffffffff);var masks=result.CopyDeletionMask();Array.Fill(masks,(byte)99);
 var copy=result.CopyAssignments();Array.Fill(copy,-999);var counts=result.CopyUseCounts();Array.Fill(counts,-999);
 var map=result.CopyOldToNew();Array.Fill(map,-999);var suppressed=result.CopySuppressedSegments();Array.Fill(suppressed,-999);
 if(!row.Output.SequenceEqual(result.CopyUnits())||!row.Mapped.SequenceEqual(result.CopyAssignments())||
    !row.Counts.SequenceEqual(result.CopyUseCounts())||!row.Map.SequenceEqual(result.CopyOldToNew())||
    !row.Suppressed.SequenceEqual(result.CopySuppressedSegments())||!row.Deletions.SequenceEqual(result.CopyDeletionMask()))throw new InvalidDataException("Mutable pruning storage.");
}
Reject(()=>LegacyPreparedMathUnusedUnitPruning.EvaluatePrepared([], [0], [],gate));
Reject(()=>LegacyPreparedMathUnusedUnitPruning.EvaluatePrepared(new uint[1001], [0], [],gate));
Reject(()=>LegacyPreparedMathUnusedUnitPruning.EvaluatePrepared([2], [], [],gate));
Reject(()=>LegacyPreparedMathUnusedUnitPruning.EvaluatePrepared([2],new int[6001], [],gate));
Reject(()=>LegacyPreparedMathUnusedUnitPruning.EvaluatePrepared([2],[0],new long[6001],gate));
Reject(()=>LegacyPreparedMathUnusedUnitPruning.EvaluatePrepared([2],[-1],[],gate));
Reject(()=>LegacyPreparedMathUnusedUnitPruning.EvaluatePrepared([2],[1],[],gate));
foreach(long[] ends in new long[][]{[0],[2],[1,1],[2,1]})Reject(()=>LegacyPreparedMathUnusedUnitPruning.EvaluatePrepared([2],[0],ends,gate));
Reject(()=>LegacyPreparedMathUnusedUnitPruning.EvaluatePrepared([2],[0],[],null!));
Reject(()=>LegacyPreparedMathUnusedUnitPruning.EvaluatePrepared([2],[0],[],gate,new CancellationToken(true)));
File.WriteAllText(args[1],JsonSerializer.Serialize(new Capture(Convert.ToHexStringLower(SHA256.HashData(resource)),gate.CopySlots(),rows.ToArray(),guards),CaptureJson.Default.Capture));
Console.WriteLine($"PASS{rows.Count} prepared041660 boundary/filter/deletion controls/{guards} guards; actual gate, no EF4 record mutation.");
record Row(uint[] Input,int[] Assignments,long[] Ends,uint[] Output,int[] Mapped,int[] Counts,int[] Map,int[] Suppressed,byte[] Deletions);
record Capture(string Resource,ushort[] Gate,Row[] Rows,int Guards);
[JsonSerializable(typeof(Capture))]
internal partial class CaptureJson:JsonSerializerContext;
