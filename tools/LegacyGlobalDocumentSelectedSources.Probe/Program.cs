using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;
if(args.Length!=3)throw new ArgumentException("MODELS RECORDED-MANIFEST CAPTURE");
string m=args[0];byte[] R(string n)=>File.ReadAllBytes(Path.Combine(m,n));
byte[] ak=R("ru-RU/ru_RU-ak-cur.lite.res");
var models=LegacyFreshMainRecognitionContext.Load(ak,R("ru-RU/ru_RU-lk-text.lite.res"),R("ru-RU/legacy-engine-profile.json"),R("line-geometry/line-2e3-30.res"),R("line-geometry/line-2e3-50.res"));
var table=LegacyCursiveSeparationTable.Load(R("ru-RU/legacy-separation-profile.json"));
using var cs=File.OpenRead(Path.Combine(m,"text-unicode/legacy-case-data.json"));using var ps=File.OpenRead(Path.Combine(m,"text-unicode/legacy-text-publication-data.json"));
using var ns=File.OpenRead(Path.Combine(m,"text-unicode/legacy-normalization-data.json"));using var cis=File.OpenRead(Path.Combine(m,"text-unicode/legacy-circled-payload-data.json"));
var cd=LegacyNativeCaseData.Load(cs);var pd=LegacyNativeTextPublicationData.Load(ps);var norm=LegacyNativeUnicodeNormalizationData.Load(ns);var circles=LegacyNativeCircledPayloadData.Load(cis);
InkStroke M()=>new(new InkPoint[]{new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)});
var samples=new List<(string Name,InkStroke[] Ink)>{("M",[M()]),("MIR",[M(),new(new InkPoint[]{new(70,10),new(70,90),new(120,10),new(120,90)}),new(new InkPoint[]{new(140,90),new(140,10),new(180,10),new(190,20),new(190,40),new(180,50),new(140,50)})]),("I",[new(new InkPoint[]{new(0,10),new(0,90)})]),("II",[new(new InkPoint[]{new(0,10),new(0,90)}),new(new InkPoint[]{new(40,10),new(40,90)})])};
var recorded=RecordedInputs.Load(args[1]);for(int i=0;i<recorded.Samples.Length;i++)samples.Add((recorded.Names[i],recorded.Samples[i]));



