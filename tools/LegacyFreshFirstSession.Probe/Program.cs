using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length is not (1 or 3))throw new ArgumentException("Usage: LegacyFreshFirstSession.Probe old-cache-controls.json [ru-ak.res engine-profile.json]");
using var own=JsonDocument.Parse(File.ReadAllBytes(args[0]));
byte[] resource=OwnSource(Convert.FromBase64String(own.RootElement.GetProperty("alphabet").GetString()!));
int offset=0;
var activations=new LegacyInkActivations(new int[48],Enumerable.Repeat(512,128).ToArray());
// Own tiny network source uses the real layout but no vendor weights.
var syntheticModel=LegacyFirstAcousticModel.ReadSource(resource,0);
int profileStart=syntheticModel.OrdinaryNodeOffset+21+8+(int)BinaryPrimitives.ReadUInt32LittleEndian(resource.AsSpan(syntheticModel.OrdinaryNodeOffset+25))+27;
BinaryPrimitives.WriteInt16LittleEndian(resource.AsSpan(profileStart),1);
BinaryPrimitives.WriteInt16LittleEndian(resource.AsSpan(profileStart+6),30);
int metadata=resource.Length-186;
for(int n=0;n<512;n++)
{
    var bytes=resource.ToArray();uint modeMask=unchecked(0x10204081u*(uint)n);
    BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(metadata+13*4),modeMask);
    BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(metadata+64+13*2),(ushort)(n+1));
    BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(metadata+96+13*2),(ushort)(n*3));
    BinaryPrimitives.WriteUInt16LittleEndian(bytes.AsSpan(metadata+162+2*2),(ushort)(n*5));
    bytes[metadata+128]=(byte)(n%32);
    short cap=(short)(3*(n%80+1)+n%3);BinaryPrimitives.WriteInt16LittleEndian(bytes.AsSpan(profileStart+6),cap);
    var policy=LegacyFreshFirstControllerPolicy.ReadFirstSource(bytes,0);
    uint expected=0;for(int bit=0;bit<32;bit++)if(((0x21703u>>bit)&1)!=0&&((modeMask>>bit)&1)==0)expected|=1u<<bit;
    if(policy.FrontendFlags!=expected||policy.SourceMode!=13||policy.SourceVariant!=2||policy.MaximumSourceArcs!=n+1||
        policy.SourceAuxiliaryLimit!=n*3||policy.SourceAlternativeLimit!=n*5||policy.PruneCandidates!=(n%32>=14)||policy.MaximumNormalizedStrokes!=n%80+1)
        throw new Exception("Constructed metadata policy decode differs.");
    var c=policy.CreateContext(new(true,false,0,.25f),[]);
    if(c.PruneCandidates!=policy.PruneCandidates||c.SuppressFallback||c.SkipOrdinaryKnowledgeRules!=((expected&16)!=0)||c.ShareRelatedFormMinimum||!c.IsWholePhysicalStroke||c.WholeStrokeEndpointCategoriesZero||c.SourceStrokeProbability!=.25f)
        throw new Exception("Fresh context scalar forwarding differs.");
}
var inactive=resource.ToArray();BinaryPrimitives.WriteInt16LittleEndian(inactive.AsSpan(profileStart),0);
if(LegacyFreshFirstControllerPolicy.ReadFirstSource(inactive,0).MaximumNormalizedStrokes!=32767)throw new Exception("Inactive pen-up profile cap differs.");
var invalid=resource.ToArray();BinaryPrimitives.WriteInt16LittleEndian(invalid.AsSpan(profileStart+6),2);ExpectInvalid(()=>LegacyFreshFirstControllerPolicy.ReadFirstSource(invalid,0));
if(args.Length==3)
{
    resource=File.ReadAllBytes(args[1]);offset=6120;using var json=JsonDocument.Parse(File.ReadAllBytes(args[2]));var a=json.RootElement.GetProperty("activations");
    activations=new(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
    var policy=LegacyFreshFirstControllerPolicy.ReadFirstSource(resource,offset);
    if(policy!=new LegacyFreshFirstControllerPolicy(0x21700,13,2,200,0,20,true,10))throw new Exception("Actual first policy differs.");
}
InkStroke[] mir=[Stroke((0,90),(0,10),(25,60),(50,10),(50,90)),Stroke((65,90),(65,10)),Stroke((65,90),(115,10)),Stroke((115,90),(115,10)),Stroke((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))];
int checkedGlyphs=0;
foreach(float scale in new[]{1f,1.5f})
{
    var input=mir.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X*scale+7,p.Y*scale-19)).ToArray())).ToArray();
    var session=LegacyFreshFirstGlyphSession.Prepare(resource,offset,input,activations);
    var duplicate=LegacyFreshFirstGlyphSession.Prepare(resource,offset,input.Select(s=>new InkStroke(s.Points.SelectMany(p=>new[]{p,p}).ToArray())).ToArray(),activations);
    byte[] mask=Enumerable.Repeat((byte)1,session.Scorer.Alphabet.LabelCount).ToArray();var counts=session.Cuts.Cuts.PreparedInk.StrokePointCounts.ToArray();
    foreach(int[] ids in new int[][]{[0],[1,2,3],[4]})
    {
        var ranges=ids.Select(i=>new LegacyInkRange(i,0,counts[i])).ToArray();var result=session.Score(ranges,0,15,mask,[]);
        var duplicateCounts=duplicate.Cuts.Cuts.PreparedInk.StrokePointCounts.ToArray();var duplicateRanges=ids.Select(i=>new LegacyInkRange(i,0,duplicateCounts[i])).ToArray();
        var repeat=duplicate.Score(duplicateRanges,0,15,mask,[]);
        if(!result.Cache.Costs.Span.SequenceEqual(repeat.Cache.Costs.Span)||!result.Glyph.IntegerCosts.Span.SequenceEqual(repeat.Glyph.IntegerCosts.Span))throw new Exception("Raw duplicate ownership changed glyph/cache scores.");
        var order=session.Cuts.Chronology.OrderRangeIndicesForNormalization(ranges);var signs=LegacyCursiveGlyphBoundaryPolicy.Resolve(order.Select(i=>ranges[i]).ToArray(),counts,0,false,false);
        var resolved=session.Scope.Evaluate(ranges,result.Glyph.MergedGeometry.Strokes.Count);
        var manual=session.Scorer.Score(session.Cuts,ranges,signs.Start,signs.End,15,10,mask,session.Policy.CreateContext(resolved,[]),[]);
        if(!result.Cache.Costs.Span.SequenceEqual(manual.Cache.Costs.Span))throw new Exception("Owned session differs from explicit prepared pipeline.");
        if(ids.Length==1&&(!resolved.BoundaryAlignedSingleNormalizedStroke||resolved.EndpointFieldsAreZero||resolved.ProbabilityStrokeIndex!=ids[0]))throw new Exception("Original full-stroke endpoints differ.");
        if(ids.Length>1&&resolved.BoundaryAlignedSingleNormalizedStroke)throw new Exception("Unmerged multistroke scope incorrectly whole.");
        string Text(ushort id)=>session.Scorer.Alphabet.TryGetText(id,out string t)?t:"?";
        Console.WriteLine($"fresh23f scale={scale} physical[{string.Join(',',ids)}] {string.Join(',',result.Cache.Candidates.ToArray().Take(4).Select(v=>$"{Text(v.Symbol)}:{v.Cost}"))}; fallback={result.Cache.FallbackCost}");checkedGlyphs++;
    }
    using var cts=new CancellationTokenSource();cts.Cancel();try{session.Score([new(0,0,counts[0])],0,15,mask,[],cts.Token);throw new Exception("Canceled session accepted.");}catch(OperationCanceledException){}
    bool called=false;
    using var callbackCancel=new CancellationTokenSource();
    try{session.Scorer.Score(session.Cuts,[new(0,0,counts[0])],1024,1024,15,10,mask,g=>{called=true;callbackCancel.Cancel();return session.Policy.CreateContext(session.Scope.Evaluate([new(0,0,counts[0])],g.MergedGeometry.Strokes.Count),[]);},[],callbackCancel.Token);throw new Exception("Post-callback cancellation ignored.");}catch(OperationCanceledException){}
    if(!called)throw new Exception("Context producer wasn't called after classification.");
    ExpectInvalid(()=>session.Scorer.Score(session.Cuts,[new(0,0,counts[0])],1024,1024,15,10,mask,_=>null!,[]));
    ExpectInvalid(()=>new LegacyFirstRowPrimitiveScope(session.Row,duplicate.Cuts,input.Select(s=>s.Points.Count).ToArray()));
}
// Point scope is validated separately from glyph feature extraction (which has
// its own nondegenerate geometry constraint). Ingress/smoothing retain one point.
var pointInput=mir.Append(Stroke((205,15))).ToArray();var pointSession=LegacyFreshFirstGlyphSession.Prepare(resource,offset,pointInput,activations);
var pointCounts=pointSession.Cuts.Cuts.PreparedInk.StrokePointCounts.ToArray();var dot=pointSession.Scope.Evaluate([new(5,0,pointCounts[5])],1);
if(!dot.BoundaryAlignedSingleNormalizedStroke||!dot.EndpointFieldsAreZero||dot.ProbabilityStrokeIndex!=5)throw new Exception("Single raw point endpoint scope differs.");
var repeatedPoint=pointInput.ToArray();repeatedPoint[^1]=Stroke((205,15),(205,15));
var repeatedPointSession=LegacyFreshFirstGlyphSession.Prepare(resource,offset,repeatedPoint,activations);
var repeatedCounts=repeatedPointSession.Cuts.Cuts.PreparedInk.StrokePointCounts.ToArray();
var repeatedDot=repeatedPointSession.Scope.Evaluate([new(5,0,repeatedCounts[5])],1);
if(repeatedCounts[5]!=pointCounts[5]||!repeatedDot.BoundaryAlignedSingleNormalizedStroke||repeatedDot.EndpointFieldsAreZero)
    throw new Exception("Discarded trailing raw duplicate endpoint interval was lost.");
