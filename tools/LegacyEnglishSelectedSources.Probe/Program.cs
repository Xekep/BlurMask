using InkRecognition;
using System.Text.Json;
using System.Reflection;
using System.Reflection.Emit;
using System.Buffers.Binary;
if(args.Length!=1||File.Exists(args[0]))throw new ArgumentException("Fresh model-free output required.");
var stores=new List<object>(640);int[] markers=[int.MinValue,-65536,-1,0,1,127,128,4095,65535,int.MaxValue];
for(int seed=0;seed<64;seed++)foreach(int marker in markers)
{
 byte[] before=Enumerable.Range(0,8).Select(i=>(byte)((seed*13+i*17)&255)).ToArray();
 byte[] known=Enumerable.Range(0,8).Select(i=>(byte)((seed>>i&1)!=0?255:0)).ToArray();
 var after=before.ToArray();var afterKnown=known.ToArray();
 LegacyEnglishCaseNode0Completion.WriteSelectedIndex44(after,afterKnown,marker);
 stores.Add(new{Seed=seed,Index=marker,Before=before,KnownBefore=known,After=after,KnownAfter=afterKnown});
}
var order=new List<object>();
foreach(int[] ids in new[]{new[]{1,0,1,1},new[]{0,0,0},new[]{2,1,0},Array.Empty<int>()})
{
 uint[] flags=[0,0x01004000,0x10000000];int[] values=[-1,-1,-1];int[] marks=ids.Select((x,i)=>i==1?-1:i+7).ToArray();
 LegacyFreshMainDocumentSelectedSources.ApplySelectedCharacters(flags,values,ids,marks);
 order.Add(new{Ids=ids,Markers=marks,Flags=flags,Indices=values});
}
var folds=new List<object>();
for(int seed=0;seed<128;seed++)
{
 uint prior=(uint)(seed*0x0010101);uint[] metadata=Enumerable.Range(0,3).Select(i=>(seed>>i&1)!=0?0x11004000u:0u).ToArray();
 uint actual=LegacyFreshMainGlobalDocumentSelectedSources.ComputeWordMetadata(prior,metadata);
 folds.Add(new{Prior=prior,Metadata=metadata,Actual=actual});
}
var snapshots=new List<object>();int copies=0;
foreach(int count in new[]{3,4})
{
 var doc=LegacyOwnedRawInkDocument.Create([new InkStroke(Enumerable.Range(0,count).Select(i=>new InkPoint(i,i*2)).ToArray())],default);
 var view=LegacyFreshRawInkCurrentView.Prepare(doc);var original=LegacyLibraryOriginalTextFieldBootstrap.Prepare(view,["english-field"]);var os=original.Run();
 var selected=LegacyLibraryOriginalSelectedFieldContext.Prepare(original,os.Inputs[0]);var ss=selected.Run();
 var raw=ss.CopyRawStorage();byte old=raw[0][0];raw[0][0]^=255;if(ss.CopyRawStorage()[0][0]!=old)throw new InvalidDataException("Raw snapshot aliases.");copies++;
 var bytes=os.Nodes.Single(n=>n.Name=="id").CopyValueBytes();bytes[0]^=255;if(os.Nodes.Single(n=>n.Name=="id").CopyValueBytes()[0]!=(byte)'e')throw new InvalidDataException("DOM snapshot aliases.");copies++;
 var curve=view.Curves[0];var entry=view.Views[0];var desc=entry.Descriptor;
 if(!ReferenceEquals(selected.CurrentView,view)||!ReferenceEquals(view.Document,doc)||!ReferenceEquals(entry.Curve,curve))throw new InvalidDataException("Real model-free raw/selected ownership.");
 snapshots.Add(new{Count=count,ConcreteType=curve.NativeConcreteType,SourceIndex=curve.OriginalSourceIndex,ViewIndex=entry.CurrentViewIndex,
  desc.FirstPoint,desc.LastPoint,desc.FirstFraction,desc.LastFraction,desc.FirstClosed,desc.LastClosed,desc.IsKnownText,
  OriginalDOM=os.Nodes.Count,SelectedDOM=ss.Nodes.Count,OriginalNext=os.NextRegistryId,SelectedNext=ss.NextRegistryId,
  NoGraphParentIssued=true,ReportedKindQueried=false});
}
var refusals=new List<string>();
Refuse("field-short",()=>LegacyEnglishCaseNode0Completion.WriteSelectedIndex44(new byte[7],new byte[8],0));
Refuse("field-long",()=>LegacyEnglishCaseNode0Completion.WriteSelectedIndex44(new byte[9],new byte[8],0));
Refuse("mask-short",()=>LegacyEnglishCaseNode0Completion.WriteSelectedIndex44(new byte[8],new byte[7],0));
Refuse("mask-long",()=>LegacyEnglishCaseNode0Completion.WriteSelectedIndex44(new byte[8],new byte[9],0));
byte[] same=new byte[8];Refuse("same-storage",()=>LegacyEnglishCaseNode0Completion.WriteSelectedIndex44(same,same,0));
byte[] partial=new byte[12];Refuse("overlap-storage",()=>LegacyEnglishCaseNode0Completion.WriteSelectedIndex44(partial.AsSpan(0,8),partial.AsSpan(4,8),0));
//Read-only own-library IL classification: no GraphParent/constructor/method
//invocation, and no model/NN. This checks the REAL compiled private body.
var chronology=InspectChronology();
File.WriteAllBytes(args[0],JsonSerializer.SerializeToUtf8Bytes(new{Scope="MODEL_FREE_DWORD44_SHARED_SCALARS_REAL_BOOTSTRAP_SNAPSHOTS_NOT_GRAPH_OWNER",Stores=stores,SharedNumericOrders=order,SharedNumericFolds=folds,RawSnapshots=snapshots,DefensiveCopies=copies,Refusals=refusals,ChronologyIL=chronology,ENOwningExecuted=false,NN=false,DocumentApplied=false}));
void Refuse(string name,Action a){try{a();}catch(Exception e)when(e.GetType()==typeof(ArgumentException)){refusals.Add(name);return;}throw new InvalidDataException("Exact ArgumentException required: "+name);}

