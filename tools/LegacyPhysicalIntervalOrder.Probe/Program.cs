using InkRecognition;
using System.Text.Json;
if(args.Length!=1)throw new ArgumentException("Supply independent interval controls.");
using var doc=JsonDocument.Parse(File.ReadAllBytes(args[0]));int count=0;
foreach(var item in doc.RootElement.GetProperty("cases").EnumerateArray())
{
    var points=item.GetProperty("boxes").EnumerateArray().Select(b=>new InkStroke(new InkPoint[]{new(b[0].GetSingle(),0),new(b[1].GetSingle(),10)})).ToArray();
    var groups=item.GetProperty("groups").EnumerateArray().Select(g=>new ReadOnlyMemory<int>(g.EnumerateArray().Select(v=>v.GetInt32()).ToArray())).ToArray();
    var actual=LegacyNativeUnknownPhysicalIntervalOrder.Order(points,groups,item.GetProperty("scale").GetDouble(),item.GetProperty("tolerance").GetSingle());
    Require(actual.SequenceEqual(item.GetProperty("expected").EnumerateArray().Select(v=>v.GetInt32())),"Independent native interval insertion.");count++;
}
InkStroke stroke=new(new InkPoint[]{new(0,0),new(10,10)});
Throws<ArgumentException>(()=>LegacyNativeUnknownPhysicalIntervalOrder.Order([stroke],[],1,.25f));
Throws<ArgumentException>(()=>LegacyNativeUnknownPhysicalIntervalOrder.Order([stroke],[new int[]{0,0}],1,.25f));
Throws<ArgumentException>(()=>LegacyNativeUnknownPhysicalIntervalOrder.Order([stroke],[new int[]{1}],1,.25f));
Throws<ArgumentException>(()=>LegacyNativeUnknownPhysicalIntervalOrder.Order([stroke],[new int[]{0}],double.NaN,.25f));
Throws<OperationCanceledException>(()=>LegacyNativeUnknownPhysicalIntervalOrder.Order([stroke],[new int[]{0}],1,.25f,new(true)));
Console.WriteLine($"PASS {count} independent grouped15F050 interval controls; actual provider/input/controller lifecycle still external.");
static void Require(bool v,string m){if(!v)throw new InvalidDataException(m);}
static void Throws<T>(Action a)where T:Exception{try{a();}catch(T){return;}throw new InvalidDataException($"Expected {typeof(T).Name}.");}
