"""Independent actual global INPUT identity export, swap and failure-prefix oracle."""
from pathlib import Path
import json,runpy,struct,sys
root=Path(__file__).resolve().parents[2]
if len(sys.argv)not in(9,10):raise SystemExit('MODELS CHAR PARENT COVERAGE SELECTED INDEX CLEANUP TRANSFORMS [NATIVE]')
models,char,parent,coverage,selected,index,cleanup,transforms=sys.argv[1:9];saved=sys.argv[:]
try:
 sys.argv=['audit_output.py',models,char,parent,coverage,selected,index,cleanup]
 runpy.run_path(str(root/'tools/LegacyGlobalControlCleanup.Probe/audit_output.py'),run_name='__main__')
finally:sys.argv=saved
parents={s['name']:s for s in json.loads(Path(parent).read_bytes())['actual']}
prior={s['name']:s for s in json.loads(Path(cleanup).read_bytes())['samples']}
raw_counts={s['name']:s['rawCounts']for s in json.loads(Path(char).read_bytes())['samples']}
d=json.loads(Path(transforms).read_bytes());assert len(d['samples'])==13
def prefix(stage,swapped=False,exported=False,following=False,count=0):return dict(stage=stage,swapped=swapped,exported=exported,following=following,count=count)
normal=[prefix('entered'),prefix('exported'),prefix('swapped',True,True),prefix('input-spans',True,True,True),prefix('complete',True,True,True)]
total=0
for sample in d['samples']:
 p=parents[sample['name']];old=prior[sample['name']];assert sample['completed']==old['completed']
 if not old['completed']:assert sample==dict(name=sample['name'],completed=False,refusal=old['refusal']);continue
 ranges=p['textKey'];assert sample['ranges']==ranges
 #Every selected original INPUT in this fixed fixture owns its complete raw
 #objects. Full-range endpoint fields independently recover their lengths.
 fragments=[]
 for a,b in ranges:
  assert a&0xffffffff==3 and b&0xffff==0xff05
  counts=raw_counts[sample['name']]
  assert 0<=a>>32<=b>>32<len(counts)and((b>>16)&65535)+1==counts[b>>32]
  for source in range(a>>32,(b>>32)+1):fragments.append([source,counts[source],0,-1,0,0,1,1])
 assert sample['fragments']==fragments and sample['matches']==list(range(len(fragments)))
 assert sample['matrixCount']==len(fragments)
 assert bytes.fromhex(sample['matrices'])==struct.pack('<6d',1,0,0,0,1,0)*len(fragments)
 assert (sample['tag'],sample['registry'])==(1,1)
 assert bytes.fromhex(sample['seed20'])==struct.pack('<qiii',1,0,0,0)
 assert(sample['alternatives'],sample['lines'],sample['transforms'],sample['daf'])==(p['documentAlternativeCount'],p['lineCount'],0,0)
 assert sample['swapped']and sample['following']and sample['prefixes']==normal
 total+=len(fragments)
# Prior fault/export/invalidations unchanged; one extra genuine owner runs7 guards:121+7=128.
assert(d['completed'],d['refused'],d['matrices'],total,d['guards'])==(12,1,13,13,128)
for at,f in enumerate(d['failures']):
 assert f['stage']==['Entered','Exported','Swapped','InputSpans','Complete'][at]
 kept=normal[:min(at+1,4)];assert f['prefixes']==kept+[dict(kept[-1],stage='fault')]
 assert f['retainedCount']==0
assert len(d['failures'])==5 and len(d['failedExports'])==3
for n,f in enumerate(d['failedExports']):
 assert f['count']==n and bytes.fromhex(f['scalars'])==bytes((i*17+3)&255 for i in range(n*64))
 assert f['prefixes']==normal[:2]+[prefix('swapped',True,False,False,n),prefix('export-failed',True,False,False,n)]
assert len(d['invalidations'])==2
for disposed,f in zip((False,True),d['invalidations']):
 assert f['disposed']==disposed and f['prefixes']==normal[:4]+[dict(normal[3],stage='fault')]
assert len(d['preSwap'])==3
for n,f in enumerate(d['preSwap']):assert f['kind']==n and f['prefixes']==normal[:2]+[prefix('fault')]
if len(sys.argv)==10:assert Path(transforms).read_bytes()==Path(sys.argv[9]).read_bytes(),'Managed/Native mismatch'
print('PASS independent12 original-selected global INPUT owners/13 identity matrices/real seed-vector swap/5 cancellation prefixes/3 failed exports/5 owner invalidations/128 guards; no final acceptance.')
