using System.Text.Json;
using InkRecognition;

if (args.Length is not (1 or 2)) throw new ArgumentException("Pass own control JSON; optional pinned PE catalog audit JSON.");
using var controls = JsonDocument.Parse(File.ReadAllBytes(args[0]));
int cases = 0, candidates = 0;
var config = new LegacyCharacterLineFitProfile(2,20,-.4f,.4f,1e-6f,200,3);
foreach (var c in controls.RootElement.GetProperty("cases").EnumerateArray())
{
    var profiles = Profiles(c.GetProperty("profiles"));
    var entries = c.GetProperty("entries").EnumerateArray().Select(e =>
        new LegacyCharacterLineCategoryEntry(e[0].GetString()!, (sbyte)e[1].GetInt32(), (sbyte)e[2].GetInt32())).ToArray();
    var alphabet = c.GetProperty("alphabet").EnumerateArray().Select(x => x.GetString()!).ToArray();
    var catalog = new LegacyPreparedCharacterLineCategoryCatalog(entries, profiles);
    var bound = catalog.BindAlphabet(alphabet);
    var producer = bound.CreateItemProducer();
    var items = new List<LegacyPreparedCharacterLineItem>();
    foreach (var input in c.GetProperty("inputs").EnumerateArray())
    {
        var b = input.GetProperty("bounds").EnumerateArray().Select(x=>x.GetSingle()).ToArray();
        var item = producer.PrepareNextItem(input.GetProperty("label").GetUInt16(), b[0],b[1],b[2],b[3]);
        var e = input.GetProperty("candidate").EnumerateArray().Select(x=>x.GetSingle()).ToArray();
        var expected = new LegacyCharacterLineCandidate(e[0],e[1],(int)e[2],e[3],e[4],e[5],e[6]);
        if (item.CandidateCount != 1 || item.KnownGlyph || item.ClipVerticalToWidth || item.DownweightMarker || item.CopyPreparedSlots()[0] != expected)
            throw new Exception("Native single-candidate item preparation mismatch.");
        items.Add(item); candidates++;
    }
    var result = LegacyPreparedCharacterLineFit.Fit(items.ToArray(), config, true, 1e100)!.Value;
    var expectedFit = c.GetProperty("expected");
    var sv = expectedFit.GetProperty("solution").EnumerateArray().Select(x=>x.GetDouble()).ToArray();
    Equal(result.Solution.HeightScale,sv[0]); Equal(result.Solution.Slope,sv[1]); Equal(result.Solution.Intercept,sv[2]);
    Equal(result.RawProfileAffinity,expectedFit.GetProperty("score").GetDouble());
    if (result.Iterations != expectedFit.GetProperty("iterations").GetInt32()) throw new Exception("Composed IRLS iterations mismatch.");
    cases++;
}
var own = new LegacyPreparedCharacterLineCategoryCatalog(
    [new("body",0,-1),new("upper",1,-1),new("lower",2,-1)], [new(0,1),new(0,2),new(1,2)]);
var ownAlphabet = own.BindAlphabet(["body","upper","lower","BODY"]);
var ownProducer = ownAlphabet.CreateItemProducer();
var body=ownProducer.PrepareNextItem(0,0,10,50,90);
var upper=ownProducer.PrepareNextItem(1,20,30,10,90);
var lower=ownProducer.PrepareNextItem(2,40,50,50,130);
var prepared=LegacyPreparedCharacterLineFit.Fit([body,upper,lower],config,true)!.Value.ToPreparedFitOutput();
Equal(prepared.BaselineY,90);Equal(prepared.XHeight,40);
Equal(LegacyHandwritingFontSizePolicy.MatchPrepared(16,8,
    [new('a',1,prepared.XHeight,0,10),new('A',1,prepared.XHeight,20,30),new('p',1,prepared.XHeight,40,50)]),80);
