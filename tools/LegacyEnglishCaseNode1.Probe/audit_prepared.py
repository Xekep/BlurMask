"""Independent prepared wire arithmetic. No NN, C# execution, or source ownership claim."""
import copy,json,struct,sys,hashlib
IMP=0x4000000
def i(b,o):return struct.unpack_from('<i',b,o)[0]
def u(b,o):return struct.unpack_from('<Q',b,o)[0]
def put(b,o,v):struct.pack_into('<i',b,o,((v+2**31)%2**32)-2**31)
def check(d):
 assert set(d)=={'Scope','Controls','Refusals','NNCalls','ParentMaskControls','RootStartControls'}
 maskcheck(d['ParentMaskControls'])
 rootcheck(d['RootStartControls'])
 assert d['NNCalls']==0 and d['Refusals']==43
 assert len(d['Controls'])==21
 pairs=[(c,n) for c in [-8192,-1,0,1,8192,123456,IMP-1] for n in [-100,0,100]]
 for x,(cost,inc) in zip(d['Controls'],pairs,strict=True):
  assert x['Cost']==cost and x['Increment']==inc
  before=bytes.fromhex(x['Before']);parent=bytes.fromhex(x['Parent']);pa=bytes.fromhex(x['ParentAlt'])
  assert len(before)==250*128 and len(parent)==88 and len(pa)==80
  assert i(before,16)==cost and i(before,20)==inc and i(parent,16)==100 and i(parent,20)==20
  assert before[32]==0 and struct.unpack_from('<H',before,30)[0]==0
  # Single candidate: assigning ordinal58=0, sorting and quota cannot reorder it.
  heap=bytearray(before);put(heap,88,0)
  assert bytes.fromhex(x['Heap'])==heap
  rows=bytearray(125*88)
  for row in range(125):put(rows,row*88+16,IMP);put(rows,row*88+20,IMP)
  rows[:88]=heap[:88]
  if struct.unpack_from('<H',rows,60)[0]==65535:struct.pack_into('<H',rows,60,0)
  assert bytes.fromhex(x['Rows'])==rows
  alts=bytearray(125*80)
  for row in range(125):put(alts,row*80,IMP);put(alts,row*80+4,IMP)
  for k in range(2):
   at=k*40;put(alts,at,i(pa,at)+cost-100);put(alts,at+4,i(pa,at+4)+inc-20)
   alts[at+8:at+24]=pa[at+8:at+24]
   alts[at+24:at+26]=heap[30:32];alts[at+26]=heap[32];alts[at+27]=k;alts[at+30]=heap[38];alts[at+31]=heap[33]&31
  assert bytes.fromhex(x['Alts'])==alts
  assert x['Query']=={'Tag':u(parent,0),'Node':struct.unpack_from('<i',parent)[0],'Skip':0,'Parameter1c':1,'Parameter20':0,'Flags24':0,'Flags26':0,'Parameter19':0}
 return {'controls':21,'allHeapBytes':21*32000,'allRowBytes':21*11000,'allAlternativeBytes':21*10000,'queryFrames':21,'NNCalls':0}
def maskcheck(m):
 assert set(m)=={'Scope','Row','RowMask','Alternatives','AlternativeMasks','Reads','Refused','OpaqueOffsets','Unchanged','InactiveClassification','InvalidAlternativesAccepted'}
 opaque=[65,66,67,76,77,78,79];reads=list(range(34))+list(range(36,39))+list(range(40,62))
 row=bytearray(88);put(row,16,100);mask=bytearray([255]*88)
 for at in opaque:row[at]=at+1;mask[at]=0
 alts=bytearray(80);am=bytearray(80)
 for at in [0,40]:put(alts,at,10);am[at:at+24]=bytes([255]*24)
 assert bytes.fromhex(m['Row'])==row and bytes.fromhex(m['RowMask'])==mask
 assert bytes.fromhex(m['Alternatives'])==alts and bytes.fromhex(m['AlternativeMasks'])==am
 assert m['Reads']==reads and m['OpaqueOffsets']==opaque
 assert m['Refused']==['row:'+str(i) for i in reads]+['alt:'+str(i) for i in list(range(24))+list(range(40,64))]+['inactive-cost:'+str(i) for i in range(16,20)]
 assert m['Unchanged'] is True and m['InactiveClassification'] is False and m['InvalidAlternativesAccepted'] is True
