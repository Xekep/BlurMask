using InkRecognition;
using System.Text.Json;
using System.Buffers.Binary;
if(args.Length!=1||File.Exists(args[0]))throw new ArgumentException("fresh prepared capture");
var flags=new uint[]{0,1,0x100000,0x200000,0x800000,0x1000000,0x80000000,uint.MaxValue};
var folded=new List<object>();foreach(uint prior in flags)foreach(uint def in flags)foreach(uint marker in flags)
    folded.Add(new{Prior=prior,Default=def,Marker=marker,Result=LegacyFreshMainGlobalWordSourceChoice.FoldFlags(prior,def,marker)});
var wordFlags=new List<object>();foreach(uint prior in flags)foreach(uint a in flags)foreach(uint b in flags)
{
 var input=new[]{new LegacyEnglishCaseNode0Completion.GraphWordChoiceData(0,a,[],[],[],[]),new LegacyEnglishCaseNode0Completion.GraphWordChoiceData(1,b,[],[],[],[])};
 wordFlags.Add(new{Prior=prior,A=a,B=b,Result=LegacyEnglishCaseNode0Completion.FoldGraphWordFlags(prior,input)});
}
var rows=new List<object>();foreach(int count in new[]{0,1,2,128,4096})foreach(uint flag in flags)
    rows.Add(new{Count=count,Flags=flag,Scalar48=LegacyFreshMainGlobalWordSourceChoice.EncodeScalar48(flag,count)});
var document=LegacyOwnedRawInkDocument.Create(new[]{new InkStroke(new InkPoint[]{new(0,0),new(7,40),new(14,0)})},default);
var selections=LegacyOwnedFreshTextSourceContext.Prepare(document);var chars=LegacyOwnedMainTextSourceInterner.Prepare(document);var words=LegacyOwnedMainTextSourceInterner.Prepare(document);
var key=new[]{new LegacyNativeInkPositionRange(new(3),new(196357))};
//EXPLICIT neutral prepared tree/registration chronology; no live EN owner.
var word=words.InternPrepared(key);if(!word.Inserted)throw new InvalidDataException();var trace=new List<string>{"word-intern"};
var ids=new List<int>();for(int choice=0;choice<2;choice++)
{
 trace.Add("word-choice48:"+choice);var item=chars.InternPrepared(key);ids.Add(item.Node.Identity);trace.Add(item.Inserted?"char-intern:new":"char-intern:reuse");
 if(item.Inserted){long id=-1;if(selections.CreatePrepared(key,0,ref id)is null)throw new InvalidDataException();}
 trace.Add("word-choice-complete:"+choice);
}
long wordId=long.MaxValue;if(selections.CreatePrepared(key,1,ref wordId)is null)throw new InvalidDataException();trace.Add("word-register");
int refused=0;foreach(int count in new[]{-1,4097,int.MaxValue})
{try{_=LegacyFreshMainGlobalWordSourceChoice.EncodeScalar48(0,count);}catch(ArgumentOutOfRangeException){refused++;}}
using(var cancel=new CancellationTokenSource()){cancel.Cancel();try{chars.InternPrepared(key,cancel.Token);}catch(OperationCanceledException e)when(e.CancellationToken==cancel.Token){refused++;}}
var pending=selections.CopyPendingSelections().Select(x=>new{x.SelectionId,x.Name,x.CommandIdentity,x.PreviousCommandIdentity,Pairs=x.CopyRanges().SelectMany(r=>new[]{r.First.Value,r.Last.Value}).ToArray()}).ToArray();
File.WriteAllBytes(args[0],JsonSerializer.SerializeToUtf8Bytes(new{Scope="PREPARED_NEUTRAL_WORD_SCALARS_REGISTRATION_NOT_LIVE_EN",Folds=folded,WordFlags=wordFlags,Rows=rows,CharacterNodeIds=ids,WordNodeId=word.Node.Identity,CharTree=chars.CopyTree(),WordTree=words.CopyTree(),Pending=pending,Trace=trace,Next=selections.NextSelectionId,GuardRefusals=refused,NN=false,LiveOwnerExecuted=false,TEXTCreated=false},new JsonSerializerOptions{WriteIndented=true}));