var missing=ownProducer.PrepareNextItem(3,0,10,0,20).CopyPreparedSlots()[0];
if (missing.Kind != 0 || missing.MainTop != 1 || missing.MainBottom != -3) throw new Exception("Case-folded category invention or native stale-profile lifecycle lost.");
var freshMissing=ownAlphabet.CreateItemProducer().PrepareNextItem(ushort.MaxValue,0,10,0,20).CopyPreparedSlots()[0];
if(freshMissing.MainTop!=0||freshMissing.MainBottom!=0)throw new Exception("Fresh template not zeroed.");
Reject(()=>ownProducer.PrepareNextItem(4,0,1,0,1));
Reject(()=>new LegacyPreparedCharacterLineCategoryCatalog([new("bad",3,-1)], [new(0,1)]));
Reject(()=>new LegacyPreparedCharacterLineCategoryCatalog([new("bad",-2,-1)], [new(0,1)]));
Reject(()=>new LegacyPreparedCharacterLineCategoryCatalog([], [new(float.NaN,1)]));
Reject(()=>new LegacyPreparedCharacterLineCategoryCatalog([new("\ud800",0,-1)], [new(0,1)]));
var altCatalog=new LegacyPreparedCharacterLineCategoryCatalog([new("both",0,1),new("main",2,-1)], [new(0,1),new(0,2),new(1,2)]);
var altProducer=altCatalog.BindAlphabet(["both","main"]).CreateItemProducer();
_ = altProducer.PrepareNextItem(0,0,10,0,20);
var afterKnown=altProducer.PrepareNextItem(1,0,10,0,20).CopyPreparedSlots()[0];
if(afterKnown.Kind!=1||afterKnown.AlternateTop!=3||afterKnown.AlternateBottom!=-1)throw new Exception("Native retained alternate profile lost.");
Reject(()=>altProducer.PrepareNextItem(0,20,10,0,20));
var afterInvalid=altProducer.PrepareNextItem(ushort.MaxValue,0,10,0,20).CopyPreparedSlots()[0];
if(afterInvalid.MainTop!=1||afterInvalid.MainBottom!=-3)throw new Exception("Invalid portable input mutated prepared template.");
using var cancel = new CancellationTokenSource(); cancel.Cancel();
try{own.BindAlphabet(["body"],cancel.Token);throw new Exception("Cancellation ignored.");}catch(OperationCanceledException){}
Console.WriteLine($"PASS {cases} independent category/item/fit cases, {candidates} candidates; mixed body/upper/descender x-height40, exact case matching, guards, cancellation.");

if (args.Length == 2)
{
    using var actual = JsonDocument.Parse(File.ReadAllBytes(args[1]));
    if(actual.RootElement.GetProperty("source_sha256").GetString()!="17f5236d257fe3175574ec29acf5116e375b247e02d2bc7c190ee908bd185a2f")
        throw new Exception("Observed catalog source pin mismatch.");
    var entries=actual.RootElement.GetProperty("rows").EnumerateArray().Select(e=>new LegacyCharacterLineCategoryEntry(
        e.GetProperty("text").GetString()!,(sbyte)e.GetProperty("main").GetInt32(),(sbyte)e.GetProperty("alternate").GetInt32())).ToArray();
    var profiles=Profiles(actual.RootElement.GetProperty("profiles"));
    if (entries.Length!=3923 || profiles.Length!=45) throw new Exception("Observed catalog cardinality mismatch.");
    var catalog=new LegacyPreparedCharacterLineCategoryCatalog(entries,profiles);
    // These are explicit actual symbol/profile diagnostics, never OCR accuracy claims.
    var alphabet=catalog.BindAlphabet(["a","A","p","\u0430","\u0410","\u0440","\u0451","\u0439","not-a-native-symbol"]);
    (sbyte,sbyte)[] expected=[(0,-1),(1,-1),(2,-1),(0,-1),(1,-1),(2,-1),(4,0),(4,0),(-1,-1)];
    for(ushort i=0;i<expected.Length;i++) if(alphabet.ReadCategories(i)!=expected[i])throw new Exception("Observed catalog mapping mismatch.");
    var nativeProducer=alphabet.CreateItemProducer();
    var items=new[]{nativeProducer.PrepareNextItem(0,0,10,50,90),nativeProducer.PrepareNextItem(1,20,30,10,90),nativeProducer.PrepareNextItem(2,40,50,50,130),
        nativeProducer.PrepareNextItem(3,60,70,50,90),nativeProducer.PrepareNextItem(4,80,90,10,90),nativeProducer.PrepareNextItem(5,100,110,50,130)};
    var fit=LegacyPreparedCharacterLineFit.Fit(items,config,true)!.Value.ToPreparedFitOutput();
    Equal(fit.BaselineY,90);Equal(fit.XHeight,40);
    Equal(LegacyHandwritingFontSizePolicy.MatchPrepared(16,8,[new(0x430,1,fit.XHeight,60,70)]),80);
    Console.WriteLine("PASS actual3923-symbol/45-profile catalog diagnostics and mixed Latin/Russian prepared geometry -> line baseline90/x-height40. No OCR execution or accuracy claim.");
}

static LegacyCharacterVerticalCategory[] Profiles(JsonElement e)=>e.EnumerateArray().Select(p=>new LegacyCharacterVerticalCategory(p[0].GetSingle(),p[1].GetSingle())).ToArray();
static void Equal(double a,double e){if(!double.IsFinite(a)||Math.Abs(a-e)>2e-8*Math.Max(1,Math.Abs(e)))throw new Exception($"Numeric mismatch {a:R}/{e:R}");}
static void Reject(Action a){try{a();throw new Exception("Invalid prepared input accepted.");}catch(ArgumentException){}catch(InvalidDataException){}}
