"""Post-run failure-prefix audit against pre-NN algorithm and pinned stage sites.
Does not modify frozen positive oracle or predict engine recognition outcomes.
"""
from pathlib import Path
import json,hashlib,sys
from repeat_export_oracle import dom_export,source_export
from audit_repeat85 import treecheck

def audit(path):
 cap=json.loads(path.read_bytes());case=cap['Actual'][0];full=case['Repeat']
 old,new=full['OriginalBefore'],full['LateBefore'];ot,nt=full['OriginalTree'],full['LateTree']
 expected=dom_export(old,new,treecheck(old,ot),treecheck(new,nt),ot,nt)
 sources=source_export(case['CurrentInput']['Registry'],case['Late']['Observed']['Registry'],case['CurrentInput']['Next'],case['Late']['Observed']['Next'])
 # Scratch operands are allocated before serialization. Native-derived clone
 # numbering fixes their boundary independently of observed failed outputs.
 payloads=[d['PayloadRoot']for d in expected['Dom']if d['PayloadRoot']]
 scratch_end=min(payloads)-1
 firstroot=expected['RawActions'][0]['NodeAt20'];assert firstroot
 first_ids={firstroot}
 for node in expected['FourthWriter']:
  if node['Parent']in first_ids:first_ids.add(node['Id'])
 first_end=max(first_ids)
 failures=cap['ExportFailures'];assert len(failures)==6 and {f['Mode']for f in failures}==set(range(6))
 results=[]
 for f in failures:
  mode=f['Mode'];g=f['Snapshot']
  assert g['Failed']and not(g['Ready']or g['Released'])
  assert g['Complete']==(mode==5)
  assert g['OriginalBefore']==old and g['LateBefore']==new
  assert g['OriginalTree']==ot and g['LateTree']==nt
  assert f['OriginalAfter']==old
  assert f['OriginalCurrent']==case['CurrentInput']
  candidate=f['Candidate'];observed=case['Late']['Observed']
  for k in ('Registry','Pending','Temporary'):assert candidate[k]==observed[k],(mode,k)
  assert candidate['RegistryNext']==observed['Next']
  assert g['Geometry']==case['Identity']
  assert g['OldNext']==case['CurrentInput']['Next']and g['NewNext']==observed['Next']
  assert g['Epoch']==case['CurrentInput']['AppliedEpoch']
  assert g['ComparisonProfile']=='admitted-library-default-schema0/native-local88-unresolved'
  assert g['DistinctFourthNames']and g['SameCurrentProjection']
  raw=expected['RawActions'][:1]if mode==0 else expected['RawActions']
  dom=[]if mode in (0,4)else expected['Dom']
  fourth=expected['FourthWriter'][:first_end]if mode==0 else expected['FourthWriter'][:scratch_end]if mode==4 else expected['FourthWriter']
  released=[]if mode in (0,4)else expected['ReleasedScratchRoots']
  source=[]if mode in (0,1,4)else sources[:1]if mode in (2,3)else sources
  for k,v in [('RawActions',raw),('Dom',dom),('FourthWriter',fourth),('ReleasedScratchRoots',released),('Source',source)]:assert g[k]==v,(mode,k)
  assert g['OriginalAfter']==(old if mode==5 else [])
  assert g['LateAfter']==(new if mode==5 else [])
  results.append(dict(Mode=mode,RawActions=len(raw),Dom=len(dom),Source=len(source),FourthNodes=len(fourth),Complete=g['Complete'],Failed=True,Ready=False,OriginalOwnersUnchanged=True))
 return dict(PASS=True,CaptureSha256=hashlib.sha256(path.read_bytes()).hexdigest(),Scope='Failure prefixes derived from frozen algorithm plus pre-NN callback stage ordering; no original receiver/apply or native local88 proof',Cases=results)
if __name__=='__main__':
 report=audit(Path(sys.argv[1]));Path(sys.argv[2]).write_text(json.dumps(report,indent=2)+'\n');print(json.dumps(report))
