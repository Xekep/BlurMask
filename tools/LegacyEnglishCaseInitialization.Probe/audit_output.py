"""Independent raw-model + height-based AVL/page oracle for initialization only."""
from pathlib import Path
import argparse,base64,copy,hashlib,json,runpy,struct,sys
p=argparse.ArgumentParser()
for key in('repo','models','capture','output'):p.add_argument('--'+key,type=Path,required=True)
p.add_argument('--sdk',type=Path,help='Optional read-only PE verification in addition to the pinned portable snapshot.')
a=p.parse_args();prefix='eci-'+hashlib.sha256(str(a.output.resolve()).encode()).hexdigest()[:12]
sys.argv=['reference',str(a.models),str(a.output.with_name(prefix+'-case.json'))];ref=runpy.run_path(str(a.repo/'tools/LegacyEnglishCaseMain.Probe/reference_case_main.py'),run_name='__main__');rawref=ref['result'];primary=ref['primary'];auxiliary=ref['auxiliary'];ac=ref['ac']
sys.argv=['reference',str(a.models),str(a.output.with_name(prefix+'-numeric.json'))];lang=runpy.run_path(str(a.repo/'tools/LegacyEnglishLkSparse101.Probe/reference_actual.py'),run_name='__main__')
cap=json.loads(a.capture.read_bytes());assert cap['Scope']=='fresh-English-initialization-only/no-query-history-terminal' and not cap['TargetsRead'] and not cap['VendorExecuted']
#Explicit equality matrix, independent stored relation maps, no C# library call.
def forms(label,mode):
 mapping=ac['maps'].get(1<<mode,ac['maps'].get(1))
 if mapping is None or mapping[label]<0:return None
 row=list(ac['triplets'][mapping[label]])
 if label not in row:
  for i in(0,1):
   if row[i]==65535:row[i]=label;break
 return row
boundary=max(i for i in range(217)if any(m[i]>=0 for m in ac['maps'].values()))
def category(label,row):
 low,up,title=row
 return 0 if low==65535 else(0 if up==65535 or title==65535 else 1+int(up!=title))if label==low else 3+int(title!=65535 and label!=title)
textToLabel={v:k for k,v in ac['text'].items()};first=[];second=[];expectedForms=[]
for signed in range(-29,217):
 label=signed&65535;f=g=None;p=q=0;zero=forms(label,0)if signed>=0 and label<=boundary else None
 if zero is not None:
  f=zero;p=category(label,zero);one=forms(label,1);two=forms(label,2);assert one is not None and two is not None;g=one.copy()
  if label==one[0]and one[1]==65535 and one[2]!=65535:g=[one[0],one[2],65535];q=1
  elif one[0]==65535:q=0
  elif label==one[0]:q=category(label,one)
  else:q=3+int(label!=one[2]and zero[2]!=65535)
  if p==0:
   if two[0]==65535:q=0
   else:
    p=category(label,two)
    if p:f=two
 elif label==65532 or label==textToLabel.get('-'):p=q=6
 elif -9<=signed<=-5:p=q=5
 elif -29<=signed<=-22:p=q=7
 if label in[textToLabel.get(t)for t in',:;']:p=8
 if label in[textToLabel.get(t)for t in'.!?']:p=9
 first.append(p);second.append(q)
 if signed>=0:expectedForms.append([label,f,g])
#AVL reconstructed by recursive HEIGHT, rather than portable/native balance-state code.
class Node:
 def __init__(self,key,slot):self.key=key;self.slot=slot;self.left=self.right=None
nodes={}
def height(n):return 0 if n is None else 1+max(height(n.left),height(n.right))
def left(n):r=n.right;n.right=r.left;r.left=n;return r
def right(n):l=n.left;n.left=l.right;l.right=n;return l
def insert(n,key):
 if n is None:n=Node(key,len(nodes));nodes[key]=n;return n
 if key<n.key:n.left=insert(n.left,key)
 elif key>n.key:n.right=insert(n.right,key)
 else:return n
 b=height(n.right)-height(n.left)
 if b>1:
  if key<n.right.key:n.right=right(n.right)
  return left(n)
 if b< -1:
  if key>n.left.key:n.left=left(n.left)
  return right(n)
 return n
