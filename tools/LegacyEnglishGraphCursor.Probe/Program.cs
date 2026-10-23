using InkRecognition;
using System.Text.Json;
var ledger=LegacyEnglishCaseNode0Ledger.Create(128L*1024*1024);
var ring=new LegacyEnglishCaseNode0Ring(122,120,ledger,default);
ring.CurrentRows[0]=77;ring.CurrentRowMask[0]=255;ring.CurrentPrimary[0]=33;ring.CurrentPrimaryMask[0]=255;
ring.EnableGraphCursor(default);int refusals=0;
void Refuse(Action x){try{x();}catch(InvalidOperationException){refusals++;return;}throw new Exception("Expected exact invalid-state refusal.");}
Refuse(()=>ring.EnableGraphCursor(default));Refuse(()=>ring.EnterGraphNode(2,default));
for(int node=1;node<=121;node++){
 ring.EnterGraphNode(node,default);
 if(ring.CurrentRowMask[0]!=0||ring.CurrentPrimaryMask[0]!=0)throw new Exception("Alias promoted old known masks.");
 if(node==120&&ring.CurrentRows[0]!=77)throw new Exception("Native ring alias residue lost.");
 ring.CurrentRows[0]=(byte)node;ring.CurrentRowMask[0]=255;ring.CurrentPrimary[0]=(byte)(node+1);ring.CurrentPrimaryMask[0]=255;
 ring.ClearNode0Counters(156,125);
 if(ring.SourceEndAt(node)!=156||ring.RetainedEndAt(node)!=125)throw new Exception("Source and retained endpoint domains conflated.");
 if(node<120&&ring.OwnedRowsAt(0)[0]!=77)throw new Exception("Current node overwrote retained node0.");
}
Refuse(()=>{_=ring.OwnedRowsAt(0).Length;});Refuse(()=>{_=ring.OwnedRowMaskAt(1).Length;});Refuse(()=>ring.EnterGraphNode(122,default));
if(ring.OwnedRowsAt(120)[0]!=120||ring.OwnedRowsAt(121)[0]!=121||ring.CurrentNode!=121)throw new Exception("Live physical slot identity incorrect.");
var languageRing=new LegacyEnglishCaseNode0Ring(2,120,ledger,default);
void SetRow(int node,int span,int anchor,int anchorOrdinal,ulong tag){var r=languageRing.CurrentRows;r.Clear();System.Buffers.Binary.BinaryPrimitives.WriteUInt64LittleEndian(r,tag);System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(r[16..],1);r[32]=(byte)span;System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(r[56..],anchor);System.Buffers.Binary.BinaryPrimitives.WriteUInt16LittleEndian(r[60..],(ushort)anchorOrdinal);languageRing.CurrentRowMask.AsSpan(0,88).Fill(255);languageRing.ClearNode0Counters(125,125);}
SetRow(0,0,0,0,77);languageRing.EnableGraphCursor(default);languageRing.EnterGraphNode(1,default);
SetRow(1,1,1,0,99);var rootAnchor=LegacyEnglishCaseLanguageAnchor.Resolve(languageRing,1,0,default);if(!rootAnchor.IsFirst||rootAnchor.PreviousWordNode!=-1)throw new Exception("Whole-start FIRST lineage incorrect.");
SetRow(1,0,1,0,99);var laterAnchor=LegacyEnglishCaseLanguageAnchor.Resolve(languageRing,1,0,default);if(laterAnchor.IsFirst||laterAnchor.PreviousWordNode!=0)throw new Exception("Later language incorrectly substituted FIRST.");
SetRow(1,0,0,0,99);var previousAnchor=LegacyEnglishCaseLanguageAnchor.Resolve(languageRing,1,0,default);if(!previousAnchor.IsFirst||previousAnchor.AnchorTag!=77)throw new Exception("Anchor resolution used current row instead.");
// AnchorTag remains diagnostic only: actual current query tag is99, never77.
int maskRefusals=0;
foreach(int at in new[]{16,17,18,19,56,57,58,59,60,61}){languageRing.CurrentRowMask[at]=0;try{_=LegacyEnglishCaseLanguageAnchor.Resolve(languageRing,1,0,default);throw new Exception("Opaque current field admitted.");}catch(NotSupportedException){maskRefusals++;}finally{languageRing.CurrentRowMask[at]=255;}}
foreach(int at in new[]{0,1,2,3,4,5,6,7,16,17,18,19,32}){languageRing.OwnedRowMaskAt(0)[at]=0;try{_=LegacyEnglishCaseLanguageAnchor.Resolve(languageRing,1,0,default);throw new Exception("Opaque anchor field admitted.");}catch(NotSupportedException){maskRefusals++;}finally{languageRing.OwnedRowMaskAt(0)[at]=255;}}
int definednessCases=0;
foreach(int activeCount in new[]{0,1,124,125}){
 var alternatives=new byte[125*80];var rowsMask=new byte[125*88];var altMask=new byte[125*80];
 // All inactive data intentionallyzero, so naive finite-cost reads would fail.
 if(activeCount>0)System.Buffers.Binary.BinaryPrimitives.WriteInt32LittleEndian(alternatives.AsSpan(40),0x4000000);
 LegacyEnglishCaseGraphPrimaryDefinedness.Fill(activeCount,alternatives,rowsMask,altMask);
 for(int i=0;i<125;i++)for(int at=0;at<80;at++){
  byte expected=i>=activeCount?(byte)0:at<24||at>=40&&at<(i==0?44:64)?(byte)255:(byte)0;
  if(altMask[i*80+at]!=expected)throw new Exception("Unproduced inactive alternative was promotedknown.");
 }
 for(int i=0;i<125;i++)for(int at=0;at<88;at++)if(rowsMask[i*88+at]!=(i<activeCount||at>=16&&at<24?(byte)255:(byte)0))throw new Exception("Primary row mask differsfrom actualproducer stores.");
 definednessCases++;
}
Console.WriteLine(JsonSerializer.Serialize(new{Scope="PREPARED generic-ring physical ownership only; no model/NN",Nodes=122,Transitions=121,Refusals=refusals,RetainedWindow=120,SourceEnd=156,RetainedEnd=125,FirstWriter=FirstWriterControls.Run(),DefinednessCases=definednessCases,LanguageAnchorControls=3,LanguageMaskRefusals=maskRefusals,NNCalls=0}));
