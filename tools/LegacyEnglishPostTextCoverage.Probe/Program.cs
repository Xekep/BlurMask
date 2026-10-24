using InkRecognition;
using System.Text.Json;
if(args.Length!=2||File.Exists(args[1]))throw new ArgumentException("controls fresh-output");
using var controls=JsonDocument.Parse(File.ReadAllBytes(args[0]));var output=new List<object>();int guards=0;
foreach(var t in controls.RootElement.GetProperty("Cases").EnumerateArray())
{
 var counts=t.GetProperty("Counts").EnumerateArray().Select(v=>v.GetInt32()).ToArray();
 var document=LegacyOwnedRawInkDocument.Create(counts.Select((n,s)=>new InkStroke(Enumerable.Range(0,n).Select(i=>new InkPoint(s,i)).ToArray())).ToArray(),default);
 var operations=LegacyOwnedRawInkRangeOperations.Prepare(document);var original=Read(t.GetProperty("Original"));
 var keys=t.GetProperty("Characters").EnumerateArray().Select(Read).ToArray();
 var union=operations.Union([],original);var remaining=original.ToArray();var before=Flat(original);
 foreach(var key in keys)remaining=operations.Subtract(remaining,key);
 var partial=operations.Subtract(original,keys[0]);
 if(!before.SequenceEqual(Flat(original)))throw new InvalidDataException("Kernel mutated caller key.");
 //Explicit numeric native branch decision, not an invented live owner.
 bool removeWhenMultiple=partial.Length!=0,keepWhenSole=true;
 var token=new CancellationToken(true);
 foreach(int kind in new[]{0,1,2})
 {
  try{if(kind==0)operations.Union([],original,token);else if(kind==1)operations.Subtract(original,keys[0],token);else operations.Intersect(original,keys[0],token);}
  catch(OperationCanceledException e)when(e.GetType()==typeof(OperationCanceledException)&&e.CancellationToken==token&&token.IsCancellationRequested){guards++;continue;}
  throw new InvalidDataException("Exact requested cancellation token required.");
 }
 output.Add(new{Union=Flat(union),Remaining=Flat(remaining),Partial=Flat(partial),MultipleChoiceRemovalRequired=removeWhenMultiple,SingleChoiceKept=keepWhenSole});
}
File.WriteAllBytes(args[1],JsonSerializer.SerializeToUtf8Bytes(new{Scope="PREPARED_C21_POSTTEXT_NUMERIC_NO_OWNING",Cases=output,RequestedTokenGuards=guards,NN=false,OwningExecuted=false,DocumentApplied=false}));
static LegacyNativeInkPositionRange[] Read(JsonElement rows)=>rows.EnumerateArray().Select(r=>new LegacyNativeInkPositionRange(new(r[0].GetUInt64()),new(r[1].GetUInt64()))).ToArray();
static ulong[] Flat(IEnumerable<LegacyNativeInkPositionRange> rows)=>rows.SelectMany(r=>new[]{r.First.Value,r.Last.Value}).ToArray();
