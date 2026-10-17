using InkRecognition;
using System.Text.Json;
if(args.Length!=11)throw new ArgumentException("AK LK engine separation case publication normalization circles line30 line50 capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);using var nf=File.OpenRead(args[6]);using var cr=File.OpenRead(args[7]);
var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
var norm=LegacyNativeUnicodeNormalizationData.Load(nf);var circles=LegacyNativeCircledPayloadData.Load(cr);
byte[] l30=File.ReadAllBytes(args[8]),l50=File.ReadAllBytes(args[9]);
InkStroke[] m=[new(new InkPoint[]{new(0,90),new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)})];
InkStroke[][] samples=[m,Word(0).Concat(Word(300)).ToArray(),Word(450).Concat(Word(0)).ToArray(),Rotate(m,.18f)];
using var file=File.Create(args[10]);using var w=new Utf8JsonWriter(file);w.WriteStartObject();w.WriteStartArray("samples");int guards=0,totalSources=0;
foreach(var raw in samples)
{
 var residue=new LegacyMainConfiguredAlternativeResidue(new byte[120],new byte[72],new byte[40],new ushort[256],new byte[64],new byte[4],new byte[4],new byte[4],new byte[4]);
 var result=LegacyFreshMainConfiguredSingleLinePlainRecognition.Prepare(ak,lk,engine,l30,l50,raw,0,table,cases,publication).Run(residue,r=>new byte[r.RequiredBytes],new byte[20000*40]);
 var completed=LegacyFreshMainConfiguredNoCandidateSourceCompletion.Prepare(ak,result,norm,circles,new byte[4],new byte[4]);completed.Run();
 using var words=LegacyFreshMainWordSourceChoices.Prepare(ak,completed,table,publication,norm,circles);
 var choices=words.Publish(new byte[40],new byte[2040*4]);var lines=LegacyFreshMainSourceLinePublication.PublishFresh(choices);
 var text=LegacyFreshMainDocumentTextSelection.Prepare(lines);text.Register();var lineResult=LegacyFreshMainDocumentLineConsistency.Apply(text);
 var source=LegacyFreshMainDocumentSourceCoverage.Prepare(lineResult);
 var coverage=LegacyFreshMainDocumentCharacterCoverage.Prepare(source);
 var state=LegacyFreshMainDocumentSelectedSources.Prepare(coverage);

 Reject(()=>LegacyFreshMainDocumentSourceIndices.Prepare(state,new(true)));
 var indices=LegacyFreshMainDocumentSourceIndices.Prepare(state);
 Reject(()=>indices.Run(new(true)));if(indices.IsReady||indices.IsFaulted)throw new InvalidDataException("Pre-entry cancellation changed index state.");
 Reject(()=>LegacyFreshMainDocumentSourceIndices.Prepare(state));
 var rebuilt=LegacyFreshMainDocumentSelectedSources.Prepare(coverage);Reject(()=>LegacyFreshMainDocumentSourceIndices.Prepare(rebuilt));
 indices.Run();indices.ValidateCurrent();if(!indices.IsReady||indices.IsFaulted)throw new InvalidDataException("Both source passes did not finish.");
 Reject(()=>indices.Run());
 var insertion=state.Characters[0].OriginalSource.SourceNode.Owner.CopyInsertionOrder();
 w.WriteStartObject();w.WriteStartArray("charInsertion");foreach(var n in insertion)w.WriteNumberValue(n.Identity);w.WriteEndArray();
 w.WriteStartArray("charInOrder");foreach(var c in state.Characters)w.WriteNumberValue(c.OriginalSource.SourceNode.Identity);w.WriteEndArray();
 w.WriteStartArray("inputs");
 foreach(var n in insertion){var c=state.Characters.Single(c=>c.OriginalSource.SourceNode.Identity==n.Identity);WriteSource(false,n.Identity,c.Flags,0x20,n.FirstProcessedSource!.CopyRanges());}
 var wr=state.OriginalWordSource.CopyOrdinaryRanges();int wh=0x30;if(wr.Length==0){wr=state.OriginalWordSource.CopySourceKeyRanges();wh=0x20;}
 WriteSource(true,state.OriginalWordSource.SourceNode.Identity,state.WordFlags,wh,wr);w.WriteEndArray();
 var sources=indices.CopySources();totalSources+=sources.Count;w.WriteBoolean("ready",indices.IsReady);w.WriteBoolean("faulted",indices.IsFaulted);
 foreach(var snapshot in new[]{indices.CopyCharacterIndex(),indices.CopyWordIndex()})
  if(!ReferenceEquals(snapshot.Owner,indices)||snapshot.Intervals.Concat(snapshot.Vectors).Any(n=>!ReferenceEquals(n.Owner,snapshot)))throw new InvalidDataException("Index/node ownership sidecar differs.");
 w.WriteStartArray("processed");foreach(var s in sources){if(!ReferenceEquals(s.State,state))throw new InvalidDataException("Source state owner mismatch.");WriteSource(s.IsWord,s.SourceNodeIdentity,s.Flags,s.NativeRangeHeaderOffset,s.CopyRanges());}
 w.WriteEndArray();WriteActual("characters",indices.CopyCharacterIndex(),sources);WriteActual("words",indices.CopyWordIndex(),sources);
 //Snapshots own their key arrays; typed sources retain immutable payloads.
 foreach(var n in indices.CopyCharacterIndex().Intervals){var key=n.CopyKey();if(key.Length!=0){var saved=key[0];key[0]=default;if(n.CopyKey()[0]!=saved)throw new InvalidDataException("Mutable key alias.");}}
 var charSource=sources.First(s=>!s.IsWord);var wordSource=sources.First(s=>s.IsWord);Reject(()=>_ = charSource.Word);Reject(()=>_ = wordSource.Character);
 state.OriginalWordSource.SourceNode.Owner.InternPrepared([]); //explicit missing WORD payload guard after inference
 Reject(()=>indices.ValidateCurrent());Reject(()=>LegacyFreshMainDocumentSourceIndices.Prepare(state));
 words.Dispose();Reject(()=>indices.ValidateCurrent());Reject(()=>LegacyFreshMainDocumentSourceIndices.Prepare(state));
 w.WriteEndObject();
}
w.WriteEndArray();w.WriteStartArray("prepared");
for(int c=0;c<40;c++)
{
 var k=new LegacyMainDocumentSourceIndexKernel<int>();var inputs=new List<LegacyNativeInkPositionRange[]>();
 for(int i=0;i<320;i++)
 {
  //All four rotations, page boundaries, duplicate vectors, empty vectors,
  //raw unsigned high-bit keys, repeated membership and unsorted key vectors.
  int v=(c%4) switch{0=>i,1=>319-i,2=>(i*73)%181,_=>(i*127+31)%193};
  ulong a=unchecked((ulong)(v*2654435761L+c*17L));if(c%3==0)a|=1UL<<63;
  LegacyNativeInkPositionRange r=new(new(a),new(a+3));
  LegacyNativeInkPositionRange[] key=i%11==0?[]:i%9==0?[r,r]:i%7==0?[new(new(a+10),new(a+12)),r]:[r];
  if(i%13==0&&inputs.Count>1)key=inputs[1].ToArray();
  inputs.Add(key);k.Add(i+1,key);
 }
 w.WriteStartObject();w.WriteStartArray("inputs");foreach(var key in inputs)WriteRangeArray(key);w.WriteEndArray();WriteKernel("index",k);w.WriteEndObject();
 //Canceled insertion makes no changes to an already-owned prefix.
 int count=k.SourcesAdded,memberCount=k.Memberships.Count;Reject(()=>k.Add(999,[],new(true)));
 if(k.SourcesAdded!=count||k.Memberships.Count!=memberCount)throw new InvalidDataException("Canceled Add changed prefix.");
}
w.WriteEndArray();w.WriteEndObject();w.Flush();Console.WriteLine($"PASS4 actual source owners/{totalSources} indexed payloads/40 prepared AVL+page controls/{guards} guards; fresh020920 only, no document acceptance.");
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or OperationCanceledException or InvalidOperationException or NotSupportedException){guards++;return;}throw new InvalidDataException("Missing source-index gate.");}
void WriteSource(bool word,int id,uint flags,int offset,LegacyNativeInkPositionRange[] ranges){w.WriteStartObject();w.WriteBoolean("word",word);w.WriteNumber("id",id);w.WriteNumber("flags",flags);w.WriteNumber("header",offset);w.WritePropertyName("ranges");WriteRangeArray(ranges);w.WriteEndObject();}
void WriteRangeArray(LegacyNativeInkPositionRange[] ranges){w.WriteStartArray();foreach(var r in ranges){w.WriteStartArray();w.WriteNumberValue(r.First.Value);w.WriteNumberValue(r.Last.Value);w.WriteEndArray();}w.WriteEndArray();}
void WriteActual(string name,LegacyMainDocumentSourceIndexSnapshot s,IReadOnlyList<LegacyOwnedMainDocumentIndexedSource> sources)
{
 w.WriteStartObject(name);w.WriteNumber("intervalRoot",s.IntervalRoot);w.WriteNumber("vectorRoot",s.VectorRoot);w.WriteBoolean("dirty",s.IntervalDirty);w.WriteNumber("added",s.SourcesAdded);
 w.WriteStartArray("intervals");foreach(var n in s.Intervals)WriteNode(n.Identity,n.CreationPageIdentity,n.Slot,n.Left,n.Right,n.Balance,n.MembershipHead,0,n.CopyKey());w.WriteEndArray();
 w.WriteStartArray("vectors");foreach(var n in s.Vectors)WriteNode(n.Identity,n.CreationPageIdentity,n.Slot,n.Left,n.Right,n.Balance,0,Find(n.CurrentSource!),n.CopyKey());w.WriteEndArray();
 w.WriteStartArray("members");foreach(var m in s.Memberships){w.WriteStartArray();foreach(int x in new[]{m.Identity,m.CreationPageIdentity,m.Slot,Find(m.Source),m.Next})w.WriteNumberValue(x);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
 int Find(LegacyOwnedMainDocumentIndexedSource source){for(int i=0;i<sources.Count;i++)if(ReferenceEquals(sources[i],source))return i+1;throw new InvalidDataException("Foreign indexed source.");}
}
void WriteKernel(string name,LegacyMainDocumentSourceIndexKernel<int> k)
{
 w.WriteStartObject(name);w.WriteNumber("intervalRoot",k.Intervals.Root?.Identity??0);w.WriteNumber("vectorRoot",k.Vectors.Root?.Identity??0);w.WriteBoolean("dirty",k.Dirty);w.WriteNumber("added",k.SourcesAdded);
 w.WriteStartArray("intervals");foreach(var n in k.Intervals.Nodes)WriteNode(n.Identity,n.PageIdentity,n.Slot,n.Left?.Identity??0,n.Right?.Identity??0,n.Balance,n.MembershipHead,0,n.Key);w.WriteEndArray();
 w.WriteStartArray("vectors");foreach(var n in k.Vectors.Nodes)WriteNode(n.Identity,n.PageIdentity,n.Slot,n.Left?.Identity??0,n.Right?.Identity??0,n.Balance,0,n.CurrentSource,n.Key);w.WriteEndArray();
 w.WriteStartArray("members");foreach(var m in k.Memberships){w.WriteStartArray();foreach(int x in new[]{m.Identity,m.PageIdentity,m.Slot,m.Source,m.Next})w.WriteNumberValue(x);w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();
}
void WriteNode(int id,int page,int slot,int left,int right,sbyte balance,int head,int source,LegacyNativeInkPositionRange[] key)
{w.WriteStartObject();w.WriteStartArray("fields");foreach(int x in new[]{id,page,slot,left,right,balance,head,source})w.WriteNumberValue(x);w.WriteEndArray();w.WritePropertyName("key");WriteRangeArray(key);w.WriteEndObject();}
InkStroke[] Rotate(InkStroke[] raw,float a)=>raw.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X*MathF.Cos(a)-p.Y*MathF.Sin(a),p.X*MathF.Sin(a)+p.Y*MathF.Cos(a))).ToArray())).ToArray();
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots){var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}
