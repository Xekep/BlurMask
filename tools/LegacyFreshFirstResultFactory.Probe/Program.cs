using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text.Json;
using InkRecognition;

if(args.Length!=3)throw new ArgumentException("Pass pinned AKresource, engine activations and independent controls.");
byte[] resource=File.ReadAllBytes(args[0]);
if(Convert.ToHexString(SHA256.HashData(resource)).ToLowerInvariant()!="7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978")throw new Exception("Pinned source required.");
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[2]));using var engine=JsonDocument.Parse(File.ReadAllBytes(args[1]));
var a=engine.RootElement.GetProperty("activations");var activations=new LegacyInkActivations(a.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),a.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var inputs=new[]{Stroke((0,90),(0,10),(25,60),(50,10),(50,90)),Stroke((65,90),(65,10)),Stroke((65,90),(115,10)),Stroke((115,90),(115,10)),Stroke((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))};
var factories=new List<LegacyFreshFirstResultOwnershipFactory>();int published=0;
foreach(var test in controls.RootElement.GetProperty("cases").EnumerateArray())
{
    byte[] snapshot=resource.ToArray();int offset=test.GetProperty("offset").GetInt32();if(offset>=0)snapshot[offset]=test.GetProperty("value").GetByte();
    var factory=LegacyFreshFirstResultOwnershipFactory.Prepare(snapshot,6120,inputs);factories.Add(factory);
    if(factory.SourceFingerprint!=test.GetProperty("fingerprint").GetString()||factory.ResourceB9Offset!=controls.RootElement.GetProperty("b9Offset").GetInt32()||
        factory.FinalizeWordGeometry!=test.GetProperty("wordGeometry").GetBoolean()||factory.SortWordsByGeometry!=factory.FinalizeWordGeometry||
        factory.CaptureGlyphGeometry||!factory.Configuration6C||factory.InvocationPolicy.Binding.OwnerFlagC4)
        throw new Exception($"Fresh flags differ: hash={factory.SourceFingerprint}; B9offset={factory.ResourceB9Offset}; geometry={factory.FinalizeWordGeometry};6C={factory.Configuration6C};C4={factory.InvocationPolicy.Binding.OwnerFlagC4}.");
    factory.ValidateInvocation(factory.AcousticModel,factory.Frame,factory.InvocationPolicy);
    if(factory.FinalizeWordGeometry){Reject(()=>factory.CreatePlainResult());continue;}
    for(int caseId=0;caseId<64;caseId++)
    {
        var result=factory.CreatePlainResult();var word=LegacyNativeWordBuilder.CreateEmpty(result.GlyphPool,new byte[120]);
        var refs=new List<ulong>();
        for(int j=0;j<4;j++)
        {
            ulong key=(uint)(100+caseId*4+j)|((ulong)(uint)(j+1)<<24);
            var inserted=word.AppendGlyph(key,j,false,new byte[40]);
            if(refs.Count!=0)word.LinkGlyph(refs[^1],inserted.Reference);refs.Add(inserted.Reference);
            result.GlyphPool.SetOrdinarySymbol(inserted.Reference,(ushort)(j+caseId));
            byte[] row=result.GlyphPool.CopyRecord(inserted.Reference);BinaryPrimitives.WriteUInt16LittleEndian(row.AsSpan(6),(ushort)(caseId<<4|j));
            result.GlyphPool.ReplaceRecord(inserted.Reference,row);
        }
        var node=result.WordPublisher.Publish(word,(n,arc)=>n-arc,(n,arc)=>n+arc);
        if(!ReferenceEquals(node.Word.Pool,result.GlyphPool)||!node.Word.FlatReferences.SequenceEqual(refs.AsEnumerable().Reverse())||
            (BinaryPrimitives.ReadUInt16LittleEndian(node.Word.CopyStorage().AsSpan(50))&0x400)!=0||!ReferenceEquals(result.Factory,factory))
            throw new Exception("Factory->actual pool/word/publisher ownership/plain flag composition differs.");
        var foreign=factory.CreatePlainResult();Reject(()=>foreign.WordPublisher.Publish(word,(n,arc)=>n,(n,arc)=>n));published++;
    }
}
var first=factories[0];var second=factories[1];
Reject(()=>first.ValidateInvocation(second.AcousticModel,first.Frame,first.InvocationPolicy));
Reject(()=>first.ValidateInvocation(first.AcousticModel,second.Frame,first.InvocationPolicy));
Reject(()=>first.ValidateInvocation(first.AcousticModel,first.Frame,second.InvocationPolicy));
var scores=first.AcousticModel.EvaluatePreparedInputs(new short[247],new short[229],activations);first.ValidateScores(scores);
Reject(()=>second.ValidateScores(scores));Reject(()=>factories[2].ValidateScores(scores));
using var cancel=new CancellationTokenSource();cancel.Cancel();
try{first.CreatePlainResult(cancel.Token);throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
var corrupted=resource.ToArray();corrupted[first.ResourceB9Offset]=2;Reject(()=>LegacyFreshFirstResultOwnershipFactory.Prepare(corrupted,6120,inputs));
//Two equal-resource raw invocations remain different owners, even though source fingerprints deliberately agree.
var same=LegacyFreshFirstResultOwnershipFactory.Prepare(resource,6120,inputs);
if(same.SourceFingerprint!=first.SourceFingerprint)throw new Exception("Equal source snapshot hash changed.");
Reject(()=>first.ValidateInvocation(first.AcousticModel,same.Frame,first.InvocationPolicy));
Console.WriteLine($"PASS {factories.Count} actual first-source flag/fingerprint variants, {published} factory->274/276/279/275 plain ownership compositions, actual fixed-input NN score fingerprint guards, same-resource foreign invocation guards and trueB9 refusal. Source callbacks in word fixtures remain prepared; no OCR winner/main247 frame claim.");
static InkStroke Stroke(params(float X,float Y)[] points)=>new(points.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
static void Reject(Action action){try{action();}catch(ArgumentException){return;}catch(InvalidDataException){return;}catch(NotSupportedException){return;}throw new Exception("Foreign/out-of-scope factory accepted.");}