var output=new List<object>();int guards=0,complete=0,refused=0,totalNodes=0,totalWrites=0;
foreach(var(name,raw)in samples)
{
 using var scope=Scope(raw);var parent=Parent(scope);parent.Publish(Enumerable.Range(0,40).Select(i=>(byte)(i*29+7)).ToArray(),new byte[8160]);
 LegacyFreshMainGlobalDocumentCoverage coverage;
 try{coverage=LegacyFreshMainGlobalDocumentCoverage.Prepare(parent);}
 catch(NotSupportedException e)when(e.Message.StartsWith("Unequal actual WORD/TEXT")){output.Add(D(("name",name),("completed",false),("refusal","238B0")));refused++;continue;}
 Reject(()=>LegacyFreshMainGlobalDocumentSelectedSources.Prepare(coverage,new(true)));
 var pending=parent.CopyPendingSelections().ToArray();var line=parent.CopySourceLines56();var word48=parent.WordSource.CopyChoiceScalar48();uint wordFlags=parent.WordSource.Flags;
 var state=LegacyFreshMainGlobalDocumentSelectedSources.Prepare(coverage);state.Validate();complete++;
 var copies=state.Characters.Select(c=>D(("node",c.OriginalSource.Node.Identity),("before",c.OriginalSource.Flags!.Value),("metadata",c.MetadataFlags),("flags",c.Flags),("before44",unchecked((int)(c.OriginalSource.CopyAssignedScalars().Field40!.Value>>32))),("index44",c.SelectedChoiceIndex44),("selection",c.OriginalSource.SelectionId!.Value),("key16",Convert.ToHexString(c.OriginalSource.Node.CopyRanges16())),("records",c.OriginalSource.CopyRecords().Select(r=>D(("scalar40",Convert.ToHexString(r.CopyStorage40())),("utf8",r.Text is{}text?Convert.ToHexString(text.ReadBytes()):null))).ToArray()))).ToArray();
 var links=parent.WordSource.CopyCharacterLinks().Select(l=>new[]{l.Source.Node.Identity,l.Marker}).ToArray();
 var writes=state.SelectionWrites.Select(w=>new[]{w.TreeOrdinal,w.SourceNodeIdentity,w.SelectedMarker}).ToArray();
 output.Add(D(("name",name),("completed",true),("root",state.CharacterTreeRootIdentity),("tree",state.CharacterTree.Select(n=>new[]{n.Identity,n.Left,n.Right,n.Balance,n.PreviousInserted,n.NextInserted}).ToArray()),("characters",copies),("links",links),("writes",writes),("wordBefore",wordFlags),("wordMetadata",state.WordMetadataFlags),("wordFlags",state.WordFlags),("wordIndex",state.WordSelectedChoiceIndex44),("pendingBefore",pending.Select(p=>p.SelectionId).ToArray()),("pendingAfter",parent.CopyPendingSelections().Select(p=>p.SelectionId).ToArray()),("lineBefore",Convert.ToHexString(line)),("lineAfter",Convert.ToHexString(parent.CopySourceLines56())),("word48Before",Convert.ToHexString(word48)),("word48After",Convert.ToHexString(parent.WordSource.CopyChoiceScalar48()))));
 if(!ReferenceEquals(state.Coverage,coverage)||state.WordSelectedChoiceIndex44!=0||state.Characters.Count!=parent.Wrapper.CharacterInterner.Count||
  !pending.SequenceEqual(parent.CopyPendingSelections())||!line.SequenceEqual(parent.CopySourceLines56())||!word48.SequenceEqual(parent.WordSource.CopyChoiceScalar48())||parent.WordSource.Flags!=wordFlags)
  throw new Exception("Actual source owner or publication mutated by overlay.");
 foreach(var c in state.Characters)if(c.OriginalSource.Flags!=c.MetadataFlags||c.OriginalSource.CopyAssignedScalars().Field40!=-1)throw new Exception("Historical CHAR changed.");
 totalNodes+=state.Characters.Count;totalWrites+=state.SelectionWrites.Count;
 //Controlled current-owner mutations after inference, separately from capture.
 parent.WordSource.Flags^=0x40000000;Reject(()=>state.Validate());parent.WordSource.Flags=wordFlags;state.Validate();
 if(complete%3==0)parent.Wrapper.CharacterInterner.InternPrepared([]);
 else{long identity=-1;_ = scope.SharedState.Selections.CreatePrepared([],0,ref identity,complete%3==1?1:0);}
 Reject(()=>state.Validate());Reject(()=>LegacyFreshMainGlobalDocumentSelectedSources.Prepare(coverage));
 scope.Dispose();Reject(()=>state.Validate());Reject(()=>LegacyFreshMainGlobalDocumentSelectedSources.Prepare(coverage));
}
var prepared=new List<object>();
for(int i=0;i<256;i++)
{
 int n=2+i%7;uint[] before=Enumerable.Range(0,n).Select(k=>unchecked((uint)(i*1640531527L+k*2654435761L))).ToArray();int[] initial=Enumerable.Repeat(-1,n).ToArray();
 int[] ordinals=[0,1,n-1,0,1,0],markers=[i%11,-1,int.MinValue,5,int.MaxValue,-i];var flags=before.ToArray();var indices=initial.ToArray();
 LegacyFreshMainDocumentSelectedSources.ApplySelectedCharacters(flags,indices,ordinals,markers);prepared.Add(D(("before",before),("initial",initial),("ordinals",ordinals),("markers",markers),("flags",flags),("indices",indices)));
}
var lazy=new List<object>();
byte[] literal=new byte[16];BinaryPrimitives.WriteUInt32LittleEndian(literal.AsSpan(4),7);BinaryPrimitives.WriteUInt32LittleEndian(literal.AsSpan(12),7);BinaryPrimitives.WriteUInt16LittleEndian(literal.AsSpan(10),3);literal[9]=255;
for(int i=0;i<8;i++)
{
 uint flags=i switch{0=>0,1=>0x00200000,2=>0x00400000,3=>0x10000000,4=>0x20000000,5=>0x20000000,6=>0x30000000,_=>0};long id=i==4?-1:37;int count=i==7?2:1;
 bool success=true;try{LegacyFreshMainGlobalDocumentSelectedSources.ValidateMetadataBranch(flags,id,count,literal);}catch(NotSupportedException){success=false;guards++;}
 bool expected=i is not(0 or 5);if(success!=expected)throw new Exception("Lazy native branch changed.");lazy.Add(D(("flags",flags),("id",id),("count",count),("key16",Convert.ToHexString(literal)),("success",success)));
}
for(int i=0;i<6;i++)
{
 byte[] key=literal.ToArray();int at=new[]{4,2,1,10,9,12}[i];key[at]^=1;
 LegacyFreshMainGlobalDocumentSelectedSources.ValidateMetadataBranch(0,37,1,key);lazy.Add(D(("flags",0u),("id",37L),("count",1),("key16",Convert.ToHexString(key)),("success",true)));
}
//Lazy skip never needs even a key; bit29 without -1 must refuse before key.
LegacyFreshMainGlobalDocumentSelectedSources.ValidateMetadataBranch(0x10000000,37,1,[]);
LegacyFreshMainGlobalDocumentSelectedSources.ValidateMetadataBranch(0x20000000,-1,1,[]);
Reject(()=>LegacyFreshMainGlobalDocumentSelectedSources.ValidateMetadataBranch(0x20000000,37,1,[]));
Reject(()=>LegacyFreshMainGlobalDocumentSelectedSources.ValidateMetadataBranch(0,37,1,[]));
Reject(()=>LegacyFreshMainDocumentSelectedSources.ApplySelectedCharacters(new uint[1],new int[2],[],[]));
Reject(()=>LegacyFreshMainDocumentSelectedSources.ApplySelectedCharacters(new uint[1],new int[1],[1],[0]));
int[] overlap=[0];Reject(()=>LegacyFreshMainDocumentSelectedSources.ApplySelectedCharacters(new uint[1],overlap,overlap,[0]));
var preparedWords=new List<object>();
for(int i=0;i<256;i++)
{
 uint before=unchecked((uint)(i*2654435761L));uint[] metadata=Enumerable.Range(0,i%9).Select(k=>(i%4==0?0u:i%4==1?0x10000000u:i%4==2?0x01000000u:(k%2==0?0x4000u:0u))|((uint)(i+k)&255)).ToArray();
 preparedWords.Add(D(("before",before),("metadata",metadata),("after",LegacyFreshMainGlobalDocumentSelectedSources.ComputeWordMetadata(before,metadata))));
}
using(var jsonFile=File.Create(args[2]))using(var jsonWriter=new Utf8JsonWriter(jsonFile))WriteJson(jsonWriter,D(("samples",output),("completed",complete),("refused",refused),("totalNodes",totalNodes),("totalWrites",totalWrites),("prepared",prepared),("preparedWords",preparedWords),("lazy",lazy),("guards",guards)));
if(complete!=11||refused!=2||totalNodes!=33||totalWrites!=33)throw new Exception("Actual supported branch changed.");
Console.WriteLine($"PASS13 global selected sources/{complete} overlays/{refused} coverage refusals/{totalNodes} fullCHAR/{totalWrites} selected writes/256 prepared alias controls/14 lazy controls/{guards} guards; no020920/DOM.");
void Reject(Action action){try{action();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or NotSupportedException or OperationCanceledException){guards++;return;}throw new Exception("Expected selected source refusal.");}
LegacyLibraryGlobalTextPublicationScope Scope(InkStroke[] ink){var d=LegacyOwnedRawInkDocument.Create(ink,default);var original=LegacyLibraryOriginalTextFieldBootstrap.Prepare(LegacyFreshRawInkCurrentView.Prepare(d),["a","b"]);original.Run();var selected=LegacyLibraryOriginalSelectedFieldContext.Prepare(original,original.State.Inputs[1]);selected.Run();return LegacyLibraryGlobalTextPublicationScope.PrepareOriginalSelected(models,selected,0,table,cd,pd);}
LegacyFreshMainGlobalWordTextPublication Parent(LegacyLibraryGlobalTextPublicationScope scope){scope.Run(new byte[4096*40],new byte[120],new byte[40],new ushort[256],new byte[64],new byte[72]);var c=scope.CompleteSources(new byte[400],new byte[4],new byte[4]);return LegacyFreshMainGlobalWordTextPublication.Prepare(LegacyFreshMainGlobalTextSourceWrapper.Prepare(scope,c,ak,pd,norm,circles));}

Dictionary<string,object?> D(params (string Name,object? Value)[] fields)=>fields.ToDictionary(p=>p.Name,p=>p.Value);
void WriteJson(Utf8JsonWriter w,object? value)
{
 switch(value)
 {
  case null:w.WriteNullValue();break;
  case string text:w.WriteStringValue(text);break;
  case bool flag:w.WriteBooleanValue(flag);break;
  case int number:w.WriteNumberValue(number);break;
  case uint number:w.WriteNumberValue(number);break;
  case long number:w.WriteNumberValue(number);break;
  case Dictionary<string,object?> values:w.WriteStartObject();foreach(var pair in values){w.WritePropertyName(pair.Key);WriteJson(w,pair.Value);}w.WriteEndObject();break;
  case System.Collections.IEnumerable values:w.WriteStartArray();foreach(var item in values)WriteJson(w,item);w.WriteEndArray();break;
  default:throw new Exception("Untracked diagnostic JSON type "+value.GetType());
 }
}