static object InspectChronology()
{
 var type=typeof(LegacyEnglishCaseNode0Completion).GetNestedType("GraphWordPublicationContext",BindingFlags.NonPublic)!;
 var method=type.GetMethod("CompleteSelectedSources",BindingFlags.Instance|BindingFlags.NonPublic)!;
 byte[] il=method.GetMethodBody()!.GetILAsByteArray()!;
 var ops=typeof(OpCodes).GetFields(BindingFlags.Public|BindingFlags.Static).Where(f=>f.FieldType==typeof(OpCode))
  .Select(f=>(OpCode)f.GetValue(null)!).ToDictionary(o=>unchecked((ushort)o.Value));
 var calls=new List<int>();var stores=new List<int>();var back=new List<(int Target,int Offset)>();
 int pos=0;
 while(pos<il.Length)
 {
  int offset=pos;ushort code=il[pos++];if(code==0xfe)code=(ushort)(0xfe00|il[pos++]);var op=ops[code];int start=pos;
  int size=op.OperandType switch{OperandType.InlineNone=>0,OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar=>1,
   OperandType.InlineVar=>2,OperandType.InlineI8 or OperandType.InlineR=>8,OperandType.InlineSwitch=>checked(4+4*BinaryPrimitives.ReadInt32LittleEndian(il.AsSpan(pos))),_=>4};
  if(pos+size>il.Length)throw new InvalidDataException("Truncated own IL.");
  if(op.OperandType==OperandType.InlineMethod)
  {
   var target=method.Module.ResolveMethod(BinaryPrimitives.ReadInt32LittleEndian(il.AsSpan(start)))!;
   if(target.Name=="WriteSelectedIndex44"&&target.DeclaringType==typeof(LegacyEnglishCaseNode0Completion))calls.Add(offset);
  }
  if(op==OpCodes.Stfld)
  {var field=method.Module.ResolveField(BinaryPrimitives.ReadInt32LittleEndian(il.AsSpan(start)))!;if(field.Name=="flags"&&field.DeclaringType==type)stores.Add(offset);}
  if(op.OperandType is OperandType.InlineBrTarget or OperandType.ShortInlineBrTarget)
  {int target=pos+size+(size==1?(sbyte)il[pos]:BinaryPrimitives.ReadInt32LittleEndian(il.AsSpan(pos)));if(target<=offset)back.Add((target,offset));}
  pos+=size;
 }
 if(calls.Count!=2||stores.Count!=2||!back.Any(b=>b.Target<=calls[0]&&calls[0]<b.Offset)||
  back.Any(b=>b.Target<=calls[1]&&calls[1]<b.Offset)||stores[0]>=calls[0]||stores[1]<=calls[0]||stores[1]>=calls[1]||
  !back.Any(b=>b.Target<=calls[0]&&calls[0]<b.Offset&&b.Offset<stores[1]))
  throw new InvalidDataException("Actual private body must fold WORDmetadata, complete CHAR loop, then WORDOR8/index44.");
 return new{Scope="READONLY_OWN_LIBRARY_IL_NOT_PRIVATE_METHOD_EXECUTION",IndexWriteCalls=2,WordFlagsStores=2,WordMetadataBeforeCharLoop=true,
  FirstIndexInsideCharBackedge=true,WordOrAndIndexAfterCharBackedge=true,CompleteSelectedSourcesInvoked=false,GraphParentConstructed=false};
}
