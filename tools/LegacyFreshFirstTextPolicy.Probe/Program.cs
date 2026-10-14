using InkRecognition;
using System.Buffers.Binary;
if(args.Length!=1)throw new ArgumentException("Usage: LegacyFreshFirstTextPolicy.Probe owned-ru-ak.res");
var bytes=File.ReadAllBytes(args[0]);var policy=LegacyFreshFirstTextContextPolicy.ReadForFreshSelectedChild(bytes,6120);
if(policy.SourceKind!=0x135||policy.NativeModeE0||policy.OwnerFlagC4||policy.AliasFold||policy.Configuration99||policy.ConfigurationA8AtBinding||policy.TrailingSymbolE6!=ushort.MaxValue||policy.Alphabet.LabelCount!=192||policy.ShortlistColumnsA0!=1||policy.ContextColumnsA2!=1||policy.MergedColumnsA4!=1)throw new Exception("Pinned actual selected-child binding policy differs from constructor/binder and source/alphabet evidence.");
//158970 overwrites bindingA8 from source-frame150 before invocation. Its value
//must be supplied from that frame; the constructor/binder is not its producer.
if(!policy.ForInvocation(true).ConfigurationA8||policy.ForInvocation(false).ConfigurationA8||policy.ForInvocation(true).Configuration99||policy.ForInvocation(true).NativeModeE0)throw new Exception("Required actual frame150 invocation override was lost.");
//Preserve the same model/alphabet, changing ONLY the serialized source-kind
//field. This tests the independent native137 route, rather than a fake label
//normalization rule or a caller bool that bypasses typed ownership.
int alphabetOffset=6120+99,kindOffset=alphabetOffset+policy.Alphabet.BytesConsumed;
if(BinaryPrimitives.ReadInt32LittleEndian(bytes.AsSpan(kindOffset))!=0x135)throw new Exception("Owned source-kind boundary does not follow inline alphabet.");
var kind137=(byte[])bytes.Clone();BinaryPrimitives.WriteInt32LittleEndian(kind137.AsSpan(kindOffset),0x137);
var byKind=LegacyFreshFirstTextContextPolicy.ReadForFreshSelectedChild(kind137,6120);
if(!byKind.NativeModeE0||byKind.SourceKind!=0x137||byKind.Alphabet.LabelCount!=192)throw new Exception("Native137 E0 branch lost typed source ownership.");
//Replace one same-size original supplementary symbol inside the inline
//alphabet. Reverse-label offsets and all network bytes remain unchanged.
var bySymbolBytes=(byte[])bytes.Clone();ReadOnlySpan<byte> oldSymbol=[0xf3,0xb0,0x80,0x81];ReadOnlySpan<byte> newSymbol=[0xf3,0xb0,0x80,0x89];
int relative=bySymbolBytes.AsSpan(alphabetOffset,policy.Alphabet.BytesConsumed).IndexOf(oldSymbol);if(relative<0)throw new Exception("Fixture's ownedF0001 supplementary symbol missing.");
newSymbol.CopyTo(bySymbolBytes.AsSpan(alphabetOffset+relative));var bySymbol=LegacyFreshFirstTextContextPolicy.ReadForFreshSelectedChild(bySymbolBytes,6120);
if(!bySymbol.NativeModeE0||bySymbol.SourceKind!=0x135||!bySymbol.Alphabet.TryGetLabel("\U000f0009",out _)||policy.Alphabet.TryGetLabel("\U000f0009",out _))throw new Exception("ExactF0009 E0 branch or immutable original alphabet ownership differs.");
var invalid=(byte[])bytes.Clone();BinaryPrimitives.WriteInt32LittleEndian(invalid.AsSpan(kindOffset),0x999);
try{LegacyFreshFirstTextContextPolicy.ReadForFreshSelectedChild(invalid,6120);throw new Exception("Unsupported source type admitted.");}catch(InvalidDataException){}
try{LegacyFreshFirstTextContextPolicy.ReadForFreshSelectedChild(bytes.AsSpan(0,12000),6120);throw new Exception("Truncated owner admitted.");}catch(InvalidDataException){}
using var cancellation=new CancellationTokenSource();cancellation.Cancel();try{LegacyFreshFirstTextContextPolicy.ReadForFreshSelectedChild(bytes,6120,cancellation.Token);throw new Exception("Policy cancellation ignored.");}catch(OperationCanceledException){}
Console.WriteLine("PASS owned firstRU selected-child135/E0=false, controlled typed137 route, exactF0009 alphabet route, original snapshot ownership, malformed owner and cancellation. No external/reused config or automatic fullRussian recognition claim.");
