using InkRecognition;
using System.Text.Json;
if(args.Length!=1||File.Exists(args[0]))throw new ArgumentException("Fresh prepared report required.");
var pool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
var publisher=new LegacyNativePlainWordPublisher(pool,false,false,false);
LegacyNativeWordBuilder Word(int i)
{
    var word=LegacyNativeWordBuilder.CreateEmpty(pool,new byte[120]);
    var inserted=word.AppendGlyph((ulong)i,i,false,new byte[40]);pool.SetOrdinarySymbol(inserted.Reference,1);return word;
}
int First(int n,ushort a)=>0;int Last(int n,ushort a)=>1;
var head=publisher.Publish(Word(0),First,Last,-1,-1);
int positives=0,negatives=0;
void Assert(bool value){if(!value)throw new InvalidDataException("Prepared scalar contract failed.");positives++;}
void Refuse(Action action,Type? expected=null)
{try{action();}catch(Exception e)when(e.GetType()==(expected??typeof(InvalidOperationException))){negatives++;return;}throw new InvalidDataException("Invalid scalar owner accepted.");}
Assert(publisher.ReadFreshSingletonTopScalars(head,pool)==(1,0));
for(int i=1;i<4095;i++)_=publisher.InternPlainWord(Word(i),head.Span,out _);
var expected=publisher.CaptureSpan(head,[head]);
var scalar=publisher.ReadFreshSingletonTopScalars(head,pool);
Assert(scalar==(4095,0));Assert(scalar.Count==expected.Words.Count&&scalar.Selected==expected.SelectedOrdinals[0]);
//Measurement is supplemental own-runtime evidence, not CLR/RSS certification.
for(int i=0;i<100;i++)_=publisher.ReadFreshSingletonTopScalars(head,pool);
long before=GC.GetAllocatedBytesForCurrentThread();
for(int i=0;i<10000;i++)_=publisher.ReadFreshSingletonTopScalars(head,pool);
long allocated=GC.GetAllocatedBytesForCurrentThread()-before;
var foreignPool=LegacyNativeGlyphRecordPool.CreateWithoutGeometryCapture(false);
Refuse(()=>publisher.ReadFreshSingletonTopScalars(head,foreignPool));
Refuse(()=>publisher.ReadFreshSingletonTopScalars(null!,pool),typeof(ArgumentNullException));
var original=head.Word;head.RebindWord(4095,original);Refuse(()=>publisher.ReadFreshSingletonTopScalars(head,pool));head.RebindWord(0,original);
head.RebindWord(0,expected.Words[1]);Refuse(()=>publisher.ReadFreshSingletonTopScalars(head,pool));head.RebindWord(0,original);
using(var cts=new CancellationTokenSource())
{cts.Cancel();try{_=publisher.ReadFreshSingletonTopScalars(head,pool,cts.Token);throw new InvalidDataException("Canceled scalar lookup accepted.");}
 catch(OperationCanceledException e){Assert(e.GetType()==typeof(OperationCanceledException)&&e.CancellationToken==cts.Token&&cts.IsCancellationRequested);}}
var extra=publisher.ForkAlternative();Refuse(()=>publisher.ReadFreshSingletonTopScalars(head,pool));extra.ReleaseAlternativeList();Refuse(()=>publisher.ReadFreshSingletonTopScalars(head,pool));
publisher.ReleaseAlternativeList();Refuse(()=>publisher.ReadFreshSingletonTopScalars(head,pool));
var second=new LegacyNativePlainWordPublisher(pool,false,false,false);var a=second.Publish(Word(4094),First,Last,-1,-1);
var b=second.Publish(Word(4093),First,Last,-1,-1);Refuse(()=>second.ReadFreshSingletonTopScalars(a,pool));Refuse(()=>second.ReadFreshSingletonTopScalars(b,pool));
var third=new LegacyNativePlainWordPublisher(pool,false,false,false);var c=third.Publish(Word(0),First,Last,-1,-1);
_ = third.InternPlainWord(Word(1),new LegacyNativeWordSourceSpan(0,2),out _);Refuse(()=>third.ReadFreshSingletonTopScalars(c,pool));
using var file=new FileStream(args[0],FileMode.CreateNew);JsonSerializer.Serialize(file,new{PreparedOnly=true,NoModelOrNN=true,Runtime=Environment.Version.ToString(),Positives=positives,Refusals=negatives,AlternativeCount=4095,ScalarLookups=10000,MeasuredAllocatedBytes=allocated,AllocationObservationOnly=true,Scope="generic publisher scalar accessor; actual EN TOP owner unexecuted"});
