using InkRecognition;
using System.Text.Json;

if(args.Length!=8)throw new ArgumentException("AK engine separation case publication line30 line50 capture");
byte[] ak=File.ReadAllBytes(args[0]),line30=File.ReadAllBytes(args[5]),line50=File.ReadAllBytes(args[6]);
using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));var activation=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(activation.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),activation.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));using var cf=File.OpenRead(args[3]);using var pf=File.OpenRead(args[4]);var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);
int prepared=0,actual=0,guards=0,actualCross=0,dead=0;using var file=File.Create(args[7]);using var w=new Utf8JsonWriter(file);
w.WriteStartObject();w.WriteStartArray("cases");
for(int n=0;n<64;n++)
{
    int cap=1+n%8;var groups=new List<LegacyMainGraphAppendGroup>();
    groups.Add(Group(0,0,(byte)n)); //No references:19C4E0 must keep this ID.
    var nodes=new LegacyMainGraphAppendArc[cap][];var rootLists=new int[cap][];
    for(int node=0;node<cap;node++)
    {
        int width=1+(n+node)%8;nodes[node]=new LegacyMainGraphAppendArc[width];rootLists[node]=Enumerable.Range(0,width).Reverse().ToArray();
        for(int a=0;a<width;a++)
        {
            int g=groups.Count;groups.Add(Group((ushort)(g%3),1,(byte)(n+g)));
            nodes[node][a]=new((uint)g,(byte)node,(sbyte)(a%3-1),(byte)(a*3),0,(uint)(node*128+3),65535,(ushort)(n+a),(uint)(n*100+a),[-1],Empty(cap));
        }
    }
    var root=new LegacyMainGraphAppendArc(0,0,0,4,0,0,65535,0,0,[],rootLists);
    var state=LegacyMainGraphAppendState.Create(cap,groups,[],nodes,root);var result=LegacyMainCompactOrdinaryState.FromState(state);
    Write(state,result,false);prepared++;
    var copy=result.Groups[0].CopyRanges();copy[0]=new(255,0,1);if(result.Groups[0].CopyRanges()[0].StrokeIndex!=0)throw new Exception("Group range copy exposed backing storage.");
    var indices=result.Root.CopyIndices();indices[0]=65535;if(result.Root.CopyIndices()[0]==65535)throw new Exception("Compact selector copy exposed backing storage.");
}
foreach(int count in new[]{2047,2048,2049})
{
    const int cap=8;var nodes=new LegacyMainGraphAppendArc[cap][];var lists=new int[cap][];int left=count;
    for(int n=0;n<cap;n++)
    {
        int width=Math.Min(left,257);left-=width;nodes[n]=Enumerable.Range(0,width).Select(_=>new LegacyMainGraphAppendArc(0,(byte)n,0,4,0,(uint)(n*128+3),65535,0,0,[-1],Empty(cap))).ToArray();
        lists[n]=Enumerable.Range(0,width).Reverse().ToArray();
    }
    var state=LegacyMainGraphAppendState.Create(cap,[Group(0,200,7)],[],nodes,new(0,0,0,4,0,0,65535,0,0,[],lists));
    if(count==2049)Reject(()=>LegacyMainCompactOrdinaryState.FromState(state));else{Write(state,LegacyMainCompactOrdinaryState.FromState(state),false);prepared++;}
}
w.WriteEndArray();w.WriteStartArray("actual");
InkStroke M(float y)=>new(new InkPoint[]{new(0,y+90),new(0,y+10),new(25,y+60),new(50,y+10),new(50,y+90)});
InkStroke V(float y)=>new(new InkPoint[]{new(0,y+10),new(0,y+90)});
var mir=new InkStroke[]{M(0),new(new InkPoint[]{new(70,10),new(70,90),new(120,10),new(120,90)}),new(new InkPoint[]{new(140,90),new(140,10),new(180,10),new(190,20),new(190,40),new(180,50),new(140,50)})};
var rows=new[]{V(0),new InkStroke(V(0).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray()),V(140),new InkStroke(V(140).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray())};
foreach(var raw in new[]{new[]{M(0),M(140)},new[]{M(0),M(250)},new[]{V(0),V(140)},new[]{V(0),V(250)},mir,mir.Concat(mir.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+160)).ToArray()))).ToArray(),rows})
{
    var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,cd,pd).Run();
    var source=LegacyFreshMainDescriptorCutSource.Prepare(ak,input.Smoothed);var local=LegacyFreshMainDescriptorGraphAppend.Prepare(ak,source,table);var global=LegacyFreshMainGlobalOrdinaryState.Prepare(ak,local,table);
    var result=LegacyFreshMainCompactOrdinaryGraph.Prepare(ak,global);if(!ReferenceEquals(result.Source,global)||result.State.Groups.Count!=global.State.GroupCount)throw new Exception("Global source/ID ownership changed.");
    for(int g=0;g<result.State.Groups.Count;g++)
    {
        var own=result.ResolveGroupSource(g);var compact=result.State.Groups[g];
        if(own.Primary.DescriptorOrdinal!=compact.DescriptorOrdinal||(own.Secondary?.DescriptorOrdinal??65535)!=compact.SecondaryDescriptorOrdinal)throw new Exception("Group descriptor owner changed.");
        if(own.Secondary is not null)actualCross++;if(global.State.Groups[g].Fields.ReferenceCounter==0)dead++;
    }
    Write(global.State,result.State,true);actual++;
    Reject(()=>LegacyFreshMainCompactOrdinaryGraph.Prepare(ak,global,new(true)));var bad=ak.ToArray();bad[0]^=1;Reject(()=>LegacyFreshMainCompactOrdinaryGraph.Prepare(bad,global));
    Reject(()=>result.State.Root.CopySelector(result.State.Root.SelectorCount));
}
w.WriteEndArray();w.WriteNumber("prepared",prepared);w.WriteNumber("actualCount",actual);w.WriteNumber("actualCross",actualCross);w.WriteNumber("actualDead",dead);w.WriteNumber("guards",guards);w.WriteEndObject();w.Flush();
if(actualCross==0)throw new Exception("No genuine cross-descriptor source group exercised.");
Console.WriteLine($"PASS {prepared} prepared compact projections +{actual} actual source owners/{actualCross} cross groups, {guards} guards;2048 accepted/2049 refused. No global120/227 or NN claim.");
static int[][] Empty(int cap)=>Enumerable.Range(0,cap).Select(_=>Array.Empty<int>()).ToArray();
static LegacyMainGraphAppendGroup Group(ushort descriptor,byte count,byte flags)=>new(new(descriptor,65535,65535,count,1,(byte)(flags+1),(byte)(flags+2),(byte)(flags+3)),new(0,0,1,1),[new(0,0,2)],[[new(0,0,2)]]);
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Unsupported/canceled compact projection accepted.");}
void Write(LegacyMainGraphAppendState input,LegacyMainCompactOrdinaryState result,bool actualCase)
{
    w.WriteStartObject();w.WriteBoolean("actual",actualCase);w.WriteNumber("cap",input.MaximumAtoms);w.WriteStartArray("inputGroups");
    foreach(var g in input.Groups){w.WriteStartObject();var f=g.Fields;Ints("fields",[f.DescriptorOrdinal,f.SecondaryDescriptorOrdinal,f.AuxiliaryContextIndex,f.ReferenceCounter,f.HistoryOwned08,f.Flags50,f.Flags51,f.Flags52]);Box(g.Bounds);Ranges(g.CopyRanges());w.WriteEndObject();}w.WriteEndArray();
    w.WriteStartArray("groups");foreach(var g in result.Groups){w.WriteStartObject();Ints("fields",[g.AllocationId,g.DescriptorOrdinal,g.SecondaryDescriptorOrdinal,g.Flags04,g.Flags05,g.Flags06,g.Kind07]);Box(g.Bounds);Ranges(g.CopyRanges());w.WriteEndObject();}w.WriteEndArray();
    w.WritePropertyName("root");Arc(input.Root,result.Root);w.WriteStartArray("nodes");for(int n=0;n<input.NodeCount;n++){w.WriteStartArray();for(int a=0;a<input.GetNode(n).Count;a++)Arc(input.GetNode(n)[a],result.GetNode(n)[a]);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
}
void Arc(LegacyMainGraphAppendArc input,LegacyMainCompactOrdinaryArc result)
{
    w.WriteStartObject();Ints("inputHeader",[(long)input.GroupId,input.Span,input.Placement,input.Flags,input.StartContextId,input.EndContextId,input.AuxiliaryContextIndex,input.Extra1a,input.Scalar1cBits]);
    Ints("header",[(long)result.GroupId,result.Span,result.Placement,result.Flags,result.StartContextId,result.EndContextId,result.AuxiliaryContextIndex,result.Extra1a,result.Scalar1cBits]);
    w.WriteStartArray("selectors");Ints(null,input.CopyPredecessors().Select(v=>(long)v));foreach(var row in input.CopySuccessors())Ints(null,row.Select(v=>(long)v));w.WriteEndArray();
    Ints("ends",result.CopyCumulativeEnds().Select(v=>(long)v));Ints("indices",result.CopyIndices().Select(v=>(long)v));w.WriteStartArray("read");for(int i=0;i<result.SelectorCount;i++)Ints(null,result.CopySelector(i).Select(v=>(long)v));w.WriteEndArray();w.WriteEndObject();
}
void Box(LegacyMainGraphAppendBounds b)=>Ints("bounds",new[]{b.MinimumX,b.MinimumY,b.MaximumX,b.MaximumY}.Select(v=>(long)BitConverter.SingleToInt32Bits(v)));
void Ranges(LegacyInkRange[] ranges){w.WriteStartArray("ranges");foreach(var r in ranges)Ints(null,[r.StrokeIndex,r.StartPoint,r.EndPointExclusive]);w.WriteEndArray();}
void Ints(string? name,IEnumerable<long> values){if(name is not null)w.WritePropertyName(name);w.WriteStartArray();foreach(long v in values)w.WriteNumberValue(v);w.WriteEndArray();}
