"""Independent C21 WORD-key union, exact VE8 and source-owned branch checks."""
import base64,importlib.util,json,struct,sys
from pathlib import Path
root=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('ops',root/'tools/LegacyRawInkRangeOperations.Probe/reference_controls.py');ops=importlib.util.module_from_spec(spec);spec.loader.exec_module(ops)
j=json.loads(Path(sys.argv[1]).read_bytes());b64=base64.b64decode
for s in j['samples']:
 assert s['selected']==0 and s['metadataEnabled']is False and s['sourceOffset']==0 and s['sourceNodes']==1
 alt=b64(s['selectedAlternative32']);assert len(alt)==32 and struct.unpack_from('<i',alt,24)[0]==1
 #Native027670 wordCount1 returns before selecting a WORD or reading gap flags.
 union=ops.union([],s['wordSourceKey']);assert union==s['wordUnion']==s['textKey']
 assert b64(s['beforeLines56'])==b64(s['afterLines56'])and len(b64(s['afterLines56']))==56
 assert s['pendingBefore']==s['pendingAfter']and s['lastPending']=='TEXT'
for c in j['preparedEqualityDiscriminators']:
 #081700 compares raw count,firstQWORD,lastQWORD. A normalized search can
 #contain both while VE8 still returns false; these are explicitly prepared.
 assert c['equal']==(c['left']==c['right'])is False
 x,y=c['right'][0]
 assert c['contains']==any(ops.n(a)<=ops.n(x)<=ops.n(b)and y<=b for a,b in c['left'])is True
assert len(j['samples'])==4 and len(j['preparedEqualityDiscriminators'])==256
print('PASS independent4 actual source owners/orderedWORDunion/exact16B TEXT-key equality/preserved56B+commands/256 explicit raw-equality discriminators; no labels or acceptance.')
