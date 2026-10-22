"""STATIC proposal reference: actual full raw AL/BA and audited B4 bytes. No C#/NN/vendor execution."""
from pathlib import Path
import argparse,ast,base64,hashlib,json,math,re,struct
p=argparse.ArgumentParser();p.add_argument('--repo',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--capture',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
def defs(path,names=None):
 t=ast.parse(path.read_text(encoding='utf-8-sig'));g={'__file__':str(path)};nodes=[n for n in t.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))and(names is None or isinstance(n,(ast.Import,ast.ImportFrom))or n.name in names)]
 exec(compile(ast.Module(body=nodes,type_ignores=[]),str(path),'exec'),g);return g
raw=(a.models/'en-US/en_US-ak-cur.res').read_bytes();assert hashlib.sha256(raw).hexdigest()=='355f15d581dee2baeb092937cef69ae8db321ce3f60e28497c13e628a3b72927'
source=raw[8843:373957];assert hashlib.sha256(source).hexdigest()=='511b3f4e562cdd1a15661b7f0426255bc7d8675b76cd444e1f58c78adf2d380c'
w,s=defs(a.repo/'tools/LegacySource247Wire.Probe/audit_output.py')['source'](source)
fields={x['destination']:base64.b64decode(x['raw'])for x in w.fields if x['owner']=='247'}
assert s['Version']==0x110 and s['SourceKind']==0x135 and s['alphabetCount']==217
assert fields['BA']==b'\x05\0' and fields['B9']==fields['B8']==b'\0'
header,labels=defs(a.repo/'tools/LegacyMainSingleLineIngress.Probe/audit_output.py',{'labels'})['labels'](source)
assert header==[181,178,65535,180,184,182,183,217]and '\u0651'not in labels
Cases=defs(a.repo/'tools/LegacyFullEnglishFirstContext.Probe/audit_output.py',{'Cases'})['Cases']
def get(b,pos,fmt):return struct.unpack_from('<'+fmt,b,pos)[0]
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]
def fb(x):return struct.pack('<f',x).hex()
def wrap(x):return(x+2**31)%2**32-2**31
out=[];totals=dict(actual=0,accepted=0,refused=0,children=0,terminals=0,empty=0,cells=0,blank=0,closed=0,boundaries=0)
for row in Cases(a.capture):
 totals['actual']+=1
 if row['Status']!='AcceptedContext':totals['refused']+=1;continue
 totals['accepted']+=1;children=[]
 for child in row['Children']:
  totals['children']+=1;nodes=[[base64.b64decode(r['Bytes'])for r in n]for n in child['B8']];assert len(nodes)==len(child['Nodes'])and all(len(n)==5 and all(len(r)==40 for r in n)for n in nodes)
  def cell(n,r,c=0):assert 0<=n<len(nodes)and 0<=r<5 and c==0;return nodes[n][r]
  best=None
  for ri in range(5):
   b=cell(len(nodes)-1,ri);cost=get(b,0,'i')
   if cost>=0x4000000:continue
   #Actual shadda=FFFF; retain exact native trailing walk in reference.
   tn,tr=len(nodes)-1,ri;trait=b
   while get(trait,12,'H')==65535:
    pn=tn-((get(trait,14,'H')>>1)&7)-1
    if pn==-1:break
    assert pn>=0;tr=trait[9];assert trait[10]==0;tn=pn;trait=cell(tn,tr)
   if trait[32]&0x13==1:cost=wrap(cost+0xa000)
   if best is None or cost<best[0]:best=(cost,ri,get(b,4,'i'),get(b,24,'Q'),[tn,tr,0])
  if best is None:totals['empty']+=1;children.append(dict(descriptor=child['Descriptor'],reference78=0,alternatives=[]));continue
  total,ri,acoustic,identity,trait=best;totals['terminals']+=1;n=len(nodes)-1;r=ri;reverse=[]
  while n>=0:
   b=cell(n,r);assert get(b,0,'i')<0x4000000;packed=get(b,14,'H');span=(packed>>1)&7;pn=n-span-1;assert pn>=-1
   arc=get(b,16,'H');symbol=get(b,12,'H');assert arc<len(child['Nodes'][n])and child['Nodes'][n][arc]['Span']==span
   primary=base64.b64decode(child['Primary'][n]['Bytes']);assert r<child['Primary'][n]['Count']and get(primary,r*40+12,'H')==symbol and get(primary,r*40+16,'H')==arc
   pa=get(cell(pn,b[9]),16,'H')if pn>=0 else 65535
   reverse.append(dict(coordinates=[n,r,0,arc,symbol,span,packed&1,pn,pa],bytes=base64.b64encode(b).decode()));totals['cells']+=1;totals['blank']+=symbol==217;totals['closed']+=symbol==211;totals['boundaries']+=bool(packed&1)
   r,n=b[9],pn;assert b[10]==0
  reverse.reverse();relative=0;normal=f(f(relative)/f(len(nodes)<<13));log=f(f(-5.*f(relative))*f(.00012207031));mass=f(math.exp(log));ac=f(f(acoustic)*f(.00012207031))
  children.append(dict(descriptor=child['Descriptor'],reference78=total,alternatives=[dict(row=ri,column=0,total=total,acoustic=acoustic,identity=identity,trait=trait,scoreBits=[relative,fb(normal),fb(log),fb(mass),fb(ac)],cells=reverse)]))
 out.append(dict(name=row['Name'],children=children))
report=dict(status='STATIC_REFERENCE_PASS',**totals,sourceFingerprint=hashlib.sha256(source).hexdigest(),captureSha256=hashlib.sha256(a.capture.read_bytes()).hexdigest(),vendorExecuted=False,targetsRead=False,scope='Proposal reference only; no new C# terminal implemented, no WORD/source720/guide')
report['results']=out;a.output.write_text(json.dumps(report,separators=(',',':'))+'\n');print(json.dumps({k:v for k,v in report.items()if k!='results'}))
