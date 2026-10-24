using InkRecognition;
using System.Buffers.Binary;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
if(args.Length!=2||args[0]!="--prepared")throw new ArgumentException("--prepared REPORT; NO inference mode");
var names=new List<string>();void Check(bool v,string n){if(!v)throw new Exception(n);names.Add(n);}
void Refuse(Action action,Type type,string n){try{action();}catch(Exception e){if(e.GetType()!=type)throw;names.Add(n);return;}throw new Exception(n);}
var compact=new byte[120];Check(!ExperimentalGlobalTextRecognizer.HasActiveAugmentation(compact,2),"actual-bit8-absent-old-route");compact[112]=8;Check(!ExperimentalGlobalTextRecognizer.HasActiveAugmentation(compact,2),"inactive-tail-not-admitted");Check(ExperimentalGlobalTextRecognizer.HasActiveAugmentation(compact,3),"actual-active-bit8-aug-route");
Refuse(()=>ExperimentalGlobalTextRecognizer.HasActiveAugmentation(compact,4),typeof(InvalidDataException),"invalid-active-extent");
using var cancel=new CancellationTokenSource();cancel.Cancel();try{ExperimentalGlobalTextRecognizer.HasActiveAugmentation(compact,3,cancel.Token);throw new Exception("cancel");}catch(OperationCanceledException e){Check(e.CancellationToken==cancel.Token,"dispatch-exact-token");}
byte[] row=new byte[40];void Score(float f)=>BinaryPrimitives.WriteSingleLittleEndian(row.AsSpan(16),f);
foreach(var (f,ok) in new(float,bool)[]{(.499f,false),(.5f,true),(1,true),(1.001f,false),(float.NaN,false),(float.PositiveInfinity,false),(float.NegativeInfinity,false)}){Score(f);Check(ExperimentalGlobalTextRecognizer.TryAdmitSelectedGlyph(row,"x",out float output)==ok&&(!ok||BitConverter.SingleToInt32Bits(output)==BitConverter.SingleToInt32Bits(f)),"score-bits-"+BitConverter.SingleToInt32Bits(f));}
Score(.8f);foreach(string v in new[]{""," ","x y","x\t","\u0001","\u200d","\ue000"})Check(!ExperimentalGlobalTextRecognizer.TryAdmitSelectedGlyph(row,v,out _),"text-filter-"+names.Count);
Check(ExperimentalGlobalTextRecognizer.TryAdmitSelectedGlyph(row,"\u0434\u0430",out _),"ordinary-unicode-text");
row[4]=1;Check(!ExperimentalGlobalTextRecognizer.TryAdmitSelectedGlyph(row,"x",out _),"nonordinary-kind-refusal");row[4]=0;Check(!ExperimentalGlobalTextRecognizer.TryAdmitSelectedGlyph(row.AsSpan(1),"x",out _),"incomplete-row-refusal");
var overloads=typeof(ExperimentalGlobalTextRecognizer).GetMethods().Where(m=>m.Name=="Load").Select(m=>m.GetParameters().Length).Order().ToArray();Check(overloads.SequenceEqual(new[]{9,11}),"old-eight-new-ten-resource-overloads");
try{ExperimentalGlobalTextRecognizer.Load([],[],[],[],[],[],[],[],[],[],cancel.Token);throw new Exception("loadcancel");}catch(OperationCanceledException e){Check(e.CancellationToken==cancel.Token,"load-before-resources-exact-cancel");}
Refuse(()=>ExperimentalGlobalTextRecognizer.Load([],[],[],[],[],[],[],[],[],[]),typeof(InvalidDataException),"new-load-extent-before-model");
byte[] Read(string key){using var s=Assembly.GetExecutingAssembly().GetManifestResourceStream("BlurMask.ExperimentalInk."+key)??throw new Exception(key);using var m=new MemoryStream();s.CopyTo(m);return m.ToArray();}
var n=Read("normalization");var c=Read("circles");Check(Convert.ToHexStringLower(SHA256.HashData(n))=="3cc1d91269959a13b5700b7463d3db7442b83b593ba70435a94665d6531226f2","embedded-normalization-exact");Check(Convert.ToHexStringLower(SHA256.HashData(c))=="701f03d601fa843a980abc4e9cf0c7175e707bb208b64866439b34871e9c05e2","embedded-circles-exact");
_ =LegacyNativeUnicodeNormalizationData.Load(new MemoryStream(n));_=LegacyNativeCircledPayloadData.Load(new MemoryStream(c));Check(true,"embedded-loaders-authenticate-native-tables");
var doc=LegacyOwnedRawInkDocument.Create([new InkStroke([new(0,0),new(1,1),new(2,2)]),new InkStroke([new(3,3),new(4,4)])],default);
var inputs=doc.CopySelectedInputs();var full=inputs.Select(i=>LegacyNativeTextSourceRangeProjection.Project(i)).ToArray();
Check(ExperimentalGlobalTextRecognizer.HasFullOriginalCoverage(doc,full),"same-original-all-strokes-full-coverage");
Check(!ExperimentalGlobalTextRecognizer.HasFullOriginalCoverage(doc,full.AsSpan(0,1)),"missing-original-stroke-refused");
var partial=full.ToArray();partial[1]=new(partial[1].First,new LegacyNativeInkPosition(((ulong)1<<32)|3UL));Check(!ExperimentalGlobalTextRecognizer.HasFullOriginalCoverage(doc,partial),"missing-stroke-tail-refused");
Check(!ExperimentalGlobalTextRecognizer.HasFullOriginalCoverage(doc,[]),"empty-WORD-not-full-TEXT");
var gap=new LegacyNativeInkPositionRange[]{new(LegacyNativeInkPosition.Pack(0,0,3,3),LegacyNativeInkPosition.Pack(0,0,5,3)),new(LegacyNativeInkPosition.Pack(0,2,3,3),LegacyNativeInkPosition.Pack(0,2,5,3)),full[1]};Check(!ExperimentalGlobalTextRecognizer.HasFullOriginalCoverage(doc,gap),"middle-gap-refused");
var extra=full.Concat(new[]{new LegacyNativeInkPositionRange(new((2UL<<32)|3UL),new((2UL<<32)|0xff05UL))}).ToArray();Check(!ExperimentalGlobalTextRecognizer.HasFullOriginalCoverage(doc,extra),"extra-object-range-refused");
try{ExperimentalGlobalTextRecognizer.HasFullOriginalCoverage(doc,full,cancel.Token);throw new Exception("coverage cancel");}catch(OperationCanceledException e){Check(e.CancellationToken==cancel.Token,"coverage-exact-token");}
var all=typeof(LegacyFreshMainGlobalWordTextPublication).GetMethods().Single(m=>m.Name=="PrepareAllWords");
Check(all.IsStatic&&all.ReturnType==typeof(LegacyFreshMainGlobalWordTextPublication)&&all.GetParameters()[0].ParameterType==typeof(LegacyFreshMainGlobalTextSourceWrapper),"all275-entry-requires-real-typed-wrapper");
Check(!typeof(ExperimentalGlobalTextRecognizer).GetMethods().Any(m=>m.Name.Contains("Alternative")||m.Name.Contains("Argmax")),"no-public-alternative-data-issuer-or-selection-policy");
File.WriteAllText(args[1],JsonSerializer.Serialize(new{Scope="Prepared hostpolicy/active-dispatch/embedded-Unicode ONLY; no model load/NN/live owner/q0 success",NNCalls=0,Guards=names.Count,Names=names},new JsonSerializerOptions{WriteIndented=true}));
