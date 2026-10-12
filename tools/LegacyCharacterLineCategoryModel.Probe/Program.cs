using System.Text;
using InkRecognition;

if(args.Length!=1)throw new ArgumentException("Pass committed portable character-line model JSON.");
var data=File.ReadAllBytes(args[0]);
using var stream=new MemoryStream(data);
var model=LegacyCharacterLineCategoryModel.Load(stream);
if(!stream.CanRead||model.EntryCount!=3923||model.ProfileCount!=45||model.CyrillicEntryCount!=122)
    throw new Exception("Portable catalog metadata/stream ownership changed.");
var alphabet=model.Catalog.BindAlphabet(["a","A","p","\u0430","\u0410","\u0440","\u0451","\u0439","missing-symbol"]);
(sbyte,sbyte)[] categories=[(0,-1),(1,-1),(2,-1),(0,-1),(1,-1),(2,-1),(4,0),(4,0),(-1,-1)];
for(ushort i=0;i<categories.Length;i++)if(alphabet.ReadCategories(i)!=categories[i])throw new Exception("Portable observed category mismatch.");
var config=new LegacyCharacterLineFitProfile(2,20,-.4f,.4f,1e-6f,float.MaxValue,3);
int composed=0;
// Prepared glyph/source geometry is explicit, independent of labels and of OCR expected outputs.
for(int n=0;n<256;n++)
{
    float bodyHeight=4+n*.5f,baseY=30+n*.25f,half=bodyHeight*.5f;
    var p=alphabet.CreateItemProducer();
    var items=new[]{p.PrepareNextItem(0,0,10,baseY-bodyHeight,baseY),p.PrepareNextItem(1,20,30,baseY-bodyHeight*2,baseY),
        p.PrepareNextItem(2,40,50,baseY-bodyHeight,baseY+bodyHeight),p.PrepareNextItem(3,60,70,baseY-bodyHeight,baseY),
        p.PrepareNextItem(4,80,90,baseY-bodyHeight*2,baseY),p.PrepareNextItem(5,100,110,baseY-bodyHeight,baseY+bodyHeight)};
    var result=LegacyPreparedCharacterLineFit.Fit(items,config,true)!.Value.ToPreparedFitOutput();
    Equal(result.BaselineY,baseY);Equal(result.XHeight,bodyHeight);
    Equal(LegacyHandwritingFontSizePolicy.MatchPrepared(16,8,[new(0x430,1,result.XHeight,60,70)]),bodyHeight*2);
    composed++;
}
var text=Encoding.UTF8.GetString(data);
Reject(text.Replace("\"version\":1", "\"version\":2",StringComparison.Ordinal));
Reject(text.Replace(LegacyCharacterLineCategoryModel.ExpectedSourceSha256,new string('0',64),StringComparison.Ordinal));
Reject(text.Replace(LegacyCharacterLineCategoryModel.ExpectedTableSha256,new string('0',64),StringComparison.Ordinal));
Reject(text.Replace("[\"a\",0,-1]","[\"a\",1,-1]",StringComparison.Ordinal));
Reject(text.Replace("[\"a\",0,-1]","[\"a\",-2,-1]",StringComparison.Ordinal));
Reject(text.Replace("[\"a\",0,-1]","[\"A\",0,-1]",StringComparison.Ordinal));
Reject(text.Replace("\"profiles\":[[0.0,1.0,0.0]","\"profiles\":[[0.0,1.0,-0.0]",StringComparison.Ordinal));
Reject("{\"format\":\"bad\",\"format\":\"blurmask-character-line-categories\"}");
Reject("{}");Reject("[]");Reject("{bad-json}");
using var huge=new MemoryStream(new byte[1024*1024+1]);
try{LegacyCharacterLineCategoryModel.Load(huge);throw new Exception("Model budget ignored.");}catch(InvalidDataException){}
using var cancelled=new CancellationTokenSource();cancelled.Cancel();
try{LegacyCharacterLineCategoryModel.Load(new MemoryStream(data),cancelled.Token);throw new Exception("Cancelled model load accepted.");}catch(OperationCanceledException){}
using var throttled=new ChunkedStream(data);
if(LegacyCharacterLineCategoryModel.Load(throttled).TableSha256!=model.TableSha256)throw new Exception("Short-read model load changed.");
Console.WriteLine($"PASS portable model3923entries/45profiles/122Cyrillic, {composed} catalog->item->IRLS->font cases; source/table pins, corruption, duplicates, bounded/short reads and cancellation. No vendor PE required.");

static void Equal(double a,double b){if(Math.Abs(a-b)>1e-5*Math.Max(1,Math.Abs(b)))throw new Exception($"Composed mismatch {a:R}/{b:R}");}
static void Reject(string text)
{
    using var s=new MemoryStream(Encoding.UTF8.GetBytes(text));
    try{LegacyCharacterLineCategoryModel.Load(s);throw new Exception("Corrupted portable model accepted (or test mutation ineffective).");}
    catch(InvalidDataException){}
}
sealed class ChunkedStream(byte[] bytes):MemoryStream(bytes)
{
    public override int Read(Span<byte> buffer)=>base.Read(buffer[..Math.Min(buffer.Length,13)]);
}
