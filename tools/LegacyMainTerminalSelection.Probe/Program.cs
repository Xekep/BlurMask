using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length!=1)throw new ArgumentException("independent-controls.json");
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[0]));int count=0,rowsChecked=0,winners=0;
foreach(var sample in controls.RootElement.EnumerateArray())
{
    var contexts=new LegacySymbolContextFactory();
    var sources=sample.GetProperty("sources").EnumerateArray().Select(s=>new LegacyPreparedTerminalSource(s[0].GetUInt32(),s[1].GetInt32())).ToArray();
    var rows=sample.GetProperty("rows").EnumerateArray().Select(s=>Convert.FromHexString(s.GetString()!)).ToArray();
    var nodes=sample.GetProperty("contexts").EnumerateArray().ToArray();
    for(int i=0;i<rows.Length;i++)if(nodes[i].ValueKind!=JsonValueKind.Null)
        BinaryPrimitives.WriteUInt64LittleEndian(rows[i].AsSpan(8),contexts.ResolveInitial(nodes[i].GetUInt16()).Identity);
    byte[] storage=rows.SelectMany(s=>s).ToArray();
    var options=new LegacyPreparedMainTerminalOptions(sample.GetProperty("primary").GetUInt16(),sample.GetProperty("sentinel").GetBoolean(),
        sample.GetProperty("filter4").GetBoolean(),sample.GetProperty("alternate").GetBoolean(),sample.GetProperty("require").GetBoolean(),sample.GetProperty("resultFlag").GetByte());
    var got=LegacyNativeMainTerminalSelection.SelectSingleAlternativeWithoutRescue(storage,(ushort)rows.Length,sources,options,sample.GetProperty("cumulative").GetInt32(),contexts);
    var expected=sample.GetProperty("winner");
    if(expected.ValueKind==JsonValueKind.Null)
    {if(got.HasWinner||got.Ordinal!=-1||got.RelativeCost!=0x4000000||got.TotalCost!=0x4000000||got.RowCost14!=0x4000000||got.RowField22!=uint.MaxValue||got.FallbackFlag!=0||got.ResultFlag8!=options.InputResultFlag8)throw new Exception("No-winner state differs.");}
    else
    {
        long[] actual=[got.Ordinal,got.RelativeCost,got.TotalCost,got.RowCost14,got.RowField22,got.FallbackFlag,got.ResultFlag8];
        if(!got.HasWinner||!actual.SequenceEqual(expected.EnumerateArray().Select(v=>v.GetInt64())))throw new Exception($"Independent terminal selection differs at{count}.");winners++;
    }
    count++;rowsChecked+=rows.Length;
}
//Stable tie, real SOURCE extent rather than a caller-truncated retained extent,
//and native post-selection cumulative wrap are separate original controls.
var factory=new LegacySymbolContextFactory();byte[] pair=new byte[176];
for(int i=0;i<2;i++){BinaryPrimitives.WriteInt32LittleEndian(pair.AsSpan(i*88+16),7);BinaryPrimitives.WriteUInt16LittleEndian(pair.AsSpan(i*88+34),(ushort)(10+i));}
var tie=LegacyNativeMainTerminalSelection.SelectSingleAlternativeWithoutRescue(pair,2,[new(0,0)],new(0,false,false,false,false,9),int.MaxValue,factory);
if(tie.Ordinal!=0||tie.RowField22!=10||tie.TotalCost!=unchecked(int.MaxValue+7)||tie.ResultFlag8!=9)throw new Exception("First tie or cumulative wrap changed.");
BinaryPrimitives.WriteInt32LittleEndian(pair.AsSpan(104),-1);
if(LegacyNativeMainTerminalSelection.SelectSingleAlternativeWithoutRescue(pair,2,[new(0,0)],default,0,factory).Ordinal!=1)throw new Exception("Source-end row was silently removed.");
var parent=factory.ResolveInitial(65);var nested=factory.ResolveChild(parent.Identity,0xfff9);
if(nested.Created)factory.LinkCreatedContext(nested.Identity,parent.Identity);
BinaryPrimitives.WriteUInt64LittleEndian(pair.AsSpan(8),nested.Identity);
var nestedWinner=LegacyNativeMainTerminalSelection.SelectSingleAlternativeWithoutRescue(pair.AsSpan(0,88),1,[new(0,0)],new(1,true,false,false,false,9),0,factory);
if(!nestedWinner.HasWinner||nestedWinner.Ordinal!=0||nestedWinner.RelativeCost!=7)throw new Exception("Single-alternative sentinel symbol was incorrectly restricted to root contexts.");
BinaryPrimitives.WriteUInt16LittleEndian(pair.AsSpan(34),ushort.MaxValue);
BinaryPrimitives.WriteUInt64LittleEndian(pair.AsSpan(8),ulong.MaxValue);
var rawFFFF=LegacyNativeMainTerminalSelection.SelectSingleAlternativeWithoutRescue(pair.AsSpan(0,88),1,[new(0,0)],new(0,false,false,false,false,0xa5),0,factory);
if(!rawFFFF.HasWinner||rawFFFF.RowField22!=65535||rawFFFF.ResultFlag8!=0xa5)throw new Exception("Successful FFFF field22 confused with no-winner UINTMAX, or unused context was read.");
pair[4]=255;BinaryPrimitives.WriteInt32LittleEndian(pair.AsSpan(16),0x4000000);
if(LegacyNativeMainTerminalSelection.SelectSingleAlternativeWithoutRescue(pair.AsSpan(0,88),1,[new(0,0)],default,0,factory).HasWinner)throw new Exception("Impossible row was not skipped before source/context access.");
try{LegacyNativeMainTerminalSelection.SelectSingleAlternativeWithoutRescue(pair,2,[new(0,0)],default,0,factory,new(true));throw new Exception("Cancellation lost.");}catch(OperationCanceledException){}
try{LegacyNativeMainTerminalSelection.SelectSingleAlternativeWithoutRescue(pair,3,[new(0,0)],default,0,factory);throw new Exception("Invalid extent accepted.");}catch(ArgumentException){}
Console.WriteLine($"PASS {count} independent terminal scans/{rowsChecked} full88 rows/{winners} winners; tie/source-end/nested-sentinel/cumulative-wrap/cancellation controls. Single alternative, rescue disabled; no SDK option selection.");