Console.WriteLine($"PASS512 metadata policy controls, fresh first source->frame/cuts/context/NN->scope/cache {checkedGlyphs} printed glyph controls + duplicate chronology + callback cancellation. Class23f acoustic-only, not native word winner/RU cursive readiness.");
static InkStroke Stroke(params(float X,float Y)[] p)=>new(p.Select(v=>new InkPoint(v.X,v.Y)).ToArray());
static void ExpectInvalid(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidDataException or NotSupportedException){return;}throw new Exception("Invalid first cache accepted.");}
static byte[] Network(int width,ushort kind,int outputs=2)
{
    using var s=new MemoryStream();using var w=new BinaryWriter(s);w.Write((ushort)0xfea0);w.Write((ushort)0x106);w.Write(0u);w.Write((byte)0x12);w.Write(kind);w.Write(width);w.Write(2);w.Write(0u);w.Write(0u);w.Write(outputs+1);w.Write((ushort)10);
    w.Write(new byte[width]);w.Write(new byte[]{1,1});w.Write(new byte[outputs*2]);w.Write(Enumerable.Repeat((byte)1,outputs+1).ToArray());var bytes=s.ToArray();BinaryPrimitives.WriteUInt32LittleEndian(bytes.AsSpan(4),(uint)bytes.Length-8);return bytes;
}
static byte[] OwnSource(byte[] alphabet)
{
    using var s=new MemoryStream();using var w=new BinaryWriter(s);
    Header(w,0x247,0x10d);w.Write((ushort)0x201);w.Write((ushort)0x100);w.Write(new byte[4+56]);w.Write(0);w.Write(0);w.Write((byte)1);w.Write(0x2bu);w.Write((ushort)0x103);w.Write(alphabet);w.Write(0x135);w.Write(new byte[4]);w.Write(0f);
    foreach(float f in new[]{.96f,.85f,3f,10f})w.Write(f);w.Write(new byte[4]);w.Write(.25f);w.Write((byte)0);w.Write((byte)0);w.Write(0f);w.Write((byte)0);w.Write(0f);w.Write((byte)0);w.Write(2);
    int p=(int)s.Position;Header(w,0x24a,0x102);w.Write(0);w.Write(100f);w.Write((byte)1);w.Write(-.2f);w.Write(.2f);w.Write(new byte[]{4,6,0});Finish(s,p);
    p=(int)s.Position;Header(w,0x265,0x102);foreach(float f in new[]{.05f,.075f,.075f,.15f,.35f,.1f,.15f,.0001f})w.Write(f);Finish(s,p);
    w.Write((byte)1);p=(int)s.Position;Header(w,0x297,0x106);Finish(s,p);p=(int)s.Position;Header(w,0x21b,0x10a);Finish(s,p);
    p=(int)s.Position;Header(w,0x21d,0x110);w.Write((ushort)0xffff);w.Write((short)100);w.Write((byte)1);w.Write(Network(30,0x7001,28));w.Write(0f);w.Write(1f);w.Write(1f);w.Write((byte)0);w.Write((byte)3);w.Write((ushort)28);w.Write(.1f);w.Write((byte)1);w.Write((byte)2);w.Write((byte)0);w.Write((byte)1);w.Write((byte)0);w.Write((byte)1);w.Write((byte)0);
    w.Write(new byte[2+4+5*2+7*4]);w.Write(new byte[6]);w.Write(new byte[2+2+4+4+2+2]);w.Write(new byte[2]);
    for(int i=0;i<7;i++){w.Write((byte)0);w.Write((byte)(i<2?1:0));w.Write((byte)(i<2?1:0));w.Write((byte)(i<2?2:0));if(i>=2)w.Write(0f);w.Write((byte)0);w.Write((byte)(i<2?1:0));if(i<2)w.Write(Network(i==0?247:229,(ushort)(i==0?0x1f:0x2f)));w.Write((byte)0);}
    w.Write((byte)0);w.Write(0f);w.Write(new byte[9]);w.Write((ushort)0);w.Write(0f);w.Write(0f);w.Write(0f);w.Write(.9f);w.Write(.9f);w.Write(0f);w.Write((byte)0);w.Write(new byte[8]);w.Write((byte)0);w.Write((short)0);w.Write(85173);w.Write((ushort)1);w.Write((ushort)10);w.Write(new byte[13]);w.Write(0f);Finish(s,p);w.Write(new byte[190]);Finish(s,0);return s.ToArray();
}
static void Header(BinaryWriter w,uint kind,ushort version){w.Write(0x0101fe90u);w.Write(0u);w.Write(kind);w.Write((ushort)0);w.Write(version);}
static void Finish(MemoryStream s,int p){long save=s.Position;s.Position=p+4;using(var w=new BinaryWriter(s,System.Text.Encoding.UTF8,true))w.Write((uint)(save-p-8));s.Position=save;}
