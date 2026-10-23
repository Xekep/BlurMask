"""Strict DATA regression against the independently reconstructed prior graph2.
Not a generic wholegraph/quality/resource or live-owner oracle.
"""
from pathlib import Path
import argparse,hashlib,json
p=argparse.ArgumentParser();p.add_argument('capture',type=Path);p.add_argument('baseline',type=Path);p.add_argument('output',type=Path);a=p.parse_args()
assert not a.output.exists()
raw=a.baseline.read_bytes();assert hashlib.sha256(raw).hexdigest()=='0cee3661348b9db5be53f7fd694c5d1997c8d27c923109db90e5cb921854eb0b'
before=json.loads(raw);d=json.loads(a.capture.read_bytes())
assert d['Scope']=='ACTUAL_OWNING_ENGLISH_GENERIC_LOOP_FIRST_LANGUAGE_PHASE1_NOT_FINAL_WORD'
assert d['Completed'] is True and d['NoExpectedLabels']is True and d['Admission']=='NONE'and d['GlobalReadinessGateUnchanged']is True
for k in('InputSha','OriginalIndex','RawPoints','RawStrokes','Resolution','GraphNodes','ParentChildren'):assert type(d[k])is type(before[k])and d[k]==before[k],k
g=d['GraphNode'];prior=before['GraphNode'];assert set(g)==set(prior)
for k in('Node','SourceEnd','RetainedEnd','OriginalT','InitializedN'):assert type(g[k])is int
assert all(type(x)is int for x in g['Ordinals156'])
assert g==prior,'Every250slot/156row/primary80B/mask/ordinal byte must match prior independent FIRST+phase1 reconstruction'
m=d['Metrics'];expected={'Nodes':2,'CompletedNodes':2,'QueryRows':before['QueryRows'],'QueryPages':before['QueryPages'],'ScoringCalls':before['ScoringCalls'],'AcceptedAllocations':before['AcceptedAllocations'],'SelectedD0Rows':before['SelectedD0Rows']}
assert set(m)==set(expected)and all(type(v)is int for v in m.values())and m==expected
report={'status':'PASS_STRICT_GRAPH2_DATA_REGRESSION','captureSha256':hashlib.sha256(a.capture.read_bytes()).hexdigest(),'baselineSha256':hashlib.sha256(raw).hexdigest(),'exactGraphFields':list(g),'metrics':m,'scope':'Only identical originalindex4 graph2 versus prior independently reconstructed data; not wholegraph/later-history/finalword/quality/lifecycle or complete resource certification'}
a.output.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8');print(json.dumps(report))
