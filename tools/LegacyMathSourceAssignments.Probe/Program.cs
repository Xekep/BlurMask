using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition.Math;
if(args.Length!=1)throw new ArgumentException("capture.json");
var rows=new List<Row>();int guards=0;
for(int trial=0;trial<4096;trial++)
{
    int n=1+trial%7,s=trial%19;bool source=trial%3!=0,all=trial%5==0;
    int[] map=Enumerable.Range(0,n).Select(i=>i*2+trial%2).ToArray();
    uint[] units=source?Enumerable.Range(0,s).Select(i=>(uint)(100+i)|((i+trial)%3==0?0u:0x01000000u)).ToArray():[];
    int retained=source&&!all?units.Count(v=>(v>>24)==0):0;
    int count=trial%11==0?s:retained;
    int[] initial=Enumerable.Range(0,count).Select(i=>i%4==0?-1:map[i%n]).ToArray();
    float[] probabilities=Enumerable.Range(0,n*s).Select(i=>trial%4==0?.5f:(float)((i*8191+trial*31)%997)/1024).ToArray();
    var result=LegacyPreparedMathSourceAssignments.EvaluatePrepared(units,initial,map,probabilities,s,all);
    rows.Add(new(units.ToArray(),initial.ToArray(),map.ToArray(),probabilities.ToArray(),s,all,result.CopyAssignments(),result.AlreadyComplete));
    Array.Fill(units,uint.MaxValue);Array.Fill(initial,-99);Array.Fill(map,-99);Array.Fill(probabilities,7);
    var copy=result.CopyAssignments();Array.Fill(copy,-99);
    if(!result.CopyAssignments().SequenceEqual(rows[^1].Output))throw new InvalidDataException("Mutable source assignments.");
}
var compositions=new List<Composed>();
void Compose(uint[] source,uint[] initial,int[] map,float[] matrix)
{
    var correction=LegacyPreparedMathExistingUnitCorrection.EvaluatePrepared(source,initial,map,matrix);
    var assignments=LegacyPreparedMathSourceAssignments.EvaluatePrepared(source,
        correction.CopyAssociations().Select(v=>unchecked((int)v)).ToArray(),map,matrix,source.Length,false);
    var inserted=LegacyPreparedMathInsertedUnits.EvaluatePrepared(source,correction.CopyUnits(),assignments.CopyAssignments(),map,matrix,299);
    compositions.Add(new(correction.CopyUnits(),assignments.CopyAssignments(),inserted.CopyUnits(),inserted.CopyAssignments(),inserted.CopyInsertionMask()));
}
Compose([42,42,0x01000063],[0x01000010,0x01000020,0x01000030],[0,2],[.2f,.3f,.9f,.9f,.8f,.1f]);
Compose([42,42,43,43,0x01000063],[0x01000010,0x01000020],[1],[.9f,.8f,.3f,.2f,.1f]);
void Reject(Action action)
{
    try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){guards++;return;}
    throw new InvalidDataException("Missing source assignment guard.");
}
Reject(()=>LegacyPreparedMathSourceAssignments.EvaluatePrepared([],[],[],[],-1,false));
Reject(()=>LegacyPreparedMathSourceAssignments.EvaluatePrepared([],[],[],[],6001,false));
Reject(()=>LegacyPreparedMathSourceAssignments.EvaluatePrepared([1],[],[0],new float[2],2,false));
Reject(()=>LegacyPreparedMathSourceAssignments.EvaluatePrepared([],[],[0,0],new float[2],1,false));
Reject(()=>LegacyPreparedMathSourceAssignments.EvaluatePrepared([],[-2],[0],[.5f],1,false));
Reject(()=>LegacyPreparedMathSourceAssignments.EvaluatePrepared([1,1],[],[0],new float[2],2,false));
Reject(()=>LegacyPreparedMathSourceAssignments.EvaluatePrepared([],[],[],[],1,true));
Reject(()=>LegacyPreparedMathSourceAssignments.EvaluatePrepared([],[],[0],[],1,true));
foreach(float bad in new[]{float.NaN,float.PositiveInfinity,-.1f,1.1f})
    Reject(()=>LegacyPreparedMathSourceAssignments.EvaluatePrepared([],[],[0],[bad],1,true));
Reject(()=>LegacyPreparedMathSourceAssignments.EvaluatePrepared([],[],[],[],0,false,new CancellationToken(true)));
File.WriteAllText(args[0],JsonSerializer.Serialize(new Capture(rows.ToArray(),compositions.ToArray(),guards),CaptureJson.Default.Capture));
Console.WriteLine($"PASS4096 prepared source assignments/2 correction-insertion compositions/{guards} guards; no request owner.");
record Row(uint[] Source,int[] Initial,int[] Map,float[] Probabilities,int Segments,bool All,int[] Output,bool Complete);
record Composed(uint[] Corrected,int[] Assigned,uint[] Inserted,int[] FinalAssignments,byte[] Mask);
record Capture(Row[] Rows,Composed[] Compositions,int Guards);
[JsonSerializable(typeof(Capture))]
internal partial class CaptureJson:JsonSerializerContext;
