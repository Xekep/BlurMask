"""Independent actual raw model -> all V58 record bytes and lexical IDs.
No production decoder is imported; tuple-table suffix scoring and DFS IDs.
Prepared raw histories are explicitly separate from public lexical receipts.
"""
from pathlib import Path
import argparse,hashlib,json,runpy,struct,sys
p=argparse.ArgumentParser();p.add_argument('--repo',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--capture',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
out=a.output.parent;out.mkdir(parents=True,exist_ok=True);prefix='ev58-'+hashlib.sha256(str(a.output.resolve()).encode()).hexdigest()[:12]
def old_reference(relative,suffix):
 sys.argv=['reference',str(a.models),str(out/(prefix+suffix+'.json'))]
 return runpy.run_path(str(a.repo/relative),run_name='__main__')
s=old_reference('tools/LegacyEnglishLkSparse101.Probe/reference_actual.py','-sparse')
d=old_reference('tools/LegacyEnglishLkPacked102.Probe/reference_actual.py','-dictionary')
arrays=s['arrays'];raws=s['raws'];classes=s['classCompiled'];backoff=s['backoffCompiled'];words=s['wordCompiled'];boundary=s['compiled']['boundary94C']
offsets=[arrays['B8.0'],arrays['B8.1']];labels=[arrays['B0.1'],arrays['B0.2']]
cost={};backs={};prefixes=[]
for h in range(1024):
 cost[(h,)]=classes[0][raws['C0.0'][h]];backs[(h,)]=backoff[0][raws['C8.0'][h]]
 for j in range(offsets[0][h],offsets[0][h+1]):
  pair=(h,labels[0][j]);prefixes.append((pair,j));cost[pair]=classes[1][raws['C0.1'][j]];backs[pair]=backoff[1][raws['C8.1'][j]]
  for k in range(offsets[1][j],offsets[1][j+1]):cost[pair+(labels[1][k],)]=classes[2][raws['C0.2'][k]]
def numerical(history,current,word,b):
 seq=tuple(history)+(current,);penalty=0
 for i in range(len(seq)):
  suffix=seq[i:]
  if suffix in cost:return penalty+cost[suffix]+words[raws['98.wordCodes'][word]]+(boundary if b else 0),len(suffix),(tuple(history)+(current,))[-2:]
  prefix=suffix[:-1]
  if prefix in backs:penalty+=backs[prefix]
 raise AssertionError('total unigram')
def record(history,current,word,b,index):
 value,order,retained=numerical(history,current,word,b)
 result=bytearray((index*17+j*29+0x5b)&255 for j in range(40));struct.pack_into('<iiB',result,0,value,order,len(retained))
 for n,v in enumerate(retained):struct.pack_into('<I',result,12+n*4,v)
 struct.pack_into('<ii',result,24,-1,word)
 written=set(range(9))|set(range(24,32))|set(range(12,12+len(retained)*4))
 mask=sum(1<<n for n in written)
 return bytes(result),value,order,retained,mask
report=json.loads((a.capture/'capture.json').read_bytes());assert report['Scope']=='independent-ordinary-lexical-terminal/suppress0/category0/no-option/no-trace'
assert report['PreparedNumericControls']==643868 and report['Terminals']==260074 and report['PublicLexicalFirst']==260074 and report['PublicLexicalContinuations']==4096
assert report['ParallelQueries']==4096 and report['Guards']==37 and report['CanceledPreparations']==6 and report['CanceledQueries']==4
budget=666547*4+644893+1536*4+(104*105//2+16)*4+105*32+2*257*4+64*1024
assert report['AdditionalLogicalAllocationBytes']==budget==3410081 and not report['TargetsRead'] and not report['VendorExecuted']
for name,digest in report['Files'].items():assert hashlib.sha256((a.capture/name).read_bytes()).hexdigest()==digest
rawDigest=hashlib.sha256();controls=0;orders=[0]*3;kinds=[0]*5
with (a.capture/'prepared-records.bin').open('rb')as stream:
 def check(kind,history,current,word=None):
  global controls
  expected,value,order,retained,mask=record(history,current,0 if word is None else word,False,controls)
  assert stream.read(40)==expected,('full prepared40B',controls,history,current,word)
  baseValue=value-(words[raws['98.wordCodes'][0]]if word is None else 0)
  rawDigest.update(struct.pack('<8i',len(history),*(tuple(history)+(0,0))[:2],current,-1 if word is None else word,baseValue,order,len(retained)))
  controls+=1;orders[order-1]+=1;kinds[kind]+=1
 def missing(level,lo,hi):
  present=set(labels[level][lo:hi]);return next(c for c in range(1024)if c not in present)
 for word,current in enumerate(arrays['88'][:260074]):check(0,(0,),current,word)
 for pair,j in prefixes:
  check(1,pair[:-1],pair[-1]);lo,hi=offsets[1][j:j+2]
  for k in range(lo,hi):check(2,pair,labels[1][k])
  check(3,pair,missing(1,lo,hi))
 for h in range(1024):check(4,(h,),missing(0,offsets[0][h],offsets[0][h+1]))
 assert not stream.read(1)
assert controls==643868 and rawDigest.hexdigest()==report['PreparedClassDigest']=='1cad8793f3a43641a912f581be810347aa1b0bba066cc0f86fba7b820f223ebe'
assert orders==report['MatchedOrders']==[248913,239471,155484]and kinds==report['ControlKinds']==[260074,113643,155484,113643,1024]
#Independent node->terminal ordinal: ordinary ordered DFS, no C# WordIndex call.
nodeByWord=[];stack=[0]
while stack:
 node=stack.pop()
 if d['labels'][node]&128:nodeByWord.append(node)
 stack.extend(reversed(range(d['starts'][node],d['starts'][node]+d['counts'][node])))
assert len(nodeByWord)==260074
def public(stream,word,b,index,history):
 node,w,bound=struct.unpack('<iii',stream.read(12));assert(node,w,bound)==(nodeByWord[word],word,int(b))
 token=bytearray(16);struct.pack_into('<I',token,4,word);token[12]=int(b);assert stream.read(16)==token
 expected,_,_,retained,_=record(history,arrays['88'][word],word,b,index);assert stream.read(40)==expected,('public lexical full40B',index,word,history)
 return retained
with(a.capture/'lexical-first-records.bin').open('rb')as stream:
 for word in range(260074):public(stream,word,word&1,word,(0,))
 assert not stream.read(1)
with(a.capture/'lexical-continuation-records.bin').open('rb')as stream:
 history=(0,arrays['88'][0])
 for index in range(4096):history=public(stream,(index*61+7)%260074,index&1,index,history)
 assert not stream.read(1)
empty=record((),0,0,False,73);assert report['PreparedEmptyHistoryRecord']==empty[0].hex().upper()and report['PreparedEmptyHistoryMask']==empty[4]
result=dict(status='PASS independent full actual EN V58 record/source/ordinal oracle',preparedRecords=643868,publicLexicalFirstRecords=260074,publicLexicalContinuationRecords=4096,fullRecordBytes=40,sourceTokenBytes=16,allUnwrittenAllocationResidue=True,preparedEmptyHistorySeparate=True,independentTerminalPreorder=True,additionalLogicalBytes=budget,guards=report['Guards'],targetsRead=False,vendorExecuted=False,handwritingSelectionClaim=False)
a.output.write_text(json.dumps(result,indent=2)+'\n');print('PASS EN V58 all643868 prepared40B +260074 lexicalFIRST +4096 continuations +actual terminal ordinals/residue')
