using System.Text.Json;
using InkRecognition;

if(args.Length!=2)throw new ArgumentException("Pass own result-line controls JSON and committed portable category model.");
using var doc=JsonDocument.Parse(File.ReadAllBytes(args[0]));
int tested=0;
foreach(var c in doc.RootElement.GetProperty("cases").EnumerateArray())
{
    var ids=c.GetProperty("ids").EnumerateArray().Select(x=>x.GetInt32()).ToArray();
    var heights=c.GetProperty("heights").EnumerateArray().Select(x=>x.GetDouble()).ToArray();
    var result=LegacyPreparedResultLineMetrics.Create(heights,ids);
    if(!result.CopyDerivedCharacterLineIds().SequenceEqual(c.GetProperty("expectedIds").EnumerateArray().Select(x=>x.GetInt32())))
        throw new Exception("Native one-neighbor ID repair mismatch.");
    var expected=c.GetProperty("ranges").EnumerateArray().Select(e=>new LegacyPreparedResultLineRange(e[0].GetInt32(),e[1].GetInt32(),e[2].GetInt32())).ToArray();
    if(!result.CopyLineRanges().SequenceEqual(expected))throw new Exception("Prepared owner/range binding mismatch.");
    for(int i=0;i<result.LineCount;i++)if(result.ReadSourceLineXHeight(i)!=(float)heights[i])throw new Exception("SDK line height narrowing mismatch.");
    tested++;
}
using var stream=File.OpenRead(args[1]);var model=LegacyCharacterLineCategoryModel.Load(stream);
var alphabet=model.Catalog.BindAlphabet(["a","A","p","\u0430","\u0410","\u0440"]);
var profile=new LegacyCharacterLineFitProfile(2,20,-.4f,.4f,1e-6f,float.MaxValue,3);
for(int k=0;k<256;k++)
{
    float h=4+k*.5f,b=40+k*.25f;var p=alphabet.CreateItemProducer();
    var items=new[]{p.PrepareNextItem(0,0,10,b-h,b),p.PrepareNextItem(1,20,30,b-h*2,b),p.PrepareNextItem(2,40,50,b-h,b+h),
        p.PrepareNextItem(3,60,70,b-h,b),p.PrepareNextItem(4,80,90,b-h*2,b),p.PrepareNextItem(5,100,110,b-h,b+h)};
    var fit=LegacyPreparedCharacterLineFit.Fit(items,profile,true)!.Value.ToPreparedFitOutput();
    // Actual C1A float -> Document source-line double -> 2DB owner reference -> SDK float -> font policy.
    var result=LegacyPreparedResultLineMetrics.Create([(double)fit.XHeight],[0,0,0,0,0,0]);
    var sample=result.CreateFontSizeSample(0,0x430,1,60,70);
    if(sample.SourceLineXHeight!=h||LegacyHandwritingFontSizePolicy.MatchPrepared(16,8,[sample])!=h*2)
        throw new Exception("Portable category/fit/source-owner/SDK/font composition mismatch.");
}
var originalHeights=new[]{40d,80d};var originalIds=new[]{0,1};var owned=LegacyPreparedResultLineMetrics.Create(originalHeights,originalIds);
originalHeights[0]=999;originalIds[0]=99;var copied=owned.CopyLineRanges();copied[0]=default;
if(owned.ReadSourceLineXHeight(0)!=40||owned.CopyDerivedCharacterLineIds()[0]!=0)throw new Exception("Owner aliases caller/copy storage.");
var nonmonotone=LegacyPreparedResultLineMetrics.Create([1d,2d,3d,4d,5d,6d],[5,4,3]);
if(!nonmonotone.CopyDerivedCharacterLineIds().SequenceEqual([4,3,3])||nonmonotone.CopyLineRanges()[4]!=new LegacyPreparedResultLineRange(4,0,1))
    throw new Exception("Native one-step repair replaced by global sorting/monotonic repair.");
Reject(()=>LegacyPreparedResultLineMetrics.Create([double.NaN],[0]));
Reject(()=>LegacyPreparedResultLineMetrics.Create([1d],[-1]));
Reject(()=>LegacyPreparedResultLineMetrics.Create(new double[4097],[0]));
Reject(()=>LegacyPreparedResultLineMetrics.Create([1d],new int[65537]));
Reject(()=>owned.ReadSourceLineXHeight(-1));
Reject(()=>LegacyPreparedResultLineMetrics.Create([double.MaxValue],[0]).ReadSourceLineXHeight(0));
Reject(()=>LegacyPreparedResultLineMetrics.Create([1d],[]).ReadSourceLineXHeight(0));
using var cancelled=new CancellationTokenSource();cancelled.Cancel();
try{LegacyPreparedResultLineMetrics.Create([1d],[0],cancelled.Token);throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
Console.WriteLine($"PASS {tested} exhaustive prepared range/owner cases +256 portable category->IRLS->source-line->result owner->SDK->font cases; exact one-step repair, source ownership, guards, cancellation. No raw OCR claim.");

static void Reject(Action a){try{a();throw new Exception("Invalid prepared owner accepted.");}catch(ArgumentException){}catch(InvalidDataException){}}
