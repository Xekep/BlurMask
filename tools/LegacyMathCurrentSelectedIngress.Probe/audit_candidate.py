from pathlib import Path
import json,base64,hashlib,importlib.util,sys
O=Path(__file__).resolve().parent;R=Path(__file__).resolve().parents[2];capture=json.loads(Path(sys.argv[1]).read_bytes())
s=importlib.util.spec_from_file_location('utf8',R/'tools/LegacyMainSourceRangeUtf8.Probe/reference_controls.py');utf8=importlib.util.module_from_spec(s);s.loader.exec_module(utf8)
def values(rs):return [[r['First']['Value'],r['Last']['Value']]for r in rs]
def fmt(rs):return base64.b64decode(utf8.replay(values(rs))['utf8']).decode()
for case in capture['Actual']:
 p=case['Prior'];c=case['Candidate'];f=case['NewNn']['FirstCopy'];assert c['Complete']and c['Ready']and not c['Failed']and not c['Released']
 assert c['RegistryNext']==p['Next'] and len(c['Registry'])==len(p['Registry'])
 for a,b in zip(c['Registry'],p['Registry'],strict=True):
  assert {k:a[k]for k in['Id','Name','Category','Ranges']}=={k:b[k]for k in['Id','Name','Category','Ranges']}
  assert a['CanonicalSelection']==b['CanonicalC22Id']
 assert c['Temporary']==p['SelectedField']
 tmp=c['Temporary'];old=[]
 oldroot=next(n['Id']for n in tmp if n['Kind']==3 and n['Name']=='mathResult')
 def scan(parent):
  old.extend(n['Number']for n in tmp if n['Parent']==parent and n['Kind']==0 and n['Name']=='tag')
  for n in tmp:
   if n['Parent']==parent and n['Kind']!=0:scan(n['Id'])
 scan(oldroot);assert old and old==c['OldTags']and all(t>=0 for t in old)
 out=[];pending=[]
 def node(kind,name,parent=0,tag=0,text=None,number=None):
  identity=len(out)+1;out.append(dict(Id=identity,Kind=kind,Name=name,Parent=parent,Origin=0,ValueTag=tag,Text=text,Number=number,NamespaceUri=None,NamespacePrefix=None));return identity
 def st(text):return 3 if not text else 2 if len(text.encode())<16 else 1
 def attr(parent,name,tag,text=None,number=None):return node(0,name,parent,tag,text,number)
 def string(parent,name,text):return attr(parent,name,st(text),text)
 def register(name,ranges):
  identity=len(pending)+2;pending.append(dict(TemporaryId=identity,Name=name,ResolvedId=-1,HasSelection=bool(ranges),Ranges=ranges));return identity
 doc=node(4,'');root=node(3,'mathResult',doc);seq=node(3,'sequence',root)
 for text,ranges in zip(f['Text'],f['Ranges'],strict=True):
  symbol=node(3,'symbol')
  if ranges:string(symbol,'range',fmt(ranges));attr(symbol,'tag',4,number=-register('MATH_SYMBOL',ranges))
  value=base64.b64decode(text['Bytes']).split(b'\0',1)[0].decode();node(1,'',symbol,st(value),value);out[symbol-1]['Parent']=seq
 domain=next(r['Ranges']for r in p['Registry']if r['Name']=='INPUT');full=fmt(domain)
 lst=node(3,'symbolSegmentList',root);segment=node(3,'symbolSegment',lst);string(segment,'range',full)
 choice=node(3,'symbolCandidate',segment);attr(choice,'nrs',10,number=0);char=node(3,'char',choice);node(1,'',char,2,'?')
 terminal=node(3,'mathTerminalNode',root);string(terminal,'name','');string(terminal,'ref',full);attr(terminal,'selected',7,number=0);attr(terminal,'tag',4,number=-register('MATH_NODE',domain))
 assert out==c['Candidate'];assert pending==c['Pending'];assert c['TemporaryNext']==len(pending)+2
 assert all(n['Number']<0 for n in out if n['Name']=='tag')
 assert not({'Patch','Actions','Diff','Receiver'}&c.keys())
assert len(capture['Actual'])==6
print('PASS6 independent genuine prior temp/third aliases/Next/names cohort + new-NN candidate schema/pending tags; no produced diff/actions asserted')
