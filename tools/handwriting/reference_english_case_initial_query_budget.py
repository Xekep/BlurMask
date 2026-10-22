"""Independent raw EN first-query control closure and bounded shape work.
No target text, NN scores, vendor loading or runtime authority.
"""
from pathlib import Path
import argparse,hashlib,json,runpy,sys
p=argparse.ArgumentParser();p.add_argument('--repo',type=Path,required=True);p.add_argument('--models',type=Path,required=True);p.add_argument('--output',type=Path,required=True);a=p.parse_args();a.output.parent.mkdir(parents=True,exist_ok=True);prefix='eic-'+hashlib.sha256(str(a.output.resolve()).encode()).hexdigest()[:12]
sys.argv=['reference',str(a.models),str(a.output.with_name(prefix+'-packed.json'))];packed=runpy.run_path(str(a.repo/'tools/LegacyEnglishLkPacked102.Probe/reference_actual.py'),run_name='__main__')
sys.argv=['reference',str(a.models),str(a.output.with_name(prefix+'-case.json'))];case=runpy.run_path(str(a.repo/'tools/LegacyEnglishCaseMain.Probe/reference_case_main.py'),run_name='__main__')
primary=case['primary'];assert len(primary)==104
#FFFF/FFFD are denied/null, not epsilon links. Every admitted control edge
#must be obtained from the actual raw lexical-to-acoustic map.
control={i:x for i,x in enumerate(primary)if x>=217 and x not in(65535,65533)}
assert control=={90:65531}
labels=packed['labels'];counts=packed['counts'];starts=packed['starts'];mask=packed['labelMask'];n=packed['n'];depth=packed['depth'];chain=[];node=0
for level in range(8):
 start=starts[node]+(node==0);end=starts[node]+counts[node];children=list(range(start,end));assert len(children)<=104
 epsilon=[c for c in children if(labels[c]&mask)in control];assert len(epsilon)<=1
 chain.append(dict(level=level,node=node,rawChildren=len(children),controlChild=epsilon[0]if epsilon else None,maximumRawLabel=max((labels[c]&mask for c in children),default=-1)))
 if not epsilon:break
 node=epsilon[0]
#All nodes still obey the exact distinct-label grammar, independently checked
#by the packed reference, so the source-specific admitted control is nonbranching.
controls_total=sum((labels[i]&mask)in control for i in range(1,n));largest=max(counts)
#This is a structural certificate; live permissions can only deny transitions.
#Five is exact maximum CountForSkip/Expand alternative count (bothcase kind2).
assert len(chain)==1 and chain[0]['controlChild']is None
rootkeys=[primary[labels[i]&mask]for i in range(starts[0]+1,starts[0]+counts[0])];assert all(k==65535 or 0<=k<217 for k in rootkeys)
raw_records=sum(row['rawChildren']for row in chain);case_records=raw_records*5
assert raw_records==80 and 89+3*case_records==1289<2048
result=dict(status='PASS actual raw initial-control closure',nodeCount=n,allDistinctOrderedChildren=True,maximumRawChildren=largest,admittedControlLexicalToAcoustic=control,controlOccurrences=controls_total,initialRootChildSkipped=True,initialSourceTag='0x0000080000000000',initialParameter1c=0,initialParameter20=0,initialFlags24=0,initialFFFBPrefixTaken=False,zeroStateMayBeNumericallySimilarButNotIssued=True,outerDepthLimit=8,actualControlChain=chain,rawRecordsUpper=raw_records,caseRecordsUpper=case_records,caseAlternativesPerRawUpper=5,queryCapacity=128,initialParentContext=0,category18=0,fullNativeQueryOrWordClaim=False,rootMappedContextMaximumDepth=1,caseRewriteLookupsUpper=case_records,delimiterPrefixLookupsUpper=case_records,queryLookupAttemptsUpper=2*case_records,queryNodeUpper=89+2*case_records,ordinaryGraftAdditionalUpper=case_records,firstConsumerNodeUpper=89+3*case_records,proposedPrivateExecutionCapacity=2048,queryPageCallsUpper=(case_records+127)//128+1,contextInsertionBoundScope='first-root-query-only/no-control-edge/null-parent/single-symbol-map',targetsRead=False,vendorExecuted=False)
a.output.write_text(json.dumps(result,indent=2)+'\n');print(json.dumps(result))
