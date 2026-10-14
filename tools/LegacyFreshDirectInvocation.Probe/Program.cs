using InkRecognition;
using System.Buffers.Binary;
if(args.Length!=1)throw new ArgumentException("Usage: LegacyFreshDirectInvocation.Probe owned-ru-ak.res");
var bytes=File.ReadAllBytes(args[0]);int fixtures=0;
foreach((float scale,float dx,float dy) in new[]{(1f,0f,0f),(.7f,150f,-50f),(1.75f,-100f,200f)})
{
 InkStroke[] raw=[Stroke((0,90),(0,10),(25,60),(50,10),(50,90)),Stroke((65,90),(65,10)),Stroke((65,90),(115,10)),Stroke((115,90),(115,10)),Stroke((130,90),(130,10),(175,10),(185,20),(185,45),(175,55),(130,55))];
 var transformed=raw.Select(s=>new InkStroke(s.Points.Select(p=>new InkPoint(p.X*scale+dx,p.Y*scale+dy)).ToArray())).ToArray();
 var frame=LegacyFreshOrdinaryPacketFrame.PrepareFirstSource(bytes,6120,transformed);var policy=LegacyFreshDirectFirstInvocationPolicy.Create(bytes,6120,frame);
 if(!ReferenceEquals(frame.Packet.Source,frame.Row.OriginalSource)||policy.SourceFrameFlag150||policy.Invocation.ConfigurationA8||policy.Invocation.Configuration99||policy.Invocation.NativeModeE0||policy.Binding.SourceKind!=frame.Packet.Source.Profile.SourceKind||frame.Descriptor.CurrentLineId!=0)throw new Exception("Owned fresh direct ingress/frame/first invocation policy differs.");
 //A different typed source-kind is a valid policy route but cannot silently
 //take ownership of this already-prepared source135 frame.
 var foreign=(byte[])bytes.Clone();int kind=6120+99+policy.Binding.Alphabet.BytesConsumed;BinaryPrimitives.WriteInt32LittleEndian(foreign.AsSpan(kind),0x137);
 try{LegacyFreshDirectFirstInvocationPolicy.Create(foreign,6120,frame);throw new Exception("Foreign typed source frame accepted.");}catch(ArgumentException){}
 using var cancellation=new CancellationTokenSource();cancellation.Cancel();try{LegacyFreshDirectFirstInvocationPolicy.Create(bytes,6120,frame,cancellation.Token);throw new Exception("Fresh direct invocation cancellation ignored.");}catch(OperationCanceledException){}
 //The general invocation helper still retains an explicitly supplied override;
 //the zero applies only to the direct factory's no-property-event scope.
 if(!policy.Binding.ForInvocation(true).ConfigurationA8)throw new Exception("General explicit source property override was erased.");
 fixtures++;
}
Console.WriteLine($"PASS {fixtures} actual raw/direct-source/24a/265 packet frames with scoped first invocation150/A8, foreign source ownership, explicit override preservation and cancellation. No reused/custom source default inferred.");
static InkStroke Stroke(params(float X,float Y)[] points)=>new(points.Select(p=>new InkPoint(p.X,p.Y)).ToArray());
