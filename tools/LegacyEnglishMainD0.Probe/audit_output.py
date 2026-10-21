"""Independent native pooled selection/permission/map oracle, no SDK execution.
N providers must match an independently audited immutable corrected N capture.
No expected handwriting targets are read.
"""
from pathlib import Path
import argparse,ast,hashlib,json,struct,subprocess,sys,tempfile,random
p=argparse.ArgumentParser();p.add_argument('--models',type=Path,required=True);p.add_argument('--capture',type=Path,required=True);p.add_argument('--acoustic',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args();root=Path(__file__).resolve().parents[2]
sha=lambda b:hashlib.sha256(b).hexdigest();captureBytes=a.capture.read_bytes();actual=json.loads(captureBytes);nnBytes=a.acoustic.read_bytes()
assert sha(nnBytes)in ['1eeff7449c49e7056b0f61ed71f6f89b83df03fef641def987cf63b225853af0','62d7caedc1ef6a248dd45ae638c57a74cc06b7cedd83529a4b10b1a4cd6ad9db']
nn=json.loads(nnBytes);assert actual['TargetsRead']is False and nn['TargetsRead']is False and actual['InputSha256']==nn['InputSha256']
assert actual['Guards']==42 and actual['Cancellations']==6
assert [r['Name']for r in actual['Rows']]==[r['Name']for r in nn['Rows']]and actual['Refused']==nn['MultilineRefused']
with tempfile.TemporaryDirectory(prefix='blurm-d0-',dir=a.output.parent)as temporary:
 refPath=Path(temporary)/'actual-maps.json';subprocess.run([sys.executable,'-I','-B',str(root/'tools/LegacyEnglishMainMasks.Probe/reference_actual.py'),str(a.models),str(refPath)],check=True);ref=json.loads(refPath.read_text())
primary=ref['sourceMaps']['primary'];assert ref['sourceMaps']['descriptor']==0xc0e
tree=ast.parse((root/'tools/LegacyCaseAlphabetRanges.Probe/reference_controls.py').read_text(encoding='utf-8-sig'));tree.body=[n for n in tree.body if isinstance(n,(ast.Import,ast.ImportFrom,ast.FunctionDef,ast.ClassDef))];scope={};exec(compile(tree,'raw-alphabet-reference','exec'),scope)
ak=(a.models/'en-US/en_US-ak-cur.lite.res').read_bytes();al=scope['read'](ak[99:]);assert al['count']==217;labels={text:i for i,text in al['text'].items()};header=struct.unpack_from('<8H',ak,99)
reserved=list(header[4:7])+[labels.get(text,65535)for text in ['\U000f0000','\u200c','\u200d']]
IMP=0x4000000
def f(x):return struct.unpack('<f',struct.pack('<f',x))[0]

class Pool:
 def __init__(self,providers):self.providers=providers;self.entries=[];self.records={}
 def run(self,groups,span,allow=False,present=None):
  present=set(groups)if present is None else set(present)
  for entry in self.entries:
   if entry['span']==span and entry['group']not in present:entry['group']=-1
  callbacks=[];computed=0;reused=0
  for group in groups:
   record=self.records.setdefault(group,dict(computed=False,entry=None,scale=0))
   recompute=not(allow and record['computed'])and not any(e['group']==group for e in self.entries)
   if recompute:
    entry=next((e for e in self.entries if e['group']==-1),None)
    if entry is None:entry=dict(group=-1,span=0,costs=[0]*220);self.entries.append(entry)
    entry['group']=group;entry['span']=span;record['entry']=entry;record['computed']=True
    costs,scale=self.providers[group];entry['costs'][:]=costs;record['scale']=scale;computed+=1
   else:reused+=1
   callbacks.append((list(record['entry']['costs']),record['scale']))
  current=[list(self.records[g]['entry']['costs'])for g in groups]
  return callbacks,current,computed,reused

# Prepared adversarial alias: reuse=true can retain A's pointer while a later
# callback recycles that buffer. This is why the production bridge refuses it.
prepared=Pool({i:([i+1]*220,1)for i in range(3)})
prepared.run([0],0);prepared.run([1],0);callbacks,current,_,_=prepared.run([0,2],0,True)
assert callbacks[0][0]!=current[0]
# The admitted false branch must restore/occupy every selected group's own
# entry. Prove it under independently generated selector and old-alias states.
rng=random.Random(317951);checks=0
for trial in range(256):
 state=Pool({i:([i*13+1]*220,1)for i in range(12)})
 for step in range(64):
  groups=rng.sample(range(12),rng.randrange(9));span=rng.randrange(8);allow=step%3==0
  callbacks,current,_,_=state.run(groups,span,allow)
  if not allow:assert [c for c,s in callbacks]==current;checks+=1

nnRows={r['Name']:r for r in nn['Rows']};passes=0;selected=0;empty=0;reused=0
for row in actual['Rows']:
 n=nnRows[row['Name']];assert [[x[:2]for x in node]for node in row['Graph']]==n['Graph']
 assert row['InitialMap']==primary and row['InitialSource68']==row['FinalSource68']and len(row['InitialSource68'])==len(primary)
 assert [v!=0 for v in row['InitialSource68']]==ref['sourceMaps']['contextMapNonzero']
 coordinates=[(node,span)for node,arcs in enumerate(row['Graph'])for span in sorted({arc[1]for arc in arcs})]
 assert coordinates
 assert [(r['Anchor'],r['Span'])for r in row['Passes']]==coordinates+list(reversed(coordinates))+[(0,7),coordinates[0]]
 ng={g['Group']:g for g in n['Groups']};providers={}
 for provider in row['Provider']:
  g=ng[provider['Group']];assert provider['Costs']==g['Costs']and provider['Span']==g['Span']and f(provider['Scale'])==f(g['PowerScale']);providers[provider['Group']]=(provider['Costs'],f(provider['Scale']))
 assert len(providers)==len(ng);state=Pool(providers);previousEpoch=0
 for result in row['Passes']:
  anchor,span=result['Anchor'],result['Span'];arcs=row['Graph'][anchor];ordinals=[i for i,x in enumerate(arcs)if x[1]==span];groups=[arcs[i][0]for i in ordinals]
  # Native invalidation sees ALL anchor groups, not just span-selected groups.
  present={x[0]for x in arcs}
  callbacks,current,computed,used=state.run(groups,span,present=present)
  assert [c for c,s in callbacks]==current
  assert result['Computed']==computed and result['Reused']==used and result['Epoch']==previousEpoch+1;previousEpoch=result['Epoch']
  assert len(result['Selected'])==len(groups)
  permission=[0]*217;eligible=[0]*220
  for label in reserved:
   if label!=65535:permission[label]=3
  for position,group,(costs,scale),selectedRow in zip(ordinals,groups,callbacks,result['Selected']):
   assert selectedRow['Ordinal']==position and selectedRow['Group']==group and selectedRow['Costs']==costs
   threshold=int(f(f(scale)*40000))
   for symbol,cost in enumerate(costs):
    if cost<IMP:eligible[symbol]=1
    if symbol<217:permission[symbol]|=(3 if cost<IMP else 0)|(4 if cost<threshold else 0)
  working=[value if value>=217 or permission[value]&1 else 65535 for value in primary]
  assert permission==result['Permissions']and eligible==result['Eligibility']and working==result['Working'],(row['Name'],anchor,span,'permissions/union/map')
  passes+=1;selected+=len(groups);empty+=not groups;reused+=used
 assert row['Operations']==len(row['Passes'])
assert passes==actual['PassCount']and selected==actual['SelectedRowCount']and actual['Cancellations']==6
report=dict(status='PASS',scope='Actual rebuilt D0/pool/permissions/union/source50 refresh only, no request/worddecoder/nativeSDK',owners=len(actual['Rows']),passes=passes,selectedRows=selected,emptySelections=empty,reusedRows=reused,guards=actual['Guards'],cancellationStages=6,preparedFalseInvariants=checks,preparedTrueAliasRefused=True,captureSha256=sha(captureBytes),auditedNProviderSha256=sha(nnBytes),targetsRead=False,nativeExecuted=False)
a.output.write_text(json.dumps(report,indent=2),encoding='utf8');print(json.dumps(report))
