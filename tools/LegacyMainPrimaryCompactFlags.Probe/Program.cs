using InkRecognition;
using System.Text.Json;
if(args.Length!=2)throw new ArgumentException("prepared-input.json prepared-output.json; no actual/NN mode");
using var input=JsonDocument.Parse(File.ReadAllBytes(args[0]));var reports=new List<object>();
foreach(var x in input.RootElement.EnumerateArray())
{
 var rows=x.GetProperty("Rows").EnumerateArray().Select(r=>new Row(r.GetProperty("Tag").GetUInt64(),r.GetProperty("Context").GetUInt64(),r.GetProperty("Auxiliary").GetInt32(),r.GetProperty("Fallback").GetBoolean(),r.GetProperty("Bit3").GetBoolean(),r.GetProperty("Bit4").GetBoolean())).ToArray();
 var replies=x.GetProperty("Replies").EnumerateArray().Select(v=>v.GetBoolean()).ToArray();int calls=0;
 var result=LegacyPreparedMainPrimaryCompactFlags.Select(rows,rows.Length-1,x.GetProperty("GroupStart").GetInt32(),x.GetProperty("Selected").GetInt32(),x.GetProperty("Quota").GetInt32(),x.GetProperty("Headers").EnumerateArray().Select(v=>v.GetUInt32()).ToArray(),()=>replies[calls++]);
 byte[] beam=new byte[88],storage=Enumerable.Repeat((byte)0xa7,40).ToArray();int[] indices=new int[1];
 var options=new LegacyPreparedCompactWordOptions(rows[^1].Tag,-1,100+(result.TerminalBit4?8:0),7,65535,-1,false,result.TerminalBit3,result.TerminalBit4,255,65535,65535,65535,65535,65535,0);
 LegacyPreparedCompactWordRecords.Append(storage,0,new[]{new LegacyPreparedCompactWordSymbol(1,0,0)},beam,options,static _=>0,indices);
 reports.Add(new{Final40=Convert.ToBase64String(storage),Id=x.GetProperty("Id").GetInt32(),result.TerminalBit3,result.TerminalBit4,result.SelectedDistinctAfter,result.ProbeCalls,ActualProbeCalls=calls});
 if(result.ProbeCalls!=calls)throw new Exception("Uncounted native probe");
}
File.WriteAllText(args[1],JsonSerializer.Serialize(new{Scope="Prepared native207CB0 primaryflag selection only; no actual owner/query/NN",NNCalls=0,Reports=reports},new JsonSerializerOptions{WriteIndented=true}));
sealed record Row(ulong Tag,ulong Context,int Auxiliary,bool FallbackByte3,bool TerminalBit3,bool Forced):ILegacyMainPrimaryCompactFlagRow;
