using InkRecognition;
using System.Buffers.Binary;

if(args.Length is not (0 or 3))throw new ArgumentException("Usage: LegacyRowAcousticInput.Probe [ru-ak.res ru-lk.res engine-profile.json]");
byte[] own=OwnSource();var profile=LegacyOrdinaryRowAcousticProfile.ReadResourceNode(own,0);
if(profile.GeometryNodeOffset!=profile.Source.EnclosureFlagOffset+16 || profile.SmoothingNodeOffset!=profile.GeometryNodeOffset+36)
    throw new Exception("Source processor order/offset mismatch.");
for(int length=0;length<own.Length;length++)ExpectInvalid(()=>LegacyOrdinaryRowAcousticProfile.ReadResourceNode(own.AsSpan(0,length),0));
byte[] bad=own.ToArray();bad[profile.Source.ParametersOffset+16]=1;ExpectInvalid(()=>LegacyOrdinaryRowAcousticProfile.ReadResourceNode(bad,0));
InkStroke[] sample=[Stroke((0,70),(0,0),(25,40),(50,0),(50,70)),Stroke((65,70),(65,0))];
var prepared=LegacyOrdinaryRowAcousticInput.Prepare(sample,profile);
var cuts=prepared.GenerateRussianCuts();
if(prepared.Prepared.Strokes.Count!=sample.Length || prepared.Chronology.SourceFactor!=200 || prepared.Prepared.PointKeys200.Length!=prepared.Prepared.Strokes.Sum(s=>s.Points.Count))
    throw new Exception("Acoustic prepared ownership/count/factor mismatch.");
