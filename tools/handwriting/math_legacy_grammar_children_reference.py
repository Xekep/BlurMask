"""Read-only actual-version grammar child schema reference, not a matcher.

Fields use native storage offsets; no names or semantic meaning are invented.
Reconstruction preserves the input version. Native latest writers can upgrade it.
Strict budgets/header admission are analysis policy, not native error parity.
"""
import argparse,hashlib,importlib.util,json,struct
from pathlib import Path
_spec=importlib.util.spec_from_file_location('math_legacy_root',Path(__file__).with_name('math_legacy_grammar_root_reference.py'))
_root=importlib.util.module_from_spec(_spec);_spec.loader.exec_module(_root)
parse_root,write_root=_root.parse_root,_root.write_root

LAYOUTS={
 0x13a9:((0x18,4),),
 0x13aa:((0x18,4),(0x1c,4),(0x20,1),(0x21,1)),
 0x13ac:((0x18,4),(0x1c,4),(0x20,4),(0x24,1),(0x25,1),(0x26,1)),
 0x13ad:((0x18,4),(0x1c,4),(0x20,1),(0x21,1)),
 0x13ae:((0x18,4),(0x1c,4),(0x20,1),(0x21,1)),
 0x13af:((0x18,4),(0x1c,4),(0x20,1),(0x21,1)),
 0x13b0:((0x18,4),(0x1c,4),(0x20,4),(0x24,1),(0x25,1),(0x26,1)),
 0x13b1:((0x18,4),(0x1c,4),(0x20,1),(0x21,1),(0x22,1)),
 0x13b2:((0x18,4),(0x1c,4),(0x20,1),(0x21,1),(0x22,1),(0x23,1)),
 0x13b3:((0x18,4),(0x1c,4),(0x20,4),(0x24,1),(0x25,1),(0x26,1),(0x27,1)),
 0x13b5:((0x18,4),(0x1c,4),(0x20,1),(0x21,1),(0x22,1)),
 0x13e2:((0x18,4),(0x1c,4),(0x20,1),(0x21,1)),
 0x13e3:((0x18,4),(0x1c,4),(0x20,1),(0x21,1)),
 0x145b:((0x18,4),(0x1c,4)),
 0x145c:((0x18,4),(0x1c,4)),
 0x145d:((0x18,4),(0x20,4),(0x1c,4),(0x24,1),(0x26,1),(0x25,1)),
}
class Cursor:
 def __init__(self,data):self.data=data;self.at=0;self.nodes=0
 def take(self,n):
  if n<0 or n>len(self.data)-self.at:raise ValueError('Truncated child payload')
  out=self.data[self.at:self.at+n];self.at+=n;return out
 def integer(self,n):return int.from_bytes(self.take(n),'little')
 def blob(self):
  n=self.integer(4)
  if n>65536:raise ValueError('Child string byte budget')
  return self.take(n)
 def trie(self,depth=0):
  if depth>64:raise ValueError('Trie depth budget')
  count=self.integer(4);self.nodes+=count
  if self.nodes>65536:raise ValueError('Whole trie-node budget')
  return [dict(bytes=self.blob(),field12=self.integer(1),field13=self.integer(1),children=self.trie(depth+1))for _ in range(count)]

def parse_child(frame):
 c=Cursor(frame['raw'][16:]);name=c.blob();version=c.integer(2);kind=frame['typeId']
 expected={0x13aa:0x101,0x13e2:0x103,0x145b:0x101}.get(kind,0x100)
 if frame['flags']!=0 or frame['version']!=0x100 or version!=expected:raise ValueError('Exact actual outer/inner child versions required')
 result=dict(typeId=kind,name=name,payloadVersion=version)
 if kind==0x13a8:
  count=c.integer(4)
  if count>65536:raise ValueError('Terminal set budget')
  result['set18']=[c.blob()for _ in range(count)]
 elif kind==0x13ab:result.update(field18=c.integer(1),trie20=c.trie())
 elif kind in LAYOUTS:
  layout=LAYOUTS[kind]+({0x13aa:((0x22,1),),0x13e2:((0x22,1),),0x145b:((0x20,1),)}.get(kind,()))
  result['fields']=[dict(offset=o,width=w,value=c.integer(w))for o,w in layout]
 else:raise ValueError('Unsupported actual child class')
 if c.at!=len(c.data):raise ValueError('Unconsumed child bytes')
 return result

def write_child(node):
 def number(x,n):return int(x).to_bytes(n,'little')
 def blob(x):return number(len(x),4)+x
 def trie(nodes):return number(len(nodes),4)+b''.join(blob(n['bytes'])+number(n['field12'],1)+number(n['field13'],1)+trie(n['children'])for n in nodes)
 kind=node['typeId'];out=blob(node['name'])+number(node['payloadVersion'],2)
 if kind==0x13a8:out+=number(len(node['set18']),4)+b''.join(blob(x)for x in node['set18'])
 elif kind==0x13ab:out+=number(node['field18'],1)+trie(node['trie20'])
 else:out+=b''.join(number(f['value'],f['width'])for f in node['fields'])
 return struct.pack('<4sIIHH',b'\x90\xfe\x01\x01',len(out)+8,kind,0,0x100)+out

def describe(data):
 root=parse_root(data);nodes=[];terminals=0;trie_nodes=0
 def clean(value):
  if isinstance(value,bytes):return {'hex':value.hex()}
  if isinstance(value,list):return [clean(x)for x in value]
  if isinstance(value,dict):return {k:clean(v)for k,v in value.items()}
  return value
 def count_trie(value):return sum(1+count_trie(n['children'])for n in value)
 for rule in root['rules']:
  for frame in rule['children']:
   node=parse_child(frame)
   if write_child(node)!=frame['raw']:raise ValueError('Child reconstruction mismatch')
   frame['raw']=write_child(node)
   terminals+=len(node.get('set18',[]));trie_nodes+=count_trie(node.get('trie20',[]))
   nodes.append(dict(rule=rule['index'],offset=frame['offset'],end=frame['end'],decoded=clean(node)))
 if write_root(root)!=data:raise ValueError('Full grammar reconstruction mismatch')
 return dict(sha256=hashlib.sha256(data).hexdigest(),consumed=len(data),rules=len(root['rules']),children=len(nodes),classes=len({n['decoded']['typeId']for n in nodes}),terminalSetEntries=terminals,trieNodes=trie_nodes,versionPreservingRoundtripExact=True,nodes=nodes,scope='Complete actual serialized child schema with outer0100 and distinct inner versions; no matching, candidate ranking or AK inference')

if __name__=='__main__':
 p=argparse.ArgumentParser(description=__doc__);p.add_argument('resource',type=Path);p.add_argument('output',type=Path);a=p.parse_args();result=describe(a.resource.read_bytes());a.output.write_text(json.dumps(result,indent=2)+'\n',encoding='utf8');print(f"PASS {result['children']} children / {result['classes']} classes / {result['terminalSetEntries']} set entries / {result['trieNodes']} trie nodes / exact version-preserving full grammar reconstruction")
