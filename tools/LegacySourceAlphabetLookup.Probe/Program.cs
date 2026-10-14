using System.Text;
using System.Text.Json;
using System.Buffers.Binary;
using System.Security.Cryptography;
using InkRecognition;
if(args.Length!=2)throw new ArgumentException("Pass own table controls and actual RU resource.");
using var doc=JsonDocument.Parse(File.ReadAllBytes(args[0]));byte[] payload=Convert.FromBase64String(doc.RootElement.GetProperty("payload").GetString()!);
var own=LegacySourceAlphabetByteLookup.ReadPayload(payload,0x103);int ownCount=0;
foreach(var q in doc.RootElement.GetProperty("queries").EnumerateArray())
{if(own.LookupUtf8(Convert.FromBase64String(q.GetProperty("bytes").GetString()!))!=q.GetProperty("label").GetUInt16())throw new Exception("Own binary lookup differs.");ownCount++;}
var byteOracle=doc.RootElement.GetProperty("queries").EnumerateArray().Where(q=>q.GetProperty("label").GetUInt16()!=65535).ToDictionary(q=>q.GetProperty("bytes").GetString()!,q=>q.GetProperty("label").GetUInt16(),StringComparer.Ordinal);
int exhaustive=0;
for(int length=1;length<=2;length++)for(int code=0;code<(length==1?256:65536);code++)
{byte[] input=length==1?[(byte)code]:[(byte)(code>>8),(byte)code];ushort expected=byteOracle.GetValueOrDefault(Convert.ToBase64String(input),ushort.MaxValue);if(own.LookupUtf8(input)!=expected)throw new Exception("Exhaustive byte lookup differs.");exhaustive++;}
byte[] resource=File.ReadAllBytes(args[1]);if(Convert.ToHexString(SHA256.HashData(resource)).ToLowerInvariant()!="7a5f08809e81fb4a2d4c78b3efd251d0986503b0d30d21de3695dab1f8a89978")throw new Exception("Pinned RU source required.");
var actual=LegacySourceAlphabetByteLookup.ReadInlineV10dSource(resource,0);var alphabet=LegacyInkAlphabet.Load(resource.AsSpan(99),0x103);int labels=0;
for(ushort i=0;i<alphabet.LabelCount;i++)if(alphabet.TryGetText(i,out var text))
{if(actual.LookupUtf8(Encoding.UTF8.GetBytes(text))!=i)throw new Exception($"Actual native lookup differs{i}.");labels++;}
var marker=actual.ResolveLineMarkers();if(marker!=new LegacyFirstResultLineMarkers(187,188,185,186,65535,65535,189))throw new Exception("Actual markers differ.");
if(actual.SourceNodeOffset!=0||actual.AlphabetPayloadOffset!=99||actual.LabelCount!=alphabet.LabelCount)throw new Exception("Inline source owner offsets differ.");
payload.AsSpan().Fill(255);if(own.LookupUtf8("alpha"u8)!=2)throw new Exception("Native table ownership leaked.");
var child=LegacySourceAlphabetByteLookup.ReadInlineV10dSource(resource,6120);if(child.SourceNodeOffset!=6120||child.AlphabetPayloadOffset!=6219)throw new Exception("Child owned inline source offsets differ.");
var mainCategory=LegacySourceSamplerCategoryOwner.ReadV10dSource(resource,0);var firstCategory=LegacySourceSamplerCategoryOwner.ReadFirstChildOf249(resource,6104);
if(mainCategory.SamplerNodeOffset!=188157||mainCategory.CategoryProfileOffset!=188222||firstCategory.SelectedSourceOffset!=6120||firstCategory.SamplerNodeOffset!=11506||firstCategory.CategoryProfileOffset!=11571||
    firstCategory.OwningProcessor249Offset!=6104||mainCategory.OwningProcessor249Offset is not null||mainCategory.Serialized249SourceScoreExponent is not null||firstCategory.Serialized249SourceScoreExponent!=.75f)
    throw new Exception("Distinct actual main/child category owner or serialized exponent binding differs.");