for(int s=0;s<prepared.Prepared.Strokes.Count;s++)
{
    var first=prepared.Chronology.Resolve(s,0).Key;var last=prepared.Chronology.Resolve(s,prepared.Prepared.Strokes[s].Points.Count-1).Key;
    if(first.Second!=s||first.Third!=0||last.Second!=s||last.Third!=sample[s].Points.Count-1)throw new Exception("Original endpoint chronology mismatch.");
}
if(!cuts.Cuts.PreparedInk.StrokePointCounts.Span.SequenceEqual(cuts.Chronology.StrokePointCounts.Span))throw new Exception("Postcut map count mismatch.");
Console.WriteLine($"PASS own source247 reader,truncation/order rejection,24a→deskew→265→factor200→297 cuts, {prepared.Prepared.PointKeys200.Length} curve points.");
if(args.Length==3)
{
    var acoustic=File.ReadAllBytes(args[0]);var model=LegacyRussianModelBundle.Load(acoustic,File.ReadAllBytes(args[1]),File.ReadAllBytes(args[2]));
    var first1=LegacyInkNetwork.Load(acoustic.AsSpan(19195,105190-19195));
    var first2=LegacyInkNetwork.Load(acoustic.AsSpan(105197,187592-105197));
    Console.WriteLine($"First source21d@13082 diagnostic network shapes {first1.InputWidth}/{first1.HiddenWidth}/{first1.OutputWidth}, {first2.InputWidth}/{first2.HiddenWidth}/{first2.OutputWidth}; dynamic weights absent (not passed to second-model loader).");
    profile=LegacyOrdinaryRowAcousticProfile.ReadResourceNode(acoustic,6120);
    if(profile.GeometryNodeOffset!=11421||profile.SmoothingNodeOffset!=11457)throw new Exception("Current RU row source profile identity mismatch.");
    //Outer source is not the current row producer: early2ab/2d0 plus sixpostprocessors.
    ExpectInvalid(()=>LegacyOrdinaryRowAcousticProfile.ReadResourceNode(acoustic,0));
    InkStroke[] mir=[Stroke((0,90),(0,10),(25,60),(50,10),(50,90)),Stroke((65,90),(65,10)),Stroke((65,90),(115,10)),Stroke((115,90),(115,10)),Stroke((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))];
    var originalCosts=new Dictionary<string,int[]>();
    foreach(float scale in new[]{1f,1.5f})
    {
        var input=mir.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X*scale+30,p.Y*scale+20)).ToArray())).ToArray();
        prepared=LegacyOrdinaryRowAcousticInput.Prepare(input,profile);cuts=prepared.GenerateRussianCuts();
        if(prepared.Chronology.SourceFactor!=200||cuts.Cuts.PreparedInk.StrokePointCounts.Length!=mir.Length)throw new Exception("Actual source265 count/map failed.");
        var options=new LegacyCursiveWordSearchOptions(prepared.Frame.Line.Baseline,prepared.Frame.Line.Height,0,1);
        var evaluator=prepared.CreateEvaluator(model,cuts,options);
        var counts=cuts.Cuts.PreparedInk.StrokePointCounts.ToArray();
        foreach(int[] ids in new int[][]{[0],[1,2,3],[4]})
        {
            var glyph=evaluator.Classify(ids.Select(i=>new LegacyInkRange(i,0,counts[i])).ToArray());
            string key=string.Join(',',ids);var currentCosts=glyph.IntegerCosts.ToArray();
            if(scale==1)originalCosts.Add(key,currentCosts);
            else if(!originalCosts[key].AsSpan().SequenceEqual(currentCosts))throw new Exception("Owned scale/translate NN costs changed.");
            var ranked=currentCosts.Select((cost,id)=>(cost,id)).OrderBy(v=>v.cost).Take(5).ToArray();
            if(glyph.IntegerCosts.Length!=180||ranked.Any(v=>v.cost<0||v.cost>=LegacyWordCandidateScorer.ImpossibleCost))throw new Exception("Actual four-family neural output invalid.");
            Console.WriteLine($"Actual current24a11421→26511457→keys/cuts→supplied secondmodel diagnostic scale{scale},frame{prepared.Frame.Line},ids{string.Join(',',ids)}: {string.Join(',',ranked.Select(v=>$"{(model.AcousticAlphabet.TryGetText((ushort)v.id,out var text)?text:"?")}:{v.cost}"))}");
        }
        var foreign=LegacyOrdinaryRowAcousticInput.Prepare(input,profile).GenerateRussianCuts();
        ExpectInvalid(()=>prepared.CreateEvaluator(model,foreign,options));
        ExpectInvalid(()=>prepared.CreateEvaluator(model,cuts,options with{SourceHeight=options.SourceHeight+1}));
    }
}
static InkStroke Stroke(params(float X,float Y)[] p)=>new(p.Select(v=>new InkPoint(v.X,v.Y)).ToArray());
static void ExpectInvalid(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException){return;}throw new Exception("Invalid source/ownership accepted.");}
static byte[] OwnSource()
{
    using var stream=new MemoryStream();using var w=new BinaryWriter(stream);
    w.Write(0x0101fe90u);w.Write(0u);w.Write(0x247u);w.Write((ushort)0);w.Write((ushort)0x10d);w.Write((ushort)0x201);w.Write((ushort)0x100);
    w.Write(new byte[4+56]);w.Write(0);w.Write(0);w.Write((byte)0);w.Write(0x135);w.Write((byte)0);w.Write((byte)0);w.Write((byte)0);w.Write((byte)0);w.Write(0f);
    foreach(float value in new[]{.96f,.85f,3f,10f})w.Write(value);
    w.Write(new byte[4]);w.Write(.25f);w.Write((byte)0);w.Write((byte)0);w.Write(0f);w.Write((byte)0);w.Write(0f);w.Write((byte)0);w.Write(2);
    w.Write(0x0101fe90u);w.Write(28u);w.Write(0x24au);w.Write((ushort)0);w.Write((ushort)0x102);w.Write(0);w.Write(100f);w.Write((byte)1);w.Write(-.2f);w.Write(.2f);w.Write((byte)4);w.Write((byte)6);w.Write((byte)0);
    w.Write(0x0101fe90u);w.Write(40u);w.Write(0x265u);w.Write((ushort)0);w.Write((ushort)0x102);foreach(float value in new[]{.05f,.075f,.075f,.15f,.35f,.10f,.15f,.0001f})w.Write(value);
    byte[] bytes=stream.ToArray();BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4),(uint)bytes.Length-8);return bytes;
}
