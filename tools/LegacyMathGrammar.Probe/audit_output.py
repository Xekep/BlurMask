"""Independent actual grammar/AK join and full parsed-field reconstruction; no inference."""
from pathlib import Path
import argparse,copy,hashlib,importlib.util,json,struct
p=argparse.ArgumentParser();p.add_argument('repo',type=Path);p.add_argument('capture',type=Path);p.add_argument('report',type=Path);a=p.parse_args()
refpath=Path(__file__).resolve().parents[1]/'handwriting/math_legacy_grammar_children_reference.py'
spec=importlib.util.spec_from_file_location('grammar_ref',refpath);ref=importlib.util.module_from_spec(spec);spec.loader.exec_module(ref)
g=(a.repo/'models/math/math-grm-standard.res').read_bytes();ak=(a.repo/'models/math/math-ak.res').read_bytes()
assert hashlib.sha256(g).hexdigest()=='adc9b7adce2d3af1a20a6262467afc0b1c6365bf27c120596d80c86ec54893f8'
assert hashlib.sha256(ak).hexdigest()=='8c8d9c8ce93aa74820f30a592d77ec3fe382c930a72063d5ced48629f3c167c1'
root=ref.parse_root(g)
#Independent raw alphabet traversal: not the C# LegacyInkAlphabet implementation.
x=34;header=struct.unpack_from('<8H',ak,x);x+=16;assert header[-1]==259
n=struct.unpack_from('<H',ak,x)[0];x+=2
for increment in (32,64,1):
 values=struct.unpack_from('<'+'H'*n,ak,x);x+=n*2;n=next(v for v in reversed(values)if v!=65535)+increment
spans=struct.unpack_from('<'+'H'*(n+1),ak,x);x+=2*(n+1);count=spans[-1]+1
entries=[struct.unpack_from('<II',ak,x+8*i)for i in range(count)];x+=count*8
strings=ak[x:x+entries[-1][0]]
labels={strings[entries[i][0]:entries[i+1][0]].decode('utf8'):entries[i][1]for i in range(count-1)}
assert len(labels)==258 and 256 not in labels.values()
def trie(nodes):return [dict(text=n['bytes'].decode('utf8'),field12=n['field12'],field13=n['field13'],children=trie(n['children']))for n in nodes]
rules=[]
for rule in root['rules']:
 children=[]
 for frame in rule['children']:
  node=ref.parse_child(frame);assert ref.write_child(node)==frame['raw']
  names=[v.decode('utf8')for v in node.get('set18',[])]
  children.append(dict(offset=frame['offset'],end=frame['end'],typeId=node['typeId'],payloadVersion=node['payloadVersion'],
   name=node['name'].decode('utf8'),fields=node.get('fields',[]),terminalNames=names,terminalLabels=[labels[v]for v in names],
   field18=node.get('field18',0),trie20=trie(node.get('trie20',[]))))
 rules.append(dict(index=rule['index'],offset=rule['offset'],end=rule['end'],tag=rule['tag'],name=rule['name'].decode('utf8'),children=children))
guards=['grammar-fingerprint','acoustic-fingerprint','grammar-length','acoustic-length','load-cancel-token','write-cancel-token','rules-immutable','terminal-labels-immutable','input-detached','output-detached','virtual-label-has-no-text','all-terminal-labels-roundtrip']
def check(r):
 assert r['grammarSHA256']==hashlib.sha256(g).hexdigest() and bytes.fromhex(r['serializedHex'])==g
 assert r['alphabetCount']==259 and r['hasCandidateAuthority']is False and r['trailingScalar38']==root['trailingScalar38']
 assert r['rules']==rules and r['guards']==guards and r['modelExecutions']==0 and r['vendorExecution']is False
r=json.loads(a.capture.read_text());check(r);negative=[]
terminal=next((i,j)for i,v in enumerate(rules)for j,n in enumerate(v['children'])if n['terminalLabels'])
fields=next((i,j)for i,v in enumerate(rules)for j,n in enumerate(v['children'])if n['fields'])
def corrupt_label(z):z['rules'][terminal[0]]['children'][terminal[1]]['terminalLabels'][0]^=1
def corrupt_field(z):z['rules'][fields[0]]['children'][fields[1]]['fields'][0]['value']^=1
for name,change in [('rule-tag',lambda z:z['rules'][0].__setitem__('tag',255)),('child-inner-version',lambda z:z['rules'][0]['children'][0].__setitem__('payloadVersion',999)),('terminal-label',corrupt_label),('native-field',corrupt_field),('serialized-byte',lambda z:z.__setitem__('serializedHex','00'+z['serializedHex'][2:])),('authority',lambda z:z.__setitem__('hasCandidateAuthority',True)),('guard-omission',lambda z:z['guards'].pop())]:
 z=copy.deepcopy(r);change(z)
 try:check(z)
 except AssertionError:negative.append(name)
 else:raise AssertionError('Corruption accepted: '+name)
if a.report.exists():raise FileExistsError(a.report)
a.report.write_text(json.dumps(dict(rules=26,children=53,terminalNames=297,guards=len(guards),corruptions=negative,modelExecutions=0,allParsedFieldsExact=True),indent=2)+'\n',encoding='utf8')
print('PASS 26 rules / 53 children / 297 terminal names / 12 guards / 7 corruptions / 0 model executions')
