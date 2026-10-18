using System.Text.Json;
using System.Text.Json.Serialization;
using InkRecognition.Math;
if(args.Length!=1)throw new ArgumentException("capture.json");
var rows=new List<Row>();int guards=0;
for(int trial=0;trial<2048;trial++)
{
    int n=trial%17,width=(trial/17)%9;
    uint[] units=Enumerable.Range(0,n).Select(i=>0xabcdef00u|(uint)i).ToArray();
    byte[] mask=Enumerable.Range(0,n).Select(i=>(byte)(((trial+i)%4) switch{0=>1,1=>255,_=>0})).ToArray();
    var initial=Enumerable.Range(0,mask.Count(v=>v==0)*width).Select(i=>new LegacyPreparedMathAuxiliaryRow(
        (ushort)(i*7919+trial),i%7==0?null:(ushort)(i*8191+trial),unchecked((uint)(i*16777213+trial*31)))).ToArray();
    var result=LegacyPreparedMathAuxiliaryRows.EvaluatePrepared(units,mask,width,initial);
    bool complete=result.TryCopyNativeImage(out byte[] image);
    rows.Add(new(units.ToArray(),mask.ToArray(),width,initial.ToArray(),result.CopyRows(),complete,image));
    Array.Fill(units,uint.MaxValue);Array.Fill(mask,(byte)0);Array.Fill(initial,default);
    var copy=result.CopyRows();Array.Fill(copy,default);
    if(!result.CopyRows().SequenceEqual(rows[^1].Output))throw new InvalidDataException("Mutable auxiliary rows.");
}
void Reject(Action action)
{
    try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException){guards++;return;}
    throw new InvalidDataException("Missing auxiliary guard.");
}
Reject(()=>LegacyPreparedMathAuxiliaryRows.EvaluatePrepared([1],[],1,[]));
Reject(()=>LegacyPreparedMathAuxiliaryRows.EvaluatePrepared([],[],-1,[]));
Reject(()=>LegacyPreparedMathAuxiliaryRows.EvaluatePrepared([],[],129,[]));
Reject(()=>LegacyPreparedMathAuxiliaryRows.EvaluatePrepared(new uint[1001],new byte[1001],0,[]));
Reject(()=>LegacyPreparedMathAuxiliaryRows.EvaluatePrepared([1],[0],1,[]));
Reject(()=>LegacyPreparedMathAuxiliaryRows.EvaluatePrepared([1],[1],1,[new(1,0,0)]));
Reject(()=>LegacyPreparedMathAuxiliaryRows.EvaluatePrepared([],[],0,[],new CancellationToken(true)));
File.WriteAllText(args[0],JsonSerializer.Serialize(new Capture(rows.ToArray(),guards),CaptureJson.Default.Capture));
Console.WriteLine($"PASS{rows.Count} auxiliary known-field controls/{guards} guards; unspecified padding never fabricated.");
record Row(uint[] Units,byte[] Mask,int Width,LegacyPreparedMathAuxiliaryRow[] Initial,LegacyPreparedMathAuxiliaryRow[] Output,bool CompleteImage,byte[] Image);
record Capture(Row[] Rows,int Guards);
[JsonSerializable(typeof(Capture))]
internal partial class CaptureJson:JsonSerializerContext;
