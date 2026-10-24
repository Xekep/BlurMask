using InkRecognition;
using System.Text.Json;

if(args.Length!=12)throw new ArgumentException("AK LK engine separation case publication normalization circles line30 line50 controls capture");
byte[] ak=File.ReadAllBytes(args[0]),lk=File.ReadAllBytes(args[1]),engine=File.ReadAllBytes(args[2]);
var table=LegacyCursiveSeparationTable.Load(File.ReadAllBytes(args[3]));
using var cf=File.OpenRead(args[4]);using var pf=File.OpenRead(args[5]);using var nf=File.OpenRead(args[6]);using var cr=File.OpenRead(args[7]);
var cases=LegacyNativeCaseData.Load(cf);var publication=LegacyNativeTextPublicationData.Load(pf);
var norm=LegacyNativeUnicodeNormalizationData.Load(nf);var circles=LegacyNativeCircledPayloadData.Load(cr);
byte[] line30=File.ReadAllBytes(args[8]),line50=File.ReadAllBytes(args[9]);
using var file=File.Create(args[11]);using var w=new Utf8JsonWriter(file);
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[10]));int faults=0,streams=0,guards=0,totalMatrices=0;
w.WriteStartObject();w.WriteStartArray("swaps");
foreach(var c in controls.RootElement.GetProperty("cases").EnumerateArray())
{
 var old=Read(c.GetProperty("old"));var current=old;var exported=Read(c.GetProperty("exported"));
 bool status=c.GetProperty("status").GetBoolean();var result=LegacyDocumentTransformSwap.Apply(ref current,exported,status);
 if(!ReferenceEquals(current,exported)||!ReferenceEquals(result.Released,old)||result.ExportSucceeded!=status)
  throw new InvalidDataException("Native unconditional vector swap/status differs.");
 Same(current,c.GetProperty("expected"));Same(result.Released,c.GetProperty("released"));
 var isolated=current.CopyScalars();if(isolated.Length>0){byte before=isolated[0];isolated[0]^=255;if(current.CopyScalars()[0]!=before)throw new InvalidDataException("Scalar aliases.");}
 var ranges=current.CopyRanges();if(ranges.Length>0&&ranges[0].Length>0){var before=ranges[0][0];ranges[0][0]=default;if(current.CopyRanges()[0][0]!=before)throw new InvalidDataException("Range aliases.");}
 w.WriteStartObject();w.WriteBoolean("status",status);WriteVector("old",old);WriteVector("exported",exported);WriteVector("current",current);WriteVector("released",result.Released);w.WriteBoolean("followingStage",result.ExportSucceeded);w.WriteEndObject();
 if(!status)faults++;streams++;
}
w.WriteEndArray();w.WriteStartArray("samples");
InkStroke[] m=[new(new InkPoint[]{new(0,90),new(0,90),new(0,10),new(25,60),new(50,10),new(50,90)})];
InkStroke[][] samples=[m,Word(0).Concat(Word(300)).ToArray(),Word(450).Concat(Word(0)).ToArray(),Rotate(m,.18f)];
for(int sample=0;sample<samples.Length;sample++)
{
 var residue=new LegacyMainConfiguredAlternativeResidue(new byte[120],new byte[72],new byte[40],new ushort[256],new byte[64],new byte[4],new byte[4],new byte[4],new byte[4]);
 var configured=LegacyFreshMainConfiguredSingleLinePlainRecognition.Prepare(ak,lk,engine,line30,line50,samples[sample],0,table,cases,publication).Run(residue,r=>new byte[r.RequiredBytes],new byte[20000*40]);
 var selected=LegacyFreshTextFieldInputContext.Prepare(LegacyFreshRawInkCurrentView.Prepare(configured.SourceDocument)).CreateSelectedCopy();
 bool bit8Refused=false;
 try{_ = LegacyFreshMainConfiguredNoCandidateSourceCompletion.Prepare(ak,configured,norm,circles,new byte[4],new byte[4]);}
 catch(NotSupportedException e)when(e.Message=="Enabled219580 candidate augmentation cannot be omitted."){bit8Refused=true;guards++;}
 if(!bit8Refused)throw new InvalidDataException("Original enabled-bit8 refusal was weakened or fixture changed.");
 var completion=LegacyFreshMainConfiguredWordAugmentation.Prepare(ak,configured,table,norm,circles,residue);completion.Run();
 if(!completion.IsTopV68Completed||completion.IsFaulted||!completion.SourcesMarkedComplete||
    completion.EligibleCandidates!=2||completion.AppendedWords!=0||completion.DuplicateWords!=2||completion.UpdatedNewDuplicates!=0||completion.QuotaAfter!=2)
  throw new InvalidDataException("Genuine configured duplicate augmentation did not complete its native mark/rank prefix.");
 using var words=LegacyFreshMainWordSourceChoices.PrepareAugmentedSelectedInput(ak,completion,selected,table,publication,norm,circles);
 var seed=words.DocumentSeed??throw new InvalidDataException("Seed missing before WORD publication.");
 var initialVector=seed.CurrentTransforms;
 byte[] initial=seed.CopyInitialScalarStorage20();
 if(seed.StringPool.Count!=0||!ReferenceEquals(seed.Input,selected.Input)||!ReferenceEquals(seed.PendingSelections,words.SelectionContext)||seed.SourceTag30!=selected.Input.RegistryId||
    !ReferenceEquals(initialVector,seed.InitialTransforms)||initialVector.Count!=0)
  throw new InvalidDataException("NEW2DA seed is not bound before shared3E/WORD publication.");
 var choices=words.Publish(new byte[40],new byte[2040*4]);
 if(!ReferenceEquals(seed.CurrentTransforms,initialVector))throw new InvalidDataException("WORD publication replaced the actual seed108 owner.");
 foreach(var choice in choices.Source.Choices)foreach(var glyph in choice.Glyphs)foreach(var row in glyph.Source.CopyRecords())
  if(!ReferenceEquals(row.Text!.Owner,seed.StringPool))throw new InvalidDataException("WORD publication uses another3E.");
 var text=LegacyFreshMainDocumentTextSelection.Prepare(LegacyFreshMainSourceLinePublication.PublishFresh(choices));text.Register();
 var lines=LegacyFreshMainDocumentLineConsistency.Apply(text);
 var state=LegacyFreshMainDocumentSelectedSources.Prepare(LegacyFreshMainDocumentCharacterCoverage.Prepare(LegacyFreshMainDocumentSourceCoverage.Prepare(lines)));
 var indices=LegacyFreshMainDocumentSourceIndices.Prepare(state);indices.Run();
 var cleanup=LegacyFreshMainDocumentControlCleanup.Prepare(indices);
 Reject(()=>LegacyFreshMainDocumentInputTransforms.Apply(cleanup));cleanup.Run();
 var pending=text.CopyPendingSelections().ToArray();long next=words.NextSelectionId;uint wordFlags=cleanup.WordFlags;
 Reject(()=>LegacyFreshMainDocumentInputTransforms.Apply(cleanup,new(true)));
 var actual=LegacyFreshMainDocumentInputTransforms.Apply(cleanup);Reject(()=>LegacyFreshMainDocumentInputTransforms.Apply(cleanup));
 if(!ReferenceEquals(actual.Seed,seed)||!ReferenceEquals(actual.Cleanup,cleanup)||actual.DocumentAlternativeCountD8!=choices.Alternatives.Count||
    !ReferenceEquals(actual.ReleasedVectorOwner,initialVector)||!ReferenceEquals(actual.VectorOwner,seed.CurrentTransforms)||
    ReferenceEquals(seed.CurrentTransforms,initialVector)||!ReferenceEquals(seed.InitialTransforms,initialVector)||
    actual.DocumentAlternativeCountD8!=2||lines.LineCount!=1||actual.TransformCount110!=0||actual.DafInputCount!=0||
    !actual.Succeeded||!actual.VectorWasSwapped||!actual.FollowingStage026EF0Executed||actual.CopyTransformScalarStorage64().Length!=0||
    !seed.CopyInitialScalarStorage20().AsSpan().SequenceEqual(initial)||wordFlags!=cleanup.WordFlags||
    words.NextSelectionId!=next||!pending.SequenceEqual(text.CopyPendingSelections()))
  throw new InvalidDataException("Native selected INPUT transform/DAF prefix or prior snapshots differ.");
 var export=actual.SourceExport.Geometry;
 w.WriteStartObject();w.WriteNumber("sample",sample);w.WriteBoolean("oldBit8Refusal",bit8Refused);w.WriteBoolean("sourceMarked",completion.SourcesMarkedComplete);
 w.WriteNumber("eligible",completion.EligibleCandidates);w.WriteNumber("appended",completion.AppendedWords);w.WriteNumber("duplicates",completion.DuplicateWords);w.WriteNumber("updatedDuplicates",completion.UpdatedNewDuplicates);w.WriteNumber("quota",completion.QuotaAfter);
 w.WriteBase64String("seed20",initial);w.WriteNumber("tag",seed.SourceTag30);
 w.WriteNumber("documentAlternatives",actual.DocumentAlternativeCountD8);w.WriteNumber("sourceLines",lines.LineCount);
 w.WriteNumber("transforms",actual.TransformCount110);w.WriteNumber("dafInputs",actual.DafInputCount);
 w.WriteBoolean("swapped",actual.VectorWasSwapped);w.WriteBoolean("followingStage",actual.FollowingStage026EF0Executed);
 w.WriteStartArray("pointCounts");foreach(var s in words.Document.Strokes)w.WriteNumberValue(s.Points.Count);w.WriteEndArray();
 WriteRanges("inputRanges",seed.Input.CopyRanges());w.WriteBase64String("matrices",export.CopyResolvedMatrices());
 w.WriteStartArray("matches");foreach(int i in export.CopyMatchedViewIndices())w.WriteNumberValue(i);w.WriteEndArray();
 w.WriteStartArray("fragments");foreach(var f in export.CopyFragments())
 {w.WriteStartArray();w.WriteNumberValue(f.SourceIndex);w.WriteNumberValue(f.PointCount);w.WriteNumberValue(f.FirstPoint);w.WriteNumberValue(f.LastPoint);w.WriteNumberValue(f.FirstFraction);w.WriteNumberValue(f.LastFraction);w.WriteBooleanValue(f.FirstClosed);w.WriteBooleanValue(f.LastClosed);w.WriteEndArray();}w.WriteEndArray();
 w.WriteNumber("pendingBefore",pending.Length);w.WriteNumber("pendingAfter",text.CopyPendingSelections().Count);w.WriteNumber("nextBefore",next);w.WriteNumber("nextAfter",words.NextSelectionId);w.WriteEndObject();totalMatrices+=export.MatricesResolved;
 using var oldWords=LegacyFreshMainWordSourceChoices.PrepareAugmented(ak,completion,table,publication,norm,circles);
 if(oldWords.DocumentSeed is not null)throw new InvalidDataException("Old API acquired a fake selected-input seed.");
 var oldChoices=oldWords.Publish(new byte[40],new byte[2040*4]);
 var oldText=LegacyFreshMainDocumentTextSelection.Prepare(LegacyFreshMainSourceLinePublication.PublishFresh(oldChoices));oldText.Register();
 var oldState=LegacyFreshMainDocumentSelectedSources.Prepare(LegacyFreshMainDocumentCharacterCoverage.Prepare(LegacyFreshMainDocumentSourceCoverage.Prepare(LegacyFreshMainDocumentLineConsistency.Apply(oldText))));
 var oldIndices=LegacyFreshMainDocumentSourceIndices.Prepare(oldState);oldIndices.Run();var oldCleanup=LegacyFreshMainDocumentControlCleanup.Prepare(oldIndices);oldCleanup.Run();
 Reject(()=>LegacyFreshMainDocumentInputTransforms.Apply(oldCleanup));
 using var faultWords=LegacyFreshMainWordSourceChoices.PrepareAugmentedSelectedInput(ak,completion,selected,table,publication,norm,circles);
 var faultSeed=faultWords.DocumentSeed!;var faultInitial=faultSeed.CurrentTransforms;
 var faultCleanup=CreateCleanup(faultWords);
 if(!ReferenceEquals(faultInitial,faultSeed.CurrentTransforms))throw new InvalidDataException("Fault seed was detached before export.");
 var prefix=LegacyDocumentTransformVector.Prepared(Enumerable.Range(0,64).Select(i=>(byte)(i^0xa7)).ToArray(),[selected.Input.CopyRanges()]);
 var failed=LegacyFreshMainDocumentInputTransforms.ApplyPreparedExport(faultCleanup,prefix,false);
 if(failed.Succeeded||failed.FollowingStage026EF0Executed||!ReferenceEquals(failed.Seed,faultSeed)||
    !ReferenceEquals(faultSeed.CurrentTransforms,prefix)||!ReferenceEquals(failed.VectorOwner,prefix)||
    !ReferenceEquals(failed.ReleasedVectorOwner,faultInitial)||faultSeed.InitialTransforms.Count!=0||failed.TransformCount110!=1)
  throw new InvalidDataException("Failed export did not retain the exact new seed-owned prefix before the outer status gate.");
 Reject(()=>LegacyFreshMainDocumentInputTransforms.Apply(faultCleanup));
 if(!ReferenceEquals(faultSeed.CurrentTransforms,prefix))throw new InvalidDataException("Retry guard reset the failed seed prefix.");
 words.Dispose();Reject(()=>LegacyFreshMainDocumentInputTransforms.Apply(cleanup));
}
w.WriteEndArray();w.WriteEndObject();w.Flush();
Console.WriteLine($"PASS{streams} independent swaps/{faults} failed export prefixes;4 actual selected INPUT owners/{totalMatrices} identity matrices/4 seed-owned failed prefixes/empty108+DAF/{guards} guards; no full0283B0 or document acceptance.");
LegacyFreshMainDocumentControlCleanup CreateCleanup(LegacyFreshMainWordSourceChoices o)
{
 var c=o.Publish(new byte[40],new byte[2040*4]);var t=LegacyFreshMainDocumentTextSelection.Prepare(LegacyFreshMainSourceLinePublication.PublishFresh(c));t.Register();
 var s=LegacyFreshMainDocumentSelectedSources.Prepare(LegacyFreshMainDocumentCharacterCoverage.Prepare(LegacyFreshMainDocumentSourceCoverage.Prepare(LegacyFreshMainDocumentLineConsistency.Apply(t))));
 var ix=LegacyFreshMainDocumentSourceIndices.Prepare(s);ix.Run();return LegacyFreshMainDocumentControlCleanup.Prepare(ix).Run();
}
LegacyDocumentTransformVector Read(JsonElement j)=>LegacyDocumentTransformVector.Prepared(j.GetProperty("scalars").GetBytesFromBase64(),j.GetProperty("ranges").EnumerateArray().Select(a=>a.EnumerateArray().Select(b=>new LegacyNativeInkPositionRange(new(b[0].GetUInt64()),new(b[1].GetUInt64()))).ToArray()).ToArray());
void Same(LegacyDocumentTransformVector v,JsonElement j){var e=Read(j);if(!v.CopyScalars().AsSpan().SequenceEqual(e.CopyScalars())||!v.CopyRanges().SelectMany(x=>x).SequenceEqual(e.CopyRanges().SelectMany(x=>x)))throw new InvalidDataException("Prepared vector prefix differs.");}
void WriteVector(string name,LegacyDocumentTransformVector v){w.WriteStartObject(name);w.WriteBase64String("scalars",v.CopyScalars());w.WriteStartArray("ranges");foreach(var a in v.CopyRanges()){w.WriteStartArray();foreach(var r in a){w.WriteStartArray();w.WriteNumberValue(r.First.Value);w.WriteNumberValue(r.Last.Value);w.WriteEndArray();}w.WriteEndArray();}w.WriteEndArray();w.WriteEndObject();}
void WriteRanges(string name,LegacyNativeInkPositionRange[] ranges){w.WriteStartArray(name);foreach(var r in ranges){w.WriteStartArray();w.WriteNumberValue(r.First.Value);w.WriteNumberValue(r.Last.Value);w.WriteEndArray();}w.WriteEndArray();}
void Reject(Action a){try{a();}catch(Exception e)when(e is ArgumentException or InvalidOperationException or OperationCanceledException or NotSupportedException){guards++;return;}throw new InvalidDataException("Required owner/lifetime guard missing.");}
InkStroke[] Rotate(InkStroke[] raw,float a)=>raw.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X*MathF.Cos(a)-p.Y*MathF.Sin(a),p.X*MathF.Sin(a)+p.Y*MathF.Cos(a))).ToArray())).ToArray();
InkStroke[] Word(float x)=>[Draw((x,80),(x,0),(x+30,45),(x+60,0),(x+60,80)),Draw((x+85,80),(x+85,0),(x+135,0),(x+135,80)),Draw((x+165,80),(x+165,0),(x+215,0),(x+215,40),(x+165,40))];
InkStroke Draw(params(float X,float Y)[] knots){var points=new List<InkPoint>();for(int i=1;i<knots.Length;i++)for(int t=0;t<8;t++){float f=t/8f;points.Add(new(knots[i-1].X+(knots[i].X-knots[i-1].X)*f,knots[i-1].Y+(knots[i].Y-knots[i-1].Y)*f));}points.Add(new(knots[^1].X,knots[^1].Y));return new(points.ToArray());}
