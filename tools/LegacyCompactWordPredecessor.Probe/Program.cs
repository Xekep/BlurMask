using System.Buffers.Binary;
using System.Text.Json;
using InkRecognition;

if(args.Length!=1)throw new ArgumentException("controls.json");
using var doc=JsonDocument.Parse(File.ReadAllBytes(args[0]));
int advanced=0,begun=0;
foreach(var group in doc.RootElement.GetProperty("groups").EnumerateArray())
{
    byte[] pool=Convert.FromHexString(group.GetProperty("pool").GetString()!);
    int[] starts=group.GetProperty("starts").EnumerateArray().Select(v=>v.GetInt32()).ToArray();
    var before=pool.ToArray();var startsBefore=starts.ToArray();
    foreach(var test in group.GetProperty("begin").EnumerateArray())
    {
        var result=LegacyPreparedCompactWordPredecessor.Begin(pool,starts,
            test.GetProperty("node").GetInt32(),test.GetProperty("relative").GetInt32(),test.GetProperty("selected").GetUInt16());
        Check(result,test.GetProperty("expected"));begun++;
    }
    foreach(var test in group.GetProperty("advance").EnumerateArray())
    {
        var a=test.GetProperty("input");
        var cursor=new LegacyCompactWordBacktrackCursor(a[0].GetInt32(),a[1].GetInt32(),a[2].GetUInt16(),a[3].GetUInt16());
        var result=LegacyPreparedCompactWordPredecessor.Advance(pool,starts,cursor,test.GetProperty("marker").GetUInt16());
        Check(result,test.GetProperty("expected"));advanced++;
    }
    Require(pool.AsSpan().SequenceEqual(before)&&starts.AsSpan().SequenceEqual(startsBefore),"Prepared inputs changed.");
}
Require(advanced==4352&&begun==256,"Control coverage changed.");

//Compose the existing206D30 writer with the restored21FFD0 cursor. This is a
//prepared compact sequence, not an OCR winner or a substitute207CB0 driver.
byte[] compact=new byte[120],beam=new byte[88];
LegacyPreparedCompactWordSymbol[] symbols=[new(65,0,0),new(66,0,1UL<<24),new(67,0,2UL<<24)];
var options=default(LegacyPreparedCompactWordOptions) with {ParentRecord=-1,TerminalField1e=65535,TerminalField24=-1};
LegacyPreparedCompactWordRecords.Append(compact,0,symbols,beam,options,_=>0,new int[3]);
var at=LegacyPreparedCompactWordPredecessor.Begin(compact,new int[]{0},0,2);
for(int expected=2;expected>=0;expected--)
{
    Require(at.RecordIndex==expected&&at.SourceNode==0&&at.Arc==expected&&at.Alternative==65535,"206D30 chain changed.");
    at=LegacyPreparedCompactWordPredecessor.Advance(compact,new int[]{0},at,65535);
}
Require(at.IsEnd&&at.RecordIndex==-1&&at.Arc==65535,"Root cursor changed.");
int guards=0;
Reject(()=>LegacyPreparedCompactWordPredecessor.Begin([],[],0,0));
Reject(()=>LegacyPreparedCompactWordPredecessor.Begin(new byte[41],new int[]{0},0,0));
Reject(()=>LegacyPreparedCompactWordPredecessor.Begin(compact,new int[]{0},-1,0));
Reject(()=>LegacyPreparedCompactWordPredecessor.Begin(compact,new int[]{0},0,-1));
Reject(()=>LegacyPreparedCompactWordPredecessor.Begin(compact,new int[]{0},0,3));
Reject(()=>LegacyPreparedCompactWordPredecessor.Begin(compact,new int[]{0},0,0,65535,new(true)));
Reject(()=>LegacyPreparedCompactWordPredecessor.Advance(compact,new int[]{0},at,0));
Reject(()=>LegacyPreparedCompactWordPredecessor.Advance(compact,new int[]{0},new(3,0,0,0),0));
Reject(()=>LegacyPreparedCompactWordPredecessor.Advance(compact,new int[]{0},new(0,0,0,0),0,new(true)));
var corrupt=compact.ToArray();BinaryPrimitives.WriteInt32LittleEndian(corrupt.AsSpan(16),-2);
Reject(()=>LegacyPreparedCompactWordPredecessor.Advance(corrupt,new int[]{0},new(0,0,0,0),0));
corrupt=compact.ToArray();BinaryPrimitives.WriteUInt16LittleEndian(corrupt.AsSpan(30),0);
BinaryPrimitives.WriteUInt64LittleEndian(corrupt.AsSpan(8),0xffffff);
Reject(()=>LegacyPreparedCompactWordPredecessor.Advance(corrupt,new int[]{0},new(1,0,0,0),0));
BinaryPrimitives.WriteUInt64LittleEndian(corrupt.AsSpan(8),0);
Reject(()=>LegacyPreparedCompactWordPredecessor.Advance(corrupt,new int[]{0},new(1,0,0,65533),0));
Console.WriteLine($"PASS {advanced} independent predecessor/{begun} initialization controls, all alternate/marker/root/direct paths and signed24/FFFD/FFFE/FFFF boundaries; {guards} guards.");
Console.WriteLine("PASS prepared206D30 three-record chain ->21FFD0 predecessor cursor; no full backtrack/glyph/text result claim.");
void Reject(Action action)
{
    try{action();}catch(Exception e)when(e is ArgumentException or InvalidDataException or OperationCanceledException){guards++;return;}
    throw new InvalidDataException("Expected unsafe prepared index/extent/cancellation refusal.");
}
static void Check(LegacyCompactWordBacktrackCursor value,JsonElement expected)
{
    Require(value.RecordIndex==expected[0].GetInt32()&&value.SourceNode==expected[1].GetInt32()&&
        value.Arc==expected[2].GetUInt16()&&value.Alternative==expected[3].GetUInt16(),"Predecessor cursor mismatch.");
}
static void Require(bool condition,string message){if(!condition)throw new InvalidDataException(message);}
