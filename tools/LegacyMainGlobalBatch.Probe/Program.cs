using InkRecognition;
using System.Text.Json;
//Prepared synthetic source geometry only: no model, Scope.Run, NN or live issuer.
if(args.Length!=1)throw new ArgumentException("fresh output JSON required");
var doc=LegacyOwnedRawInkDocument.Create([new InkStroke(Enumerable.Range(0,20).Select(i=>new InkPoint(i,0)).ToArray())],default);
var ops=LegacyOwnedRawInkRangeOperations.Prepare(doc);int cases=0,guards=0;
LegacyNativeInkPosition P(int p,byte m)=>new(((ulong)p<<16)|m);
LegacyNativeInkPositionRange R(int a,int b)=>new(P(a,3),P(b,5));
LegacyNativeInkPositionRange S(int a,byte am,int b,byte bm)=>new(P(a,am),P(b,bm));
void Check(LegacyNativeInkPositionRange[][] input,LegacyNativeInkPositionRange[][] expected)
{
 var before=input.Select(x=>x.ToArray()).ToArray();var result=LegacyPreparedMainWordRangeLookahead.Run(ops,input,8_386_560,128L*1024*1024);
 if(!result.Keys.Select((k,i)=>k.SequenceEqual(expected[i])).All(x=>x)||!input.Select((k,i)=>k.SequenceEqual(before[i])).All(x=>x))throw new InvalidDataException($"Range case{cases} differs/mutates input");cases++;
}
Check([[R(0,2)],[R(4,6)]],[[R(0,2)],[R(4,6)]]);
Check([[R(0,9)],[R(0,9)]],[[R(0,9)],[]]);
Check([[R(0,9)],[R(3,6)]],[[S(0,3,3,1),S(6,7,9,5)],[R(3,6)]]);
Check([[R(3,6)],[R(0,9)]],[[R(3,6)],[S(0,3,3,1),S(6,7,9,5)]]);
Check([[R(0,9)],[R(3,5)],[R(6,8)]],[[S(0,3,3,1),S(5,7,9,5)],[R(3,5)],[R(6,8)]]);
Check([[],[R(0,9)]],[[],[R(0,9)]]);Check([[R(0,9)],[]],[[R(0,9)],[]]);
void Refuse(Action action,Type exact){try{action();}catch(Exception e){if(e.GetType()==exact){guards++;return;}throw;}throw new InvalidDataException("Prepared guard not reached");}
Refuse(()=>LegacyPreparedMainWordRangeLookahead.Run(ops,[[R(0,9)],[R(0,9)]],1,1024,new CancellationToken(true)),typeof(OperationCanceledException));
Refuse(()=>LegacyPreparedMainWordRangeLookahead.Run(ops,[[R(0,9)],[R(0,9)],[R(0,9)]],2,1024),typeof(NotSupportedException));
Refuse(()=>LegacyPreparedMainWordRangeLookahead.Run(ops,[[R(0,9)],[R(0,9)]],1,31),typeof(NotSupportedException));
Refuse(()=>LegacyPreparedMainWordRangeLookahead.Run(ops,[Enumerable.Repeat(R(0,9),300).ToArray(),Enumerable.Repeat(R(0,9),300).ToArray()],1,128L*1024*1024),typeof(NotSupportedException));
int topologies=0;
for(int trial=0;trial<64;trial++)
{
 var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);var publisher=new LegacyNativePlainWordPublisher(pool,false,false,false);
 var words=new LegacyNativeWordBuilder[4];
 for(int owner=0;owner<words.Length;owner++)
 {
  var word=LegacyNativeWordBuilder.CreateEmpty(pool,new byte[120]);word.AppendGlyph((ulong)owner,owner,false,new byte[40]);
  word.SetRawScalars(owner+1,1,owner,0);words[owner]=word;
 }
 int n=1+trial%12;int[] order=Enumerable.Range(0,n).Select(j=>(trial+j*j)%4).ToArray();
 var nodes=new List<LegacyNativePublishedWordNode>();
 for(int j=order.Length-1;j>=0;j--){int owner=order[j];nodes.Insert(0,publisher.Publish(words[owner],(_,_)=>owner*128,(_,_)=>owner*128+1));}
 var binding=publisher.CaptureSourceBatch(default);publisher.RequireSourceBatch(binding,true,default);
 if(!binding.Nodes.SequenceEqual(nodes)||!binding.Owners.Select(x=>x.Span.First/128).SequenceEqual(order.Distinct()))throw new InvalidDataException("Prepared actual publisher encounter order differs");
 foreach(var owner in binding.Owners)
 {
  var current=publisher.CaptureSpan(owner,binding.Heads);
  if(current.Words.Count!=1||current.SelectedOrdinals.Any(x=>x!=0)||current.SelectedNodes.Count!=order.Count(x=>x==owner.Span.First/128))throw new InvalidDataException("Prepared same-owner selected279 binding differs");
 }
 _=publisher.ForkAlternative();
 Refuse(()=>publisher.RequireSourceBatch(binding,false,default),typeof(InvalidOperationException));topologies++;
}
{
 var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);var publisher=new LegacyNativePlainWordPublisher(pool,false,false,false);
 for(int i=0;i<2;i++){var word=LegacyNativeWordBuilder.CreateEmpty(pool,new byte[120]);word.AppendGlyph((ulong)i,i,false,new byte[40]);publisher.Publish(word,(_,_)=>0,(_,_)=>127);}
 Refuse(()=>publisher.CaptureSourceBatch(default),typeof(NotSupportedException));
}
File.WriteAllText(args[0],JsonSerializer.Serialize(new{Scope="Prepared synthetic WORD-level range cases only; no actual batch/NN/whole allocation proof",Cases=cases,Topologies=topologies,Guards=guards}));
Console.WriteLine($"PASS {cases} range cases/{topologies} topologies/{guards} guards");