var corrupt=resource.ToArray();corrupt[92]=0;Reject(()=>LegacySourceAlphabetByteLookup.ReadInlineV10dSource(corrupt,0));
var sourceStrokes=new[]{new InkStroke([new InkPoint(1,1),new InkPoint(1,1),new InkPoint(20,20),new InkPoint(30,90),new InkPoint(50,120)])};
int branches=0;
foreach(float resolution in new float[]{0,96,300})
{
    var controller=LegacyMainFirstResultLineController.Prepare(resource,0,sourceStrokes,resolution);
    if(controller.Processor24BOffset!=187937||controller.FirstSourceOffset!=6120||controller.PhysicalResolution!=resolution||controller.Markers!=marker)throw new Exception("Owned main24B/AL/resolution binding differs.");
    if(controller.Source.Strokes[0].Points.Count!=4)throw new Exception("Main source skipped actual ingress duplicate filter.");
    ushort label=controller.Alphabet.LookupUtf8("м"u8);var alt=new byte[40];BinaryPrimitives.WriteUInt16LittleEndian(alt.AsSpan(8),label);BinaryPrimitives.WriteSingleLittleEndian(alt.AsSpan(12),.75f);
    var row=controller.CreateFirstResultRow(alt,0,[new(new(0,0,1),new(0,0,2))],[0]);
    if(controller.FirstCategoryOwner.SourceFingerprint!=firstCategory.SourceFingerprint||controller.SourceScoreExponent!=.75f||Math.Abs(BinaryPrimitives.ReadSingleLittleEndian(row.Record.CopyStorage().Span[80..])-(float)Math.Pow(.75,.75))>1e-7)throw new Exception("Loaded cloned249 source exponent or category owner differs.");
    byte[] descriptor=Enumerable.Repeat((byte)0x5a,88).ToArray();BinaryPrimitives.WriteUInt16LittleEndian(descriptor,0);BinaryPrimitives.WriteInt32LittleEndian(descriptor.AsSpan(16),-1);
    BinaryPrimitives.WriteInt32LittleEndian(descriptor.AsSpan(36),0);BinaryPrimitives.WriteInt32LittleEndian(descriptor.AsSpan(40),0);BinaryPrimitives.WriteInt32LittleEndian(descriptor.AsSpan(52),0);BinaryPrimitives.WriteInt32LittleEndian(descriptor.AsSpan(56),1);
    var template=Enumerable.Repeat((byte)0x6b,40).ToArray();BinaryPrimitives.WriteSingleLittleEndian(template.AsSpan(20),0);
    var output=controller.ProcessDescriptor(descriptor,[row],[],template);
    if(!output.AddedLine||output.NewLineFit is null||output.Deskew is null||BinaryPrimitives.ReadInt32LittleEndian(output.CopyDescriptor().Span[16..])!=0||output.CopyGuides().Length!=1)throw new Exception("Fresh unknown line failed actual fit/append/deskew.");
    byte[] copy=output.CopyDescriptor().ToArray();copy.AsSpan(16,4).CopyTo(descriptor.AsSpan(16));if(!copy.AsSpan().SequenceEqual(descriptor))throw new Exception("Descriptor changed unrelated bytes.");
    var guide=output.CopyGuides()[0];if(BinaryPrimitives.ReadInt32LittleEndian(guide.AsSpan(32))!=-1||BinaryPrimitives.ReadInt32LittleEndian(guide.AsSpan(36))!=int.MinValue||guide[28]!=0||!guide.AsSpan(29,3).SequenceEqual(template.AsSpan(29,3)))throw new Exception("Native metadata initialization/opaque tail changed.");
    var existing=guide.ToArray();BinaryPrimitives.WriteSingleLittleEndian(existing,200);BinaryPrimitives.WriteSingleLittleEndian(existing.AsSpan(4),40);BinaryPrimitives.WriteSingleLittleEndian(existing.AsSpan(8),.5f);
    var assigned=controller.ProcessDescriptor(descriptor,[row],[existing],template);
    if(assigned.AddedLine||assigned.NewLineFit is not null||assigned.Deskew is null)throw new Exception("Fresh assigned line incorrectly refit720 withC1A.");
    if(!assigned.CopyGuides()[0].AsSpan(8).SequenceEqual(existing.AsSpan(8)))throw new Exception("Assigned line changed pre-deskew metadata.");
    BinaryPrimitives.WriteInt32LittleEndian(descriptor.AsSpan(16),-1);BinaryPrimitives.WriteInt32LittleEndian(descriptor.AsSpan(56),0);
    var skipped=controller.ProcessDescriptor(descriptor,[],[],template);if(skipped.AddedLine||skipped.Deskew is not null||!skipped.CopyDescriptor().Span.SequenceEqual(descriptor))throw new Exception("Unknown empty interval should be untouched.");
    BinaryPrimitives.WriteInt32LittleEndian(descriptor.AsSpan(56),1);
    var foreign=LegacyMainFirstResultLineController.Prepare(resource,0,sourceStrokes,resolution);Reject(()=>foreign.ProcessDescriptor(descriptor,[row],[],template));
    using var cancel=new CancellationTokenSource();cancel.Cancel();Reject(()=>controller.ProcessDescriptor(descriptor,[row],[],template,cancel.Token));branches+=3;
}
Reject(()=>LegacyMainFirstResultLineController.Prepare(resource,0,sourceStrokes,float.NaN));
var exponentVariant=resource.ToArray();BinaryPrimitives.WriteSingleLittleEndian(exponentVariant.AsSpan(187933),2.5f);
var variantOwner=LegacySourceSamplerCategoryOwner.ReadFirstChildOf249(exponentVariant,6104);var variantController=LegacyMainFirstResultLineController.Prepare(exponentVariant,0,sourceStrokes,96);
if(variantOwner.Serialized249SourceScoreExponent!=2.5f||variantOwner.SourceFingerprint!=firstCategory.SourceFingerprint||variantController.SourceScoreExponent!=2.5f)throw new Exception("Serialized reader exponent not forwarded by loaded clone.");
var absent=resource.ToArray();absent[firstCategory.CategoryProfileOffset-1]=0;Reject(()=>LegacySourceSamplerCategoryOwner.ReadV10dSource(absent,6120));
Console.WriteLine($"PASS {ownCount} original byte lookup controls/{exhaustive} exhaustive1/2-byte probes, {labels} actual RU stored labels, resource-owned24B markers{marker}, {branches} owned AL+ingress+resolution→unknown-fit/assigned-no-refit/empty main24B branches and foreign-owner guards; distinct first/main categories and loaded.75→clone exponent variants. Actual glyph labels/selections/descriptors/upstream guide remain prepared; no mutable2A symbol insertion or full main247 claim.");
static void Reject(Action action){try{action();}catch(Exception e)when(e is InvalidDataException or NotSupportedException or ArgumentException or OperationCanceledException){return;}throw new Exception("Foreign/missing inline parent alphabet accepted.");}
