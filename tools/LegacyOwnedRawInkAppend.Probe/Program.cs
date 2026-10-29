using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;
using System.Security.Cryptography;
using State=InkRecognition.LegacyOwnedRawInkAppendState;
if(args.Length!=1)throw new ArgumentException("Pinned fixtures only");
byte[] bytes=File.ReadAllBytes(args[0]);if(Convert.ToHexStringLower(SHA256.HashData(bytes))!="80b88eb48371bf74e19c048f8b8de61ad8ac0a20873093aba6b90cf8d7262a1e")throw new Exception("Fixture pin");
int controls=0,cases=0;
void Need(bool ok,string why){controls++;if(!ok)throw new Exception(why);}
void Refuse<T>(Action action,string why)where T:Exception{try{action();}catch(T){controls++;return;}throw new Exception("Missing refusal: "+why);}
string XY(IReadOnlyList<InkPoint> points){byte[] b=new byte[points.Count*8];for(int i=0;i<points.Count;i++){BinaryPrimitives.WriteSingleLittleEndian(b.AsSpan(i*8),points[i].X);BinaryPrimitives.WriteSingleLittleEndian(b.AsSpan(i*8+4),points[i].Y);}return Convert.ToHexStringLower(b);}
InkPoint[] Points(JsonElement x)=>x.EnumerateArray().Select(p=>new InkPoint(p[0].GetSingle(),p[1].GetSingle())).ToArray();
using var data=JsonDocument.Parse(bytes);
foreach(var fixture in data.RootElement.EnumerateArray())
{
 var ink=fixture.GetProperty("original").EnumerateArray().Select(x=>new InkStroke(Points(x))).ToArray();var document=LegacyOwnedRawInkDocument.Create(ink,default);var fresh=LegacyFreshRawInkCurrentView.Prepare(document);using var state=State.Adopt(fresh);var before=state.Capture();
 var expected=fixture.GetProperty("expectedProjectionXY").EnumerateArray().Select(x=>x.GetString()).ToArray();var input=Points(fixture.GetProperty("append"));var originalInput=input.ToArray();
 var appended=state.Append(before,new InkStroke(input),fixture.GetProperty("expectedCloneReservation").GetInt64());Need(!ReferenceEquals(appended,before),"New receipt");
 State.Borrow? escaped=null;LegacyOwnedRawInkDocument? projection=null;
 state.WithCurrent(appended,b=>{escaped=b;projection=b.Document;Need(b.Count==3&&b.Sequence==1,"Full frame");Need(b.ClonePayloadReservation==fixture.GetProperty("expectedCloneReservation").GetInt64(),"Reservation");Need(!ReferenceEquals(b.Document,document),"Minted child");for(int i=0;i<2;i++){Need(ReferenceEquals(b.Curve(i),fresh.Curves[i]),"Prefix C49 identity");Need(ReferenceEquals(b.View(i),fresh.Views[i]),"Prefix link identity");Need(ReferenceEquals(b.Curve(i).Document,document),"Historical curve owner retained");}for(int i=0;i<3;i++){Need(Convert.ToHexStringLower(b.Curve(i).CopyPointStorage())==expected[i],"Exact curve float bits");Need(XY(b.Document.Strokes[i].Points)==expected[i],"Exact projection float bits");Need(b.Curve(i).OriginalSourceIndex==i&&b.View(i).CurrentViewIndex==i&&b.View(i).Descriptor.SourceIndex==i,"IDs");}Need(ReferenceEquals(b.Curve(2).Document,b.Document),"Appended C49 minted owner");Refuse<InvalidOperationException>(()=>state.Capture(),"Reentrant capture");Refuse<InvalidOperationException>(()=>state.Append(appended,new InkStroke(originalInput)),"Reentrant append");Refuse<InvalidOperationException>(()=>state.WithCurrent(appended,_=>0),"Reentrant borrow");Need(b.Count==3,"Nested refusal leaves lease valid");return 0;});
 Refuse<InvalidOperationException>(()=>{_=escaped!.Count;},"Escaped borrow");Refuse<InvalidOperationException>(()=>state.WithCurrent(before,_=>0),"Stale receipt");Refuse<InvalidOperationException>(()=>state.WithCurrent(new State.Revision(),_=>0),"Forged receipt");using(var foreign=State.Adopt(fresh))Refuse<InvalidOperationException>(()=>state.WithCurrent(foreign.Capture(),_=>0),"Foreign authority same geometry");
 input[0]=new(999,999);state.WithCurrent(appended,b=>{Need(XY(b.Document.Strokes[2].Points)==expected[2],"Caller mutation isolated");return 0;});Need(XY(document.Strokes[0].Points)==expected[0],"Old document immutable");
 State.Borrow? faultLease=null;Refuse<FormatException>(()=>state.WithCurrent<int>(appended,b=>{faultLease=b;throw new FormatException();}),"Callback fault");Refuse<InvalidOperationException>(()=>{_=faultLease!.Count;},"Fault-finally revocation");Need(ReferenceEquals(state.Capture(),appended),"Fault no commit");
 var prefix=state.WithCurrent(appended,b=>Enumerable.Range(0,b.Count).Select(b.Curve).ToArray());var second=state.Append(appended,new InkStroke(originalInput));state.WithCurrent(second,b=>{Need(b.Count==4&&b.Sequence==2&&b.Curve(3).OriginalSourceIndex==3,"Sequential append ID3");for(int i=0;i<3;i++)Need(ReferenceEquals(prefix[i],b.Curve(i)),"All prior C49 retained second append");return 0;});
 cases++;
}
// Refusal controls use fresh separate owners, no NN/model input.
LegacyFreshRawInkCurrentView View(int count=2)=>LegacyFreshRawInkCurrentView.Prepare(LegacyOwnedRawInkDocument.Create(Enumerable.Range(0,count).Select(i=>new InkStroke(new[]{new InkPoint(i,0)})).ToArray(),default));
using(var s=State.Adopt(View()))
{
 var old=s.Capture();var poison=new ObservedPoints(2,_=>throw new Exception("Must not read point"));using var canceled=new CancellationTokenSource();canceled.Cancel();
 Refuse<OperationCanceledException>(()=>s.Append(old,new(poison),token:canceled.Token),"Pre-cancel");Need(poison.Reads==0,"Pre-cancel before point read");Refuse<NotSupportedException>(()=>s.Append(old,new(poison),0),"Budget before point read");Need(poison.Reads==0,"Budget prefix");var oversized=new ObservedPoints(4097,_=>throw new Exception());Refuse<NotSupportedException>(()=>s.Append(old,new(oversized)),"Point cap before read");Need(oversized.Reads==0,"Point cap prefix");
 using var mid=new CancellationTokenSource();var cancelPoints=new ObservedPoints(2,i=>{mid.Cancel();return new(i,0);});Refuse<OperationCanceledException>(()=>s.Append(old,new(cancelPoints),token:mid.Token),"Mid-copy cancel");Need(cancelPoints.Reads==1&&ReferenceEquals(old,s.Capture()),"Exact cancellation prefix unchanged");
 Refuse<ArgumentException>(()=>s.Append(old,new(new[]{new InkPoint(float.NaN,0)})),"NaN refusal");Refuse<ArgumentException>(()=>s.Append(old,new(new[]{new InkPoint(1000001,0)})),"Coordinate cap");Need(ReferenceEquals(old,s.Capture()),"Raw refusal receipt unchanged");
 var finite=new ObservedPoints(2,i=>new(i,0));long exact=(2+2)*24+3*256+512;Refuse<NotSupportedException>(()=>s.Append(old,new(finite),exact-1),"One-less exact reservation");Need(finite.Reads==0,"One-less before read");s.Append(old,new(finite),exact);Need(finite.Reads==2,"Exact reservation accepts");
}
using(var s=State.Adopt(View(256))){var old=s.Capture();var points=new ObservedPoints(1,_=>throw new Exception());Refuse<NotSupportedException>(()=>s.Append(old,new(points)),"256 object cap");Need(points.Reads==0&&ReferenceEquals(old,s.Capture()),"Object prefix unchanged");}
using(var s=State.Adopt(View())){var old=s.Capture();State.Borrow? borrowed=null;Refuse<InvalidOperationException>(()=>s.WithCurrent(old,b=>{borrowed=b;s.Dispose();return 0;}),"Disposal lease fence");Refuse<InvalidOperationException>(()=>{_=borrowed!.Count;},"Disposal escaped fence");Refuse<ObjectDisposedException>(()=>s.Capture(),"Disposed authority");}
// V2 prepared lifecycle and all explicit refusal boundaries.
using(var a=State.Adopt(View()))using(var b=State.Adopt(View()))
{
 var ra=a.Capture();var rb=b.Capture();var geometry=new InkStroke(new[]{new InkPoint(1,2),new InkPoint(3,4)});var pa=a.PrepareAppend(ra,geometry);var pb=b.PrepareAppend(rb,geometry);
 Need(a.PreparedPointCount(pa)==2&&ReferenceEquals(ra,a.Capture()),"Prepare reserves but does not publish");a.WithCurrent(ra,x=>{Need(x.Count==2,"Old current still readable while pending");return 0;});
 var poison=new ObservedPoints(2,_=>throw new Exception());Refuse<InvalidOperationException>(()=>a.PrepareAppend(ra,new(poison)),"Exclusive pending");Need(poison.Reads==0,"Exclusive before read");
 Refuse<InvalidOperationException>(()=>a.CommitPrepared(pb),"Foreign prepared commit");Refuse<InvalidOperationException>(()=>a.AbandonPrepared(pb),"Foreign prepared abandon");Refuse<InvalidOperationException>(()=>a.CommitPrepared(new State.PreparedAppend()),"Forged prepared commit");Refuse<InvalidOperationException>(()=>a.PreparedPointCount(new State.PreparedAppend()),"Forged prepared read");Need(a.PreparedPointCount(pa)==2&&b.PreparedPointCount(pb)==2,"Foreign failures preserve both real pending tokens");
 a.AbandonPrepared(pa);a.AbandonPrepared(pa);Need(ReferenceEquals(ra,a.Capture()),"Abandon no commit");Refuse<InvalidOperationException>(()=>a.CommitPrepared(pa),"Abandoned token stale");
 var pc=a.PrepareAppend(ra,geometry);using var canceled=new CancellationTokenSource();canceled.Cancel();Refuse<OperationCanceledException>(()=>a.CommitPrepared(pc,canceled.Token),"Commit pre-cancel");Need(ReferenceEquals(ra,a.Capture()),"Canceled prepared commit unchanged");Refuse<InvalidOperationException>(()=>a.PreparedPointCount(pc),"Failed commit clears its own pending");
 var pd=a.PrepareAppend(ra,geometry);var committed=a.CommitPrepared(pd);Need(!ReferenceEquals(ra,committed),"Prepared commit publishes once");Refuse<InvalidOperationException>(()=>a.CommitPrepared(pd),"Committed token cannot retry");
 var pe=a.PrepareAppend(committed,geometry);Refuse<InvalidOperationException>(()=>a.AbandonPrepared(pd),"Prior cleanup cannot clear successor pending");Need(a.PreparedPointCount(pe)==2,"Successor pending retained");a.AbandonPrepared(pe);
 var stalePoints=new ObservedPoints(2,_=>throw new Exception());Refuse<InvalidOperationException>(()=>a.Append(ra,new(stalePoints)),"Stale Append before point read");Refuse<InvalidOperationException>(()=>a.Append(rb,new(stalePoints)),"Foreign Append before point read");Need(stalePoints.Reads==0&&ReferenceEquals(committed,a.Capture()),"Refused Append current unchanged");
}
using(var s=State.Adopt(LegacyFreshRawInkCurrentView.Prepare(LegacyOwnedRawInkDocument.Create(new[]{new InkStroke(Enumerable.Range(0,4096).Select(i=>new InkPoint(i,0)).ToArray())},default))))
{var old=s.Capture();var poison=new ObservedPoints(1,_=>throw new Exception());Refuse<NotSupportedException>(()=>s.PrepareAppend(old,new(poison)),"4096 plus1 total cap");Need(poison.Reads==0&&ReferenceEquals(old,s.Capture()),"Total cap before point read/no commit");}
using(var s=State.Adopt(View())){var old=s.Capture();var poison=new ObservedPoints(0,_=>throw new Exception());Refuse<NotSupportedException>(()=>s.PrepareAppend(old,new(poison)),"Empty append");Refuse<ArgumentOutOfRangeException>(()=>s.PrepareAppend(old,new(poison),-1),"Negative reservation");Refuse<ArgumentOutOfRangeException>(()=>s.PrepareAppend(old,new(poison),State.DefaultClonePayloadBudget+1),"Above envelope reservation");Need(poison.Reads==0&&ReferenceEquals(old,s.Capture()),"Argument refusals before read");var pending=s.PrepareAppend(old,new(new[]{new InkPoint(1,1)}));s.Dispose();Refuse<ObjectDisposedException>(()=>s.CommitPrepared(pending),"Disposed prepared token");Refuse<ObjectDisposedException>(()=>s.PreparedPointCount(pending),"Disposed prepared count");}
// Concurrency regression: a competing successor prepare cannot enter the
// Append wrapper's Prepare/Commit/finally-Abandon critical section. No hook or
// expected recognition output participates; geometry is ordinary raw input.
using(var s=State.Adopt(View()))
using(var entered=new ManualResetEventSlim())
using(var release=new ManualResetEventSlim())
using(var competitorStarted=new ManualResetEventSlim())
{
 var old=s.Capture();var points=new ObservedPoints(2,i=>{if(i==0){entered.Set();if(!release.Wait(TimeSpan.FromSeconds(5)))throw new TimeoutException("Release append reader");}return new(i,1);});
 var append=Task.Run(()=>s.Append(old,new(points)));
 Task<(State.Revision Receipt,State.PreparedAppend Pending)>? competitor=null;
 try
 {
  Need(entered.Wait(TimeSpan.FromSeconds(5)),"Append entered point reader while exclusive");
  competitor=Task.Run(()=>{competitorStarted.Set();var current=s.Capture();return(current,s.PrepareAppend(current,new InkStroke(new[]{new InkPoint(7,8),new InkPoint(9,10)})));});
  Need(competitorStarted.Wait(TimeSpan.FromSeconds(5)),"Competing thread started");
  Need(!competitor.Wait(TimeSpan.FromMilliseconds(25)),"Competing prepare blocked by entire append transaction");
 }
 finally{release.Set();}
 Need(append.Wait(TimeSpan.FromSeconds(5)),"Append completes without successor cleanup exception");
 Need(competitor!.Wait(TimeSpan.FromSeconds(5)),"Successor prepare completes after append transaction");
 var next=competitor.Result;Need(ReferenceEquals(next.Receipt,append.Result),"Competitor sees committed receipt");
 Need(s.PreparedPointCount(next.Pending)==2,"Append finally cannot abandon successor token");
 var last=s.CommitPrepared(next.Pending);s.WithCurrent(last,b=>{Need(b.Sequence==2&&b.Count==4,"Concurrent successor token remains committable");return 0;});
}
Need(typeof(State).GetMethods(System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Where(x=>x.Name=="Append").All(x=>x.GetParameters().All(p=>p.ParameterType!=typeof(LegacyOwnedRawInkDocument))),"No foreign Document ingress");
Console.WriteLine($"PASS permanent raw-only {cases} frozen append fixtures/{controls} controls; no C51/INPUT/Next/public ownership/NN claim");
sealed class ObservedPoints(int count,Func<int,InkPoint> read):IReadOnlyList<InkPoint>{public int Reads;public int Count=>count;public InkPoint this[int index]{get{Reads++;return read(index);}}public IEnumerator<InkPoint> GetEnumerator(){for(int i=0;i<count;i++)yield return this[i];}System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()=>GetEnumerator();}
