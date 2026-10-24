using InkRecognition;
using System.Buffers.Binary;
using System.Text.Json;
if(args.Length!=1||File.Exists(args[0]))throw new ArgumentException("fresh prepared capture");
var nulls=new List<object>();int[][] countsCases=[[1],[3],[3,2],[1,2,1],[5,3,7,2],[1,1,1,1],[32,1,31]];
foreach(var counts in countsCases)
{
 var doc=LegacyOwnedRawInkDocument.Create(counts.Select((n,s)=>new InkStroke(Enumerable.Range(0,n).Select(i=>new InkPoint(s*10+i,i)).ToArray())).ToArray(),default);
 var inputs=doc.CopySelectedInputs();var op=LegacyOwnedRawInkRangeOperations.Prepare(doc);LegacyNativeInkPositionRange[] ranges=[];
 foreach(var input in inputs){var r=LegacyNativeTextSourceRangeProjection.Project(input);if(r.First.Mode!=4)ranges=op.Union(ranges,[r]);}
 var empty=new LegacyNativeTextSourceInputMap(inputs,[],0).ProjectNormalReferences([]);
 nulls.Add(new{Counts=counts,Selected=inputs,NullPairs=ranges.SelectMany(r=>new[]{r.First.Value,r.Last.Value}).ToArray(),NonNullEmptyCount=empty.Length});
}
var guides=new List<object>();var lines=new List<object>();
foreach(int corrected in new[]{0,1})
{
 byte[] raw=new byte[40];void F(int o,float v)=>BinaryPrimitives.WriteSingleLittleEndian(raw.AsSpan(o),v);
 F(0,12345);F(4,6789);F(8,corrected==0?0:.75f);F(12,40);F(16,12);F(20,60);F(24,corrected==0?0:8);
 var g=LegacyEnglishCaseNode0Completion.ReadGraphTextGuide(raw);guides.Add(new{Corrected=corrected,SourceGuide40=raw,FieldBits=new[]{Bits(g.Slope),Bits(g.Baseline),Bits(g.Height),Bits(g.CorrectedBaseline),Bits(g.CorrectedHeight)}});
 var pub=LegacyNativeSourceLinePublication.Append([],0,[new(0,0)],[g],0);
 lines.Add(new{Corrected=corrected,Next=pub.NextWrapperLineOffset,Full56=LegacyFreshMainSourceLinePublication.Encode(pub.Lines)});
}
var masks=new[]{new{Count=24,Pointer=8,Bytes=LegacyEnglishCaseNode0Completion.KnownWithPointer(24,8)},new{Count=32,Pointer=16,Bytes=LegacyEnglishCaseNode0Completion.KnownWithPointer(32,16)},new{Count=48,Pointer=16,Bytes=LegacyEnglishCaseNode0Completion.KnownWithPointer(48,16)}};
int guards=0;foreach(int length in new[]{0,39,41})try{_=LegacyEnglishCaseNode0Completion.ReadGraphTextGuide(new byte[length]);}catch(ArgumentException){guards++;}
foreach(var a in new[]{(-1,8),(24,16),(32,8),(48,0)})try{_=LegacyEnglishCaseNode0Completion.KnownWithPointer(a.Item1,a.Item2);}catch(ArgumentException){guards++;}
var d=LegacyOwnedRawInkDocument.Create([new InkStroke([new(0,0),new(7,40),new(14,0)])],default);var owner=LegacyOwnedFreshTextSourceContext.Prepare(d);var key=new[]{new LegacyNativeInkPositionRange(new(3),new(196357))};
foreach(int kind in new[]{0,1,2}){long output=-1;if(owner.CreatePrepared(key,kind,ref output)is null)throw new InvalidDataException();}
var pending=owner.CopyPendingSelections().Select(c=>new{c.SelectionId,c.Name,c.CommandIdentity,c.PreviousCommandIdentity,c.OperationCode,c.Enabled,c.Category,Pairs=c.CopyRanges().SelectMany(r=>new[]{r.First.Value,r.Last.Value}).ToArray()}).ToArray();
File.WriteAllBytes(args[0],JsonSerializer.SerializeToUtf8Bytes(new{Scope="PREPARED_RAW_NULL_GUIDE_LINE_MASKS_NOT_LIVE_EN_TEXT",Nulls=nulls,Guides=guides,Lines=lines,Masks=masks,WordLinkKnown24=LegacyEnglishCaseNode0Completion.KnownWordLink24(),Pending=pending,Next=owner.NextSelectionId,Guards=guards,NN=false,OwningTEXTExecuted=false,AcceptedText=false},new JsonSerializerOptions{WriteIndented=true}));
static uint Bits(float x)=>BitConverter.SingleToUInt32Bits(x);
