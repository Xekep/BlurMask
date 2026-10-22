"""Raw full FIRST admission + conservative B3 byte/work arithmetic; no NN/labels."""
from pathlib import Path
import argparse,ast,base64,struct,hashlib,json,collections
ap=argparse.ArgumentParser();ap.add_argument('--repo',type=Path,required=True);ap.add_argument('--capture',action='append',type=Path,required=True);ap.add_argument('--output',type=Path,required=True);args=ap.parse_args()
def definitions(path):
 tree=ast.parse(path.read_text(encoding='utf-8-sig'));g={'__file__':str(path)};exec(compile(ast.Module(body=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))],type_ignores=[]),str(path),'exec'),g);return g
sha=lambda x:hashlib.sha256(x).hexdigest()
wire=definitions(args.repo/'tools/LegacyFirstAcoustic119Wire.Probe/audit_output.py');source=definitions(args.repo/'tools/LegacySource247Wire.Probe/audit_output.py');alphabet=definitions(args.repo/'tools/LegacyMainSingleLineIngress.Probe/audit_output.py')['labels']
resources=[]
for name,expected in [('en_US-ak-cur.res','355f15d581dee2baeb092937cef69ae8db321ce3f60e28497c13e628a3b72927'),('en_US-ak-cur.lite.res','ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657')]:
 d=(args.repo/'models/en-US'/name).read_bytes();assert sha(d)==expected
 selected=next(c for c in wire['source'](d,0)[3]if c[3]==0x249);start=selected[1]+16;end,kind,count,children=wire['source'](d,start)
 assert selected[4]==0x100 and end==selected[2]-4 and struct.unpack_from('<f',d,end)[0]==.75
 fields,summary=source['source'](d[start:end]);assert summary['Version']==0x110;f={x['destination']:base64.b64decode(x['raw'])for x in fields.fields if x['owner']=='247'}
 assert count==217 and kind==0x135 and {k:f[k].hex()for k in ['B8','B9','BA','BC']}==dict(B8='00',B9='00',BA='0500',BC='ffff')
 h,labels=alphabet(d[start:end]);assert h==[181,178,65535,180,184,182,183,217]
 assert labels['\U000f0001']==211 and labels['\ufffd']==209 and '\u0651'not in labels and '\U000f0009'not in labels
 sampler=next(c for c in children if c[0]=='130');graph=next(c for c in children if c[0]=='138');nn=next(c for c in children if c[0]=='140')
 assert [c[3]for c in children if c[0]=='processor']==[0x24a,0x24c,0x265] and d[graph[1]+16]==7
 resources.append(dict(selectedExponent=.75,path=name,resourceSha256=sha(d),selected=selected,source=[start,end],sourceFingerprint=sha(d[start:end]),policy={k:f[k].hex()for k in ['B8','B9','BA','BC']},header=h,sampler=sampler,samplerSha256=sha(d[sampler[1]:sampler[2]]),graph=graph,graphSha256=sha(d[graph[1]:graph[2]]),nn=nn,nnSha256=sha(d[nn[1]:nn[2]])))
assert resources[0]['sourceFingerprint']!=resources[1]['sourceFingerprint'] and resources[0]['graphSha256']!=resources[1]['graphSha256'] and resources[0]['nnSha256']!=resources[1]['nnSha256'] and resources[0]['samplerSha256']==resources[1]['samplerSha256']=='8e58024717f376c9cccb8626498eed00784a49fd43bc3df410e5ac613920b24c'
def bounds(g,a,n,e):
 assert 0<=g<=4096 and 0<=a<=4096 and 0<=n<=4096 and 0<=e<=1000000
 return dict(maxRequests=41*a,peak=2*1048576+8192*a+1536*n+512*g+16*e,byteWork=2*1048576+36864*a+4096*n+512*g+24*e)
maximum=bounds(4096,4096,4096,1000000);assert maximum['peak']<=64*1048576 and maximum['byteWork']<=192*1048576
captures=[]
for path in args.capture:
 assert sha(path.read_bytes())in ['9ac5e75e68bd25073bc4aba1c89ab74c6d8692a43042652585d862e0d8294804','d0c75b15e6c6455dc664e41e1ccb584eea4ee12c914947092140b4c3794b3c27']
 d=json.loads(path.read_bytes());assert d['TargetsRead']is False;rows=[]
 for row in d['Actual']:
  if row['Status']!='AcceptedInitializer':rows.append(dict(name=row['Name'],upstream=row['Status']));continue
  assert 1<=len(row['Children'])<=256
  g=a=n=e=visits=0;refusals=[];childrows=[]
  for c in row['Children']:
   assert all(1<=len(g['Stored'])<=15 and len(g['Entries'])<=150 for g in c['Groups'])
   nodes=c['Nodes'];nn=len(nodes);aa=sum(map(len,nodes));gg=len(c['Groups']);ee=0;maxD=0;maxP=0
   if not nn or not aa or any(not x for x in nodes):refusals.append('nonempty ordinary graph required')
   for node,arcs in enumerate(nodes):
    for arc in arcs:
     assert arc['Span']<=7 and arc['Span']<=node
     vec=collections.Counter(x[0]for x in arc['Successors']);ee+=len(arc['Successors']);maxD=max(maxD,max(vec.values(),default=0));maxP=max(maxP,len(arc['Pred']))
     if node==arc['Span']and-1 in arc['Pred']:ee+=1
   roots=collections.Counter(arc['Span']for node,arcs in enumerate(nodes)for arc in arcs if node==arc['Span']and-1 in arc['Pred']);maxD=max(maxD,max(roots.values(),default=0))
   q=41*aa;v=q*192+8*aa+15*gg+q*(1+5*(nn+maxD+4*maxP+32))+15*nn
   visits+=v;g+=gg;a+=aa;n+=nn;e+=ee;childrows.append(dict(nodes=nn,arcs=aa,groups=gg,entries=ee,maxDegree=maxD,maxPredecessors=maxP,visitUpper=v))
  try:v=bounds(g,a,n,e)
  except AssertionError:refusals.append('new cumulative structural admission');v=None
  if visits>128*1048576:refusals.append('new visit-work admission')
  rows.append(dict(name=row['Name'],status='AcceptedProposal'if not refusals else'RefusedProposal',reason=refusals,counts=dict(groups=g,arcs=a,nodes=n,entries=e),bounds=v,visitUpper=visits,children=childrows))
 captures.append(dict(path=str(path),sha256=sha(path.read_bytes()),total=len(rows),accepted=sum(r.get('status')=='AcceptedProposal'for r in rows),refused=sum(r.get('status')=='RefusedProposal'for r in rows),maxPeak=max((r['bounds']['peak']for r in rows if r.get('bounds')),default=0),maxByteWork=max((r['bounds']['byteWork']for r in rows if r.get('bounds')),default=0),maxVisits=max((r.get('visitUpper',0)for r in rows),default=0),rows=rows))
out=dict(scope='STATIC/proposal arithmetic only, not B3 runtime admission or NN output',resources=resources,limits=dict(groups=4096,arcs=4096,nodes=4096,entries=1000000,peak=64*1048576,byteWork=192*1048576,visits=128*1048576),maximumIndependentCounts=maximum,captures=captures,targetsRead=False,vendorExecuted=False);args.output.write_text(json.dumps(out,indent=2));print('PASS raw full/lite distinct owners, identical108 and bounded arithmetic',[(x['total'],x['accepted'],x['refused'],x['maxPeak'],x['maxVisits'])for x in captures])