def export(n):
 def ident(x):return 0 if x is None else x.slot+1
 return [[x.slot+1,0,0,ident(x.left),ident(x.right),x.key,height(x.right)-height(x.left),x.slot//102,x.slot%102]for x in sorted(nodes.values(),key=lambda x:x.slot)]
tree=None
for key in primary:
 if key not in(65535,65533):tree=insert(tree,key)
assert len(nodes)==86;before=export(tree);beforeRoot=tree.slot+1;source68=[0 if key in(65535,65533)else nodes[key].slot+1 for key in primary];controls=[]
for key in(65531,65529,65531,65532,65530):
 created=key not in nodes;tree=insert(tree,key);controls.append([key,nodes[key].slot+1,int(created)])
assert len(nodes)==89;post=export(tree);postRoot=tree.slot+1
for row in cap['Rows']:
 s=row['Snapshot'];assert s['Initial']==before and s['Post']==post,(row['Name'],'full AVL9fields')
 assert(s['InitialRoot'],s['PostRoot'],s['InitialPages'],s['PostPages'],s['InitialPageCapacity'],s['PostPageCapacity'],s['InitialCursor'],s['PostCursor'])==(beforeRoot,postRoot,[86],[89],1,1,0,0)
 assert s['Preserved']==primary and s['Auxiliary']==auxiliary and s['Source68']==source68 and s['Controls']==controls
 assert s['Ordinary']==first and s['Alternate']==second
 assert(s['ContextCapacity'],s['SourceKind'],s['CoordinateMode'],s['CaseVersion'],s['Descriptor'],s['Priority'])==(104,0x135,2,512,0xc0e,1024)
 for key,expected in(('Tokens',rawref['caseTokens']),('Seed',rawref['maskDiagnostic']['seed']),('Primary',rawref['maskDiagnostic']['primary']),('Secondary',rawref['maskDiagnostic']['secondary'])):assert row[key]==expected,(row['Name'],key)
 assert row['Budget']==2907401 and row['SourceUnchanged']
assert cap['Numeric']['Forms']==expectedForms
for name,raw in cap['Numeric']['Codes'].items():assert base64.b64decode(raw)==lang['raws'][name]
assert cap['Numeric']['Costs']==[lang['wordCompiled']]+lang['classCompiled']+lang['backoffCompiled']
#Portable reference bytes are extracted from the pinned PE, never C# constants
#or the capture. Independent literal hashes reject a self-rehashed replacement.
module_sha='18f1b4fc211ae96cecd05611f72dbef52d93ac4484c61b735c8296dc01c3977f'
expected_spans={'Policies':('0x1802e86d0',420,'21b02f8f05c9f0d7dae744b1d3011cc913dde7a1cc228d1b4af22be07aa4c216'),'Selectors':('0x1802e8880',224,'43bd68ac941fd1061586016897539d64aa7aedbd067dd931a8631593e355e942')}
policy_path=Path(__file__).with_name('reference_policy.json');policy=json.loads(policy_path.read_bytes())
assert(policy['format'],policy['module'],policy['moduleSha256'],policy['imageBase'])==(1,'MyScriptText.dll',module_sha,'0x180000000')
assert set(policy['spans'])==set(expected_spans)
for name,(addr,size,digest)in expected_spans.items():
 span=policy['spans'][name];payload=base64.b64decode(span['base64'],validate=True)
 assert(span['address'],span['bytes'],span['sha256'])==(addr,size,digest)
 assert len(payload)==size and hashlib.sha256(payload).hexdigest()==digest
 assert bytes(cap['Numeric'][name])==payload
if a.sdk is not None:
 raw=(a.sdk/'MyScriptText.dll').read_bytes();assert hashlib.sha256(raw).hexdigest()==module_sha
 pe=struct.unpack_from('<I',raw,60)[0];at=pe+24+struct.unpack_from('<H',raw,pe+20)[0];sections=[struct.unpack_from('<IIII',raw,at+40*i+8)for i in range(struct.unpack_from('<H',raw,pe+6)[0])]
 def data(addr,n):
  v=addr-0x180000000
  for _,rva,size,offset in sections:
   if rva<=v and n<=rva+size-v:return raw[offset+v-rva:offset+v-rva+n]
  raise AssertionError('bounded PE')
 for name,(addr,size,digest)in expected_spans.items():assert data(int(addr,16),size)==base64.b64decode(policy['spans'][name]['base64'],validate=True)
assert cap['Guards']==58==len(cap['GuardNames'])and cap['Cancellations']==18 and len(set(cap['GuardNames']))==cap['Guards']
required={'snapshot-blocks-D0','snapshot-blocks-score','D0-blocks-snapshot','score-blocks-snapshot','forged-inspection-complete','stale-complete','one-use-inputs','one-use-prefix','repeat-foreign-bind','foreign-numeric-source','busy-canceled-snapshot','budget-before-entry','budget-upper','forged-binding-before-publication','repeat-binding-before-publication','foreign-context-root-before-bind','second-plan-issuer'};assert required<=set(cap['GuardNames'])
assert len(cap['Rows'])in(1,134,100)and (len(cap['Refused'])==7 if len(cap['Rows'])==134 else not cap['Refused'])
result=dict(status='PASS independent actual initialization oracle',owners=len(cap['Rows']),initialNodes=86,postNodes=89,allAVLFields=9,classifications=492,forms=217,codeBytes=sum(len(base64.b64decode(v))for v in cap['Numeric']['Codes'].values()),costEntries=1536,policyBytes=420,selectorBytes=224,guards=cap['Guards'],cancellations=18,additionalLogicalBytes=2907401,targetsRead=False,vendorExecuted=False,queryOrHistoryClaim=False,policyReferenceSha256=hashlib.sha256(policy_path.read_bytes()).hexdigest(),sdkPeVerified=a.sdk is not None,captureSha256=hashlib.sha256(a.capture.read_bytes()).hexdigest());a.output.write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(result))
