"""Independent prepared NULL-selected/guide/mask/pending-command checks; no owning TEXT proof."""
import ast,base64,copy,hashlib,json,struct,sys
from pathlib import Path

def check(c,reference):
 tree=ast.parse(reference.read_bytes());g={'MASK':2**64-1}
 exec(compile(ast.Module([n for n in tree.body if isinstance(n,ast.FunctionDef)],[]),str(reference),'exec'),g)
 def exact(a,b):
  assert type(a)is type(b),(type(a),type(b))
  if isinstance(b,dict):
   assert a.keys()==b.keys()
   for k in b:exact(a[k],b[k])
  elif isinstance(b,list):
   assert len(a)==len(b)
   for x,y in zip(a,b,strict=True):exact(x,y)
  else:assert a==b,(a,b)
 def b64(b):return base64.b64encode(b).decode()
 nulls=[]
 for counts in [[1],[3],[3,2],[1,2,1],[5,3,7,2],[1,1,1,1],[32,1,31]]:
  selected=[];ranges=[]
  for i,n in enumerate(counts):
   selected.append(dict(SourceIndex=i,PointCount=n,FirstPoint=0,LastPoint=-1,FirstFraction=0,LastFraction=0,FirstClosed=True,LastClosed=True,IsKnownText=False))
   ranges=g['union'](ranges,[[(i<<32)|(0xff00 if n==1 else 0)|3,(i<<32)|((n-1)<<16)|0xff00|5]])
  nulls.append(dict(Counts=counts,Selected=selected,NullPairs=[x for r in ranges for x in r],NonNullEmptyCount=0))
 guides=[];lines=[]
 for corrected in [0,1]:
  raw=bytearray(40)
  vals=[0 if corrected==0 else .75,40,12,60,0 if corrected==0 else 8]
  for o,v in zip([0,4,8,12,16,20,24],[12345,6789]+vals,strict=True):struct.pack_into('<f',raw,o,v)
  guides.append(dict(Corrected=corrected,SourceGuide40=b64(raw),FieldBits=[struct.unpack('<I',struct.pack('<f',x))[0] for x in vals]))
  line=bytearray(56)
  for o,v in [(8,-0.0),(16,40 if corrected==0 else 60),(24,0.0),(32,12 if corrected==0 else 8),(48,40)]:struct.pack_into('<d',line,o,v)
  struct.pack_into('<q',line,40,-1)
  lines.append(dict(Corrected=corrected,Next=1,Full56=b64(line)))
 masks=[]
 for n,p in [(24,8),(32,16),(48,16)]:
  m=bytearray([255]*n);m[p:p+8]=bytes(8);masks.append(dict(Count=n,Pointer=p,Bytes=b64(m)))
 pending=[dict(SelectionId=i,Name=name,CommandIdentity=i,PreviousCommandIdentity=i-1,OperationCode=4,Enabled=1,Category=100,Pairs=[3,196357])for i,name in enumerate(['CHAR','WORD','TEXT'],1)]
 expected=dict(Scope='PREPARED_RAW_NULL_GUIDE_LINE_MASKS_NOT_LIVE_EN_TEXT',Nulls=nulls,Guides=guides,Lines=lines,Masks=masks,WordLinkKnown24=b64(bytes([255]*8+[0]*8+[255]*5+[0]*3)),Pending=pending,Next=4,Guards=7,NN=False,OwningTEXTExecuted=False,AcceptedText=False)
 exact(c,expected)

def main():
 capture,reference,output=map(Path,sys.argv[1:4]);c=json.loads(capture.read_bytes())
 assert hashlib.sha256(reference.read_bytes()).hexdigest()=='dc5e998a42e4dcedbfda714d97dd58d06136af608c5f4aa5bc03bf70b89fb750'
 check(c,reference);mutations=[]
 def add(name,fn):
  q=copy.deepcopy(c);fn(q);mutations.append((name,q))
 add('null-key',lambda q:q['Nulls'][0]['NullPairs'].__setitem__(0,3))
 add('empty-is-not-null',lambda q:q['Nulls'][1].__setitem__('NonNullEmptyCount',1))
 add('source-index-bool',lambda q:q['Nulls'][0]['Selected'][0].__setitem__('SourceIndex',False))
 add('point-count-float',lambda q:q['Nulls'][0]['Selected'][0].__setitem__('PointCount',1.0))
 add('mode-known-text',lambda q:q['Nulls'][0]['Selected'][0].__setitem__('IsKnownText',True))
 add('guide-offset',lambda q:q['Guides'][0]['FieldBits'].__setitem__(0,12345))
 add('corrected-height',lambda q:q['Guides'][1]['FieldBits'].__setitem__(4,0))
 for offset,name in [(8,'negative-zero'),(48,'original-baseline')]:
  def change(q,o=offset):
   b=bytearray(base64.b64decode(q['Lines'][1]['Full56']));b[o]^=128 if o==48 else 1;q['Lines'][1]['Full56']=base64.b64encode(b).decode()
  add(name,change)
 def known(q):
  b=bytearray(base64.b64decode(q['Masks'][0]['Bytes']));b[8]=255;q['Masks'][0]['Bytes']=base64.b64encode(b).decode()
 add('pointer-promotion',known)
 def tail(q):
  b=bytearray(base64.b64decode(q['WordLinkKnown24']));b[21]=255;q['WordLinkKnown24']=base64.b64encode(b).decode()
 add('word-link-padding-promotion',tail)
 add('command-kind',lambda q:q['Pending'][2].__setitem__('Name','WORD'))
 add('command-order',lambda q:q['Pending'].reverse())
 add('text-claim',lambda q:q.__setitem__('OwningTEXTExecuted',True))
 add('extra-row',lambda q:q['Nulls'].append(q['Nulls'][0]))
 add('missing-row',lambda q:q['Pending'].pop())
 add('bool-counter',lambda q:q.__setitem__('Guards',True))
 refused=[]
 for name,q in mutations:
  try:check(q,reference)
  except AssertionError:refused.append(name)
  else:raise AssertionError('accepted mutation '+name)
 report=dict(Scope='PREPARED_ONLY_NO_OWNING_TEXT_EXECUTION',Passed=True,NullCases=7,GuideCases=2,FullLineBytes=112,KnownMaskCases=3,PendingCommands=3,TamperRefused=refused,CaptureSha256=hashlib.sha256(capture.read_bytes()).hexdigest())
 output.write_bytes((json.dumps(report,indent=2)+'\n').encode());print(json.dumps(report))
if __name__=='__main__':main()
