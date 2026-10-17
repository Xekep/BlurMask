using InkRecognition;
using System.Text.Json;

if(args.Length!=8)throw new ArgumentException("AK engine separation case publication line30 line50 capture");
byte[] ak=File.ReadAllBytes(args[0]),line30=File.ReadAllBytes(args[5]),line50=File.ReadAllBytes(args[6]);
using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var a=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));using var cf=File.OpenRead(args[3]);using var pf=File.OpenRead(args[4]);
var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);
int prepared=0,counterCases=0,guards=0,actual=0;using var output=File.Create(args[7]);using var w=new Utf8JsonWriter(output);
w.WriteStartObject();w.WriteStartArray("prepared");
for(int seed=0;seed<96;seed++)
{
    int cap=2+seed%7;var state=LegacyMainGraphAppendState.Empty(cap);
    for(int n=0;n<2+seed%3;n++)
    {
        var local=Fixture(seed+n*13,cap);var before=state;var result=state.Append(local);WriteAppend(before,local,result);state=result.State;prepared++;
    }
}
w.WriteEndArray();w.WriteStartArray("counters");
foreach(int allocated in new[]{0,1,198,199,200,201,500})foreach(int removed in new[]{0,allocated/2,allocated}.Distinct())
{
    byte counter=LegacyMainGraphAppendState.ReplayReferenceCounter(allocated,removed);
    var local=Counter(allocated-removed,counter);var result=LegacyMainGraphAppendState.Empty(3).Append(local);
    w.WriteStartObject();w.WriteNumber("allocated",allocated);w.WriteNumber("removed",removed);w.WriteNumber("counter",counter);
    w.WritePropertyName("local");WriteState(local);w.WritePropertyName("output");WriteState(result.State);Ints("map",result.CopyLocalGroupMap());w.WriteEndObject();counterCases++;
}
w.WriteEndArray();w.WriteStartArray("pending");
for(int code=0;code<32;code++)
{
    byte[] prefix=Enumerable.Range(0,16).Select(i=>(byte)(i*7+code)).ToArray();
    for(int i=0;i<4;i++)prefix[i]=(code&(1<<i))==0?(byte)0:(byte)(i+1);
    var contexts=new List<LegacyMainGraphAppendContext>{new(new byte[16],[17])};
    ushort ordinal=LegacyMainGraphAppendContext.AppendPending(contexts,prefix,[unchecked((int)0xfffffff9),code]);
    w.WriteStartObject();w.WriteBase64String("input",prefix);w.WriteNumber("ordinal",ordinal);w.WriteStartArray("contexts");foreach(var c in contexts)WriteContext(c);w.WriteEndArray();w.WriteEndObject();
}
w.WriteEndArray();w.WriteStartArray("actual");
foreach(float gap in new float[]{140,180,250})
{
    var raw=new InkStroke[]{new(new InkPoint[]{new(0,10),new(0,90)}),new(new InkPoint[]{new(0,gap+10),new(0,gap+90)})};
    var input=Prepare(raw);var cuts=LegacyFreshMainDescriptorCutSource.Prepare(ak,input.Smoothed);
    var result=LegacyFreshMainSingleAtomDescriptorAppend.Prepare(ak,cuts);
    if(!ReferenceEquals(result.Source,cuts)||result.Steps.Count!=2||result.State.NodeCount!=2||result.State.GroupCount!=2||result.State.ContextCount!=1||
        result.State.Groups[0].Fields.DescriptorOrdinal!=0||result.State.Groups[1].Fields.DescriptorOrdinal!=2||
        result.State.GetNode(1)[0].StartContextId!=128||result.State.GetNode(1)[0].EndContextId!=131||
        result.State.GetNode(1)[0].AuxiliaryContextIndex!=0||!result.State.GetNode(1)[0].CopyPredecessors().SequenceEqual(new[]{0}))
        throw new Exception("Actual descriptor/source/pending/seed/root append ownership differs.");
    if(result.ResolveGroupSource(0).LineId!=0||result.ResolveGroupSource(1).LineId!=1||result.ResolveGroupSource(1).FirstPhysical!=1)
        throw new Exception("Global group-to-real-descriptor/guide/physical lineage differs.");
    w.WriteStartObject();w.WriteNumber("gapBits",BitConverter.SingleToInt32Bits(gap));w.WriteStartArray("steps");
    var before=LegacyMainGraphAppendState.Empty(8);
    foreach(var step in result.Steps)
    {
        w.WriteStartObject();w.WriteNumber("descriptor",step.DescriptorOrdinal);w.WriteNumber("line",step.LineId);
        w.WritePropertyName("before");WriteState(before);w.WritePropertyName("local");WriteState(step.LocalState);
        w.WritePropertyName("output");WriteState(step.Append.State);Ints("map",step.Append.CopyLocalGroupMap());
        w.WriteStartArray("base");w.WriteNumberValue(step.Append.NodeOffset);w.WriteNumberValue(step.Append.GroupOffset);w.WriteNumberValue(step.Append.ContextOffset);w.WriteEndArray();w.WriteEndObject();before=step.Append.State;
    }w.WriteEndArray();w.WriteEndObject();actual++;
    var foreign=ak.ToArray();foreign[^1]^=1;Reject(()=>LegacyFreshMainSingleAtomDescriptorAppend.Prepare(foreign,cuts));
    Reject(()=>LegacyFreshMainSingleAtomDescriptorAppend.Prepare(ak,cuts,new(true)));
    var vectors=result.State.GetNode(0)[0].CopySuccessors();vectors[0][0]=999;
    if(result.State.GetNode(0)[0].CopySuccessors()[0][0]!=0)throw new Exception("Public successor snapshot aliases owner.");
}
w.WriteEndArray();
var tooLargeRaw=new InkStroke[]{new(new InkPoint[]{new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)}),new(new InkPoint[]{new(0,230),new(0,150),new(25,200),new(50,150),new(50,230)})};
var larger=Prepare(tooLargeRaw);var largerCuts=LegacyFreshMainDescriptorCutSource.Prepare(ak,larger.Smoothed);
Reject(()=>LegacyFreshMainSingleAtomDescriptorAppend.Prepare(ak,largerCuts));
Reject(()=>LegacyMainGraphAppendState.ReplayReferenceCounter(199,200));
var sample=Fixture(11,3);Reject(()=>LegacyMainGraphAppendState.Empty(2).Append(sample));
Reject(()=>LegacyMainGraphAppendState.Empty(3).Append(sample,new(true)));
var bad=sample.GetNode(0)[0].WithConnections([42],sample.GetNode(0)[0].CopySuccessors());
var rows=Enumerable.Range(0,sample.NodeCount).Select(n=>sample.GetNode(n).ToArray()).ToArray();rows[0][0]=bad;
Reject(()=>LegacyMainGraphAppendState.Create(3,sample.Groups,sample.Contexts,rows,sample.Root));
var zero=sample.Groups.Select(g=>new LegacyMainGraphAppendGroup(g.Fields with{ReferenceCounter=0},g.Bounds,g.CopyRanges(),g.CopyHistory())).ToArray();
Reject(()=>LegacyMainGraphAppendState.Create(3,zero,sample.Contexts,Enumerable.Range(0,sample.NodeCount).Select(n=>sample.GetNode(n).ToArray()).ToArray(),sample.Root));
var empty=LegacyMainGraphAppendState.Empty(3);if(!ReferenceEquals(sample.Append(empty).State,sample))throw new Exception("Empty-source append changed state.");
var many=Counter(1000,200);Reject(()=>many.Append(many));
w.WriteNumber("preparedCount",prepared);w.WriteNumber("counterCases",counterCases);w.WriteNumber("actualCount",actual);w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS{prepared} prepared appends/{counterCases} sticky199/200/201 cases/32 pending contexts;{actual} actual two-descriptor single-atom owners/{guards} guards. Append-stage only; no cross merge/boundary pruning/global NN graph.");

