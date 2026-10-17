"""Independent actual ordered position bytes -> node identity/tree audit."""
import argparse,base64,importlib.util,json,struct
from pathlib import Path

root=Path(__file__).resolve().parents[2]
spec=importlib.util.spec_from_file_location('source_node_avl_oracle',Path(__file__).with_name('reference_controls.py'))
oracle=importlib.util.module_from_spec(spec);spec.loader.exec_module(oracle)
p=argparse.ArgumentParser(description=__doc__);p.add_argument('capture',type=Path);a=p.parse_args()
capture=json.loads(a.capture.read_bytes());keys=[]
for row in capture['actual']:
    raw=base64.b64decode(row['ranges16']);assert len(raw)%16==0
    keys.append(list(struct.iter_unpack('<QQ',raw)))
expected=oracle.simulate(keys)
for actual,wanted in zip(capture['actual'],expected['answers']):
    assert [actual['identity'],actual['inserted']]==wanted
assert capture['root']==expected['root'] and capture['tree']==expected['tree']
print(f"PASS{len(keys)} actual source-position byte keys/{len(expected['tree'])} native source nodes, identical identities/AVL/list; no source payload inferred.")
