using InkRecognition;
using System.Text.Json;
using System.Text.Json.Serialization;

// SOURCE skeleton only. No authorization to build/run/NN. The library factory
// and same-owner gate remain unconditional refusals independently of this CLI.
if(args.Length!=3||args[0]!="--future-authorized")
    throw new NotSupportedException("SOURCE skeleton only; runtime authorization is separate and no NN may run today.");
var root=args[1];var capturePath=args[2];byte[] Read(string path)=>File.ReadAllBytes(Path.Combine(root,path));
byte[] ak=Read("ru-RU/ru_RU-ak-cur.lite.res"),lk=Read("ru-RU/ru_RU-lk-text.lite.res");
using var engine=JsonDocument.Parse(Read("ru-RU/legacy-engine-profile.json"));var ac=engine.RootElement.GetProperty("activations");
var activations=new LegacyInkActivations(ac.GetProperty("hidden").EnumerateArray().Select(v=>v.GetInt32()).ToArray(),ac.GetProperty("output").EnumerateArray().Select(v=>v.GetInt32()).ToArray());
var table=LegacyCursiveSeparationTable.Load(Read("ru-RU/legacy-separation-profile.json"));
using var cf=File.OpenRead(Path.Combine(root,"text-unicode/legacy-case-data.json"));using var pf=File.OpenRead(Path.Combine(root,"text-unicode/legacy-text-publication-data.json"));
var cd=LegacyNativeCaseData.Load(cf);var pd=LegacyNativeTextPublicationData.Load(pf);var line30=Read("line-geometry/line-2e3-30.res");var line50=Read("line-geometry/line-2e3-50.res");
var linguistic=LegacyLinguisticRoot13FWireLayout.Read(lk);var wire=LegacyFst23CWireLayout.ReadActualRussianNumber(linguistic);
var outgoing=LegacyFst13AOutgoingSource.CreateActualRussianNumber(linguistic,wire,ak);
var primary=LegacyFst28CPrimaryAlphabetMap.CreateActualRussianNumber(outgoing,ak);
var policy=LegacyFst28CTargetPreparation.CreateActualRussianNumberFresh(primary,ak);var initial=LegacyFst28CInitialState.CreateActualRussianNumberFresh(policy);
LegacyFreshMainCompactOrdinaryGraph Graph(InkStroke[] strokes)
{
 var input=LegacyFreshMainOwnedTextInputPreparation.Prepare(ak,line30,line50,strokes,0,activations,table,cd,pd).Run();
 var cuts=LegacyFreshMainDescriptorCutSource.Prepare(ak,input.Smoothed);var local=LegacyFreshMainDescriptorGraphAppend.Prepare(ak,cuts,table);
 return LegacyFreshMainCompactOrdinaryGraph.Prepare(ak,LegacyFreshMainGlobalOrdinaryState.Prepare(ak,local,table));
}
var patterns=new InkStroke[][]{
 [new(new InkPoint[]{new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)})],
 [new(new InkPoint[]{new(0,10),new(30,90)}),new(new InkPoint[]{new(40,90),new(70,10)})],
 [new(new InkPoint[]{new(0,10),new(40,90),new(80,10)}),new(new InkPoint[]{new(0,90),new(40,10),new(80,90)})],
 [new(new InkPoint[]{new(0,90),new(0,10)}),new(new InkPoint[]{new(30,90),new(30,10)}),new(new InkPoint[]{new(0,230),new(0,150)})]};
var graphs=patterns.Select(Graph).ToArray();
var boots=graphs.Select(g=>LegacyFst28CFreshPrimaryControllerBootstrap.CreateDirectSelectedNumberFresh(initial,g,ak)).ToArray();
LegacyFst28CNumberSymbolMasks NewMasks(int i=0)=>LegacyFst28CNumberSymbolMasks.CreateActualDirectNumberFresh(boots[i],ak);
var masks=Enumerable.Range(0,4).Select(i=>NewMasks(i)).ToArray();
var owners=masks.Select(v=>LegacyFst28CNumberInitialD0.CreateActualDirectNumberFresh(v,ak)).ToArray();

int ContextCount(LegacyFreshMainCompactOrdinaryGraph graph)
{
 int total=0;foreach(var s in graph.Source.Source.Steps)
 {if(s.IsPendingSeparator)continue;int k=s.LastPhysical-s.FirstPhysical;total+=k==0?0:k==1?1:3*k-3;}
 return total;
}
int glyph=owners.Sum(x=>x.AcousticEvaluations)*4,contextNN=graphs.Sum(ContextCount);
if(glyph!=16||contextNN!=3)throw new InvalidDataException("Canonical old fixture NN topology must remain16+3.");
var results=new List<Case>();
for(int i=0;i<4;i++)
{
    // Frozen independent raw graph observation860cb7 gives TRUE only1/3.
    bool expectedTrue=i is 1 or 3;
    try
    {
        var owner=LegacyFst28CNumberTrueLanguageSourceDraft.CreateFreshForOwnedProbe(owners[i],ak);
        if(!expectedTrue)throw new InvalidDataException("False graph branch must refuse before prefix claim.");
        var d=owner.CopyOwnedDiagnostic();
        if(d.SourceEnd is <125 or >156||d.RetainedEnd<125||d.RetainedEnd>d.SourceEnd||
            d.LanguageHeapCount is <0 or >124||d.PhysicalStoredRows!=157||d.PrimaryRows!=125||
            d.HasQueryAuthority||d.HasParentHistoryAuthority||d.HasAcceptedText)
            throw new InvalidDataException("Real private TRUE diagnostic bounds/authority differ.");
        results.Add(new(i,true,string.Empty,d));
    }
    catch(NotSupportedException e)when(!expectedTrue&&e.Message=="Actual graph selects skipped-language branch; no TRUE prefix claimed.")
    {results.Add(new(i,false,e.Message,null));}
}
// This is NOT a complete execution oracle/capture. Full stored/mask/context/
// source-cache snapshots and strict tamper controls must precede any runtime
// authorization; no positive controls or byte results are fabricated here.
var capture=new Capture("SOURCE owning-factory skeleton; full numeric oracle still required",false,
    glyph,contextNN,glyph+contextNN,results.ToArray());
File.WriteAllText(capturePath,JsonSerializer.Serialize(capture,ProbeJson.Default.Capture));
internal sealed record Case(int Index,bool TrueBranch,string Refusal,LegacyFst28CNumberTrueSourceDiagnostic? Diagnostic);
internal sealed record Capture(string Scope,bool FullOracleComplete,int OldGlyphNN,int OldContextNN,int TotalOldNN,Case[] Cases);
[JsonSerializable(typeof(Capture))]internal partial class ProbeJson:JsonSerializerContext{}