LegacyMainGraphAppendState Fixture(int seed,int cap)
{
    int length=1+seed%6,width=1+seed%3;var descriptions=new List<(int Group,byte Span,int[] Previous)>[length];int groupCount=1;
    var context=new[]{new LegacyMainGraphAppendContext(Enumerable.Range(0,16).Select(n=>(byte)(seed+n)).ToArray(),[seed,~seed]),new LegacyMainGraphAppendContext(new byte[16],[unchecked((int)0xfffffff9)])};
    for(int n=0;n<length;n++)
    {
        descriptions[n]=[];
        for(int v=0;v<width;v++)
        {
            int group=groupCount++;if(group%2==0)groupCount++;
            int[] previous=n==0?new[]{-1}:Enumerable.Range(0,descriptions[n-1].Count).Reverse().ToArray();
            if(seed%5==0&&n>0)previous=previous.Concat(previous.Take(1)).ToArray();descriptions[n].Add((group,0,previous));
        }
        if(n>0&&seed%2!=0)descriptions[n].Add((groupCount++,1,n==1?new[]{-1}:Enumerable.Range(0,descriptions[n-2].Count).ToArray()));
    }
    var vectors=descriptions.Select(row=>row.Select(_=>Enumerable.Range(0,cap).Select(_=>new List<int>()).ToArray()).ToArray()).ToArray();
    var rootVectors=Enumerable.Range(0,cap).Select(_=>new List<int>()).ToArray();
    for(int n=0;n<length;n++)for(int v=0;v<descriptions[n].Count;v++)
    {var d=descriptions[n][v];int previous=n-d.Span-1;foreach(int p in d.Previous)(previous<0?rootVectors[d.Span]:vectors[previous][p][d.Span]).Add(v);}
    var counts=new int[groupCount];foreach(var d in descriptions.SelectMany(v=>v))counts[d.Group]++;
    var groups=Enumerable.Range(0,groupCount).Select(g=>new LegacyMainGraphAppendGroup(new((ushort)(seed%13),(ushort)(g%3==0?seed%13+1:65535),(ushort)(g%4==0?65535:g%2),(byte)counts[g],(byte)(g%2),(byte)(g+1),(byte)(g+2),(byte)(g+3)),
        new(g,g+1,g+2,g+3),new LegacyInkRange[]{new(seed%8,0,3)},g%3==0?Array.Empty<LegacyInkRange[]>():new[]{new[]{new LegacyInkRange(seed%8,0,2)},new[]{new LegacyInkRange(seed%8,1,3)}})).ToArray();
    var nodes=descriptions.Select((row,n)=>row.Select((d,v)=>new LegacyMainGraphAppendArc((uint)d.Group,d.Span,(sbyte)(v%3-1),(byte)(4|v%2),unchecked((uint)(seed%7==0?0xfffffff0u:(uint)n*128)+(uint)v*2),
        unchecked((uint)(seed%7==0?0xfffffff1u:(uint)n*128+3)+(uint)v*2),(ushort)(v%3==0?65535:v%2),(ushort)(seed+v),unchecked((uint)(0x80000000u+(uint)seed)),d.Previous,vectors[n][v].Select(x=>x.ToArray()).ToArray())).ToArray()).ToArray();
    var root=new LegacyMainGraphAppendArc((uint)(seed+7),0,0,4,0,0,ushort.MaxValue,(ushort)seed,unchecked((uint)~seed),[],rootVectors.Select(v=>v.ToArray()).ToArray());
    return LegacyMainGraphAppendState.Create(cap,groups,context,nodes,root);
}
LegacyMainGraphAppendState Counter(int retained,byte reference)
{
    var vectors=new[]{Array.Empty<int>(),Array.Empty<int>(),Array.Empty<int>()};var rows=Enumerable.Range(0,retained).Select(n=>new LegacyMainGraphAppendArc(0,0,0,4,0,3,ushort.MaxValue,0,0,[-1],vectors)).ToArray();
    var group=new LegacyMainGraphAppendGroup(new(0,ushort.MaxValue,ushort.MaxValue,reference,1,0,0,0),new(0,0,1,1),[new(0,0,2)],[]);
    var root=new LegacyMainGraphAppendArc(0,0,0,4,0,0,ushort.MaxValue,0,0,[],new[]{Enumerable.Range(0,retained).ToArray(),Array.Empty<int>(),Array.Empty<int>()});
    return LegacyMainGraphAppendState.Create(3,[group],[],[rows],root);
}
void WriteAppend(LegacyMainGraphAppendState before,LegacyMainGraphAppendState local,LegacyMainGraphAppendResult result)
{w.WriteStartObject();w.WritePropertyName("before");WriteState(before);w.WritePropertyName("local");WriteState(local);w.WritePropertyName("output");WriteState(result.State);Ints("map",result.CopyLocalGroupMap());w.WriteStartArray("base");w.WriteNumberValue(result.NodeOffset);w.WriteNumberValue(result.GroupOffset);w.WriteNumberValue(result.ContextOffset);w.WriteEndArray();w.WriteEndObject();}
void WriteState(LegacyMainGraphAppendState state)
{
    w.WriteStartObject();w.WriteNumber("cap",state.MaximumAtoms);w.WriteStartArray("groups");foreach(var g in state.Groups)
    {
        var f=g.Fields;w.WriteStartObject();Ints("fields",[f.DescriptorOrdinal,f.SecondaryDescriptorOrdinal,f.AuxiliaryContextIndex,f.ReferenceCounter,f.HistoryOwned08,f.Flags50,f.Flags51,f.Flags52]);
        var b=g.Bounds;Ints("bbox",[BitConverter.SingleToInt32Bits(b.MinimumX),BitConverter.SingleToInt32Bits(b.MinimumY),BitConverter.SingleToInt32Bits(b.MaximumX),BitConverter.SingleToInt32Bits(b.MaximumY)]);
        w.WriteStartArray("ranges");foreach(var r in g.CopyRanges())Ints(null,[r.StrokeIndex,r.StartPoint,r.EndPointExclusive]);w.WriteEndArray();
        w.WriteStartArray("history");foreach(var h in g.CopyHistory()){w.WriteStartArray();foreach(var r in h)Ints(null,[r.StrokeIndex,r.StartPoint,r.EndPointExclusive]);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
    }w.WriteEndArray();w.WriteStartArray("contexts");foreach(var c in state.Contexts)WriteContext(c);w.WriteEndArray();
    w.WriteStartArray("nodes");for(int n=0;n<state.NodeCount;n++){w.WriteStartArray();foreach(var arc in state.GetNode(n))WriteArc(arc);w.WriteEndArray();}w.WriteEndArray();w.WritePropertyName("root");WriteArc(state.Root);w.WriteEndObject();
}
void WriteArc(LegacyMainGraphAppendArc a)
{w.WriteStartObject();w.WriteStartArray("header");w.WriteNumberValue(a.GroupId);w.WriteNumberValue(a.Span);w.WriteNumberValue(a.Placement);w.WriteNumberValue(a.Flags);w.WriteNumberValue(a.StartContextId);w.WriteNumberValue(a.EndContextId);w.WriteNumberValue(a.AuxiliaryContextIndex);w.WriteNumberValue(a.Extra1a);w.WriteNumberValue(a.Scalar1cBits);w.WriteEndArray();Ints("previous",a.CopyPredecessors());w.WriteStartArray("next");foreach(var v in a.CopySuccessors())Ints(null,v);w.WriteEndArray();w.WriteEndObject();}
void WriteContext(LegacyMainGraphAppendContext c){w.WriteStartObject();w.WriteBase64String("prefix",c.CopyPrefix());Ints("controls",c.CopyControls());w.WriteEndObject();}
void Ints(string? name,IEnumerable<int> values){if(name is null)w.WriteStartArray();else w.WriteStartArray(name);foreach(int v in values)w.WriteNumberValue(v);w.WriteEndArray();}
void Reject(Action call){try{call();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Invalid append input accepted.");}
LegacyFreshMainOwnedTextPreprocessedInput Prepare(InkStroke[] raw)=>LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,cd,pd).Run();
