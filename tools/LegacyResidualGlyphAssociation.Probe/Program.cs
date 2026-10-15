using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length!=1)throw new ArgumentException("capture.json");
var reports=new List<Case>();int guards=0;
for(int test=0;test<768;test++)
{
    int rowCount=test%7,referenceCount=test/7%7,mergeFailure=test%11==0?1:-1;
    bool bypass=test%29==0,ordered=test%3==0;
    var facts=Enumerable.Range(0,24).Select(n=>new Fact((byte)(test%17==0&&n==13?2:test%5==0?n%2:0),
        test%4==0?-1:n%3,(n*17+test*3)%31-15,(n*11+test)%23-12,
        (n*17+test*3)%31-15+(n%4),(n*11+test)%23-12+(n%5),n%2==0)).ToArray();
    byte[] rows=new byte[rowCount*72];
    for(int j=0;j<rows.Length;j++)rows[j]=(byte)(j*73+test);
    for(int i=0;i<rowCount;i++)
    {
        int p=i*72,node=(i*3+test)%12;
        BinaryPrimitives.WriteInt32LittleEndian(rows.AsSpan(p),node);
        BinaryPrimitives.WriteUInt16LittleEndian(rows.AsSpan(p+4),(ushort)(i%3));
        BinaryPrimitives.WriteUInt64LittleEndian(rows.AsSpan(p+48),Pack(node,(ushort)(i%3)));
        BinaryPrimitives.WriteUInt16LittleEndian(rows.AsSpan(p+58),(ushort)((i+test)%10));
        rows[p+60]=(byte)(i%3==2?1:0);BinaryPrimitives.WriteInt32LittleEndian(rows.AsSpan(p+64),0);
    }
    ulong[] refs=Enumerable.Range(0,referenceCount).Select(i=>Pack(12+(i*3+test)%12,(ushort)(i%3))).ToArray();
    var trace=new List<ulong[]>();int reads=0;
    Fact Get(LegacyNativeResidualSource s){reads++;return facts[s.Node];}
    var ops=new LegacyNativeResidualAssociationOperations(s=>Get(s).Kind,s=>Get(s).Line,
        s=>{var f=Get(s);return new(f.X0,f.Y0,f.X1,f.Y1);},
        line=>new(2f+line,3f+line),label=>label%2==0,s=>Get(s).Top,
        (sources,token)=>{token.ThrowIfCancellationRequested();trace.Add(sources.ToArray());return new(trace.Count!=mergeFailure,0x5a000000UL+(ulong)trace.Count);});
    var result=LegacyNativeResidualGlyphAssociation.Apply(rows,refs,bypass,ordered,9,ops);
    reports.Add(new(test,Convert.ToHexString(rows),refs,bypass,ordered,mergeFailure,facts,
        result.Succeeded,result.RemainingCount,Convert.ToHexString(result.CopyRows()),result.CopyResidualStorage(),trace.ToArray(),reads));
    byte[] copy=result.CopyRows();if(copy.Length>0){copy[0]^=255;if(copy.AsSpan().SequenceEqual(result.CopyRows()))throw new Exception("Mutable row snapshot");}
}
var noops=new LegacyNativeResidualAssociationOperations(_=>0,_=>-1,_=>new(),_=>new(),_=>false,_=>false,(_,_)=>new(true,0));
Reject(()=>LegacyNativeResidualGlyphAssociation.Apply(new byte[1],[],false,false,192,noops));
Reject(()=>LegacyNativeResidualGlyphAssociation.Apply([],[],false,false,192,null!));
Reject(()=>LegacyNativeResidualGlyphAssociation.Apply([],[],false,false,192,noops,new(true)));
using(var file=File.Create(args[0]))JsonSerializer.Serialize(file,new Report(reports.ToArray(),guards),Context.Default.Report);
Console.WriteLine($"PASS{reports.Count} prepared residual association cases/{guards} guards; actual source-service binding separate.");
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or OperationCanceledException){guards++;return;}throw new Exception("Invalid input accepted");}
static ulong Pack(int node,ushort arc)=>((ulong)(uint)node&0xffffff)|((ulong)arc<<24);
record Fact(byte Kind,int Line,float X0,float Y0,float X1,float Y1,bool Top);
record Case(int Id,string Input,ulong[] References,bool Bypass,bool Ordered,int MergeFailure,Fact[] Facts,
    bool Succeeded,int Remaining,string Rows,ulong[] Storage,ulong[][] MergeCalls,int Reads);
record Report(Case[] Cases,int Guards);
[System.Text.Json.Serialization.JsonSerializable(typeof(Report))]
partial class Context:System.Text.Json.Serialization.JsonSerializerContext;
