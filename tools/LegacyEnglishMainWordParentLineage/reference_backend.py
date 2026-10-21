"""Independent literal field/mask/frontend128/backend58 numeric projection.
Inputs are actual pooled D0 costs, but requests are PREPARED diagnostic controls;
this does not implement or authorize a dictionary query/word decoder.
"""
import argparse,ast,hashlib,json,struct
from pathlib import Path
p=argparse.ArgumentParser()
for name in ('repo','models','d0','geometry','actual','output'):p.add_argument('--'+name,type=Path,required=True)
a=p.parse_args();d=json.loads(a.d0.read_bytes());g=json.loads(a.geometry.read_bytes());actual=json.loads(a.actual.read_bytes())
assert d['TargetsRead'] is False and actual['GenuineQueryRows'] is False
catBytes=(Path(__file__).parent/'actual-category-reference.json').read_bytes();assert hashlib.sha256(catBytes).hexdigest()=='82ca0232e4005fc352f92c830b13bef5765b212619448ff49634ca14aa78502d';cat=json.loads(catBytes)
tree=ast.parse((a.repo/'tools/LegacyCaseAlphabetRanges.Probe/reference_controls.py').read_text(encoding='utf-8-sig'));tree.body=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))];scope={};exec(compile(tree,'raw-alphabet-reference','exec'),scope)
ak=(a.models/'en-US/en_US-ak-cur.lite.res').read_bytes();assert hashlib.sha256(ak).hexdigest()=='ca46297ada4a80de0d4bbd40650a8ace5c3809af9edbd493fb8f4eb692a18657'
al=scope['read'](ak[99:]);assert al['count']==217;labels={text:i for i,text in al['text'].items()};bypass={labels.get(chr(c),65535) for c in (0xf0001,0xf0002,0xf0003,0xf0004,0xf0008)}
geometry={r['Name']:dict(r['Groups'])for r in g['rows']};h=hashlib.sha256();cases=accepted=rejected=selected=finite=missing=0
wrap=lambda x:((x+2**31)%2**32)-2**31
for row in d['Rows']:
 boxes=geometry[row['Name']]
 for ps in row['Passes']:
  node=ps['Anchor'];span=ps['Span']
  for sel in ps['Selected']:
   selected+=1;arc=sel['Ordinal'];group=sel['Group'];costs=sel['Costs'];assert len(costs)==220;haveImpossible=False
   for symbol,cost in enumerate(costs[:217]):
    if cost==0x4000000:
     if haveImpossible:continue
     haveImpossible=True
    else:finite+=1
    for variant in range(2):
     category=cat['startComposite'if variant==0 else'endComposite'][symbol]
     if category<0:missing+=1;continue
     index=cat['compositePairs'][category][1];assert 0<=index<24
     for pattern in range(2):
      buf=bytearray((0xa5 if pattern==0 else 0x3c)^i for i in range(80))
      def put(offset,fmt,value):struct.pack_into('<'+fmt,buf,offset,value)
      for offset,value in [(0,node),(4,0xffffffff),(8,0xffffffff),(20,0xffffffff),(24,0xffffffff),(32,span),(48,0),(52,0x4000000),(64,6),(68,0)]:put(offset,'I',value)
      for offset,value in [(12,arc),(14,65535),(16,65535),(36,symbol),(38,65535)]:put(offset,'H',value)
      put(40,'Q',cases+1)
      for offset,value in [(18,index),(19,255),(28,0),(56,0),(57,0),(58,0),(72,0),(73,0)]:buf[offset]=value
      if cost==0x4000000:total=0x4000000;ok=False
      else:total=wrap(cost+(0 if symbol in bypass else boxes[group]));ok=total<0x4000000
      put(48,'i',total if ok else 0x4000000)
      h.update(buf);h.update(struct.pack('<iBiiiHH',total,int(ok),node,arc,group,symbol,category));cases+=1
      if ok:accepted+=1
      else:rejected+=1
expected=dict(Owners=len(d['Rows']),SelectedRows=selected,FiniteSymbols=finite,MissingCategoryDiagnosticCases=missing,Cases=cases,Accepted=accepted,Rejected=rejected,Digest=h.hexdigest())
for k,v in expected.items():assert actual[k]==v,(k,actual[k],v)
report=dict(status='PASS independent literal backend/frontend and complete80B residue masks',**expected,d0Sha256=hashlib.sha256(a.d0.read_bytes()).hexdigest(),geometrySha256=hashlib.sha256(a.geometry.read_bytes()).hexdigest(),targetsRead=False,nativeExecuted=False,genuineQueryRows=False)
a.output.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8');print('PASS',cases,'PREPARED numeric projections from actual D0 rows; no decoder authority')
