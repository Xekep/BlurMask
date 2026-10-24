"""Read-only PE/native ordinary sourceв†’metadata and typed C# admission proof.
No vendor loading, execution, analysis database or third-party disassembler.
"""
import argparse,hashlib,json,struct
from pathlib import Path
p=argparse.ArgumentParser();p.add_argument('--repo',type=Path,required=True);p.add_argument('--sdk',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args()
here=Path(__file__).resolve().parent;proof=json.loads((here/'native-pins.json').read_bytes());b=(a.sdk/'MyScriptText.dll').read_bytes();sha=lambda x:hashlib.sha256(x).hexdigest();assert sha(b)==proof['sha256']
pe=struct.unpack_from('<I',b,0x3c)[0];assert b[pe:pe+4]==b'PE\0\0';sections=struct.unpack_from('<H',b,pe+6)[0];optional=pe+24;assert struct.unpack_from('<H',b,optional)[0]==0x20b;base=struct.unpack_from('<Q',b,optional+24)[0];table=optional+struct.unpack_from('<H',b,pe+20)[0]
def read(at,n):
 rva=at-base
 for i in range(sections):
  at=table+i*40;virtual,addr,rawsize,raw=struct.unpack_from('<IIII',b,at+8)
  if addr<=rva and rva+n<=addr+rawsize:return b[raw+rva-addr:raw+rva-addr+n]
 raise ValueError('Pinned initialized PE span outside actual sections')
for r in proof['regions']:
 at=int(r['start'],16);end=int(r['endExclusive'],16);assert end-at==r['bytes'] and sha(read(at,end-at))==r['sha256']
for r in proof['calls']+proof['writeAndCopyWitnesses']+proof['bindings']+proof['data']:
 raw=bytes.fromhex(r['bytes']);assert read(int(r['at'],16),len(raw))==raw
assert proof['leaf']['bytes']=='32c0c3'and read(0x18005a400,3)==bytes.fromhex('32c0c3')
def restored_source(repo,path,expected):
 import runpy
 helper=repo/'tools/LegacyEnglishCharacters.Probe/source_preimage.py'
 assert hashlib.sha256(helper.read_bytes().replace(b'\r\n',b'\n')).hexdigest()=='3839cc530b618f397eb1757f02455f124053561f3ab8f1f77a46d391e4b76d12'
 return runpy.run_path(str(helper))['restore'](Path(path).name,(repo/path).read_bytes(),expected)
for r in proof['sourceDependencies']:
 data=(a.repo/r['path']).read_bytes()
 if Path(r['path']).name=='LegacyOrdinaryGraphSourceRanges.cs':data=restored_source(a.repo,r['path'],r['sha256'])
 assert sha(data)==r['sha256']
local=(a.repo/'src/InkRecognition/LegacyEnglish119MainLocalCandidates.cs').read_text(encoding='utf-8-sig');seed=(a.repo/'src/InkRecognition/LegacyOrdinaryCursiveSeedProducer.cs').read_text(encoding='utf-8-sig')
compact=lambda s:''.join(s.split())
assert 'GenerateLeftToRight(cut,Height,seedProfile,newbyte[points.Length],Enumerable.Repeat(-1,points.Length).ToArray(),token)'in compact(local)
assert 'if(sourceDirectionFlags[i]!=0||physicalBoundaryIds[i]!=-1)thrownewNotSupportedException' in compact(seed)
assert 'thrownewNotSupportedException' in compact(seed)
assert 'raw.WithSamplerPruning(pruned,token)'in local and 'LegacyOrdinaryGraphSourceRanges.PrepareBounded(seed,map,original,8,4096,workBudget,token)'in compact(local)
#Compose immutable upstream ordinary status/fresh binding/active geometry and
#lifecycle pointer/mode proofs by actual PE bytes, not their C# cached values.
upstream=[]
for relative in ['tools/handwriting/english-main-word-policy-transition-pins.json','tools/handwriting/english-word-lifecycle-pins.json']:
 q=json.loads((a.repo/relative).read_bytes());assert q['sha256']==proof['sha256']
 for r in q['regions']:
  start=int(r['start'],16);end=int(r['endExclusive'],16);assert sha(read(start,end-start))==r['sha256']
 for name in ['calls','leaves','tables','parents']:
  for r in q.get(name,[]):
   raw=bytes.fromhex(r['bytes']);assert read(int(r.get('at',r.get('start')),16),len(raw))==raw
 upstream.append(dict(path=relative,fullBodies=q.get('fullBodies'),regions=len(q['regions'])))
report=dict(status='PASS static producer-derived ordinary group metadata admission',fullBodies=len({r['function']for r in proof['regions']}),completeRegions=len(proof['regions']),instructions=sum(r['instructions']for r in proof['regions']),calls=len(proof['calls']),writeCopyWitnesses=len(proof['writeAndCopyWitnesses']),dispatchBindings=len(proof['bindings']),unregisteredFalseLeaf=True,upstream=upstream,
 distinction='arc.Flags differs from wideword6(kind7) and wide50(compact6). Ordinary19F850 zero96 and merged1A3B30 zero50;19C4E0 copies50 to6 and7 to7. Native type1=0100/50=1 not admitted by sealed ordinary0/-1 MainLocal producer.',targetsRead=False,vendorExecuted=False)
a.output.write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report,ensure_ascii=True))
