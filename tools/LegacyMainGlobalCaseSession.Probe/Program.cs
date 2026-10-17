using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;

if(args.Length!=9)throw new ArgumentException("AK engine separation case publication line30 line50 capture LK");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[8]),engine=File.ReadAllBytes(args[1]);
using var ed=JsonDocument.Parse(engine);var activation=ed.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(activation.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),activation.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[2]));using var cf=File.OpenRead(args[3]);using var pf=File.OpenRead(args[4]);var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);
var model=LegacyRussianModelBundle.Load(ak,lk,engine);byte[] line30=File.ReadAllBytes(args[5]),line50=File.ReadAllBytes(args[6]);
InkStroke M(float y)=>new(new InkPoint[]{new(0,y+90),new(0,y+10),new(25,y+60),new(50,y+10),new(50,y+90)});
InkStroke V(float y)=>new(new InkPoint[]{new(0,y+10),new(0,y+90)});
var mir=new InkStroke[]{M(0),new(new InkPoint[]{new(70,10),new(70,90),new(120,10),new(120,90)}),new(new InkPoint[]{new(140,90),new(140,10),new(180,10),new(190,20),new(190,40),new(180,50),new(140,50)})};
var rows=new[]{V(0),new InkStroke(V(0).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray()),V(140),new InkStroke(V(140).Points.Select(p=>new InkPoint(p.X+40,p.Y)).ToArray())};
int owners=0,spans=0,totalRows=0,computed=0,reused=0,guards=0;
using var file=File.Create(args[7]);using var w=new Utf8JsonWriter(file);w.WriteStartArray();
foreach(var raw in new[]{new[]{M(0),M(140)},new[]{M(0),M(250)},new[]{V(0),V(140)},new[]{V(0),V(250)},mir,mir.Concat(mir.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X,p.Y+160)).ToArray()))).ToArray(),rows,Enumerable.Range(0,14).Select(n=>new InkStroke(M(0).Points.Select(p=>new InkPoint(p.X+n*70,p.Y)).ToArray())).ToArray()})
{
    var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,raw,0,activations,table,cd,pd).Run();
    var cuts=LegacyFreshMainDescriptorCutSource.Prepare(ak,input.Smoothed);var local=LegacyFreshMainDescriptorGraphAppend.Prepare(ak,cuts,table);var global=LegacyFreshMainGlobalOrdinaryState.Prepare(ak,local,table);
    var graph=LegacyFreshMainCompactOrdinaryGraph.Prepare(ak,global);var source=LegacyFreshMainGlobalSourceRanges.Prepare(graph);var context=LegacyFreshMainGlobalContext.Prepare(ak,source,activations);var geometry=LegacyFreshMainGlobalGlyphGeometry.Prepare(ak,context);
    var owner=LegacyFreshMainGlobalCaseSession.Prepare(ak,lk,model,geometry,cd,pd);
    if(!ReferenceEquals(owner.Graph,graph)||!ReferenceEquals(owner.Symbols.Symbols.Symbols.Contexts,owner.Contexts)||!ReferenceEquals(owner.Model,model))throw new Exception("Actual global source/symbol/model ownership lost.");
    LegacyPreparedDictionaryQuery Q()=>new(){Capacity=512,ContextRoot=owner.Contexts.ContainerIdentity};
    var rawPage=owner.Symbols.Symbols.Symbols.CreateQuerySource(model.Lexicon,owner.Symbols.Descriptor.Flags).Read(Q(),false);
    w.WriteStartObject();w.WriteNumber("nodeCount",graph.State.NodeCount);w.WriteString("raw",Convert.ToHexString(rawPage.Records.Span));w.WriteStartArray("arcs");for(int n=0;n<graph.State.NodeCount;n++){w.WriteStartArray();foreach(var a in graph.State.GetNode(n))Ints(null,[(int)a.GroupId,a.Span]);w.WriteEndArray();}w.WriteEndArray();
    w.WriteStartArray("spans");var query=Q();var epochs=new List<int>{-1};owner.AdvanceNodeEpoch(-1);LegacyOwnedGlobalMainWordSpan? previous=null;
    for(int n=0;n<graph.State.NodeCount;n++)
    {
        for(byte s=0;s<owner.GetSpanExtent(n);s++)
        {
            int before=owner.AcousticEvaluations;var span=owner.PrepareSpan(n,s);
            if(owner.AcousticEvaluations-before!=span.ComputedRows)throw new Exception("Acoustic lifecycle count differs.");
            if(previous is not null)Reject(()=>owner.Read(previous,8UL<<40,Q()));
            LegacyOrdinaryDictionaryQueryPreparation.ApplyInitial(query,new(0,0,false,false,120),(uint)n,s,0,false);
            var page=owner.Read(span,8UL<<40,query);int after=owner.AcousticEvaluations;
            var again=owner.Read(span,8UL<<40,query);if(!again.Records.Span.SequenceEqual(page.Records.Span)||after!=owner.AcousticEvaluations)throw new Exception("Repeated source query changed cache/NN state.");
            query.Parameter20=uint.MaxValue;if(owner.Read(span,8UL<<40,query,true).ReturnedCount!=0)throw new Exception("Actual immutable auxiliary60 gained letters.");
            LegacyOrdinaryDictionaryQueryPreparation.ApplyInitial(query,new(0,0,false,false,120),(uint)n,s,0,false);
            if(!owner.Read(span,8UL<<40,query).Records.Span.SequenceEqual(page.Records.Span))throw new Exception("Normal/auxiliary slot mode interleave changed results.");
            for(int i=0;i<page.ReturnedCount;i++)
            {
                var row=page.Records.Span.Slice(i*32,32);ulong id=BinaryPrimitives.ReadUInt64LittleEndian(row[8..]);
                if(row[4]!=1||owner.Contexts.ReadNode(id).Symbol!=BinaryPrimitives.ReadUInt16LittleEndian(row[20..]))throw new Exception("Case fulltag/context ownership changed.");
            }
            w.WriteStartObject();w.WriteNumber("node",n);w.WriteNumber("span",s);w.WriteNumber("computed",span.ComputedRows);w.WriteNumber("reused",span.ReusedRows);w.WriteNumber("evaluations",owner.AcousticEvaluations);
            w.WriteBase64String("permissions",span.CopyPermissions());w.WriteBase64String("eligibility",span.CopyEligibility());Ints("map",span.CopyWorkingMap().Select(v=>(int)v));w.WriteString("records",Convert.ToHexString(page.Records.Span));
            w.WriteStartArray("costs");for(ushort a=0;a<graph.State.GetNode(n).Count;a++)if(graph.State.GetNode(n)[a].Span==s){var c=owner.CopyClassCosts(span,n,a);Ints(null,c);int saved=c[0];c[0]^=7;if(owner.CopyClassCosts(span,n,a)[0]!=saved)throw new Exception("Public cache snapshot mutated retained pool data.");}w.WriteEndArray();w.WriteEndObject();
            var oldPermissions=span.CopyPermissions();oldPermissions[0]^=7;if(oldPermissions.SequenceEqual(span.CopyPermissions()))throw new Exception("Public permission copy retained.");
            Reject(()=>owner.Read(span,8UL<<40,new(){Capacity=512,ContextRoot=owner.Contexts.ContainerIdentity+1}));
            Reject(()=>owner.Read(span,8UL<<40,new(){Capacity=512,ContextRoot=owner.Contexts.ContainerIdentity,ParentContext=ulong.MaxValue}));
            if(owner.Read(span,8UL<<40,query).ReturnedCount!=page.ReturnedCount)throw new Exception("Rejected caller guard poisoned valid current owner.");
            computed+=span.ComputedRows;reused+=span.ReusedRows;totalRows+=page.ReturnedCount;spans++;previous=span;
        }
        owner.AdvanceNodeEpoch(n);epochs.Add(n);if(previous is not null)Reject(()=>owner.Read(previous,8UL<<40,Q()));previous=null;
    }
    w.WriteEndArray();Ints("epochs",epochs);w.WriteStartArray("contexts");foreach(var c in owner.Contexts.CopyNodes()){w.WriteStartArray();w.WriteNumberValue(c.Identity);w.WriteNumberValue(c.ParentLink);w.WriteNumberValue(c.Symbol);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
    Reject(()=>LegacyFreshMainGlobalCaseSession.Prepare(ak,lk,model,geometry,cd,pd,new(true)));owners++;
}
w.WriteEndArray();w.Flush();if(computed==0||totalRows==0)throw new Exception("Actual global cache/queries not exercised.");Console.WriteLine($"PASS {owners} global owners/{spans} native ordered D0 spans/{computed} computed/{reused} reused rows/{totalRows} case records/{guards} guards; repeated queries perform no NN; ONE backend and node-only epochs.");
void Ints(string? name,IEnumerable<int> values){if(name is not null)w.WritePropertyName(name);w.WriteStartArray();foreach(int v in values)w.WriteNumberValue(v);w.WriteEndArray();}
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidDataException or InvalidOperationException or OperationCanceledException){guards++;return;}throw new Exception("Unsupported global session operation accepted.");}