def rootcheck(m):
 assert set(m)=={'Scope','Controls','DefaultRootRefusals','Competition','Mixed','Query'}
 assert m['DefaultRootRefusals']==9 and len(m['Controls'])==9
 for x,(cost,inc) in zip(m['Controls'],[(c,i) for c in [-1,0,200] for i in [-20,0,50]],strict=True):
  assert x['Cost']==cost and x['Increment']==inc
  before=bytes.fromhex(x['Before']);assert len(before)==32000
  assert i(before,16)==cost and i(before,20)==inc and before[32]==1
  assert struct.unpack_from('<H',before,30)[0]==65535
  heap=bytearray(before);put(heap,88,0);assert bytes.fromhex(x['Heap'])==heap
  rows=emptyrows();rows[:88]=heap[:88];struct.pack_into('<H',rows,60,0);assert bytes.fromhex(x['Rows'])==rows
  alts=emptyalts();rootalt(alts,0,heap);assert bytes.fromhex(x['Alts'])==alts
 mix=m['Mixed'];assert mix['Count']==2
 root=prefix(0x100000003,200,50,True);parent=prefix(0x100000002,100,30,False)
 heap=bytearray(32000);heap[:88]=root;heap[128:216]=parent;put(heap,128+88,1)
 assert bytes.fromhex(mix['Heap'])==heap
 rows=emptyrows();rows[:88]=root;struct.pack_into('<H',rows,60,0);rows[88:176]=parent;assert bytes.fromhex(mix['Rows'])==rows
 alts=emptyalts();rootalt(alts,0,root)
 pa=bytes.fromhex(mix['ParentAlt']);pr=bytes.fromhex(mix['Parent']);assert i(pr,16)==100 and i(pr,20)==20
 for k in range(2):
  at=80+k*40;put(alts,at,i(pa,k*40));put(alts,at+4,i(pa,k*40+4)+10)
  alts[at+8:at+24]=pa[k*40+8:k*40+24];alts[at+27]=k
 assert bytes.fromhex(mix['Alts'])==alts
 assert m['Query']=={'Tag':8<<40,'Node':0,'Skip':0,'Parameter1c':1,'Parameter20':1,'Flags24':0,'Flags26':0}
 assert len(m['Competition'])==3
 for c,rootcost in zip(m['Competition'],[-100,10300,20000],strict=True):
  assert c['InputRootCost']==rootcost and c['Inserted'] is (rootcost<10498) and c['Count']==125
  expected=sorted([10000+2*i for i in range(250)]+[rootcost])[:125]
  assert sorted(c['RetainedCosts'])==expected
  assert c['RootRows']==(1 if rootcost==-100 else 0)
def prefix(tag,cost,inc,root):
 p=bytearray(88);struct.pack_into('<Q',p,0,tag);put(p,16,cost);put(p,20,inc);put(p,24,1)
 struct.pack_into('<H',p,30,65535 if root else 0);p[32]=int(root);struct.pack_into('<H',p,34,65535);struct.pack_into('<H',p,36,2);struct.pack_into('<Q',p,40,2**64-1)
 put(p,48,-1);put(p,56,1 if root else 0);struct.pack_into('<H',p,60,65535 if root else 0);struct.pack_into('<H',p,62,65535);return p
def emptyrows():
 r=bytearray(11000)
 for n in range(125):put(r,n*88+16,IMP);put(r,n*88+20,IMP)
 return r
def emptyalts():
 a=bytearray(10000)
 for n in range(125):put(a,n*80,IMP);put(a,n*80+4,IMP)
 return a
def rootalt(a,at,slot):
 a[at:at+8]=slot[16:24];a[at+24:at+26]=slot[30:32];a[at+26]=slot[32];a[at+30]=slot[38];a[at+31]=(slot[33]&31)|32;put(a,at+40,IMP);put(a,at+44,IMP)
def main():
 raw=open(sys.argv[1],'rb').read();d=json.loads(raw);report=check(d);neg=0
 for field in ['Heap','Rows','Alts','ParentAlt']:
  t=copy.deepcopy(d);v=bytearray.fromhex(t['Controls'][0][field]);v[16 if field=='ParentAlt' else 0]^=1;t['Controls'][0][field]=v.hex()
  try:check(t)
  except AssertionError:neg+=1
  else:raise AssertionError('tamper accepted '+field)
 for kind in ['query','omitted','extra','NN']:
  t=copy.deepcopy(d)
  if kind=='query':t['Controls'][0]['Query']['Node']+=1
  elif kind=='omitted':t['Controls'].pop()
  elif kind=='extra':t['Controls'].append(t['Controls'][-1])
  else:t['NNCalls']=1
  try:check(t)
  except AssertionError:neg+=1
  else:raise AssertionError('tamper accepted '+kind)
 for kind in ['promotion','missing-refusal','extra-read','changed-value','changed-flag','omitted-opaque']:
  t=copy.deepcopy(d);m=t['ParentMaskControls']
  if kind=='promotion':v=bytearray.fromhex(m['RowMask']);v[65]=255;m['RowMask']=v.hex()
  elif kind=='missing-refusal':m['Refused'].pop()
  elif kind=='extra-read':m['Reads'].append(65)
  elif kind=='changed-value':v=bytearray.fromhex(m['Row']);v[65]=0;m['Row']=v.hex()
  elif kind=='changed-flag':m['Unchanged']=False
  else:m['OpaqueOffsets'].pop()
  try:check(t)
  except AssertionError:neg+=1
  else:raise AssertionError('mask tamper accepted '+kind)
 for kind in ['root-missing','root-bytes','mixed-rows','competition','root-default','root-query']:
  t=copy.deepcopy(d);m=t['RootStartControls']
  if kind=='root-missing':m['Controls'].pop()
  elif kind=='root-bytes':v=bytearray.fromhex(m['Controls'][0]['Alts']);v[0]^=1;m['Controls'][0]['Alts']=v.hex()
  elif kind=='mixed-rows':v=bytearray.fromhex(m['Mixed']['Rows']);v[32]=0;m['Mixed']['Rows']=v.hex()
  elif kind=='competition':m['Competition'][1]['RootRows']=1
  elif kind=='root-default':m['DefaultRootRefusals']=0
  else:m['Query']['Parameter20']=0
  try:check(t)
  except AssertionError:neg+=1
  else:raise AssertionError('root tamper accepted '+kind)
 report.update(rootStartCases=9,mixedSameHeapCases=1,competitionCases=3,parentMaskRequiredBytes=59,parentFiniteAlternativeBytes=48,parentMaskRefusals=111,negatives=neg,inputSha256=hashlib.sha256(raw).hexdigest(),scope='prepared numeric join only')
 print(json.dumps(report,sort_keys=True))
if __name__=='__main__':main()
