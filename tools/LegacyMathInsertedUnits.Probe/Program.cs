using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition.Math;
if (args.Length != 1) throw new ArgumentException("capture.json");
var rows = new List<Row>(); int guards = 0;
void Check(uint[] existing, uint[] initial, int[] assignments, int[] map, float[] matrix, ushort replacement)
{
    var result = LegacyPreparedMathInsertedUnits.EvaluatePrepared(existing, initial, assignments, map, matrix, replacement);
    rows.Add(new(existing.ToArray(),initial.ToArray(),assignments.ToArray(),map.ToArray(),matrix.ToArray(),replacement,
        result.CopyUnits(),result.CopyAssignments(),result.CopyInsertionMask()));
    Array.Fill(existing,uint.MaxValue);Array.Fill(initial,uint.MaxValue);Array.Fill(assignments,-1);Array.Fill(map,-1);Array.Fill(matrix,7);
    var copy=result.CopyUnits();Array.Fill(copy,uint.MaxValue);
    if(!result.CopyUnits().SequenceEqual(rows[^1].Output))throw new InvalidDataException("Mutable inserted units.");
}
for(int trial=0;trial<4096;trial++)
{
    int n=trial%9,s=trial%17;bool source=trial%2!=0;
    uint[] initial=Enumerable.Range(0,n).Select(i=>0x01000000u|(uint)(100+i)).ToArray();
    int[] map=Enumerable.Range(0,n).Where(i=>(i+trial)%3!=0).ToArray();
    uint[] existing=source?Enumerable.Range(0,s).Select(i=>0x01000100u|(uint)i).ToArray():[];
    int[] assignment=Enumerable.Range(0,s).Select(i=>n==0||(i+trial)%3==0?-1:i%n).ToArray();
    if(source)for(int i=0;i+1<s;i+=3)
    {
        existing[i]=(uint)(0x10000*((trial+i)%256)+500+i);existing[i+1]=existing[i];
        if((i+trial)%2==0||n==0){assignment[i]=-1;assignment[i+1]=-1;}
        else{assignment[i]=i%n;assignment[i+1]=(i+1)%n;}
    }
    float[] matrix=Enumerable.Range(0,map.Length*s).Select(i=>trial%5==0?.5f:(float)((i*8191+trial*31)%997)/1024).ToArray();
    Check(existing,initial,assignment,map,matrix,(ushort)(trial%3==0?299:trial%3==1?65535:0));
}
void Reject(Action action)
{
    try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){guards++;return;}
    throw new InvalidDataException("Missing insertion guard.");
}
Reject(()=>LegacyPreparedMathInsertedUnits.EvaluatePrepared([],[42],[-2],[],[],299));
Reject(()=>LegacyPreparedMathInsertedUnits.EvaluatePrepared([],[42],[1],[],[],299));
Reject(()=>LegacyPreparedMathInsertedUnits.EvaluatePrepared([42],[43],[-1],[],[],299));
Reject(()=>LegacyPreparedMathInsertedUnits.EvaluatePrepared([42,42],[43],[-1,0],[],[],299));
Reject(()=>LegacyPreparedMathInsertedUnits.EvaluatePrepared([42],[43],[-1,-1],[],[],299));
Reject(()=>LegacyPreparedMathInsertedUnits.EvaluatePrepared([],[43],[-1],[1],[.5f],299));
Reject(()=>LegacyPreparedMathInsertedUnits.EvaluatePrepared([],[43,44],[-1],[0,0],[.5f,.5f],299));
Reject(()=>LegacyPreparedMathInsertedUnits.EvaluatePrepared([],[43],[-1],[0],[],299));
foreach(float bad in new[]{float.NaN,float.PositiveInfinity,-.01f,1.01f})
    Reject(()=>LegacyPreparedMathInsertedUnits.EvaluatePrepared([],[43],[-1],[0],[bad],299));
Reject(()=>LegacyPreparedMathInsertedUnits.EvaluatePrepared([],new uint[1000],[-1],[],[],299));
Reject(()=>LegacyPreparedMathInsertedUnits.EvaluatePrepared([],[43],[],[],[],299,new CancellationToken(true)));
File.WriteAllText(args[0],JsonSerializer.Serialize(new Capture(rows.ToArray(),guards),CaptureJson.Default.Capture));
Console.WriteLine($"PASS{rows.Count} prepared insertion/mask controls/{guards} guards; no request/record owner.");
record Row(uint[] Existing,uint[] Initial,int[] Assignments,int[] Map,float[] Matrix,ushort Replacement,uint[] Output,int[] Mapped,byte[] Mask);
record Capture(Row[] Rows,int Guards);
[JsonSerializable(typeof(Capture))]
internal partial class CaptureJson:JsonSerializerContext;
