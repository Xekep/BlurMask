using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text.Json;
using InkRecognition;

if(args.Length!=9)throw new ArgumentException("AK LK engine separation case-data publication-data line30 line50 output.json");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]),separationBytes=File.ReadAllBytes(args[3]);
using var caseFile=File.OpenRead(args[4]);using var publicationFile=File.OpenRead(args[5]);
var caseData=LegacyNativeCaseData.Load(caseFile);var publication=LegacyNativeTextPublicationData.Load(publicationFile);
var model=LegacyRussianModelBundle.Load(ak,lk,engine);var table=LegacyCursiveSeparationTable.Load(separationBytes);
var sampled=MainFixture.Create(ak,engine,table);
var dictionary=LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,model,sampled,caseData,publication);
var owner=new LegacyOwnedMainWordSpanPreparation(ak,dictionary,sampled,table);
var policy=LegacyFreshCaseMainSourcePolicy.Prepare(ak,lk,LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak,owner));
var confidence=LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,dictionary);
var producer=LegacyOwnedRussianOrdinaryGlyphProducer.Prepare(ak,policy,confidence,table);
var line=MainLineWordFixture.Create(ak,engine,separationBytes,File.ReadAllBytes(args[6]),File.ReadAllBytes(args[7]),caseData,publication,2);
var lineDictionary=LegacyFreshCaseRussianDictionaryPreparation.Prepare(ak,lk,model,line.Source,caseData,publication);
var lineOwner=new LegacyOwnedMainWordSpanPreparation(ak,lineDictionary,line.Source,line.Context,table);
var linePolicy=LegacyFreshCaseMainSourcePolicy.Prepare(ak,lk,LegacyOwnedFreshMainWordControllerPolicy.Prepare(ak,lineOwner));
var lineConfidence=LegacyOwnedRussianMainGlyphConfidence.Prepare(ak,lineDictionary,line.Context);
var lineProducer=LegacyOwnedRussianOrdinaryGlyphProducer.Prepare(ak,linePolicy,lineConfidence,table);
int controls=0,guards=0,partialFailures=0,applied=0;
using(var output=File.Create(args[8]))using(var w=new Utf8JsonWriter(output))
{
    w.WriteStartObject();w.WriteStartArray("sources");
    Capture("onePhysical",producer,confidence,owner);
    Capture("twoPhysical",lineProducer,lineConfidence,lineOwner);
    w.WriteEndArray();

    byte[] row=new byte[72];W16(row,0x3a,0);W32(row,0x20,-1);var valid=Scratch();
    Reject(()=>LegacyOwnedRussianOrdinaryGlyphProducer.Prepare(ak,null!,confidence,table));
    Reject(()=>LegacyOwnedRussianOrdinaryGlyphProducer.Prepare(ak,policy,null!,table));
    Reject(()=>LegacyOwnedRussianOrdinaryGlyphProducer.Prepare(ak,policy,confidence,null!));
    Reject(()=>LegacyOwnedRussianOrdinaryGlyphProducer.Prepare(ak,policy,lineConfidence,table));
    Reject(()=>LegacyOwnedRussianOrdinaryGlyphProducer.Prepare(ak,policy,confidence,table,new(true)));
    var corrupt=ak.ToArray();corrupt[566416]^=1;Reject(()=>LegacyOwnedRussianOrdinaryGlyphProducer.Prepare(corrupt,policy,confidence,table));
    Reject(()=>producer.Produce(row,null!));Reject(()=>producer.Produce(row.AsSpan(1),valid));
    Reject(()=>new LegacyOrdinaryGlyphProductionScratch(new ushort[255],256));
    var wrongNode=row.ToArray();W32(wrongNode,0,-1);Unchanged(()=>producer.Produce(wrongNode,valid),valid);
    var wrongArc=row.ToArray();W16(wrongArc,4,65535);Unchanged(()=>producer.Produce(wrongArc,valid),valid);
    var wrongType=row.ToArray();wrongType[0x3c]=1;Unchanged(()=>producer.Produce(wrongType,valid),valid);
    var protocol=row.ToArray();W16(protocol,0x3a,0xfffa);Unchanged(()=>producer.Produce(protocol,valid),valid);
    var unknown=row.ToArray();W16(unknown,0x3a,1000);Unchanged(()=>producer.Produce(unknown,valid),valid);
    Unchanged(()=>producer.Produce(row,valid,token:new(true)),valid);
    var badCompact=row.ToArray();W32(badCompact,0x20,0);Unchanged(()=>producer.Produce(badCompact,valid),valid);
    Unchanged(()=>producer.Produce(badCompact,valid,preparedCompact40:new byte[39]),valid);
    W32(badCompact,0x20,-2);Unchanged(()=>producer.Produce(badCompact,valid,preparedCompact40:new byte[40]),valid);
    //FFFD is an unresolved native service AFTER the native decrement; there
    //is no manufactured callback result or claim that native returns false.
    var replacement=row.ToArray();W16(replacement,0x3a,189);var partial=Scratch();
    byte[] oldLabels=LabelBytes(partial.CopyLabels());Reject(()=>producer.Produce(replacement,partial));
    Require(partial.RemainingCursor==255&&partial.ComponentFlag==0&&partial.ExternalVariant==0&&
        partial.Score0CBits==0x7fc12345&&partial.Score10Bits==0x80000000&&partial.Cost14==-123456789&&
        partial.PublicationFlags06==0xa55a&&LabelBytes(partial.CopyLabels()).AsSpan().SequenceEqual(oldLabels),"Unsupported resolver rolled back or over-wrote a prefix.");partialFailures++;
    var zero=Scratch(0);Reject(()=>producer.Produce(row,zero));Require(zero.RemainingCursor==-1&&zero.ComponentFlag==0&&zero.ExternalVariant==0,"Scratch OOB prefix differs.");partialFailures++;
    var wrap=Scratch(int.MinValue);Reject(()=>producer.Produce(row,wrap));Require(wrap.RemainingCursor==int.MaxValue,"Native cursor wrap differs.");partialFailures++;
    var tooLarge=Scratch(257);Reject(()=>producer.Produce(row,tooLarge));Require(tooLarge.RemainingCursor==256,"Scratch capacity prefix differs.");partialFailures++;
    w.WriteNumber("controls",controls);w.WriteNumber("guards",guards);w.WriteNumber("partialFailures",partialFailures);w.WriteNumber("applied",applied);
    w.WriteEndObject();w.Flush();

    void Capture(string name,LegacyOwnedRussianOrdinaryGlyphProducer target,LegacyOwnedRussianMainGlyphConfidence actual,LegacyOwnedMainWordSpanPreparation spanOwner)
    {
        Require(ReferenceEquals(actual.CaseDictionaryOwner,target.Policy.DictionaryOwner),"Confidence identity lost.");
        w.WriteStartObject();w.WriteString("name",name);w.WriteNumber("height",spanOwner.SourceHeight);
        w.WriteStartArray("points");foreach(var point in spanOwner.SourceInk.Points.Span){w.WriteStartArray();w.WriteNumberValue(point.X);w.WriteNumberValue(point.Y);w.WriteEndArray();}w.WriteEndArray();
        w.WriteStartArray("counts");foreach(int count in spanOwner.SourceInk.StrokePointCounts.Span)w.WriteNumberValue(count);w.WriteEndArray();
        w.WriteStartArray("groups");
        for(int group=0;group<actual.Graph.Groups.Count;group++)
        {
            var scored=actual.ScoreGroup(group);w.WriteStartObject();w.WriteNumber("group",group);
            w.WriteStartArray("probabilities");for(int f=0;f<4;f++){w.WriteStartArray();foreach(int p in scored.CopyFamilyProbabilities(f))w.WriteNumberValue(p);w.WriteEndArray();}w.WriteEndArray();
            w.WriteStartArray("candidates");foreach(var c in scored.Candidates.Span){w.WriteStartArray();w.WriteNumberValue(c.Symbol);w.WriteNumberValue(c.Cost);w.WriteNumberValue(BitConverter.SingleToUInt32Bits(c.Confidence));w.WriteEndArray();}w.WriteEndArray();
            w.WriteStartArray("ranges");foreach(var range in actual.Graph.Groups[group].Ranges.Span){w.WriteStartArray();w.WriteNumberValue(range.StrokeIndex);w.WriteNumberValue(range.StartPoint);w.WriteNumberValue(range.EndPointExclusive);w.WriteEndArray();}w.WriteEndArray();
            w.WriteStartArray("cases");
            var endpoint=Enumerable.Range(0,actual.Graph.NodeCount).SelectMany(n=>actual.Graph.GetNode(n).Select((a,i)=>(n,a,i))).First(x=>x.a.GroupId==group);
            //These72-byte rows are prepared scalar controls over genuine
            //graph/cache groups. None is asserted to be a winning backtrack.
            ushort[] symbols=scored.Candidates.ToArray().Select(c=>c.Symbol).Concat(new ushort[]{178,191,193,194,0xfff7,0xfff8,0xfff9}).Distinct().ToArray();
            foreach(ushort symbol in symbols)for(int mode=0;mode<4;mode++)
            {
                byte[] prepared=Enumerable.Range(0,72).Select(i=>(byte)(i*13+17)).ToArray();W32(prepared,0,endpoint.n);W16(prepared,4,(ushort)endpoint.i);
                W16(prepared,0x3a,symbol);prepared[0x3c]=0;prepared[0x3d]=(byte)(mode&1);prepared[0x3e]=new byte[]{0,1,127,255}[mode];
                byte[] compact=mode<2?[]:Enumerable.Range(0,40).Select(i=>(byte)(i*7+11)).ToArray();W32(prepared,0x20,mode<2?-1:0);
                int cursor=mode==3?1:256;var state=Scratch(cursor);ulong conversion=(ulong)(mode&2);
                var result=target.Produce(prepared,state,conversion,compact);Require(result.ProducerSucceeded&&result.RemainingCursor==cursor-1,"Ordinary success/cursor differs.");
                var labels=state.CopyLabels();labels[cursor-1]^=0x1234;Require(labels[cursor-1]!=state.CopyLabels()[cursor-1],"Scratch snapshot escaped.");
                byte[] initial=Enumerable.Range(0,40).Select(i=>(byte)(i*31+7)).ToArray();W32(initial,0,123);
                var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);ulong reference=pool.AppendDescriptor(2,123,false,new byte[40]).Reference;
                pool.ReplaceRecord(reference,initial);bool ok=LegacyPreparedGlyphFinalization.Apply(pool,reference,prepared,result,false);if(ok)applied++;
                w.WriteStartObject();w.WriteString("source",Convert.ToHexString(prepared));w.WriteString("compact40",Convert.ToHexString(compact));w.WriteNumber("cursor",cursor);w.WriteNumber("conversion",conversion);
                w.WriteNumber("remaining",state.RemainingCursor);w.WriteNumber("score0C",state.Score0CBits);w.WriteNumber("score10",state.Score10Bits);w.WriteNumber("cost",state.Cost14);
                w.WriteNumber("component",state.ComponentFlag);w.WriteNumber("external",state.ExternalVariant);w.WriteNumber("publication",state.PublicationFlags06);
                w.WriteString("labels",Convert.ToBase64String(LabelBytes(state.CopyLabels())));w.WriteString("before40",Convert.ToHexString(initial));w.WriteString("after40",Convert.ToHexString(pool.CopyRecord(reference)));w.WriteBoolean("finalized",ok);w.WriteEndObject();controls++;
            }
            w.WriteEndArray();w.WriteEndObject();
        }
        w.WriteEndArray();w.WriteEndObject();
    }
}
Console.WriteLine($"PASS {controls} prepared72/actualNN ordinary216BC0 controls, {applied} full40 finalizations; {guards} guards/{partialFailures} retained-prefix refusals. Actual backtrack/winner producer remains separate.");
static LegacyOrdinaryGlyphProductionScratch Scratch(int cursor=256)=>new(Enumerable.Range(0,256).Select(i=>(ushort)(0x8000+i*3)).ToArray(),cursor,7,9,0x7fc12345,0x80000000,-123456789,0xa55a);
void Unchanged(Action action,LegacyOrdinaryGlyphProductionScratch state){var before=state.CopyLabels();int cursor=state.RemainingCursor;Reject(action);Require(state.RemainingCursor==cursor&&state.ComponentFlag==7&&state.ExternalVariant==9&&state.CopyLabels().AsSpan().SequenceEqual(before),"Before-entry guard wrote scratch.");}
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException or OperationCanceledException or InvalidOperationException){guards++;return;}throw new Exception("Invalid scope accepted.");}
static byte[] LabelBytes(ushort[] labels){var bytes=new byte[512];for(int i=0;i<256;i++)W16(bytes,i*2,labels[i]);return bytes;}
static void W16(Span<byte> bytes,int at,ushort value)=>BinaryPrimitives.WriteUInt16LittleEndian(bytes[at..],value);
static void W32(Span<byte> bytes,int at,int value)=>BinaryPrimitives.WriteInt32LittleEndian(bytes[at..],value);
static void Require(bool value,string message){if(!value)throw new Exception(message);}
