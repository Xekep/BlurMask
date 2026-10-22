"""Independent full-source B4-byte numeric terminal oracle, no C#/NN/vendor execution."""
from pathlib import Path
import argparse,ast,base64,copy,hashlib,json,subprocess,sys,tempfile
p=argparse.ArgumentParser();p.add_argument('--repo',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--b4',type=Path,required=True);p.add_argument('--capture',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
with tempfile.TemporaryDirectory()as td:
 ref=Path(td)/'reference.json';subprocess.run([sys.executable,str(Path(__file__).with_name('reference_terminal.py')),'--repo',str(a.repo),'--models',str(a.models),'--capture',str(a.b4),'--output',str(ref)],check=True,capture_output=True,text=True);expected=json.loads(ref.read_bytes())
tree=ast.parse((a.repo/'tools/LegacyFullEnglishFirstContext.Probe/audit_output.py').read_text(encoding='utf-8-sig'));nodes=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom))or isinstance(n,ast.ClassDef)and n.name=='Cases'];g={};exec(compile(ast.Module(body=nodes,type_ignores=[]),'B4-stream-reader','exec'),g)
old=g['Cases'](a.b4);current=json.loads(a.capture.read_bytes());assert current['TargetsRead']is False and current['Guards']>=29 and current['Cancellations']==9 and current['PreparedEmptyGrid']==1
assert len(current['Actual'])==expected['actual'];refrows=iter(expected['results']);count=cells=nodesCount=0;mutations=0

def validate(child,reference,original):
 actualnodes=child['Contexts'];oldnodes=original['Contexts'];assert len(actualnodes)==len(oldnodes)
 def index(rows):
  out={0:None};slots=set()
  for n in rows:
   assert len(n)==9 and n[0]!=0 and n[0]not in out and (n[7],n[8])not in slots;out[n[0]]=(n[7],n[8]);slots.add((n[7],n[8]))
  return out
 am,om=index(actualnodes),index(oldnodes);reverse={slot:i for i,slot in om.items()};assert set(am.values())==set(om.values())
 def normalized(rows,m):return sorted([tuple([m[n[k]]for k in range(5)]+n[5:])for n in rows],key=lambda n:n[-2:])
 assert normalized(actualnodes,am)==normalized(oldnodes,om),'full owned pool topology/fields/pages'
 def oldid(i):return reverse[am[i]]
 assert(child['Descriptor'],child['Reference78'])==(reference['descriptor'],reference['reference78'])
 assert child['HasFinite']==bool(reference['alternatives']);assert len(child['Alternatives'])==len(reference['alternatives'])
 for ac,ex in zip(child['Alternatives'],reference['alternatives']):
  for aa,ee in [('Row','row'),('Column','column'),('Total','total'),('Acoustic','acoustic'),('Trait','trait'),('ScoreBits','scoreBits')]:assert ac[aa]==ex[ee],('scalar',aa)
  assert oldid(ac['Identity'])==ex['identity'];assert len(ac['Cells'])==len(ex['cells'])
  for c,e in zip(ac['Cells'],ex['cells']):
   assert c['Coordinates']==e['coordinates'],'trace coordinates'
   b=bytearray(base64.b64decode(c['Bytes'],validate=True));assert len(b)==40;identity=int.from_bytes(b[24:32],'little');b[24:32]=oldid(identity).to_bytes(8,'little');assert bytes(b)==base64.b64decode(e['bytes']),'full40B trace'
 return len(actualnodes)
for actual,prior in zip(current['Actual'],old):
 assert actual['Name']==prior['Name']
 if prior['Status']!='AcceptedContext':assert actual['Status']=='Refused';continue
 reference=next(refrows);assert reference['name']==actual['Name']and actual['Status']=='AcceptedTerminal'and actual['NetworkCount']==prior['NetworkCount']
 assert len(actual['Children'])==len(prior['Children'])==len(reference['children'])
 N=sum(len(c['B8'])for c in prior['Children']);C=len(prior['Children']);assert(actual['Peak'],actual['Work'],actual['Visits'])==(4194304+2048*N+1024*C,4194304+4096*N+2048*C,1024*N+4096*C)
 for child,ex,original in zip(actual['Children'],reference['children'],prior['Children']):
  nodesCount+=validate(child,ex,original);count+=1;cells+=sum(len(c['Cells'])for c in child['Alternatives'])
  if mutations<4 and child['Alternatives']and child['Alternatives'][0]['Cells']:
   #Changed score, source coordinate,40B channel and pool metadata must all fail.
   for kind in range(4):
    m=copy.deepcopy(child)
    if kind==0:m['Alternatives'][0]['Total']+=1
    elif kind==1:m['Alternatives'][0]['Cells'][0]['Coordinates'][3]+=1
    elif kind==2:
     b=bytearray(base64.b64decode(m['Alternatives'][0]['Cells'][0]['Bytes']));b[4]^=1;m['Alternatives'][0]['Cells'][0]['Bytes']=base64.b64encode(b).decode()
    else:m['Contexts'][0][5]^=1
    try:validate(m,ex,original)
    except AssertionError:mutations+=1
    else:raise AssertionError('mutation accepted')
assert next(old,None)is None and next(refrows,None)is None and count==current['Children']==expected['children']and cells==current['Cells']==expected['cells']and current['Accepted']==expected['accepted']and current['Refused']==expected['refused']
r=dict(status='PASS',actual=expected['actual'],accepted=current['Accepted'],refused=current['Refused'],children=count,cells=cells,fullSymbolNodes=nodesCount,guards=current['Guards'],cancellations=current['Cancellations'],mutationsRejected=mutations,captureSha256=hashlib.sha256(a.capture.read_bytes()).hexdigest(),b4Sha256=hashlib.sha256(a.b4.read_bytes()).hexdigest(),scope='Full raw-source/B4 scalar bits, all40B selectedcells and normalized complete upstream symbolic topology. No WORD/nativeABI/quality claim.',vendorExecuted=False,targetsRead=False)
a.output.write_text(json.dumps(r,indent=2)+'\n');print(json.dumps(r))
