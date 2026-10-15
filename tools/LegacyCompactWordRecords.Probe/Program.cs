using System.Text.Json;
using InkRecognition;

if (args.Length != 1) throw new ArgumentException("controls.json required");
using var document = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int controls=0, records=0;
foreach (var test in document.RootElement.EnumerateArray())
{
    byte[] storage=Convert.FromHexString(test.GetProperty("before").GetString()!);
    byte[] beam=Convert.FromHexString(test.GetProperty("beam").GetString()!);
    var symbols=test.GetProperty("symbols").EnumerateArray().Select(s=>new LegacyPreparedCompactWordSymbol(
        s[0].GetUInt16(),s[1].GetByte(),s[2].GetUInt64())).ToArray();
    var o=test.GetProperty("options");
    var options=new LegacyPreparedCompactWordOptions(o[0].GetUInt64(),o[1].GetInt32(),o[2].GetInt32(),
        o[3].GetInt32(),o[4].GetUInt16(),o[5].GetInt32(),o[6].GetBoolean(),o[7].GetBoolean(),
        o[8].GetBoolean(),o[9].GetByte(),o[10].GetUInt16(),o[11].GetUInt16(),o[12].GetUInt16(),
        o[13].GetUInt16(),o[14].GetUInt16(),o[15].GetUInt32());
    int calls=0; byte Category(ushort symbol){Require(symbol==symbols[^1].Symbol,"Wrong category symbol");calls++;return test.GetProperty("category").GetByte();}
    int start=test.GetProperty("start").GetInt32();
    int[] indices=Enumerable.Repeat(-97,symbols.Length+2).ToArray();
    var result=LegacyPreparedCompactWordRecords.Append(storage,start,symbols,beam,options,Category,indices);
    Require(storage.AsSpan().SequenceEqual(Convert.FromHexString(test.GetProperty("after").GetString()!)),"Whole-storage mismatch");
    Require(calls==test.GetProperty("calls").GetInt32()&&result.TerminalBit6==test.GetProperty("eligible").GetBoolean(),"Category gate mismatch");
    Require(result.RecordCount==start+symbols.Length&&result.TerminalRecord==result.RecordCount-1&&
        indices.Take(symbols.Length).SequenceEqual(Enumerable.Range(start,symbols.Length))&&
        indices[^1]==-97&&indices[^2]==-97,"Published indices mismatch");
    controls++;records+=symbols.Length;
}
byte[] pool=new byte[120],source=new byte[88];
LegacyPreparedCompactWordSymbol[] one=[new(65,255,ulong.MaxValue)];
int guards=0;
Reject(()=>LegacyPreparedCompactWordRecords.Append(pool,0,one,source,default,_=>0,new int[1],new(true)));guards++;
Reject(()=>LegacyPreparedCompactWordRecords.Append(pool,0,one,source,default,null!,new int[1]));guards++;
Reject(()=>LegacyPreparedCompactWordRecords.Append(pool,0,Array.Empty<LegacyPreparedCompactWordSymbol>(),source,default,_=>0,new int[1]));guards++;
Reject(()=>LegacyPreparedCompactWordRecords.Append(new byte[41],0,one,source,default,_=>0,new int[1]));guards++;
Reject(()=>LegacyPreparedCompactWordRecords.Append(pool,3,one,source,default,_=>0,new int[1]));guards++;
Reject(()=>LegacyPreparedCompactWordRecords.Append(pool,0,one,new byte[87],default,_=>0,new int[1]));guards++;
Reject(()=>LegacyPreparedCompactWordRecords.Append(pool,0,one,source,default,_=>0,Array.Empty<int>()));guards++;
Reject(()=>LegacyPreparedCompactWordRecords.Append(pool,0,one,pool.AsSpan(0,88),default,_=>0,new int[1]));guards++;
Console.WriteLine($"PASS {controls} independent whole-storage cases/{records} compact40 rows; lazy category gates, untouched residue/indices and {guards} guards. Prepared206D30, detailed costs disabled.");
static void Require(bool condition,string message){if(!condition)throw new InvalidDataException(message);}
static void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or OperationCanceledException){return;}throw new InvalidDataException("Expected refusal");}
